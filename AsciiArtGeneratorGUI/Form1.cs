using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Windows.Forms.DataVisualization.Charting;

namespace AsciiArtGeneratorGUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeChart();
            pictureBox1.BorderStyle = BorderStyle.Fixed3D;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.AllowDrop = true;
            pictureBox1.DragDrop += PictureBox1_DragDrop;
            pictureBox1.DragEnter += PictureBox1_DragEnter;
            // richTextBox
            listView1.Items.Add(new ListViewItem(new string[] { "문자 갯수", "" }));
            listView1.Items.Add(new ListViewItem(new string[] { "행 최대 길이", "" }));
            listView1.Items.Add(new ListViewItem(new string[] { "줄 수", "" }));
            listView1.Items.Add(new ListViewItem(new string[] { "글꼴 이름", "" }));
            listView1.Items.Add(new ListViewItem(new string[] { "글꼴 크기", "" }));

            // Image
            listView1.Items.Add(new ListViewItem(new string[] { "원본 이미지", "" }));
            listView1.Items.Add(new ListViewItem(new string[] { "너비", "" }));
            listView1.Items.Add(new ListViewItem(new string[] { "높이", "" }));
            listView1.Items.Add(new ListViewItem(new string[] { "축소 배율", "" }));
            
            listView1.DoubleBuffered(true);

            checkBox1.Checked = true;
            checkBox2.Checked = true;
            checkBox3.Checked = true;

            displayCharacter = textBox2.Text;
        }

        private void Chart1_Invalidated(object sender, InvalidateEventArgs e)
        {
            if(_dp != null)
                lbl_Debug.Text = _dp.YValues[0].ToString();
        }

        double[] levelBoundary;
        private void Chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left) && _dp != null){
                float mh = _dp.MarkerSize / 2f;
                double vx = _dp.XValue;

                try
                {
                    double vy = (int)_ca.AxisY.PixelPositionToValue(e.Location.Y);
                    if (vy < 0)
                    {
                        vy = 0;
                    }
                    if (vy > 255)
                    {
                        vy = 255;
                    }
                    _dp.SetValueXY(vx, vy);
                    chart1.Invalidate();
                }
                catch { }
               

            }
        }

        private void Chart1_MouseUp(object sender, MouseEventArgs e)
        {
            _dp = null;

            levelBoundary = new double[chart1.Series[0].Points.Count-2]; // 시작점 , 끝점 제외

            int cnt = 0;
            for (int i = 0; i < levelBoundary.Length; i++)
            {
                levelBoundary[cnt++] = chart1.Series[0].Points[i + 1].YValues[0];
            }
            if (checkBox2.Checked)
                DrawAsciiArt();
        }

        private void Chart1_MouseDown(object sender, MouseEventArgs e)
        {
            var r = chart1.HitTest(e.X, e.Y);
            if (r.ChartElementType == ChartElementType.DataPoint)
            {
                int index = r.PointIndex;

                if (index != 0 && index != chart1.Series[0].Points.Count - 1)
                    _dp = (DataPoint)r.Object;
            }
        }

        private void PictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            
            if (data != null)
            {
                var filenames = data as string[];
                if(filenames.Length > 0)
                {
                    originImage = new Bitmap(filenames[0]);
                    pictureBox1.Image = originImage;

                    double ratioX, ratioY;
                    ratioX = ((double)pictureBox1.Width / originImage.Width);
                    ratioY = ((double)pictureBox1.Height / originImage.Height);
                    
                }
            }
        }

        private void PictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        string recentPath = Properties.Settings.Default.RecentPath;
        Bitmap originImage;
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory =
                recentPath == null
                ? Environment.GetFolderPath( Environment.SpecialFolder.MyPictures)
                : recentPath;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "모든 파일(*.*)|*.*";
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fileName = openFileDialog1.FileName;
                textBox1.Text = fileName;
                recentPath = openFileDialog1.InitialDirectory;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.SelectionLength = 0;
                originImage = new Bitmap(fileName);
                pictureBox1.Image = originImage; double ratioX, ratioY;
                ratioX = ((double)originImage.Width / pictureBox1.Width);
                ratioY = ((double)originImage.Height / pictureBox1.Height);

                listView1.Items[5].SubItems[1].Text = openFileDialog1.FileName;
                listView1.Items[6].SubItems[1].Text = originImage.Width.ToString();
                listView1.Items[7].SubItems[1].Text = originImage.Height.ToString();
                listView1.Items[8].SubItems[1].Text = numericUpDown1.Value.ToString();
                DrawAsciiArt();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form _form = new Form();
            
            _form.Width = this.Size.Width / 2;
            _form.Height = this.Size.Height / 2;
            _form.AutoScroll = true;
            _form.StartPosition = FormStartPosition.CenterParent;

            PictureBox _pictureBox = new PictureBox();
            _pictureBox.Image = pictureBox1.Image;
            _pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            _pictureBox.Dock = DockStyle.Fill;
            _pictureBox.Click += (_sender, _e) => { _form.Dispose(); _form.Close(); };

            Panel _panel = new Panel();
            _panel.Size = _pictureBox.Image.Size;

            _panel.Controls.Add(_pictureBox);

            _form.Controls.Add(_panel);
            _form.ShowDialog();
        }

        string displayCharacter;
        private void button2_Click(object sender, EventArgs e)
        {
            DrawAsciiArt();
        }
        private void DrawAsciiArt()
        {
            string pad = "";

            for (int i = 0; i < 4 - textBox2.Text.Length; i++)
            {
                pad += ' ';
            }
            if (checkBox4.Checked)
                displayCharacter = (textBox2.Text + pad);
            else
                displayCharacter = new string((textBox2.Text + pad).Reverse().ToArray());

            double ratio = Convert.ToDouble(numericUpDown1.Value);
            richTextBox1.Text = AsciiGenerator.ThreeLevel(originImage, ratio, displayCharacter, levelBoundary);

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateListView();
        }

        private void UpdateListView()
        {
            listView1.BeginUpdate();

            int max = 0;
            listView1.Items[0].SubItems[1].Text = richTextBox1.Text.Length.ToString();
            foreach (string str in richTextBox1.Text.Split('\n'))
            {
                if (str.Length > max)
                    max = str.Length;
            }
            listView1.Items[1].SubItems[1].Text = max.ToString();
            listView1.Items[2].SubItems[1].Text = richTextBox1.Text.Split('\n').Length.ToString();
            listView1.Items[3].SubItems[1].Text = richTextBox1.Font.Name.ToString();
            listView1.Items[4].SubItems[1].Text = richTextBox1.Font.Size.ToString("N1");

            listView1.EndUpdate();
        }
        Series s = new Series("Boundary", 255);
        double[] defaultBoundary = new double[3] { 3.0*16, 9.5*16, 13*16 };
        private void InitializeChart()
        {
            s.ChartType = SeriesChartType.Line;

            s.Points.AddXY(0, 0);
            s.Points.AddXY(1, defaultBoundary[0]);
            s.Points.AddXY(2, defaultBoundary[1]);
            s.Points.AddXY(3, defaultBoundary[2]);
            s.Points.AddXY(4, 255);

            chart1.ChartAreas[0].AxisX.Interval = 100;
            chart1.ChartAreas[0].AxisY.Interval = 51;

            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 4;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 255;

            chart1.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart1.Series.Add(s);

            _ca = chart1.ChartAreas[0];
            chart1.Series[0].MarkerStyle = MarkerStyle.Circle;
            chart1.Series[0].MarkerSize = 10;

            chart1.MouseDown += Chart1_MouseDown;
            chart1.MouseUp += Chart1_MouseUp;
            chart1.MouseMove += Chart1_MouseMove;
            chart1.Invalidated += Chart1_Invalidated;
        }
        ChartArea _ca = null;
        DataPoint _dp = null;

        private void button3_Click(object sender, EventArgs e)
        {
            Form _form = new Form();

            RichTextBox _richTextBox = new RichTextBox();

            _form.AutoScroll = true;
            _form.Controls.Add(_richTextBox);
            _form.Size = Screen.PrimaryScreen.Bounds.Size;

            _richTextBox.Dock = DockStyle.Fill;
            _richTextBox.Text = richTextBox1.Text;
            _richTextBox.ReadOnly = true;
            _richTextBox.ZoomFactor = (float)0.1;
            _richTextBox.WordWrap = false;
            _richTextBox.SelectionCharOffset = 3;
            _richTextBox.Font = new Font("Consolas", 4, FontStyle.Regular);

            _form.ShowDialog();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
                DrawAsciiArt();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            DrawAsciiArt();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                DrawAsciiArt();
        }
    }

    public static class Extensions
    {
        public static void DoubleBuffered(this Control control, bool enabled)
        {
            var prop = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(control, enabled, null);
        }
    }
}
