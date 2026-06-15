using System;
using System.Collections.Generic;
using System.Linq;
using StudentGradeManager.Data;

namespace StudentGradeManager.Services
{
    public class GradeStatsResult
    {
        public string CourseName { get; set; } = string.Empty;
        public double AverageScore { get; set; }
        public double MaxScore { get; set; }
        public double MinScore { get; set; }
        public int TotalStudents { get; set; }
        public int PassCount { get; set; }
        public double PassRate { get; set; }
        public List<(string StudentName, double Score)> Ranking { get; set; } = new();
    }

    public class OverallStats
    {
        public double OverallAverage { get; set; }
        public int TotalGrades { get; set; }
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
    }

    public class StatisticsService
    {
        private readonly DatabaseHelper _db;
        public StatisticsService(DatabaseHelper db) { _db = db; }

        public OverallStats GetOverallStats()
        {
            var grades = _db.GetAllGradesWithDetails();
            var students = _db.GetAllStudents();
            var courses = _db.GetAllCourses();
            return new OverallStats
            {
                OverallAverage = grades.Any() ? Math.Round(grades.Average(g => g.Grade.Score), 1) : 0,
                TotalGrades = grades.Count,
                TotalStudents = students.Count,
                TotalCourses = courses.Count
            };
        }

        public List<GradeStatsResult> GetStatsByCourse(int courseId = 0)
        {
            var allGrades = _db.GetAllGradesWithDetails();
            var filtered = courseId > 0 ? allGrades.Where(g => g.Grade.CourseId == courseId).ToList() : allGrades;
            var results = new List<GradeStatsResult>();
            foreach (var group in filtered.GroupBy(g => g.CourseName))
            {
                var scores = group.Select(g => g.Grade.Score).ToList();
                results.Add(new GradeStatsResult
                {
                    CourseName = group.Key,
                    AverageScore = Math.Round(scores.Average(), 1),
                    MaxScore = scores.Max(),
                    MinScore = scores.Min(),
                    TotalStudents = group.Select(g => g.StudentName).Distinct().Count(),
                    PassCount = scores.Count(s => s >= 60),
                    PassRate = Math.Round((double)scores.Count(s => s >= 60) / scores.Count * 100, 1),
                    Ranking = group.Select(g => (g.StudentName, g.Grade.Score)).OrderByDescending(x => x.Score).ToList()
                });
            }
            return results;
        }

        public List<(string ClassName, double AvgScore)> GetStatsByClass()
        {
            var grades = _db.GetAllGradesWithDetails();
            return grades.GroupBy(g => g.StudentClass)
                .Select(g => (g.Key, Math.Round(g.Average(x => x.Grade.Score), 1)))
                .OrderByDescending(x => x.Item2).ToList();
        }

        public Dictionary<string, int> GetScoreDistribution(int courseId)
        {
            var scores = _db.GetAllGradesWithDetails()
                .Where(g => g.Grade.CourseId == courseId)
                .Select(g => g.Grade.Score).ToList();
            return new Dictionary<string, int>
            {
                ["90-100"] = scores.Count(s => s >= 90),
                ["80-89"] = scores.Count(s => s >= 80 && s < 90),
                ["70-79"] = scores.Count(s => s >= 70 && s < 80),
                ["60-69"] = scores.Count(s => s >= 60 && s < 70),
                ["0-59"] = scores.Count(s => s < 60)
            };
        }
    }
}
