using StudentGradeManager.Data;
using StudentGradeManager.Models;

namespace StudentGradeManager.Services
{
    public class StudentService
    {
        private readonly DatabaseHelper _db;
        public StudentService(DatabaseHelper db) { _db = db; }
        public List<Student> GetAllStudents() => _db.GetAllStudents();
        public List<Student> GetStudentsByClass(string c) => _db.GetStudentsByClass(c);
        public void AddStudent(Student s) => _db.AddStudent(s);
        public void UpdateStudent(Student s) => _db.UpdateStudent(s);
        public void DeleteStudent(int id) => _db.DeleteStudent(id);
    }
}
