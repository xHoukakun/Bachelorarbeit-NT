using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Bachelorarbeit_NT
{
    public partial class Form1 : Form
    {       
        static int cpus = 2; //Anzahl cpus
        static double n1 = 10 * 10e17;  //Anzahl der Werte die berechnet werden sollen
        static ulong n = Convert.ToUInt64(n1);
        static decimal GesamtIntervall = 40M;  //Die Länge des GesamtIntervalls 
        static ulong AnzahlIntervalle = 4_000;  //Anzahl der Teilintervalle 
        CancellationTokenSource ctsrc = new CancellationTokenSource();  //CancellationToken um die Threads zu beenden falls nötig
        public static bool saved = false;    //Sicheres Beenden
        static List<ulong> StatisticRootOfTwo = new List<ulong>();         //Statistik für  Wurzel2
        static List<ulong> StatisticEuler = new List<ulong>();  //Statistik für Euler 
        static List<ulong> StatisticZeta3 = new List<ulong>();  //Statistik für Zeta3
        static bool Wurzel2aufgerufen = false;                  //Wenn das erste mal die Statistik aufgerufen wurde
        static bool Euleraufgerufen = false;                    
        static bool Zeta3aufgerufen = false;
        static ulong uZeta3;                            //wie viele Zeta3 Werte gibt es
        static ulong uRootOfTwo;
        static ulong uEuler;
        static decimal minR;                        //Diese Variablnen stehen für das Minimum und das Maximum Folgende min für Minimum und der Buchstabe gibt an welches alpha
        static decimal maxR;
        static decimal minE;
        static decimal maxE;
        static decimal minZ;
        static decimal maxZ;
        private bool delete = false;  //Falls die Dateien Gelöscht werden sollen

        public Form1()
        {
            InitializeComponent();   //Visual studio code
            Series series = new Series();       //Relevant für die Chart
            button1.Enabled = true;             // aktiviert die Button 1 2 und 3 diese sind für das wechseln der Statistik verantwortlich. 
            button2.Enabled = true;
            button3.Enabled = true;
            series.ChartType = SeriesChartType.Column;    
            var Start = new Starter(cpus, n, Convert.ToInt32(AnzahlIntervalle), GesamtIntervall, ctsrc.Token);   //hier wird die Starter Klasse aufgerufen


        }

        private void Form1_Load(object sender, EventArgs e)
        {



        }
        public static void Change_Text()
        {

        }
        public static void save()
        {
            saved = true;  //Falls alles abgespeichert wurde

        }
        public static void Liste_RootOfTwo(List<ulong> Liste, decimal Minimum, decimal Maximum) 
        {
            minR = Minimum;   //gebe min und Max wieder
            maxR = Maximum;   
            ulong Zähle = 0;
            if (Wurzel2aufgerufen == false) //falls es das erste Mal aufgerufen wurde
            {

                Wurzel2aufgerufen = true;                    
                for (int i = 0; i < Liste.Count(); i++)  //Die Liste Befüllen
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
        public static void Liste_Euler(List<ulong> Liste, decimal Minimum, decimal Maximum)
        {
            minE = Minimum;
            maxE = Maximum;
            ulong Zähle = 0;
            if (Euleraufgerufen == false)
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
        public static void Liste_Zeta3(List<ulong> Liste, decimal Minimum, decimal Maximum)
        {
            minZ = Minimum;  
            maxZ = Maximum;
            ulong Zähle = 0;
            if (Zeta3aufgerufen == false)
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

            Delta = GesamtIntervall / Convert.ToDecimal(AnzahlIntervalle);
            chart1.Annotations.Clear();
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
            chart1.ChartAreas[0].AxisY.Maximum = Math.Ceiling(Convert.ToDouble(max * 1.10d));
            My_Text_Annotation(uRootOfTwo, maxR, minR);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ulong max = 0;
            decimal Delta;

            Delta = GesamtIntervall / Convert.ToDecimal(AnzahlIntervalle);
            chart1.Annotations.Clear();
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
            chart1.ChartAreas[0].AxisY.Maximum = Math.Ceiling(Convert.ToDouble(max * 1.10d));

            My_Text_Annotation(uZeta3, maxZ, minZ);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            decimal Delta;
            ulong max = 0;
            Delta = GesamtIntervall / Convert.ToDecimal(AnzahlIntervalle);
            chart1.Annotations.Clear();
            chart1.Series.Clear();
            Series series = new Series();
            series.ChartType = SeriesChartType.Column;
            series.Name = "Euler";
            chart1.Series.Add(series);
            for (int i = 0; i < StatisticEuler.Count(); i++)
            {
                chart1.Series["Euler"].Points.AddXY(Convert.ToDecimal(i) * Delta, StatisticEuler[i]);   //Füge Punkte zum Diagramm hinzu
                max = Math.Max(StatisticEuler[i], max);                     //laufendes Maximum
            }
            chart1.ChartAreas[0].AxisY.Maximum = Math.Ceiling(Convert.ToDouble(max * 1.10d));  //skaliere das Diagram         
            My_Text_Annotation(uEuler, maxE, minE);    //Text annotation
        }

        private void Anzeige_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void klAbstand_Click(object sender, EventArgs e)
        {

        }

        private void grAbstand_Click(object sender, EventArgs e)
        {

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }
        /// <summary>
        /// Chart als PNG speichern 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|PNG files (*.png)|*.png|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    
                    chart1.SaveImage(myStream, ChartImageFormat.Png); //speichere die Chart als png
                    myStream.Close();  
                }
            }
        }
        /// <summary>
        ///Hier wird nur Der Text an die Grafik geheftet 
        /// </summary>
        /// <param name="Anzahl">Wie viele N wurden Berechnet</param>
        /// <param name="Maximum">Maximum</param>
        /// <param name="Minimum">Minimum</param>
        private void My_Text_Annotation(ulong Anzahl, decimal Maximum, decimal Minimum)
        {
            string Hilfsstring = "N:";
            TextAnnotation CAT1 = new TextAnnotation();
            CAT1.Text = Hilfsstring;
            CAT1.AnchorX = 93;
            CAT1.AnchorY = 20;
            chart1.Annotations.Add(CAT1);
            Hilfsstring = "Maximum:";
            TextAnnotation CAT2 = new TextAnnotation();
            CAT2.Text = Hilfsstring;
            CAT2.AnchorX = 93;
            CAT2.AnchorY = 30;
            chart1.Annotations.Add(CAT2);
            Hilfsstring = "Minimum:";
            TextAnnotation CAT3 = new TextAnnotation();
            CAT3.Text = Hilfsstring;
            CAT3.AnchorX = 93;
            CAT3.AnchorY = 40;
            chart1.Annotations.Add(CAT3);
            Hilfsstring = Convert.ToString(Anzahl);
            TextAnnotation CAT12 = new TextAnnotation();
            CAT12.Text = Hilfsstring;
            CAT12.AnchorX = 93;
            CAT12.AnchorY = 25;
            chart1.Annotations.Add(CAT12);
            Hilfsstring = Convert.ToString(Maximum);
            TextAnnotation CAT22 = new TextAnnotation();
            CAT22.Text = Hilfsstring;
            CAT22.AnchorX = 93;
            CAT22.AnchorY = 35;
            chart1.Annotations.Add(CAT22);
            Hilfsstring = Convert.ToString(Minimum);
            TextAnnotation CAT32 = new TextAnnotation();
            CAT32.Text = Hilfsstring;
            CAT32.AnchorX = 93;
            CAT32.AnchorY = 45;
            chart1.Annotations.Add(CAT32);
        }

        private void bDelete_Click(object sender, EventArgs e)
        {
            delete = true;
            this.Dispose();
        }
    }
}
