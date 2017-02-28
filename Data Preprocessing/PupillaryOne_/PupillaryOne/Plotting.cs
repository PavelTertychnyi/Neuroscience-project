using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace PupillaryOne
{
    public static class Plotting
    {
        public static void GeneratePlot(Trial trial, string dir)
        {

            using (var ch = new Chart())
            {
                ch.ChartAreas.Add(new ChartArea());
                var s2 = new Series() { ChartType = SeriesChartType.Line, Color = Color.Red };

                trial.Frames.ForEach(i => s2.Points.AddXY(i.Timestamp, i.D));

                ch.Series.Add(s2);

                if (trial.Cue != null)
                {
                    ch.Titles.Add(new Title() { Text = trial.Cue.Item2, Font = new Font(FontFamily.GenericSansSerif, 24) });

                    var @event = new Series() { ChartType = SeriesChartType.Line, BorderWidth = 4, Color = Color.Green };
                    @event.Points.AddXY(trial.Cue.Item1, 2000);
                    @event.Points.AddXY(trial.Cue.Item1 + 1, 3001);
                    ch.Series.Add(@event);
                }

                if (trial.Stimulus != null)
                {
                    var stimulus = new Series() { ChartType = SeriesChartType.Line, BorderWidth = 4, Color = Color.Orange };
                    stimulus.Points.AddXY(trial.Stimulus.Item1, 2000);
                    stimulus.Points.AddXY(trial.Stimulus.Item1, 3001);
                    ch.Series.Add(stimulus);
                }



                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string fname = dir + trial.Number + ".jpg";
                if (File.Exists(fname))
                    File.Delete(fname);



                var outputStream = File.Create(fname);
                ch.Width = 1600;
                ch.Height = 1200;
                //ch.Legends.Add(new Legend() { Title = trial.Number.ToString() });
                ch.SaveImage(outputStream, ChartImageFormat.Jpeg);
            }
        }

        public static void GenerateSomethingPlot(IEnumerable<Tuple<double, double>> points, string fname)
        {

            using (var ch = new Chart())
            {
                ch.ChartAreas.Add(new ChartArea());
                var s2 = new Series() { ChartType = SeriesChartType.Line, Color = Color.Red };

                points.ToList().ForEach(i => s2.Points.AddXY(i.Item1, i.Item2));

                ch.Series.Add(s2);
                
                var outputStream = File.Create(fname);
                ch.Width = 1600;
                ch.Height = 1200;

                ch.SaveImage(outputStream, ChartImageFormat.Jpeg);
            }
        }

        public static void GenerateAvgPlot(Trial trial, string dir, string name)
        {
            int median = (int)trial.Frames.Where(i=> i.Timestamp > trial.Cue.Item1 && i.Timestamp < trial.Stimulus.Item1).Average(i => i.D);
            using (var ch = new Chart())
            {
                var ca = new ChartArea();
                ch.ChartAreas.Add(ca);
                var s1 = new Series() { ChartType = SeriesChartType.Line, Color = Color.Red };

                trial.Frames.ForEach(i => s1.Points.AddXY(i.Timestamp, i.D));
                ch.Series.Add(s1);

                var @event = new Series() { ChartType = SeriesChartType.Line, BorderWidth = 4, Color = Color.Green };
                @event.Points.AddXY(trial.Cue.Item1, 0);
                @event.Points.AddXY(trial.Cue.Item1 + 1, 1);
                ch.Series.Add(@event);

                var stimulus = new Series() { ChartType = SeriesChartType.Line, BorderWidth = 4, Color = Color.Orange };
                stimulus.Points.AddXY(trial.Stimulus.Item1, 0);
                stimulus.Points.AddXY(trial.Stimulus.Item1, 1);
                ch.Series.Add(stimulus);

                ch.Titles.Add(new Title() { Text = name, Font = new Font(FontFamily.GenericSansSerif, 24) });

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string fname = dir + "avg_" + name + ".jpg";
                if (File.Exists(fname))
                    File.Delete(fname);

                var outputStream = File.Create(fname);
                ch.Width = 1600;
                ch.Height = 1200;
                
                ch.SaveImage(outputStream, ChartImageFormat.Jpeg);
            }
        }

    }
}
