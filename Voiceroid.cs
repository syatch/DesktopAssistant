using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

//using Codeer.Friendly;
//using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using Ong.Friendly.FormsStandardControls;

namespace DesktopAssistant
{
    class Voiceroid
    {
        public void voiceroid()
        {
            string PATH_VOICEROID = @"C:\Program Files (x86)\AHS\VOICEROID2\VoiceroidEditor.exe";
            string processName = "VOICEROID";
            ProcessStartInfo voiceRoid = new ProcessStartInfo();
            voiceRoid.FileName = PATH_VOICEROID;
            voiceRoid.CreateNoWindow = true;
            voiceRoid.UseShellExecute = false;

            //VOICEROIDの起動
            Process.Start(voiceRoid);
            //起動の待ち時間: 5000ms = 5s
            Thread.Sleep(20000);
            Console.WriteLine("get process by name");
            Process voiceRoidProcess = Process.GetProcessesByName(processName)[0]; <<< error
            Console.WriteLine("get windows app friend");
            var w = new WindowsAppFriend(voiceRoidProcess);
            Console.WriteLine("start");
            var main = WindowControl.FromZTop(w);
            Console.WriteLine("start");
            //テキストボックス
            var textBox = main.IdentifyFromZIndex(2, 0, 0, 1, 0, 1, 1);
            Console.WriteLine("start");
            //再生ボタン
            var playButton = new FormsButton(main.IdentifyFromZIndex(2, 0, 0, 1, 0, 1, 0, 3));
            //保存ボタン
            var saveButton = new FormsButton(main.IdentifyFromZIndex(2, 0, 0, 1, 0, 1, 0, 1));

            string text = "てすと";
            textBox["Text"](text);
            Thread.Sleep(100);
            //再生
            playButton.EmulateClick();
            //音声読み終わりまで待つ
            while (!saveButton.Enabled)
            {
                Thread.Sleep(100);
            }
        }
    }
}
