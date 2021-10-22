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



        private decimal Delta;
        private decimal GesamtIntervall;
        public Starter(int workerNum, ulong N, int AnzahlIntervalle, decimal GesamtIntervall, CancellationToken cToken)
        {
            this.GesamtIntervall = GesamtIntervall;       //Berechne die intervalle
            AnzahlWerte = N;
            this.AnzahlIntervalle = AnzahlIntervalle;
            this.workerNum = workerNum;    //setze die Arbeiteranzahl
            Delta = this.GesamtIntervall / Convert.ToDecimal(AnzahlIntervalle);  //Berechne das Detla
            Channel<Coordinate> jobChannel = Channel.CreateBounded<Coordinate>(1024);  //erstelle den Job Queue 8388608 65536
            Channel<Result> resultChannel = Channel.CreateBounded<Result>(1024);   //erstelle die result queue 
            Channel<ExtDecimal> cWurzel2 = Channel.CreateBounded<ExtDecimal>(1024); //Erstelle einen Channel für wo nur werte einer Quadratischen Form liegen      
            Channel<ExtDecimal> cEuler = Channel.CreateBounded<ExtDecimal>(1024);    
            Channel<ExtDecimal> cZeta3 = Channel.CreateBounded<ExtDecimal>(1024);
            Channel<Decimal> AbstandWurzel2 = Channel.CreateBounded<Decimal>(1024);
            Channel<Decimal> AbstandEuler = Channel.CreateBounded<Decimal>(1024);
            Channel<Decimal> AbstandZeta3 = Channel.CreateBounded<Decimal>(1024);

            var sWurzel2 = new Thread(() => Statistic(AbstandWurzel2, "RootOfTwo")); //Hier wird per Lambda ein neuer Thread erstellt für die Statistik
            sWurzel2.Start();
            worker.Add(sWurzel2);
            var sEuler = new Thread(() => Statistic(AbstandEuler, "Euler"));
            sEuler.Start();
            worker.Add(sEuler);
            var sZeta3 = new Thread(() => Statistic(AbstandZeta3, "Zeta3"));
            sZeta3.Start();
            worker.Add(sZeta3);

           
            var holdListWurzel2 = new Thread(() => HoldList(cWurzel2, AbstandWurzel2, "RootOfTwo", cToken)); //Hier wird ein Thread erstellt der sich darum kümmert, dass die Werte in einer Liste verwaltet werden
            holdListWurzel2.Start();
            worker.Add(holdListWurzel2);
            var holdListZeta3 = new Thread(() => HoldList(cZeta3, AbstandZeta3, "Zeta3", cToken));     
            holdListZeta3.Start();
            worker.Add(holdListZeta3);
            var holdListEuler = new Thread(() => HoldList(cEuler, AbstandEuler, "Euler", cToken));
            holdListEuler.Start();
            worker.Add(holdListEuler);

            var sorter1 = new Thread(() => Sorter(resultChannel, cEuler, cWurzel2, cZeta3, cToken));// lambda ausdruck um den thread zu initialisieren.
            sorter1.Start();  //Der thread sorgt dafür, dass die Werte auf einzelne Channels verteilt werden. Für jede quadratische Form gibt es einen seperaten Channel
            worker.Add(sorter1);
           


            var controller = new Thread(() => Controller(jobChannel, resultChannel, cEuler, cWurzel2, cZeta3, AbstandEuler, AbstandWurzel2, AbstandZeta3, cToken));  //Hier wird der Controller erstellt der für ein sicheres beenden sorgt
            controller.Start();
            worker.Add(controller);
            for (int i = 0; i < workerNum; i++)
            {
                var trh = new Thread(() => Worker(jobChannel, resultChannel, cToken, N));        //hier werden Worker erstellt mittels lambda ausdruck man braucht hier eine Anonyme Funktion der man diese Parameter übergibt
                trh.Start();                                                                    //beispiel für einen "verständlicheren" Lambda findet man unten
                worker.Add(trh);                                                                //ich kenne den Worker ja sogesehen nicht. Der bekommt erst später einen genauen Typen.
            }
           
            var JobProd = new Thread(() => JobProducer(jobChannel, cToken, N));         //hier startet der Job Producer
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
                    ulong n = jobItem.n;  //Finde heraus der wie vielte Wert es ist
                    decimal result = jobItem.Calc();   //Berechne es.
                    await resultChan.WriteAsync(new Result(s, result, n));  //Warte bis du auf  Result schreiben kannst
                }

            }
            workerFinished++; //wenn der JobChannel Geschlossen wird dann werden die Fertigen Arbeiter gezählt 
            Thread.Sleep(2000);
            if (workerNum == workerFinished) //Wenn alle Arbeiter Fertig sind wird der ResultChannel geschlossen
            {
                resultChan.TryComplete();  // hier wird der ResultChannel Geschlossen.
            }
            return;
        }
        public async void JobProducer(ChannelWriter<Coordinate> jobChan, CancellationToken cToken, ulong ende)
        {
            if (File.Exists("Fertig.txt"))  //Wenn Schonmal alles berechnet wurde muss das nicht noch einmal passieren 
            {
                jobChan.TryComplete(); //Der JobChannel wird dann ohne etwas darauf zu legen geschlossen und damit schließen sich automatisch auch alle Threads.
                return;
            }
            decimal x = 1, y = 1;
            decimal x2 = x;
            if (File.Exists("Merken.txt")) //falls es die Datei Merken gibt benutze diese Daten. Dort stehen die Coordinaten drin bei denen aufgehört wurden.
            {

                string hile = "Merken.txt"; 
                StreamReader reader = new StreamReader(hile);  //lese aus der Datei
                x = Convert.ToDecimal(reader.ReadLine());
                x2 = x;
                y = Convert.ToDecimal(reader.ReadLine());
                AnzahlJobs = Convert.ToUInt64(reader.ReadLine());
                reader.Close();
                while (x2 >= 1)
                {

                    await jobChan.WriteAsync(new Coordinate(new RootOfTwo(), x2, y, AnzahlJobs + 1)); //schreibe einen Job in die "Queue" der bearbeitet werden soll 
                    await jobChan.WriteAsync(new Coordinate(new Zeta3(), x2, y, AnzahlJobs + 1));
                    await jobChan.WriteAsync(new Coordinate(new Euler(), x2, y, AnzahlJobs + 1));
                    x2 = x2 - 1;  //Gehe einen x wert runter 
                    y++;  //gehe einen y wert hoch     //diagonal
                    AnzahlJobs++;
                    if (cToken.IsCancellationRequested == true) //Wenn das Programm enden soll dann wird der JobChan zu gemacht
                    {

                        StreamWriter sw = new StreamWriter("Merken" + ".txt");
                        sw.WriteLine(x);   //erst x
                        sw.WriteLine(y);    //dann y
                        sw.WriteLine(AnzahlJobs); //Dann die Anzahl an Jobs
                        sw.Close();
                        jobChan.TryComplete();
                        return;
                    }

                }
                x++; //Gehe beim Ursprünglichen X einen hoch.
            }

            while (AnzahlJobs < Math.Ceiling(ende * 1.5))  //gehe diagonal über das Feld um schneller Ergebnisse zu bekommen
            {
                x2 = x;
                y = 1M;

                while (x2 >= 1)
                {

                    await jobChan.WriteAsync(new Coordinate(new RootOfTwo(), x2, y, AnzahlJobs + 1)); //schreibe einen Job in die "Queue" der bearbeitet werden soll 
                    await jobChan.WriteAsync(new Coordinate(new Zeta3(), x2, y, AnzahlJobs + 1));
                    await jobChan.WriteAsync(new Coordinate(new Euler(), x2, y, AnzahlJobs + 1));
                    x2 = x2 - 1;
                    y++;
                    AnzahlJobs++;
                    if (cToken.IsCancellationRequested == true) //Wenn das Programm enden soll dann wird der JobChan zu gemacht
                    {

                        StreamWriter sw = new StreamWriter("Merken" + ".txt");
                        sw.WriteLine(x);   //erst x
                        sw.WriteLine(y);    //dann y
                        sw.WriteLine(AnzahlJobs); //Dann die Anzahl an Jobs
                        sw.Close();
                        jobChan.TryComplete();
                        return;






                    }
                }
                x++;



            }
            StreamWriter sw2 = new StreamWriter("Fertig.txt"); //Falls alle Jobs erstellt wurden wird die Datei erstellt um sich zu Merken das es nichts mehr zu berechnen gibt
            sw2.WriteLine("Fertig");
            sw2.Close();


            jobChan.TryComplete(); //Wenn die Jobs fertig geladen wurden wird dieser Channel Geschlossen. 

            return;

        }


        /// <summary>
        /// Hier ist ein Binder, dieser nimmt sachen aus dem Channel  packt es in Päckchen und sortiert es in jeweilige Channels
        /// </summary>
        /// <param name="resultChan">Aus diesem Channel nimmt der Binder</param>
        /// <param name="DBEuler"> Channel Euler</param>
        /// <param name="DBWurzel2"> Channel Wurzel2</param>
        /// <param name="DBZeta3"> channel Zeta3 </param>
        /// <param name="cToken"></param>
        public async void Sorter(ChannelReader<Result> resultChan, ChannelWriter<ExtDecimal> cEuler, ChannelWriter<ExtDecimal> cWurzel2, ChannelWriter<ExtDecimal> cZeta3, CancellationToken cToken) //Das ist der Sortierer er sortiert alles in die Passenden Channels
        {





            while (await resultChan.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            {


                while (resultChan.TryRead(out var jobItem))
                {

                    string s = jobItem.type;   //in Welche Liste muss es einsortiert werden
                    decimal u = jobItem.result; //das ergebnis
                    ulong n = jobItem.n;     //Laufende Zahl
                    switch (s)
                    {
                        case "RootOfTwo":  //wenn es vom Typen Wurzel2 ist dann lege es auf diesen Channel

                            await cWurzel2.WriteAsync(new ExtDecimal(u, n));
                            break;


                        case "Zeta3":


                            await cZeta3.WriteAsync(new ExtDecimal(u, n));


                            break;
                        case "Euler":

                            await cEuler.WriteAsync(new ExtDecimal(u, n));

                            break;
                        default: throw new ArgumentException(); //Falls es ein s gibt welches zu nichts passt
                    }
                }
            }

            Binder++;
            Thread.Sleep(2000);
            if (Binder == 1)  //Wenn alle Fertig sind schließt der Letzte die Channels.
            {


                cEuler.TryComplete();  //Beende die Channels wenn nichts mehr auf dem ResultChannel liegt
                cZeta3.TryComplete();
                cWurzel2.TryComplete();

            }
            return;
        }
        /// <summary>
        /// hier werden listen gehalten
        /// </summary>
        /// <param name="chReader">Die Werte</param>
        /// <param name="chWriter">Schreibe die Abstände</param>
        public async void HoldList(ChannelReader<ExtDecimal> chReader, ChannelWriter<Decimal> chWriter, string Typ, CancellationToken cToken)
        {
            List<decimal> Ergebnisse = new List<decimal>();
            ulong AnzahlListe = 0;  //dieses N muss noch gesetzt werden das es auch mitgespeichert wird.
            ulong N = 0;
            decimal Maximum = 0;
            if (File.Exists(Typ + "Abstände.txt"))   //Die Gespeicherten Daten Auslesen.
            {
                Stopwatch Zeitmessung = new Stopwatch();
                Zeitmessung.Start();
                string line;
                string hile = Typ + "Abstände.txt";
                StreamReader reader = new StreamReader(hile);
                line = reader.ReadLine();
                AnzahlListe = Convert.ToUInt64(line);
                line = reader.ReadLine();
                while (line != null)  //Lese solange die Zeile nicht null ist, man weiß nicht wie Lang die Liste genau ist. Man könnte dies Zwar noch dazu speichern aber das wäre unnötig
                {
                    Ergebnisse.Add(Convert.ToDecimal(line));
                    line = reader.ReadLine();
                }
                reader.Close();
                Zeitmessung.Stop();
                Console.Write(Typ + " Abstände auslesen: ");
                Console.WriteLine(Zeitmessung.Elapsed);    //Gebe aus wie lange diese Aktion gedauert hat.

            }
            while (await chReader.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            {


                while (chReader.TryRead(out var jobItem))
                {
                    decimal u = jobItem.d;       
                    N = Math.Max(jobItem.n, N);


                    if (AnzahlListe >= AnzahlWerte)  //falls wir schon n werte berechnet haben 
                    {
                        if (u > Maximum)     //bei einem Größeren Wert mache nichts 
                        {
                            AnzahlListe++;   //Man muss trotzdem hochzählen damit es mit dem N Konsistent bleibt deswegen auch das Größer Gleich
                        }
                        else if (u < Maximum)  //falls ein kleinerer Wert errechnet wurde
                        {

                            Ergebnisse.Remove(Maximum);      //Entferne das Maximum.
                            Ergebnisse.Add(u);   //Füge den wErt ein.
                            Maximum = Ergebnisse.Max();       //setze das Maximum neu, Folgende Situation: Es sind in der Liste die werte 1, 3, 4, und 
                            AnzahlListe++;                                  //Jetzt wird die 2 hinzugefügt dafür fliegt die 4 raus. Jetzt ist das Maximum natürlich die 3 und nicht die hinzugefügte 2
                        }
                    }
                    else
                    {

                        Maximum = Math.Max(u, Maximum);  //laufendes Maximum
                        AnzahlListe++;  //So viele Werte sind und waren in der Liste, man soll ja die kleinsten werte berechnen. 
                        Ergebnisse.Add(u);  //hinzufügen zur Liste


                    }
                    if (Ergebnisse.Count == 30_000_000)   //Da man bei 10^x ein speicher problem bekommt halte ich 20_000_000  Werte in einer Liste
                    {
                        var gc2 = new Thread(() => GC.Collect());  //lasse den Garbage Collecter laufen um Arbeitsspeicher zu sparen.
                        gc2.Start();

                        Abstandsrechner(Ergebnisse, chWriter);   //Lasse die Differenzen berechnen

                    }

                }

            }
            if (cToken.IsCancellationRequested == true) //Speichere den Fortschritt bis hierhin wenn der Channel Geschlossen wurde.
            {
                if (AnzahlListe == N) //Wenn Alles gut abgelaufen ist sollten diese Werte Gleich sein. Dann speichere diese auch. 
                {
                    Console.Write("True ");
                    Console.WriteLine(Typ);
                    StreamWriter sw = new StreamWriter(Typ + "Abstände.txt");  
                    sw.WriteLine(AnzahlListe); //Als erstes wird AnzahlListe dort hineingeschrieben.
                    foreach (decimal Wert in Ergebnisse)
                    {

                        sw.WriteLine(Wert);

                    }
                    sw.Close();
                    Ergebnisse.Clear();
                    Console.WriteLine(Typ + "Abstände gespeichert");
                }
                else //Falls es irgendwo einen Fehler gab sind die daten nicht mehr Konsistent deswegen müsste man ganz von neu Anfangen.
                {
                    Fehler = true;
                    Console.WriteLine("Etwas ist Schief gegangen");
                }

                chWriter.TryComplete(); //In jedem Falle wird der Channel geschlossen.

                return;
            } //Falls der Token nicht auf True gesetzt wurde werden die Werte die noch in der Liste sind berechnet. 
            else
            {
                Ergebnisse.Sort(); //sortiere die Liste
                Abstandsrechner2(Ergebnisse, chWriter);  //Berechne die abstände
                chWriter.TryComplete();
            }


        }
        /// <summary>
        /// Hält eine Statistik für die Werte
        /// </summary>
        /// <param name="chReader">Channel für die Abstände</param>
        /// <param name="Typ">Welcher Typ ist die Statistik</param>
        public async void Statistic(ChannelReader<Decimal> chReader, string Typ)
        {
            List<ulong> Anzahl = new List<ulong>(); //Erstelle eine liste für die Statistik
            decimal Maximum = 0; //setze den Maximalen Abstand auf 0 
            decimal Minimum = 100; //setze den Minimalen Abstand auf 100 Das mache ich um die Werte einfacher zu aktualisieren. Denn hier sind die Funktionen monoton.
            if (File.Exists("Merken.txt")||File.Exists("Fertig.txt")) //Wenn schonmal was berechnet wurde dann suche die Datei Statistik 
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

                    switch (Typ) //Welchen Typen hat die Statistik: rufe dann die jeweilige Funktion auf der Form auf.
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
            else //Falls diese Dateien nicht existieren gibt es auch keine Statistik. Diese muss erstellt werden.
            {
                for (int i = 0; i < AnzahlIntervalle; i++)
                {
                    Anzahl.Add(0);                                  //Liste Erstellen für die Statistik.
                }
                Anzahl.Add(0);
            }
            ulong AWerte = 0; //setze anzahl der Werte auf 0 
            while (await chReader.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            {

                while (chReader.TryRead(out var jobItem))
                {
                    decimal u = jobItem;
                    mIntervallRechner(u, Anzahl); //in ein Intervall einsortieren
                    Maximum = Math.Max(u, Maximum);  //laufendes Max und min 
                    Minimum = Math.Min(u, Minimum);
                    AWerte++;
                    if (AWerte == 100) //update Funktion für die Statistik
                    {
                        AWerte = 0;
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
            }//Falls der Channel beendet wurde wird nochmal die funktion geupdatet
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
            StreamWriter sw = new StreamWriter(Typ + "Statistik.txt");  //Speichere die Ganzen werte.
            sw.WriteLine(Minimum);  //Erst Minimum
            sw.WriteLine(Maximum);  //Dann Maximum
            for (int i = 0; i < Anzahl.Count(); i++)
            {
                sw.WriteLine(Anzahl[i]);
            }
            sw.Close();
            Console.WriteLine(Typ + "Statistik gespeichert");
            Anzahl.Clear();


        }
        /// <summary>
        /// Absteigende Implementierung von Insertion Sort
        /// </summary>
        /// <param name="insert">Was zu sortieren ist</param>
        /// <param name="Liste">Die Liste in die hineinsortiert wird</param>
        /// <returns></returns>
        public static List<decimal> InsertionSort(decimal insert, List<decimal> Liste) //InsertionSort Die Liste ist absteigend Sortiert;
        {
            for (int i = 0; i < Liste.Count(); i++)
            {
                if (insert > Liste[i])
                {
                    while (i < Liste.Count())
                    {
                        decimal temp = Liste[i];
                        Liste[i] = insert;
                        insert = temp;
                        i++;
                    }


                }

            }
            Liste.Add(insert);
            return Liste;
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
        public void Controller(ChannelReader<Coordinate> jobChannel, ChannelReader<Result> ResultChannel, ChannelReader<ExtDecimal> cEuler, ChannelReader<ExtDecimal> cWurzel2, ChannelReader<ExtDecimal> cZeta3, ChannelReader<Decimal> AbstandEuler, ChannelReader<Decimal> AbstandWurzel2, ChannelReader<Decimal> AbstandZeta3, CancellationToken cToken)
        {

            while (cToken.IsCancellationRequested == false) //wenn der Token false ist dann wird nichts gemacht.
            {
                Thread.Sleep(1000);
                if ((jobChannel.Completion.IsCompleted && ResultChannel.Completion.IsCompleted && cEuler.Completion.IsCompleted && cWurzel2.Completion.IsCompleted && cZeta3.Completion.IsCompleted && AbstandEuler.Completion.IsCompleted && AbstandWurzel2.Completion.IsCompleted && AbstandZeta3.Completion.IsCompleted))
                {
                    if (File.Exists("EulerStatistik.txt") && File.Exists("Zeta3Statistik.txt") && File.Exists("RootOfTwoStatistik.txt"))
                    {
                        Form1.save(); //Wenn alles gespeichert wurde dann beende alles.
                        return;
                    }
                }


            }
            if (cToken.IsCancellationRequested == true) //wenn der Token auf True fällt dann wird ein sicherer abbruch initialisiert.
            {

                while (true)
                {

                    
                    if ((jobChannel.Completion.IsCompleted && ResultChannel.Completion.IsCompleted && cEuler.Completion.IsCompleted && cWurzel2.Completion.IsCompleted && cZeta3.Completion.IsCompleted && AbstandEuler.Completion.IsCompleted && AbstandWurzel2.Completion.IsCompleted && AbstandZeta3.Completion.IsCompleted))
                    {

                        if (File.Exists("EulerStatistik.txt") && File.Exists("Zeta3Statistik.txt") && File.Exists("RootOfTwoStatistik.txt"))
                        {
                            string[] del = { "Zeta3", "Euler", "RootOfTwo" };

                            Thread.Sleep(10000);
                            if (Fehler == true) //Falls ein Fehler aufgetreten ist dann soll alles gelöscht werden werden.
                            {
                                if (File.Exists("Merken.txt"))
                                {
                                    File.Delete("Merken.txt");
                                }
                                foreach (string f in del)  //StreamWriter sw = new StreamWriter(Typ + "Statistik.txt");                     StreamWriter sw = new StreamWriter(Typ + "Abstände.txt");
                                {
                                    if (File.Exists(f + "Statistik.txt"))
                                    {
                                        File.Delete(f + "Statistik.txt");
                                    }
                                    if (File.Exists(f + "Abstände.txt"))
                                    {
                                        File.Delete(f + "Abstände.txt");
                                    }
                                }
                                Console.WriteLine("EtwasIstSchiefGelaufen");
                                Thread.Sleep(10000);
                                Form1.save(); //beende das programm
                                return;
                            }
                            Thread.Sleep(10000);
                            Form1.save();
                            return;
                        }
                        else
                        {

                          
                            Thread.Sleep(1000);


                        }

                    }

                    Thread.Sleep(1000);

                }

            }
        }


        /// <summary>
        /// Berechne Abstände der gegebenen Zahlen
        /// </summary>
        /// <param name="Werte">Die</param>
        /// <param name="Abstand"></param>
        public async void Abstandsrechner(List<decimal> Werte, ChannelWriter<Decimal> Abstand)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start(); //stoppe die Zeit das ist zur diagnose
            decimal lmax = Werte[600];
            for (int i = 0; i < Werte.Count(); i++)  //Die Liste wird auf 2 Eigenschaften geprüft: 
            {
                if (i < 600)                       //1. Die Ersten 600 Einträge sind sortiert. 
                {
                    if (Werte[i] > Werte[i + 1]) //falls ein Wert nicht soritert ist (Hier prüfe ich eigentlich die Monotonie Eigenschaft der sortierten Liste
                    {

                        Werte.Sort();  //Sortiere wenn die Eigenschaft verletzt ist
                        i = Werte.Count() + 100; //setze das i so das man aus der Schleife springt.
                    }

                }
                else                                    //2. Nach dem 150. Eintrag in der Liste kommt kein kleinerer Eintrag mehr. 
                {                                               //Man kann diese Eigenschaften innerhalb von Maximal 5 Sekunden überprüfen es dauert aber 25 Sekunden eine Liste zu Sortieren. 
                    if (Werte[i] < lmax)                                   // Damit muss man nicht jedes mal Sortieren. 
                    {
                        Werte.Sort();    //falls es einen wert gibt der Kleiner als der größte Wert in den ersten  600 Einträgen ist dann wird neu sortiert.
                        i = Werte.Count() + 100; //springe aus der Schleife
                    }
                    if (i + 1 < Werte.Count())                //Hier wird die Reihenfolge kontrolliert und gegebenenfalls getauscht
                    {                                           //das sorgt dafür, dass es seltener wird die liste im gesamten neu zu sortieren
                        if (Werte[i] > Werte[i + 1])            //wenn man schon durch die Liste geht kann man 2 Elemente Tauschen die nicht richtig sind. das verringert die Wahrscheinlichkeit, dass die Eigenschaft 1 verletzt wird. 
                        {
                            Werte = Swap(Werte, i);   //Tausche die Reihenfolge

                        }
                    }
                }
            }
            watch.Stop();
            Console.Write("Sortieren: ");
            Console.WriteLine(watch.Elapsed);
            decimal x = 0;
            if (Werte.Count() > 500)
            {
                for (int i = 0; i < 500; i++)    //berechne die Abstände 
                {
                    x = Werte[1] - Werte[0];  //Da sich der index verschiebt muss man das so machen
                    Werte.RemoveAt(0);
                    await Abstand.WriteAsync(x); //schreibe auf den channel
                }
            }
            else
            {
                for (int i = 0; i + 1 < Werte.Count();)
                {
                    x = Werte[1] - Werte[0];
                    Werte.RemoveAt(0);
                    await Abstand.WriteAsync(x);
                }
            }
        }
        /// <summary>
        /// tauscht die Einträge an der Stelle i und i+1
        /// </summary>
        /// <param name="Liste">Liste in der die Einträge getauscht werden sollen</param>
        /// <param name="i">Stelle an dem der Eintrag getauscht werden soll</param>
        /// <returns></returns>
        public List<decimal> Swap(List<Decimal> Liste, int intex)
        {
            decimal temp = Liste[intex];
            Liste[intex] = Liste[intex + 1];
            Liste[intex + 1] = temp;
            return Liste;
        }
        public async void Abstandsrechner2(List<Decimal> Werte, ChannelWriter<Decimal> Abstand)
        {

            decimal x = 0;
            for (int i = 0; i + 1 < Werte.Count();)
            {
                x = Werte[1] - Werte[0];
                Werte.RemoveAt(0);
                await Abstand.WriteAsync(x);
            }

        }
        public void mIntervallRechner(decimal Abstand, List<ulong> Zähler) //implementation der Methode IntervallRechner;
        {
            int i = 0;
            while (i < AnzahlIntervalle) //Erechnet in welches Intervall der Abstand fällt. Da die Abstände Poisson verteilt sind reicht ein Lineares suchen aus um es zu bestimmen. Was man implementieren Könnte wäre ein bisektionsverfahren dieses hätte Laufzeit log(n)
            {
                if (Convert.ToDecimal(i) * Delta <= Abstand && Abstand <= Convert.ToDecimal(i + 1) * Delta) //liegt x in dem intervall
                {

                    
                    Zähler[i]++; //dann zähle hoch und springe zurück 
                    return;
                }
                else
                {
                    i++;
                }
            }
            Zähler[i]++; //falls x in keinem Intervall lag heißt es, dass x zu groß war und deswegen rausfällt


        }
        /// <summary>
        /// implementiert Eine Art Mergesort in threads. Es werden 2 Teillisten erstellt die gleichzeitig sortiert werden.
        /// </summary>
        /// <param name="Liste">die zu sortierende Liste</param>
        /// <returns></returns>
        public static List<decimal> MergeSort(List<decimal> Liste)
        {
            List<decimal> liste1 = new List<decimal>();
            List<decimal> liste2 = new List<decimal>();
            int i = 0;
            int Count = Liste.Count();
            while (i < Convert.ToInt32(Math.Floor(Convert.ToDecimal(Count / 2))))
            {
                liste1.Add(Liste[0]);
                Liste.RemoveAt(0);
                i++;
            }
            while (i < Count)
            {
                liste2.Add(Liste[0]);
                Liste.RemoveAt(0);
                i++;
            }
            var Sort1 = new Thread(() => liste1.Sort());
            Sort1.Start();
            var Sort2 = new Thread(() => liste2.Sort());
            Sort2.Start();
            Sort1.Join();
            Sort2.Join();
            for (int j = 0; j < Count; j++)
            {
                if (liste1[0] < liste2[0])
                {
                    Liste.Add(liste1[0]);
                    liste1.RemoveAt(0);
                    if (liste1.Count() == 0)
                    {
                        while (liste2.Count() != 0)
                        {
                            j++;
                            Liste.Add(liste2[0]);

                            liste2.RemoveAt(0);
                        }


                    }
                }
                else if (liste1[0] >= liste2[0])
                {
                    Liste.Add(liste2[0]);

                    liste2.RemoveAt(0);
                    if (liste2.Count() == 0)
                    {
                        while (liste1.Count() != 0)
                        {
                            j++;
                            Liste.Add(liste1[0]);

                            liste1.RemoveAt(0);
                        }
                    }
                }
            }
            liste1.Clear();
            liste2.Clear();

            return Liste;
        }
    }
}




//beispiel für Lambda:
//System.Linq.Expressions.Expression<Func<int, int>> e = x => x * x;
//Console.WriteLine(e);
//// Output:
//// x => (x * x)