using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace ELEBEATMusicEditer
{
    class MusicPlayer
    {
        //mciSendStringを使うためのおまじない
        [DllImport("winmm.dll")]
        extern static int mciSendString(string s1, StringBuilder s2, int i1, int i2);

        //音楽ファイルのパス
        public string MusicFilePath;
        //音楽が取り込んであるか
        public bool FetchMusicFlag;
        //音楽ファイルの名前
        public string MusicFileName;
        //音楽再生状態
        public string PlayingMusicMode;
        //合計再生時間 ミリ秒
        public int TotalMusicTime;
        //合計再生時間 DateTime型
        public DateTime TotalMusicDateTime = new DateTime();
        //現在の再生時間。ミリ秒
        public int PresentMusicTime;
        //現在の再生時間。DateTime型
        private DateTime PresentMusicDateTime = new DateTime();
        //再生速度
        public int PlayMusicSpeed;

        //Temp
        StringBuilder TempPresentTime = new StringBuilder(32);
        StringBuilder SoundMode = new StringBuilder(32);

        //コンストラクタ
        public MusicPlayer()
        {
            FetchMusicFlag = false;
            TotalMusicTime = 0;
            PresentMusicTime = 0;
            PlayMusicSpeed = 1000;

        }

        //リセット
        public void MusicPlayerReset()
        {
            MusicFilePath = null;
            FetchMusicFlag = false;
            MusicFileName = null;
            PlayingMusicMode = null;
            TotalMusicTime = 0;
            TotalMusicDateTime = new DateTime();
            PresentMusicTime = 0;
            PresentMusicDateTime = new DateTime();
            mciSendString("close MySound", null, 0, 0);

        }

        //再生状態の取得
        public string GetPlayingMusicMode()
        {
            mciSendString("status MySound mode", SoundMode, SoundMode.Capacity, 0);
            PlayingMusicMode = SoundMode.ToString();
            return (PlayingMusicMode);
        }

        //音楽の取り込み。再生時間の取得など。
        public void FetchMusic(string FileName,string SafeFileName)
        {
            if (FileName == null || SafeFileName == null)
            {
                throw new ArgumentNullException();
            }

            //すでに音楽ファイルを開いているときの処理
            mciSendString("close all", null, 0, 0);

            //選択したファイルの再生準備
            try
            {
                mciSendString("open \"" + FileName + "\" alias MySound", null, 0, 0);
            }
            catch
            {
                throw new Exception("対象のファイルを再生できませんでした。");
            }
            //ファイルパスを保存
            MusicFilePath = FileName;

            //ファイル名の取得
            ////////string[] MusicFileNameTemp = MusicFilePath.Split('\\');
            ////////if (MusicFileNameTemp.Count() > 0)
            ////////{
            ////////    MusicFileNameTemp = MusicFileNameTemp[MusicFileNameTemp.Count() - 1].Split('.');
            ////////}
            ////////else
            ////////{
            ////////    MusicFileNameTemp = MusicFileNameTemp[0].Split('.');
            ////////}
            ////////for (int i = 0; i < MusicFileNameTemp.Length - 1; i++)
            ////////{
            ////////    if (i > 0)
            ////////    {
            ////////        MusicFileName += ".";
            ////////    }
            ////////    MusicFileName += MusicFileNameTemp[i];
            ////////}
            MusicFileName = SafeFileName;

            mciSendString("set MusicAlias time format milliseconds", null, 0, 0);

            //再生時間の取得
            StringBuilder TempTotalSoundTime = new StringBuilder(32);
            try
            {
                mciSendString("status MySound length", TempTotalSoundTime, TempTotalSoundTime.Capacity, 0);
                TotalMusicDateTime = new DateTime();
                TotalMusicTime = int.Parse(TempTotalSoundTime.ToString());
            }
            catch
            {
                throw new Exception("音楽ファイルの再生時間を取得する際に例外が発生しました。");
            }
            //DateTime型の再生時間も
            TotalMusicDateTime = TotalMusicDateTime.AddMilliseconds(TotalMusicTime);

            TempTotalSoundTime = null;

            //取り込みフラグをtrueに
            FetchMusicFlag = true;
        }

        //現在の再生時間の取得
        public int GetPresentTime()
        {
            if (FetchMusicFlag == true)
            {
                mciSendString("status MySound position", TempPresentTime, TempPresentTime.Capacity, 0);
                PresentMusicTime = int.Parse(TempPresentTime.ToString());
            }
            else
            {
                PresentMusicTime = 0;
            }
            return (PresentMusicTime);
        }

        //現在の再生時間の取得
        public DateTime GetPresentDateTime()
        {
            GetPresentTime();
            PresentMusicDateTime = new DateTime();
            PresentMusicDateTime = PresentMusicDateTime.AddMilliseconds(PresentMusicTime);
            return (PresentMusicDateTime);
        }

        //再生する
        public void PlayMusic()
        {
            //音楽ファイルが取り込んであるか
            if (FetchMusicFlag == true)
            {
                GetPlayingMusicMode();
                GetPresentTime();
                if (PlayingMusicMode == "stopped")
                {
                    if (PresentMusicTime >= TotalMusicTime)
                    {
                        mciSendString("play MySound from 0", null, 0, 0);
                    }
                    else
                    {
                        mciSendString("play MySound", null, 0, 0);
                    }
                }
                else if (PlayingMusicMode == "paused")
                {
                    mciSendString("resume MySound", null, 0, 0);
                }
            }
        }

        //一時停止する
        public void StopMusic()
        {
            if (FetchMusicFlag == true)
            {
                GetPlayingMusicMode();
                if (PlayingMusicMode == "playing")
                {
                    mciSendString("pause MySound", null, 0, 0);
                }
            }
        }

        //任意のミリ秒にシークする
        public void SeekPresentMusicTime(int TimeToSeek)
        {
            if (FetchMusicFlag == true)
            {
                if (TotalMusicTime > TimeToSeek)
                {
                    GetPlayingMusicMode();
                    if (PlayingMusicMode == "playing")
                    {
                        mciSendString("play MySound from " + TimeToSeek.ToString(), null, 0, 0);
                    }
                    else if (PlayingMusicMode == "stopped")
                    {
                        mciSendString("seek MySound to " + TimeToSeek.ToString(), null, 0, 0);
                    }
                    else if (PlayingMusicMode == "paused")
                    {
                        mciSendString("seek MySound to " + TimeToSeek.ToString(), null, 0, 0);
                    }
                    else
                    {
                    }
                }
            }
        }

        //1拍戻る
        public void BackBeat(int PresentBPM)
        {
            if (FetchMusicFlag == true)
            {
                GetPresentTime();
                int TempPresentBPM = PresentBPM;
                int BeatInterVal = (int)(60000.0 / TempPresentBPM);
                if (PresentMusicTime - BeatInterVal > 0)
                {
                    SeekPresentMusicTime(PresentMusicTime - BeatInterVal);
                }
                else
                {
                    SeekPresentMusicTime(TotalMusicTime + (PresentMusicTime - BeatInterVal));
                }
            }
        }

        //1拍進む
        public void GoBeat(int PresentBPM)
        {
            if (FetchMusicFlag == true)
            {
                GetPresentTime();
                int TempPresentBPM = PresentBPM;
                int BeatInterVal = (int)(60000.0 / TempPresentBPM);
                if (PresentMusicTime + BeatInterVal < TotalMusicTime)
                {
                    SeekPresentMusicTime(PresentMusicTime + BeatInterVal);
                }
                else
                {
                    SeekPresentMusicTime(PresentMusicTime+BeatInterVal-TotalMusicTime);
                }
            }
        }

        //1小節戻る
        public void BackMeasure(int PresentBPM)
        {
             if (FetchMusicFlag == true)
            {
                GetPresentTime();
                int TempPresentBPM = PresentBPM;
                int BeatInterVal = (int)(60000.0 / TempPresentBPM * 4.0);
                if (PresentMusicTime - BeatInterVal > 0)
                {
                    SeekPresentMusicTime(PresentMusicTime - BeatInterVal);
                }
                else
                {
                    SeekPresentMusicTime(TotalMusicTime + (PresentMusicTime - BeatInterVal));
                }
            }
        }

        //1小節進む
        public void GoMeasure(int PresentBPM)
        {
            if (FetchMusicFlag == true)
            {
                GetPresentTime();
                int TempPresentBPM = PresentBPM;
                int BeatInterVal = (int)(60000.0 / TempPresentBPM * 4.0);
                if (PresentMusicTime + BeatInterVal < TotalMusicTime)
                {
                    SeekPresentMusicTime(PresentMusicTime + BeatInterVal);
                }
                else
                {
                    SeekPresentMusicTime(PresentMusicTime + BeatInterVal - TotalMusicTime);
                }
            }
        }
 
        //再生速度を変更する
        public void ChangeMusicSpeed(int NewPlayMusicSpeed)
        {
            if (FetchMusicFlag == true)
            {
                if (NewPlayMusicSpeed > 0 && NewPlayMusicSpeed < 2000)
                {
                    mciSendString("set MySound speed " + NewPlayMusicSpeed.ToString(), null, 0, 0);
                    PlayMusicSpeed = NewPlayMusicSpeed;
                }
            }
        }

        //音楽ファイルを閉じる
        public void CloseMusicPlayer()
        {
            mciSendString("close MySound", null, 0, 0);
        }
    }
}
