using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PupillaryOne
{
    public class Frame : IComparable<Frame>
    {
        public int Timestamp { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float D { get; set; }
        public float InterpolatedD { get; set; }
        public bool Abnormal { get; set; }
        public bool WithoutData
        {
            get { return Abnormal && !Interpolated; }
            set { Abnormal = value; }
        }
        public bool Interpolated { get; set; }

        public int CompareTo(Frame other)
        {
            return Timestamp.CompareTo(other?.Timestamp);
        }

        public Frame Clone()
        {
            return (Frame)this.MemberwiseClone();
        }
    }

    public class Trial
    {
        public int Number { get; set; }
        public int StartTimestamp { get; set; }
        public int EndTimestamp { get; set; }
        public List<Frame> Frames { get; set; }
        public float[] Ds => Frames.Select(i => i.D).ToArray();
        public Tuple<int, string> Cue { get; set; }
        public Tuple<int, string> Stimulus { get; set; }
        public Trial Clone()
        {
            var t = (Trial)this.MemberwiseClone();
            t.Frames = Frames.Select(i => i.Clone()).ToList();
            return t;
        }
    }

}
