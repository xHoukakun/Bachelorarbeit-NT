using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace Bachelorarbeit_NT
{
    public partial class Form1 : Form
    {
        string connectDB = Application.StartupPath;

        static int cpus = 4;
        static double n1 = 10 * 10e17;
        static ulong n = Convert.ToUInt64(n1);
        CancellationTokenSource ctsrc = new CancellationTokenSource();
        public static bool saved = false;
        static List<ulong> StatisticRootOfTwo = new List<ulong>();
        static List<ulong> StatisticEuler = new List<ulong>();
        static List<ulong> StatisticZeta3 = new List<ulong>();
        static bool Wurzel2aufgerufen = false;
        static bool Euleraufgerufen = false;
        static bool Zeta3aufgerufen = false;
        static ulong uZeta3;
        static ulong uRootOfTwo;
        static ulong uEuler;
        public Form1()
        {
            InitializeComponent();
            Series series = new Series();
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            series.ChartType = SeriesChartType.Column;
            var Start = new Starter(cpus, n, 1_000,ctsrc.Token);
         

        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
            

        }
        public static void Change_Text()
        {

        }
        public static void save()
        {
            saved = true;

        }
        public static void Liste_RootOfTwo(List<ulong> Liste)
        {

            ulong Zähle = 0;
            if(Wurzel2aufgerufen==false)
            {
               
                Wurzel2aufgerufen = true;
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticRootOfTwo.Add(Liste[i]);

                    Zähle = Zähle + Liste[i];
                }
            }
            else
            {
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticRootOfTwo[i] = Liste[i];

                    Zähle = Zähle + Liste[i];
                }
            }
            uRootOfTwo = Zähle;
           
            Console.WriteLine("RootOfTwo gesamt: {0}", Zähle);
           
        }
        public static void Liste_Euler(List<ulong> Liste)
        {
            ulong Zähle = 0;
            if(Euleraufgerufen==false)
            {
                Euleraufgerufen = true;
            
                for (int i = 0; i < Liste.Count(); i++)
                {
                StatisticEuler.Add(Liste[i]);
                Zähle = Zähle + Liste[i];
                }
            }
            else
            {
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticEuler[i] = Liste[i];
                    Zähle = Zähle + Liste[i];
                }
            }
            uEuler = Zähle;
            Console.WriteLine("Euler gesamt: {0}", Zähle);
        }
        public static void Liste_Zeta3(List<ulong> Liste)
        {
            ulong Zähle = 0;
           if(Zeta3aufgerufen==false)
            {
                Zeta3aufgerufen = true;
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticZeta3.Add(Liste[i]);
                    Zähle = Zähle + Liste[i];
                }
           }
            else
            {
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticZeta3[i] = Liste[i];
                    Zähle = Zähle + Liste[i];
                }

            }

            uZeta3 = Zähle;
            Console.WriteLine("Zeta3 gesamt: {0}", Zähle);
        }
        private void Cancel()
        {
            
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ulong max = 0;
            decimal Delta;
            decimal GesamtIntervall = 10M;
            Delta = GesamtIntervall / Convert.ToDecimal(1_000);
            chart1.Series.Clear();
            Series series = new Series();
            series.ChartType = SeriesChartType.Column;
            series.Name = "RootOfTwo";
            chart1.Series.Add(series);
            for (int i = 0; i < StatisticRootOfTwo.Count(); i++)
            {
                chart1.Series["RootOfTwo"].Points.AddXY(Convert.ToDecimal(i) * Delta, StatisticRootOfTwo[i]);
                max = Math.Max(StatisticRootOfTwo[i], max);
            }
            chart1.ChartAreas[0].AxisY.Maximum = max;
            Anzeige.Text = Convert.ToString(uRootOfTwo);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ulong max = 0;
            decimal Delta;
            decimal GesamtIntervall = 10M;
            Delta = GesamtIntervall / Convert.ToDecimal(1_000);
            chart1.Series.Clear();
            Series series = new Series();
            series.ChartType = SeriesChartType.Column;
            series.Name = "Zeta3";
            chart1.Series.Add(series);
            for (int i = 0; i < StatisticZeta3.Count(); i++)
            {
                chart1.Series["Zeta3"].Points.AddXY(Convert.ToDecimal(i) * Delta, StatisticZeta3[i]);
                max = Math.Max(StatisticZeta3[i], max);
            }
            chart1.ChartAreas[0].AxisY.Maximum = max;
            Anzeige.Text = Convert.ToString(uZeta3);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            decimal Delta;
            decimal GesamtIntervall = 10M;
            ulong max = 0;
            Delta = GesamtIntervall / Convert.ToDecimal(1_000);
            chart1.Series.Clear();
            Series series = new Series();
            series.ChartType = SeriesChartType.Column;
            series.Name = "Euler";
            chart1.Series.Add(series);
            for (int i = 0; i < StatisticEuler.Count(); i++)
            {
                chart1.Series["Euler"].Points.AddXY(Convert.ToDecimal(i) * Delta, StatisticEuler[i]);
                max = Math.Max(StatisticEuler[i], max);
            }
            chart1.ChartAreas[0].AxisY.Maximum = max;
            Anzeige.Text = Convert.ToString(uEuler);
        }

        private void Anzeige_Click(object sender, EventArgs e)
        {

        }
    }
}
