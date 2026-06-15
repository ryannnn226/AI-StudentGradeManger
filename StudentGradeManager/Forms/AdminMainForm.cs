using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentGradeManager.Models;
using StudentGradeManager.Services;

namespace StudentGradeManager.Forms
{
    public class AdminMainForm : Form
    {
        private readonly AuthService _auth;
        private readonly StudentService _stuSvc;
        private readonly CourseService _crsSvc;
        private readonly GradeService _grdSvc;
        private readonly StatisticsService _sttSvc;
        private readonly AIService _aiSvc;
        private TabControl tabs = null!;
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

        private void InitUI()
        {
            this.Text = "AI 成绩管理系统 - 管理员：" + (_auth.CurrentUser?.FullName ?? "");
            this.Size = new Size(1050, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 600);

            tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10) };
            tabs.TabPages.Add(MakeStudentTab());
            tabs.TabPages.Add(MakeCourseTab());
            tabs.TabPages.Add(MakeGradeTab());
            tabs.TabPages.Add(MakeStatsTab());
            tabs.TabPages.Add(MakeAITab());
            tabs.TabPages.Add(MakeUserTab());

            var status = new Label { Text = "当前用户：" + (_auth.CurrentUser?.FullName ?? "") + " | 角色：管理员", Dock = DockStyle.Bottom, Height = 25, BackColor = Color.FromArgb(236, 240, 241), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(5, 0, 0, 0) };

            this.Controls.Add(tabs);
            this.Controls.Add(status);

            RefreshStudents();
            RefreshCourses();
            RefreshGrades();
            RefreshUsers();
        }

        static Button Btn(string text, Color c) => new Button { Text = text, Size = new Size(110, 30), BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Microsoft YaHei", 9), Margin = new Padding(3), UseVisualStyleBackColor = false };

        // ============ 学生管理 ============
        TabPage MakeStudentTab()
        {
            var tab = new TabPage("学生管理");
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(0, 5, 0, 5) };
            bar.Controls.AddRange(new Control[] { Btn("添加学生", Color.FromArgb(46, 204, 113)), Btn("编辑学生", Color.FromArgb(52, 152, 219)), Btn("删除学生", Color.FromArgb(231, 76, 60)), Btn("刷新", Color.FromArgb(149, 165, 166)) });

            dgvStu = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };

            ((Button)bar.Controls[0]).Click += (s, e) => { DlgStudent(); RefreshStudents(); };
            ((Button)bar.Controls[1]).Click += (s, e) => EditStudent();
            ((Button)bar.Controls[2]).Click += (s, e) => DelStudent();
            ((Button)bar.Controls[3]).Click += (s, e) => RefreshStudents();

            pnl.Controls.Add(dgvStu); pnl.Controls.Add(bar);
            tab.Controls.Add(pnl); return tab;
        }

        void RefreshStudents()
        {
            var list = _stuSvc.GetAllStudents().Select(s => new { s.Id, 学号 = s.StudentNo, 姓名 = s.Name, 性别 = s.Gender, 班级 = s.Class, 电话 = s.Phone, 邮箱 = s.Email }).ToList();
            dgvStu.DataSource = null; dgvStu.DataSource = list;
            if (dgvStu.Columns["Id"] != null) dgvStu.Columns["Id"].Visible = false;
        }

        void EditStudent()
        {
            if (dgvStu.CurrentRow?.DataBoundItem is null) return;
            dynamic r = dgvStu.CurrentRow.DataBoundItem;
            DlgStudent(new Student { Id = r.Id, StudentNo = r.学号, Name = r.姓名, Gender = r.性别, Class = r.班级, Phone = r.电话, Email = r.邮箱 });
            RefreshStudents();
        }

        void DelStudent()
        {
            if (dgvStu.CurrentRow?.DataBoundItem is null) return;
            dynamic r = dgvStu.CurrentRow.DataBoundItem;
            if (MessageBox.Show("确定删除学生 " + r.姓名 + " 及其所有成绩吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            { _stuSvc.DeleteStudent((int)r.Id); RefreshStudents(); }
        }

        void DlgStudent(Student? ex = null)
        {
            var dlg = new Form { Text = ex == null ? "添加学生" : "编辑学生", Size = new Size(450, 380), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
            var fields = MakeStudentFields(dlg, ex);
            var ok = new Button { Text = "确定", Location = new Point(160, 290), Size = new Size(100, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) =>
            {
                var s2 = new Student { Id = ex?.Id ?? 0, StudentNo = fields[0].Text.Trim(), Name = fields[1].Text.Trim(), Gender = fields[2].Text.Trim(), Class = fields[3].Text.Trim(), Phone = fields[4].Text.Trim(), Email = fields[5].Text.Trim() };
                if (string.IsNullOrEmpty(s2.StudentNo) || string.IsNullOrEmpty(s2.Name)) { MessageBox.Show("学号和姓名不能为空。"); return; }
                if (ex == null) _stuSvc.AddStudent(s2); else _stuSvc.UpdateStudent(s2);
                dlg.DialogResult = DialogResult.OK; dlg.Close();
            };
            dlg.Controls.Add(ok);
            dlg.ShowDialog(this);
        }

        TextBox[] MakeStudentFields(Form dlg, Student? ex)
        {
            var labels = new[] { "学号", "姓名", "性别", "班级", "电话", "邮箱" };
            var defs = ex != null ? new[] { ex.StudentNo, ex.Name, ex.Gender, ex.Class, ex.Phone, ex.Email } : new[] { "", "", "", "", "", "" };
            var boxes = new TextBox[6];
            for (int i = 0; i < 6; i++)
            {
                dlg.Controls.Add(new Label { Text = labels[i] + "：", Location = new Point(30, 25 + i * 40), Size = new Size(60, 25) });
                boxes[i] = new TextBox { Location = new Point(100, 25 + i * 40), Size = new Size(300, 25), Text = defs[i] };
                dlg.Controls.Add(boxes[i]);
            }
            return boxes;
        }

        // ============ 课程管理 ============
        TabPage MakeCourseTab()
        {
            var tab = new TabPage("课程管理");
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            bar.Controls.AddRange(new Control[] { Btn("添加课程", Color.FromArgb(46, 204, 113)), Btn("编辑课程", Color.FromArgb(52, 152, 219)), Btn("删除课程", Color.FromArgb(231, 76, 60)) });

            dgvCrs = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };

            ((Button)bar.Controls[0]).Click += (s, e) => { DlgCourse(); RefreshCourses(); };
            ((Button)bar.Controls[1]).Click += (s, e) => EditCourse();
            ((Button)bar.Controls[2]).Click += (s, e) => DelCourse();

            pnl.Controls.Add(dgvCrs); pnl.Controls.Add(bar);
            tab.Controls.Add(pnl); return tab;
        }

        void RefreshCourses()
        {
            var list = _crsSvc.GetAllCourses().Select(c => new { c.Id, 课程编号 = c.CourseNo, 课程名称 = c.Name, 学分 = c.Credit, 授课教师 = c.Teacher, 学期 = c.Semester }).ToList();
            dgvCrs.DataSource = null; dgvCrs.DataSource = list;
            if (dgvCrs.Columns["Id"] != null) dgvCrs.Columns["Id"].Visible = false;
        }

        void EditCourse() { if (dgvCrs.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvCrs.CurrentRow.DataBoundItem; DlgCourse(new Course { Id = r.Id, CourseNo = r.课程编号, Name = r.课程名称, Credit = r.学分, Teacher = r.授课教师, Semester = r.学期 }); RefreshCourses(); } }
        void DelCourse() { if (dgvCrs.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvCrs.CurrentRow.DataBoundItem; if (MessageBox.Show("确定删除课程 " + r.课程名称 + " 吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { _crsSvc.DeleteCourse((int)r.Id); RefreshCourses(); } } }

        void DlgCourse(Course? ex = null)
        {
            var dlg = new Form { Text = ex == null ? "添加课程" : "编辑课程", Size = new Size(450, 340), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
            var labels = new[] { "课程编号", "课程名称", "学分", "授课教师", "开课学期" };
            var defs = ex != null ? new[] { ex.CourseNo, ex.Name, ex.Credit.ToString(), ex.Teacher, ex.Semester } : new[] { "", "", "3", "", "2025-2026-2" };
            var boxes = new TextBox[5];
            for (int i = 0; i < 5; i++)
            {
                dlg.Controls.Add(new Label { Text = labels[i] + "：", Location = new Point(30, 25 + i * 42), Size = new Size(80, 25) });
                boxes[i] = new TextBox { Location = new Point(115, 25 + i * 42), Size = new Size(290, 25), Text = defs[i] };
                dlg.Controls.Add(boxes[i]);
            }
            var ok = new Button { Text = "确定", Location = new Point(160, 250), Size = new Size(100, 35), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) =>
            {
                var c = new Course { Id = ex?.Id ?? 0, CourseNo = boxes[0].Text.Trim(), Name = boxes[1].Text.Trim(), Credit = double.TryParse(boxes[2].Text, out var cr) ? cr : 3, Teacher = boxes[3].Text.Trim(), Semester = boxes[4].Text.Trim() };
                if (string.IsNullOrEmpty(c.CourseNo) || string.IsNullOrEmpty(c.Name)) return;
                if (ex == null) _crsSvc.AddCourse(c); else _crsSvc.UpdateCourse(c);
                dlg.DialogResult = DialogResult.OK; dlg.Close();
            };
            dlg.Controls.Add(ok); dlg.ShowDialog(this);
        }

        // ============ 成绩管理 ============
        TabPage MakeGradeTab()
        {
            var tab = new TabPage("成绩管理");
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            bar.Controls.AddRange(new Control[] { Btn("录入成绩", Color.FromArgb(46, 204, 113)), Btn("删除成绩", Color.FromArgb(231, 76, 60)), Btn("刷新", Color.FromArgb(149, 165, 166)) });

            dgvGrd = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };

            ((Button)bar.Controls[0]).Click += (s, e) => { DlgGrade(); RefreshGrades(); };
            ((Button)bar.Controls[1]).Click += (s, e) => DelGrade();
            ((Button)bar.Controls[2]).Click += (s, e) => RefreshGrades();

            pnl.Controls.Add(dgvGrd); pnl.Controls.Add(bar);
            tab.Controls.Add(pnl); return tab;
        }

        void RefreshGrades()
        {
            var list = _grdSvc.GetAllGradesWithDetails().Select(g => new { g.Grade.Id, 学生 = g.StudentName, 课程 = g.CourseName, 班级 = g.StudentClass, 分数 = g.Grade.Score, 考试类型 = g.Grade.ExamType, 日期 = g.Grade.ExamDate.ToString("yyyy-MM-dd") }).ToList();
            dgvGrd.DataSource = null; dgvGrd.DataSource = list;
            if (dgvGrd.Columns["Id"] != null) dgvGrd.Columns["Id"].Visible = false;
        }

        void DelGrade() { if (dgvGrd.CurrentRow?.DataBoundItem is not null) { dynamic r = dgvGrd.CurrentRow.DataBoundItem; if (MessageBox.Show("确定删除该成绩记录吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { _grdSvc.DeleteGrade((int)r.Id); RefreshGrades(); } } }

        void DlgGrade()
        {
            var students = _stuSvc.GetAllStudents();
            var courses = _crsSvc.GetAllCourses();
            var dlg = new Form { Text = "录入成绩", Size = new Size(420, 300), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };

            var cmbStu = new ComboBox { Location = new Point(100, 25), Size = new Size(280, 25), DataSource = students, DisplayMember = "Name", ValueMember = "Id", DropDownStyle = ComboBoxStyle.DropDownList };
            var cmbCrs = new ComboBox { Location = new Point(100, 70), Size = new Size(280, 25), DataSource = courses, DisplayMember = "Name", ValueMember = "Id", DropDownStyle = ComboBoxStyle.DropDownList };
            var txtScore = new TextBox { Location = new Point(100, 115), Size = new Size(80, 25), Text = "85" };
            var cmbType = new ComboBox { Location = new Point(100, 160), Size = new Size(120, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new[] { "期中", "期末", "平时" }); cmbType.SelectedIndex = 1;
            var dtp = new DateTimePicker { Location = new Point(100, 205), Size = new Size(160, 25), Value = DateTime.Now };

            dlg.Controls.AddRange(new Control[] { new Label { Text = "学生：", Location = new Point(30, 28), Size = new Size(65, 25) }, cmbStu, new Label { Text = "课程：", Location = new Point(30, 73), Size = new Size(65, 25) }, cmbCrs, new Label { Text = "分数：", Location = new Point(30, 118), Size = new Size(65, 25) }, txtScore, new Label { Text = "类型：", Location = new Point(30, 163), Size = new Size(65, 25) }, cmbType, new Label { Text = "日期：", Location = new Point(30, 208), Size = new Size(65, 25) }, dtp });

            var ok = new Button { Text = "确定", Location = new Point(150, 240), Size = new Size(100, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) =>
            {
                if (!double.TryParse(txtScore.Text, out var sc) || sc < 0 || sc > 100) { MessageBox.Show("请输入 0-100 之间的分数。"); return; }
                _grdSvc.AddGrade(new Grade { StudentId = (int)cmbStu.SelectedValue!, CourseId = (int)cmbCrs.SelectedValue!, Score = sc, ExamType = cmbType.SelectedItem?.ToString() ?? "期末", ExamDate = dtp.Value });
                dlg.DialogResult = DialogResult.OK; dlg.Close();
            };
            dlg.Controls.Add(ok); dlg.ShowDialog(this);
        }

        // ============ 成绩统计 ============
        TabPage MakeStatsTab()
        {
            var tab = new TabPage("成绩统计");
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 420 };

            var left = new Panel { Dock = DockStyle.Fill };
            var lblOverall = new Label { Dock = DockStyle.Top, Height = 100, Font = new Font("Microsoft YaHei", 10), TextAlign = ContentAlignment.TopLeft, Padding = new Padding(10) };
            var btnRefresh = Btn("刷新统计", Color.FromArgb(52, 152, 219));
            btnRefresh.Dock = DockStyle.Top; btnRefresh.Height = 35; btnRefresh.FlatAppearance.BorderSize = 0;

            dgvStats = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false };

            btnRefresh.Click += (s, e) =>
            {
                var ov = _sttSvc.GetOverallStats();
                lblOverall.Text = "综合统计" + Environment.NewLine + "成绩总数：" + ov.TotalGrades + Environment.NewLine + "学生总数：" + ov.TotalStudents + Environment.NewLine + "课程总数：" + ov.TotalCourses + Environment.NewLine + "总平均分：" + ov.OverallAverage;

                var st = _sttSvc.GetStatsByCourse();
                dgvStats.DataSource = null;
                dgvStats.DataSource = st.Select(x => new { x.CourseName, 平均分 = x.AverageScore, 最高分 = x.MaxScore, 最低分 = x.MinScore, 学生数 = x.TotalStudents, 及格人数 = x.PassCount, 及格率 = x.PassRate + "%" }).ToList();
            };

            left.Controls.Add(dgvStats); left.Controls.Add(btnRefresh); left.Controls.Add(lblOverall);

            var right = new Panel { Dock = DockStyle.Fill };
            var lblRank = new Label { Text = "班级排名", Dock = DockStyle.Top, Height = 30, Font = new Font("Microsoft YaHei", 12, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            var lstRank = new ListBox { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10) };
            var btnRank = Btn("查看班级排名", Color.FromArgb(155, 89, 182));
            btnRank.Dock = DockStyle.Top; btnRank.Height = 35; btnRank.FlatAppearance.BorderSize = 0;
            btnRank.Click += (s, e) =>
            {
                lstRank.Items.Clear();
                var ranks = _sttSvc.GetStatsByClass();
                for (int i = 0; i < ranks.Count; i++)
                    lstRank.Items.Add("第" + (i + 1) + "名：" + ranks[i].ClassName + " - 平均分 " + ranks[i].AvgScore);
            };

            right.Controls.Add(lstRank); right.Controls.Add(btnRank); right.Controls.Add(lblRank);

            split.Panel1.Controls.Add(left); split.Panel2.Controls.Add(right);
            pnl.Controls.Add(split); tab.Controls.Add(pnl); return tab;
        }

        // ============ AI 分析 ============
        TabPage MakeAITab()
        {
            var tab = new TabPage("AI 智能分析");
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 5, 0, 5) };
            var lbl = new Label { Text = "选择学生：", Size = new Size(75, 25), TextAlign = ContentAlignment.MiddleRight };
            cmbAIStu = new ComboBox { Size = new Size(170, 25), DropDownStyle = ComboBoxStyle.DropDownList, DataSource = _stuSvc.GetAllStudents(), DisplayMember = "Name", ValueMember = "Id" };
            var btnTrend = Btn("成绩波动分析", Color.FromArgb(52, 73, 94));
            var btnComment = Btn("生成评语", Color.FromArgb(41, 128, 185));
            bar.Controls.AddRange(new Control[] { lbl, cmbAIStu, btnTrend, btnComment });

            rtbAI = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Microsoft YaHei", 10), ReadOnly = true, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            btnTrend.Click += async (s, e) => { if (cmbAIStu.SelectedValue is int sid) { rtbAI.Text = "正在调用 AI 分析，请稍候..."; rtbAI.Text = await _aiSvc.AnalyzeGradeTrendAsync(sid); } };
            btnComment.Click += async (s, e) => { if (cmbAIStu.SelectedValue is int sid) { rtbAI.Text = "正在调用 AI 生成评语，请稍候..."; rtbAI.Text = await _aiSvc.GenerateCommentAsync(sid); } };

            pnl.Controls.Add(rtbAI); pnl.Controls.Add(bar);
            tab.Controls.Add(pnl); return tab;
        }

        // ============ 用户管理 ============
        TabPage MakeUserTab()
        {
            var tab = new TabPage("用户管理");
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 40 };
            bar.Controls.AddRange(new Control[] { Btn("添加用户", Color.FromArgb(46, 204, 113)), Btn("删除用户", Color.FromArgb(231, 76, 60)) });

            dgvUsr = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false };

            ((Button)bar.Controls[0]).Click += (s, e) => { DlgUser(); RefreshUsers(); };
            ((Button)bar.Controls[1]).Click += (s, e) => DelUser();

            pnl.Controls.Add(dgvUsr); pnl.Controls.Add(bar);
            tab.Controls.Add(pnl); return tab;
        }

        void RefreshUsers()
        {
            var list = _auth.GetAllUsers().Select(u => new { u.Id, 用户名 = u.Username, 角色 = u.Role, 姓名 = u.FullName }).ToList();
            dgvUsr.DataSource = null; dgvUsr.DataSource = list;
            if (dgvUsr.Columns["Id"] != null) dgvUsr.Columns["Id"].Visible = false;
        }

        void DelUser()
        {
            if (dgvUsr.CurrentRow?.DataBoundItem is null) return;
            dynamic r = dgvUsr.CurrentRow.DataBoundItem;
            if ((string)r.用户名 == "admin") { MessageBox.Show("不能删除管理员账号！"); return; }
            if (MessageBox.Show("确定删除用户 " + r.用户名 + " 吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            { _auth.DeleteUser((int)r.Id); RefreshUsers(); }
        }

        void DlgUser()
        {
            var dlg = new Form { Text = "添加用户", Size = new Size(400, 280), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            var txtU = new TextBox { Location = new Point(100, 25), Size = new Size(250, 25) };
            var txtN = new TextBox { Location = new Point(100, 70), Size = new Size(250, 25) };
            var cmbR = new ComboBox { Location = new Point(100, 115), Size = new Size(150, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbR.Items.AddRange(new[] { "Admin", "Teacher", "Student" }); cmbR.SelectedIndex = 2;

            dlg.Controls.AddRange(new Control[] { new Label { Text = "用户名：", Location = new Point(30, 28), Size = new Size(65, 25) }, txtU, new Label { Text = "真实姓名：", Location = new Point(30, 73), Size = new Size(65, 25) }, txtN, new Label { Text = "角色：", Location = new Point(30, 118), Size = new Size(65, 25) }, cmbR });

            var ok = new Button { Text = "确定（默认密码 123456）", Location = new Point(90, 180), Size = new Size(200, 35), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            ok.FlatAppearance.BorderSize = 0;
            ok.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(txtU.Text)) return;
                _auth.AddUser(new User { Username = txtU.Text.Trim(), FullName = txtN.Text.Trim(), Role = cmbR.SelectedItem?.ToString() ?? "Student" });
                dlg.DialogResult = DialogResult.OK; dlg.Close();
            };
            dlg.Controls.Add(ok); dlg.ShowDialog(this);
        }
    }
}
