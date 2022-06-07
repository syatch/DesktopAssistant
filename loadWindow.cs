using System;
using System.Drawing;
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

        /// <summary>
        /// Update progress bar
        /// </summary>
        /// <param name="progress">Progress (0 to 100)</param>
        /// <returns>If icon and progress bar moved to 100%</returns>
        public bool UpdateLoadProgress(int progress)
        {
            if ((float)loadProgressBar.Value / loadProgressBar.Maximum < progress / 100.0f)
                loadProgressBar.Value += 1;

            if (iconPictureBox.Left < (progressBarLeft + iconMoveLength * progress / 100.0f))
                iconPictureBox.Left += iconMoveLength / 300;

            if ((loadProgressBar.Value >= loadProgressBar.Maximum)
             && (iconPictureBox.Left >= (progressBarLeft + iconMoveLength * progress / 100.0f)))
                return true;
            else
                return false;
        }

        // when mouse downe
        private void LoadWindow_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }

        // when mouse moving while down
        private void LoadWindow_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }

        private void LoadWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void LoadWindow_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Hide frame of the load window when shown
        /// </summary>
        /// <param name="object">Object</param>
        /// <param name="EventArgs">EventArg</param>
        private void LoadWindow_Shown(object sender, EventArgs e)
        {
            // hide frame
            FormBorderStyle = FormBorderStyle.None;
        }
    }
}
