using backend.DTO;
using backend.Entities;
using backend.Enums;

namespace backend.Utilities
{
    public static class Utility
    {
        public static CategoryDTO CategoryEntityToDTO(this Category entity)
        {
            return new CategoryDTO()
            {
                CategoryId = entity.CategoryId,
                CategoryName = entity.CategoryName,
                Perfix = entity.Perfix,
            };
        }
        public static Category CategoryDTOToEntity(this CategoryDTO category)
        {
            return new Category()
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Perfix = category.Perfix,
            };
        }
        public static AssetDTO AssetEntityToDTO(this Asset entity)
        {
            return new AssetDTO()
            {
                AssetId = entity.AssetId,
                CategoryId = entity.CategoryId,
                CategoryName = entity.CategoryName,
                AssetName = entity.AssetName,
                AssetCode = entity.AssetCode,
                Specification = entity.Specification,
                Location = entity.Location,
                InstalledDate = entity.InstalledDate,
                AssetState = entity.AssetState.ToString(),
            };
        }
        public static Asset AssetDTOToEntity(this AssetDTO asset)
        {
            AssetState enumParseResult;
            Asset result = new Asset
            {
                AssetId = asset.AssetId,
                CategoryId = asset.CategoryId,
                AssetName = asset.AssetName,
                AssetCode = asset.AssetCode,
                Specification = asset.Specification,
                Location = asset.Location,
                InstalledDate = asset.InstalledDate,
                AssetState = Enum.TryParse(asset.AssetState, out enumParseResult)
                    ? enumParseResult
                    : AssetState.Available,
            };
            return result;
        }
        public static AssignmentDTO AssignmentEntityToDTO(this Assignment entity)
        {
            AssignmentDTO result = new AssignmentDTO
            {
                // AssignmentId = entity.AssignmentId,
                AssignedToUserId = entity.AssignedToUserId,
                AssignedByUserID = entity.AssignedByUserId,
                AssetId = entity.AssetId,
                AssignedDate = entity.AssignedDate,
                Note = entity.Note
            };
            return result;
        }
        public static Assignment AssignmentDTOToEntity(this AssignmentDTO assignment)
        {
            Assignment result = new Assignment
            {
                // AssignmentId = assignment.AssignmentId,
                AssignedToUserId = assignment.AssignedToUserId,
                AssignedByUserId = assignment.AssignedByUserID,
                AssetId = assignment.AssetId,
                AssignedDate = assignment.AssignedDate,
                Note = assignment.Note
            };
            return result;
        }
        public static UserDTO UserEntityToDTO(this User entity)
        {
            UserDTO result = new UserDTO
            {
                UserId = entity.UserId,
                UserName = entity.UserName,
                PasswordHash = entity.PasswordHash,
                Role = entity.Role.ToString(),
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                JoinedDate = entity.JoinedDate,
                Gender = entity.Gender.ToString(),
                Location = entity.Location.ToString(),
                IsFirstLogin = entity.IsFirstLogin,
                DateOfBirth = entity.DateOfBirth.ToString(),
                StaffCode = entity.StaffCode,
                UserState = entity.UserState.ToString()
                
            };
            return result;
        }
        public static User UserDTOToEntity(this UserDTO user)
        {
            Role enumParseResult;
            Gender enumParseResultGender;
            Location enumParseResultLocation;
            UserState enumParseResultState;
            DateTime dateTimeParseResult;
            User result = new User
            {
                UserId = user.UserId,
                UserName = user.UserName,
                PasswordHash = user.PasswordHash,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JoinedDate = user.JoinedDate,
                IsFirstLogin = user.IsFirstLogin,
                StaffCode = user.StaffCode,
                Location = Enum.TryParse(user.Location, out enumParseResultLocation)
                    ?enumParseResultLocation
                    :Location.Hanoi,
                Role = Enum.TryParse(user.Role, out enumParseResult)
                    ? enumParseResult
                    : Role.Staff,
                Gender = Enum.TryParse(user.Gender, out enumParseResultGender)
                    ? enumParseResultGender
                    : Gender.Male,
                DateOfBirth = DateTime.TryParse(user.DateOfBirth, out dateTimeParseResult)
                    ? dateTimeParseResult
                    : DateTime.Now,
                UserState = Enum.TryParse(user.UserState, out enumParseResultState)
                    ? enumParseResultState
                    : UserState.Active,
            };
            return result;
        }
        public static ReturningRequestDTO ReturningRequestEntityToDTO(this ReturningRequest entity)
        {
            return new ReturningRequestDTO()
            {
                RequestedByUserId = entity.RequestedByUserId,
                ProcessedByUserId = entity.ProcessedByUserId,
                AssignmentId = entity.AssignmentId,
                RequestState = entity.RequestState.ToString()
            };
        }
        public static ReturningRequest ReturningRequestDTOToEntity(this ReturningRequestDTO returningRequest)
        {
            RequestState enumParseResult;
            ReturningRequest result = new ReturningRequest
            {
                RequestedByUserId = returningRequest.RequestedByUserId,
                ProcessedByUserId = returningRequest.ProcessedByUserId,
                AssignmentId = returningRequest.AssignmentId,
                RequestState = Enum.TryParse(returningRequest.RequestState, out enumParseResult)
                    ? enumParseResult
                    : RequestState.WaitingForReturning,
            };
            return result;
        }
    }
}