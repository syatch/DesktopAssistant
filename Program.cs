using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DxLibDLL;
using System.Threading;

namespace DesktopAssistant
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static async Task Main()
        {

            // Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CharacterWindow charaWindow = new CharacterWindow();
            Voiceroid voiceroidTalker = new Voiceroid();
            VoiceRecognize VoiceRecognizer = new VoiceRecognize();

            // show load window
            Task loadingScreenTask = Task.Run(() =>
            {
                LoadWindow loadWindow = new LoadWindow();
                //loadCharacter.Owner = loadWindow;
                loadWindow.Show();
                loadWindow.Left = (int)(Screen.PrimaryScreen.Bounds.Width * 0.5 - loadWindow.Size.Width / 2);
                loadWindow.Top = (int)(Screen.PrimaryScreen.Bounds.Height * 0.5 - loadWindow.Size.Height / 2);

                while (!loadWindow.UpdateLoadProgress(charaWindow.InitProgress()))
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }

                loadWindow.Close();
            });

            if (charaWindow.InitDX() < 0)
                return;

            // wait for load task
            loadingScreenTask.Wait();

            voiceroidTalker.PlayAsync(0, "起動しました");
            // charaWindow.StartPosition = FormStartPosition.CenterScreen;
            charaWindow.Left = (int)(Screen.PrimaryScreen.Bounds.Width - charaWindow.Size.Width);
            charaWindow.Top = (int)(Screen.PrimaryScreen.Bounds.Height - charaWindow.Size.Height * 0.9);

            // while (true) {

            // }
/*
            charaWindow.Left = (int)(Screen.PrimaryScreen.Bounds.Width * 0.5 - charaWindow.Size.Width / 2);
            charaWindow.Top = (int)(Screen.PrimaryScreen.Bounds.Height * 0.5 - charaWindow.Size.Height / 2);
*/

            // charaWindow.playDance(2, "Data/Music/do-natu.mp4", 0.18);
            // charaWindow.playDance(3, "Data/Music/onegai.mp4", 0.8);

            charaWindow.Show();

            //Application.Runではなく自分でループを作成
            while (charaWindow.Created)
            {
                charaWindow.MainLoop();
                Application.DoEvents();
            }
        }
    }
}