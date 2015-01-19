using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ELEBEATMusicEditer
{
    public partial class Form4 : Form
    {
        //アニメーション１枚当たりの時間
        private static int AnimeTime = 25;

        //メインフォームから受け取る
        public Note PreviewNote;
        public int PresentTime;
        public int PreviewKeys;
        public int NoteOffsetTime;


        bool[] tempKeyDown = new bool[16];
        bool[] tempNoteDoubleCheck = new bool[16];
        bool[] tempNoteisDouble = new bool[16];
        int[] tempTime=new int[16];
        Image[] NoteImages;
        Image[] LongNoteImages;
        Rectangle tempRectangle = new Rectangle();
        Pen tempPen = new Pen(Color.Red);
        Rectangle tempKeyRectangle = new Rectangle();
        static Font tempFont = new Font("MS UI Gothic", 10, FontStyle.Bold);
        
        public Form4()
        {
            InitializeComponent();
            tempPen.Width = 3;
        }

        private void Form4_Paint(object sender, PaintEventArgs e)
        {
            int tempClientSize = this.ClientSize.Height;
            if (PreviewKeys == 16)
            {
                int Border = tempClientSize / 30;
                tempRectangle.Height = (tempClientSize - Border) / 4 - Border;
                tempRectangle.Width = tempRectangle.Height;
                for (int i = 0; i < 16; i++)
                {
                    tempRectangle.X = (i % 4) * (tempClientSize - Border) / 4 + Border;
                    tempRectangle.Y = (i / 4) * (tempClientSize - Border) / 4 + Border;
                    e.Graphics.FillRectangle(Brushes.Black, tempRectangle);
                    //キーが押されたときに赤枠表示
                    if (tempKeyDown[i] == true)
                    {
                        tempKeyRectangle.X = (i % 4) * (tempClientSize - Border) / 4 + Border - 2;
                        tempKeyRectangle.Y = (i / 4) * (tempClientSize - Border) / 4 + Border - 2;
                        tempKeyRectangle.Width = tempRectangle.Width + 3;
                        tempKeyRectangle.Height = tempRectangle.Height + 3;
                        e.Graphics.DrawRectangle(tempPen, tempKeyRectangle);
                    }
                    tempNoteDoubleCheck[i] = false;
                }
                for (int i = 0; i < PreviewNote.NoteList.Count(); i++)
                {
                    if (PreviewNote.NoteList[i].Time + AnimeTime * 6 > PresentTime && PresentTime >= PreviewNote.NoteList[i].Time - AnimeTime * 23)
                    {
                        tempRectangle.X = (PreviewNote.NoteList[i].Button % 4) * (tempClientSize - Border) / 4 + Border;
                        tempRectangle.Y = (PreviewNote.NoteList[i].Button / 4) * (tempClientSize - Border) / 4 + Border;
                        e.Graphics.DrawImage(NoteImages[(PresentTime - ((int)PreviewNote.NoteList[i].Time - AnimeTime * 23)) / AnimeTime], tempRectangle);
                        if (tempNoteDoubleCheck[PreviewNote.NoteList[i].Button] == true && PresentTime > -NoteOffsetTime + 500)
                        {
                            tempTime[PreviewNote.NoteList[i].Button] = PresentTime;
                            tempNoteisDouble[PreviewNote.NoteList[i].Button] = true;
                        }
                        tempNoteDoubleCheck[PreviewNote.NoteList[i].Button] = true;
                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    if (tempNoteisDouble[i] == true)
                    {
                        tempRectangle.X = (i % 4) * (tempClientSize - Border) / 4 + Border;
                        tempRectangle.Y = (i / 4) * (tempClientSize - Border) / 4 + Border;
                        e.Graphics.FillRectangle(Brushes.Red, tempRectangle);
                        e.Graphics.DrawString("ERROR\nノートの間隔が狭すぎます。", tempFont, Brushes.White, tempRectangle);
                        if (PresentTime > tempTime[i] + 500 || tempTime[i] > PresentTime)
                        {
                            tempNoteisDouble[i] = false;
                        }
                    }
                }
            }
            else if (PreviewKeys == 9)
            {
                int Border = tempClientSize / 30;
                tempRectangle.Height = (tempClientSize - Border) / 3 - Border;
                tempRectangle.Width = tempRectangle.Height;
                for (int i = 0; i < 9; i++)
                {
                    tempRectangle.X = (i % 3) * (tempClientSize - Border) / 3 + Border;
                    tempRectangle.Y = (i / 3) * (tempClientSize - Border) / 3 + Border;
                    e.Graphics.FillRectangle(Brushes.Black, tempRectangle);
                    //キーが押されたときに赤枠表示
                    if (tempKeyDown[i] == true)
                    {
                        tempKeyRectangle.X = (i % 3) * (tempClientSize - Border) / 3 + Border - 2;
                        tempKeyRectangle.Y = (i / 3) * (tempClientSize - Border) / 3 + Border - 2;
                        tempKeyRectangle.Width = tempRectangle.Width + 3;
                        tempKeyRectangle.Height = tempRectangle.Height + 3;
                        e.Graphics.DrawRectangle(tempPen, tempKeyRectangle);
                    }
                    tempNoteDoubleCheck[i] = false;
                }
                for (int i = 0; i < PreviewNote.NoteList.Count(); i++)
                {
                    if (PresentTime >= PreviewNote.NoteList[i].Time - AnimeTime * 23 && PreviewNote.NoteList[i].Time + AnimeTime * 6 > PresentTime)
                    {
                        tempRectangle.X = (PreviewNote.NoteList[i].Button % 3) * (tempClientSize - Border) / 3 + Border;
                        tempRectangle.Y = (PreviewNote.NoteList[i].Button / 3) * (tempClientSize - Border) / 3 + Border;
                        e.Graphics.DrawImage(NoteImages[(PresentTime - ((int)PreviewNote.NoteList[i].Time - AnimeTime * 23)) / AnimeTime], tempRectangle);
                        if (tempNoteDoubleCheck[PreviewNote.NoteList[i].Button] == true && PresentTime > -NoteOffsetTime + 500)
                        {
                            tempTime[PreviewNote.NoteList[i].Button] = PresentTime;
                            tempNoteisDouble[PreviewNote.NoteList[i].Button] = true;
                        }
                        tempNoteDoubleCheck[PreviewNote.NoteList[i].Button] = true;
                    }
                }
                for (int i = 0; i < 16; i++)
                {
                    if (tempNoteisDouble[i] == true)
                    {
                        tempRectangle.X = (i % 3) * (tempClientSize - Border) / 3 + Border;
                        tempRectangle.Y = (i / 3) * (tempClientSize - Border) / 3 + Border;
                        e.Graphics.FillRectangle(Brushes.Red, tempRectangle);
                        e.Graphics.DrawString("ERROR!!\nノートの間隔が狭すぎます。", tempFont, Brushes.White, tempRectangle);
                        if (PresentTime > tempTime[i] + 500 || tempTime[i] > PresentTime)
                        {
                            tempNoteisDouble[i] = false;
                        }
                    }
                }
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            //クライアント領域を初期化する
            this.ClientSize = new Size(300, 300);
            //埋め込まれたリソースのストリーム
            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            //イメージに読み込み
            NoteImages = new Image[30];
            for (int i = 0; i <= 29; i++)
            {
                NoteImages[i] = Image.FromStream(myAssembly.GetManifestResourceStream("ELEBEATMusicEditer.Images.Note." + i.ToString() + ".png"));
            }
            LongNoteImages = new Image[51];
            for (int i = 0; i <= 50; i++)
            {
                LongNoteImages[i] = Image.FromStream(myAssembly.GetManifestResourceStream("ELEBEATMusicEditer.Images.LongNote." + i.ToString() + ".png"));
            }
            myAssembly = null;
        }

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            for (int i = 0; i <= 29; i++)
            {
                NoteImages[i].Dispose();
                NoteImages[i] = null;
            }
            NoteImages = null;
        }

        protected override void WndProc(ref Message m)
        {
            //この一行を追加します。
            this.AspectRatioSizeWndProc(ref m, 1f, true);
            base.WndProc(ref m);
        }

        private void Form4_KeyDown(object sender, KeyEventArgs e)
        {
            if (PreviewKeys == 16)
            {
                if (e.KeyCode == Keys.D1) tempKeyDown[0]=true;
                if (e.KeyCode == Keys.D2) tempKeyDown[1] = true;
                if (e.KeyCode == Keys.D3) tempKeyDown[2] = true;
                if (e.KeyCode == Keys.D4) tempKeyDown[3] = true;
                if (e.KeyCode == Keys.Q) tempKeyDown[4] = true;
                if (e.KeyCode == Keys.W) tempKeyDown[5] = true;
                if (e.KeyCode == Keys.E) tempKeyDown[6] = true;
                if (e.KeyCode == Keys.R) tempKeyDown[7] = true;
                if (e.KeyCode == Keys.A) tempKeyDown[8] = true;
                if (e.KeyCode == Keys.S) tempKeyDown[9] = true;
                if (e.KeyCode == Keys.D) tempKeyDown[10] = true;
                if (e.KeyCode == Keys.F) tempKeyDown[11] = true;
                if (e.KeyCode == Keys.Z) tempKeyDown[12] = true;
                if (e.KeyCode == Keys.X) tempKeyDown[13] = true;
                if (e.KeyCode == Keys.C) tempKeyDown[14] = true;
                if (e.KeyCode == Keys.V) tempKeyDown[15] = true;
            }
            else if (PreviewKeys == 9)
            {
                if (e.KeyCode == Keys.W) tempKeyDown[0] = true;
                if (e.KeyCode == Keys.E) tempKeyDown[1] = true;
                if (e.KeyCode == Keys.R) tempKeyDown[2] = true;
                if (e.KeyCode == Keys.S) tempKeyDown[3] = true;
                if (e.KeyCode == Keys.D) tempKeyDown[4] = true;
                if (e.KeyCode == Keys.F) tempKeyDown[5] = true;
                if (e.KeyCode == Keys.X) tempKeyDown[6] = true;
                if (e.KeyCode == Keys.C) tempKeyDown[7] = true;
                if (e.KeyCode == Keys.V) tempKeyDown[8] = true;
                if (e.KeyCode == Keys.NumPad7) tempKeyDown[0] = true;
                if (e.KeyCode == Keys.NumPad8) tempKeyDown[1] = true;
                if (e.KeyCode == Keys.NumPad9) tempKeyDown[2] = true;
                if (e.KeyCode == Keys.NumPad4) tempKeyDown[3] = true;
                if (e.KeyCode == Keys.NumPad5) tempKeyDown[4] = true;
                if (e.KeyCode == Keys.NumPad6) tempKeyDown[5] = true;
                if (e.KeyCode == Keys.NumPad1) tempKeyDown[6] = true;
                if (e.KeyCode == Keys.NumPad2) tempKeyDown[7] = true;
                if (e.KeyCode == Keys.NumPad3) tempKeyDown[8] = true;
            }
        }

        private void Form4_KeyUp(object sender, KeyEventArgs e)
        {
            if (PreviewKeys == 16)
            {
                if (e.KeyCode == Keys.D1) tempKeyDown[0] = false;
                if (e.KeyCode == Keys.D2) tempKeyDown[1] = false;
                if (e.KeyCode == Keys.D3) tempKeyDown[2] = false;
                if (e.KeyCode == Keys.D4) tempKeyDown[3] = false;
                if (e.KeyCode == Keys.Q) tempKeyDown[4] = false;
                if (e.KeyCode == Keys.W) tempKeyDown[5] = false;
                if (e.KeyCode == Keys.E) tempKeyDown[6] = false;
                if (e.KeyCode == Keys.R) tempKeyDown[7] = false;
                if (e.KeyCode == Keys.A) tempKeyDown[8] = false;
                if (e.KeyCode == Keys.S) tempKeyDown[9] = false;
                if (e.KeyCode == Keys.D) tempKeyDown[10] = false;
                if (e.KeyCode == Keys.F) tempKeyDown[11] = false;
                if (e.KeyCode == Keys.Z) tempKeyDown[12] = false;
                if (e.KeyCode == Keys.X) tempKeyDown[13] = false;
                if (e.KeyCode == Keys.C) tempKeyDown[14] = false;
                if (e.KeyCode == Keys.V) tempKeyDown[15] = false;
            }
            else if (PreviewKeys == 9)
            {
                if (e.KeyCode == Keys.W) tempKeyDown[0] = false;
                if (e.KeyCode == Keys.E) tempKeyDown[1] = false;
                if (e.KeyCode == Keys.R) tempKeyDown[2] = false;
                if (e.KeyCode == Keys.S) tempKeyDown[3] = false;
                if (e.KeyCode == Keys.D) tempKeyDown[4] = false;
                if (e.KeyCode == Keys.F) tempKeyDown[5] = false;
                if (e.KeyCode == Keys.X) tempKeyDown[6] = false;
                if (e.KeyCode == Keys.C) tempKeyDown[7] = false;
                if (e.KeyCode == Keys.V) tempKeyDown[8] = false;
                if (e.KeyCode == Keys.NumPad7) tempKeyDown[0] = false;
                if (e.KeyCode == Keys.NumPad8) tempKeyDown[1] = false;
                if (e.KeyCode == Keys.NumPad9) tempKeyDown[2] = false;
                if (e.KeyCode == Keys.NumPad4) tempKeyDown[3] = false;
                if (e.KeyCode == Keys.NumPad5) tempKeyDown[4] = false;
                if (e.KeyCode == Keys.NumPad6) tempKeyDown[5] = false;
                if (e.KeyCode == Keys.NumPad1) tempKeyDown[6] = false;
                if (e.KeyCode == Keys.NumPad2) tempKeyDown[7] = false;
                if (e.KeyCode == Keys.NumPad3) tempKeyDown[8] = false;
            }
        }

    }
}
