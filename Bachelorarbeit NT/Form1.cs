using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;
using MathNet.Numerics.Distributions;

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
        static decimal meanZ;                   //Durchschnitt von Zeta3 
        static List<decimal> propR = new List<decimal>();    //Wahrscheinlichkeitsvektoren von den Abständen
        static List<decimal> propE = new List<decimal>();
        static List<decimal> propZ = new List<decimal>();
        static decimal meanE;
        static decimal meanR;
        static decimal Delta;              
        private bool delete = false;  //Falls die Dateien Gelöscht werden sollen
        static List<decimal> DelMinR = new List<decimal>();
        static List<decimal> DelMaxR = new List<decimal>();
        static List<decimal> DelMinE = new List<decimal>();
        static List<decimal> DelMaxE = new List<decimal>();
        static List<decimal> DelMinZ = new List<decimal>();
        static List<decimal> DelMaxZ = new List<decimal>();
       


        public Form1()
        {
            InitializeComponent();   //Visual studio code
            button1.Enabled = true;             // aktiviert die Button 1 2 und 3 diese sind für das wechseln der Statistik verantwortlich. 
            button2.Enabled = true;
            button3.Enabled = true;
            DelLesen();
            var Start = new Starter(cpus, n, Convert.ToInt32(AnzahlIntervalle), GesamtIntervall, ctsrc.Token);   //hier wird die Starter Klasse aufgerufen
            Delta = GesamtIntervall/Convert.ToDecimal(AnzahlIntervalle);
            
        }
        private void DelLesen() //MethodeZumAuslesenDerAbstandsfunktion
        {
            if (File.Exists("EulerMin.txt"))   //Die Gespeicherten Daten Auslesen.
            {


                string line;
                string hile = "EulerMin.txt";
                StreamReader reader = new StreamReader(hile);

                line = reader.ReadLine();
                while (line != null)  //Lese solange die Zeile nicht null ist, man weiß nicht wie Lang die Liste genau ist. Man könnte dies Zwar noch dazu speichern aber das wäre unnötig
                {
                    DelMinE.Add(Convert.ToDecimal(line));
                    line = reader.ReadLine();
                }
                reader.Close();

            }
            if (File.Exists("EulerMax.txt"))   //Die Gespeicherten Daten Auslesen.
            {


                string line;
                string hile = "EulerMax.txt";
                StreamReader reader = new StreamReader(hile);

                line = reader.ReadLine();
                while (line != null)  //Lese solange die Zeile nicht null ist, man weiß nicht wie Lang die Liste genau ist. Man könnte dies Zwar noch dazu speichern aber das wäre unnötig
                {
                    DelMaxE.Add(Convert.ToDecimal(line));
                    line = reader.ReadLine();
                }
                reader.Close();

            }
            if (File.Exists("Zeta3Max.txt"))   //Die Gespeicherten Daten Auslesen.
            {


                string line;
                string hile = "Zeta3Max.txt";
                StreamReader reader = new StreamReader(hile);

                line = reader.ReadLine();
                while (line != null)  //Lese solange die Zeile nicht null ist, man weiß nicht wie Lang die Liste genau ist. Man könnte dies Zwar noch dazu speichern aber das wäre unnötig
                {
                    DelMaxZ.Add(Convert.ToDecimal(line));
                    line = reader.ReadLine();
                }
                reader.Close();

            }
            if (File.Exists("Zeta3Min.txt"))   //Die Gespeicherten Daten Auslesen.
            {


                string line;
                string hile = "Zeta3Min.txt";
                StreamReader reader = new StreamReader(hile);

                line = reader.ReadLine();
                while (line != null)  //Lese solange die Zeile nicht null ist, man weiß nicht wie Lang die Liste genau ist. Man könnte dies Zwar noch dazu speichern aber das wäre unnötig
                {
                    DelMinZ.Add(Convert.ToDecimal(line));
                    line = reader.ReadLine();
                }
                reader.Close();

            }
            if (File.Exists("RootOfTwoMin.txt"))   //Die Gespeicherten Daten Auslesen.
            {


                string line;
                string hile = "RootOfTwoMin.txt";
                StreamReader reader = new StreamReader(hile);

                line = reader.ReadLine();
                while (line != null)  //Lese solange die Zeile nicht null ist, man weiß nicht wie Lang die Liste genau ist. Man könnte dies Zwar noch dazu speichern aber das wäre unnötig
                {
                    DelMinR.Add(Convert.ToDecimal(line));
                    line = reader.ReadLine();
                }
                reader.Close();

            }
            if (File.Exists("RootOfTwoMax.txt"))   //Die Gespeicherten Daten Auslesen.
            {


                string line;
                string hile = "RootOfTwoMax.txt";
                StreamReader reader = new StreamReader(hile);

                line = reader.ReadLine();
                while (line != null)  //Lese solange die Zeile nicht null ist, man weiß nicht wie Lang die Liste genau ist. Man könnte dies Zwar noch dazu speichern aber das wäre unnötig
                {
                    DelMaxR.Add(Convert.ToDecimal(line));
                    line = reader.ReadLine();
                }
                reader.Close();

            }
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
            decimal tmp = 0.0M;
            ulong Zähle = 0;
            if(DelMinR.Count()<1000)
            {
                DelMinR.Add(minR);
                DelMaxR.Add(maxR);
            }
            if (Wurzel2aufgerufen == false) //falls es das erste Mal aufgerufen wurde
            {

                Wurzel2aufgerufen = true;                    
                for (int i = 0; i < Liste.Count(); i++)  //Die Liste Befüllen
                {
                    StatisticRootOfTwo.Add(Liste[i]);                    
                    Zähle = Zähle + Liste[i];
                }
                for(int i = 0;i<Liste.Count();i++)
                {
                    if (Zähle != 0)
                    {
                        propR.Add(Convert.ToDecimal(Liste[i]) / Convert.ToDecimal(Zähle));   //Rechne den Wahrscheinlichkeitsvektor aus
                        tmp += Convert.ToDecimal(i) * propR[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticRootOfTwo[i] = Liste[i];                   
                    Zähle = Zähle + Liste[i];
                   
                }
                for(int i = 0; i < Liste.Count();i++)
                {
                    if (Zähle != 0)
                    {
                        propR[i] = Convert.ToDecimal(Liste[i]) / Convert.ToDecimal(Zähle);
                        tmp += Convert.ToDecimal(i) * propR[i];
                    }
                }
            }
            
            uRootOfTwo = Zähle;
            meanR = tmp;
            

            Console.WriteLine("RootOfTwo gesamt: {0}", Zähle);
            Console.WriteLine("Mean RootOfTwo: {0}", meanR*Delta);

        }
        public static void Liste_Euler(List<ulong> Liste, decimal Minimum, decimal Maximum)
        {
            minE = Minimum;
            maxE = Maximum;
            decimal tmp = 0.0M;
            ulong Zähle = 0;
            if (DelMinE.Count() < 1000)
            {
                DelMinE.Add(minE);
                DelMaxE.Add(maxE);
            }
            if (Euleraufgerufen == false)
            {
                Euleraufgerufen = true;

                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticEuler.Add(Liste[i]);
                    Zähle = Zähle + Liste[i];
                }
                for (int i = 0; i < Liste.Count(); i++)
                {
                    if(Zähle != 0)
                    {
                        propE.Add(Convert.ToDecimal(Liste[i]) / Convert.ToDecimal(Zähle));
                        tmp += Convert.ToDecimal(i) * propE[i];
                    }

                }
            }
            else
            {
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticEuler[i] = Liste[i];
                    Zähle = Zähle + Liste[i];
                }
                for (int i = 0; i < Liste.Count(); i++)
                {
                    if (Zähle != 0)
                    {
                        propE[i] = Convert.ToDecimal(Liste[i]) / Convert.ToDecimal(Zähle);
                        tmp += Convert.ToDecimal(i) * propE[i];
                    }
                }
            }
            uEuler = Zähle;
            meanE = tmp;
            Console.WriteLine("Euler gesamt: {0}", Zähle);
            Console.WriteLine("Mean Euler: {0}", meanE*Delta);
        }
        public static void Liste_Zeta3(List<ulong> Liste, decimal Minimum, decimal Maximum)
        {

            minZ = Minimum;  
            maxZ = Maximum;
            decimal tmp=0;
            ulong Zähle = 0;
            if (DelMinZ.Count() < 1000)
            {
                DelMinZ.Add(minZ);
                DelMaxZ.Add(maxZ);
            }
            if (Zeta3aufgerufen == false)
            {
                Zeta3aufgerufen = true;
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticZeta3.Add(Liste[i]);
                    Zähle = Zähle + Liste[i];

                }
                for (int i = 0; i < Liste.Count(); i++)
                {
                    if (Zähle != 0)
                    {
                        propZ.Add(Convert.ToDecimal(Liste[i]) / Convert.ToDecimal(Zähle));
                        tmp += Convert.ToDecimal(i) * propZ[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < Liste.Count(); i++)
                {
                    StatisticZeta3[i] = Liste[i];
                    Zähle = Zähle + Liste[i];
                }
                for (int i = 0; i < Liste.Count(); i++)
                {
                    if (Zähle != 0)
                    {
                        propZ.Add(Convert.ToDecimal(Liste[i]) / Convert.ToDecimal(Zähle));
                        tmp += Convert.ToDecimal(i) * propZ[i];
                    }
                }
            }
            meanZ = tmp;
            uZeta3 = Zähle;
            Console.WriteLine("Zeta3 gesamt: {0}", Zähle);
            Console.WriteLine("Mean Zeta3: {0}", meanZ*Delta);
        }
        private void Cancel()
        {

        }
        private void chart1_Click(object sender, EventArgs e)
        {

        }
        private void GraphAusgeben(decimal mean, string Name, List<ulong> statistic, ulong N, decimal max1, decimal min1)
        {
            if (mean != 0)   
            {

                decimal max = 0;
                decimal Delta;
                decimal funktionswert;
                Delta = GesamtIntervall / Convert.ToDecimal(AnzahlIntervalle);
                decimal Summe1 = 0;
                decimal Summe2 = 0;

                //Poisson poi = new Poisson(Convert.ToDouble(mean));
                chart1.Annotations.Clear();
                chart1.Series.Clear();
                Series series = new Series();
                series.ChartType = SeriesChartType.Column;
                series.Name = Name;
                chart1.Series.Add(series);
                //Series series1 = new Series();
                //series1.ChartType = SeriesChartType.FastLine;
                //series1.Name = "Poisson";
                //chart1.Series.Add(series1);
                Series series2 = new Series();
                series2.ChartType = SeriesChartType.FastLine;
                series2.Name = "Exponentialverteilung";
                chart1.Series.Add(series2);




                for (int i = 0; i < statistic.Count(); i++)
                {

                    //funktionswert = funktionswert = Convert.ToDecimal(poi.Probability(i)) * Convert.ToDecimal(N);
                    //Summe1 += funktionswert;
                    //chart1.Series["Poisson"].Points.AddXY(i * Delta, funktionswert);
                    funktionswert = expver(1 / (mean), Convert.ToDecimal(i)) * Convert.ToDecimal(N);

                    chart1.Series["Exponentialverteilung"].Points.AddXY(Convert.ToDecimal(i) * Delta, funktionswert);
                    Summe2 += funktionswert;
                    chart1.Series[Name].Points.AddXY(Convert.ToDecimal(i) * Delta, statistic[i]);
                    max = Math.Max(statistic[i], max);
                    

                }

                //Console.WriteLine("Gesamt Poisson: {0}", Summe1);
                Console.WriteLine("Gesamt Exponential: {0}", Summe2);
                chart1.ChartAreas[0].AxisY.Maximum = Math.Ceiling(Convert.ToDouble(max * 1.10m));
                My_Text_Annotation(N, max1, min1);
            }



        }


        private void button1_Click(object sender, EventArgs e)
        {
            GraphAusgeben(meanR, "RootOfTwo", StatisticRootOfTwo, uRootOfTwo, maxR, minR);

     
        }
        private decimal expver(decimal lam, decimal xwert)
        {
            return Convert.ToDecimal(lam * Convert.ToDecimal(Math.Exp(-Convert.ToDouble(lam * xwert))));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GraphAusgeben(meanZ, "Zeta3",StatisticZeta3, uZeta3, maxZ, minZ);
          
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GraphAusgeben(meanE, "Euler", StatisticEuler, uEuler, maxE, minE);
           
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
            delete = true;          //Das wird für das Beenden des Porgramms benötigt
            this.Dispose();
        }

        private void DelEuler_Click(object sender, EventArgs e)
        {if (DelMaxE.Count != 0)
            {
                DeltaNMax(DelMaxE);
                My_Text_Annotation(uEuler, maxE, minE);
            }
        }
        private void DeltaNMax(List<decimal> delMax)
        {
            if (delMax.Count != 0)
            { 

            chart1.Annotations.Clear();
            chart1.Series.Clear();
            Series smax = new Series();
            smax.ChartType = SeriesChartType.FastLine;
            smax.Name = "Max";
            chart1.Series.Add(smax);
            for (int i = 0; i < delMax.Count(); i++)
            {
                chart1.Series["Max"].Points.AddXY(i * 100, delMax[i]);
            }
            chart1.ChartAreas[0].AxisY.Maximum = Convert.ToDouble(delMax[delMax.Count - 1]) * 1.10;
        }
        }
        private void DeltaNMin(List<decimal> delMin)
        {
            if (delMin.Count != 0)
            {
                chart1.Annotations.Clear();
                chart1.Series.Clear();
                Series smin = new Series();
                smin.ChartType = SeriesChartType.FastLine;
                smin.Name = "Min";
                chart1.Series.Add(smin);
                for (int i = 0; i < delMin.Count(); i++)
                {
                    chart1.Series["Min"].Points.AddXY(i * 100, delMin[i]);
                }
                chart1.ChartAreas[0].AxisY.Maximum = Convert.ToDouble(delMin[0]) * 1.10;
            }

        }
        private void DelZeta3_Click(object sender, EventArgs e)
        {
            if (DelMaxZ.Count != 0)
            {
                DeltaNMax(DelMaxZ);
                My_Text_Annotation(uZeta3, maxZ, minZ);
            }
        }

        private void DelRootOfTwo_Click(object sender, EventArgs e)
        {
            if (DelMaxR.Count != 0)
            {
                DeltaNMax(DelMaxR);
                My_Text_Annotation(uRootOfTwo, maxR, minR);
            }
        }

        private void bRMin_Click(object sender, EventArgs e)
        {
            if (DelMinR.Count != 0)
            {
                DeltaNMin(DelMinR);
                My_Text_Annotation(uRootOfTwo, maxR, minR);
            }
        }

        private void dMinZ_Click(object sender, EventArgs e)
        {
            if (DelMinZ.Count != 0)
            {
                DeltaNMin(DelMinZ);
                My_Text_Annotation(uZeta3, maxZ, minZ);
            }
        }

        private void bEMin_Click(object sender, EventArgs e)
        {if (DelMinE.Count != 0)
            {
                DeltaNMin(DelMinE);
                My_Text_Annotation(uEuler, maxE, minE);
            }
        }
    }
}
