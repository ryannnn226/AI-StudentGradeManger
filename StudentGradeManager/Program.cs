using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using StudentGradeManager.Data;
using StudentGradeManager.Forms;
using StudentGradeManager.Services;

namespace StudentGradeManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (!File.Exists(configPath))
                    configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                
                if (!File.Exists(configPath))
                {
                    MessageBox.Show("appsettings.json not found at:\n" + configPath, "Fatal Error");
                    return;
                }

                var config = JObject.Parse(File.ReadAllText(configPath));
                var connStr = config["Database"]?["ConnectionString"]?.ToString() ?? "Data Source=StudentGrade.db";
                var apiKey = config["DeepSeek"]?["ApiKey"]?.ToString() ?? "";
                var baseUrl = config["DeepSeek"]?["BaseUrl"]?.ToString() ?? "https://api.deepseek.com/v1";
                var model = config["DeepSeek"]?["Model"]?.ToString() ?? "deepseek-chat";

                var db = new DatabaseHelper(connStr);
                var authService = new AuthService(db);
                var studentService = new StudentService(db);
                var courseService = new CourseService(db);
                var gradeService = new GradeService(db);
                var statsService = new StatisticsService(db);
                var aiService = new AIService(db, baseUrl, apiKey, model);

                Application.Run(new LoginForm(authService, studentService, courseService,
                    gradeService, statsService, aiService));
            }
            catch (Exception ex)
            {
                var msg = "Startup Error:\n\n" + ex.GetType().FullName + "\n" + ex.Message + "\n\nStack:\n" + ex.StackTrace;
                if (ex.InnerException != null)
                    msg += "\n\nInner: " + ex.InnerException.Message;
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"), msg);
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
