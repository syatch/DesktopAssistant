using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            // Create instance
            CharacterWindow charaWindow = new CharacterWindow();
            Voiceroid voiceroidTalker = new Voiceroid();
            VoiceRecognize VoiceRecognizer = new VoiceRecognize();

            Task loadingScreenTask = Task.Run(() =>
            {
                // Show the load window
                LoadWindow loadWindow = new LoadWindow();
                //loadCharacter.Owner = loadWindow;
                loadWindow.Show();
                loadWindow.Left = (int)(Screen.PrimaryScreen.Bounds.Width * 0.5 - loadWindow.Size.Width / 2);
                loadWindow.Top = (int)(Screen.PrimaryScreen.Bounds.Height * 0.5 - loadWindow.Size.Height / 2);
                // Show progress while the character window init
                while (!loadWindow.UpdateLoadProgress(charaWindow.InitProgress()))
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                }

                loadWindow.Close();
            });

            // Wait for init of the character window
            if (charaWindow.InitDX() < 0)
                return;

            // wait for load task
            loadingScreenTask.Wait();

            // talk
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

            // Show the character window
            charaWindow.Show();

            // To draw the character with DX lib, not Application.Run() but while loop used
            while (charaWindow.Created)
            {
                charaWindow.MainLoop();
                Application.DoEvents();
            }
        }
    }
}