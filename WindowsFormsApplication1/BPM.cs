using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ELEBEATMusicEditer
{
    public class BPM
    {
        //基本BPM
        public double BaseBPM;

        //コンストラクタ
        public BPM()
        {
            BaseBPM = 60;
        }

        //BPM変化クラス
        public class BPMChange
        {
            public double BPM;
            public double BeatForChange;
            public double TimeForChange;
        }

        public List<BPMChange> BPMChangeList = new List<BPMChange>();

        //リセット
        public void BPMReset()
        {
            BPMChangeList.Clear();
        }

        //ある時刻のBPMを得る
        public double GetBPM(int TimeForBPM)
        {
            //戻り値を基準BPMで初期化
            double ReturnBPM = BaseBPM;
            CalcTimeFromBeat();
            for (int i = 0; i < BPMChangeList.Count; i++)
            {
                if (BPMChangeList[i].TimeForChange < TimeForBPM)
                {
                    ReturnBPM = BPMChangeList[i].BPM;
                }
            }
            return (ReturnBPM);
        }

        //拍数からBPMChangeList中のすべての時間を計算する
        public void CalcTimeFromBeat()
        {
            int ListCount = 0;
            if (BPMChangeList.Count == 0)
            {
                return;
            }
            else
            {
                double TempTime = 0;
                while (ListCount < BPMChangeList.Count)
                {
                    if (ListCount == 0)
                    {
                        TempTime += (60000.0 / BaseBPM) / 8.0 * BPMChangeList[0].BeatForChange;
                    }
                    else
                    {
                        TempTime += (60000.0 / BPMChangeList[ListCount - 1].BPM) / 8.0 * (BPMChangeList[ListCount].BeatForChange - BPMChangeList[ListCount - 1].BeatForChange);
                    }
                    BPMChangeList[ListCount].TimeForChange = (int)TempTime;
                    ListCount++;
                }
            }
        }

        //BPMCangeのListのSortを使うとき。BeatForTimeだけで判断+
        public class BPMChangeComparer : IComparer<BPMChange>
        {
            public int Compare(BPMChange x, BPMChange y)
            {
                if (x.BeatForChange < y.BeatForChange)
                {
                    return -1;
                }
                else if (x.BeatForChange > y.BeatForChange)
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
