using backend.Data;
using backend.Entities;
using backend.Enums;
using backend.Helpers;
using backend.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories
{
    public interface IUserRepository
    {
        public Task AddUser(UserCreateModel user, int userId);
        public Task UpdateUser(UserUpdateModel user, int userId);
        public Task DeleteUser(int id);
        public Task DisableUser(int id);
        public Task ChangePasswordFirstLogin(FirstLogin login);
        public Task ChangePassWord(ChangePassword changePassword);
        public Task<List<User>> GetAllUser();
        public Task<User> GetUserById(int id);
    }
    public class UserRepository : IUserRepository
    {
        private MyDbContext _context;

        public UserRepository(MyDbContext context)
        {
            _context = context;
        }

        private bool CheckAssignment(int userId)
        {
            var foundUser = _context.Users.Include(x => x.AssignedTo).FirstOrDefault(x => x.UserId == userId);
            var foudAssignment = foundUser.AssignedTo;
            if(!foudAssignment.Any()
                || foudAssignment.All(x => x.AssignmentState == AssignmentState.Completed))
            {
                return false;
            }
            else
            {
                return true;
            }            
        }

        private bool CheckAge(DateTime date)
        {
            var today = DateTime.Now;
            if (today.Year - date.Year <= 18)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool CheckJoinedDate(DateTime date)
        {
            if (DateTime.Compare(DateTime.Now, date) < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private string GenerateStaffCode()
        {
            var lastUserId = _context.Users?.OrderByDescending(o => o.UserId).FirstOrDefault()?.UserId + 1;
            var userId = "SD" + String.Format("{0,0:D4}", lastUserId++);
            return userId;
        }

        private string GenerateUserName(string? firstname, string? lastname)
        {
            var prefix = "";
            var postfix = "";
            if (firstname == null)
            {
                prefix = "";
            }
            else
            {
                var firstnames = firstname.Trim().Split(' ');
                foreach (var fn in firstnames)
                {
                    prefix += fn.Trim();
                }
            }

            if (lastname == null)
            {
                postfix = "";
            }
            else
            {
                var lastnames = lastname.Trim().Split(' ');
                foreach (var ln in lastnames)
                {
                    if (ln != "") postfix += ln.Trim().Substring(0, 1);
                }
            }

            var rawusername = (prefix + postfix).ToLower();

            //generate code
            var check = _context.Users.Any(o => o.UserName.Equals(rawusername));
            if (check)
            {
                var postNumber = 0;
                var flag = true;
                var username = "";
                do
                {
                    postNumber++;
                    username = rawusername + postNumber.ToString();
                    flag = CheckUsernameDb(username);
                } while (flag);
                return username;
            }
            else
            {
                return rawusername;
            }

        }
        private bool CheckUsernameDb(string username)
        {
            if (_context.Users.Any(o => o.UserName.Equals(username)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task AddUser(UserCreateModel user, int userId)
        {
            try
            {
                if (!CheckAge(user.DateOfBirth)) throw new AppException("user is under 18");
                if (!CheckJoinedDate(DateTime.Parse(user.JoinedDate))) throw new AppException("joined date is in the future");
                var foundUser = await _context.Users.FindAsync(userId);
                if (foundUser != null)
                {
                    var username = GenerateUserName(user.FirstName, user.LastName);
                    DateTime dateTimeParseResult;
                    var newUser = new User
                    {
                        UserName = username,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(GeneratePassword(username, user.DateOfBirth)),
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Gender = !user.Gender.Equals("Male") ? Gender.Female : Gender.Male,
                        Location = foundUser.Location,
                        IsFirstLogin = true,
                        StaffCode = GenerateStaffCode(),
                        Role = !user.Role.Equals("Staff") ? Role.Admin : Role.Staff,
                        DateOfBirth = user.DateOfBirth,
                        JoinedDate = DateTime.TryParse(user.JoinedDate, out dateTimeParseResult)
                        ? dateTimeParseResult
                        : DateTime.Now,
                    };
                    await _context.Users.AddAsync(newUser);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string GeneratePassword(string username, DateTime dateOfBirth)
        {
            return username + "@" + dateOfBirth.ToString("ddMMyyyy");
        }

        public async Task ChangePassWord(ChangePassword changePassword)
        {
            try
            {
                var foundUser = _context.Users.FirstOrDefault(user => user.UserName == changePassword.UserName);
                if (!BCrypt.Net.BCrypt.Verify(changePassword.OldPassword, foundUser.PasswordHash)) throw new AppException("Wrong old password");
                if (changePassword.OldPassword == changePassword.NewPassword) throw new AppException("New password and password is the same");
                if (changePassword.NewPassword.Length > 255) throw new AppException("Password should less than 255 characters");
                if (changePassword.NewPassword.Length <= 8) throw new AppException("Password should have more than 8 characters");
                if (foundUser != null)
                {
                    foundUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePassword.NewPassword);

                    _context.Users.Update(foundUser);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task ChangePasswordFirstLogin(FirstLogin login)
        {
            try
            {
                var foundUser = _context.Users.FirstOrDefault(x => x.UserName == login.UserName);
                if (login.NewPassword.Length == 0) throw new AppException("new password can not be null");
                if (login.NewPassword.Length >= 255) throw new AppException("Your password should less than 255 chatacters");
                if (login.NewPassword.Length <= 8) throw new AppException("Your password should more than 8 chatacters");
                if (foundUser.IsFirstLogin == false) throw new AppException("This is not your first login");
                if (foundUser != null

                    && login.NewPassword.Length > 8
                    && login.NewPassword.Length < 255)
                {
                    foundUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(login.NewPassword);
                    foundUser.IsFirstLogin = false;

                    _context.Users.Update(foundUser);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task DeleteUser(int id)
        {
            try
            {
                var foundUser = await _context.Users.FindAsync(id);
                if (foundUser != null)
                {
                    _context.Users.Remove(foundUser);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<List<User>> GetAllUser()
        {
            return _context.Users.ToList();
        }

        public async Task<User> GetUserById(int id)
        {
            var foundUser = await _context.Users.FindAsync(id);
            if (foundUser != null)
            {
                return foundUser;
            }
            return null;
        }

        public async Task UpdateUser(UserUpdateModel user, int userId)
        {
            try
            {
                if (!CheckAge(DateTime.Parse(user.DateOfBirth))) throw new AppException("user in under 18");
                if (!CheckJoinedDate(DateTime.Parse(user.JoinedDate))) throw new AppException("joined date is in the future");
                var foundUser = await _context.Users.FindAsync(userId);
                if (foundUser != null)
                {
                    DateTime dateTimeBirthResult;
                    DateTime dateTimeJoinedResult;
                    foundUser.Gender = !user.Gender.Equals("Male") ? Gender.Female : Gender.Male;
                    foundUser.DateOfBirth = DateTime.TryParse(user.DateOfBirth, out dateTimeBirthResult)
                    ? dateTimeBirthResult
                    : DateTime.Now;
                    foundUser.JoinedDate = DateTime.TryParse(user.JoinedDate, out dateTimeJoinedResult)
                    ? dateTimeJoinedResult
                    : DateTime.Now;
                    foundUser.Role = !user.Role.Equals("Admin") ? Role.Staff : Role.Admin;

                    _context.Users.Update(foundUser);
                    await _context.SaveChangesAsync();
                };

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task DisableUser(int id)
        {
            try
            {
                var foundUser = _context.Users.FirstOrDefault(user => user.UserId == id);
                if (CheckAssignment(id)) throw new AppException("can disable this user");
                if (foundUser != null)
                {
                    foundUser.UserState = UserState.Disable;
                    _context.Users.Update(foundUser);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}