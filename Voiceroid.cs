using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
//using Ong.Friendly.FormsStandardControls;
using Codeer.Friendly.Windows.NativeStandardControls;
using RM.Friendly.WPFStandardControls;

namespace DesktopAssistant
{
    public class Voiceroid
    {
        public SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private readonly string PATH_VOICEROID = @"C:\Program Files (x86)\AHS\VOICEROID2\VoiceroidEditor";
        private readonly string voiceroidProcessName = "VoiceroidEditor";
        private readonly int CidBase = 2000;
        private Process voiceroidProcess = null;
        private WindowsAppFriend app = null;
        private WindowControl uiTreeTop = null;
        private WPFTabControl VoicePresetTab = null;
        private WPFTabControl TuneTab = null;
        private WPFTextBox    TalkTextBox = null;
        private WPFButtonBase PlayButton = null;
        private WPFButtonBase SaveButton = null;
        private WPFListView AvatorListView_std = null;
        private WPFListView AvatorListView_usr = null;

        public Voiceroid()
        {
            GetProcess();
            GetElements();
        }

        private void GetProcess()
        {
            var VrStartInfo = new ProcessStartInfo();
            VrStartInfo.FileName = PATH_VOICEROID;
            VrStartInfo.CreateNoWindow = true;
            VrStartInfo.UseShellExecute = false;
            voiceroidProcess = Process.Start(VrStartInfo);
            Thread.Sleep(5000);

        }

        private void GetElements()
        {
            if (voiceroidProcess != null)
            {
                try
                {
                    var processArray = Process.GetProcessesByName(voiceroidProcessName);
                    app = new WindowsAppFriend(processArray[0]);
                    uiTreeTop = WindowControl.FromZTop(app);

                    //判明しているGUI要素特定
                    var tabs = uiTreeTop.GetFromTypeFullName("AI.Framework.Wpf.Controls.TitledTabControl");
                    VoicePresetTab = new WPFTabControl(tabs[0]);  // ボイス（プリセット）のタブコントロール
                    TuneTab = new WPFTabControl(tabs[1]);  // チューニングのタブコントロール

                    var editUis = uiTreeTop.GetFromTypeFullName("AI.Talk.Editor.TextEditView")[0].LogicalTree();
                    TalkTextBox = new WPFTextBox(editUis[4]);     // テキストボックス
                    PlayButton = new WPFButtonBase(editUis[6]);  // 再生ボタン
                    SaveButton = new WPFButtonBase(editUis[24]); // 音声保存ボタン

                    // //標準タブにいる各話者毎のGUI要素データを取得
                    // TuneTab.EmulateChangeSelectedIndex(1);
                    // VoicePresetTab.EmulateChangeSelectedIndex(0);
                    // AvatorListView_std = new WPFListView(uiTreeTop.GetFromTypeFullName("System.Windows.Controls.ListView")[0]);
                    // ScanPreset(AvatorListView_std, 0, 0);

                    // //ユーザータブにいる各話者プリセット毎のGUI要素データを取得
                    // TuneTab.EmulateChangeSelectedIndex(1);
                    // VoicePresetTab.EmulateChangeSelectedIndex(1);
                    // AvatorListView_usr = new WPFListView(uiTreeTop.GetFromTypeFullName("System.Windows.Controls.ListView")[1]);
                    // ScanPreset(AvatorListView_usr, AvatorListView_std.ItemCount, 1);
                }
                catch (Exception e)
                {
                    // ThrowException(string.Format(@"{0} {1}", e.Message, e.StackTrace));
                }
            }
        }

        /// <summary>
        /// 指定話者で指定テキストで発声
        /// </summary>
        /// <param name="cid">話者CID</param>
        /// <param name="talkText">発声させるテキスト</param>
        /// <returns>発声にかかった時間（ミリ秒）</returns>
        public void PlayAsync(int cid, string talkText)
        {
            Task.Run(() =>
            {
                Semaphore.Wait();

                bool iconFind = false;
                //int avatorIndex = ConvertAvatorIndex(cid);

                // 最後の確認ダイアログを殺し切れていなかった時のための処理
                var infoOldDlgs = WindowControl.GetFromWindowText(app, "情報");
                try
                {
                    if ((infoOldDlgs.Length != 0) && (infoOldDlgs[0].WindowClassName == "#32770"))
                    {
                        //OKボタンを押す
                        NativeButton btn = new NativeButton(infoOldDlgs[0].IdentifyFromWindowClass("Button"));
                        btn.EmulateClick(new Async());
                        Thread.Sleep(10);
                    }
                }
                catch (Exception)
                {
                    //
                }

                if (PlayButton == null) return;
                if (SaveButton == null) return;
                if (TalkTextBox == null) return;

                TuneTab.EmulateChangeSelectedIndex(1);

                // 話者切り替え
                //AvatorSelect(avatorIndex);

                // 音声保存ボタンを使った再生終了判定を止めて、再生ボタンのアイコンの状態で判定する方法に変更する。
                var items01 = PlayButton.LogicalTree(TreeRunDirection.Descendants).ByType("System.Windows.Controls.Image");
                dynamic playButtonImage1 = items01[0].Dynamic(); // 再生アイコン。このアイコンが有効時？の判定はまだないな...
                dynamic playButtonImage2 = items01[1].Dynamic(); // 停止アイコン。処理ではこのアイコンのプロパティを持ている

                //ApplyEffectParameters(avatorIndex);
                //ApplyEmotionParameters(avatorIndex);

                // 再生中なので再生終了を待つ
                // ※再生ボタンのアイコンが再生アイコンに切り替わるのを待つ
                iconFind = playButtonImage2.IsVisible;
                if (iconFind)
                {
                    while (iconFind)
                    {
                        Thread.Sleep(10);
                        iconFind = playButtonImage2.IsVisible;
                    }
                }

                TalkTextBox.EmulateChangeText(talkText);
                Thread.Sleep(10);

                PlayButton.EmulateClick();

                // 再生開始を待つ
                // 再生ボタンのアイコンが停止アイコンに切り替わるのを待つ
                iconFind = playButtonImage2.IsVisible;
                if (!iconFind)
                {
                    while (!iconFind)
                    {
                        Thread.Sleep(10);
                        iconFind = playButtonImage2.IsVisible;
                    }
                }

                // 再生終了を待つ
                // 再生ボタンのアイコンが再生アイコンに切り替わるのを待つ
                iconFind = playButtonImage2.IsVisible;
                if (iconFind)
                {
                    while (iconFind)
                    {
                        Thread.Sleep(10);
                        iconFind = playButtonImage2.IsVisible;
                    }
                }

                Semaphore.Release();
            });
        }

/*
        private void ScanPreset(WPFListView avatorListView, int idxBase, int presetTabIndex)
        {
            for (int avatorIdx = 0; avatorIdx < avatorListView.ItemCount; avatorIdx++)
            {
                VOICEROID2.AvatorParam avator = new VOICEROID2.AvatorParam();

                avator.AvatorUI = new VOICEROID2.AvatorUIParam();
                avator.VoiceEmotions = new Dictionary<string, EffectValueInfo>();
                avator.VoiceEmotions_default = new Dictionary<string, EffectValueInfo>();
                avator.AvatorUI.EmotionSliderIndexs = new Dictionary<string, int>();
                avator.AvatorUI.EmotionSliders = new Dictionary<int, WPFSlider>();

                avatorListView.EmulateChangeSelectedIndex(avatorIdx);
                TuneTab.EmulateChangeSelectedIndex(1); // ボイスタブ

                //プリセット名取得（話者名）
                var params1 = TuneTab.VisualTree(TreeRunDirection.Descendants).ByType("AI.Framework.Wpf.Controls.TextBoxEx")[0];
                WPFTextBox nameTextBox = new WPFTextBox(params1);

                avator.AvatorName = nameTextBox.Text;

                //スライダーの配列を取得(共通)
                try
                {
                    TuneTab.EmulateChangeSelectedIndex(1); // チューニング:ボイスタブ
                    var params2 = TuneTab.VisualTree(TreeRunDirection.Descendants).ByType("System.Windows.Controls.Slider");
                    avator.AvatorUI.VolumeSlider = new WPFSlider(params2[0]);
                    avator.AvatorUI.SpeedSlider = new WPFSlider(params2[1]);
                    avator.AvatorUI.PitchSlider = new WPFSlider(params2[2]);
                    avator.AvatorUI.IntonationSlider = new WPFSlider(params2[3]);
                    avator.AvatorUI.ShortPauseSlider = new WPFSlider(params2[4]);
                    avator.AvatorUI.LongPauseSlider = new WPFSlider(params2[5]);
                    avator.AvatorUI.WithEmotionParams = false;

                    avator.VoiceEffects_default = new Dictionary<EnumVoiceEffect, EffectValueInfo>
                    {
                        {EnumVoiceEffect.volume,     new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.VolumeSlider.Value), Convert.ToDecimal(avator.AvatorUI.VolumeSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.VolumeSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.speed,      new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.SpeedSlider.Value), Convert.ToDecimal(avator.AvatorUI.SpeedSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.SpeedSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.pitch,      new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.PitchSlider.Value), Convert.ToDecimal(avator.AvatorUI.PitchSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.PitchSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.intonation, new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.IntonationSlider.Value), Convert.ToDecimal(avator.AvatorUI.IntonationSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.IntonationSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.shortpause, new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.ShortPauseSlider.Value), Convert.ToDecimal(avator.AvatorUI.ShortPauseSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.ShortPauseSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.longpause,  new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.LongPauseSlider.Value), Convert.ToDecimal(avator.AvatorUI.LongPauseSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.LongPauseSlider.Maximum), 0.01m)}
                    };
                    avator.VoiceEffects = new Dictionary<EnumVoiceEffect, EffectValueInfo>
                    {
                        {EnumVoiceEffect.volume,     new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.VolumeSlider.Value), Convert.ToDecimal(avator.AvatorUI.VolumeSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.VolumeSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.speed,      new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.SpeedSlider.Value), Convert.ToDecimal(avator.AvatorUI.SpeedSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.SpeedSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.pitch,      new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.PitchSlider.Value), Convert.ToDecimal(avator.AvatorUI.PitchSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.PitchSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.intonation, new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.IntonationSlider.Value), Convert.ToDecimal(avator.AvatorUI.IntonationSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.IntonationSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.shortpause, new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.ShortPauseSlider.Value), Convert.ToDecimal(avator.AvatorUI.ShortPauseSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.ShortPauseSlider.Maximum), 0.01m)},
                        {EnumVoiceEffect.longpause,  new EffectValueInfo(Convert.ToDecimal(avator.AvatorUI.LongPauseSlider.Value), Convert.ToDecimal(avator.AvatorUI.LongPauseSlider.Minimum), Convert.ToDecimal(avator.AvatorUI.LongPauseSlider.Maximum), 0.01m)}
                    };
                }
                catch (Exception ep2)
                {
                    ThrowException(string.Format("ep2 fail. unknown gui(LinearFader capture).{0}", ep2.Message));
                }

                //スライダーの配列を取得(スタイル)
                try
                {
                    var params3 = TuneTab.VisualTree(TreeRunDirection.Descendants).ByType("System.Windows.Controls.ListBox").Single();

                    WPFListBox sliders = new WPFListBox(params3);

                    if ((sliders != null) && (sliders.ItemCount != 0))
                    {
                        for (int sidx = 0; sidx < sliders.ItemCount; sidx++)
                        {
                            var sitem = sliders.GetItem(sidx);
                            var textblocks = sitem.VisualTree().ByType("System.Windows.Controls.TextBlock");
                            WPFSlider slider = new WPFSlider(sitem.VisualTree().ByType("AI.Framework.Wpf.Controls.LinearFader").Single());
                            WPFTextBlock emoname = new WPFTextBlock(textblocks[textblocks.Count > 2 ? (textblocks.Count - 2) : 0]);

                            avator.VoiceEmotions_default.Add(emoname.Text, new EffectValueInfo(Convert.ToDecimal(slider.Value), Convert.ToDecimal(slider.Minimum), Convert.ToDecimal(slider.Maximum), 0.01m));
                            avator.VoiceEmotions.Add(emoname.Text, new EffectValueInfo(Convert.ToDecimal(slider.Value), Convert.ToDecimal(slider.Minimum), Convert.ToDecimal(slider.Maximum), 0.01m));
                            avator.AvatorUI.EmotionSliderIndexs.Add(emoname.Text, sidx);
                            avator.AvatorUI.EmotionSliders.Add(sidx, slider); // 将来のため保持しているだけ。
                        }

                        avator.AvatorUI.WithEmotionParams = true;
                    }
                }
                catch (Exception ep3)
                {
                    ThrowException(string.Format("ep3 fail. unknown gui(Style LinearFader capture).{0}", ep3.Message));
                }

                avator.AvatorIndex = idxBase + avatorIdx;
                avator.AvatorUI.PresetTabIndex = presetTabIndex;
                avator.AvatorUI.IndexOnPresetTab = avatorIdx;

                AvatorParams.Add(idxBase + avatorIdx, avator);
            }
        }

        private void AvatorSelect(int avatorIndex)
        {
            VOICEROID2.AvatorParam avator = AvatorParams[avatorIndex] as VOICEROID2.AvatorParam;

            if (AvatorListView_std == null) return;
            if (AvatorListView_usr == null) return;

            VoicePresetTab.EmulateChangeSelectedIndex(avator.AvatorUI.PresetTabIndex);
            switch (avator.AvatorUI.PresetTabIndex)
            {
                case 0:
                    AvatorListView_std.EmulateChangeSelectedIndex(avator.AvatorUI.IndexOnPresetTab);
                    break;

                case 1:
                    AvatorListView_usr.EmulateChangeSelectedIndex(avator.AvatorUI.IndexOnPresetTab);
                    break;
            }
        }

        private void ApplyEmotionParameters(int avatorIndex)
        {
            VOICEROID2.AvatorParam avator = AvatorParams[avatorIndex] as VOICEROID2.AvatorParam;

            // スタイルを持っている話者なら処理する
            if (avator.AvatorUI.WithEmotionParams)
            {
                TuneTab.EmulateChangeSelectedIndex(1); //ボイスタブ
                WPFListBox emoList = new WPFListBox(TuneTab.VisualTree(TreeRunDirection.Descendants).ByType("System.Windows.Controls.ListBox").Single());

                foreach (KeyValuePair<string, EffectValueInfo> item in avator.VoiceEmotions)
                {
                    double p = Convert.ToDouble(avator.VoiceEmotions[item.Key].value);
                    int eidx = avator.AvatorUI.EmotionSliderIndexs[item.Key];
                    WPFSlider slider = new WPFSlider(emoList.GetItem(eidx).VisualTree().ByType("AI.Framework.Wpf.Controls.LinearFader").Single());
                    slider["Value"](p);
                }
            }
        }

        private void ApplyEffectParameters(int avatorIndex)
        {
            WPFSlider slider = null;
            VOICEROID2.AvatorParam avator = AvatorParams[avatorIndex] as VOICEROID2.AvatorParam;

            TuneTab.EmulateChangeSelectedIndex(1);

            foreach (KeyValuePair<EnumVoiceEffect, EffectValueInfo> item in avator.VoiceEffects)
            {
                switch (item.Key)
                {
                    case EnumVoiceEffect.volume:
                        slider = avator.AvatorUI.VolumeSlider;
                        break;

                    case EnumVoiceEffect.speed:
                        slider = avator.AvatorUI.SpeedSlider;
                        break;

                    case EnumVoiceEffect.pitch:
                        slider = avator.AvatorUI.PitchSlider;
                        break;

                    case EnumVoiceEffect.intonation:
                        slider = avator.AvatorUI.IntonationSlider;
                        break;

                    case EnumVoiceEffect.shortpause:
                        slider = avator.AvatorUI.ShortPauseSlider;
                        break;

                    case EnumVoiceEffect.longpause:
                        slider = avator.AvatorUI.LongPauseSlider;
                        break;
                }

                double p = Convert.ToDouble(avator.VoiceEffects[item.Key].value);

                if (slider != null) slider.EmulateChangeValue(p);
            }
        }
*/






    }
}
