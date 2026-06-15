using StudentGradeManager.Data;
using StudentGradeManager.Models;

namespace StudentGradeManager.Services
{
    public class GradeService
    {
        private readonly DatabaseHelper _db;
        public GradeService(DatabaseHelper db) { _db = db; }
        public List<(Grade Grade, string StudentName, string CourseName, string StudentClass)> GetAllGradesWithDetails()
            => _db.GetAllGradesWithDetails();
        public List<(Grade Grade, string CourseName)> GetGradesByStudent(int sid)
            => _db.GetGradesByStudent(sid);
        public void AddGrade(Grade g) => _db.AddGrade(g);
        public void UpdateGrade(Grade g) => _db.UpdateGrade(g);
        public void DeleteGrade(int id) => _db.DeleteGrade(id);
    }
}
