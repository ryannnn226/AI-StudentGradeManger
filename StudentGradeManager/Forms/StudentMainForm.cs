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

        static readonly Color C_BG = Color.FromArgb(245, 247, 250);
        static readonly Color C_WHITE = Color.White;
        static readonly Color C_BLUE = Color.FromArgb(59, 130, 246);
        static readonly Color C_TEXT = Color.FromArgb(30, 41, 59);

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

        static void AutoFitGrid(DataGridView g)
        {
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            g.Refresh();
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (DataGridViewColumn col in g.Columns)
                col.MinimumWidth = 70;
        }

        private void InitializeComponent()
        {
            this.Text = "成绩管理系统 - " + (_auth.CurrentUser?.FullName ?? "");
            this.Size = new Size(950, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(750, 500);
            this.BackColor = C_BG;

            var topBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = C_WHITE };
            topBar.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240)), 0, 47, topBar.Width, 47);
            topBar.Controls.Add(new Label { Text = "学生工作台", Font = new Font("Microsoft YaHei", 14, FontStyle.Bold), ForeColor = C_TEXT, Location = new Point(20, 10), Size = new Size(200, 28) });

            var sidebar = new Panel { Dock = DockStyle.Left, Width = 180, BackColor = Color.FromArgb(30, 41, 59) };
            sidebar.Controls.Add(new Label { Text = "功能菜单", Font = new Font("Microsoft YaHei", 9), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(16, 16), Size = new Size(150, 24) });
            // 退出登录
            var btnLogout = new Button { Text = "退出登录", Font = new Font("Microsoft YaHei", 9), Size = new Size(165, 40), Location = new Point(8, 60 + 2 * 50 + 16), FlatStyle = FlatStyle.Flat, ForeColor = Color.FromArgb(248, 113, 113), TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { if (MessageBox.Show("确定要退出登录吗？", "退出", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) this.Close(); };
            sidebar.Controls.Add(btnLogout);

            var panels = new Panel[] { new Panel(), new Panel() };
            var btns = new Button[2];
            string[] items = { "我的成绩", "AI 智能分析" };
            for (int i = 0; i < 2; i++)
            {
                btns[i] = new Button { Text = "  " + items[i], Font = new Font("Microsoft YaHei", 10), Size = new Size(165, 42), Location = new Point(8, 60 + i * 50), FlatStyle = FlatStyle.Flat, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand, Tag = i };
                btns[i].FlatAppearance.BorderSize = 0;
                btns[i].Click += (s2, e2) => { for (int j = 0; j < 2; j++) { btns[j].BackColor = j == (int)((Button)s2!).Tag! ? C_BLUE : Color.FromArgb(30, 41, 59); panels[j].Visible = j == (int)((Button)s2!).Tag!; } };
                sidebar.Controls.Add(btns[i]);
            }

            var content = new Panel { Dock = DockStyle.Fill, BackColor = C_BG, Padding = new Padding(16), AutoScroll = true };
            for (int i = 0; i < 2; i++) { panels[i].Dock = DockStyle.Fill; content.Controls.Add(panels[i]); }

            // ---- 我的成绩 ----
            var pg = panels[0];
            var card1 = new Panel { Location = new Point(0, 8), BackColor = C_WHITE, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            card1.Width = content.ClientSize.Width - 16;
            card1.Height = content.ClientSize.Height - 16;
            content.Resize += (s, e) => { card1.Width = content.ClientSize.Width - 16; card1.Height = content.ClientSize.Height - 16; };

            lblInfo = new Label { Location = new Point(16, 12), Size = new Size(card1.Width - 32, 35), Font = new Font("Microsoft YaHei", 10), ForeColor = C_TEXT, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            dgv = new DataGridView { Location = new Point(16, 52), BackgroundColor = C_WHITE, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(241, 245, 249), RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 44,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252), ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Microsoft YaHei", 10, FontStyle.Bold), Padding = new Padding(8, 10, 8, 10) }, DefaultCellStyle = new DataGridViewCellStyle { BackColor = C_WHITE, ForeColor = C_TEXT, Font = new Font("Microsoft YaHei", 9), Padding = new Padding(4) }, AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252) }, RowTemplate = new DataGridViewRow { Height = 34 }, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            dgv.Width = card1.Width - 32;
            dgv.Height = card1.Height - 130;
            var btnRef = new Button { Text = "刷新成绩", Size = new Size(110, 35), FlatStyle = FlatStyle.Flat, BackColor = C_BLUE, ForeColor = Color.White, Font = new Font("Microsoft YaHei", 9), Cursor = Cursors.Hand, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnRef.FlatAppearance.BorderSize = 0; btnRef.Location = new Point(16, dgv.Bottom + 4);
            btnRef.Click += (s, e) => RefreshGrades();
            card1.Resize += (s, e) => { lblInfo.Width = card1.Width - 32; dgv.Width = card1.Width - 32; dgv.Height = card1.Height - 130; btnRef.Location = new Point(16, dgv.Bottom + 4); };
            card1.Controls.Add(lblInfo); card1.Controls.Add(dgv); card1.Controls.Add(btnRef);
            pg.Controls.Add(card1);

            // ---- AI 分析 ----
            var pa = panels[1];
            var aiCard = new Panel { Location = new Point(0, 8), BackColor = C_WHITE, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            aiCard.Width = content.ClientSize.Width - 16;
            aiCard.Height = content.ClientSize.Height - 16;
            content.Resize += (s, e) => { aiCard.Width = content.ClientSize.Width - 16; aiCard.Height = content.ClientSize.Height - 16; };

            var bar = new FlowLayoutPanel { Location = new Point(16, 12), Size = new Size(aiCard.Width - 32, 36), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            var btnTrend = new Button { Text = "AI 成绩波动分析", Size = new Size(150, 32), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 9), Cursor = Cursors.Hand, Anchor = AnchorStyles.Bottom | AnchorStyles.Left }; btnTrend.FlatAppearance.BorderSize = 0;
            var btnComment = new Button { Text = "AI 生成学期评语", Size = new Size(150, 32), FlatStyle = FlatStyle.Flat, BackColor = C_BLUE, ForeColor = Color.White, Font = new Font("Microsoft YaHei", 9), Cursor = Cursors.Hand, Anchor = AnchorStyles.Bottom | AnchorStyles.Left }; btnComment.FlatAppearance.BorderSize = 0;
            bar.Controls.AddRange(new Control[] { btnTrend, btnComment });
            rtbAI = new RichTextBox { Location = new Point(16, 55), Font = new Font("Microsoft YaHei", 10), ReadOnly = true, BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(248, 250, 252), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            rtbAI.Width = aiCard.Width - 32; rtbAI.Height = aiCard.Height - 70;
            aiCard.Resize += (s, e) => { bar.Width = aiCard.Width - 32; rtbAI.Width = aiCard.Width - 32; rtbAI.Height = aiCard.Height - 70; };
            btnTrend.Click += async (s, e) => { rtbAI.Text = "正在分析..."; rtbAI.Text = await _aiSvc.AnalyzeGradeTrendAsync(_sid); };
            btnComment.Click += async (s, e) => { rtbAI.Text = "正在生成评语..."; rtbAI.Text = await _aiSvc.GenerateCommentAsync(_sid); };
            aiCard.Controls.Add(bar); aiCard.Controls.Add(rtbAI);
            pa.Controls.Add(aiCard);

            this.Controls.Add(content); this.Controls.Add(sidebar); this.Controls.Add(topBar);
            btns[0].BackColor = C_BLUE; panels[0].Visible = true; panels[1].Visible = false;
            this.Load += (s, e) => RefreshGrades();
        }

        void RefreshGrades()
        {
            var grades = _grdSvc.GetGradesByStudent(_sid);
            dgv.DataSource = null;
            dgv.DataSource = grades.Select(g => new { 课程 = g.CourseName, 分数 = g.Grade.Score, 类型 = g.Grade.ExamType, 日期 = g.Grade.ExamDate.ToString("yyyy-MM-dd") }).ToList();
            if (grades.Count > 0) { var avg = Math.Round(grades.Average(g => g.Grade.Score), 1); lblInfo.Text = "共 " + grades.Count + " 条记录  |  平均分：" + avg + "  |  最高：" + grades.Max(g => g.Grade.Score) + "  |  最低：" + grades.Min(g => g.Grade.Score); }
            else lblInfo.Text = "暂无成绩数据";
            AutoFitGrid(dgv);
        }
    }
}
