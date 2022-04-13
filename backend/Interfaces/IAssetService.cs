using backend.DTO;
using backend.Entities;
using backend.Models.Assets;
using Microsoft.AspNetCore.Mvc;

namespace backend.Interfaces
{
    public interface IAssetService
    {
        public Task AddAsset(AssetCreateModel asset, int userId);
        public Task UpdateAsset(AssetUpdateModel asset, int assetId);
        public Task DeleteAsset(int id);
        public Task<ActionResult<AssetDTO>> GetAssetById(int id);
        public Task<ActionResult<List<AssetDTO>>> GetAllAsset();
        public Task<List<AssetDTO>> GetAssetByLocation(User user);
    }
}