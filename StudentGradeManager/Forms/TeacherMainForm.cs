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

        static readonly Color C_BG = Color.FromArgb(245, 247, 250);
        static readonly Color C_WHITE = Color.White;
        static readonly Color C_BLUE = Color.FromArgb(59, 130, 246);
        static readonly Color C_GREEN = Color.FromArgb(16, 185, 129);
        static readonly Color C_TEXT = Color.FromArgb(30, 41, 59);
        static readonly Color C_TEXT_LIGHT = Color.FromArgb(100, 116, 139);

        public TeacherMainForm(AuthService auth, StudentService ss, CourseService cs,
            GradeService gs, StatisticsService sts, AIService ai)
        {
            _auth = auth; _stuSvc = ss; _crsSvc = cs;
            _grdSvc = gs; _sttSvc = sts; _aiSvc = ai;
            InitializeComponent();
        }

        static void AutoFitGrid(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.Refresh();
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (DataGridViewColumn col in dgv.Columns)
                col.MinimumWidth = 70;
        }

        private void InitializeComponent()
        {
            this.Text = "成绩管理系统 - 教师：" + (_auth.CurrentUser?.FullName ?? "");
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 520);
            this.BackColor = C_BG;

            var topBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = C_WHITE };
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 47, topBar.Width, 47);
            topBar.Controls.Add(new Label { Text = "教师工作台", Font = new Font("Microsoft YaHei", 14, FontStyle.Bold), ForeColor = C_TEXT, Location = new Point(20, 10), Size = new Size(200, 28) });

            var sidebar = new Panel { Dock = DockStyle.Left, Width = 180, BackColor = Color.FromArgb(30, 41, 59) };
            sidebar.Controls.Add(new Label { Text = "功能菜单", Font = new Font("Microsoft YaHei", 9), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(16, 16), Size = new Size(150, 24) });

            var panels = new Panel[] { new Panel(), new Panel(), new Panel() };
            var btns = new Button[3];
            string[] items = { "成绩录入", "成绩统计", "AI 智能分析" };
            for (int i = 0; i < 3; i++)
            {
                btns[i] = new Button { Text = "  " + items[i], Font = new Font("Microsoft YaHei", 10), Size = new Size(165, 42), Location = new Point(8, 60 + i * 50), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, Tag = i };
                btns[i].FlatAppearance.BorderSize = 0;
                btns[i].Click += (s2, e2) => { for (int j = 0; j < 3; j++) { btns[j].BackColor = j == (int)((Button)s2!).Tag! ? C_BLUE : Color.FromArgb(30, 41, 59); panels[j].Visible = j == (int)((Button)s2!).Tag!; } };
                sidebar.Controls.Add(btns[i]);
            }

            var content = new Panel { Dock = DockStyle.Fill, BackColor = C_BG, Padding = new Padding(16), AutoScroll = true };
            for (int i = 0; i < 3; i++) { panels[i].Dock = DockStyle.Fill; content.Controls.Add(panels[i]); }

            // ---- 成绩录入 ----
            var pg = panels[0];
            var card1 = new Panel { Location = new Point(0, 8), BackColor = C_WHITE, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            card1.Width = content.ClientSize.Width - 16;
            content.Resize += (s, e) => card1.Width = content.ClientSize.Width - 16;

            var students = _stuSvc.GetAllStudents(); var courses = _crsSvc.GetAllCourses();
            int y = 25;
            void AddRow(string label, Control ctrl) { card1.Controls.Add(new Label { Text = label, Font = new Font("Microsoft YaHei", 10), ForeColor = C_TEXT_LIGHT, Location = new Point(30, y), Size = new Size(80, 28), TextAlign = ContentAlignment.MiddleRight }); ctrl.Location = new Point(120, y); ctrl.Font = new Font("Microsoft YaHei", 10); card1.Controls.Add(ctrl); y += 48; }
            var cmbStu = new ComboBox { Size = new Size(260, 28), DataSource = students, DisplayMember = "Name", ValueMember = "Id", DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat }; AddRow("学生：", cmbStu);
            var cmbCrs = new ComboBox { Size = new Size(260, 28), DataSource = courses, DisplayMember = "Name", ValueMember = "Id", DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat }; AddRow("课程：", cmbCrs);
            var txtScore = new TextBox { Size = new Size(80, 28), Text = "85", BorderStyle = BorderStyle.FixedSingle }; AddRow("分数：", txtScore);
            var cmbType = new ComboBox { Size = new Size(120, 28), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat }; cmbType.Items.AddRange(new[] { "期中", "期末", "平时" }); cmbType.SelectedIndex = 1; AddRow("类型：", cmbType);
            var dtp = new DateTimePicker { Size = new Size(160, 28), Value = DateTime.Now }; AddRow("日期：", dtp);
            var btnSubmit = new Button { Text = "提交成绩", Size = new Size(120, 38), Location = new Point(120, y + 10), FlatStyle = FlatStyle.Flat, BackColor = C_GREEN, ForeColor = Color.White, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold), Cursor = Cursors.Hand }; btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, e) => { if (!double.TryParse(txtScore.Text, out var sc) || sc < 0 || sc > 100) { MessageBox.Show("请输入 0-100 的分数。"); return; } _grdSvc.AddGrade(new Grade { StudentId = (int)cmbStu.SelectedValue!, CourseId = (int)cmbCrs.SelectedValue!, Score = sc, ExamType = cmbType.SelectedItem?.ToString() ?? "期末", ExamDate = dtp.Value }); MessageBox.Show("成绩录入成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information); txtScore.Text = "85"; };
            card1.Height = btnSubmit.Bottom + 20;
            card1.Controls.Add(btnSubmit);
            pg.Controls.Add(card1);

            // ---- 成绩统计 ----
            var ps = panels[1];
            var dgv = new DataGridView { Location = new Point(0, 8), BackgroundColor = C_WHITE, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(241, 245, 249), RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, EnableHeadersVisualStyles = false, ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252), ForeColor = C_TEXT_LIGHT, Font = new Font("Microsoft YaHei", 9, FontStyle.Bold), Padding = new Padding(0, 6, 0, 6) }, DefaultCellStyle = new DataGridViewCellStyle { BackColor = C_WHITE, ForeColor = C_TEXT, Font = new Font("Microsoft YaHei", 9), Padding = new Padding(4) }, AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252) }, RowTemplate = new DataGridViewRow { Height = 34 }, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            dgv.Width = content.ClientSize.Width - 16;
            content.Resize += (s, e) => dgv.Width = content.ClientSize.Width - 16;
            dgv.Height = 400;
            var btnRef = new Button { Text = "刷新统计", Location = new Point(0, 418), Size = new Size(120, 35), FlatStyle = FlatStyle.Flat, BackColor = C_BLUE, ForeColor = Color.White, Font = new Font("Microsoft YaHei", 9), Cursor = Cursors.Hand }; btnRef.FlatAppearance.BorderSize = 0;
            btnRef.Click += (s, e) => { var st = _sttSvc.GetStatsByCourse(); dgv.DataSource = null; dgv.DataSource = st.Select(x => new { x.CourseName, 平均分 = x.AverageScore, 最高分 = x.MaxScore, 最低分 = x.MinScore, 学生数 = x.TotalStudents, 及格人数 = x.PassCount, 及格率 = x.PassRate + "%" }).ToList(); AutoFitGrid(dgv); };
            ps.Controls.Add(dgv); ps.Controls.Add(btnRef);

            // ---- AI 分析 ----
            var pa = panels[2];
            var aiCard = new Panel { Location = new Point(0, 8), BackColor = C_WHITE, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            aiCard.Width = content.ClientSize.Width - 16;
            content.Resize += (s, e) => aiCard.Width = content.ClientSize.Width - 16;
            var cmbAI = new ComboBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(90, 15), Size = new Size(170, 28), DropDownStyle = ComboBoxStyle.DropDownList, DataSource = _stuSvc.GetAllStudents(), DisplayMember = "Name", ValueMember = "Id" };
            aiCard.Controls.Add(new Label { Text = "选择学生：", Font = new Font("Microsoft YaHei", 10), Location = new Point(16, 18), Size = new Size(72, 25), TextAlign = ContentAlignment.MiddleRight }); aiCard.Controls.Add(cmbAI);
            var btnTrend = new Button { Text = "成绩波动分析", Size = new Size(120, 32), Location = new Point(280, 13), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 9), Cursor = Cursors.Hand }; btnTrend.FlatAppearance.BorderSize = 0;
            var btnComment = new Button { Text = "生成评语", Size = new Size(100, 32), Location = new Point(410, 13), FlatStyle = FlatStyle.Flat, BackColor = C_BLUE, ForeColor = Color.White, Font = new Font("Microsoft YaHei", 9), Cursor = Cursors.Hand }; btnComment.FlatAppearance.BorderSize = 0;
            aiCard.Controls.Add(btnTrend); aiCard.Controls.Add(btnComment);
            var rtb = new RichTextBox { Location = new Point(16, 55), Font = new Font("Microsoft YaHei", 10), ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(248, 250, 252), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            rtb.Width = aiCard.Width - 32; rtb.Height = 300;
            aiCard.Resize += (s, e) => rtb.Width = aiCard.Width - 32;
            btnTrend.Click += async (s, e) => { if (cmbAI.SelectedValue is int sid) { rtb.Text = "正在分析..."; rtb.Text = await _aiSvc.AnalyzeGradeTrendAsync(sid); } };
            btnComment.Click += async (s, e) => { if (cmbAI.SelectedValue is int sid) { rtb.Text = "正在生成评语..."; rtb.Text = await _aiSvc.GenerateCommentAsync(sid); } };
            aiCard.Controls.Add(rtb); aiCard.Height = rtb.Bottom + 20;
            pa.Controls.Add(aiCard);

            this.Controls.Add(content); this.Controls.Add(sidebar); this.Controls.Add(topBar);
            btns[0].BackColor = C_BLUE; panels[0].Visible = true;
            for (int i = 1; i < 3; i++) panels[i].Visible = false;
        }
    }
}
