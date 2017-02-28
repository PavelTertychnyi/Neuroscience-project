using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PupillaryOne
{
    public class AvgCalculator
    {
        int start = 0;
        Trial avgAcc = null;
        string avgCue;
        int rcnt = 1;

        public AvgCalculator(string cue)
        {
            avgCue = cue;
        }

        public void Add(Trial p)
        {
            if (p.Cue.Item2 == avgCue)
                if (avgAcc == null)
                {
                    avgAcc = p.Clone();
                }
                else
                {
                    for (int i = 0; i < p.Frames.Count; i++)
                    {
                        avgAcc.Frames[i].D += p.Frames[i].D;
                    }

                    ++rcnt;
                }
        }

        public void SaveGraph(string dir, string name = "")
        {
            avgAcc.Frames.ForEach(x => x.D /= rcnt);
            Plotting.GenerateAvgPlot(avgAcc, dir, name+avgCue);
            avgAcc = null;
            rcnt = 0;
        }
    }
}
