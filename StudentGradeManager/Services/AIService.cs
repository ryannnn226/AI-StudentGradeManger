using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentGradeManager.Data;

namespace StudentGradeManager.Services
{
    public class AIService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly DatabaseHelper _db;

        public AIService(DatabaseHelper db, string baseUrl, string apiKey, string model = "deepseek-chat")
        {
            _db = db;
            _apiKey = apiKey;
            _model = model;
            _http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(60) };
            _http.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
        }

        public async Task<string> AnalyzeGradeTrendAsync(int studentId)
        {
            var grades = _db.GetGradesByStudent(studentId);
            if (grades.Count == 0) return "该学生暂无成绩数据，无法进行分析。";

            var students = _db.GetAllStudents();
            var student = students.Find(s => s.Id == studentId);
            var name = student?.Name ?? "未知";

            var sb = new StringBuilder();
            sb.AppendLine("学生姓名：" + name + "（学号：" + (student?.StudentNo ?? "未知") + "）");
            sb.AppendLine("成绩记录：");
            foreach (var (g, cn) in grades)
                sb.AppendLine("  - " + cn + "（" + g.ExamType + "）：" + g.Score + "分，日期：" + g.ExamDate.ToString("yyyy-MM-dd"));

            var nl = Environment.NewLine;
            var prompt = "你是一位专业的教育数据分析师。请根据以下学生的成绩数据，进行成绩波动分析并给出具体学习建议。" + nl + "要求：1. 分析成绩趋势：哪些科目进步、哪些退步；2. 识别薄弱环节；3. 给出3-5条具体可行的学习建议；4. 语气要鼓励和建设性；5. 控制在300字以内。" + nl + nl + sb.ToString();
            return await CallApi(prompt);
        }

        public async Task<string> GenerateCommentAsync(int studentId, int courseId = 0)
        {
            var grades = _db.GetGradesByStudent(studentId);
            if (grades.Count == 0) return "暂无成绩数据，无法生成评语。";

            var students = _db.GetAllStudents();
            var student = students.Find(s => s.Id == studentId);
            var name = student?.Name ?? "未知";

            var filtered = courseId > 0 ? grades.Where(g => g.Grade.CourseId == courseId).ToList() : grades;
            var sb = new StringBuilder();
            sb.AppendLine("学生：" + name + "，班级：" + (student?.Class ?? "未知"));
            double sum = 0;
            foreach (var (g, cn) in filtered)
            {
                sb.AppendLine("  - " + cn + "（" + g.ExamType + "）：" + g.Score + "分");
                sum += g.Score;
            }
            var avg = filtered.Count > 0 ? Math.Round(sum / filtered.Count, 1) : 0;

            var nl = Environment.NewLine;
            var prompt = "你是一位大学教师。请根据学生成绩生成一份个性化的学期综合评语。" + nl + "要求：1. 总结学生整体表现（平均分：" + avg + "）；2. 表扬优秀科目，鼓励待提高科目；3. 语气亲切鼓励，适合写入成绩单；4. 用中文回复，控制在200字以内。" + nl + nl + sb.ToString();
            return await CallApi(prompt);
        }

        private async Task<string> CallApi(string prompt)
        {
            try
            {
                var body = new { model = _model, messages = new[] { new { role = "system", content = "你是一位专业的教育数据分析师，请用中文回复。" }, new { role = "user", content = prompt } }, temperature = 0.7, max_tokens = 800 };
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _http.PostAsync("/chat/completions", content);
                var text = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode) return "AI 服务错误（HTTP " + (int)resp.StatusCode + "）：" + text;
                var obj = JObject.Parse(text);
                return obj["choices"]?[0]?["message"]?["content"]?.ToString()?.Trim() ?? "AI 未返回有效内容。";
            }
            catch (TaskCanceledException) { return "AI 服务请求超时，请检查网络后重试。"; }
            catch (Exception ex) { return "AI 服务异常：" + ex.Message; }
        }
    }
}
