using System;

namespace StudentGradeManager.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
