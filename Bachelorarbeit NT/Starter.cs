using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Data.SQLite;
using System.IO;
using System.Numerics;
using System.Data.Sql;




namespace Bachelorarbeit_NT
{/// <summary>
/// die Starter Klasse kümmert sich um organisation der Form wie alles berechnet werden soll. Es wurde sehr viel Abstrahiert. 
/// </summary>
    public class Starter
    {
        public List<Thread> worker = new List<Thread>(); //Ich erstelle eine Liste von Threads die Aktiv sind.
        private List<Thread> dbcreater = new List<Thread>();

        
       


        
        public int workerNum = 0;
        public int workerFinished = 0;
        public int Binder = 0;    
        ulong AnzahlJobs = 0;
        ulong AnzahlWerte = 0;
        ulong AnzahlDB = 0;
        public int AnzahlIntervalle = 0;
        

        private ulong AnzahlWurzel2;
        private decimal MaxWurzel2;
        private ulong AnzahlEuler;
        private decimal MaxEuler;
        private ulong AnzahlZeta3;
        private decimal MaxZeta3;
        private decimal Delta;
        private decimal GesamtIntervall=10M;
        public Starter(int workerNum, ulong N,int AnzahlIntervalle, CancellationToken cToken)
        {

            AnzahlWerte = N;
            this.AnzahlIntervalle = AnzahlIntervalle;
            this.workerNum = workerNum;
            Delta = GesamtIntervall / Convert.ToDecimal(AnzahlIntervalle);
            Channel<Coordinate> jobChannel = Channel.CreateBounded<Coordinate>(65536);  //erstelle den Job Queue 8388608
            Channel<Result> resultChannel = Channel.CreateBounded<Result>(33554432);   //erstelle die result queue 
            Channel<Decimal> cWurzel2 = Channel.CreateBounded<Decimal>(65536); //Erstelle einen Channel für wo nur werte einer Quadratischen Form liegen      
            Channel<Decimal> cEuler = Channel.CreateBounded<Decimal>(65536);  
            Channel<Decimal> cZeta3 = Channel.CreateBounded<Decimal>(65536);
            Channel<Decimal> AbstandWurzel2 = Channel.CreateBounded<Decimal>(512);
            Channel<Decimal> AbstandEuler = Channel.CreateBounded<Decimal>(512);
            Channel<Decimal> AbstandZeta3 = Channel.CreateBounded<Decimal>(512);

            var sWurzel2 = new Thread(() => Statistic(AbstandWurzel2, "RootOfTwo"));
            sWurzel2.Start();
            worker.Add(sWurzel2);
            var sEuler = new Thread(() => Statistic(AbstandEuler, "Euler"));
            sEuler.Start();
            worker.Add(sEuler);
            var sZeta3 = new Thread(() => Statistic(AbstandZeta3, "Zeta3"));
            sZeta3.Start();
            worker.Add(sZeta3);

            //Database_Management(dbChannelW2, dbChannelEuler, dbChannelZeta3, cToken); //hier wird die Datenbank erstellt. Ich habe das in eine Methode ausgelagert für die Lesbarkeit
            var holdListWurzel2 = new Thread(() => HoldList(cWurzel2, AbstandWurzel2)); //den Listen Halter erstellen 
            holdListWurzel2.Start();
            worker.Add(holdListWurzel2);
            var holdListZeta3 = new Thread(() => HoldList(cZeta3, AbstandZeta3));
            holdListZeta3.Start();
            worker.Add(holdListZeta3);
            var holdListEuler = new Thread(() => HoldList(cEuler, AbstandEuler));
            holdListEuler.Start();
            worker.Add(holdListEuler);

                var sorter1 = new Thread(() => sorter(resultChannel, cEuler, cWurzel2, cZeta3, cToken));// lambda ausdruck um den thread zu initialisieren.
                sorter1.Start();
                worker.Add(sorter1);
                var sorter2 = new Thread(() => sorter(resultChannel, cEuler, cWurzel2, cZeta3, cToken));// lambda ausdruck um den thread zu initialisieren.
                sorter2.Start();
                worker.Add(sorter2);
               
             //Hier werden 2 sorter erstellt 
              //Erstelle Worker
            var controller = new Thread(() => Controller(jobChannel, resultChannel, cEuler, cWurzel2, cZeta3, cToken));
            controller.Start();
            worker.Add(controller);
            for (int i = 0; i < workerNum; i++)
            {
                var trh = new Thread(() => Worker(jobChannel, resultChannel, cToken, N));        //hier werden Worker erstellt mittels lambda ausdruck man braucht hier eine Anonyme Funktion der man diese Parameter übergibt
                trh.Start();                                                                    //beispiel für einen "verständlicheren" Lambda findet man unten
                worker.Add(trh);                                                                //ich kenne den Worker ja sogesehen nicht. Der bekommt erst später einen genauen Typen.
            }
           /* dbcreater.ForEach(delegate (Thread thread) //Warte auf die erstellung der Datenbanken
            {

                thread.Join();


            });
           */
            var JobProd = new Thread(() => JobProducer(jobChannel, cToken, N)); //hier startet der Job Producer
            JobProd.Start();
            worker.Add(JobProd);

        }

        public async void Worker(ChannelReader<Coordinate> jobChan, ChannelWriter<Result> resultChan, CancellationToken cToken, ulong N)
        {
            while (await jobChan.WaitToReadAsync()) //Warte bis irgendwas in der queue ist um es zu berechnen
            {
                while (jobChan.TryRead(out var jobItem)) //wenn etwas in der Queue ist versuche es zu nehmen
                {
                    var s = jobItem.Alpha(); // finde raus welches Item du gerade berechnet hast ( alpha)


                    decimal result = jobItem.Calc();   //Berechne es.
                     
                        switch (s)  //unsicher ob ich das wirklich hier machen soll
                        {
                            case "RootOfTwo":

                                if (AnzahlWurzel2 == AnzahlWerte)  //Hier passiert laufzeit optimierung. Wenn ich n werte habe brauche ich ja nur kleinere Werte. 
                                {                                  
                                    if (result > MaxWurzel2)
                                    {

                                    }
                                    else if (result < MaxWurzel2)
                                    {
                                        
                                        await resultChan.WriteAsync(new Result(s, result));  //Warte bis du auf  Result schreiben kannst
                                    }

                                }
                                else
                                {
                                    AnzahlWurzel2++;
                                    if (result > MaxWurzel2)
                                    {
                                        MaxWurzel2 = result;
                                    }

                                    await resultChan.WriteAsync(new Result(s, result));  //Warte bis du auf Result schreiben kannst
                                }

                                break;
                            case "Euler":
                                if (AnzahlEuler == AnzahlWerte)
                                {
                                    if (result > MaxEuler)
                                    {

                                    }
                                    else if (result < MaxEuler)
                                    {
                                        
                                        await resultChan.WriteAsync(new Result(s, result)); //Warte bis du auf Result schreiben kannst
                                    }

                                }
                                else
                                {
                                    AnzahlEuler++;
                                    if (result > MaxEuler)
                                    {
                                        MaxEuler = result;
                                    }

                                    await resultChan.WriteAsync(new Result(s, result)); //Warte bis du auf Result schreiben kannst
                                }

                                break;
                            case "Zeta3":
                                if (AnzahlZeta3 == AnzahlWerte)
                                {
                                    if (result > MaxZeta3)
                                    {

                                    }
                                    else if (result < MaxZeta3)
                                    {
                                        
                                        await resultChan.WriteAsync(new Result(s, result)); //Warte bis du auf Result schreiben kannst
                                    }

                                }
                                else
                                {
                                    AnzahlZeta3++;
                                    if (result > MaxZeta3)
                                    {
                                        MaxZeta3 = result;
                                    }

                                    await resultChan.WriteAsync(new Result(s, result)); //Warte bis du auf Result schreiben kannst
                                }

                                break;
                            default: throw new ArgumentException();
                        }

                }
               

                
             

                


               

            }
            workerFinished++;
            Thread.Sleep(2000);
            if(workerNum==workerFinished)
            {
                resultChan.TryComplete();
            }
            return;
        }
        public async void JobProducer(ChannelWriter<Coordinate> jobChan, CancellationToken cToken, ulong ende)
        {
            decimal x = 1, y = 1;
            decimal x2 = x;
           /* if (File.Exists("Merken.txt"))
            {

                string hile = "Merken.txt";

                StreamReader reader = new StreamReader(hile);
                x = Convert.ToDecimal(reader.ReadLine());
                x2 = x;
                y = Convert.ToDecimal(reader.ReadLine());
                AnzahlJobs = Convert.ToUInt64(reader.ReadLine());
                reader.Close();
                while (x2 >= 1)
                {

                    await jobChan.WriteAsync(new Coordinate(new RootOfTwo(), x2, y)); //schreibe einen Job in die "Queue" der bearbeitet werden soll 
                    await jobChan.WriteAsync(new Coordinate(new Zeta3(), x2, y));
                    await jobChan.WriteAsync(new Coordinate(new Euler(), x2, y));
                    x2 = x2 - 1;
                    y++;
                    AnzahlJobs++;


                }
                x++;
            }*/
            while (AnzahlJobs < Math.Ceiling(ende * 1.5))  //gehe diagonal über das Feld um schneller Ergebnisse zu bekommen
            {
                x2 = x;
                y = 1M;
                while (x2 >= 1)
                {

                    await jobChan.WriteAsync(new Coordinate(new RootOfTwo(), x2, y)); //schreibe einen Job in die "Queue" der bearbeitet werden soll 
                    await jobChan.WriteAsync(new Coordinate(new Zeta3(), x2, y));
                    await jobChan.WriteAsync(new Coordinate(new Euler(), x2, y));
                    x2 = x2 - 1;
                    y++;
                    AnzahlJobs++;
                    if (cToken.IsCancellationRequested == true)
                    {
                        
                        
                        jobChan.TryComplete();
                       
                           

                            
                            
                                   //hier werden alle wichtigen Daten zur Aufnahme der nächsten Session gespeichert.
                       StreamWriter sw = new StreamWriter("Merken" + ".txt");
                       sw.WriteLine(x);   //erst x
                       sw.WriteLine(y);    //dann y
                       sw.WriteLine(AnzahlJobs); //Dann die Anzahl an Jobs
                       sw.Close();
                             
                                
                        return;

                            
                            

                        

                    }
                }
                x++;



            }



            jobChan.TryComplete();

            return;
            //jobChan.Complete();  //wenn die Jobs fertig geladen wurden. funktioniert der JobChan.Complete wirklich so? Bei mir sieht es nach einer Löschung des Channels aus....
        }
        public class Result  //hier wird ein Result datentyp definiert der besteht aus dem result der rechnung und dem "term" der rechnung da der term nur von Alpha abhängt gebe ich Alpha zurück
        {
            public string type;
            public decimal result;
            public Result(string s, decimal r)
            {
                this.type = s;
                this.result = r;
            }
        }
         //Datentyp für den Datenbank Channel
        /// <summary>
        /// Hier ist ein Binder, dieser nimmt sachen aus dem Channel  packt es in Päckchen und sortiert es in jeweilige Channels
        /// </summary>
        /// <param name="resultChan">Aus diesem Channel nimmt der Binder</param>
        /// <param name="DBEuler">Datenbank Channel Euler</param>
        /// <param name="DBWurzel2">Datenbank Channel Wurzel2</param>
        /// <param name="DBZeta3">Datenbank channel Zeta3 </param>
        /// <param name="cToken"></param>
        public async void sorter(ChannelReader<Result> resultChan, ChannelWriter<Decimal> DBEuler, ChannelWriter<Decimal> DBWurzel2, ChannelWriter<Decimal> DBZeta3, CancellationToken cToken) //Das ist der DB worker der schreibt alles  in die Datenbank
        {




            
            while (await resultChan.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            {

               
                    while (resultChan.TryRead(out var jobItem))
                    {

                        string s = jobItem.type;   //in Welche Liste muss es einsortiert werden
                        decimal u = jobItem.result; //das ergebnis
                        switch (s)
                        {
                            case "RootOfTwo":
                              
                                await DBWurzel2.WriteAsync(u);
                                break;

                               
                            case "Zeta3":
                               
                                
                                    await DBZeta3.WriteAsync(u);
                                    
                                
                                break;
                            case "Euler":
                               
                                    await DBEuler.WriteAsync(u);
                                   
                                break;
                            default: throw new ArgumentException(); //Falls es ein s gibt welches zu nichts passt
                        }
                    }
            }      
     
            Binder++;
            Thread.Sleep(2000);
            if (Binder == 2)  //Wenn alle Fertig sind schließt der Letzte die Channels.
            {
               

                DBEuler.TryComplete();
                DBZeta3.TryComplete();
                DBWurzel2.TryComplete();

            }
            return;
        }
        /// <summary>
        /// hier werden listen gehalten
        /// </summary>
        /// <param name="chReader"></param>
        /// <param name="chWriter"></param>
        public async void HoldList(ChannelReader<Decimal> chReader, ChannelWriter<Decimal> chWriter)
        {
            List<Decimal> Ergebnisse = new List<Decimal>();
            ulong AnzahlListe = 0;
            decimal Maximum = 0;
            while (await chReader.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            {


                while (chReader.TryRead(out var jobItem))
                {
                    decimal u = jobItem;
                    
                    if (AnzahlListe == AnzahlWerte)
                    {
                        if(u>Maximum)
                        {

                        }
                        else if(u<Maximum)
                        {
                            
                            Ergebnisse.Remove(Maximum);
                            Ergebnisse.Add(u);
                            Maximum = Ergebnisse.Max();
                        }
                    }
                    else
                    {
                        Maximum=Math.Max(u,Maximum);
                        AnzahlListe++;
                        Ergebnisse.Add(u);
                    }
                    if(Ergebnisse.Count==20_000_000)
                    {
                        Abstandsrechner(Ergebnisse, chWriter);
                    }

                }
            }
            while(Ergebnisse.Count!=1)
            {
                Abstandsrechner(Ergebnisse, chWriter);
            }
            chWriter.TryComplete();
        }
        public async void Statistic(ChannelReader<Decimal> chReader, string Typ)
        {
            List<ulong> Anzahl = new List<ulong>();
            for(int i =0;i<AnzahlIntervalle;i++)
            {
                Anzahl.Add(0);
            }
            Anzahl.Add(0);
            ulong AWerte = 0;
            while (await chReader.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            {


                while (chReader.TryRead(out var jobItem))
                {
                    decimal u = jobItem;
                    mIntervallRechner(u, Anzahl);
                    AWerte++;
                    if (AWerte == 1000) //update Funktion für die Statistic
                    {
                        AWerte = 0;
                        switch (Typ)
                        {
                            case "RootOfTwo":
                                Form1.Liste_RootOfTwo(Anzahl);

                                break;


                            case "Zeta3":

                                Form1.Liste_Zeta3(Anzahl);



                                break;
                            case "Euler":
                                Form1.Liste_Euler(Anzahl);



                                break;
                            default: throw new ArgumentException(); //Falls es ein s gibt welches zu nichts passt
                        }
                    }
                }
            }
            switch (Typ)
            {
                case "RootOfTwo":
                    Form1.Liste_RootOfTwo(Anzahl);
                    
                    break;


                case "Zeta3":

                    Form1.Liste_Zeta3(Anzahl);
                   


                    break;
                case "Euler":
                    Form1.Liste_Euler(Anzahl);

                    

                    break;
                default: throw new ArgumentException(); //Falls es ein s gibt welches zu nichts passt
            }

            //Hier muss dann die Statistik übergeben werden. ( Ich werde versuchen dies als PythonSkript zu machen;
        }

           
        /// <summary>
        /// Hier werden die Tables erst gedroppt und dann neu erstellt. Somit fängt die Berechnung von neu an. Man hat keine Inkonsistenten Daten
        /// </summary>
        /// <param name="connect">Connection String für die database</param>
        public void Create_Database(string connect)
        {
            using (var connection = new SQLiteConnection(connect))
            {
                connection.Open();
               
               
                
                
                


                
                var cmd = new SQLiteCommand(connection);
                Console.WriteLine("drop table");
                cmd.CommandText = "DROP TABLE IF EXISTS Wurzel2";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DROP TABLE IF EXISTS Eulersche_Zahl";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DROP TABLE IF EXISTS Zeta3";
                cmd.ExecuteNonQuery();
                Console.WriteLine("Dropped Tables");
                //SQL befehle für das ERstellen der Datenbank
                //Create Table der PK ist das Ergebnis also Value mit hoher genauigkeit. und ohne RowID ( Die Datenbank soll möglichst klein gehalten werden)
                cmd.CommandText = @"CREATE TABLE Wurzel2 (

                 

                    Value  Decimal NOT NULL UNIQUE,
                    PRIMARY KEY(Value)
                    )WITHOUT ROWID;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Eulersche_Zahl (

                    

                    Value Decimal NOT NULL UNIQUE,
                    PRIMARY KEY(Value)
                )WITHOUT ROWID;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Zeta3 (

                    

                    Value Decimal NOT NULL UNIQUE,
                    PRIMARY KEY(Value)
                )WITHOUT ROWID;";
                cmd.ExecuteNonQuery();
                //Hier werden nun Indexe auf die Values gesetzt damit man einfacher drauf zugreifen kann ( Sortierung)
                //Dies hat einen sehr großen Einfluss auf die Laufzeit des Programmes. 
                //Da ich nicht weiß in welcher Reihenfolge die Werte rein geschrieben werden 
                //Sorge ich nachher mit Select befehl das die sehr schnell aufsteigend sortiert werden
                cmd.CommandText = @"CREATE INDEX Sortiert_Euler ON Eulersche_Zahl(
                     Value ASC
                     )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX Sortiert_Wurzel2 ON Wurzel2(
                     Value ASC
                     )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE INDEX Sortiert_Zeta3 ON Zeta3 (

                     Value ASC
                 )";
                cmd.ExecuteNonQuery();
                connection.Close();

            }
        }
       
        /// <summary>
        /// Der Controller wird darauf verwendet das falls ein Cancel Requested wird alles noch gesaved wird.
        /// </summary>
        /// <param name="jobChannel"></param>
        /// <param name="ResultChannel"></param>
        /// <param name="DBA"></param>
        /// <param name="DBB"></param>
        /// <param name="DBC"></param>
        /// <param name="cToken"></param>
        public async void Controller(ChannelReader<Coordinate> jobChannel, ChannelReader<Result> ResultChannel, ChannelReader<Decimal> DBA, ChannelReader<Decimal> DBB, ChannelReader<Decimal> DBC,CancellationToken cToken)
        {
            
            while(cToken.IsCancellationRequested==false) //wenn der Token false ist dann wird nichts gemacht.
            {
                Thread.Sleep(1000);
                
            }
            if (cToken.IsCancellationRequested == true) //wenn der Token auf True fällt dann wird ein sicherer abbruch initialisiert.
            {
                
                while (true)
                {

                    Console.WriteLine(AnzahlDB);
                    if ((jobChannel.Completion.IsCompleted && ResultChannel.Completion.IsCompleted && DBA.Completion.IsCompleted && DBB.Completion.IsCompleted && DBC.Completion.IsCompleted)) 
                    {



                        Thread.Sleep(10000);
                   
                        StreamWriter sw2 = new StreamWriter("Anzahl.txt");  //wenn alles Geschlossen ist werden einzelne Werte gespeichert.
                                sw2.WriteLine(AnzahlWurzel2);
                                sw2.WriteLine(MaxWurzel2);
                                sw2.WriteLine(AnzahlEuler);
                                sw2.WriteLine(MaxEuler);
                                sw2.WriteLine(AnzahlZeta3);
                                sw2.WriteLine(MaxZeta3);
                                sw2.Close();
                                Thread.Sleep(10000);
                                Form1.save();
                                return;
                            
                             

                            
                            

                    }
                    
                    Thread.Sleep(1000);

                }
            }

        }
        
        public void Abstandsrechner(List<Decimal> Werte, ChannelWriter<Decimal> Abstand)
        {
            Werte.Sort();
            decimal x = 0;
            if (Werte.Count()>100)
            {
                for (int i = 0; i < 100; i++)
                {
                    x = Werte[i + 1] - Werte[i];
                    Werte.RemoveAt(i);
                    Abstand.WriteAsync(x);
                }
            }
            else
            {
                for(int i=0;i+1<Werte.Count(); i++)  //bug
                {
                    x = Werte[i + 1] - Werte[i];
                    Werte.RemoveAt(i);
                    Abstand.WriteAsync(x);
                }
            }
        }
        public void mIntervallRechner(decimal Abstand, List<ulong> Zähler) //implementation der Methode IntervallRechner;
        {
            int i = 0;
            while (i < AnzahlIntervalle) 
            {
                if (Convert.ToDecimal(i) * Delta <= Abstand && Abstand <= Convert.ToDecimal(i + 1) * Delta) 
                {

                    //hier muss dann hochgezählt werden
                    Zähler[i]++;
                    return;
                }
                else
                {
                    i++;
                }
            }
            Zähler[i]++;
                 

        } 
    }
}

        
    

//beispiel für Lambda:
//System.Linq.Expressions.Expression<Func<int, int>> e = x => x * x;
//Console.WriteLine(e);
//// Output:
//// x => (x * x)