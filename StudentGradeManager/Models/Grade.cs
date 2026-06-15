using System;

namespace StudentGradeManager.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public double Score { get; set; }
        public string ExamType { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
    }
}
