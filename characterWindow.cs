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


namespace DesktopAssistant
{
    public partial class CharacterWindow : Form
    {
        private enum WINDOW_STATE
        {
            WINDOW_STARTING,
            WINDOW_NORMAL,
            WINDOW_CLOSING,
            WINDOW_SHOWED,
            WINDOW_HIDDEN,
        }

        private enum CHARA_STATE
        {
            CHARA_SHOW,
            CHARA_HIDE
        }
        /*private enum MOTION_STATE
        {
            MOTION_HI,
            MOTION_WAIT,
            MOTION_WORK,
            MOTION_DANCE,
            MOTION_BYE
        }*/

        public enum MOTION_INDEX
        {
            MOTION_START,
            MOTION_CLOSE,
            MOTION_DANCE_0,
            MOTION_DANCE_1,
            MOTION_DANCE_2
        }

        private int initStage;
        private int modelHandle;
        private int attachIndex;
        private int poseIndex;
        private int musicHandle;
        private int imageHandle;
        private int maximumHeight = (int)(Screen.PrimaryScreen.Bounds.Height * 1.5);
        private int minimumHeight = (int)(Screen.PrimaryScreen.Bounds.Height * 0.3);
        public int backGroundRed = 0;
        public int backGroundGreen = 128;
        public int backGroundBlue = 0;
        private float playTime;
        private float totalTime;
        private float imagePlayTime;
        private float poseTime;
        private float imageShowTime = 3.0f;
        private float imageShowHeight;
        private bool pose = false;
        private bool charaDisplaySwitched = false;
        private WINDOW_STATE windowState;
        private CHARA_STATE charaState;
        // private MOTION_STATE motionState;
        private DX.VECTOR cameraTargetPosition = DX.VGet(0.0f, 13.0f, -0.0f);
        private DX.VECTOR cameraHomePosition = DX.VGet(0.0f, 13.0f, -30.0f);
        private DX.VECTOR listenerHomePos = DX.VGet(0.0f, 0.0f, 0.0f);
        private DX.VECTOR listenerPos = DX.VGet(0.0f, 0.0f, 0.0f);
        private DX.VECTOR listenerDir = DX.VGet(0.0f, 0.0f, 1.0f);
        private DateTime startTime;
        private DateTime nowTime;
        private DateTime imageStartTime;
        private Point mousePoint;

        public CharacterWindow()
        {
            InitializeComponent();//フォームの初期設定
            ClientSize = new Size((int)(Screen.PrimaryScreen.Bounds.Width*0.8), (int)(Screen.PrimaryScreen.Bounds.Height*1.4));//画面サイズの設定
            this.BackColor = Color.Black;
            // set this window top most
            Text = "キャラクターウィンドウ";//ウインドウの名前を設定
            FormBorderStyle = FormBorderStyle.None;//フォームの枠を非表示にする
            TransparencyKey = Color.FromArgb(backGroundRed, backGroundGreen, backGroundBlue);//透過色を設定
        }

        public int InitProgress()
        {
            return initStage;
        }

        public void Ready()
        {
            initStage = 120;
        }

        public int InitDX()
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
            ChangeListenerDir();

            DX.SetBackgroundColor(backGroundRed, backGroundGreen, backGroundBlue);

            initStage = 80;
            if (DX.DxLib_Init() < 0)
                return -1;
            // set Physics Mode
            DX.MV1SetLoadModelUsePhysicsMode(DX.DX_LOADMODEL_PHYSICS_REALTIME);

            initStage = 99;
            modelHandle = DX.MV1LoadModel("Data/Model/character.pmx");//3Dモデルの読み込み
            if (modelHandle < 0)
                return -1;
            DX.MV1PhysicsResetState(modelHandle);
            charaState = CHARA_STATE.CHARA_HIDE;

            initStage = 100;

            // DX.MV1SetPhysicsWorldGravity(modelHandle, DX.VGet(0f, -50f, 0f));
            DX.MV1SetPhysicsWorldGravity(modelHandle, DX.VGet(0f, -230f, 0f));
            DX.SetCameraNearFar(0.1f, 1000.0f);//奥行0.1～1000をカメラの描画範囲とする
            DX.SetCameraPositionAndTarget_UpVecY(cameraHomePosition, cameraTargetPosition);//第1引数の位置から第2引数の位置を見る角度にカメラを設置

            imageHandle = DX.LoadGraph("Data/Image/loadCharaPicture.jpg");
            
            this.TopMost = true;
            windowState = WINDOW_STATE.WINDOW_STARTING;
            charaDisplaySwitched = false;

            this.Width = (int)(Screen.PrimaryScreen.Bounds.Width * 0.4);
            this.Height = (int)(Screen.PrimaryScreen.Bounds.Height * 0.7);

            imageStartTime = DateTime.Now;
            return 0;
        }
        public void MainLoop()
        {
            DX.ClearDrawScreen();//裏画面を消す
            nowTime = DateTime.Now;
            playTime = (float)(nowTime - startTime).TotalMilliseconds * 30f / 1000f;
            if (!pose)
                DX.MV1SetAttachAnimTime(modelHandle, attachIndex, playTime);//モーションの再生位置を設定
            else
                DX.MV1SetAttachAnimTime(modelHandle, poseIndex, poseTime);//ポーズの再生位置を設定

            DX.MV1PhysicsCalculation(modelHandle, 1000f / 30f);

            if (charaState == CHARA_STATE.CHARA_SHOW)
                DX.MV1DrawModel(modelHandle);

            if ((windowState == WINDOW_STATE.WINDOW_STARTING) || (windowState == WINDOW_STATE.WINDOW_CLOSING))
                ImageHandle();

            if (windowState == WINDOW_STATE.WINDOW_NORMAL)
                NormalProcess();

            // if escape hit key, end this aplication
            if (DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) != 0)
            {
                // stop music
                if (DX.CheckSoundFile() == 1)
                    DX.StopSoundFile();
                DX.InitSoundMem();

                PlayMotion(MOTION_INDEX.MOTION_CLOSE, true);
                windowState = WINDOW_STATE.WINDOW_CLOSING;
                charaDisplaySwitched = false;
                double cameraDistance = Math.Sqrt(Math.Pow(cameraHomePosition.x, 2) + Math.Pow(cameraHomePosition.z, 2));
                DX.SetCameraPositionAndTarget_UpVecY(DX.VGet((float)(cameraDistance*Math.Cos(Math.PI*2f*(-1/4f+50/360f))), cameraHomePosition.y, (float)(cameraDistance*Math.Sin(Math.PI*2f*(-1/4f+50/360f)))),
                                                     cameraTargetPosition);//第1引数の位置から第2引数の位置を見る角度にカメラを設置
            }
            DX.ScreenFlip();//裏画面を表画面にコピー
        }

        private void ImageHandle()
        {
            if ((windowState == WINDOW_STATE.WINDOW_CLOSING) && (playTime < totalTime))
            {
                imageStartTime = DateTime.Now;
            }
            else
            {
                imagePlayTime = (float)(nowTime - imageStartTime).TotalMilliseconds / 1000f;

                imageShowHeight = 13.0f * (float)Math.Sin(Math.PI * (imagePlayTime / imageShowTime));
                DX.VECTOR camPosition = DX.GetCameraPosition();
                DX.VECTOR camTarget = DX.GetCameraTarget();
                double cameraDistance = Math.Sqrt(Math.Pow(camPosition.x - camTarget.x, 2) + Math.Pow(camPosition.z - camTarget.z, 2));
                double angle = Math.Asin(camPosition.x / cameraDistance) / (2.0f * Math.PI) * 360;
                DX.DrawExtendGraph3D((2.0f * camPosition.x + camTarget.x) / 3.0f, imageShowHeight, (2.0f * camPosition.z + camTarget.z) / 3.0f, 0.015, 0.015, imageHandle, DX.FALSE);

                if ((!charaDisplaySwitched) && (imagePlayTime >= imageShowTime / 2))
                {
                    SwitchCharaDisplay();
                    charaDisplaySwitched = true;
                    playPose(MOTION_INDEX.MOTION_CLOSE, 0.0f);
                }
                
                if (imagePlayTime >= imageShowTime)
                {
                    if (windowState == WINDOW_STATE.WINDOW_STARTING)
                    {
                        windowState = WINDOW_STATE.WINDOW_NORMAL;
                        PlayMotion(MOTION_INDEX.MOTION_CLOSE);
                    }
                    else
                    {
                        Close();
                    }
                }
            }
        }

        private void NormalProcess()
        {
            //ESCキーを押したら終了
            if (DX.CheckHitKey(DX.KEY_INPUT_2) != 0)
            {
                // stop music
                if (DX.CheckSoundFile() == 1)
                    DX.StopSoundFile();
                DX.InitSoundMem();
                PlayDance(MOTION_INDEX.MOTION_DANCE_0, "Data/Music/do-natu.mp4", 0.18, true);
            }

            if (DX.CheckHitKey(DX.KEY_INPUT_3) != 0)
            {
                // stop music
                if (DX.CheckSoundFile() == 1)
                    DX.StopSoundFile();
                DX.InitSoundMem();
                PlayDance(MOTION_INDEX.MOTION_DANCE_1, "Data/Music/onegai.mp4", 0.8);

            }

            if (DX.CheckHitKey(DX.KEY_INPUT_4) != 0)
            {
                // stop music
                if (DX.CheckSoundFile() == 1)
                    DX.StopSoundFile();
                DX.InitSoundMem();
                PlayDance(MOTION_INDEX.MOTION_DANCE_2, "Data/Music/onegai.mp4", 0.8);

            }
        }

        private void SwitchCharaDisplay()
        {
            if (charaState == CHARA_STATE.CHARA_HIDE)
                charaState = CHARA_STATE.CHARA_SHOW;
            else
                charaState = CHARA_STATE.CHARA_HIDE;
        }

        private void playPose(MOTION_INDEX motionIndex, float time)
        {
            poseIndex = DX.MV1DetachAnim(modelHandle, poseIndex);//ポーズの中止
            attachIndex = DX.MV1DetachAnim(modelHandle, attachIndex);//モーションの中止
            poseIndex = DX.MV1AttachAnim(modelHandle, (int)motionIndex, -1, DX.FALSE);//モーションの選択
            DX.MV1PhysicsResetState(modelHandle);
            playTime = time;
            pose = true;
        }

        private void PlayMotion(MOTION_INDEX motionIndex, bool resetPhysics = false)
        {
            poseIndex = DX.MV1DetachAnim(modelHandle, poseIndex);//ポーズの中止
            attachIndex = DX.MV1DetachAnim(modelHandle, attachIndex);//モーションの中止
            attachIndex = DX.MV1AttachAnim(modelHandle, (int)motionIndex, -1, DX.FALSE);//モーションの選択
            Console.WriteLine("attachIndex : " + attachIndex);

            totalTime = DX.MV1GetAttachAnimTotalTime(modelHandle, attachIndex);//モーションの総再生時間を取得
            if (resetPhysics)
            {
                DX.MV1PhysicsResetState(modelHandle);
                Console.WriteLine("reset physics");

            }
            startTime = DateTime.Now;
            pose = false;
            
        }

        public void PlayDance(MOTION_INDEX motionIndex, String FileName, double delayTime = 0.0, bool resetPhysics = false)
        {
            PlayMotion(motionIndex, resetPhysics);

            DX.SetCreate3DSoundFlag(DX.TRUE);
            musicHandle = DX.LoadSoundMem(FileName);
            DX.SetCreate3DSoundFlag(DX.FALSE);
            DX.Set3DRadiusSoundMem(10.0f, musicHandle);
            DX.Set3DPresetReverbParamSoundMem(DX.DX_REVERB_PRESET_GENERIC, musicHandle);
            DX.SetNextPlay3DPositionSoundMem(DX.VGet(0.0f, 0.0f, 2.0f), musicHandle);

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

        private void ChangeListenerDir()
        {
            int centerLeft = this.Left + this.Size.Width / 2;
            int centerTop = this.Top + this.Size.Height / 2;
            float widthRate = (centerLeft - Screen.PrimaryScreen.Bounds.Width / 2f) / Screen.PrimaryScreen.Bounds.Width;
            DX.VECTOR listenerPos = DX.VGet(listenerHomePos.x - 3.0f * widthRate, listenerHomePos.y, listenerHomePos.z);
            DX.Set3DSoundListenerPosAndFrontPos_UpVecY(listenerPos, DX.VAdd(listenerPos, listenerDir));
        }

        private void CharacterWindow_Move(object sender, EventArgs e)
        {
            ChangeListenerDir();
        }

        //Form1のMouseDownイベントハンドラ
        //マウスのボタンが押されたとき
        private void CharacterWindow_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        //Form1のMouseMoveイベントハンドラ
        //マウスが動いたとき
        private void CharacterWindow_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
        }
        private void CharacterWindow_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
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

            private void CharacterWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            DX.DxLib_End();//DxLibの終了処理
        }

        private void CharacterWindow_Load(object sender, EventArgs e)
        {
            Console.WriteLine("characterWindow_Load");
            this.MouseDown += new MouseEventHandler(CharacterWindow_MouseDown);
            this.MouseMove += new MouseEventHandler(CharacterWindow_MouseMove);
            this.MouseWheel += new MouseEventHandler(CharacterWindow_MouseWheel);
            this.Move += new EventHandler(CharacterWindow_Move);
            DX.ScreenFlip();

        }
        private void CharacterWindow_Shown(object sender, EventArgs e)
        {
            DX.ScreenFlip();
        }
    }
}
