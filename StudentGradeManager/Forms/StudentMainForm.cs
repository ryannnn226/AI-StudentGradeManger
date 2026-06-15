using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentGradeManager.Services;

namespace StudentGradeManager.Forms
{
    public class StudentMainForm : Form
    {
        private readonly AuthService _auth;
        private readonly StudentService _stuSvc;
        private readonly CourseService _crsSvc;
        private readonly GradeService _grdSvc;
        private readonly StatisticsService _sttSvc;
        private readonly AIService _aiSvc;
        private DataGridView dgv = null!;
        private RichTextBox rtbAI = null!;
        private Label lblInfo = null!;
        private int _sid;

        public StudentMainForm(AuthService auth, StudentService ss, CourseService cs,
            GradeService gs, StatisticsService sts, AIService ai)
        {
            _auth = auth; _stuSvc = ss; _crsSvc = cs;
            _grdSvc = gs; _sttSvc = sts; _aiSvc = ai;
            var all = _stuSvc.GetAllStudents();
            var st = all.FirstOrDefault(s => s.Name == _auth.CurrentUser?.FullName);
            _sid = st?.Id ?? 1;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "成绩管理系统 - 学生：" + (_auth.CurrentUser?.FullName ?? "");
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(750, 500);

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10) };

            // ===== 我的成绩 =====
            var tabG = new TabPage("我的成绩");
            var pg = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            lblInfo = new Label { Dock = DockStyle.Top, Height = 50, Font = new Font("Microsoft YaHei", 10), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10), Text = "正在加载..." };
            dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            var btnRef = new Button { Text = "刷新成绩", Dock = DockStyle.Bottom, Height = 35, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRef.FlatAppearance.BorderSize = 0;
            btnRef.Click += (s, e) => RefreshGrades();
            pg.Controls.Add(dgv); pg.Controls.Add(lblInfo); pg.Controls.Add(btnRef);
            tabs.TabPages.Add(tabG);

            // ===== AI 分析 =====
            var tabAI = new TabPage("AI 智能分析");
            var pa = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 5, 0, 5) };
            var btnTrend = new Button { Text = "AI 成绩波动分析", Size = new Size(160, 30), BackColor = Color.FromArgb(52, 73, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnTrend.FlatAppearance.BorderSize = 0;
            var btnComment = new Button { Text = "AI 生成学期评语", Size = new Size(160, 30), BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnComment.FlatAppearance.BorderSize = 0;
            bar.Controls.AddRange(new Control[] { btnTrend, btnComment });

            rtbAI = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10), ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            btnTrend.Click += async (s, e) => { rtbAI.Text = "正在调用 AI 分析..."; rtbAI.Text = await _aiSvc.AnalyzeGradeTrendAsync(_sid); };
            btnComment.Click += async (s, e) => { rtbAI.Text = "正在调用 AI 生成评语..."; rtbAI.Text = await _aiSvc.GenerateCommentAsync(_sid); };
            pa.Controls.Add(rtbAI); pa.Controls.Add(bar);
            tabs.TabPages.Add(tabAI);

            this.Controls.Add(tabs);
            this.Load += (s, e) => RefreshGrades();
        }

        void RefreshGrades()
        {
            var grades = _grdSvc.GetGradesByStudent(_sid);
            dgv.DataSource = null;
            dgv.DataSource = grades.Select(g => new { 课程 = g.CourseName, 分数 = g.Grade.Score, 类型 = g.Grade.ExamType, 日期 = g.Grade.ExamDate.ToString("yyyy-MM-dd") }).ToList();
            if (grades.Count > 0)
            {
                var avg = Math.Round(grades.Average(g => g.Grade.Score), 1);
                lblInfo.Text = "记录数：" + grades.Count + " | 平均分：" + avg + " | 最高分：" + grades.Max(g => g.Grade.Score) + " | 最低分：" + grades.Min(g => g.Grade.Score);
            }
            else lblInfo.Text = "暂无成绩数据";
        }
    }
}
