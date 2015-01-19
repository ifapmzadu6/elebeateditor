using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ELEBEATMusicEditer
{
    public class MusicInfo
    {
        public string Title;
        public string SubTitle;
        public string Artist;
        public string AlbumArt;
        public string Music;

        public int DifficultyBASIC9;
        public int DifficultyNORMAL9;
        public int DifficultyADVANCED9;
        public int DifficultyBASIC16;
        public int DifficultyNORMAL16;
        public int DifficultyADVANCED16;

        public int NoteCountBASIC9;
        public int NoteCountNORMAL9;
        public int NoteCountADVANCED9;
        public int NoteCountBASIC16;
        public int NoteCountNORMAL16;
        public int NoteCountADVANCED16;

        //コンストラクタ
        public MusicInfo()
        {
            DifficultyBASIC9 = 1;
            DifficultyNORMAL9 = 1;
            DifficultyADVANCED9 = 1;
            DifficultyBASIC16 = 1;
            DifficultyNORMAL16 = 1;
            DifficultyADVANCED16 = 1;
        }

    }
}
