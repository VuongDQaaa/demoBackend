using backend.Data;
using backend.DTO;
using backend.Entities;
using backend.Enums;
using backend.Helpers;
using backend.Models.Assets;
using backend.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public interface IAssetRepository
    {
        public Task AddAsset(AssetCreateModel asset, int userId);
        public Task UpdateAsset(AssetUpdateModel asset, int assetId);
        public Task DeleteAsset(int id);
        public Task<ActionResult<AssetDTO>> GetAssetById(int id);
        public Task<ActionResult<List<AssetDTO>>> GetAllAsset();
        public Task<List<AssetDTO>> GetAssetByLocation(User user);
    }
    public class AssetRepository : IAssetRepository
    {
        private MyDbContext _context;
        public AssetRepository(MyDbContext context)
        {
            _context = context;
        }

        private bool ValidAsset(int assetId)
        {
            var foundAssignment = _context.Assignments.SingleOrDefault(a => a.AssetId == assetId);
            if (foundAssignment != null)
            {
                var foundRequest = _context.ReturningRequests.SingleOrDefault(r => r.AssignmentId == foundAssignment.AssignmentId);
                if(foundRequest != null)
                {
                    if(foundRequest.RequestState == RequestState.Completed)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool CheckInstalledDate(DateTime date)
        {
            if (DateTime.Compare(DateTime.Now, date) < 0)
            {
                return false;
            }
            return true;
        }

        private bool CheckValidCategory(int categoryId)
        {
            return _context.Categories.Any(c => c.CategoryId == categoryId);
        }

        private bool CheckUser(int userId)
        {
            return _context.Users.Any(u => u.UserId == userId);
        }

        private string GenerateAssetCode(int categoryId)
        {
            var prefix = _context.Categories.Find(categoryId).Perfix;
            var sameCatalogue = _context.Assets.Where(a => a.CategoryId == categoryId);
            if (sameCatalogue.Count() == 0)
            {
                return prefix + "000001";
            }
            else
            {
                var lastAssetCode = sameCatalogue.OrderByDescending(o => o.AssetId).FirstOrDefault()?.AssetCode;
                var lastAssetId = Convert.ToInt32(lastAssetCode?.Substring(lastAssetCode.Length - 6)) + 1;
                return prefix + String.Format("{0,0:D6}", lastAssetId++);
            }
        }
        
        public async Task AddAsset(AssetCreateModel asset, int userId)
        {
            try
            {
                if (!CheckValidCategory(asset.CategoryId)) throw new AppException("CategoryId is not valid");
                if (!CheckInstalledDate(DateTime.Parse(asset.InstalledDate))) throw new AppException("Installed Date must not be in the future");
                if (!CheckUser(userId)) throw new AppException ("Undentify user");
                var foundUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                var foundCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == asset.CategoryId);
                if (foundCategory != null && foundUser != null)
                {
                    DateTime dateTimeParseResult;
                    var newAsset = new Asset
                    {
                        AssetCode = GenerateAssetCode(asset.CategoryId),
                        AssetName = asset.AssetName,
                        CategoryId = asset.CategoryId,
                        CategoryName = foundCategory.CategoryName,
                        Specification = asset.Specification,
                        InstalledDate = DateTime.TryParse(asset.InstalledDate, out dateTimeParseResult)                    
                        ? dateTimeParseResult
                        : DateTime.Now,
                        Location = foundUser.Location.ToString(),
                        AssetState = asset.AssetState.Equals("Available") ? AssetState.Available : AssetState.NotAvailable
                    };
                    await _context.Assets.AddAsync(newAsset);
                    await _context.SaveChangesAsync();
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task DeleteAsset(int id)
        {
            try
            {
                if(ValidAsset(id)) throw new AppException ("you can not delete this asset");
                var foundAsset = await _context.Assets.FindAsync(id);
                if (foundAsset != null)
                {
                    _context.Assets.Remove(foundAsset);
                    await _context.SaveChangesAsync();
                }
                else{
                    throw new AppException("User not found");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<ActionResult<List<AssetDTO>>> GetAllAsset()
        {
            if (_context.Assets != null)
            {
                try
                {
                    var assets = await _context.Assets
                        .Include(c => c.Category)
                        .Select(asset => asset.AssetEntityToDTO())
                        .ToListAsync();
                    return new OkObjectResult(assets);
                }
                catch (Exception e)
                {
                    return new BadRequestObjectResult(e);
                }
            }
            return new NoContentResult();
        }

        public async Task<ActionResult<AssetDTO>> GetAssetById(int id)
        {
            if (_context.Assets != null)
            {
                try
                {
                    var foundAsset = await _context.Assets.FindAsync(id);
                    if (foundAsset != null)
                        return new OkObjectResult(foundAsset.AssetEntityToDTO());
                    else
                        return new NotFoundResult();
                }
                catch (Exception e)
                {
                    return new BadRequestObjectResult(e);
                }
            }
            return new NoContentResult();
        }

        public async Task UpdateAsset(AssetUpdateModel asset, int assetId)
        {
            try
            {
                DateTime dateTimeParseResult;
                var foundAsset = await _context.Assets.FindAsync(assetId);
                var foundCategory = await _context.Categories.FindAsync(asset.CategoryId);
                if (foundAsset != null)
                {
                    foundAsset.AssetName = asset.AssetName;
                    foundAsset.CategoryId = asset.CategoryId;
                    foundAsset.CategoryName = foundCategory.CategoryName;
                    foundAsset.InstalledDate = DateTime.TryParse(asset.InstalledDate, out dateTimeParseResult)                    
                        ? dateTimeParseResult
                        : DateTime.Now;

                    foundAsset.Specification = asset.Specification;

                    if (asset.AssetState.Equals("NotAvailable"))
                    {
                        foundAsset.AssetState = AssetState.NotAvailable;
                    }
                    else if (asset.AssetState.Equals("WaitingForRecycle"))
                    {
                        foundAsset.AssetState = AssetState.WaitingForRecycle;
                    }
                    else if (asset.AssetState.Equals("Recycled"))
                    {
                        foundAsset.AssetState = AssetState.Recycled;
                    }
                    else
                    {
                        foundAsset.AssetState = AssetState.Available;
                    }

                    _context.Assets.Update(foundAsset);
                    await _context.SaveChangesAsync();
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Task<List<AssetDTO>> GetAssetByLocation(User user)
        {
            var foundAsset = _context.Assets.Where(asset => asset.Location == user.Location.ToString());
            if (foundAsset != null)
            {
                return foundAsset.Select(x => x.AssetEntityToDTO()).ToListAsync();
            }
            return null;
        }
    }
}