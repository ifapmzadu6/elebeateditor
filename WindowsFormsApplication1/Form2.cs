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
    public partial class Form2 : Form
    {
        public MusicInfo TempMusicInfo = new MusicInfo();

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = TempMusicInfo.Title;
            textBox2.Text = TempMusicInfo.SubTitle;
            textBox4.Text = TempMusicInfo.Artist;
            textBox3.Text = TempMusicInfo.AlbumArt;
            textBox5.Text = TempMusicInfo.Music;
            label7.Text = TempMusicInfo.NoteCountBASIC9.ToString();
            label10.Text = TempMusicInfo.NoteCountNORMAL9.ToString();
            label13.Text = TempMusicInfo.NoteCountADVANCED9.ToString();
            label16.Text = TempMusicInfo.NoteCountBASIC16.ToString();
            label19.Text = TempMusicInfo.NoteCountNORMAL16.ToString();
            label22.Text = TempMusicInfo.NoteCountADVANCED16.ToString();
            if (TempMusicInfo.DifficultyBASIC9 == 0)
            {
                numericUpDown1.Value = 1;
            }
            else
            {
                numericUpDown1.Value = TempMusicInfo.DifficultyBASIC9;
            }
            if (TempMusicInfo.DifficultyNORMAL9 == 0)
            {
                numericUpDown2.Value = 1;
            }
            else
            {
                numericUpDown2.Value = TempMusicInfo.DifficultyNORMAL9;
            }
            if (TempMusicInfo.DifficultyADVANCED9 == 0)
            {
                numericUpDown3.Value = 1;
            }
            else
            {
                numericUpDown3.Value = TempMusicInfo.DifficultyADVANCED9;
            }
            if (TempMusicInfo.DifficultyBASIC16 == 0)
            {
                numericUpDown4.Value = 1;
            }
            else
            {
                numericUpDown4.Value = TempMusicInfo.DifficultyBASIC16;
            }
            if (TempMusicInfo.DifficultyNORMAL16 == 0)
            {
                numericUpDown5.Value = 1;
            }
            else
            {
                numericUpDown5.Value = TempMusicInfo.DifficultyNORMAL16;
            }
            if (TempMusicInfo.DifficultyADVANCED16 == 0)
            {
                numericUpDown6.Value = 1;
            }
            else
            {
                numericUpDown6.Value = TempMusicInfo.DifficultyADVANCED16;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TempMusicInfo.Title = textBox1.Text;
            TempMusicInfo.SubTitle = textBox2.Text;
            TempMusicInfo.Artist = textBox4.Text;
            TempMusicInfo.AlbumArt = textBox3.Text;
            TempMusicInfo.Music = textBox5.Text;
            TempMusicInfo.DifficultyBASIC9 = (int)numericUpDown1.Value;
            TempMusicInfo.DifficultyNORMAL9 = (int)numericUpDown2.Value;
            TempMusicInfo.DifficultyADVANCED9 = (int)numericUpDown3.Value;
            TempMusicInfo.DifficultyBASIC16 = (int)numericUpDown4.Value;
            TempMusicInfo.DifficultyNORMAL16 = (int)numericUpDown5.Value;
            TempMusicInfo.DifficultyADVANCED16 = (int)numericUpDown6.Value;
        }
    }
}
