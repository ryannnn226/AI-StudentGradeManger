using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentGradeManager.Models;
using StudentGradeManager.Services;

namespace StudentGradeManager.Forms
{
    /// <summary>GDI+ 手绘柱状图</summary>
    public class BarChartPanel : Panel
    {
        private List<(string Label, double Value, Color Color)> _data = new();
        private string _title = "", _xAxis = "";
        private const int PL = 38, PR = 12, PT = 16, PB = 24;

        public void SetData(string title, string xAxis, List<(string Label, double Value, Color Color)> data)
        { _title = title; _xAxis = xAxis; _data = data; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            if (_data.Count == 0)
            {
                g.DrawString("暂无数据", new Font("Microsoft YaHei", 10), Brushes.Gray,
                    new RectangleF(0, 0, Width, Height),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                return;
            }

            int cw = Math.Max(1, Width - PL - PR);
            int ch = Math.Max(1, Height - PT - PB);
            double maxV = _data.Max(d => d.Value);
            if (maxV == 0) maxV = 100;

            int barW = Math.Min(60, Math.Max(20, cw / _data.Count - 12));
            int gap = (cw - barW * _data.Count) / (_data.Count + 1);

            using var tFont = new Font("Microsoft YaHei", 8, FontStyle.Bold);
            g.DrawString(_title, tFont, new SolidBrush(Color.FromArgb(30, 41, 59)),
                new RectangleF(0, 0, Width, 16),
                new StringFormat { Alignment = StringAlignment.Center });

            using var kFont = new Font("Microsoft YaHei", 6.5f);
            using var tPen = new Pen(Color.FromArgb(230, 235, 240));
            using var aPen = new Pen(Color.FromArgb(200, 205, 210));

            for (int i = 0; i <= 5; i++)
            {
                int y = PT + ch - ch * i / 5;
                double val = Math.Round(maxV * i / 5, 0);
                g.DrawString(val.ToString(), kFont, Brushes.Gray,
                    new RectangleF(0, y - 9, PL - 6, 18),
                    new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                if (i > 0) g.DrawLine(tPen, PL, y, PL + cw, y);
            }
            g.DrawLine(aPen, PL, PT, PL, PT + ch);
            g.DrawLine(aPen, PL, PT + ch, PL + cw, PT + ch);

            for (int i = 0; i < _data.Count; i++)
            {
                int x = PL + gap + i * (barW + gap);
                int bh = Math.Max(1, (int)(ch * _data[i].Value / maxV));
                int y = PT + ch - bh;
                var bar = new Rectangle(x, y, barW, bh);

                using var br = new System.Drawing.Drawing2D.LinearGradientBrush(bar,
                    _data[i].Color, ControlPaint.Light(_data[i].Color),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical);
                g.FillRectangle(br, bar);
                g.DrawRectangle(new Pen(ControlPaint.Dark(_data[i].Color)), bar);

                g.DrawString(_data[i].Value.ToString("F1"),
                    new Font("Microsoft YaHei", 6.5f, FontStyle.Bold), Brushes.Black,
                    new RectangleF(x, y - 13, barW, 12),
                    new StringFormat { Alignment = StringAlignment.Center });

                g.DrawString(_data[i].Label, kFont, Brushes.Gray,
                    new RectangleF(x - 8, PT + ch + 2, barW + 16, 22),
                    new StringFormat { Alignment = StringAlignment.Center });
            }
        }
    }
    public class AdminMainForm : Form
    {
        private readonly AuthService _auth;
        private readonly StudentService _stuSvc;
        private readonly CourseService _crsSvc;
        private readonly GradeService _grdSvc;
        private readonly StatisticsService _sttSvc;
        private readonly AIService _aiSvc;

        private Panel sidebar = null!, contentPanel = null!;
        private Label lblPageTitle = null!;
        private Button[] navButtons = null!;

        private DataGridView dgvStu = null!, dgvCrs = null!, dgvGrd = null!, dgvUsr = null!, dgvStats = null!;
        private RichTextBox rtbAI = null!;
        
        private ComboBox cmbAIStu = null!;

        public AdminMainForm(AuthService auth, StudentService ss, CourseService cs,
            GradeService gs, StatisticsService sts, AIService ai)
        {
            _auth = auth; _stuSvc = ss; _crsSvc = cs;
            _grdSvc = gs; _sttSvc = sts; _aiSvc = ai;
            InitUI();
        }

        static readonly Color C_BG = Color.FromArgb(245, 247, 250);
        static readonly Color C_SIDEBAR = Color.FromArgb(30, 41, 59);
        static readonly Color C_SIDEBAR_ACTIVE = Color.FromArgb(59, 130, 246);
        static readonly Color C_WHITE = Color.White;
        static readonly Color C_BLUE = Color.FromArgb(59, 130, 246);
        static readonly Color C_GREEN = Color.FromArgb(16, 185, 129);
        static readonly Color C_RED = Color.FromArgb(239, 68, 68);
        static readonly Color C_TEXT = Color.FromArgb(30, 41, 59);
        static readonly Color C_TEXT_LIGHT = Color.FromArgb(100, 116, 139);
        

        private void InitUI()
        {
            this.Text = "AI 智能成绩管理系统";
            this.Size = new Size(1150, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 550);
            this.BackColor = C_BG;

            var topBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = C_WHITE };
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 47, topBar.Width, 47);
            topBar.Controls.Add(new Label { Text = "AI 智能成绩管理系统", Font = new Font("Microsoft YaHei", 13, FontStyle.Bold), ForeColor = C_TEXT, Location = new Point(20, 10), Size = new Size(300, 28) });
            var lblUser = new Label { Text = "管理员：" + (_auth.CurrentUser?.FullName ?? ""), Font = new Font("Microsoft YaHei", 9), ForeColor = C_TEXT_LIGHT, TextAlign = ContentAlignment.MiddleRight };
            lblUser.Location = new Point(topBar.Width - 270, 14); lblUser.Size = new Size(250, 22); lblUser.Anchor = AnchorStyles.Right;
            topBar.Controls.Add(lblUser);

            sidebar = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = C_SIDEBAR };
            string[] navItems = { "学生管理", "课程管理", "成绩管理", "成绩统计", "AI 智能分析", "用户管理" };
            navButtons = new Button[navItems.Length];
            for (int i = 0; i < navItems.Length; i++)
            {
                var btn = new Button { Text = "  " + navItems[i], Font = new Font("Microsoft YaHei", 10), Size = new Size(185, 44), Location = new Point(8, 70 + i * 52), FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, Tag = i };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += NavClick;
                navButtons[i] = btn;
                sidebar.Controls.Add(btn);

            var btnLogout = new Button { Text = "退出登录", Font = new Font("Microsoft YaHei", 9), Size = new Size(185, 40), Location = new Point(8, 70 + navItems.Length * 52 + 16), FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(248, 113, 113), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { if (MessageBox.Show("确定要退出登录吗？", "退出", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) this.Close(); };
            sidebar.Controls.Add(btnLogout);
            }
            sidebar.Controls.Add(new Label { Text = "导航菜单", Font = new Font("Microsoft YaHei", 9), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(20, 20), Size = new Size(160, 24) });

            contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = C_BG, Padding = new Padding(16), AutoScroll = true };
            lblPageTitle = new Label { Text = "学生管理", Font = new Font("Microsoft YaHei", 16, FontStyle.Bold), ForeColor = C_TEXT, Location = new Point(16, 15), Size = new Size(400, 35) };
            contentPanel.Controls.Add(lblPageTitle);

            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);
            this.Controls.Add(topBar);
            SetActiveNav(0);
        }

        private void NavClick(object? sender, EventArgs e) { if (sender is Button btn && btn.Tag is int idx) SetActiveNav(idx); }

        private void SetActiveNav(int idx)
        {
            for (int i = 0; i < navButtons.Length; i++) navButtons[i].BackColor = i == idx ? C_SIDEBAR_ACTIVE : C_SIDEBAR;
            while (contentPanel.Controls.Count > 1) contentPanel.Controls.RemoveAt(1);
            lblPageTitle.Text = new[] { "学生管理", "课程管理", "成绩管理", "成绩统计", "AI 智能分析", "用户管理" }[idx];
            switch (idx) { case 0: BuildStudentPanel(); break; case 1: BuildCoursePanel(); break; case 2: BuildGradePanel(); break; case 3: BuildStatsPanel(); break; case 4: BuildAIPanel(); break; case 5: BuildUserPanel(); break; }
        }

        static Button MakeBtn(string text, Color c) => new Button { Text = text, Size = new Size(100, 32), FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Font = new Font("Microsoft YaHei", 9), Cursor = Cursors.Hand, UseVisualStyleBackColor = false };

        static DataGridView MakeGrid()
        {
            return new DataGridView
            {
                BackgroundColor = C_WHITE, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(241, 245, 249),
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false, ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 44,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252), ForeColor = C_TEXT_LIGHT, Font = new Font("Microsoft YaHei", 10, FontStyle.Bold), Padding = new Padding(8, 10, 8, 10) },
                DefaultCellStyle = new DataGridViewCellStyle { BackColor = C_WHITE, ForeColor = C_TEXT, Font = new Font("Microsoft YaHei", 9), SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = C_TEXT, Padding = new Padding(4) },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252) },
                RowTemplate = new DataGridViewRow { Height = 34 }
            };
        }

        static void AutoFitGrid(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgv.Refresh();
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (DataGridViewColumn col in dgv.Columns)
                col.MinimumWidth = 70;
        }

        /// <summary>创建带卡片的自适应布局：toolbar 顶部固定，grid 填充剩余空间</summary>
        Panel MakeResponsiveCard(int yOffset)
        {
            var card = new Panel { Location = new Point(16, yOffset), BackColor = C_WHITE };
            // card fills remaining width/height of contentPanel
            card.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            card.Width = contentPanel.ClientSize.Width - 32;
            card.Height = contentPanel.ClientSize.Height - yOffset - 16;
            contentPanel.Resize += (s, e) =>
            {
                card.Width = contentPanel.ClientSize.Width - 32;
                card.Height = contentPanel.ClientSize.Height - yOffset - 16;
            };
            return card;
        }

        // ============ 学生管理 ============
        void BuildStudentPanel()
        {
            var card = MakeResponsiveCard(60);
            var bar = new FlowLayoutPanel { Location = new Point(12, 12), Size = new Size(card.Width - 24, 36), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            bar.Controls.AddRange(new Control[] { MakeBtn("添加学生", C_GREEN), MakeBtn("编辑", C_BLUE), MakeBtn("删除", C_RED) });
            dgvStu = MakeGrid();
            dgvStu.Location = new Point(12, 55);
            dgvStu.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvStu.Width = card.Width - 24;
            dgvStu.Height = card.Height - 70;
            card.Resize += (s, e) => { bar.Width = card.Width - 24; dgvStu.Width = card.Width - 24; dgvStu.Height = card.Height - 70; };
            bar.Controls[0].Click += (s, e) => { DlgStudent(); RefreshStudents(); };
            bar.Controls[1].Click += (s, e) => EditStudent();
            bar.Controls[2].Click += (s, e) => DelStudent();
            card.Controls.Add(bar); card.Controls.Add(dgvStu);
            contentPanel.Controls.Add(card);
            RefreshStudents();
        }

        void RefreshStudents()
        {
            var list = _stuSvc.GetAllStudents().Select(s => new { s.Id, 学号 = s.StudentNo, 姓名 = s.Name, 性别 = s.Gender, 班级 = s.Class, 电话 = s.Phone, 邮箱 = s.Email }).ToList();
            dgvStu.DataSource = null; dgvStu.DataSource = list;
            if (dgvStu.Columns["Id"] != null) dgvStu.Columns["Id"].Visible = false;
            AutoFitGrid(dgvStu);
        }

        void EditStudent() { if (dgvStu.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvStu.CurrentRow.DataBoundItem; DlgStudent(new Student { Id = r.Id, StudentNo = r.学号, Name = r.姓名, Gender = r.性别, Class = r.班级, Phone = r.电话, Email = r.邮箱 }); RefreshStudents(); } }
        void DelStudent() { if (dgvStu.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvStu.CurrentRow.DataBoundItem; if (MessageBox.Show("确定删除 " + r.姓名 + " 及所有成绩？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { _stuSvc.DeleteStudent((int)r.Id); RefreshStudents(); } } }

        void DlgStudent(Student? ex = null)
        {
            var dlg = new Form { Text = ex == null ? "添加学生" : "编辑学生", Size = new Size(420, 370), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, BackColor = C_WHITE };
            var labels = new[] { "学号", "姓名", "性别", "班级", "电话", "邮箱" };
            var defs = ex != null ? new[] { ex.StudentNo, ex.Name, ex.Gender, ex.Class, ex.Phone, ex.Email } : new[] { "", "", "", "", "", "" };
            var boxes = new TextBox[6];
            for (int i = 0; i < 6; i++) { dlg.Controls.Add(new Label { Text = labels[i], Font = new Font("Microsoft YaHei", 10), ForeColor = C_TEXT_LIGHT, Location = new Point(25, 20 + i * 42), Size = new Size(50, 28), TextAlign = ContentAlignment.MiddleRight }); boxes[i] = new TextBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(85, 20 + i * 42), Size = new Size(290, 28), Text = defs[i], BorderStyle = BorderStyle.FixedSingle }; dlg.Controls.Add(boxes[i]); }
            var ok = new Button { Text = "确定", Font = new Font("Microsoft YaHei", 10), Location = new Point(145, 290), Size = new Size(110, 35), FlatStyle = FlatStyle.Flat, BackColor = C_BLUE, ForeColor = Color.White, Cursor = Cursors.Hand }; ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) => { var s2 = new Student { Id = ex?.Id ?? 0, StudentNo = boxes[0].Text.Trim(), Name = boxes[1].Text.Trim(), Gender = boxes[2].Text.Trim(), Class = boxes[3].Text.Trim(), Phone = boxes[4].Text.Trim(), Email = boxes[5].Text.Trim() }; if (string.IsNullOrEmpty(s2.StudentNo) || string.IsNullOrEmpty(s2.Name)) { MessageBox.Show("学号和姓名不能为空。"); return; } if (ex == null) _stuSvc.AddStudent(s2); else _stuSvc.UpdateStudent(s2); dlg.DialogResult = DialogResult.OK; dlg.Close(); };
            dlg.Controls.Add(ok); dlg.ShowDialog(this);
        }

        // ============ 课程管理 ============
        void BuildCoursePanel()
        {
            var card = MakeResponsiveCard(60);
            var bar = new FlowLayoutPanel { Location = new Point(12, 12), Size = new Size(card.Width - 24, 36), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            bar.Controls.AddRange(new Control[] { MakeBtn("添加课程", C_GREEN), MakeBtn("编辑", C_BLUE), MakeBtn("删除", C_RED) });
            dgvCrs = MakeGrid();
            dgvCrs.Location = new Point(12, 55); dgvCrs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvCrs.Width = card.Width - 24; dgvCrs.Height = card.Height - 70;
            card.Resize += (s, e) => { bar.Width = card.Width - 24; dgvCrs.Width = card.Width - 24; dgvCrs.Height = card.Height - 70; };
            bar.Controls[0].Click += (s, e) => { DlgCourse(); RefreshCourses(); };
            bar.Controls[1].Click += (s, e) => EditCourse();
            bar.Controls[2].Click += (s, e) => DelCourse();
            card.Controls.Add(bar); card.Controls.Add(dgvCrs);
            contentPanel.Controls.Add(card);
            RefreshCourses();
        }

        void RefreshCourses()
        {
            var list = _crsSvc.GetAllCourses().Select(c => new { c.Id, 课程编号 = c.CourseNo, 课程名称 = c.Name, 学分 = c.Credit, 授课教师 = c.Teacher, 学期 = c.Semester }).ToList();
            dgvCrs.DataSource = null; dgvCrs.DataSource = list;
            if (dgvCrs.Columns["Id"] != null) dgvCrs.Columns["Id"].Visible = false;
            AutoFitGrid(dgvCrs);
        }

        void EditCourse() { if (dgvCrs.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvCrs.CurrentRow.DataBoundItem; DlgCourse(new Course { Id = r.Id, CourseNo = r.课程编号, Name = r.课程名称, Credit = r.学分, Teacher = r.授课教师, Semester = r.学期 }); RefreshCourses(); } }
        void DelCourse() { if (dgvCrs.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvCrs.CurrentRow.DataBoundItem; if (MessageBox.Show("确定删除 " + r.课程名称 + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { _crsSvc.DeleteCourse((int)r.Id); RefreshCourses(); } } }

        void DlgCourse(Course? ex = null)
        {
            var dlg = new Form { Text = ex == null ? "添加课程" : "编辑课程", Size = new Size(420, 320), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, BackColor = C_WHITE };
            var labels = new[] { "课程编号", "课程名称", "学分", "授课教师", "开课学期" };
            var defs = ex != null ? new[] { ex.CourseNo, ex.Name, ex.Credit.ToString(), ex.Teacher, ex.Semester } : new[] { "", "", "3", "", "2025-2026-2" };
            var boxes = new TextBox[5];
            for (int i = 0; i < 5; i++) { dlg.Controls.Add(new Label { Text = labels[i], Font = new Font("Microsoft YaHei", 10), ForeColor = C_TEXT_LIGHT, Location = new Point(25, 20 + i * 42), Size = new Size(70, 28), TextAlign = ContentAlignment.MiddleRight }); boxes[i] = new TextBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(105, 20 + i * 42), Size = new Size(270, 28), Text = defs[i], BorderStyle = BorderStyle.FixedSingle }; dlg.Controls.Add(boxes[i]); }
            var ok = new Button { Text = "确定", Font = new Font("Microsoft YaHei", 10), Location = new Point(145, 245), Size = new Size(110, 35), FlatStyle = FlatStyle.Flat, BackColor = C_BLUE, ForeColor = Color.White, Cursor = Cursors.Hand }; ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) => { var c = new Course { Id = ex?.Id ?? 0, CourseNo = boxes[0].Text.Trim(), Name = boxes[1].Text.Trim(), Credit = double.TryParse(boxes[2].Text, out var cr) ? cr : 3, Teacher = boxes[3].Text.Trim(), Semester = boxes[4].Text.Trim() }; if (string.IsNullOrEmpty(c.CourseNo) || string.IsNullOrEmpty(c.Name)) return; if (ex == null) _crsSvc.AddCourse(c); else _crsSvc.UpdateCourse(c); dlg.DialogResult = DialogResult.OK; dlg.Close(); };
            dlg.Controls.Add(ok); dlg.ShowDialog(this);
        }

        // ============ 成绩管理 ============
        void BuildGradePanel()
        {
            var card = MakeResponsiveCard(60);
            var bar = new FlowLayoutPanel { Location = new Point(12, 12), Size = new Size(card.Width - 24, 36), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            bar.Controls.AddRange(new Control[] { MakeBtn("录入成绩", C_GREEN), MakeBtn("删除", C_RED) });
            dgvGrd = MakeGrid();
            dgvGrd.Location = new Point(12, 55); dgvGrd.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvGrd.Width = card.Width - 24; dgvGrd.Height = card.Height - 70;
            card.Resize += (s, e) => { bar.Width = card.Width - 24; dgvGrd.Width = card.Width - 24; dgvGrd.Height = card.Height - 70; };
            bar.Controls[0].Click += (s, e) => { DlgGrade(); RefreshGrades(); };
            bar.Controls[1].Click += (s, e) => DelGrade();
            card.Controls.Add(bar); card.Controls.Add(dgvGrd);
            contentPanel.Controls.Add(card);
            RefreshGrades();
        }

        void RefreshGrades()
        {
            var list = _grdSvc.GetAllGradesWithDetails().Select(g => new { g.Grade.Id, 学生 = g.StudentName, 课程 = g.CourseName, 班级 = g.StudentClass, 分数 = g.Grade.Score, 考试类型 = g.Grade.ExamType, 日期 = g.Grade.ExamDate.ToString("yyyy-MM-dd") }).ToList();
            dgvGrd.DataSource = null; dgvGrd.DataSource = list;
            if (dgvGrd.Columns["Id"] != null) dgvGrd.Columns["Id"].Visible = false;
            AutoFitGrid(dgvGrd);
        }

        void DelGrade() { if (dgvGrd.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvGrd.CurrentRow.DataBoundItem; if (MessageBox.Show("确定删除该成绩？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { _grdSvc.DeleteGrade((int)r.Id); RefreshGrades(); } } }

        void DlgGrade()
        {
            var students = _stuSvc.GetAllStudents(); var courses = _crsSvc.GetAllCourses();
            var dlg = new Form { Text = "录入成绩", Size = new Size(400, 300), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, BackColor = C_WHITE };
            var cmbStu = new ComboBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(90, 22), Size = new Size(270, 25), DataSource = students, DisplayMember = "Name", ValueMember = "Id", DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            var cmbCrs = new ComboBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(90, 62), Size = new Size(270, 25), DataSource = courses, DisplayMember = "Name", ValueMember = "Id", DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
            var txtScore = new TextBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(90, 102), Size = new Size(80, 25), Text = "85", BorderStyle = BorderStyle.FixedSingle };
            var cmbType = new ComboBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(90, 142), Size = new Size(110, 25), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat }; cmbType.Items.AddRange(new[] { "期中", "期末", "平时" }); cmbType.SelectedIndex = 1;
            var dtp = new DateTimePicker { Font = new Font("Microsoft YaHei", 10), Location = new Point(90, 182), Size = new Size(150, 25), Value = DateTime.Now };
            dlg.Controls.AddRange(new Control[] { new Label { Text = "学生", Location = new Point(20, 25), Size = new Size(65, 25), TextAlign = ContentAlignment.MiddleRight }, cmbStu, new Label { Text = "课程", Location = new Point(20, 65), Size = new Size(65, 25), TextAlign = ContentAlignment.MiddleRight }, cmbCrs, new Label { Text = "分数", Location = new Point(20, 105), Size = new Size(65, 25), TextAlign = ContentAlignment.MiddleRight }, txtScore, new Label { Text = "类型", Location = new Point(20, 145), Size = new Size(65, 25), TextAlign = ContentAlignment.MiddleRight }, cmbType, new Label { Text = "日期", Location = new Point(20, 185), Size = new Size(65, 25), TextAlign = ContentAlignment.MiddleRight }, dtp });
            var ok = new Button { Text = "确定", Font = new Font("Microsoft YaHei", 10), Location = new Point(135, 225), Size = new Size(110, 35), FlatStyle = FlatStyle.Flat, BackColor = C_BLUE, ForeColor = Color.White, Cursor = Cursors.Hand }; ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) => { if (!double.TryParse(txtScore.Text, out var sc) || sc < 0 || sc > 100) { MessageBox.Show("请输入 0-100 的分数。"); return; } _grdSvc.AddGrade(new Grade { StudentId = (int)cmbStu.SelectedValue!, CourseId = (int)cmbCrs.SelectedValue!, Score = sc, ExamType = cmbType.SelectedItem?.ToString() ?? "期末", ExamDate = dtp.Value }); dlg.DialogResult = DialogResult.OK; dlg.Close(); };
            dlg.Controls.Add(ok); dlg.ShowDialog(this);
        }

        // ============ 成绩统计 ============
        void BuildStatsPanel()
        {
            var card = MakeResponsiveCard(60);
            var overviewCard = new Panel { Location = new Point(12, 12), Size = new Size(card.Width - 24, 80), BackColor = Color.FromArgb(248, 250, 252), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            var lblOverview = new Label { Location = new Point(15, 10), Size = new Size(overviewCard.Width - 30, 60), Font = new Font("Microsoft YaHei", 10), ForeColor = C_TEXT, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            overviewCard.Controls.Add(lblOverview);

            dgvStats = MakeGrid();
            dgvStats.Location = new Point(12, 100); dgvStats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvStats.Width = card.Width - 24; dgvStats.Height = card.Height - 165;
            card.Resize += (s, e) => { overviewCard.Width = card.Width - 24; dgvStats.Width = card.Width - 24; dgvStats.Height = card.Height - 165; };

            var btnChart = MakeBtn("📊 查看图表", C_GREEN);
            btnChart.Location = new Point(12, card.Height - 50); btnChart.Size = new Size(140, 32);
            btnChart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnChart.Click += (s, e) => { var f = new ChartsForm(_sttSvc, _crsSvc); f.ShowDialog(this); };

            var btnRefresh = MakeBtn("刷新统计", C_BLUE);
            btnRefresh.Location = new Point(162, card.Height - 50); btnRefresh.Size = new Size(120, 32);
            btnRefresh.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRefresh.Click += (s, e) =>
            {
                var ov = _sttSvc.GetOverallStats();
                lblOverview.Text = "  成绩总数：" + ov.TotalGrades + "    学生总数：" + ov.TotalStudents + "    课程总数：" + ov.TotalCourses + "    总平均分：" + ov.OverallAverage;
                var st = _sttSvc.GetStatsByCourse();
                dgvStats.DataSource = null;
                dgvStats.DataSource = st.Select(x => new { x.CourseName, 平均分 = x.AverageScore, 最高分 = x.MaxScore, 最低分 = x.MinScore, 学生数 = x.TotalStudents, 及格人数 = x.PassCount, 及格率 = x.PassRate + "%" }).ToList();
                AutoFitGrid(dgvStats);
            };
            card.Resize += (s, e) => { btnChart.Location = new Point(12, card.Height - 50); btnRefresh.Location = new Point(162, card.Height - 50); };

            card.Controls.Add(overviewCard); card.Controls.Add(dgvStats); card.Controls.Add(btnChart); card.Controls.Add(btnRefresh);
            contentPanel.Controls.Add(card);
        }
// ============ AI 分析 ============
        void BuildAIPanel()
        {
            var card = MakeResponsiveCard(60);
            var bar = new FlowLayoutPanel { Location = new Point(12, 12), Size = new Size(card.Width - 24, 36), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            cmbAIStu = new ComboBox { Font = new Font("Microsoft YaHei", 10), Size = new Size(170, 28), DropDownStyle = ComboBoxStyle.DropDownList, DataSource = _stuSvc.GetAllStudents(), DisplayMember = "Name", ValueMember = "Id" };
            bar.Controls.AddRange(new Control[] { new Label { Text = "选择学生：", Font = new Font("Microsoft YaHei", 10), Size = new Size(75, 28), TextAlign = ContentAlignment.MiddleRight }, cmbAIStu, MakeBtn("成绩波动分析", Color.FromArgb(30, 41, 59)), MakeBtn("生成评语", C_BLUE) });
            rtbAI = new RichTextBox { Location = new Point(12, 55), Font = new Font("Microsoft YaHei", 10), ReadOnly = true, BackColor = C_WHITE, BorderStyle = BorderStyle.None, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            rtbAI.Width = card.Width - 24; rtbAI.Height = card.Height - 70;
            card.Resize += (s, e) => { rtbAI.Width = card.Width - 24; rtbAI.Height = card.Height - 70; bar.Width = card.Width - 24; };
            bar.Controls[2].Click += async (s, e) => { if (cmbAIStu.SelectedValue is int sid) { rtbAI.Text = "正在分析，请稍候...\n"; rtbAI.Text = await _aiSvc.AnalyzeGradeTrendAsync(sid); } };
            bar.Controls[3].Click += async (s, e) => { if (cmbAIStu.SelectedValue is int sid) { rtbAI.Text = "正在生成评语，请稍候...\n"; rtbAI.Text = await _aiSvc.GenerateCommentAsync(sid); } };
            card.Controls.Add(bar); card.Controls.Add(rtbAI);
            contentPanel.Controls.Add(card);
        }

        // ============ 用户管理 ============
        void BuildUserPanel()
        {
            var card = MakeResponsiveCard(60);
            var bar = new FlowLayoutPanel { Location = new Point(12, 12), Size = new Size(card.Width - 24, 36), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            bar.Controls.AddRange(new Control[] { MakeBtn("添加用户", C_GREEN), MakeBtn("删除", C_RED) });
            dgvUsr = MakeGrid();
            dgvUsr.Location = new Point(12, 55); dgvUsr.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvUsr.Width = card.Width - 24; dgvUsr.Height = card.Height - 70;
            card.Resize += (s, e) => { bar.Width = card.Width - 24; dgvUsr.Width = card.Width - 24; dgvUsr.Height = card.Height - 70; };
            bar.Controls[0].Click += (s, e) => { DlgUser(); RefreshUsers(); };
            bar.Controls[1].Click += (s, e) => DelUser();
            card.Controls.Add(bar); card.Controls.Add(dgvUsr);
            contentPanel.Controls.Add(card);
            RefreshUsers();
        }

        void RefreshUsers()
        {
            var list = _auth.GetAllUsers().Select(u => new { u.Id, 用户名 = u.Username, 角色 = u.Role, 姓名 = u.FullName }).ToList();
            dgvUsr.DataSource = null; dgvUsr.DataSource = list;
            if (dgvUsr.Columns["Id"] != null) dgvUsr.Columns["Id"].Visible = false;
            AutoFitGrid(dgvUsr);
        }

        void DelUser() { if (dgvUsr.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvUsr.CurrentRow.DataBoundItem; if ((string)r.用户名 == "admin") { MessageBox.Show("不能删除管理员账号！"); return; } if (MessageBox.Show("确定删除 " + r.用户名 + "？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { _auth.DeleteUser((int)r.Id); RefreshUsers(); } } }

        void DlgUser()
        {
            var dlg = new Form { Text = "添加用户", Size = new Size(400, 260), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, BackColor = C_WHITE };
            var txtU = new TextBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(100, 22), Size = new Size(250, 25), BorderStyle = BorderStyle.FixedSingle };
            var txtN = new TextBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(100, 62), Size = new Size(250, 25), BorderStyle = BorderStyle.FixedSingle };
            var cmbR = new ComboBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(100, 102), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat }; cmbR.Items.AddRange(new[] { "Admin", "Teacher", "Student" }); cmbR.SelectedIndex = 2;
            dlg.Controls.AddRange(new Control[] { new Label { Text = "用户名", Location = new Point(25, 25), Size = new Size(70, 25), TextAlign = ContentAlignment.MiddleRight }, txtU, new Label { Text = "真实姓名", Location = new Point(25, 65), Size = new Size(70, 25), TextAlign = ContentAlignment.MiddleRight }, txtN, new Label { Text = "角色", Location = new Point(25, 105), Size = new Size(70, 25), TextAlign = ContentAlignment.MiddleRight }, cmbR });
            var ok = new Button { Text = "确定（默认密码 123456）", Font = new Font("Microsoft YaHei", 10), Location = new Point(90, 160), Size = new Size(200, 35), FlatStyle = FlatStyle.Flat, BackColor = C_GREEN, ForeColor = Color.White, Cursor = Cursors.Hand }; ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) => { if (string.IsNullOrEmpty(txtU.Text)) return; _auth.AddUser(new User { Username = txtU.Text.Trim(), FullName = txtN.Text.Trim(), Role = cmbR.SelectedItem?.ToString() ?? "Student" }); dlg.DialogResult = DialogResult.OK; dlg.Close(); };
            dlg.Controls.Add(ok); dlg.ShowDialog(this);
        }
    }
}
