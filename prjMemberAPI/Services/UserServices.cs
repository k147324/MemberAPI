using prjMemberAPI.Models;

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
        
    }
}
