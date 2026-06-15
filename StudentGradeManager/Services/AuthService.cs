using StudentGradeManager.Data;
using StudentGradeManager.Models;

namespace StudentGradeManager.Services
{
    public class AuthService
    {
        private readonly DatabaseHelper _db;
        private User? _currentUser;
        public User? CurrentUser => _currentUser;

        public AuthService(DatabaseHelper db) { _db = db; }

        public bool Login(string username, string password)
        {
            _currentUser = _db.AuthenticateUser(username, password);
            return _currentUser != null;
        }

        public void Logout() { _currentUser = null; }
        public List<User> GetAllUsers() => _db.GetAllUsers();
        public void AddUser(User user) => _db.AddUser(user);
        public void DeleteUser(int id) => _db.DeleteUser(id);
        public static string HashPassword(string p) => DatabaseHelper.ComputeHash(p);
    }
}
