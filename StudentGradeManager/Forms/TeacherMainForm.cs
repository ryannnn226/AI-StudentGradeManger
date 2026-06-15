using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentGradeManager.Models;
using StudentGradeManager.Services;

namespace StudentGradeManager.Forms
{
    public class TeacherMainForm : Form
    {
        private readonly AuthService _auth;
        private readonly StudentService _stuSvc;
        private readonly CourseService _crsSvc;
        private readonly GradeService _grdSvc;
        private readonly StatisticsService _sttSvc;
        private readonly AIService _aiSvc;

        public TeacherMainForm(AuthService auth, StudentService ss, CourseService cs,
            GradeService gs, StatisticsService sts, AIService ai)
        {
            _auth = auth; _stuSvc = ss; _crsSvc = cs;
            _grdSvc = gs; _sttSvc = sts; _aiSvc = ai;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "成绩管理系统 - 教师：" + (_auth.CurrentUser?.FullName ?? "");
            this.Size = new Size(950, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 550);

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10) };

            // ===== 成绩录入 =====
            var tabGrade = new TabPage("成绩录入");
            var pg = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
            var students = _stuSvc.GetAllStudents();
            var courses = _crsSvc.GetAllCourses();

            int y = 25;
            pg.Controls.Add(new Label { Text = "选择学生：", Location = new Point(30, y), Size = new Size(80, 25), Font = new Font("Microsoft YaHei", 10) });
            var cmbStu = new ComboBox { Location = new Point(120, y), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList, DataSource = students, DisplayMember = "Name", ValueMember = "Id" };
            pg.Controls.Add(cmbStu);

            y += 45; pg.Controls.Add(new Label { Text = "选择课程：", Location = new Point(30, y), Size = new Size(80, 25) });
            var cmbCrs = new ComboBox { Location = new Point(120, y), Size = new Size(250, 25), DropDownStyle = ComboBoxStyle.DropDownList, DataSource = courses, DisplayMember = "Name", ValueMember = "Id" };
            pg.Controls.Add(cmbCrs);

            y += 45; pg.Controls.Add(new Label { Text = "考试分数：", Location = new Point(30, y), Size = new Size(80, 25) });
            var txtScore = new TextBox { Location = new Point(120, y), Size = new Size(80, 25), Text = "85" };
            pg.Controls.Add(txtScore);

            y += 45; pg.Controls.Add(new Label { Text = "考试类型：", Location = new Point(30, y), Size = new Size(80, 25) });
            var cmbType = new ComboBox { Location = new Point(120, y), Size = new Size(120, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new[] { "期中", "期末", "平时" }); cmbType.SelectedIndex = 1;
            pg.Controls.Add(cmbType);

            y += 45; pg.Controls.Add(new Label { Text = "考试日期：", Location = new Point(30, y), Size = new Size(80, 25) });
            var dtp = new DateTimePicker { Location = new Point(120, y), Size = new Size(160, 25), Value = DateTime.Now };
            pg.Controls.Add(dtp);

            var btnSubmit = new Button { Text = "提交成绩", Location = new Point(120, y + 50), Size = new Size(120, 35), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, e) =>
            {
                if (!double.TryParse(txtScore.Text, out var sc) || sc < 0 || sc > 100) { MessageBox.Show("请输入 0-100 之间的分数。"); return; }
                _grdSvc.AddGrade(new Grade { StudentId = (int)cmbStu.SelectedValue!, CourseId = (int)cmbCrs.SelectedValue!, Score = sc, ExamType = cmbType.SelectedItem?.ToString() ?? "期末", ExamDate = dtp.Value });
                MessageBox.Show("成绩录入成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtScore.Text = "85";
            };
            pg.Controls.Add(btnSubmit);
            tabs.TabPages.Add(tabGrade);

            // ===== 成绩统计 =====
            var tabStats = new TabPage("成绩统计");
            var ps = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var dgv = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };
            var btnRef = new Button { Text = "刷新统计", Dock = DockStyle.Top, Height = 35, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRef.FlatAppearance.BorderSize = 0;
            btnRef.Click += (s, e) =>
            {
                var st = _sttSvc.GetStatsByCourse();
                dgv.DataSource = null;
                dgv.DataSource = st.Select(x => new { x.CourseName, 平均分 = x.AverageScore, 最高分 = x.MaxScore, 最低分 = x.MinScore, 学生数 = x.TotalStudents, 及格人数 = x.PassCount, 及格率 = x.PassRate + "%" }).ToList();
            };
            ps.Controls.Add(dgv); ps.Controls.Add(btnRef);
            tabs.TabPages.Add(tabStats);

            // ===== AI 分析 =====
            var tabAI = new TabPage("AI 智能分析");
            var pa = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45 };
            var cmbAI = new ComboBox { Size = new Size(170, 25), DropDownStyle = ComboBoxStyle.DropDownList, DataSource = _stuSvc.GetAllStudents(), DisplayMember = "Name", ValueMember = "Id" };
            var btnTrend = new Button { Text = "成绩波动分析", Size = new Size(140, 30), BackColor = Color.FromArgb(52, 73, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnTrend.FlatAppearance.BorderSize = 0;
            var btnComment = new Button { Text = "生成评语", Size = new Size(100, 30), BackColor = Color.FromArgb(41, 128, 185), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnComment.FlatAppearance.BorderSize = 0;
            bar.Controls.AddRange(new Control[] { new Label { Text = "选择学生：", Size = new Size(75, 25), TextAlign = ContentAlignment.MiddleRight }, cmbAI, btnTrend, btnComment });

            var rtb = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10), ReadOnly = true, BorderStyle = BorderStyle.FixedSingle };
            btnTrend.Click += async (s, e) => { if (cmbAI.SelectedValue is int sid) { rtb.Text = "正在分析..."; rtb.Text = await _aiSvc.AnalyzeGradeTrendAsync(sid); } };
            btnComment.Click += async (s, e) => { if (cmbAI.SelectedValue is int sid) { rtb.Text = "正在生成评语..."; rtb.Text = await _aiSvc.GenerateCommentAsync(sid); } };
            pa.Controls.Add(rtb); pa.Controls.Add(bar);
            tabs.TabPages.Add(tabAI);

            this.Controls.Add(tabs);
        }
    }
}
