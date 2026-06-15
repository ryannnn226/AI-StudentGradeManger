using System;

namespace StudentGradeManager.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Credit { get; set; }
        public string Teacher { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
    }
}
