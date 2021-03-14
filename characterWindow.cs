using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DxLibDLL;


namespace HomeHack
{
    public partial class characterWindow : Form
    {
        private enum WINDOW_STATE
        {
            WINDOW_OPENING,
            WINDOW_NORMAL,
            WINDOW_CLOSING
        }

        private enum MOTION_STATE
        {
            MOTION_WAIT,
            MOTION_WORK,
            MOTION_DANCE
        }

        private double initStage;
        private int modelHandle;
        private int attachIndex;
        private int musicHandle;
        private int maximumHeight = (int)(Screen.PrimaryScreen.Bounds.Height * 1.5);
        private int minimumHeight = (int)(Screen.PrimaryScreen.Bounds.Height * 0.3);
        private float playTime;
        private float totalTime;
        private WINDOW_STATE windowState;
        private MOTION_STATE motionState;
        private DX.VECTOR cameraTargetPosition = DX.VGet(0.0f, 13.0f, -0.0f);
        private DX.VECTOR cameraHomePosition = DX.VGet(0.0f, 13.0f, -30.0f);
        private DX.VECTOR listenerHomePos = DX.VGet(0.0f, 0.0f, 0.0f);
        private DX.VECTOR listenerPos = DX.VGet(0.0f, 0.0f, 0.0f);
        private DX.VECTOR listenerDir = DX.VGet(0.0f, 0.0f, 1.0f);
        private DateTime startTime;
        private DateTime nowTime;
        private Point mousePoint;

        public characterWindow()
        {
            InitializeComponent();//フォームの初期設定
            ClientSize = new Size((int)(Screen.PrimaryScreen.Bounds.Width*0.8), (int)(Screen.PrimaryScreen.Bounds.Height*1.4));//画面サイズの設定
            this.BackColor = Color.Black;
            // set this window top most
            Text = "キャラクターウィンドウ";//ウインドウの名前を設定
        }

        public double initProgress()
        {
            return initStage;
        }

        public int initDX()
        {
            initStage = 0;
            // set screen setting
            DX.SetOutApplicationLogValidFlag(DX.FALSE);//Log.txtを生成しないように設定
            DX.SetUserWindow(Handle);//DxLibの親ウインドウをこのフォームに設定
            DX.SetZBufferBitDepth(24);//Zバッファの深度を24bitに変更
            DX.SetCreateDrawValidGraphZBufferBitDepth(24);//裏画面のZバッファの深度を24bitに変更
            // draw 3D model smoothly
            DX.SetFullSceneAntiAliasingMode(24, 3);//画面のフルスクリーンアンチエイリアスモードの設定をする(sample, quality)
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);//描画先を裏画面に設定

            // set sound setting
            DX.SetEnableXAudioFlag(DX.TRUE);
            if (DX.DxLib_Init() < 0)
                return -1;
            // set Physics Mode
            DX.MV1SetLoadModelUsePhysicsMode(DX.DX_LOADMODEL_PHYSICS_REALTIME);
            modelHandle = DX.MV1LoadModel("Data/Model/character.pmx");//3Dモデルの読み込み
            if (modelHandle < 0)
                return -1;

            DX.MV1SetPhysicsWorldGravity(modelHandle, DX.VGet(0f, -230f, 0f));
            // DX.MV1SetPhysicsWorldGravity(modelHandle, DX.VGet(0f, -230f, 0f));
            DX.SetCameraNearFar(0.1f, 1000.0f);//奥行0.1～1000をカメラの描画範囲とする
            DX.SetCameraPositionAndTarget_UpVecY(cameraHomePosition, cameraTargetPosition);//第1引数の位置から第2引数の位置を見る角度にカメラを設置

            this.TopMost = true;
            windowState = WINDOW_STATE.WINDOW_OPENING;
            // motionState = MOTION_STATE.MOTION_WAIT;
            windowState = WINDOW_STATE.WINDOW_NORMAL;
            initStage = 100;

            this.Width = (int)(Screen.PrimaryScreen.Bounds.Width * 0.4);
            this.Height = (int)(Screen.PrimaryScreen.Bounds.Height * 0.7);
            return 0;
        }
        public void MainLoop()
        {
            DX.ClearDrawScreen();//裏画面を消す
            DX.DrawBox(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, DX.GetColor(0, 0, 0), DX.TRUE);//背景を設定(透過させる)


            nowTime = DateTime.Now;
            playTime = (float)(nowTime - startTime).TotalMilliseconds * 30f / 1000f;

            // Console.WriteLine("Play time : " + playTime);

            //Console.WriteLine("Update Motion");
            DX.MV1SetAttachAnimTime(modelHandle, attachIndex, playTime);//モーションの再生位置を設定
            DX.MV1PhysicsCalculation(modelHandle, 1000f/30f);
            DX.MV1DrawModel(modelHandle);//3Dモデルの描画

            DX.ScreenFlip();//裏画面を表画面にコピー



            // if closing and finish animating, close window
            if (windowState == WINDOW_STATE.WINDOW_CLOSING)
            {
                //DX.VECTOR nowPosition = DX.GetCameraPosition();
                //DX.VECTOR targetPosition = DX.VGet(0.0f, 13.0f, -30.0f);
               
                if (playTime >= totalTime)
                {
                    Close();
                }
            }

            //ESCキーを押したら終了
            if (DX.CheckHitKey(DX.KEY_INPUT_2) != 0)
            {
                // stop music
                if (DX.CheckSoundFile() == 1)
                    DX.StopSoundFile();
                DX.InitSoundMem();
                playDance(2, "Data/Music/do-natu.mp4", 0.18);
            }

            if (DX.CheckHitKey(DX.KEY_INPUT_3) != 0)
            {
                // stop music
                if (DX.CheckSoundFile() == 1)
                    DX.StopSoundFile();
                DX.InitSoundMem();
                playDance(3, "Data/Music/onegai.mp4", 0.8);

            }
            if (DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) != 0)
            {
                Console.WriteLine("start Close");
                
                // stop music
                if (DX.CheckSoundFile() == 1)
                    DX.StopSoundFile();
                DX.InitSoundMem();

                playClose();
                windowState = WINDOW_STATE.WINDOW_CLOSING;
                double cameraDistance = Math.Sqrt(Math.Pow(cameraHomePosition.x, 2) + Math.Pow(cameraHomePosition.z, 2));
                DX.SetCameraPositionAndTarget_UpVecY(DX.VGet((float)(cameraDistance*Math.Cos(Math.PI*2f*(-1/4f+50/360f))), cameraHomePosition.y, (float)(cameraDistance*Math.Sin(Math.PI*2f*(-1/4f+50/360f)))),
                                                     cameraTargetPosition);//第1引数の位置から第2引数の位置を見る角度にカメラを設置
            }
        }

        public void playClose()
        {
            attachIndex = DX.MV1DetachAnim(modelHandle, attachIndex);//モーションの中止
            attachIndex = DX.MV1AttachAnim(modelHandle, 1, -1, DX.FALSE);//モーションの選択
            Console.WriteLine("attachIndex : " + attachIndex);

            totalTime = DX.MV1GetAttachAnimTotalTime(modelHandle, attachIndex);//モーションの総再生時間を取得
            DX.MV1PhysicsResetState(modelHandle);
            startTime = DateTime.Now;
            
        }

        public void playDance(int motionIndex, String FileName, double delayTime = 0.0)
        {
            attachIndex = DX.MV1DetachAnim(modelHandle, attachIndex);//モーションの中止
            attachIndex = DX.MV1AttachAnim(modelHandle, motionIndex, -1, DX.FALSE);//モーションの選択

            DX.SetCreate3DSoundFlag(DX.TRUE);
            musicHandle = DX.LoadSoundMem(FileName);
            DX.SetCreate3DSoundFlag(DX.FALSE);
            DX.Set3DRadiusSoundMem(10.0f, musicHandle);
            DX.Set3DPresetReverbParamSoundMem(DX.DX_REVERB_PRESET_GENERIC, musicHandle);
            DX.SetNextPlay3DPositionSoundMem(DX.VGet(0.0f, 0.0f, 2.0f), musicHandle);

            totalTime = DX.MV1GetAttachAnimTotalTime(modelHandle, attachIndex);//モーションの総再生時間を取得
            DX.MV1PhysicsResetState(modelHandle);

            startTime = DateTime.Now;
            if (delayTime < 0)
            {
                DX.SetSoundCurrentTime((int)(-delayTime * 1000f), musicHandle);
                DX.PlaySoundMem(musicHandle, DX.DX_PLAYTYPE_BACK, DX.FALSE);
            }
            else
            {
                startTime = startTime.AddMilliseconds(-delayTime * 1000f);
                DX.PlaySoundMem(musicHandle, DX.DX_PLAYTYPE_BACK, DX.TRUE);
            }

        }

        /*
                public void playMusic(String FileName)
                {
                    musicSuccess = DX.PlaySoundFile(FileName, DX.DX_PLAYTYPE_BACK);
                    Console.WriteLine("musicSuccess : " + musicSuccess);
                }

        */

        private void changeListenerDir()
        {
            int centerLeft = this.Left + this.Size.Width / 2;
            int centerTop = this.Top + this.Size.Height / 2;
            float widthRate = (centerLeft - Screen.PrimaryScreen.Bounds.Width / 2f) / Screen.PrimaryScreen.Bounds.Width;
            DX.VECTOR listenerPos = DX.VGet(listenerHomePos.x - 3.0f * widthRate, listenerHomePos.y, listenerHomePos.z);
            DX.Set3DSoundListenerPosAndFrontPos_UpVecY(listenerPos, DX.VAdd(listenerPos, listenerDir));
        }

        private void characterWindow_Move(object sender, EventArgs e)
        {
            changeListenerDir();
        }

        //Form1のMouseDownイベントハンドラ
        //マウスのボタンが押されたとき
        private void characterWindow_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        //Form1のMouseMoveイベントハンドラ
        //マウスが動いたとき
        private void characterWindow_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
        }
        private void characterWindow_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int scrollValue = e.Delta / 120 * SystemInformation.MouseWheelScrollLines;
            double magnification;
            Console.WriteLine("scroll num : " + scrollValue);

            if (scrollValue > 0)
                magnification = scrollValue/2.8;
            else
                magnification = -2.8/scrollValue;

            if ((this.Height > maximumHeight) || (this.Height < minimumHeight))
                return;


            int newHeight = (int)(this.Height * magnification);

            if (newHeight > maximumHeight)
                newHeight = maximumHeight;
            else if (newHeight < minimumHeight)
                newHeight = minimumHeight;

            int centerLeft = this.Left + this.Size.Width / 2;
            int centerTop = this.Top + this.Size.Height / 2;

            this.Width = (int)(this.Width * (double)newHeight / this.Height);
            this.Height = newHeight;

            this.Left = centerLeft - this.Size.Width / 2;
            this.Top = centerTop - this.Size.Height / 2;
        }

            private void characterWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            DX.DxLib_End();//DxLibの終了処理
        }

        private void characterWindow_Load(object sender, EventArgs e)
        {
            Console.WriteLine("characterWindow_Load");
            this.MouseDown += new MouseEventHandler(characterWindow_MouseDown);
            this.MouseMove += new MouseEventHandler(characterWindow_MouseMove);
            this.MouseWheel += new MouseEventHandler(characterWindow_MouseWheel);
            this.Move += new EventHandler(characterWindow_Move);

        }
        private void characterWindow_Shown(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;//フォームの枠を非表示にする
            TransparencyKey = Color.FromArgb(0, 0, 0);//透過色を設定
            changeListenerDir();
        }
    }
}
