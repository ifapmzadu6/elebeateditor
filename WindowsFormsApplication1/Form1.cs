using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;


namespace ELEBEATMusicEditer
{
    public partial class Form1 : Form
    {
        //mciSendStringを使うためのおまじない
        [DllImport("winmm.dll")]
        extern static int mciSendString(string s1, StringBuilder s2, int i1, int i2);

        //スレッド用
        Thread trd;
        volatile bool ThreadStop;

        Form4 PreviewForm = new Form4();

        //譜面ファイルのパス
        string EleFilePath;

        //描画中か
        bool isPainting;
        //描画カウント
        int DrawCount;
        //イベントハンドラ実行中でないか
        bool isProcessing;

        //クラスの宣言
        private Note Note = new Note();
        public NotePaint NotePaint = new NotePaint();
        private MusicPlayer MusicPlayer = new MusicPlayer();
        private MusicInfo MusicInfo = new MusicInfo();
        private BPM BPM = new BPM();

        //オフセット。NotePaintに移したい
        private int NoteOffsetTime;
        //コンテキストメニューで使う
        int tempBPMBeat;

        //四角形の位置と大きさの設定
        static private Rectangle NoteForm = new Rectangle(5, 35, 980, 505);

        //マウスの最初にクリックした位置
        Point MouseDownPosition = new Point();
        Point TempMousePosition = new Point();
        //マウスのノートのドラッグアンドドロップ
        bool isMouseDrag;
        //コピーのリスト
        Note CopyedNote = new Note();
        //コピーしたときのボタン数
        int CopyedKeys;

        //コンストラクタ
        public Form1()
        {
            InitializeComponent();
            NotePaint.SetNoteForm(NoteForm);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //スレッドの作成
            trd = new Thread(new ThreadStart(this.MyPaint));
            trd.IsBackground = true;
            trd.Priority = ThreadPriority.Highest;
            trd.Start();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //try
            //{
                isPainting = true;
                if (MusicPlayer.FetchMusicFlag == true)
                {
                    //////再生時間と速度の更新
                    MusicPlayer.GetPresentTime();
                    MusicPlayer.GetPlayingMusicMode();
                    trackBar1.Value = (int)((double)MusicPlayer.PresentMusicTime / MusicPlayer.TotalMusicTime * 500.0);
                }
                if (DrawCount > 20)
                {
                    if (NotePaint.NoteisDouble == true)
                    {
                        toolStripStatusLabel1.Text = "ノートの間隔が狭すぎる場所があります。";
                    }
                    else
                    {
                        if (MusicPlayer.FetchMusicFlag == true)
                        {
                            //音楽が止まっていたら再生ボタンを再生に
                            if (MusicPlayer.PlayingMusicMode == "paused" || MusicPlayer.PlayingMusicMode == "stopped")
                            {
                                button1.Text = "再生";
                            }
                            toolStripStatusLabel1.Text = "";
                            toolStripStatusLabel3.Text = "　再生時間 : " + MusicPlayer.GetPresentDateTime().ToLongTimeString();
                            toolStripStatusLabel4.Text = "　再生速度 : " + (MusicPlayer.PlayMusicSpeed / 10).ToString() + "%";
                        }
                        else
                        {
                            toolStripStatusLabel1.Text = "譜面ファイルまたは音楽ファイルを読み込んでください。";
                        }
                    }
                    DrawCount = 0;
                }
                //描写の位置の変更
                NotePaint.MoveBaseLine((MusicPlayer.PresentMusicTime + NoteOffsetTime), BPM);
                NotePaint.PaintNoteToForm(e.Graphics, BPM);
                //プレビュー画面があればプレビュー画面の表示
                if (PreviewForm.Created == true)
                {
                    PreviewForm.PresentTime = MusicPlayer.PresentMusicTime;
                    PreviewForm.PreviewNote = NotePaint.PresentNote();
                    PreviewForm.PreviewKeys = NotePaint.Keys;
                    PreviewForm.NoteOffsetTime = NoteOffsetTime;
                    PreviewForm.Invalidate();
                }
                DrawCount++;
                isPainting = false;
            //}
            //catch
            //{
            //}
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ThreadStop = true;
            MusicPlayer.FetchMusicFlag = false;
            MusicPlayer.CloseMusicPlayer();
        }
        //画面の更新(60fps)
        public void MyPaint()
        {
            double nextframe = (double)System.Environment.TickCount;
            float wait = 1000f / 60f;
            while (!ThreadStop)
            {
                if (System.Environment.TickCount >= nextframe)
                {
                    if (isPainting==false)
                    {
                        this.Invalidate();
                    }
                    nextframe += wait;
                }
                Thread.Sleep(1);
            }
        }

        //再生ボタン
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (MusicPlayer.FetchMusicFlag == true)
            {
                string TempMusicMode;
                TempMusicMode = MusicPlayer.GetPlayingMusicMode();
                if (TempMusicMode == "playing")
                {
                    MusicPlayer.StopMusic();
                    button1.Text = "再生";
                }
                else
                {
                    MusicPlayer.PlayMusic();
                    button1.Text = "一時停止";
                }
            }
        }
        //1泊進むボタン
        private void button3_Click(object sender, EventArgs e)
        {
            MusicPlayer.GetPresentTime();
            MusicPlayer.GoBeat((int)BPM.GetBPM(MusicPlayer.PresentMusicTime));
        }
        //1小節進むボタン
        private void button2_Click(object sender, EventArgs e)
        {
            MusicPlayer.GetPresentTime();
            MusicPlayer.GoMeasure((int)BPM.GetBPM(MusicPlayer.PresentMusicTime));
        }
        //1拍戻るボタン
        private void button4_Click(object sender, EventArgs e)
        {
            MusicPlayer.GetPresentTime();
            MusicPlayer.BackBeat((int)BPM.GetBPM(MusicPlayer.PresentMusicTime));
        }
        //1小節戻るボタン
        private void button5_Click(object sender, EventArgs e)
        {
            MusicPlayer.GetPresentTime();
            MusicPlayer.BackMeasure((int)BPM.GetBPM(MusicPlayer.PresentMusicTime));
        }

        //オフセットのテキストボックス
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int i;
            try
            {
                i = int.Parse(textBox1.Text);
            }
            catch
            {
                return;
            }
            if (i < MusicPlayer.TotalMusicTime)
            {
                NoteOffsetTime = i;
            }
            CalcNoteTime(NotePaint.PresentNote());
        }
        //Enterキー入力でフォーカス移動
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && textBox1.Text != "")
            {
                e.SuppressKeyPress = true;
                this.ActiveControl = null;
            }
        }
        //BPMのテキストボックス
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            double i;
            try
            {
                i = Double.Parse(textBox2.Text);
            }
            catch
            {
                return;
            }
            if (i > 0 && i < 5000)
            {
                BPM.BaseBPM = i;
            }
            CalcNoteTime(NotePaint.PresentNote());
        }
        //Enterキー入力でフォーカス移動
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && textBox2.Text != "")
            {
                e.SuppressKeyPress = true;
                this.ActiveControl = null;
            }
        }
        //再生時間トラックバーがスクロールされたとき、再生時間変更
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (MusicPlayer.FetchMusicFlag == true)
            {
                int TrackValue = (int)((double)trackBar1.Value * MusicPlayer.TotalMusicTime / 500.0);
                MusicPlayer.SeekPresentMusicTime(TrackValue);
            }
        }
        //拡大率トラックバー
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            NotePaint.VerticalLineBeatInterval = 320 / 4 + (5 - trackBar2.Value) * 8;
        }
        //再生速度トラックバー
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            int TempSoundSpeed = 1000 + (trackBar3.Value - 5) * 100;
            MusicPlayer.ChangeMusicSpeed(TempSoundSpeed);
        }

        //キーボードを押すとき
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBox1.Focused == false && textBox2.Focused == false)
            {
                int ButtonNumber = 0;
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.X: 切り取り(); break;
                        case Keys.C: コピー(); break;
                        case Keys.V: 貼り付け(); break;
                    }
                }
                else
                {
                    if (NotePaint.Keys == 16)
                    {
                        if (e.KeyCode == Keys.D1) ButtonNumber = 1;
                        if (e.KeyCode == Keys.D2) ButtonNumber = 2;
                        if (e.KeyCode == Keys.D3) ButtonNumber = 3;
                        if (e.KeyCode == Keys.D4) ButtonNumber = 4;
                        if (e.KeyCode == Keys.Q) ButtonNumber = 5;
                        if (e.KeyCode == Keys.W) ButtonNumber = 6;
                        if (e.KeyCode == Keys.E) ButtonNumber = 7;
                        if (e.KeyCode == Keys.R) ButtonNumber = 8;
                        if (e.KeyCode == Keys.A) ButtonNumber = 9;
                        if (e.KeyCode == Keys.S) ButtonNumber = 10;
                        if (e.KeyCode == Keys.D) ButtonNumber = 11;
                        if (e.KeyCode == Keys.F) ButtonNumber = 12;
                        if (e.KeyCode == Keys.Z) ButtonNumber = 13;
                        if (e.KeyCode == Keys.X) ButtonNumber = 14;
                        if (e.KeyCode == Keys.C) ButtonNumber = 15;
                        if (e.KeyCode == Keys.V) ButtonNumber = 16;
                        if (ButtonNumber != 0)
                        {
                            Note.NotePosition NewNotePosition = new Note.NotePosition();
                            NewNotePosition.Measure = NotePaint.NoteFormPresentBeat().Measure;
                            NewNotePosition.Beat = NotePaint.NoteFormPresentBeat().Beat;
                            NewNotePosition.Button = ButtonNumber - 1;
                            NewNotePosition.Selected = true;
                            NotePaint.PresentNote().UnSelectAllnote();
                            NotePaint.PresentNote().AddNewNotePosition(NewNotePosition);
                            CalcNoteTime(NotePaint.PresentNote());
                        }
                    }
                    else if (NotePaint.Keys == 9)
                    {
                        if (e.KeyCode == Keys.W) ButtonNumber = 1;
                        if (e.KeyCode == Keys.E) ButtonNumber = 2;
                        if (e.KeyCode == Keys.R) ButtonNumber = 3;
                        if (e.KeyCode == Keys.S) ButtonNumber = 4;
                        if (e.KeyCode == Keys.D) ButtonNumber = 5;
                        if (e.KeyCode == Keys.F) ButtonNumber = 6;
                        if (e.KeyCode == Keys.X) ButtonNumber = 7;
                        if (e.KeyCode == Keys.C) ButtonNumber = 8;
                        if (e.KeyCode == Keys.V) ButtonNumber = 9;
                        if (e.KeyCode == Keys.NumPad1) ButtonNumber = 7;
                        if (e.KeyCode == Keys.NumPad2) ButtonNumber = 8;
                        if (e.KeyCode == Keys.NumPad3) ButtonNumber = 9;
                        if (e.KeyCode == Keys.NumPad4) ButtonNumber = 4;
                        if (e.KeyCode == Keys.NumPad5) ButtonNumber = 5;
                        if (e.KeyCode == Keys.NumPad6) ButtonNumber = 6;
                        if (e.KeyCode == Keys.NumPad7) ButtonNumber = 1;
                        if (e.KeyCode == Keys.NumPad8) ButtonNumber = 2;
                        if (e.KeyCode == Keys.NumPad9) ButtonNumber = 3;
                        if (ButtonNumber != 0)
                        {
                            Note.NotePosition NewNotePosition = new Note.NotePosition();
                            NewNotePosition.Measure = NotePaint.NoteFormPresentBeat().Measure;
                            NewNotePosition.Beat = NotePaint.NoteFormPresentBeat().Beat;
                            NewNotePosition.Button = ButtonNumber - 1;
                            NewNotePosition.Selected = true;
                            NotePaint.PresentNote().UnSelectAllnote();
                            NotePaint.PresentNote().AddNewNotePosition(NewNotePosition);
                            CalcNoteTime(NotePaint.PresentNote());
                        }
                    }
                    if (e.KeyCode == Keys.Delete)
                    {
                        NotePaint.PresentNote().RemoveSelectedNote();
                    }
                }
            }
        }
        //矢印キーの処理。輸入
        [System.Security.Permissions.UIPermission(
    System.Security.Permissions.SecurityAction.LinkDemand,
    Window = System.Security.Permissions.UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            //左キーが押されているか調べる
            if ((keyData & Keys.KeyCode) == Keys.Left)
            {
                //左キーの本来の処理（左側のコントロールにフォーカスを移す）をさせたくないときは、trueを返す
                if (NotePaint.Keys == 9)
                {
                    NotePaint.PresentNote().MoveSelectedNote(-1, 0, 9);
                }
                else
                {
                    NotePaint.PresentNote().MoveSelectedNote(-1, 0, 16);
                }
                return true;
            }
            if ((keyData & Keys.KeyCode) == Keys.Right)
            {
                //左キーの本来の処理（左側のコントロールにフォーカスを移す）をさせたくないときは、trueを返す
                if (NotePaint.Keys == 9)
                {
                    NotePaint.PresentNote().MoveSelectedNote(1, 0, 9);
                }
                else
                {
                    NotePaint.PresentNote().MoveSelectedNote(1, 0, 16);
                }
                return true;
            }
            if ((keyData & Keys.KeyCode) == Keys.Up)
            {
                //左キーの本来の処理（左側のコントロールにフォーカスを移す）をさせたくないときは、trueを返す
                if (NotePaint.Keys == 9)
                {
                    NotePaint.PresentNote().MoveSelectedNote(0, -1, 9);
                }
                else
                {
                    NotePaint.PresentNote().MoveSelectedNote(0, -1, 16);
                }
                return true;
            }
            if ((keyData & Keys.KeyCode) == Keys.Down)
            {
                //左キーの本来の処理（左側のコントロールにフォーカスを移す）をさせたくないときは、trueを返す
                if (NotePaint.Keys == 9)
                {
                    NotePaint.PresentNote().MoveSelectedNote(0, 1, 9);
                }
                else
                {
                    NotePaint.PresentNote().MoveSelectedNote(0, 1, 16);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        //マウスのボタンを押すとき
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //押した位置を保存する
            MouseDownPosition = e.Location;
            TempMousePosition.X = 0;
            TempMousePosition.Y = 0;
            NotePaint.CheckedNote TempCheckNote;
            TempCheckNote = NotePaint.NoteFormClick(e.X, e.Y);
            //クリックした場所がおかしくないとき
            if (TempCheckNote != null)
            {
                Note.NotePosition TempNotePosition = new Note.NotePosition();
                TempNotePosition.Measure = TempCheckNote.Measure;
                TempNotePosition.Beat = TempCheckNote.Beat;
                TempNotePosition.Button = TempCheckNote.Button;
                Note.NoteComparer Compare = new Note.NoteComparer();
                NotePaint.PresentNote().NoteList.Sort(Compare);
                int Searched = NotePaint.PresentNote().NoteList.BinarySearch(TempNotePosition, Compare);
                //ノートがクリックした位置にないとき
                if (Searched < 0)
                {
                    NotePaint.isSelectRectangle = true;
                    NotePaint.SelectRectangle.X = e.Location.X;
                    NotePaint.SelectRectangle.Y = e.Location.Y;
                    NotePaint.FirstCheckedNote = TempCheckNote;
                    isMouseDrag = false;
                }
                //ノートがクリックした位置にあるが、そのノートが選択されているとき
                else if (NotePaint.PresentNote().NoteList[Searched].Selected == true)
                {
                    NotePaint.isSelectRectangle = false;
                    isMouseDrag = true;
                }
                //ノートがクリックした位置にあるが、そのノートが選択されているとき
                else
                {
                    NotePaint.isSelectRectangle = false;
                    isMouseDrag = false;
                }
            }
        }
        //マウスが動いたとき
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            //左クリック
            if (e.Button == MouseButtons.Left)
            {
                //ドラッグで移動の動作のとき
                if (isMouseDrag == true)
                {
                    //選択されたノートを動かす
                    NotePaint.PresentNote().MoveSelectedNote((e.X - MouseDownPosition.X) / (NotePaint.VerticalLineBeatInterval / 8) - TempMousePosition.X, 0,NotePaint.Keys);
                    TempMousePosition.X = (e.X - MouseDownPosition.X) / (NotePaint.VerticalLineBeatInterval / 8);
                    NotePaint.PresentNote().MoveSelectedNote(0, (e.Y - MouseDownPosition.Y) / NotePaint.ParallelLineInterval - TempMousePosition.Y,NotePaint.Keys);
                    TempMousePosition.Y = (e.Y - MouseDownPosition.Y) / NotePaint.ParallelLineInterval;
                }
                //範囲選択の時
                if (NotePaint.isSelectRectangle == true)
                {
                    NotePaint.CheckedNote tempCheckedNote = NotePaint.NoteFormClick(e.Location.X, e.Location.Y);
                    if (tempCheckedNote != null && tempCheckedNote.BPM == false)
                    {
                        if (MouseDownPosition.X > e.Location.X)
                        {
                            NotePaint.SelectRectangle.Width = MouseDownPosition.X - e.Location.X;
                            NotePaint.SelectRectangle.X = e.Location.X;
                        }
                        else
                        {
                            NotePaint.SelectRectangle.Width = e.Location.X - MouseDownPosition.X;
                        }
                        if (MouseDownPosition.Y > e.Location.Y)
                        {
                            NotePaint.SelectRectangle.Height = MouseDownPosition.Y - e.Location.Y;
                            NotePaint.SelectRectangle.Y = e.Location.Y;
                        }
                        else
                        {
                            NotePaint.SelectRectangle.Height = e.Location.Y - MouseDownPosition.Y;
                        }
                        NotePaint.LastCheckedNote = tempCheckedNote;
                    }
                }
            }
        }
        //マウスを離したとき
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Location != MouseDownPosition)
            {
                if (e.Button == MouseButtons.Left)
                {
                    //範囲選択されていたら
                    if (NotePaint.isSelectRectangle == true)
                    {
                        Note.NotePosition FirstNotePosition = new Note.NotePosition();
                        Note.NotePosition LastNotePosition = new Note.NotePosition();
                        FirstNotePosition.Measure = NotePaint.FirstCheckedNote.Measure;
                        FirstNotePosition.Beat = NotePaint.FirstCheckedNote.Beat;
                        FirstNotePosition.Button = NotePaint.FirstCheckedNote.Button;
                        LastNotePosition.Measure = NotePaint.LastCheckedNote.Measure;
                        LastNotePosition.Beat = NotePaint.LastCheckedNote.Beat;
                        LastNotePosition.Button = NotePaint.LastCheckedNote.Button;
                        //範囲選択が逆転してたら入れ替える
                        if (MouseDownPosition.X > e.Location.X)
                        {
                            Note.NotePosition tempNotePosition;
                            tempNotePosition = FirstNotePosition;
                            FirstNotePosition = LastNotePosition;
                            LastNotePosition = tempNotePosition;
                        }
                        NotePaint.PresentNote().SelectRectangle(FirstNotePosition, LastNotePosition);
                        NotePaint.SelectRectangle.Width = 0;
                        NotePaint.SelectRectangle.Height = 0;
                        NotePaint.isSelectRectangle = false;
                    }
                    if (isMouseDrag == true)
                    {
                        isMouseDrag = false;
                    }
                }
            }
            CalcNoteTime(NotePaint.PresentNote());
        }
        //マウスのクリック
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            this.ActiveControl = null;
            //左クリック
            if (e.Button == MouseButtons.Left)
            {
                if (e.Location == MouseDownPosition)
                {
                    NotePaint.CheckedNote TempCheckNote;
                    TempCheckNote = NotePaint.NoteFormClick(e.X, e.Y);
                    if (TempCheckNote == null)
                    {
                        NotePaint.PresentNote().UnSelectAllnote();
                        return;
                    }
                    else
                    {
                        //BPMが選択されたとき
                        if (TempCheckNote.BPM == true)
                        {
                            Form3 BPMForm = new Form3();
                            BPM.BPMChange TempBPMChange = new BPM.BPMChange();
                            TempBPMChange.BeatForChange = TempCheckNote.Measure * 32 + TempCheckNote.Beat;
                            BPM.BPMChangeComparer comparer = new BPM.BPMChangeComparer();
                            BPM.BPMChangeList.Sort(comparer);
                            int Searched = BPM.BPMChangeList.BinarySearch(TempBPMChange, comparer);
                            if (Searched < 0)
                            {
                                BPMForm.TextBoxOldBPM = 60;
                                if (BPMForm.ShowDialog() == DialogResult.OK)
                                {
                                    TempBPMChange.BPM = BPMForm.NewBPM;
                                    TempBPMChange.TimeForChange = 999999;
                                    BPM.BPMChangeList.Add(TempBPMChange);
                                }
                            }
                            else
                            {
                                BPMForm.TextBoxOldBPM = BPM.BPMChangeList[Searched].BPM;
                                if (BPMForm.ShowDialog() == DialogResult.OK)
                                {
                                    BPM.BPMChangeList[Searched].BPM = BPMForm.NewBPM;
                                }
                            }
                            BPM.BPMChangeList.Sort(comparer);
                            BPM.CalcTimeFromBeat();
                            CalcNoteTime(NotePaint.PresentNote());
                        }
                        //ノートの位置が選択されたとき
                        else
                        {
                            Note.NotePosition TempNotePosition = new Note.NotePosition();
                            int tempBeat = TempCheckNote.Measure * 32 + TempCheckNote.Beat;
                            TempNotePosition.Measure = tempBeat / 32;
                            TempNotePosition.Beat = tempBeat % 32;
                            TempNotePosition.Button = TempCheckNote.Button;
                            Note.NoteComparer Compare = new Note.NoteComparer();
                            NotePaint.PresentNote().NoteList.Sort(Compare);
                            int Searched = NotePaint.PresentNote().NoteList.BinarySearch(TempNotePosition, Compare);
                            if (Searched < 0)
                            {
                                if (NotePaint.PresentNote().NoteSelected == false)
                                {
                                    NotePaint.PresentNote().UnSelectAllnote();
                                    TempNotePosition.Selected = true;
                                    NotePaint.PresentNote().AddNewNotePosition(TempNotePosition);
                                    CalcNoteTime(NotePaint.PresentNote());
                                    //NotePaint.PresentNote().NoteSelected = true;
                                }
                                else
                                {
                                    NotePaint.PresentNote().UnSelectAllnote();
                                }
                            }
                            else
                            {
                                NotePaint.PresentNote().UnSelectAllnote();
                                NotePaint.PresentNote().NoteList[Searched].Selected = true;
                                NotePaint.PresentNote().NoteSelected = true;
                            }
                        }
                    }
                }
            }
            //右クリック
            else if (e.Button == MouseButtons.Right)
            {
                NotePaint.CheckedNote TempCheckNote;
                TempCheckNote = NotePaint.NoteFormClick(e.Location.X, e.Location.Y);
                if (TempCheckNote == null)
                {
                    return;
                }
                else
                {
                    //BPMが選択されたとき
                    if (TempCheckNote.BPM == true)
                    {
                        BPM.BPMChange TempBPMChange = new BPM.BPMChange();
                        TempBPMChange.BeatForChange = TempCheckNote.Measure * 32 + TempCheckNote.Beat;
                        BPM.BPMChangeComparer comparer = new BPM.BPMChangeComparer();
                        BPM.BPMChangeList.Sort(comparer);
                        int Searched = BPM.BPMChangeList.BinarySearch(TempBPMChange, comparer);
                        if (Searched >= 0)
                        {
                            tempBPMBeat = Searched;
                            コピーToolStripMenuItem1.Visible = false;
                            コピーCToolStripMenuItem.Visible = false;
                            貼り付けPToolStripMenuItem.Visible = false;
                            削除DToolStripMenuItem.Visible = false;
                            toolStripSeparator1.Visible = false;
                            toolStripSeparator9.Visible = false;
                            すべて選択ToolStripMenuItem1.Visible = false;
                            選択解除XToolStripMenuItem.Visible = false;
                            ノートの反転ToolStripMenuItem.Visible = false;
                            bPM削除DToolStripMenuItem.Visible = true;
                        }
                    }
                    contextMenuStrip1.Show(MousePosition);
                }
            }
        }

        //コンテキストメニュー・すべて選択する
        private void すべて選択ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            すべて選択();
        }
        //コンテキストメニュー・選択解除
        private void 選択解除XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            選択解除();
        }
        //コンテキストメニュー・削除する
        private void 削除DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            削除();
        }
        //コンテキストメニュー・BPMの削除
        private void bPMの削除DToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ノートの選択解除
            NotePaint.PresentNote().UnSelectAllnote();
            if (tempBPMBeat >= 0)
            {
                BPM.BPMChangeList.RemoveAt(tempBPMBeat);
            }
            BPM.CalcTimeFromBeat();
            CalcNoteTime(NotePaint.PresentNote());
        }
        //コンテキストメニューが閉じたとき、表示する項目を初期状態に
        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            コピーToolStripMenuItem1.Visible = true;
            コピーCToolStripMenuItem.Visible = true;
            貼り付けPToolStripMenuItem.Visible = true;
            削除DToolStripMenuItem.Visible = true;
            toolStripSeparator1.Visible = true;
            toolStripSeparator9.Visible = true;
            すべて選択ToolStripMenuItem1.Visible = true;
            選択解除XToolStripMenuItem.Visible = true;
            bPM削除DToolStripMenuItem.Visible = false;
            ノートの反転ToolStripMenuItem.Visible = true;
        }
        //コンテキストメニュー・コピー
        private void コピーCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            コピー();
        }
        //コンテキストメニュー・貼り付け
        private void 貼り付けPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            貼り付け();
        }
        //コンテキストメニュー・切り取り
        private void コピーToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            切り取り();
        }
        //コンテキストメニュー・ノートの反転
        private void ノートの反転ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ノートの反転();
        }
        //コンテキストメニュー・ロングノートの切り替え
        private void ロングノートの切り替えToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ロングノートの切り替え();
        }
        //ノートをすべて選択する
        void すべて選択()
        {
            NotePaint.PresentNote().SelectAllnote();
        }
        //ノートの選択解除
        void 選択解除()
        {
            NotePaint.PresentNote().UnSelectAllnote();
        }
        //ノートの削除
        void 削除()
        {
            NotePaint.PresentNote().RemoveSelectedNote();
        }
        //ノートのコピー
        void コピー()
        {

            //すでにコピーされていたらそれを削除する
            if (CopyedNote.NoteList.Count() > 0)
            {
                CopyedNote.NoteList.Clear();
            }
            for (int i = 0; i < NotePaint.PresentNote().NoteList.Count(); i++)
            {
                if (NotePaint.PresentNote().NoteList[i].Selected == true)
                {
                    Note.NotePosition NewNotePosition = new Note.NotePosition();
                    NewNotePosition.Measure = NotePaint.PresentNote().NoteList[i].Measure;
                    NewNotePosition.Beat = NotePaint.PresentNote().NoteList[i].Beat;
                    NewNotePosition.Button = NotePaint.PresentNote().NoteList[i].Button;
                    CopyedNote.AddNewNotePosition(NewNotePosition);
                    CopyedKeys = NotePaint.Keys;
                }
            }
        }
        //ノートの貼り付け
        void 貼り付け()
        {
            //保存したキー数と同じなら処理する
            if (NotePaint.Keys == CopyedKeys)
            {
                int tempBeat = NotePaint.NoteFormPresentBeat().Measure * 32 + NotePaint.NoteFormPresentBeat().Beat;
                //コピーされていたら
                if (CopyedNote.NoteList.Count > 0)
                {
                    //すべて選択解除
                    NotePaint.PresentNote().UnSelectAllnote();
                    //コピーリストの先頭との差を求める
                    tempBeat -= CopyedNote.NoteList[0].Measure * 32 + CopyedNote.NoteList[0].Beat;
                }
                //現在位置からコピーリストを追加していく
                for (int i = 0; i < CopyedNote.NoteList.Count(); i++)
                {
                    Note.NotePosition tempNotePosition = new Note.NotePosition();
                    tempNotePosition.Measure = CopyedNote.NoteList[i].Measure + tempBeat / 32;
                    tempNotePosition.Beat = CopyedNote.NoteList[i].Beat + tempBeat % 32;
                    tempNotePosition.Button = CopyedNote.NoteList[i].Button;
                    tempNotePosition.Selected = true;
                    NotePaint.PresentNote().AddNewNotePosition(tempNotePosition);
                }
                Note.NoteComparer tempComparer = new Note.NoteComparer();
                NotePaint.PresentNote().NoteList.Sort(tempComparer);
                NotePaint.PresentNote().NoteSelected = true;
                CalcNoteTime(NotePaint.PresentNote());
            }
            //保存したキー数と違うときはエラーメッセージの表示
            else
            {
                MessageBox.Show("貼り付け先のキー数が違うため、コピー出来ません。");
            }
        }
        //ノートの切り取り
        void 切り取り()
        {
            //すでにコピーされていたらそれを削除する
            if (CopyedNote.NoteList.Count() > 0)
            {
                CopyedNote.NoteList.Clear();
            }
            for (int i = 0; i < NotePaint.PresentNote().NoteList.Count(); i++)
            {
                if (NotePaint.PresentNote().NoteList[i].Selected == true)
                {
                    Note.NotePosition NewNotePosition = new Note.NotePosition();
                    NewNotePosition.Measure = NotePaint.PresentNote().NoteList[i].Measure;
                    NewNotePosition.Beat = NotePaint.PresentNote().NoteList[i].Beat;
                    NewNotePosition.Button = NotePaint.PresentNote().NoteList[i].Button;
                    CopyedNote.AddNewNotePosition(NewNotePosition);
                    //削除する
                    NotePaint.PresentNote().NoteList.RemoveAt(i);
                    i--;
                    CopyedKeys = NotePaint.Keys;
                }
            }
        }
        //ノートの反転
        void ノートの反転()
        {
            for (int i = 0; i < NotePaint.PresentNote().NoteList.Count(); i++)
            {
                if (NotePaint.PresentNote().NoteList[i].Selected == true)
                {
                    if (NotePaint.Keys == 16)
                    {
                        NotePaint.PresentNote().NoteList[i].Button = 15 - NotePaint.PresentNote().NoteList[i].Button;
                    }
                    else
                    {
                        NotePaint.PresentNote().NoteList[i].Button = 8 - NotePaint.PresentNote().NoteList[i].Button;
                    }
                }
            }
        }
        //ロングノートの切り替え
        void ロングノートの切り替え()
        {

        }

        //譜面ファイルを新規作成
        private void 譜面ファイルを新規作成ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //新規作成可能か
            bool isEnable = false;
            //新規作成の確認
            DialogResult result = MessageBox.Show("編集中の情報はすべて破棄されます。\n続行しますか？", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.OK)
            {
                isEnable = true;
            }
            //新規作成する
            if (isEnable == true)
            {
                EleFilePath = string.Empty;
                NotePaint.NotepaintReset();
                MusicPlayer.MusicPlayerReset();
                BPM.BPMReset();
                MusicInfo = new MusicInfo();
                toolStripStatusLabel1.Text = "譜面ファイルまたは音楽ファイルを読み込んでください。";
                toolStripStatusLabel2.Text = "";
                toolStripStatusLabel3.Text = "";
                toolStripStatusLabel4.Text = "";
                toolStripStatusLabel5.Text = "";
            }
        }
        //名前をつけて譜面ファイルを保存
        private void 名前をつけて保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EleFilePath = null;
            名前をつけて譜面ファイルを保存・上書き保存();
        }
        //譜面ファイルを上書き保存
        private void 上書き保存ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            名前をつけて譜面ファイルを保存・上書き保存();
        }
        //譜面ファイルを開く
        private void 新規ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //上書き可能か
            bool isEnable = false;
            //上書きの確認
            DialogResult result = MessageBox.Show("編集中の情報はすべて破棄されます。\n続行しますか？", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.OK)
            {
                isEnable = true;
            }
            //上書き可能ならば
            if (isEnable == true)
            {
                //譜面ファイルを正しく開けたか
                bool isOK = false;
                //ファイルパス
                string DialogFileName = null;
                //ファイル名
                string DialogSafeFileName = null;
                //ファイルを選択するダイアログをおっぴろげ！
                OpenFileDialog dialog = new OpenFileDialog();
                //譜面ファイルの形式はtxt？
                dialog.Filter = "譜面ファイル (*.ele)|*.ele";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    DialogFileName = dialog.FileName;
                    DialogSafeFileName = dialog.SafeFileName;
                }
                dialog.Dispose();
                if (DialogFileName != null)
                {
                    //ノートリストのクリア
                    NotePaint.NotepaintReset();
                    //BPMリストのクリア
                    BPM.BPMChangeList.Clear();
                    //音楽情報のクリア
                    MusicInfo = new MusicInfo();
                    StreamReader Stream = new StreamReader(dialog.FileName, Encoding.GetEncoding("shift_jis"));
                    string TempString = string.Empty;
                    string[] ArrayTempString;
                    try
                    {
                        //#TITLE:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(7);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.Title = TempString;
                        //#SUBTITLE:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(10);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.SubTitle = TempString;
                        //#ARTIST:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(8);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.Artist = TempString;
                        //#ALBUMART:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(10);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.AlbumArt = TempString;
                        //#MUSIC:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(7);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.Music = TempString;
                        //#OFFSET:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(8);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        NoteOffsetTime = int.Parse(TempString);
                        //#BPMS:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(6);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        ArrayTempString = TempString.Split(',');
                        for (int i = 0; i < ArrayTempString.Count(); i++)
                        {
                            if (ArrayTempString[i] != "")
                            {
                                if (i == 0)
                                {
                                    BPM.BaseBPM = double.Parse(ArrayTempString[i]);
                                }
                                else
                                {
                                    BPM.BPMChange TempBPMChange = new BPM.BPMChange();
                                    TempBPMChange.BPM = double.Parse(ArrayTempString[i]);
                                    BPM.BPMChangeList.Add(TempBPMChange);
                                }
                            }
                        }
                        //#BPMPOSITIONS:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(14);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        ArrayTempString = TempString.Split(',');
                        for (int i = 0; i < ArrayTempString.Count(); i++)
                        {
                            if (ArrayTempString[i] != "")
                            {
                                if (i != 0)
                                {
                                    BPM.BPMChangeList[i - 1].BeatForChange = int.Parse(ArrayTempString[i]);
                                }
                            }
                        }
                        BPM.BPMChangeComparer BPMListComparer = new BPM.BPMChangeComparer();
                        BPM.BPMChangeList.Sort(BPMListComparer);
                        BPM.CalcTimeFromBeat();
                        //#NOTE:
                        Stream.ReadLine();
                        ////*------------9keys--------------*/
                        Stream.ReadLine();
                        //keys:9
                        Stream.ReadLine();
                        //BASIC:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(6);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.DifficultyBASIC9 = int.Parse(TempString);
                        while (true)
                        {
                            Note.NotePosition TempNote = new Note.NotePosition();
                            //ファイルから読み込み
                            TempString = Stream.ReadLine();
                            //,で区切る
                            ArrayTempString = TempString.Split(',');
                            //正しく区切れたとき
                            if (ArrayTempString.Count() == 3)
                            {
                                TempNote.Measure = int.Parse(ArrayTempString[0]);
                                TempNote.Beat = int.Parse(ArrayTempString[1]);
                                TempNote.Button = int.Parse(ArrayTempString[2]) - 1;
                                NotePaint.BasicNote9.AddNewNotePosition(TempNote);
                            }
                            else if (ArrayTempString.Count() == 1)
                            {
                                break;
                            }
                        }
                        //NORMAL:
                        TempString = TempString.Substring(7);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.DifficultyNORMAL9 = int.Parse(TempString);
                        while (true)
                        {
                            Note.NotePosition TempNote = new Note.NotePosition();
                            //ファイルから読み込み
                            TempString = Stream.ReadLine();
                            //,で区切る
                            ArrayTempString = TempString.Split(',');
                            //正しく区切れたとき
                            if (ArrayTempString.Count() == 3)
                            {
                                TempNote.Measure = int.Parse(ArrayTempString[0]);
                                TempNote.Beat = int.Parse(ArrayTempString[1]);
                                TempNote.Button = int.Parse(ArrayTempString[2]) - 1;
                                NotePaint.NormalNote9.AddNewNotePosition(TempNote);
                            }
                            else if (ArrayTempString.Count() == 1)
                            {
                                break;
                            }
                        }
                        //ADVANCED:
                        TempString = TempString.Substring(9);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.DifficultyADVANCED9 = int.Parse(TempString);
                        while (true)
                        {
                            Note.NotePosition TempNote = new Note.NotePosition();
                            //ファイルから読み込み
                            TempString = Stream.ReadLine();
                            //,で区切る
                            ArrayTempString = TempString.Split(',');
                            //正しく区切れたとき
                            if (ArrayTempString.Count() == 3)
                            {
                                TempNote.Measure = int.Parse(ArrayTempString[0]);
                                TempNote.Beat = int.Parse(ArrayTempString[1]);
                                TempNote.Button = int.Parse(ArrayTempString[2]) - 1;
                                NotePaint.AdvancedNote9.AddNewNotePosition(TempNote);
                            }
                            else if (ArrayTempString.Count() == 1)
                            {
                                break;
                            }
                        }
                        ////*------------16keys--------------*/
                        //keys:16
                        Stream.ReadLine();
                        //BASIC:
                        TempString = Stream.ReadLine();
                        TempString = TempString.Substring(6);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.DifficultyBASIC16 = int.Parse(TempString);
                        while (true)
                        {
                            Note.NotePosition TempNote = new Note.NotePosition();
                            //ファイルから読み込み
                            TempString = Stream.ReadLine();
                            //,で区切る
                            ArrayTempString = TempString.Split(',');
                            //正しく区切れたとき
                            if (ArrayTempString.Count() == 3)
                            {
                                TempNote.Measure = int.Parse(ArrayTempString[0]);
                                TempNote.Beat = int.Parse(ArrayTempString[1]);
                                TempNote.Button = int.Parse(ArrayTempString[2]) - 1;
                                NotePaint.BasicNote16.AddNewNotePosition(TempNote);
                            }
                            else if (ArrayTempString.Count() == 1)
                            {
                                break;
                            }
                        }
                        //NORMAL:
                        TempString = TempString.Substring(7);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.DifficultyNORMAL16 = int.Parse(TempString);
                        while (true)
                        {
                            Note.NotePosition TempNote = new Note.NotePosition();
                            //ファイルから読み込み
                            TempString = Stream.ReadLine();
                            //,で区切る
                            ArrayTempString = TempString.Split(',');
                            //正しく区切れたとき
                            if (ArrayTempString.Count() == 3)
                            {
                                TempNote.Measure = int.Parse(ArrayTempString[0]);
                                TempNote.Beat = int.Parse(ArrayTempString[1]);
                                TempNote.Button = int.Parse(ArrayTempString[2]) - 1;
                                NotePaint.NormalNote16.AddNewNotePosition(TempNote);
                            }
                            else if (ArrayTempString.Count() == 1)
                            {
                                break;
                            }
                        }
                        //ADVANCED:
                        TempString = TempString.Substring(9);
                        TempString = TempString.Remove(TempString.Count() - 1);
                        MusicInfo.DifficultyADVANCED16 = int.Parse(TempString);
                        while (true)
                        {
                            Note.NotePosition TempNote = new Note.NotePosition();
                            //ファイルから読み込み
                            TempString = Stream.ReadLine();
                            if (TempString == null)
                            {
                                break;
                            }
                            //,で区切る
                            ArrayTempString = TempString.Split(',');
                            //正しく区切れたとき
                            if (ArrayTempString.Count() == 3)
                            {
                                TempNote.Measure = int.Parse(ArrayTempString[0]);
                                TempNote.Beat = int.Parse(ArrayTempString[1]);
                                TempNote.Button = int.Parse(ArrayTempString[2]) - 1;
                                NotePaint.AdvancedNote16.AddNewNotePosition(TempNote);
                            }
                            else if (ArrayTempString.Count() == 1)
                            {
                                break;
                            }
                        }
                        //最後まで読み込めたらtrue
                        isOK = true;
                    }
                    catch
                    {
                        MessageBox.Show("譜面ファイルの形式が正しくありません。\n譜面ファイルを読み込めませんでした。");
                    }
                    finally
                    {
                        Stream.Dispose();
                    }
                    if (isOK)
                    {
                        //テキストボックスに反映
                        textBox1.Text = NoteOffsetTime.ToString();
                        textBox2.Text = BPM.BaseBPM.ToString();
                        //トラックバーの初期化
                        trackBar2.Value = 5;
                        trackBar3.Value = 5;
                        //音楽ファイルが同ディレクトリに存在すればそれを開く
                        TempString = DialogFileName.Remove(DialogFileName.Count() - DialogSafeFileName.Count());
                        TempString += MusicInfo.Music;
                        //音楽に関してリセット
                        MusicPlayer.MusicPlayerReset();
                        toolStripStatusLabel1.Text = "注意：音楽ファイルを読み込んでください。";
                        toolStripStatusLabel2.Text = "";
                        toolStripStatusLabel3.Text = "";
                        toolStripStatusLabel4.Text = "";
                        toolStripStatusLabel5.Text = "";
                        if (System.IO.File.Exists(TempString))
                        {
                            //再生プレイヤーの準備
                            MusicPlayer.FetchMusic(TempString, MusicInfo.Music);
                            //ステータスバーに表示
                            toolStripStatusLabel2.Text = "曲名 : " + MusicPlayer.MusicFileName;
                            toolStripStatusLabel5.Text = "　合計再生時間 : " + MusicPlayer.TotalMusicDateTime.ToLongTimeString();
                        }
                        BPM.CalcTimeFromBeat();
                        CalcNoteTime(NotePaint.BasicNote9);
                        CalcNoteTime(NotePaint.NormalNote9);
                        CalcNoteTime(NotePaint.AdvancedNote9);
                        CalcNoteTime(NotePaint.BasicNote16);
                        CalcNoteTime(NotePaint.NormalNote16);
                        CalcNoteTime(NotePaint.AdvancedNote16);
                    }
                }
            }
        }
        //音楽ファイルを開く
        private void 開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ファイルを選択するダイアログをおっぴろげ！
            OpenFileDialog dialog = new OpenFileDialog();
            //wavとmp3とmidiとwmaが再生可能っぽい！
            dialog.Filter = "すべての再生可能なファイル (*.wav,*.mp3,*.mid,*.midi,*.wma)|*.wav;*.mp3;*.mid;*.midi;*.wma"
                            + "|WAVEファイル (*.wav)|*.wav|MP3ファイル (*.mp3)|*.mp3"
                            + "|MIDIファイル (*.mid,*.midi)|*.mid;*.midi|WMAファイル(*.wma)|*.wma";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                //再生プレイヤーの準備
                MusicPlayer.FetchMusic(dialog.FileName,dialog.SafeFileName);
                //楽曲情報の更新
                MusicInfo.Music = dialog.SafeFileName;
                //ステータスバーに表示
                toolStripStatusLabel1.Text = "";
                toolStripStatusLabel2.Text = "曲名 : " + MusicPlayer.MusicFileName;
                toolStripStatusLabel5.Text = "　合計再生時間 : " + MusicPlayer.TotalMusicDateTime.ToLongTimeString();
                //トラックバーの初期化
                trackBar2.Value = 5;
                trackBar3.Value = 5;
            }
            dialog.Dispose();
        }
        //エディタの終了
        private void エディタの終了XToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //名前をつけて譜面ファイルを保存・上書き保存
        void 名前をつけて譜面ファイルを保存・上書き保存()
        {
            //保存を実行するか
            bool isEnable = false;
            //すでにファイルパスが存在すれば
            if (EleFilePath != null)
            {
                isEnable = true;
            }
            else
            {
                SaveFileDialog dialog = new SaveFileDialog();
                //はじめのファイル名を指定する
                dialog.FileName = "新しい譜面ファイル.ele";
                //はじめに表示されるフォルダを指定する
                dialog.Filter = "譜面ファイル(*.ele)|*.ele";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    dialog.Dispose();
                    EleFilePath = dialog.FileName;
                    isEnable = true;
                }
            }
            if (isEnable)
            {
                StreamWriter Stream = new StreamWriter(EleFilePath, false, Encoding.GetEncoding("shift_jis"));
                //楽曲情報を書き込む
                Stream.WriteLine("#TITLE:" + MusicInfo.Title + ";");
                Stream.WriteLine("#SUBTITLE:" + MusicInfo.SubTitle + ";");
                Stream.WriteLine("#ARTIST:" + MusicInfo.Artist + ";");
                Stream.WriteLine("#ALBUMART:" + MusicInfo.AlbumArt + ";");
                Stream.WriteLine("#MUSIC:" + MusicInfo.Music + ";");
                if (NoteOffsetTime == 0)
                {
                    Stream.WriteLine("#OFFSET:" + "0" + ";");
                }
                else
                {
                    Stream.WriteLine("#OFFSET:" + NoteOffsetTime.ToString() + ";");
                }
                //BPM変化を書き込む
                Stream.Write("#BPMS:");
                Stream.Write(BPM.BaseBPM.ToString());
                if (BPM.BPMChangeList.Count() == 0)
                {
                    Stream.Write(";");
                }
                else
                {
                    Stream.Write(",");
                }
                for (int i = 0; i < BPM.BPMChangeList.Count(); i++)
                {
                    if (i + 1 == BPM.BPMChangeList.Count())
                    {
                        Stream.Write(BPM.BPMChangeList[i].BPM + ";");
                    }
                    else
                    {
                        Stream.Write(BPM.BPMChangeList[i].BPM + ",");
                    }
                }
                Stream.WriteLine();
                Stream.Write("#BPMPOSITIONS:");
                Stream.Write("0");
                if (BPM.BPMChangeList.Count() == 0)
                {
                    Stream.Write(";");
                }
                else
                {
                    Stream.Write(",");
                }
                for (int i = 0; i < BPM.BPMChangeList.Count; i++)
                {
                    if (i + 1 == BPM.BPMChangeList.Count)
                    {
                        Stream.Write(BPM.BPMChangeList[i].BeatForChange + ";");
                    }
                    else
                    {
                        Stream.Write(BPM.BPMChangeList[i].BeatForChange + ",");
                    }
                }
                Stream.WriteLine();
                //ノートを書き込む。
                Stream.WriteLine("#NOTE:");
                Stream.WriteLine("/*------------9keys--------------*/");
                Stream.WriteLine("keys:9");
                //ノート数が0であれば難易度を０に
                if (NotePaint.BasicNote9.NoteList.Count() == 0)
                {
                    Stream.WriteLine("BASIC:" +"0" + ";");
                }
                else
                {
                    Stream.WriteLine("BASIC:" + MusicInfo.DifficultyBASIC9.ToString() + ";");
                }
                //ノートの書き込み
                for (int i = 0; i < NotePaint.BasicNote9.NoteList.Count; i++)
                {
                    string temp;
                    temp = NotePaint.BasicNote9.NoteList[i].Measure.ToString() + ","
                        + NotePaint.BasicNote9.NoteList[i].Beat.ToString() + ","
                        + (NotePaint.BasicNote9.NoteList[i].Button + 1).ToString();
                    //ボタンを1～16にする
                    Stream.WriteLine(temp);
                }
                //ノート数が0であれば難易度を０に
                if (NotePaint.NormalNote9.NoteList.Count() == 0)
                {
                    Stream.WriteLine("NORMAL:" + "0" + ";");
                }
                else
                {
                    Stream.WriteLine("NORMAL:" + MusicInfo.DifficultyNORMAL9.ToString() + ";");
                }
                //ノートの書き込み
                for (int i = 0; i < NotePaint.NormalNote9.NoteList.Count; i++)
                {
                    string temp;
                    temp = NotePaint.NormalNote9.NoteList[i].Measure.ToString() + ","
                        + NotePaint.NormalNote9.NoteList[i].Beat.ToString() + ","
                        + (NotePaint.NormalNote9.NoteList[i].Button + 1).ToString();
                    //ボタンを1～16にする
                    Stream.WriteLine(temp);
                }
                //ノート数が0であれば難易度を０に
                if (NotePaint.AdvancedNote9.NoteList.Count() == 0)
                {
                    Stream.WriteLine("ADVANCED:" + "0" + ";");
                }
                else
                {
                    Stream.WriteLine("ADVANCED:" + MusicInfo.DifficultyADVANCED9.ToString() + ";");
                }
                //ノートの書き込み
                for (int i = 0; i < NotePaint.AdvancedNote9.NoteList.Count; i++)
                {
                    string temp;
                    temp = NotePaint.AdvancedNote9.NoteList[i].Measure.ToString() + ","
                        + NotePaint.AdvancedNote9.NoteList[i].Beat.ToString() + ","
                        + (NotePaint.AdvancedNote9.NoteList[i].Button + 1).ToString();
                    Stream.WriteLine(temp);
                }
                Stream.WriteLine("/*------------16keys--------------*/");
                Stream.WriteLine("keys:16");
                //ノート数が0であれば難易度を０に
                if (NotePaint.BasicNote16.NoteList.Count() == 0)
                {
                    Stream.WriteLine("BASIC:" + "0" + ";");
                }
                else
                {
                    Stream.WriteLine("BASIC:" + MusicInfo.DifficultyBASIC16.ToString() + ";");
                }
                //ノートの書き込み
                for (int i = 0; i < NotePaint.BasicNote16.NoteList.Count; i++)
                {
                    string temp;
                    temp = NotePaint.BasicNote16.NoteList[i].Measure.ToString() + ","
                        + NotePaint.BasicNote16.NoteList[i].Beat.ToString() + ","
                        + (NotePaint.BasicNote16.NoteList[i].Button + 1).ToString();
                    Stream.WriteLine(temp);
                }
                //ノート数が0であれば難易度を０に
                if (NotePaint.NormalNote16.NoteList.Count() == 0)
                {
                    Stream.WriteLine("NORMAL:" + "0" + ";");
                }
                else
                {
                    Stream.WriteLine("NORMAL:" + MusicInfo.DifficultyNORMAL16.ToString() + ";");
                }
                //ノートの書き込み
                for (int i = 0; i < NotePaint.NormalNote16.NoteList.Count; i++)
                {
                    string temp;
                    temp = NotePaint.NormalNote16.NoteList[i].Measure.ToString() + ","
                        + NotePaint.NormalNote16.NoteList[i].Beat.ToString() + ","
                        + (NotePaint.NormalNote16.NoteList[i].Button + 1).ToString();
                    Stream.WriteLine(temp);
                }
                //ノート数が0であれば難易度を０に
                if (NotePaint.AdvancedNote16.NoteList.Count() == 0)
                {
                    Stream.WriteLine("ADVANCED:" + "0" + ";");
                }
                else
                {
                    Stream.WriteLine("ADVANCED:" + MusicInfo.DifficultyADVANCED16.ToString() + ";");
                }
                //ノートの書き込み
                for (int i = 0; i < NotePaint.AdvancedNote16.NoteList.Count; i++)
                {
                    string temp;
                    temp = NotePaint.AdvancedNote16.NoteList[i].Measure.ToString() + ","
                        + NotePaint.AdvancedNote16.NoteList[i].Beat.ToString() + ","
                        + (NotePaint.AdvancedNote16.NoteList[i].Button + 1).ToString();
                        Stream.WriteLine(temp);
                }
                Stream.Write("/*------------END--------------*/");
                Stream.Dispose();
            }
        }

        //難易度(BASIC)
        private void eASYEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NotePaint.NoteDifficluty = NotePaint.NoteDifficlutyMode.BASIC;
            eASYEToolStripMenuItem.Checked = true;
            sTANDARDSToolStripMenuItem.Checked = false;
            aDVANCEDAToolStripMenuItem1.Checked = false;
        }
        //難易度(NORMAL)
        private void sTANDARDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NotePaint.NoteDifficluty = NotePaint.NoteDifficlutyMode.NORMAL;
            eASYEToolStripMenuItem.Checked = false;
            sTANDARDSToolStripMenuItem.Checked = true;
            aDVANCEDAToolStripMenuItem1.Checked = false;
        }
        //難易度(ADVANCED)
        private void aDVANCEDAToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NotePaint.NoteDifficluty = NotePaint.NoteDifficlutyMode.ADVANCED;
            eASYEToolStripMenuItem.Checked = false;
            sTANDARDSToolStripMenuItem.Checked = false;
            aDVANCEDAToolStripMenuItem1.Checked = true;
        }

        //キー数。9キー
        private void kEYSEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NotePaint.Keys = 9;
            kEYSEToolStripMenuItem.Checked = true;
            kEYS16JToolStripMenuItem.Checked = false;
        }
        //キー数。16キー
        private void kEYSJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NotePaint.Keys = 16;
            kEYSEToolStripMenuItem.Checked = false;
            kEYS16JToolStripMenuItem.Checked = true;
        }

        //楽曲情報
        private void 未定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 dialog = new Form2();
            dialog.TempMusicInfo.Title = MusicInfo.Title;
            dialog.TempMusicInfo.SubTitle = MusicInfo.SubTitle;
            dialog.TempMusicInfo.Artist = MusicInfo.Artist;
            dialog.TempMusicInfo.AlbumArt = MusicInfo.AlbumArt;
            dialog.TempMusicInfo.Music = MusicInfo.Music;
            dialog.TempMusicInfo.NoteCountBASIC9 = NotePaint.BasicNote9.NoteList.Count;
            dialog.TempMusicInfo.NoteCountNORMAL9 = NotePaint.NormalNote9.NoteList.Count;
            dialog.TempMusicInfo.NoteCountADVANCED9 = NotePaint.AdvancedNote9.NoteList.Count;
            dialog.TempMusicInfo.NoteCountBASIC16 = NotePaint.BasicNote16.NoteList.Count;
            dialog.TempMusicInfo.NoteCountNORMAL16 = NotePaint.NormalNote16.NoteList.Count;
            dialog.TempMusicInfo.NoteCountADVANCED16 = NotePaint.AdvancedNote16.NoteList.Count;
            dialog.TempMusicInfo.DifficultyBASIC9 = MusicInfo.DifficultyBASIC9;
            dialog.TempMusicInfo.DifficultyNORMAL9 = MusicInfo.DifficultyNORMAL9;
            dialog.TempMusicInfo.DifficultyADVANCED9 = MusicInfo.DifficultyADVANCED9;
            dialog.TempMusicInfo.DifficultyBASIC16 = MusicInfo.DifficultyBASIC16;
            dialog.TempMusicInfo.DifficultyNORMAL16 = MusicInfo.DifficultyNORMAL16;
            dialog.TempMusicInfo.DifficultyADVANCED16 = MusicInfo.DifficultyADVANCED16;

            dialog.ShowDialog(this);
            if (dialog.DialogResult == DialogResult.OK)
            {
                MusicInfo.Title = dialog.TempMusicInfo.Title;
                MusicInfo.SubTitle = dialog.TempMusicInfo.SubTitle;
                MusicInfo.Artist = dialog.TempMusicInfo.Artist;
                MusicInfo.AlbumArt = dialog.TempMusicInfo.AlbumArt;
                MusicInfo.Music = dialog.TempMusicInfo.Music;
                MusicInfo.DifficultyBASIC9 = dialog.TempMusicInfo.DifficultyBASIC9;
                MusicInfo.DifficultyNORMAL9 = dialog.TempMusicInfo.DifficultyNORMAL9;
                MusicInfo.DifficultyADVANCED9 = dialog.TempMusicInfo.DifficultyADVANCED9;
                MusicInfo.DifficultyBASIC16 = dialog.TempMusicInfo.DifficultyBASIC16;
                MusicInfo.DifficultyNORMAL16 = dialog.TempMusicInfo.DifficultyNORMAL16;
                MusicInfo.DifficultyADVANCED16 = dialog.TempMusicInfo.DifficultyADVANCED16;
            }
        }

        //補助線32分
        private void 分ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NotePaint.VerticalSubLine = 1;
            分ToolStripMenuItem.Checked = true;
            分ToolStripMenuItem1.Checked = false;
            分ToolStripMenuItem2.Checked = false;
            分ToolStripMenuItem3.Checked = false;
        }
        //補助線16分
        private void 分ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            NotePaint.VerticalSubLine = 2;
            分ToolStripMenuItem.Checked = false;
            分ToolStripMenuItem1.Checked = true;
            分ToolStripMenuItem2.Checked = false;
            分ToolStripMenuItem3.Checked = false;
        }
        //補助線8分
        private void 分ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            NotePaint.VerticalSubLine = 4;
            分ToolStripMenuItem.Checked = false;
            分ToolStripMenuItem1.Checked = false;
            分ToolStripMenuItem2.Checked = true;
            分ToolStripMenuItem3.Checked = false;
        }
        //補助線4分
        private void 分ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            NotePaint.VerticalSubLine = 8;
            分ToolStripMenuItem.Checked = false;
            分ToolStripMenuItem1.Checked = false;
            分ToolStripMenuItem2.Checked = false;
            分ToolStripMenuItem3.Checked = true;
        }

        //eLEBEATwiki
        private void eLEBEATwikiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www48.atwiki.jp/elebeat/");
        }

        //プレビュー画面の表示
        private void プレビュー画面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PreviewForm.Created == false)
            {
                PreviewForm.Dispose();
                PreviewForm = null;
                PreviewForm = new Form4();
                PreviewForm.Show(this);
            }
        }

        //切り取り
        private void 切り取りToolStripMenuItem_Click(object sender, EventArgs e)
        {
            切り取り();
        }
        //コピー
        private void コピーToolStripMenuItem_Click(object sender, EventArgs e)
        {
            コピー();
        }
        //貼り付け
        private void 貼り付けToolStripMenuItem_Click(object sender, EventArgs e)
        {
            貼り付け();
        }
        //削除
        private void クリアToolStripMenuItem_Click(object sender, EventArgs e)
        {
            削除();
        }
        //すべて選択
        private void すべて選択ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            すべて選択();
        }
        //選択解除
        private void 選択解除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            選択解除();
        }
        //ノートの反転
        private void ノートの反転ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ノートの反転();
        }

        //ノートの時間を計算する
        private void CalcNoteTime(Note CalcNote)
        {
            for (int i = 0; i < CalcNote.NoteList.Count(); i++)
            {
                int tempBeat = CalcNote.NoteList[i].Measure * 32 + CalcNote.NoteList[i].Beat;
                int j;
                for (j = 0; j < BPM.BPMChangeList.Count(); j++)
                {
                    if (BPM.BPMChangeList[j].BeatForChange > tempBeat)
                    {
                        break;
                    }
                }
                if (j != 0)
                {
                    j--;
                    CalcNote.NoteList[i].Time = -NoteOffsetTime + BPM.BPMChangeList[j].TimeForChange + (60000.0 / BPM.BPMChangeList[j].BPM) / 8.0 * (tempBeat - BPM.BPMChangeList[j].BeatForChange);
                }
                else
                {
                    CalcNote.NoteList[i].Time = -NoteOffsetTime + (int)((60000.0 / BPM.BaseBPM) / 8.0 * tempBeat);
                }
            }
        }

    }
}