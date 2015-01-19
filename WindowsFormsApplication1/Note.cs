using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ELEBEATMusicEditer
{
    public class Note
    {
        public class NotePosition
        {
            public int Measure;
            public int Beat;
            public int Button;
            public double Time;
            public bool Selected;
        }

        public List<NotePosition> NoteList = new List<NotePosition>();

        //ノートが一つでも選択されていたら
        public bool NoteSelected;

        NoteComparer Comparer = new NoteComparer();

        //コンストラクタ
        public Note()
        {
            NoteList.Capacity = 5000;
        }

        //ノートを重複しないように追加する。
        public void AddNewNotePosition(NotePosition NewNotePosition)
        {
            NoteList.Sort(Comparer);
            //以下の3行は一時的に設置
            int tempBeat = NewNotePosition.Measure * 32 + NewNotePosition.Beat;
            NewNotePosition.Measure = tempBeat / 32;
            NewNotePosition.Beat = tempBeat % 32;
            int i=NoteList.BinarySearch(NewNotePosition, Comparer);
            if (i < 0)
            {
                NoteList.Add(NewNotePosition);
                NoteList.Sort(Comparer);
            }
        }

        //選択されたノートのみ削除する
        public void RemoveSelectedNote()
        {
            for (int i = 0; i < NoteList.Count(); i++)
            {
                if (NoteList[i].Selected == true)
                {
                    NoteList.RemoveAt(i);
                    i--;
                }
            }
            NoteList.Sort(Comparer);
            NoteSelected = false;
        }

        //選択されたノートを差分移動させる
        public void MoveSelectedNote(int MoveBeat, int MoveButton,int Keys)
        {
            //ノートを移動させる
            if (MoveBeat != 0)
            {
                for (int i = 0; i < NoteList.Count(); i++)
                {
                    if (NoteList[i].Selected == true)
                    {
                        //最初のノートが0以下にならないようにする
                        if (NoteList[i].Measure * 32 + NoteList[i].Beat + MoveBeat >= 0)
                        {
                            NoteList[i].Measure += MoveBeat / 32;
                            NoteList[i].Beat += MoveBeat % 32;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            //ボタンを移動させる
            if (MoveButton != 0)
            {
                bool EnableMove = true;
                for (int i = 0; i < NoteList.Count(); i++)
                {
                    if (NoteList[i].Selected == true)
                    {
                        if (0 > NoteList[i].Button + MoveButton || NoteList[i].Button + MoveButton >= Keys)
                        {
                            EnableMove = false;
                        }
                    }
                }
                if (EnableMove == true)
                {
                    for (int i = 0; i < NoteList.Count(); i++)
                    {
                        if (NoteList[i].Selected == true)
                        {
                            if (NoteList[i].Button + MoveButton >= 0 && NoteList[i].Button + MoveButton < Keys)
                            {
                                NoteList[i].Button += MoveButton;
                            }
                        }
                    }
                }
            }
            NoteList.Sort(Comparer);
        }

        //重複したノートを消す
        public void RemoveDoubleNote()
        {
            NotePosition TempNotePosition = new NotePosition();
            NoteList.Sort(Comparer);
            for (int i = 0; i < NoteList.Count(); i++)
            {
                //ノートを一旦取り除いて、検索する
                TempNotePosition.Measure = NoteList[i].Measure;
                TempNotePosition.Beat = NoteList[i].Beat;
                TempNotePosition.Button = NoteList[i].Button;
                NoteList.RemoveAt(i);
                int tes = NoteList.BinarySearch(TempNotePosition, Comparer);
                //すべて検索してすべて削除する
                while (true)
                {
                    tes = NoteList.BinarySearch(TempNotePosition, Comparer);
                    if (tes >= 0)
                    {
                        NoteList.RemoveAt(tes);
                        tes--;
                    }
                    else
                    {
                        break;
                    }
                }
                NotePosition NewNotePosition = new NotePosition();
                NewNotePosition.Beat = TempNotePosition.Beat;
                NewNotePosition.Measure = TempNotePosition.Measure;
                NewNotePosition.Button = TempNotePosition.Button;
                AddNewNotePosition(NewNotePosition);
                NoteList.Sort(Comparer);
            }
        }

        //すべて選択する
        public void SelectAllnote()
        {
            for (int i = 0; i < NoteList.Count(); i++)
            {
                NoteList[i].Selected = true;
            }
            NoteSelected = true;
        }

        //すべて選択解除する。ついでにRemoveDoubleNote()
        public void UnSelectAllnote()
        {
            RemoveDoubleNote();
            for (int i = 0; i < NoteList.Count(); i++)
            {
                NoteList[i].Selected = false;
            }
            NoteSelected = false;
        }

        //最初と最後のノートで作られる四角形に入るノートのみ選択する
        public void SelectRectangle(NotePosition FirstNotePosition,NotePosition LastNotePosition)
        {
            //まずすべてのノートの選択解除
            UnSelectAllnote();
            int tempBeatFirst = 0;
            tempBeatFirst = FirstNotePosition.Measure * 32 + FirstNotePosition.Beat;
            int tempBeatLast = 0;
            tempBeatLast = LastNotePosition.Measure * 32 + LastNotePosition.Beat;
            int tempBeat = 0;
            for (int i = 0; i < NoteList.Count(); i++)
            {
                tempBeat = NoteList[i].Measure * 32 + NoteList[i].Beat;
                if (tempBeatLast >= tempBeat)
                {
                    if (tempBeat >= tempBeatFirst)
                    {
                        if (NoteList[i].Button >= FirstNotePosition.Button && LastNotePosition.Button >= NoteList[i].Button
                            || NoteList[i].Button <= FirstNotePosition.Button && LastNotePosition.Button <= NoteList[i].Button)
                        {
                            NoteList[i].Selected = true;
                            NoteSelected = true;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        //NotePositionのListのSortを使うとき
        public class NoteComparer : IComparer<Note.NotePosition>
        {
            public int Compare(Note.NotePosition x, Note.NotePosition y)
            {
                if (x.Measure < y.Measure)
                {
                    return -1;
                }
                else if (x.Measure > y.Measure)
                {
                    return 1;
                }
                else
                {
                    if (x.Beat < y.Beat)
                    {
                        return -1;
                    }
                    else if (x.Beat > y.Beat)
                    {
                        return 1;
                    }
                    else
                    {
                        if (x.Button < y.Button)
                        {
                            return -1;
                        }
                        else if (x.Button > y.Button)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }

    }
}
