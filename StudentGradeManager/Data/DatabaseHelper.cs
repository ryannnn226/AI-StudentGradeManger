using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using StudentGradeManager.Models;

namespace StudentGradeManager.Data
{
    public class DatabaseHelper
    {
        private readonly string _connStr;

        public DatabaseHelper(string connStr)
        {
            _connStr = connStr;
            InitDb();
        }

        public void InitDb()
        {
            using var c = new SqliteConnection(_connStr);
            c.Open();
            foreach (var sql in new[] {
                "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT NOT NULL UNIQUE, PasswordHash TEXT NOT NULL, Role TEXT NOT NULL, FullName TEXT NOT NULL)",
                "CREATE TABLE IF NOT EXISTS Students (Id INTEGER PRIMARY KEY AUTOINCREMENT, StudentNo TEXT NOT NULL UNIQUE, Name TEXT NOT NULL, Gender TEXT NOT NULL, Class TEXT NOT NULL, Phone TEXT, Email TEXT)",
                "CREATE TABLE IF NOT EXISTS Courses (Id INTEGER PRIMARY KEY AUTOINCREMENT, CourseNo TEXT NOT NULL UNIQUE, Name TEXT NOT NULL, Credit REAL NOT NULL, Teacher TEXT NOT NULL, Semester TEXT NOT NULL)",
                "CREATE TABLE IF NOT EXISTS Grades (Id INTEGER PRIMARY KEY AUTOINCREMENT, StudentId INTEGER NOT NULL, CourseId INTEGER NOT NULL, Score REAL NOT NULL, ExamType TEXT NOT NULL, ExamDate TEXT NOT NULL, FOREIGN KEY(StudentId) REFERENCES Students(Id), FOREIGN KEY(CourseId) REFERENCES Courses(Id))"
            })
            {
                using var cmd = new SqliteCommand(sql, c);
                cmd.ExecuteNonQuery();
            }

            using var check = new SqliteCommand("SELECT COUNT(*) FROM Users", c);
            if ((long)check.ExecuteScalar()! == 0)
            {
                using var tx = c.BeginTransaction();
                SeedUsers(c, tx);
                SeedStudents(c, tx);
                SeedCourses(c, tx);
                SeedGrades(c, tx);
                tx.Commit();
            }
        }

        public static string ComputeHash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private SqliteConnection Open() { var c = new SqliteConnection(_connStr); c.Open(); return c; }

        private void SeedUsers(SqliteConnection c, SqliteTransaction tx)
        {
            var users = new[] {
                ("admin","admin123","Admin","系统管理员"),
                ("teacher1","teacher123","Teacher","张老师"),
                ("teacher2","teacher123","Teacher","李老师"),
                ("student1","student123","Student","王小明"),
                ("student2","student123","Student","李小华"),
                ("student3","student123","Student","赵小红")
            };
            foreach (var (u, p, r, n) in users)
            {
                using var cmd = new SqliteCommand("INSERT INTO Users(Username,PasswordHash,Role,FullName) VALUES(@u,@p,@r,@n)", c, tx);
                cmd.Parameters.AddWithValue("@u", u);
                cmd.Parameters.AddWithValue("@p", ComputeHash(p));
                cmd.Parameters.AddWithValue("@r", r);
                cmd.Parameters.AddWithValue("@n", n);
                cmd.ExecuteNonQuery();
            }
        }

        private void SeedStudents(SqliteConnection c, SqliteTransaction tx)
        {
            var students = new[] {
                ("2023150848","何施翰","男","计科2101","13800001001","he@example.com"),
                ("2023150801","王小明","男","计科2101","13800001002","wang@example.com"),
                ("2023150802","李小华","女","计科2101","13800001003","li@example.com"),
                ("2023150803","赵小红","女","计科2102","13800001004","zhao@example.com"),
                ("2023150804","陈小刚","男","计科2102","13800001005","chen@example.com"),
                ("2023150805","刘小丽","女","计科2101","13800001006","liu@example.com"),
                ("2023150806","周小强","男","计科2102","13800001007","zhou@example.com"),
                ("2023150807","吴小美","女","计科2101","13800001008","wu@example.com"),
                ("2023150808","郑小文","男","计科2103","13800001009","zheng@example.com"),
                ("2023150809","孙小芳","女","计科2103","13800001010","sun@example.com")
            };
            foreach (var (no, name, gender, cls, phone, email) in students)
            {
                using var cmd = new SqliteCommand("INSERT INTO Students(StudentNo,Name,Gender,Class,Phone,Email) VALUES(@a,@b,@c,@d,@e,@f)", c, tx);
                cmd.Parameters.AddWithValue("@a", no); cmd.Parameters.AddWithValue("@b", name);
                cmd.Parameters.AddWithValue("@c", gender); cmd.Parameters.AddWithValue("@d", cls);
                cmd.Parameters.AddWithValue("@e", phone); cmd.Parameters.AddWithValue("@f", email);
                cmd.ExecuteNonQuery();
            }
        }

        private void SeedCourses(SqliteConnection c, SqliteTransaction tx)
        {
            var courses = new[] {
                ("CS001","Windows编程",3.0,"王娜","2025-2026-2"),
                ("CS002","数据结构",4.0,"张老师","2025-2026-2"),
                ("CS003","操作系统",3.5,"李老师","2025-2026-2"),
                ("CS004","计算机网络",3.0,"张老师","2025-2026-2"),
                ("CS005","数据库原理",3.5,"李老师","2025-2026-2"),
                ("MATH001","高等数学",5.0,"刘教授","2025-2026-1")
            };
            foreach (var (no, name, credit, teacher, sem) in courses)
            {
                using var cmd = new SqliteCommand("INSERT INTO Courses(CourseNo,Name,Credit,Teacher,Semester) VALUES(@a,@b,@c,@d,@e)", c, tx);
                cmd.Parameters.AddWithValue("@a", no); cmd.Parameters.AddWithValue("@b", name);
                cmd.Parameters.AddWithValue("@c", credit); cmd.Parameters.AddWithValue("@d", teacher);
                cmd.Parameters.AddWithValue("@e", sem);
                cmd.ExecuteNonQuery();
            }
        }

        private void SeedGrades(SqliteConnection c, SqliteTransaction tx)
        {
            var rng = new Random(42);
            for (int sid = 1; sid <= 8; sid++)
            {
                for (int cid = 1; cid <= 5; cid++)
                {
                    var mid = Math.Round(55 + rng.NextDouble() * 43, 1);
                    using var cmd1 = new SqliteCommand("INSERT INTO Grades(StudentId,CourseId,Score,ExamType,ExamDate) VALUES(@a,@b,@c,'期中','2025-04-20')", c, tx);
                    cmd1.Parameters.AddWithValue("@a", sid); cmd1.Parameters.AddWithValue("@b", cid); cmd1.Parameters.AddWithValue("@c", mid);
                    cmd1.ExecuteNonQuery();

                    var fin = Math.Round(Math.Max(0, Math.Min(100, mid + (rng.NextDouble() - 0.5) * 20)), 1);
                    using var cmd2 = new SqliteCommand("INSERT INTO Grades(StudentId,CourseId,Score,ExamType,ExamDate) VALUES(@a,@b,@c,'期末','2025-06-25')", c, tx);
                    cmd2.Parameters.AddWithValue("@a", sid); cmd2.Parameters.AddWithValue("@b", cid); cmd2.Parameters.AddWithValue("@c", fin);
                    cmd2.ExecuteNonQuery();
                }
            }
        }

        public User? AuthenticateUser(string username, string password)
        {
            var hash = ComputeHash(password);
            using var c = Open();
            using var cmd = new SqliteCommand("SELECT Id,Username,PasswordHash,Role,FullName FROM Users WHERE Username=@u AND PasswordHash=@p", c);
            cmd.Parameters.AddWithValue("@u", username); cmd.Parameters.AddWithValue("@p", hash);
            using var r = cmd.ExecuteReader();
            if (r.Read()) return new User { Id=r.GetInt32(0), Username=r.GetString(1), PasswordHash=r.GetString(2), Role=r.GetString(3), FullName=r.GetString(4) };
            return null;
        }

        public List<User> GetAllUsers()
        {
            var list = new List<User>();
            using var c = Open();
            using var cmd = new SqliteCommand("SELECT Id,Username,PasswordHash,Role,FullName FROM Users", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(new User { Id=r.GetInt32(0), Username=r.GetString(1), PasswordHash=r.GetString(2), Role=r.GetString(3), FullName=r.GetString(4) });
            return list;
        }

        public void AddUser(User u)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("INSERT INTO Users(Username,PasswordHash,Role,FullName) VALUES(@a,@b,@c,@d)", c);
            cmd.Parameters.AddWithValue("@a", u.Username); cmd.Parameters.AddWithValue("@b", ComputeHash("123456"));
            cmd.Parameters.AddWithValue("@c", u.Role); cmd.Parameters.AddWithValue("@d", u.FullName);
            cmd.ExecuteNonQuery();
        }

        public void DeleteUser(int id)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("DELETE FROM Users WHERE Id=@id", c);
            cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery();
        }

        public List<Student> GetAllStudents()
        {
            var list = new List<Student>();
            using var c = Open();
            using var cmd = new SqliteCommand("SELECT Id,StudentNo,Name,Gender,Class,Phone,Email FROM Students ORDER BY StudentNo", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(new Student { Id=r.GetInt32(0), StudentNo=r.GetString(1), Name=r.GetString(2), Gender=r.GetString(3), Class=r.GetString(4), Phone=r.IsDBNull(5)?"":r.GetString(5), Email=r.IsDBNull(6)?"":r.GetString(6) });
            return list;
        }

        public List<Student> GetStudentsByClass(string cls)
        {
            var list = new List<Student>();
            using var c = Open();
            using var cmd = new SqliteCommand("SELECT Id,StudentNo,Name,Gender,Class,Phone,Email FROM Students WHERE Class=@c ORDER BY StudentNo", c);
            cmd.Parameters.AddWithValue("@c", cls);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(new Student { Id=r.GetInt32(0), StudentNo=r.GetString(1), Name=r.GetString(2), Gender=r.GetString(3), Class=r.GetString(4), Phone=r.IsDBNull(5)?"":r.GetString(5), Email=r.IsDBNull(6)?"":r.GetString(6) });
            return list;
        }

        public void AddStudent(Student s)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("INSERT INTO Students(StudentNo,Name,Gender,Class,Phone,Email) VALUES(@a,@b,@c,@d,@e,@f)", c);
            cmd.Parameters.AddWithValue("@a", s.StudentNo); cmd.Parameters.AddWithValue("@b", s.Name);
            cmd.Parameters.AddWithValue("@c", s.Gender); cmd.Parameters.AddWithValue("@d", s.Class);
            cmd.Parameters.AddWithValue("@e", s.Phone); cmd.Parameters.AddWithValue("@f", s.Email);
            cmd.ExecuteNonQuery();
        }

        public void UpdateStudent(Student s)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("UPDATE Students SET StudentNo=@a,Name=@b,Gender=@c,Class=@d,Phone=@e,Email=@f WHERE Id=@id", c);
            cmd.Parameters.AddWithValue("@a", s.StudentNo); cmd.Parameters.AddWithValue("@b", s.Name);
            cmd.Parameters.AddWithValue("@c", s.Gender); cmd.Parameters.AddWithValue("@d", s.Class);
            cmd.Parameters.AddWithValue("@e", s.Phone); cmd.Parameters.AddWithValue("@f", s.Email);
            cmd.Parameters.AddWithValue("@id", s.Id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteStudent(int id)
        {
            using var c = Open(); using var tx = c.BeginTransaction();
            using (var cmd = new SqliteCommand("DELETE FROM Grades WHERE StudentId=@id", c, tx)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            using (var cmd = new SqliteCommand("DELETE FROM Students WHERE Id=@id", c, tx)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            tx.Commit();
        }

        public List<Course> GetAllCourses()
        {
            var list = new List<Course>();
            using var c = Open();
            using var cmd = new SqliteCommand("SELECT Id,CourseNo,Name,Credit,Teacher,Semester FROM Courses ORDER BY CourseNo", c);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(new Course { Id=r.GetInt32(0), CourseNo=r.GetString(1), Name=r.GetString(2), Credit=r.GetDouble(3), Teacher=r.GetString(4), Semester=r.GetString(5) });
            return list;
        }

        public void AddCourse(Course co)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("INSERT INTO Courses(CourseNo,Name,Credit,Teacher,Semester) VALUES(@a,@b,@c,@d,@e)", c);
            cmd.Parameters.AddWithValue("@a", co.CourseNo); cmd.Parameters.AddWithValue("@b", co.Name);
            cmd.Parameters.AddWithValue("@c", co.Credit); cmd.Parameters.AddWithValue("@d", co.Teacher); cmd.Parameters.AddWithValue("@e", co.Semester);
            cmd.ExecuteNonQuery();
        }

        public void UpdateCourse(Course co)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("UPDATE Courses SET CourseNo=@a,Name=@b,Credit=@c,Teacher=@d,Semester=@e WHERE Id=@id", c);
            cmd.Parameters.AddWithValue("@a", co.CourseNo); cmd.Parameters.AddWithValue("@b", co.Name);
            cmd.Parameters.AddWithValue("@c", co.Credit); cmd.Parameters.AddWithValue("@d", co.Teacher); cmd.Parameters.AddWithValue("@e", co.Semester);
            cmd.Parameters.AddWithValue("@id", co.Id); cmd.ExecuteNonQuery();
        }

        public void DeleteCourse(int id)
        {
            using var c = Open(); using var tx = c.BeginTransaction();
            using (var cmd = new SqliteCommand("DELETE FROM Grades WHERE CourseId=@id", c, tx)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            using (var cmd = new SqliteCommand("DELETE FROM Courses WHERE Id=@id", c, tx)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
            tx.Commit();
        }

        public List<(Grade Grade, string StudentName, string CourseName, string StudentClass)> GetAllGradesWithDetails()
        {
            var list = new List<(Grade, string, string, string)>();
            using var c = Open();
            using var cmd = new SqliteCommand("SELECT g.Id,g.StudentId,g.CourseId,g.Score,g.ExamType,g.ExamDate,s.Name,c.Name,s.Class FROM Grades g JOIN Students s ON g.StudentId=s.Id JOIN Courses c ON g.CourseId=c.Id ORDER BY g.ExamDate DESC,s.StudentNo", c);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var g = new Grade { Id=r.GetInt32(0), StudentId=r.GetInt32(1), CourseId=r.GetInt32(2), Score=r.GetDouble(3), ExamType=r.GetString(4), ExamDate=DateTime.Parse(r.GetString(5)) };
                list.Add((g, r.GetString(6), r.GetString(7), r.GetString(8)));
            }
            return list;
        }

        public List<(Grade Grade, string CourseName)> GetGradesByStudent(int sid)
        {
            var list = new List<(Grade, string)>();
            using var c = Open();
            using var cmd = new SqliteCommand("SELECT g.Id,g.StudentId,g.CourseId,g.Score,g.ExamType,g.ExamDate,c.Name FROM Grades g JOIN Courses c ON g.CourseId=c.Id WHERE g.StudentId=@sid ORDER BY g.ExamDate DESC", c);
            cmd.Parameters.AddWithValue("@sid", sid);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var g = new Grade { Id=r.GetInt32(0), StudentId=r.GetInt32(1), CourseId=r.GetInt32(2), Score=r.GetDouble(3), ExamType=r.GetString(4), ExamDate=DateTime.Parse(r.GetString(5)) };
                list.Add((g, r.GetString(6)));
            }
            return list;
        }

        public void AddGrade(Grade g)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("INSERT INTO Grades(StudentId,CourseId,Score,ExamType,ExamDate) VALUES(@a,@b,@c,@d,@e)", c);
            cmd.Parameters.AddWithValue("@a", g.StudentId); cmd.Parameters.AddWithValue("@b", g.CourseId);
            cmd.Parameters.AddWithValue("@c", g.Score); cmd.Parameters.AddWithValue("@d", g.ExamType);
            cmd.Parameters.AddWithValue("@e", g.ExamDate.ToString("yyyy-MM-dd"));
            cmd.ExecuteNonQuery();
        }

        public void UpdateGrade(Grade g)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("UPDATE Grades SET StudentId=@a,CourseId=@b,Score=@c,ExamType=@d,ExamDate=@e WHERE Id=@id", c);
            cmd.Parameters.AddWithValue("@a", g.StudentId); cmd.Parameters.AddWithValue("@b", g.CourseId);
            cmd.Parameters.AddWithValue("@c", g.Score); cmd.Parameters.AddWithValue("@d", g.ExamType);
            cmd.Parameters.AddWithValue("@e", g.ExamDate.ToString("yyyy-MM-dd")); cmd.Parameters.AddWithValue("@id", g.Id);
            cmd.ExecuteNonQuery();
        }

        public void DeleteGrade(int id)
        {
            using var c = Open();
            using var cmd = new SqliteCommand("DELETE FROM Grades WHERE Id=@id", c);
            cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery();
        }
    }
}
