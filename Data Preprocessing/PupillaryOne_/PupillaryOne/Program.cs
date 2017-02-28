using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PupillaryOne
{
    class Program
    {
        static void Main(string[] args)
        {
            Diff();
        }

        private static void WidenPreCue(List<Trial> trials)
        {
            int last = 500;
            for (int i = 1; i < trials.Count; ++i)
            {
                var prev = trials[i - 1].Frames.Skip(trials[i - 1].Frames.Count - last).Select(x=>x.Clone());
                trials[i].Frames.InsertRange(0, prev);
                int j = 0;
                trials[i].Cue = new Tuple<int, string>(trials[i].Cue.Item1 - trials[i].StartTimestamp + last*2, trials[i].Cue.Item2);
                trials[i].Stimulus = new Tuple<int, string>(trials[i].Stimulus.Item1 - trials[i].StartTimestamp + last*2, trials[i].Stimulus.Item2);

                foreach (var k in trials[i].Frames)
                {
                    k.Timestamp = j;
                    j += 2;
                }
            }
        }

        static void BeforeGraph()
        {
            string prefix = "PLR";
            for (int j = 110; j < 137; j++)
            {
                Log(j.ToString());
                string file = prefix + j;
                var trials = Parse($@"C:\Users\Vasiliy\Documents\NeuroScience\Pupillary\Technical\Data\ASC\{file}.asc", 0);
                foreach (var trial in trials.Take(5))
                {
                    var p = RemoveBlinks(trial);
                    Plotting.GeneratePlot(p, $"before/{j}/");
                }
            }
        }

        static void Convert()
        {
            string prefix = "PLR";
            for (int j = 110; j < 137; j++)
            {
                Log(j.ToString());
                string file = prefix + j;
                var trials = Parse($@"C:\Users\Vasiliy\Documents\NeuroScience\Pupillary\Technical\Data\ASC\{file}.asc", 5);
                foreach (var trial in trials.Skip(9))
                {
                    var p = RemoveBlinks(trial);
                    //Normalize(p);
                    SaveDGraphData(p, $"csv/PLR{j}_trial{trial.Number}_data.csv");
                    SaveDGraphInfo(p, $"csv/PLR{j}_trial{trial.Number}_info.csv");
                }
            }
        }

        class DiffAvgInfo
        {
            public int Number { get; set; }
            public string Cue { get; set; }
            public float AvgBefore { get; set; }
            public float AvgDuring { get; set; }
            public float Diff => AvgDuring - AvgBefore;

            public override string ToString()
            {
                return $"{Number,3} {Cue,11} {AvgBefore,11} {AvgDuring,11} {AvgDuring - AvgBefore,11}";
            }
        }

        static void DiffAvg()
        {
            string prefix = "PLR";
            var outFile = "DifferenceBetweenAverageDiameterBeforeAndAfterCue.txt";
            for (int j = 110; j < 137; j++)
            {
                //j = 136;

                List<DiffAvgInfo> diff = new List<DiffAvgInfo>();

                Log(j.ToString());
                string file = prefix + j; // 110-136
                string dir = file + "/";
                Log("Parsing..");
                var trials = Parse($@"C:\Users\Vasiliy\Documents\NeuroScience\Pupillary\Technical\Data\ASC\{file}.asc", 5);

                foreach (var i in trials)
                {
                    var p = RemoveBlinks(i);

                    var avgBefore = p.Frames.Where(x => x.Timestamp < p.Cue.Item1).Average(x => x.D);
                    var avgDuring = p.Frames.Where(x => x.Timestamp > p.Cue.Item1 && x.Timestamp < p.Stimulus.Item1).Average(x => x.D);

                    diff.Add(new DiffAvgInfo()
                    {
                        Number = p.Number,
                        Cue = p.Cue.Item2,
                        AvgBefore = avgBefore,
                        AvgDuring = avgDuring,
                    });
                }


                File.AppendAllText(outFile, $"----------{j}----------\n");
                foreach (var k in diff.GroupBy(x => x.Cue).OrderBy(x => x.Key))
                {
                    File.AppendAllText(outFile, $"{k.Key} {k.Average(x => x.Diff)}\n");
                }

            }

            Log("Done");
        }

        class DiffInfo
        {
            public int Number { get; set; }
            public string Cue { get; set; }
            public float Before { get; set; }
            public float After { get; set; }
            public float Diff => After - Before;

            public override string ToString()
            {
                return $"{Number,3} {Cue,11} {Before,11} {After,11} {After - Before,11}";
            }
        }

        static void Diff()
        {
            float a = 0, b = 0, c = 0, d = 0;
            string prefix = "PLR";
            var outFile = "DifferenceBetweenBeforeCueAndBeforeStimulus.txt";
            for (int j = 110; j < 137; j++)
            {
                //j = 136;

                List<DiffInfo> diff = new List<DiffInfo>();

                Log(j.ToString());
                string file = prefix + j; // 110-136
                string dir = file + "/";
                Log("Parsing..");
                var trials = Parse($@"C:\Users\Vasiliy\Documents\NeuroScience\Pupillary\Technical\Data\ASC\{file}.asc", 5);

                foreach (var i in trials)
                {
                    var p = RemoveBlinks(i);

                    var before = p.Frames.First(x => x.Timestamp > p.Cue.Item1).D;
                    var after = p.Frames.First(x => x.Timestamp > p.Stimulus.Item1).D;

                    diff.Add(new DiffInfo()
                    {
                        Number = p.Number,
                        Cue = p.Cue.Item2,
                        Before = before,
                        After = after
                    });
                }


                Log($"----------{j}----------");//, outFile);
                foreach (var k in diff.GroupBy(x => x.Cue).OrderBy(x => x.Key))
                {
                    if (k.Key == "left")
                    {
                        a = k.Average(x => x.Diff);
                    }
                    if (k.Key == "right")
                    {
                        b = k.Average(x => x.Diff);
                    }

                    Log($"{k.Key} {k.Average(x => x.Diff)}");//, outFile);
                }

                if (a > b) //left>right; left -- ^, right -- v
                    ++d;
                else
                    --d;
                a = b = c = 0;
            }

            Console.WriteLine(d);
            Log("Done");
        }

        static void Do()
        {
            string prefix = "PLR";
            string imgdir = "";
            //var leftA = new AvgCalculator("left");
            //var rightA = new AvgCalculator("right");
            //var rndA = new AvgCalculator("rndCue");

            for (int j = 110; j < 137; j++)
            {
                j = 136;
                var left = new AvgCalculator("left");
                var right = new AvgCalculator("right");
                var rnd = new AvgCalculator("rndCue");

                Log(j.ToString());
                string file = prefix + j; // 110-136
                string dir = file + "/";
                imgdir = dir + "img/";
                Log("Parsing..");
                var trials = Parse($@"C:\Users\Vasiliy\Documents\NeuroScience\Pupillary\Technical\Data\ASC\{file}.asc", 5);

                Log("Processing and saving");
                foreach (var i in trials)
                {
                    //Log($"Saving graph for {i.Number}");
                    //SaveDGraphData(i, dir);
                    //Log($"Removing blinks for {i.Number}");
                    var p = RemoveBlinks(i);
                    //Normalize(p);

                    // avg person
                    left.Add(p);
                    right.Add(p);
                    rnd.Add(p);

                    // avg all
                    //leftA.Add(p);
                    //rightA.Add(p);
                    //rndA.Add(p);

                    //Log($"Saving graph for {i.Number} without blinks");
                    //SaveDGraphData(p, "csv/", "i");

                    //Plotting.GeneratePlot(p, imgdir);
                    //break;
                }

                left.SaveGraph(imgdir);
                right.SaveGraph(imgdir);
                rnd.SaveGraph(imgdir);
            }

            //leftA.SaveGraph("avg/");
            //rightA.SaveGraph("avg/");
            //rndA.SaveGraph("avg/");

            Log("Done");

            //trials.ForEach(i =>
            //    Log(
            //        $"#{i.Number,2}   Duration: { i.EndTimestamp - i.StartTimestamp,5} Cue: {i.Cue.Item1 - i.StartTimestamp,5} Stimulus: {i.Stimulus.Item1 - i.StartTimestamp,5} " +
            //        $"BeforeCue: { i.Cue.Item1 - i.StartTimestamp,5} Cue-Stimulus interval: {i.Stimulus.Item1 - i.Cue.Item1,5} AfterStimulus: {i.EndTimestamp - i.Stimulus.Item1,5}\r\n"
            //        ));

            //trials.ForEach(i =>
            //    Log($"#{i.Number},{i.Cue.Item1 - i.StartTimestamp},{i.Stimulus.Item1 - i.StartTimestamp},{i.EndTimestamp - i.StartTimestamp}\n", "forPlotting"));

            //trials.ForEach(i =>
            //    Log($"#{i.Number} #{i.Frames.Count()} diff:{(i.EndTimestamp - i.StartTimestamp) / 2} d2:{(i.EndTimestamp - i.StartTimestamp) / 2 - i.Frames.Count()}"));
        }

        private static void SaveDGraphData(Trial trial, string path)
        {
            string dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.Delete(path);

            StringBuilder sb = new StringBuilder();
            float prev = -1;
            int first = 0;
            foreach (var i in trial.Frames)
            {
                if (prev == -1)
                {
                    prev = i.D;
                    first = i.Timestamp;
                }

                sb.Append($"{i.Timestamp - first},{i.D}\n");
                prev = i.D;
            }

            File.WriteAllText(path, sb.ToString());
        }

        private static void SaveDGraphInfo(Trial trial, string path)
        {
            string dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.Delete(path);

            int first = trial.Frames.First().Timestamp;
            string info = $"{trial.Cue.Item1 - first},{trial.Cue.Item2},{trial.Stimulus.Item1 - first},{trial.Stimulus.Item2}\n";

            File.WriteAllText(path, info);
        }

        static Stopwatch sw = new Stopwatch();
        static void Start()
        {
            sw.Restart();
        }
        static void Stop(string comment = "")
        {
            sw.Stop();
            Console.WriteLine($"{comment} {sw.ElapsedMilliseconds}ms");
        }

        public static void Normalize(Trial trial)
        {
            int stimulusi = trial.Frames.FindIndex(x => x.Timestamp == (trial.Stimulus.Item1 & (~1)));
            trial.Frames = trial.Frames.Skip(stimulusi - 1950).Take(4000).ToList();
            float min = trial.Frames.Min(i => i.D);
            float max = trial.Frames.Max(i => i.D);
            float delta = max - min;
            trial.Frames.ForEach(i => i.D = (i.D - min) / delta);
        }

        private static List<Trial> Parse(string filename, int skip)    // first trials are different experiment
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            var trials = new List<Trial>(60);

            string startPattern = @"START\s+(\d+)";                     // example: 'START	899724 	RIGHT	SAMPLES	EVENTS'
            string endPattern = @"END\s+(\d+)";                         //          'END	909255 	SAMPLES	EVENTS	RES	  27.02	  27.96'
            var itemPattern = @"^(\d+)\s+(\d+\.\d)\s+(\d+\.\d)\s+(\d+\.\d)\s+";  // '16145510	  541.7	  441.7	 6246.0	...'
            var eventPattern = @"MSG\s+(\d+)\s+(left|right|rndCue)";    //          'MSG	16621534 right'
            var stimulusPattern = @"MSG\s+(\d+)\s+(white|black)";       //          'MSG	16625341 black'
            Regex startRgx = new Regex(startPattern);
            Regex endRgx = new Regex(endPattern);
            Regex itemRgx = new Regex(itemPattern);
            Regex eventRgx = new Regex(eventPattern);
            Regex stimulusRgx = new Regex(stimulusPattern);

            var lines = File.ReadAllLines(filename);    // ~200ms with ssd

            int num = 0;
            List<Frame> trial = null;
            int start = -1;
            int last = -1;
            int end = -1;
            Tuple<int, string> cue = null;
            Tuple<int, string> stimulus = null;

            foreach (var line in lines)
            {
                if (skip > 0)
                {
                    if (startRgx.IsMatch(line))
                        --skip;

                    continue;
                }

                if (trial == null)
                {
                    var match = startRgx.Match(line);
                    if (match.Success)
                    {

                        start = int.Parse(match.Groups[1].Value);
                        last = start;
                        trial = new List<Frame>();
                        cue = null;
                        stimulus = null;
                        end = -1;
                    }
                }
                else
                {
                    var match = itemRgx.Match(line);
                    if (match.Success)              // data
                    {

                        int time = int.Parse(match.Groups[1].Value) & (~1);
                        if (time % 2 == 1)
                        {
                            Console.WriteLine("odd time");
                            continue;
                        }

                        CheckForOmittedFrames(trial, start, time);
                        trial.Add(new Frame()
                        {
                            Timestamp = time,
                            X = float.Parse(match.Groups[2].Value),
                            Y = float.Parse(match.Groups[3].Value),
                            D = float.Parse(match.Groups[4].Value)
                        });
                    }
                    else if ((match = eventRgx.Match(line)).Success)        // event (number -- l,r,rnd)
                    {
                        int time = int.Parse(match.Groups[1].Value);
                        string value = match.Groups[2].Value;
                        cue = new Tuple<int, string>(time, value);
                    }
                    else if ((match = stimulusRgx.Match(line)).Success)     // stimulus (black/white)
                    {
                        int time = int.Parse(match.Groups[1].Value);
                        string value = match.Groups[2].Value;
                        stimulus = new Tuple<int, string>(time, value);
                    }
                    else if ((match = endRgx.Match(line)).Success)          // END
                    {
                        int time = int.Parse(match.Groups[1].Value);
                        CheckForOmittedFrames(trial, start, time);
                        end = time;
                        trials.Add(new Trial()
                        {
                            Number = ++num,
                            StartTimestamp = start,
                            EndTimestamp = end + 1,       // in eyelink end time is 1ms after last frame, now it will be 2ms after (for consistency (as we use 500 fps difference in timestamp between all the frames is 2))
                            Cue = cue,
                            Stimulus = stimulus,
                            Frames = trial
                        });

                        trial = null;
                    }
                }
            }

            return trials;
        }

        const ushort marginBeforeBlink = 6;
        const ushort marginAfterBlink = 10;
        const ushort marginInsideBlink = marginBeforeBlink + marginAfterBlink + 5; // min. +0
        const float velocityCutoff = 20f;  // min. 20

        // check coordinates; more than half looking away -- discart
        private static Trial RemoveBlinks(Trial trial)
        {
            trial = trial.Clone();
            var frames = trial.Frames;

            if (frames == null || frames.Count() == 0)
                throw new ArgumentException("is null or empty");

            frames = frames.ToList();

            Func<int, int> checkRightBoundary = x => Math.Min(frames.Count - 2, x);  // to avoid index out of bound
            Func<int, int> checkLeftBoundary = x => Math.Max(1, x);

            if (frames[0].WithoutData)  // if record starts with blink
            {
                for (int i = 0; i <= marginBeforeBlink; i++)
                {
                    frames[i].WithoutData = true;
                }

                int goodIndex = frames.FindIndex(i => !i.WithoutData);
                if (!frames[checkRightBoundary(goodIndex + marginAfterBlink)].WithoutData)
                    goodIndex = checkRightBoundary(goodIndex + marginAfterBlink);

                var good = frames[goodIndex];

                for (int i = 0; i < goodIndex; i++)
                {
                    frames[i].D = good.D;
                    frames[i].X = good.X;
                    frames[i].Y = good.Y;
                    frames[i].Interpolated = true;
                }
            }

            if (frames.Last().WithoutData)  // if record ends with blink
            {
                for (int i = frames.Count - marginAfterBlink - 1; i < frames.Count; i++)
                {
                    frames[i].WithoutData = true;
                }

                int goodIndex = frames.FindLastIndex(i => !i.WithoutData);
                if (!frames[checkLeftBoundary(goodIndex - marginBeforeBlink)].WithoutData)
                    goodIndex = checkLeftBoundary(goodIndex - marginBeforeBlink);

                var good = frames[goodIndex];

                for (int i = goodIndex; i < frames.Count; ++i)
                {
                    frames[i].D = good.D;
                    frames[i].X = good.X;
                    frames[i].Y = good.Y;
                    frames[i].Interpolated = true;
                }
            }

            //  mark as abnormal regions with high dilation velocity
            for (int i = 1; i < frames.Count - 1; i++)
            {
                float velocity = Math.Abs(frames[i].D - frames[i - 1].D);
                if (velocity > velocityCutoff)
                    frames[i].WithoutData = true;
            }

            for (int i = 1; i < frames.Count - marginInsideBlink; i++)       // remove short 'islands' of data inside anomalies (anomaly means started blink)
            {
                if (!frames[i].WithoutData && frames[i - 1].WithoutData)
                {
                    bool ok = true;
                    for (int j = i + 1; j < i + marginInsideBlink; j++)
                        ok = ok && !frames[j].WithoutData;

                    if (!ok)
                        for (int j = i + 1; j < i + marginInsideBlink; j++)
                            frames[j].WithoutData = true;

                }
            }

            // extend boundaries using before/after variables
            for (int i = 1; i < frames.Count; ++i)
            {
                if (frames[i].WithoutData && !frames[i - 1].WithoutData)        //before
                    for (int j = checkLeftBoundary(i - marginBeforeBlink); j < i; ++j)
                        frames[j].WithoutData = true;

                if (!frames[i].WithoutData && frames[i - 1].WithoutData)        //after
                {
                    for (int j = i; j <= checkRightBoundary(i + marginAfterBlink); ++j)
                        frames[j].WithoutData = true;

                    i = checkRightBoundary(i + marginAfterBlink) + 1;
                }
            }

            if (frames.First().WithoutData || frames.Last().WithoutData)
                Console.WriteLine(" check edge values");

            // interpolate values in anomalies
            for (int i = 0; i < frames.Count; ++i)
            {
                if (frames[i].WithoutData)
                {
                    Frame next = null;
                    // get next normal
                    int j;
                    for (j = i; frames[j].WithoutData; ++j) ;
                    next = frames[j];
                    int duration = j - i;
                    for (int k = 0; k < duration; ++k)
                    {
                        var current = frames[k + i];
                        var prev = frames[i];
                        current.Interpolated = true;
                        current.D = prev.D + (next.D - prev.D) * k / ((float)duration);
                    }
                }
            }

            return trial;
        }

        private static void CheckForOmittedFrames(List<Frame> trial, int startTime, int time)   // blinks
        {
            int last = 0;
            if (trial.Count() == 0)
                last = startTime;
            else
                last = trial.Last().Timestamp + 2;

            while (last < time)
            {
                trial.Add(new Frame() { Timestamp = last, WithoutData = true });
                last += 2;
            }
        }

        public static void Log(string s, string path = null)
        {
            if (path == null)
                Console.WriteLine(s);
            else
                File.AppendAllText(path + "\r\n", s);
        }
    }
}
