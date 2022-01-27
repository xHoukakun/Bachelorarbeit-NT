using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;



namespace Bachelorarbeit_NT
{/// <summary>
/// die Starter Klasse kümmert sich um organisation der Form wie alles berechnet werden soll. Es wurde sehr viel Abstrahiert. 
/// </summary>
    public class Starter
    {
        public List<Thread> worker = new List<Thread>(); //Ich erstelle eine Liste von Threads die Aktiv sind.







        public int workerNum = 0;            
        public int workerFinished = 0;
        public int Binder = 0;
        ulong AnzahlJobs = 0;
        ulong AnzahlWerte = 0;
        public int AnzahlIntervalle = 0;
        bool Fehler = false;
        decimal sx;
        decimal sy;




        private decimal Delta;
        private decimal GesamtIntervall;
        public Starter(int workerNum, ulong N, int AnzahlIntervalle, decimal GesamtIntervall, CancellationToken cToken)
        {
            this.GesamtIntervall = GesamtIntervall;       //Berechne die intervalle
            AnzahlWerte = N;
            this.AnzahlIntervalle = AnzahlIntervalle;
            this.workerNum = workerNum;    //setze die Arbeiteranzahl
            Delta = this.GesamtIntervall / Convert.ToDecimal(AnzahlIntervalle);  //Berechne das Detla
            //Channel<Coordinate> jobChannel = Channel.CreateBounded<Coordinate>(1024);  //erstelle den Job Queue 8388608 65536
            //Channel<Result> resultChannel = Channel.CreateBounded<Result>(1024);   //erstelle die result queue 
            //Channel<ExtDecimal> cWurzel2 = Channel.CreateBounded<ExtDecimal>(1024); //Erstelle einen Channel für wo nur werte einer Quadratischen Form liegen      
            //Channel<ExtDecimal> cEuler = Channel.CreateBounded<ExtDecimal>(1024);    
            //Channel<ExtDecimal> cZeta3 = Channel.CreateBounded<ExtDecimal>(1024);
            //Channel<Decimal> AbstandWurzel2 = Channel.CreateBounded<Decimal>(1024);
            //Channel<Decimal> AbstandEuler = Channel.CreateBounded<Decimal>(1024);
            //Channel<Decimal> AbstandZeta3 = Channel.CreateBounded<Decimal>(1024);

           // var sWurzel2 = new Thread(() => Statistic("RootOfTwo")); //Hier wird per Lambda ein neuer Thread erstellt für die Statistik
          //  sWurzel2.Start();
           // worker.Add(sWurzel2);
           // var sEuler = new Thread(() => Statistic("Euler"));
            //sEuler.Start();
            //worker.Add(sEuler);
            //var sZeta3 = new Thread(() => Statistic("Zeta3"));
            //sZeta3.Start();
            //worker.Add(sZeta3);

           
            

        }
        public async void Auslesen(string path)
        {
            Console.WriteLine(path);
            var sWurzel2 = new Thread(() => Statistic("RootOfTwo",path)); //Hier wird per Lambda ein neuer Thread erstellt für die Statistik
            sWurzel2.Start();
            worker.Add(sWurzel2);
            var sEuler = new Thread(() => Statistic("Euler",path));
            sEuler.Start();
            worker.Add(sEuler);
            var sZeta3 = new Thread(() => Statistic("Zeta3",path));
            sZeta3.Start();
            worker.Add(sZeta3);
        }







        /// <summary>
        /// Hält eine Statistik für die Werte
        /// </summary>
        /// <param name="chReader">Channel für die Abstände</param>
        /// <param name="Typ">Welcher Typ ist die Statistik</param>
        public async void Statistic(string Typ, string path)
        {
            List<ulong> Anzahl = new List<ulong>(); //Erstelle eine liste für die Statistik
            decimal Maximum = 0; //setze den Maximalen Abstand auf 0 
            decimal Minimum = 100; //setze den Minimalen Abstand auf 100 Das mache ich um die Werte einfacher zu aktualisieren. Denn hier sind die Funktionen monoton.
            if (File.Exists("Merken.txt") || File.Exists("Fertig.txt")) //Wenn schonmal was berechnet wurde dann suche die Datei Statistik 
            {
                if (File.Exists(Typ + "Statistik.txt")) //Falls schonmal eine Statistik erstellt wurde führe diese Weiter.
                {
                    string hile = Typ + "Statistik.txt"; //Lese aus der Datei die Werte
                    StreamReader reader = new StreamReader(hile);
                    Minimum = Convert.ToDecimal(reader.ReadLine());
                    Maximum = Convert.ToDecimal(reader.ReadLine());
                    for (int i = 0; i < AnzahlIntervalle; i++)  //befülle die Liste mit den Werten. Hier ist sicher, dass es immer AnzahlIntervall viele Einträge gibt.
                    {
                        Anzahl.Add(Convert.ToUInt64(reader.ReadLine()));
                    }
                    Anzahl.Add(Convert.ToUInt64(reader.ReadLine()));
                    reader.Close();


                }
            }
            else //Falls diese Dateien nicht existieren gibt es auch keine Statistik. Diese muss erstellt werden.
            {
                for (int i = 0; i < AnzahlIntervalle; i++)
                {
                    Anzahl.Add(0);                                  //Liste Erstellen für die Statistik.
                }
                Anzahl.Add(0);
            }


            switch (Typ)
            {
                case "RootOfTwo":
                    Form1.Liste_RootOfTwo(Anzahl, Minimum, Maximum);

                    break;


                case "Zeta3":

                    Form1.Liste_Zeta3(Anzahl, Minimum, Maximum);



                    break;
                case "Euler":
                    Form1.Liste_Euler(Anzahl, Minimum, Maximum);



                    break;
                default: throw new ArgumentException(); //Falls es ein s gibt welches zu nichts passt
            }



        }


        

    
              
    }
}




//beispiel für Lambda:
//System.Linq.Expressions.Expression<Func<int, int>> e = x => x * x;
//Console.WriteLine(e);
//// Output:
//// x => (x * x)