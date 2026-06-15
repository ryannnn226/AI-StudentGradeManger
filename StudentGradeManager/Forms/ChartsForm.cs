using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using StudentGradeManager.Services;

namespace StudentGradeManager.Forms
{
    public class ChartsForm : Form
    {
        private readonly StatisticsService _sttSvc;
        private readonly CourseService _crsSvc;
        private BarChartPanel chartAvg = null!, chartDist = null!;
        private ComboBox cmbCourse = null!;

        static readonly Color[] CHART_COLORS = {
            Color.FromArgb(59, 130, 246), Color.FromArgb(16, 185, 129),
            Color.FromArgb(245, 158, 11), Color.FromArgb(239, 68, 68),
            Color.FromArgb(139, 92, 246), Color.FromArgb(236, 72, 153),
            Color.FromArgb(20, 184, 166), Color.FromArgb(168, 85, 247)
        };

        public ChartsForm(StatisticsService sts, CourseService cs)
        {
            _sttSvc = sts; _crsSvc = cs;
            this.Text = "成绩统计图表";
            this.Size = new Size(860, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);
            this.BackColor = Color.FromArgb(245, 247, 250);
            BuildUI();
            RefreshCharts();
        }

        void BuildUI()
        {
            // 课程平均分
            var panelAvg = new Panel
            {
                Dock = DockStyle.Top,
                Height = this.ClientSize.Height * 45 / 100,
                BackColor = Color.White,
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 0, 4)
            };
            var lblAvg = new Label
            {
                Dock = DockStyle.Top, Height = 28,
                Text = "  各课程平均分对比",
                Font = new Font("Microsoft YaHei", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                TextAlign = ContentAlignment.MiddleLeft
            };
            chartAvg = new BarChartPanel { Dock = DockStyle.Fill, BackColor = Color.White };
            panelAvg.Controls.Add(chartAvg);
            panelAvg.Controls.Add(lblAvg);

            // 成绩分布
            var panelDist = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(8)
            };
            var topBar = new Panel { Dock = DockStyle.Top, Height = 32 };
            topBar.Controls.Add(new Label
            {
                Text = "  成绩分布",
                Font = new Font("Microsoft YaHei", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(0, 0), Size = new Size(200, 28),
                TextAlign = ContentAlignment.MiddleLeft
            });
            cmbCourse = new ComboBox
            {
                Font = new Font("Microsoft YaHei", 9),
                Size = new Size(180, 24),
                Location = new Point(210, 3),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = _crsSvc.GetAllCourses(),
                DisplayMember = "Name",
                ValueMember = "Id"
            };
            cmbCourse.SelectedIndexChanged += (s, e) => RefreshDistChart();
            topBar.Controls.Add(cmbCourse);
            chartDist = new BarChartPanel { Dock = DockStyle.Fill, BackColor = Color.White };
            panelDist.Controls.Add(chartDist);
            panelDist.Controls.Add(topBar);

            // 关闭按钮
            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 44 };
            var btnClose = new Button
            {
                Text = "关闭", Size = new Size(100, 32),
                Location = new Point(0, 6),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei", 9),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            bottomBar.Controls.Add(btnClose);

            this.Controls.Add(panelDist);
            this.Controls.Add(panelAvg);
            this.Controls.Add(bottomBar);

            // 窗口缩放时调整比例
            this.Resize += (s, e) =>
            {
                panelAvg.Height = Math.Max(120, this.ClientSize.Height * 45 / 100);
                chartAvg.Height = Math.Max(60, panelAvg.Height - 36);
            };
        }

        void RefreshCharts()
        {
            var stats = _sttSvc.GetStatsByCourse();
            var avgData = stats.Select((s, i) =>
                (s.CourseName, s.AverageScore, CHART_COLORS[i % CHART_COLORS.Length])).ToList();
            chartAvg.SetData("课程平均分", "课程", avgData);
            RefreshDistChart();
        }

        void RefreshDistChart()
        {
            if (cmbCourse?.SelectedValue is int cid)
            {
                var dist = _sttSvc.GetScoreDistribution(cid);
                var colors = new[] {
                    Color.FromArgb(16, 185, 129), Color.FromArgb(59, 130, 246),
                    Color.FromArgb(245, 158, 11), Color.FromArgb(249, 115, 22),
                    Color.FromArgb(239, 68, 68)
                };
                var labels = new[] { "90-100", "80-89", "70-79", "60-69", "0-59" };
                var data = new List<(string, double, Color)>();
                int i = 0;
                foreach (var kv in dist)
                    data.Add((labels[i], kv.Value, colors[i++]));
                chartDist.SetData("成绩分布", "分数段", data);
            }
        }
    }
}