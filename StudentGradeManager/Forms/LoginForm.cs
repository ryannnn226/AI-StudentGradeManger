using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using StudentGradeManager.Services;

namespace StudentGradeManager.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _auth;
        private readonly StudentService _studentSvc;
        private readonly CourseService _courseSvc;
        private readonly GradeService _gradeSvc;
        private readonly StatisticsService _statsSvc;
        private readonly AIService _aiSvc;
        private TextBox txtUser = null!, txtPass = null!;

        public LoginForm(AuthService auth, StudentService ss, CourseService cs,
            GradeService gs, StatisticsService sts, AIService ai)
        {
            _auth = auth; _studentSvc = ss; _courseSvc = cs;
            _gradeSvc = gs; _statsSvc = sts; _aiSvc = ai;
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "AI 智能学生成绩管理系统";
            this.Size = new Size(480, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 247, 250);

            // 主卡片
            var card = new Panel
            {
                Size = new Size(380, 320),
                Location = new Point(50, 35),
                BackColor = Color.White,
            };
            card.Paint += (s, e) =>
            {
                var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using var pen = new Pen(Color.FromArgb(225, 230, 235), 1);
                e.Graphics.DrawRectangle(pen, r);
            };

            // 标题
            var title = new Label
            {
                Text = "AI 智能成绩管理",
                Font = new Font("Microsoft YaHei", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Size = new Size(300, 35),
                Location = new Point(40, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var subtitle = new Label
            {
                Text = "Student Grade Management System",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(150, 160, 170),
                Size = new Size(300, 20),
                Location = new Point(40, 58),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 用户名输入
            var userPanel = CreateInputPanel("用户名", 95);
            txtUser = (TextBox)userPanel.Controls[1];
            txtUser.PlaceholderText = "请输入用户名";

            // 密码输入
            var passPanel = CreateInputPanel("密  码", 145);
            txtPass = (TextBox)passPanel.Controls[1];
            txtPass.PasswordChar = '\u25CF';
            txtPass.PlaceholderText = "请输入密码";

            // 登录按钮
            var btnLogin = new Button
            {
                Text = "登  录",
                Font = new Font("Microsoft YaHei", 11, FontStyle.Bold),
                Size = new Size(300, 42),
                Location = new Point(40, 210),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += OnLogin;

            // 底部提示
            var hint = new Label
            {
                Text = "默认账号：admin / teacher1 / student1  |  密码与账号对应",
                Font = new Font("Microsoft YaHei", 8),
                ForeColor = Color.FromArgb(160, 170, 180),
                Size = new Size(300, 25),
                Location = new Point(40, 270),
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.AddRange(new Control[] { title, subtitle, userPanel, passPanel, btnLogin, hint });
            this.Controls.Add(card);
            this.AcceptButton = btnLogin;
        }

        private Panel CreateInputPanel(string label, int y)
        {
            var panel = new Panel { Size = new Size(300, 42), Location = new Point(40, y), BackColor = Color.FromArgb(248, 250, 252) };
            panel.Paint += (s, e) =>
            {
                var r = new Rectangle(0, panel.Height - 2, panel.Width, 2);
                using var brush = new SolidBrush(Color.FromArgb(59, 130, 246));
                e.Graphics.FillRectangle(brush, r);
            };

            var lbl = new Label { Text = label, Font = new Font("Microsoft YaHei", 9, FontStyle.Regular), ForeColor = Color.FromArgb(100, 110, 120), Size = new Size(55, 22), Location = new Point(8, 10), TextAlign = ContentAlignment.MiddleLeft };
            var txt = new TextBox { Font = new Font("Microsoft YaHei", 10), Size = new Size(220, 22), Location = new Point(65, 10), BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(248, 250, 252), ForeColor = Color.FromArgb(44, 62, 80) };
            panel.Controls.Add(lbl);
            panel.Controls.Add(txt);
            return panel;
        }

        private void OnLogin(object? sender, EventArgs e)
        {
            var u = txtUser.Text.Trim();
            var p = txtPass.Text.Trim();
            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            {
                MessageBox.Show("请输入用户名和密码。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_auth.Login(u, p))
            {
                var user = _auth.CurrentUser!;
                var roleName = user.Role switch { "Admin" => "管理员", "Teacher" => "教师", "Student" => "学生", _ => user.Role };
                Form main = user.Role switch
                {
                    "Admin" => new AdminMainForm(_auth, _studentSvc, _courseSvc, _gradeSvc, _statsSvc, _aiSvc),
                    "Teacher" => new TeacherMainForm(_auth, _studentSvc, _courseSvc, _gradeSvc, _statsSvc, _aiSvc),
                    _ => new StudentMainForm(_auth, _studentSvc, _courseSvc, _gradeSvc, _statsSvc, _aiSvc)
                };
                this.Hide();
                main.FormClosed += (s2, args) => { this.Show(); txtUser.Clear(); txtPass.Clear(); };
                main.Show();
            }
            else
            {
                MessageBox.Show("用户名或密码错误，请重试。", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPass.Clear();
                txtPass.Focus();
            }
        }
    }
}
