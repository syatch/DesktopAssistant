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
        }
        /*
                public void setPairWindow(Form pairForm)
                {
                    pairForm.MouseDown += new MouseEventHandler(loadWindow_MouseDown);
                    pairForm.MouseMove += new MouseEventHandler(loadWindow_MouseMove);
                }
        */
        //Form1のMouseDownイベントハンドラ
        //マウスのボタンが押されたとき
        private void loadWindow_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        //Form1のMouseMoveイベントハンドラ
        //マウスが動いたとき
        private void loadWindow_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // move this form
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
/*
                // move child form
                this.OwnedForms[0].Left += e.X - mousePoint.X;
                this.OwnedForms[0].Top += e.Y - mousePoint.Y;
*/
            }
        }
        private void loadWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Console.WriteLine("close Load");
        }

        private void loadWindow_Load(object sender, EventArgs e)
        {
            Console.WriteLine("loadWindow Load");
            this.MouseDown += new MouseEventHandler(loadWindow_MouseDown);
            this.MouseMove += new MouseEventHandler(loadWindow_MouseMove);
        }
        private void loadWindow_Shown(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;//フォームの枠を非表示にする
            // TransparencyKey = Color.FromArgb(0, 0, 0);//透過色を設定
        }
    }
}
