using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopAssistant
{
    public partial class loadWindow : Form
    {
        private Point mousePoint;

        public loadWindow()
        {
            InitializeComponent();
            
            // make not to change size of window
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            // set size and place of loadProgressBar
            loadProgressBar.Width = this.Width + 3;
            loadProgressBar.Top = this.Top + this.Height - this.Height/10;
            // set color and design of loadProgressBar
            loadProgressBar.ForeColor = Color.Orange;
            loadProgressBar.BackColor = Color.Black;
            loadProgressBar.Style = ProgressBarStyle.Continuous;
            // set size of pictureBox
            pictureBox.Size = this.Size;
            // set picture of pictureBox
            pictureBox.ImageLocation = @"./Data/Image/loadPicture.png";
            // set size mode of pictureBox
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        }

        // update progress bar
        public void updateLoadProgress(int progress)
        {
            if (loadProgressBar.Value < progress * 10)
                loadProgressBar.Value++;
        }

        // when mouse downe
        private void loadWindow_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // remember mouse point
                mousePoint = new Point(e.X, e.Y);
            }
        }

        // when mouse moving while down
        private void loadWindow_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // move this form
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
        }
        private void loadWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void loadWindow_Load(object sender, EventArgs e)
        {
            // add mouse event handler
            this.MouseDown += new MouseEventHandler(loadWindow_MouseDown);
            this.MouseMove += new MouseEventHandler(loadWindow_MouseMove);
        }
        private void loadWindow_Shown(object sender, EventArgs e)
        {
            // hide frame
            FormBorderStyle = FormBorderStyle.None;
        }
    }
}
