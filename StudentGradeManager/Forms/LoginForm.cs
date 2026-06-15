using System;
using System.Drawing;
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
        private Button btnLogin = null!;

        public LoginForm(AuthService auth, StudentService ss, CourseService cs,
            GradeService gs, StatisticsService sts, AIService ai)
        {
            _auth = auth; _studentSvc = ss; _courseSvc = cs;
            _gradeSvc = gs; _statsSvc = sts; _aiSvc = ai;
            InitUI();
        }

        private void InitUI()
        {
            this.Text = "AI 智能学生成绩管理系统 - 登录";
            this.Size = new Size(450, 360);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 242, 245);

            var title = new Label
            {
                Text = "AI 智能学生成绩管理系统",
                Font = new Font("Microsoft YaHei", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                Size = new Size(380, 40),
                Location = new Point(35, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblUser = new Label { Text = "用户名：", Font = new Font("Microsoft YaHei", 10), Location = new Point(65, 95), Size = new Size(80, 25) };
            txtUser = new TextBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(150, 95), Size = new Size(220, 25), PlaceholderText = "请输入用户名" };

            var lblPass = new Label { Text = "密  码：", Font = new Font("Microsoft YaHei", 10), Location = new Point(65, 140), Size = new Size(80, 25) };
            txtPass = new TextBox { Font = new Font("Microsoft YaHei", 10), Location = new Point(150, 140), Size = new Size(220, 25), PasswordChar = '*', PlaceholderText = "请输入密码" };

            btnLogin = new Button
            {
                Text = "登  录",
                Font = new Font("Microsoft YaHei", 11, FontStyle.Bold),
                Location = new Point(150, 195),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += OnLogin;

            var hint = new Label
            {
                Text = "默认账号：admin/admin123 | teacher1/teacher123 | student1/student123",
                Font = new Font("Microsoft YaHei", 8),
                ForeColor = Color.Gray,
                Location = new Point(50, 260),
                Size = new Size(350, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.AddRange(new Control[] { title, lblUser, txtUser, lblPass, txtPass, btnLogin, hint });
            this.AcceptButton = btnLogin;
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
                MessageBox.Show("欢迎，" + user.FullName + "（" + roleName + "）！", "登录成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Form main = user.Role switch
                {
                    "Admin" => new AdminMainForm(_auth, _studentSvc, _courseSvc, _gradeSvc, _statsSvc, _aiSvc),
                    "Teacher" => new TeacherMainForm(_auth, _studentSvc, _courseSvc, _gradeSvc, _statsSvc, _aiSvc),
                    _ => new StudentMainForm(_auth, _studentSvc, _courseSvc, _gradeSvc, _statsSvc, _aiSvc)
                };

                this.Hide();
                main.FormClosed += (s, args) => this.Close();
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
