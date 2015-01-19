using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;


namespace ELEBEATMusicEditer
{
    public class NotePaint
    {
        //縦線の１拍の間隔
        public int VerticalLineBeatInterval;
        //縦線の１拍の間隔の基準
        private int VerticalLineBeatBaseInterval;
        //縦線の開始位置（ここをマイナス方向へ動かしていく）
        private int VerticalBaseLine;
        //横線の間隔
        public int ParallelLineInterval;
        //横線の開始位置
        public int ParallelLine;
        //前回の描画ノート数
        private int OldNoteCount;

        //ノートがだぶってないか
        public bool NoteisDouble;

        //Noteクラスを確保する。16Keys
        public Note BasicNote16 = new Note();
        public Note NormalNote16 = new Note();
        public Note AdvancedNote16 = new Note();
        //Noteクラスを確保する。9Keys
        public Note BasicNote9 = new Note();
        public Note NormalNote9 = new Note();
        public Note AdvancedNote9 = new Note();

        //各番号のノート
        private double[] oldNoteTime = new double[16];
        //ロングノートの継続
        private bool[] LongNoteEnable = new bool[16];

        //クリックされたアイテム、場所のクラス
        public class CheckedNote
        {
            public int Measure;
            public int Beat;
            public int Button;
            public bool BPM;
        }

        //難易度
        public enum NoteDifficlutyMode { BASIC = 0, NORMAL, ADVANCED };
        public NoteDifficlutyMode NoteDifficluty;
        //キー数
        public int Keys;

        //ペンの準備
        Pen NoteFormPen = new Pen(Color.Black);
        Pen NoteFormPenDash = new Pen(Color.YellowGreen);
        //フォントの準備
        static Font VerticalFont = new Font("MS UI Gothic", 10);
        static Font ParallelFont = new Font("MS UI Gothic", 10);
        StringFormat ParallelFontFormat = new StringFormat();
        //２点間の直線を引くためのポイント
        Point[] WaveFormPointsVerticalLine = new Point[2];
        //ノートを描画する四角形のサイズ
        public Rectangle NoteFormRectangle = new Rectangle();
        //ノートのサイズ
        Rectangle NoteRectangle = new Rectangle(0, 0, 7, 15);
        //BPMノートのサイズ
        Rectangle BPMChangeRectangle = new Rectangle(0, 0, 11, 11);

        //範囲選択の四角形のサイズ
        public Rectangle SelectRectangle = new Rectangle();
        //範囲選択中か
        public bool isSelectRectangle;
        //範囲選択の最初のノート
        public CheckedNote FirstCheckedNote = new CheckedNote();
        //範囲選択の最後のノート
        public CheckedNote LastCheckedNote = new CheckedNote();

        //補助線
        //public enum VerticalSubLineMode { Mode32 = 1, Mode16 = 2, Mode8 = 4, Mode4 = 8 };
        public int VerticalSubLine;

        //コンストラクタ
        public NotePaint()
        {
            VerticalBaseLine = 275;
            //間隔
            VerticalLineBeatInterval = 320 / 4;
            VerticalLineBeatBaseInterval = 320 / 4;
            //BPMの文字を縦に
            ParallelFontFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //デフォルトの難易度
            NoteDifficluty = NoteDifficlutyMode.NORMAL;
            //デフォルトのキー数
            Keys = 16;
            //デフォルトの補助線
            VerticalSubLine = 2;
            NoteFormPenDash.DashStyle = DashStyle.Dash;
        }

        //デストラクタ
        ~NotePaint()
        {
            //ペンの削除
            NoteFormPen.Dispose();
            NoteFormPen = null;
            VerticalFont.Dispose();
            VerticalFont = null;
            ParallelFont.Dispose();
            ParallelFont = null;
            ParallelFontFormat.Dispose();
            ParallelFontFormat = null;
            WaveFormPointsVerticalLine = null;
        }

        //リセット
        public void NotepaintReset()
        {
            BasicNote9 = new Note();
            NormalNote9 = new Note();
            AdvancedNote9 = new Note();
            BasicNote16 = new Note();
            NormalNote16 = new Note();
            AdvancedNote16 = new Note();
        }

        //描画する範囲の設定
        public void SetNoteForm(Rectangle NewNoteForm)
        {
            NoteFormRectangle.X = NewNoteForm.X + 20;
            NoteFormRectangle.Width = NewNoteForm.Width - 20 * 2 - 1;
            NoteFormRectangle.Y = NewNoteForm.Y;
            NoteFormRectangle.Height = NewNoteForm.Height - 20;
        }

        //現在選択されているノートを返す
        public Note PresentNote()
        {
            if (Keys == 9)
            {
                if (NoteDifficluty == NoteDifficlutyMode.BASIC)
                {
                    return BasicNote9;
                }
                else if (NoteDifficluty == NoteDifficlutyMode.NORMAL)
                {
                    return NormalNote9;
                }
                else if (NoteDifficluty == NoteDifficlutyMode.ADVANCED)
                {
                    return AdvancedNote9;
                }
            }
            else if (Keys == 16)
            {
                if (NoteDifficluty == NoteDifficlutyMode.BASIC)
                {
                    return BasicNote16;
                }
                else if (NoteDifficluty == NoteDifficlutyMode.NORMAL)
                {
                    return NormalNote16;
                }
                else if (NoteDifficluty == NoteDifficlutyMode.ADVANCED)
                {
                    return AdvancedNote16;
                }
            }
            return null;
        }

        //描画する。
        public void PaintNoteToForm(Graphics g, BPM BPM)
        {
            //すべての描写のクリア
            g.Clear(SystemColors.Control);
            //四角形の描写
            g.FillRectangle(Brushes.LightYellow, NoteFormRectangle);
            NoteFormPen.Color = Color.Black;
            g.DrawRectangle(NoteFormPen, NoteFormRectangle);
            //横軸の描写
            NoteFormPen.Color = Color.Green;
            //X座標は固定
            WaveFormPointsVerticalLine[0].X = NoteFormRectangle.X + 1;
            WaveFormPointsVerticalLine[1].X = NoteFormRectangle.Right - 1;
            //16キーのとき
            if (Keys == 16)
            {
                ParallelLine = NoteFormRectangle.Y + 18;
                ParallelLineInterval = 28;
                int TempParallelLine = ParallelLine;
                for (int i = 0; i < 16; i++)
                {
                    WaveFormPointsVerticalLine[0].Y = TempParallelLine;
                    WaveFormPointsVerticalLine[1].Y = TempParallelLine;
                    if (i / 4 == 0 || i / 4 == 2)
                    {
                        NoteFormPen.Color = Color.DarkGreen;
                    }
                    else
                    {
                        NoteFormPen.Color = Color.ForestGreen;
                    }
                    g.DrawLine(NoteFormPen, WaveFormPointsVerticalLine[0], WaveFormPointsVerticalLine[1]);
                    TempParallelLine += ParallelLineInterval;
                }
            }
            //9キーのとき
            else if (Keys == 9)
            {
                ParallelLine = NoteFormRectangle.Y + 33;
                ParallelLineInterval = 48;
                int TempParallelLine = ParallelLine;
                for (int i = 0; i < 9; i++)
                {
                    WaveFormPointsVerticalLine[0].Y = TempParallelLine;
                    WaveFormPointsVerticalLine[1].Y = TempParallelLine;
                    if (i / 3 == 0 || i / 4 == 3)
                    {
                        NoteFormPen.Color = Color.DarkGreen;
                    }
                    else
                    {
                        NoteFormPen.Color = Color.ForestGreen;
                    }
                    g.DrawLine(NoteFormPen, WaveFormPointsVerticalLine[0], WaveFormPointsVerticalLine[1]);
                    TempParallelLine += ParallelLineInterval;
                }
            }
            //BPMの横線の描写
            NoteFormPen.Color = Color.DarkOrange;
            WaveFormPointsVerticalLine[0].Y = NoteFormRectangle.Bottom - 18;
            WaveFormPointsVerticalLine[1].Y = NoteFormRectangle.Bottom - 18;
            g.DrawLine(NoteFormPen, WaveFormPointsVerticalLine[0], WaveFormPointsVerticalLine[1]);
            //Y座標は固定
            WaveFormPointsVerticalLine[0].Y = NoteFormRectangle.Y + 1;
            WaveFormPointsVerticalLine[1].Y = NoteFormRectangle.Bottom - 1;
            //縦線の描画
            String VerticalFontCountString;
            float VerticalFontPositionX;
            float VerticalFontPositionY = NoteFormRectangle.Bottom + 2;
            int WaveFormPointsVerticalLineTemp = VerticalBaseLine;
            int WaveFormPointsVerticalLineMax = NoteFormRectangle.Right;
            int WaveFormPointsVerticalLineMin = NoteFormRectangle.X;
            //描画回数
            int DrawCount = 0;
            OldNoteCount = 0;
            //ノートのだぶりの初期化
            NoteisDouble = false;
            //間隔
            int TempVerticalLineBeatInterval = VerticalLineBeatInterval / 8;
            while (true)
            {
                if (WaveFormPointsVerticalLineTemp > WaveFormPointsVerticalLineMin)
                {
                    if (WaveFormPointsVerticalLineTemp < WaveFormPointsVerticalLineMax)
                    {
                        //縦線の描画
                        if (DrawCount % 32 == 0)
                        {
                            //小節の線の色
                            NoteFormPen.Color = Color.OrangeRed;
                            //小節の縦線の描画
                            WaveFormPointsVerticalLine[0].X = WaveFormPointsVerticalLineTemp;
                            WaveFormPointsVerticalLine[1].X = WaveFormPointsVerticalLineTemp;
                            g.DrawLine(NoteFormPen, WaveFormPointsVerticalLine[0], WaveFormPointsVerticalLine[1]);
                            //小節数の文字の描画
                            VerticalFontPositionX = WaveFormPointsVerticalLineTemp - 7;
                            VerticalFontCountString = (DrawCount / 32).ToString();
                            g.DrawString(VerticalFontCountString, VerticalFont, Brushes.Black, VerticalFontPositionX, VerticalFontPositionY);
                        }
                        else if (DrawCount % 8 == 0)
                        {
                            //拍の線の色
                            NoteFormPen.Color = Color.BlueViolet;
                            //拍の縦線の描画
                            WaveFormPointsVerticalLine[0].X = WaveFormPointsVerticalLineTemp;
                            WaveFormPointsVerticalLine[1].X = WaveFormPointsVerticalLineTemp;
                            g.DrawLine(NoteFormPen, WaveFormPointsVerticalLine[0], WaveFormPointsVerticalLine[1]);
                        }
                        else if (DrawCount % VerticalSubLine == 0)
                        {
                            //補助線の縦線の描画
                            WaveFormPointsVerticalLine[0].X = WaveFormPointsVerticalLineTemp;
                            WaveFormPointsVerticalLine[1].X = WaveFormPointsVerticalLineTemp;
                            g.DrawLine(NoteFormPenDash, WaveFormPointsVerticalLine[0], WaveFormPointsVerticalLine[1]);
                        }
                        //ノートの描画の範囲
                        if (WaveFormPointsVerticalLineTemp > WaveFormPointsVerticalLineMin + 4)
                        {
                            if (WaveFormPointsVerticalLineTemp < WaveFormPointsVerticalLineMax - 4)
                            {
                                //BPMの描画の範囲
                                int TempBPMChangeCount = 0;
                                while (TempBPMChangeCount < BPM.BPMChangeList.Count)
                                {
                                    if (BPM.BPMChangeList[TempBPMChangeCount].BeatForChange == DrawCount)
                                    {
                                        //BPM変化の描画
                                        BPMChangeRectangle.X = WaveFormPointsVerticalLineTemp - 5;
                                        BPMChangeRectangle.Y = NoteFormRectangle.Bottom - 23;
                                        g.FillRectangle(Brushes.Blue, BPMChangeRectangle);
                                        //BPM変化の数字の描画
                                        VerticalFontPositionX = WaveFormPointsVerticalLineTemp + 4;
                                        VerticalFontCountString = BPM.BPMChangeList[TempBPMChangeCount].BPM.ToString();
                                        g.DrawString(VerticalFontCountString, VerticalFont, Brushes.Black, BPMChangeRectangle.X + 12, BPMChangeRectangle.Y - 1);
                                    }
                                    TempBPMChangeCount++;
                                }
                                //ノートの描画
                                //リストの描画したところまで保存して、そこから次の描画を始める。
                                int TempNoteCount = OldNoteCount;
                                //ノートの難易度にあわせて色を変化して描画
                                while (TempNoteCount < PresentNote().NoteList.Count)
                                {
                                    if (PresentNote().NoteList[TempNoteCount].Measure * 32 + PresentNote().NoteList[TempNoteCount].Beat == DrawCount)
                                    {
                                        NoteRectangle.X = WaveFormPointsVerticalLineTemp - 3;
                                        //ロングノート部分
                                        if (PresentNote().NoteList[TempNoteCount].Button < 0)
                                        {
                                            //開始位置の判別
                                            if (PresentNote().NoteList[TempNoteCount].Button / 100 < 0)
                                            {
                                                int tempButton = -PresentNote().NoteList[TempNoteCount].Button / 100;
                                                //ロングノート継続中
                                                LongNoteEnable[tempButton] = true;

                                                NoteRectangle.Y = tempButton * ParallelLineInterval + ParallelLine - 7;
                                                //ノートがだぶっているとき、ノートを塗りつぶさない
                                                if (PresentNote().NoteList[TempNoteCount].Time - oldNoteTime[tempButton] < 25 * 30
                                                    && PresentNote().NoteList[TempNoteCount].Time - oldNoteTime[tempButton] > 0)
                                                {
                                                    g.FillRectangle(Brushes.DarkBlue, NoteRectangle);
                                                    NoteisDouble = true;
                                                }
                                                else
                                                {
                                                    g.FillRectangle(Brushes.Blue, NoteRectangle);
                                                }
                                                //枠線は黒に
                                                NoteFormPen.Color = Color.Black;
                                                //選択されていたら破線に
                                                if (PresentNote().NoteList[TempNoteCount].Selected == true)
                                                {
                                                    NoteFormPen.DashStyle = DashStyle.Dot;
                                                    g.DrawRectangle(NoteFormPen, NoteRectangle.X - 1, NoteRectangle.Y - 1, NoteRectangle.Width + 1, NoteRectangle.Height + 1);
                                                    NoteFormPen.DashStyle = DashStyle.Solid;
                                                }
                                                else
                                                {
                                                    g.DrawRectangle(NoteFormPen, NoteRectangle.X - 1, NoteRectangle.Y - 1, NoteRectangle.Width + 1, NoteRectangle.Height + 1);
                                                }
                                                //描画したノートポジションを保存
                                                oldNoteTime[tempButton] = PresentNote().NoteList[TempNoteCount].Time;
                                            }
                                            //ロングノート終了
                                            else
                                            {
                                                //ロングノート終了
                                                LongNoteEnable[-PresentNote().NoteList[TempNoteCount].Button] = false;

                                                NoteRectangle.Y = -PresentNote().NoteList[TempNoteCount].Button * ParallelLineInterval + ParallelLine - 7;
                                                //ノートがだぶっているとき、ノートを塗りつぶさない
                                                if (PresentNote().NoteList[TempNoteCount].Time - oldNoteTime[-PresentNote().NoteList[TempNoteCount].Button] < 25 * 30
                                                    && PresentNote().NoteList[TempNoteCount].Time - oldNoteTime[-PresentNote().NoteList[TempNoteCount].Button] > 0)
                                                {
                                                    g.FillRectangle(Brushes.DarkBlue, NoteRectangle);
                                                    NoteisDouble = true;
                                                }
                                                else
                                                {
                                                    g.FillRectangle(Brushes.Orange, NoteRectangle);
                                                }
                                                //枠線は黒に
                                                NoteFormPen.Color = Color.Black;
                                                //選択されていたら破線に
                                                if (PresentNote().NoteList[TempNoteCount].Selected == true)
                                                {
                                                    NoteFormPen.DashStyle = DashStyle.Dot;
                                                    g.DrawRectangle(NoteFormPen, NoteRectangle.X - 1, NoteRectangle.Y - 1, NoteRectangle.Width + 1, NoteRectangle.Height + 1);
                                                    NoteFormPen.DashStyle = DashStyle.Solid;
                                                }
                                                else
                                                {
                                                    g.DrawRectangle(NoteFormPen, NoteRectangle.X - 1, NoteRectangle.Y - 1, NoteRectangle.Width + 1, NoteRectangle.Height + 1);
                                                }
                                                //描画したノートポジションを保存
                                                oldNoteTime[-PresentNote().NoteList[TempNoteCount].Button] = PresentNote().NoteList[TempNoteCount].Time;
                                            }
                                        }
                                        //通常ノート部分
                                        else
                                        {
                                            NoteRectangle.Y = PresentNote().NoteList[TempNoteCount].Button * ParallelLineInterval + ParallelLine - 7;
                                            //ノートがだぶっているとき、ノートを塗りつぶさない
                                            if (PresentNote().NoteList[TempNoteCount].Time - oldNoteTime[PresentNote().NoteList[TempNoteCount].Button] < 25 * 30
                                                && PresentNote().NoteList[TempNoteCount].Time - oldNoteTime[PresentNote().NoteList[TempNoteCount].Button] > 0)
                                            {
                                                NoteisDouble = true;
                                            }
                                            //ノートがだぶってないとき
                                            else
                                            {
                                                if (NoteDifficluty == NoteDifficlutyMode.BASIC)
                                                {
                                                    g.FillRectangle(Brushes.Green, NoteRectangle);
                                                }
                                                else if (NoteDifficluty == NoteDifficlutyMode.NORMAL)
                                                {
                                                    g.FillRectangle(Brushes.Yellow, NoteRectangle);
                                                }
                                                else if (NoteDifficluty == NoteDifficlutyMode.ADVANCED)
                                                {
                                                    g.FillRectangle(Brushes.Red, NoteRectangle);
                                                }
                                            }
                                            //枠線は黒に
                                            NoteFormPen.Color = Color.Black;
                                            //選択されていたら破線に
                                            if (PresentNote().NoteList[TempNoteCount].Selected == true)
                                            {
                                                NoteFormPen.DashStyle = DashStyle.Dot;
                                                g.DrawRectangle(NoteFormPen, NoteRectangle.X - 1, NoteRectangle.Y - 1, NoteRectangle.Width + 1, NoteRectangle.Height + 1);
                                                NoteFormPen.DashStyle = DashStyle.Solid;
                                            }
                                            else
                                            {
                                                g.DrawRectangle(NoteFormPen, NoteRectangle.X - 1, NoteRectangle.Y - 1, NoteRectangle.Width + 1, NoteRectangle.Height + 1);
                                            }
                                            //描画したノートポジションを保存
                                            oldNoteTime[PresentNote().NoteList[TempNoteCount].Button] = PresentNote().NoteList[TempNoteCount].Time;
                                        }
                                        //少し余裕を取って描画回数を保存（現在位置の左側にあるノート数）
                                        if (TempNoteCount > 100)
                                        {
                                            OldNoteCount = TempNoteCount - 100;
                                        }
                                        else
                                        {
                                            OldNoteCount = 0;
                                        }
                                    }
                                    TempNoteCount++;
                                }
                            }
                        }
                    }
                    else
                    {
                        //描画範囲を超えたときにbreak
                        break;
                    }
                }
                //(１拍/8)づつ進んでいく
                WaveFormPointsVerticalLineTemp += TempVerticalLineBeatInterval;
                DrawCount++;
            }
            //現在位置の縦線の描画（赤線）
            NoteFormPen.Color = Color.Red;
            WaveFormPointsVerticalLine[0].X = NoteFormRectangle.X + 250;
            WaveFormPointsVerticalLine[1].X = NoteFormRectangle.X + 250;
            g.DrawLine(NoteFormPen, WaveFormPointsVerticalLine[0], WaveFormPointsVerticalLine[1]);
            //横線のボタン数の描画
            int ParallelFontCount;
            float ParallelFontPositionX;
            float ParallelFontPositionY;
            String ParallelFontCountString;
            //16キーのとき
            if (Keys == 16)
            {
                ParallelFontPositionX = 14;
                ParallelFontPositionY = NoteFormRectangle.Y + 12;
                ParallelFontCount = 1;
                for (int i = 1; i <= 9; i++)
                {
                    ParallelFontCountString = ParallelFontCount.ToString();
                    g.DrawString(ParallelFontCountString, ParallelFont, Brushes.Black, ParallelFontPositionX, ParallelFontPositionY);
                    ParallelFontPositionY += ParallelLineInterval;
                    ParallelFontCount++;
                }
                ParallelFontPositionX = 6;
                for (int i = 10; i <= 16; i++)
                {
                    ParallelFontCountString = ParallelFontCount.ToString();
                    g.DrawString(ParallelFontCountString, ParallelFont, Brushes.Black, ParallelFontPositionX, ParallelFontPositionY);
                    ParallelFontPositionY += ParallelLineInterval;
                    ParallelFontCount++;
                }
                ParallelFontPositionX = NoteFormRectangle.Right + 2;
                ParallelFontPositionY = NoteFormRectangle.Y + 12;
                ParallelFontCount = 1;
                for (int i = 1; i <= 16; i++)
                {
                    ParallelFontCountString = ParallelFontCount.ToString();
                    g.DrawString(ParallelFontCountString, ParallelFont, Brushes.Black, ParallelFontPositionX, ParallelFontPositionY);
                    ParallelFontPositionY += ParallelLineInterval;
                    ParallelFontCount++;
                }
            }
            //9キーのとき
            else if (Keys == 9)
            {
                ParallelFontPositionX = 14;
                ParallelFontPositionY = NoteFormRectangle.Y + 27;
                ParallelFontCount = 1;
                for (int i = 1; i <= 9; i++)
                {
                    ParallelFontCountString = ParallelFontCount.ToString();
                    g.DrawString(ParallelFontCountString, ParallelFont, Brushes.Black, ParallelFontPositionX, ParallelFontPositionY);
                    ParallelFontPositionY += ParallelLineInterval;
                    ParallelFontCount++;
                }
                ParallelFontPositionX = NoteFormRectangle.Right + 2;
                ParallelFontPositionY = NoteFormRectangle.Y + 12;
                ParallelFontCount = 1;
                for (int i = 1; i <= 9; i++)
                {
                    ParallelFontCountString = ParallelFontCount.ToString();
                    g.DrawString(ParallelFontCountString, ParallelFont, Brushes.Black, ParallelFontPositionX, ParallelFontPositionY);
                    ParallelFontPositionY += ParallelLineInterval;
                    ParallelFontCount++;
                }
            }
            //BPMの文字の描写
            g.DrawString("BPM", ParallelFont, Brushes.Black, 6, NoteFormRectangle.Y + 453, ParallelFontFormat);
            g.DrawString("BPM", ParallelFont, Brushes.Black, NoteFormRectangle.Right + 2, NoteFormRectangle.Y + 453, ParallelFontFormat);

            //範囲選択の描画
            if (isSelectRectangle == true)
            {
                NoteFormPenDash.Color = Color.Black;
                g.DrawRectangle(NoteFormPenDash, SelectRectangle);
                NoteFormPenDash.Color = Color.YellowGreen;
            }
        }

        //描画する位置を動かす
        public void MoveBaseLine(int PresentMusicTime, BPM BPM)
        {
            //BPMに対応するための処理
            double TempTime = 0;
            if (BPM.BPMChangeList.Count == 0)
            {
                TempTime = PresentMusicTime * (BPM.BaseBPM / 60.0);
            }
            else
            {
                int ListCount=0;
                for (ListCount = 0; ListCount < BPM.BPMChangeList.Count; ListCount++)
                {
                    if (BPM.BPMChangeList[ListCount].TimeForChange < PresentMusicTime)
                    {
                        if (ListCount == 0)
                        {
                            TempTime += (BPM.BPMChangeList[0].TimeForChange) * (BPM.BaseBPM / 60.0);
                        }
                        else
                        {
                            TempTime += (BPM.BPMChangeList[ListCount].TimeForChange - BPM.BPMChangeList[ListCount - 1].TimeForChange) * (BPM.BPMChangeList[ListCount - 1].BPM / 60.0);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (ListCount == 0)
                {
                    TempTime = PresentMusicTime * (BPM.BaseBPM / 60.0);
                }
                else
                {
                    TempTime += (PresentMusicTime - BPM.BPMChangeList[ListCount - 1].TimeForChange) * (BPM.BPMChangeList[ListCount - 1].BPM / 60.0);
                }
            }
            //位置の移動。
            //1小節1000ミリ秒で320進める
            double tempbeat = -TempTime * (VerticalLineBeatBaseInterval / 1000.0) * ((double)VerticalLineBeatInterval / VerticalLineBeatBaseInterval);
            VerticalBaseLine = (int)tempbeat + NoteFormRectangle.X + 250;
        }

        //左クリック時の判定。範囲外だとnullを返す
        public CheckedNote NoteFormClick(int X, int Y)
        {
            CheckedNote TempClickedItem = new CheckedNote();
            //x座標について。
            //小節数を求める
            double TempBeat = Math.Round((double)(X - VerticalBaseLine) / ((double)VerticalLineBeatInterval / 8.0));
            if (TempBeat < 0)
            {
                return null;
            }
            TempClickedItem.Measure = (int)TempBeat / 32;
            TempClickedItem.Beat = (int)TempBeat % 32;
            //縦軸。ボタン、BPMについて。
            double TempVertialPosition = ((double)Y - (ParallelLine)) / ParallelLineInterval;
            int TempButton = (int)Math.Round(TempVertialPosition);
            //1~16ボタンのとき
            if (TempButton >= 0 && Keys > TempButton)
            {
                TempClickedItem.Button = TempButton;
            }
            //BPMが選択されたとき
            else if (Y < NoteFormRectangle.Bottom && Y > NoteFormRectangle.Bottom - 36)
            {
                TempClickedItem.BPM = true;
            }
            else
            {
                return null;
            }
            return TempClickedItem;
        }
         
        //現在の小節数、拍数
        public CheckedNote NoteFormPresentBeat()
        {
            CheckedNote TempClickedItem = new CheckedNote();
            //x座標について。
            //小節数を求める
            double TempBeat = Math.Round(-(VerticalBaseLine - 250-NoteFormRectangle.X) / (VerticalLineBeatInterval / 8.0));
            TempClickedItem.Measure = (int)TempBeat / 32;
            TempClickedItem.Beat = (int)TempBeat % 32;
            return TempClickedItem;
        }

    }
}