using StudentGradeManager.Data;
using StudentGradeManager.Models;

namespace StudentGradeManager.Services
{
    public class CourseService
    {
        private readonly DatabaseHelper _db;
        public CourseService(DatabaseHelper db) { _db = db; }
        public List<Course> GetAllCourses() => _db.GetAllCourses();
        public void AddCourse(Course c) => _db.AddCourse(c);
        public void UpdateCourse(Course c) => _db.UpdateCourse(c);
        public void DeleteCourse(int id) => _db.DeleteCourse(id);
    }
}
