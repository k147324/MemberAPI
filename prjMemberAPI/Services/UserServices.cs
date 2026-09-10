using prjMemberAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace prjMemberAPI.Services
{
    public class UserServices
    {
        private tempdbContext _db;
        public UserServices(tempdbContext db)
        {
            _db = db;
        }   
        //註冊會員
        public async Task<bool> IsUsernameExists(string username)
        {
            return await Task.Run(() => _db.TUsers.Any(user => user.FUsername == username));
        }
        public async Task<bool> IsEmailExists(string email)
        {
            return await Task.Run(() => _db.TUsers.Any(user => user.FEmail == email));
        }
        public async Task<string> GetPasswordByUsername(string username)
        {
            var user = await Task.Run(() => _db.TUsers.FirstOrDefault(u => u.FUsername == username));
            return user?.FPassword;
        }
        public async Task<string> GetPasswordByEmail(string email)
        {
            var user = await Task.Run(() => _db.TUsers.FirstOrDefault(u => u.FEmail == email));
            return user?.FPassword;
        }
        public async Task<TUser> GetUserByUsername(string username)
        {
            var user = await Task.Run(() => _db.TUsers.FirstOrDefault(u => u.FUsername == username));
            return user;
        }
        public async Task<TUser> GetUserByEmail(string email)
        {
            var user = await Task.Run(() => _db.TUsers.FirstOrDefault(u => u.FEmail == email));
            return user;
        }
        public async void AddUser(UserRegisterDTO u, string password)
        {
            TUser user = new TUser{
                FUsername = u.fUsername,
                FPassword = password,
                FEmail = u.fEmail,
                FPhone = u.fPhone,
                FAddress = u.fAddress,
                FIdNum = u.fIdNum,
                FCreateTime = DateTime.Now,
                FIsAdmin = false
            };

            _db.TUsers.Add(user);
            await _db.SaveChangesAsync();
            
        }
    }
}
