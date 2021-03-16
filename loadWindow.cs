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
    public partial class LoadWindow : Form
    {
        private Point mousePoint;
        private int progressBarLeft;
        private int iconMoveLength;

        public LoadWindow()
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

            // set size of pictureBox
            iconPictureBox.Height = this.Height / 4;
            iconPictureBox.Width = this.Width / 4;
            // set picture of pictureBox
            iconPictureBox.ImageLocation = @"./Data/Image/iconPicture.png";
            iconPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            iconPictureBox.BackColor = Color.White;

            progressBarLeft = loadProgressBar.Left;
            iconMoveLength = (loadProgressBar.Width - iconPictureBox.Width);
        }

        // update progress bar
        public void UpdateLoadProgress(int progress)
        {
            if (loadProgressBar.Value < progress * 10)
                loadProgressBar.Value += 5;

            if (iconPictureBox.Left < (progressBarLeft + iconMoveLength * progress / 100))
                iconPictureBox.Left += iconMoveLength / 300;
        }

        // when mouse downe
        private void LoadWindow_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // remember mouse point
                mousePoint = new Point(e.X, e.Y);
            }
        }

        // when mouse moving while down
        private void LoadWindow_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // move this form
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
        }
        private void LoadWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void LoadWindow_Load(object sender, EventArgs e)
        {
            // add mouse event handler
            this.MouseDown += new MouseEventHandler(LoadWindow_MouseDown);
            this.MouseMove += new MouseEventHandler(LoadWindow_MouseMove);
        }
        private void LoadWindow_Shown(object sender, EventArgs e)
        {
            // hide frame
            FormBorderStyle = FormBorderStyle.None;
        }
    }
}
