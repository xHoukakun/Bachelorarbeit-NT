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
        private int Counter = 200_000;

        private ulong AnzahlWurzel2;
        private decimal MaxWurzel2;
        private ulong AnzahlEuler;
        private decimal MaxEuler;
        private ulong AnzahlZeta3;
        private decimal MaxZeta3;
    
        public Starter(int workerNum, ulong N, CancellationToken cToken)
        {

            AnzahlWerte = N;
            this.workerNum = workerNum;
            Channel<Coordinate> jobChannel = Channel.CreateBounded<Coordinate>(65536);  //erstelle den Job Queue 8388608
            Channel<Result> resultChannel = Channel.CreateBounded<Result>(33554432);   //erstelle die result queue 
            Channel<Listb> dbChannelW2 = Channel.CreateBounded<Listb>(8); //erstelle einen DB channel              
            Channel<Listb> dbChannelEuler = Channel.CreateBounded<Listb>(8);
            Channel<Listb> dbChannelZeta3 = Channel.CreateBounded<Listb>(8);

            

            Database_Management(dbChannelW2, dbChannelEuler, dbChannelZeta3, cToken); //hier wird die Datenbank erstellt. Ich habe das in eine Methode ausgelagert für die Lesbarkeit

            {
                var Binder1 = new Thread(() => binder(resultChannel, dbChannelEuler, dbChannelW2, dbChannelZeta3, cToken));// lambda ausdruck um den thread zu initialisieren.
                Binder1.Start();
                worker.Add(Binder1);
                var Binder2 = new Thread(() => binder(resultChannel, dbChannelEuler, dbChannelW2, dbChannelZeta3, cToken));// lambda ausdruck um den thread zu initialisieren.
                Binder2.Start();
                worker.Add(Binder2);
                var Binder3 = new Thread(() => binder(resultChannel, dbChannelEuler, dbChannelW2, dbChannelZeta3, cToken));
                Binder3.Start();
                worker.Add(Binder3);
                var Binder4 = new Thread(() => binder(resultChannel, dbChannelEuler, dbChannelW2, dbChannelZeta3, cToken));
                Binder4.Start();
                worker.Add(Binder4); 
            } //Hier werden 4 Binder erstellt 
              //Erstelle Worker
            var controller = new Thread(() => Controller(jobChannel, resultChannel, dbChannelEuler, dbChannelW2, dbChannelZeta3, cToken));
            controller.Start();
            worker.Add(controller);
            for (int i = 0; i < workerNum; i++)
            {
                var trh = new Thread(() => Worker(jobChannel, resultChannel, cToken, N));        //hier werden Worker erstellt mittels lambda ausdruck man braucht hier eine Anonyme Funktion der man diese Parameter übergibt
                trh.Start();                                                                    //beispiel für einen "verständlicheren" Lambda findet man unten
                worker.Add(trh);                                                                //ich kenne den Worker ja sogesehen nicht. Der bekommt erst später einen genauen Typen.
            }
            dbcreater.ForEach(delegate (Thread thread) //Warte auf die erstellung der Datenbanken
            {

                thread.Join();


            });

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
                     
                        switch (s)
                        {
                            case "RootOfTwo":

                                if (AnzahlWurzel2 == AnzahlWerte)  //Hier passiert laufzeit optimierung. Wenn ich n werte habe brauche ich ja nur kleinere Werte. 
                                {                                  
                                    if (result > MaxWurzel2)
                                    {

                                    }
                                    else if (result < MaxWurzel2)
                                    {
                                        MaxWurzel2 = result;
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
                                        MaxEuler = result;
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
                                        MaxZeta3 = result;
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
            if (File.Exists("Merken.txt"))
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
            }
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
        public class Listb
        {
            public List<Decimal> r = new List<Decimal>();
            public string type;
            public Listb(List<Decimal> _r, string t) //eine Liste wird mit Call by referenz übergeben das müssen wir ändern da wir ansonsten ein problem bekommen
            {
                for (int i = 0; i < _r.Count; i++)
                {
                    r.Add(_r[i]);
                }

                type = t;

            }
            public List<decimal> getList()
            {
                return r;
            }
        }  //Datentyp für den Datenbank Channel
        /// <summary>
        /// Hier ist ein Binder, dieser nimmt sachen aus dem Channel  packt es in Päckchen und sortiert es in jeweilige Channels
        /// </summary>
        /// <param name="resultChan">Aus diesem Channel nimmt der Binder</param>
        /// <param name="DBEuler">Datenbank Channel Euler</param>
        /// <param name="DBWurzel2">Datenbank Channel Wurzel2</param>
        /// <param name="DBZeta3">Datenbank channel Zeta3 </param>
        /// <param name="cToken"></param>
        public async void binder(ChannelReader<Result> resultChan, ChannelWriter<Listb> DBEuler, ChannelWriter<Listb> DBWurzel2, ChannelWriter<Listb> DBZeta3, CancellationToken cToken) //Das ist der DB worker der schreibt alles  in die Datenbank
        {




            List<Decimal> RootOfTwo = new List<Decimal>();
            List<Decimal> Zeta3 = new List<Decimal>();
            List<Decimal> Euler = new List<Decimal>();
            while (await resultChan.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            {

               
                    while (resultChan.TryRead(out var jobItem))
                    {

                        string s = jobItem.type;   //in Welche Liste muss es einsortiert werden
                        decimal u = jobItem.result; //das ergebnis
                        switch (s)
                        {
                            case "RootOfTwo":
                                RootOfTwo.Add(u);
                                if (RootOfTwo.Count == Counter) //Päckchen von 200_000 Einheiten. Für Bulk Insert
                                {
                                    await DBWurzel2.WriteAsync(new Listb(RootOfTwo, "RootOfTwo"));
                                    RootOfTwo.Clear();

                                }
                                break;
                            case "Zeta3":
                                Zeta3.Add(u);
                                if (Zeta3.Count == Counter)
                                {
                                    await DBZeta3.WriteAsync(new Listb(Zeta3, "Zeta3"));
                                    Zeta3.Clear();
                                }
                                break;
                            case "Euler":
                                Euler.Add(u);
                                if (Euler.Count == Counter)
                                {
                                    await DBEuler.WriteAsync(new Listb(Euler, "Euler"));
                                    Euler.Clear();
                                }
                                break;
                            default: throw new ArgumentException(); //Falls es ein s gibt welches zu nichts passt
                        }
                    }
            }      
            if (RootOfTwo.Count != 0)  //in der Liste könnten Reste sein. Diese sollen natürlich in die DB geschrieben werden.
            {
                await DBWurzel2.WriteAsync(new Listb(RootOfTwo, "RootOfTwo"));
                RootOfTwo.Clear();
            }
            if (Euler.Count != 0)
            {
                await DBEuler.WriteAsync(new Listb(Euler, "Euler"));
                Euler.Clear();
            }
            if (Zeta3.Count != 0)
            {
                await DBZeta3.WriteAsync(new Listb(Zeta3, "Zeta3"));
                Zeta3.Clear();
            }
            Binder++;
            Thread.Sleep(2000);
            if (Binder == 4)  //Wenn alle Fertig sind schließt der Letzte die Channels.
            {
               

                DBEuler.TryComplete();
                DBZeta3.TryComplete();
                DBWurzel2.TryComplete();

            }
            return;
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

                 

                    Value REAL NOT NULL UNIQUE,
                    PRIMARY KEY(Value)
                    )WITHOUT ROWID;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Eulersche_Zahl (

                    

                    Value REAL NOT NULL UNIQUE,
                    PRIMARY KEY(Value)
                )WITHOUT ROWID;";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Zeta3 (

                    

                    Value REAL NOT NULL UNIQUE,
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
        /// Bulk Insert für Performance
        /// </summary>
        /// <param name="dbChannel"></param>
        /// <param name="Connect"></param>
        /// <param name="cToken"></param>
        public async void BulkInsert(ChannelReader<Listb> dbChannel, string Connect, CancellationToken cToken) // https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert 
        {
            
            while( await dbChannel.WaitToReadAsync())   //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            { //versuche es rauszulesen
                while (dbChannel.TryRead(out var jobItem))
                {
                    

                    var connection = new SQLiteConnection(Connect); //SQLite wrapper
                    connection.Open();  //hier wird die Connection zur Datenbank geöffnet


                    using (var transaction = connection.BeginTransaction())  //eine Transaktion wird begonnen 
                    {
                        string Tabelname = jobItem.type; 
                        List<decimal> Value = jobItem.getList();  //call by refrence 

                        var command = connection.CreateCommand();
                        switch (Tabelname)
                        {
                            case "RootOfTwo":
                                command.CommandText = @"INSERT or REPLACE INTO Wurzel2(Value) VALUES(@Value) "; //falls diese Zahl schon existiert wird sie erstetzt. Die Zahl kann nur einmal vor kommen.
                                command.Prepare();
                                break;
                            case "Euler":
                                command.CommandText = @"INSERT or REPLACE INTO Eulersche_Zahl(Value) VALUES(@Value) ";
                                command.Prepare();
                                break;
                            case "Zeta3":
                                command.CommandText = @"INSERT or REPLACE INTO Zeta3(Value) VALUES(@Value) ";
                                command.Prepare();
                                break;
                            default: throw new ArgumentException();
                        }
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@Value";
                        command.Parameters.Add(parameter);

                        for (int i = 0; i < Value.Count(); i++)
                        {


                            command.Parameters.AddWithValue("@Value", Value[i]);
                            command.Prepare();
                            command.ExecuteNonQuery();  //Bulk insert
                                
                            


                        }

                        transaction.Commit(); //Transaktion wird durchgeführt
                        connection.Close(); //die Connection wird geschlossen
                        var gc = new Thread(() => GC.Collect());
                        gc.Start();   //hier lasse ich den Garbage Collector laufen. das reduziert die Speicherauslastung. 


                    }



                }
            
               
                    
               
                
            
        
    


            }
            AnzahlDB++;

        }
        public async void BulkInsertWurzel2(ChannelReader<Listb> dbChannel,string Connect,CancellationToken cToken)
        {
            BulkInsert(dbChannel, Connect, cToken); //Rufe Buld Insert mit dem jeweiligen Channel auf( Code redundanz)
                   
        }
        public async void BulkInsertZeta3(ChannelReader<Listb> dbChannel, string Connect, CancellationToken cToken)
        {
            BulkInsert(dbChannel, Connect, cToken); //Rufe Buld Insert mit dem jeweiligen Channel auf( Code redundanz)
            
        }
        public async void BulkInsertEuler(ChannelReader<Listb> dbChannel, string Connect, CancellationToken cToken)
        {
            BulkInsert(dbChannel, Connect, cToken); //Rufe Buld Insert mit dem jeweiligen Channel auf( Code redundanz)
              
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
        public async void Controller(ChannelReader<Coordinate> jobChannel, ChannelReader<Result> ResultChannel, ChannelReader<Listb> DBA, ChannelReader<Listb> DBB, ChannelReader<Listb> DBC,CancellationToken cToken)
        {
            
            while(cToken.IsCancellationRequested==false) //wenn der Token false ist dann wird nichts gemacht.
            {
                Thread.Sleep(1000);
                Console.WriteLine(AnzahlDB);
            }
            if (cToken.IsCancellationRequested == true) //wenn der Token auf True fällt dann wird ein sicherer abbruch initialisiert.
            {
                Counter = 1_000_000;
                while (true)
                {

                    Console.WriteLine(AnzahlDB);
                    if ((jobChannel.Completion.IsCompleted && ResultChannel.Completion.IsCompleted && DBA.Completion.IsCompleted && DBB.Completion.IsCompleted && DBC.Completion.IsCompleted)) 
                    {


                           

                           
                                
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
        public void Database_Management(ChannelReader<Listb> dbChannelW2, ChannelReader<Listb> dbChannelEuler, ChannelReader<Listb> dbChannelZeta3, CancellationToken cToken)
        {
            if (File.Exists("Anzahl.txt"))
            {
                string file = "Anzahl.txt";

                StreamReader reader = new StreamReader(file);
                AnzahlWurzel2 = Convert.ToUInt64(reader.ReadLine()); //Ein Controller der alles überwacht aber kein Item rausnimmt. 
                MaxWurzel2 = Convert.ToDecimal(reader.ReadLine());
                AnzahlEuler = Convert.ToUInt64(reader.ReadLine());
                MaxEuler = Convert.ToDecimal(reader.ReadLine());
                AnzahlZeta3 = Convert.ToUInt64(reader.ReadLine());
                MaxZeta3 = Convert.ToDecimal(reader.ReadLine());
                reader.Close();
            }
            else
            {
                StringBuilder connectZeta3 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
                connectZeta3.Remove(connectZeta3.Length - 5, 5);
                connectZeta3.Append("Zeta3.db");
                StringBuilder zeta3 = new StringBuilder(@"URI=file:");
                zeta3.Append(connectZeta3);
                string DBZeta3 = Convert.ToString(zeta3);

                var crDB = new Thread(() => Create_Database(DBZeta3));
                crDB.Start();
                dbcreater.Add(crDB);

                StringBuilder connectEuler = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
                connectEuler.Remove(connectEuler.Length - 5, 5);
                connectEuler.Append("Euler.db");
                StringBuilder Euler2 = new StringBuilder(@"URI=file:");
                Euler2.Append(connectEuler);
                string DBEuler = Convert.ToString(Euler2);

                var crDB1 = new Thread(() => Create_Database(DBEuler));
                crDB1.Start();
                dbcreater.Add(crDB1);

                StringBuilder connectWurzel2 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
                connectWurzel2.Remove(connectWurzel2.Length - 5, 5);
                connectWurzel2.Append("Wurzel2.db");
                StringBuilder W2 = new StringBuilder(@"URI=file:");
                W2.Append(connectWurzel2);
                string DBW2 = Convert.ToString(W2);

                var crDB2 = new Thread(() => Create_Database(DBW2));
                crDB2.Start();
                dbcreater.Add(crDB2);
            }
        
            {
                StringBuilder connectWurzel2 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
                connectWurzel2.Remove(connectWurzel2.Length - 5, 5);
                connectWurzel2.Append("Wurzel2.db");
                StringBuilder W2 = new StringBuilder(@"URI=file:");
                W2.Append(connectWurzel2);
                string DBW2 = Convert.ToString(W2);

                var Wurzel2t = new Thread(() => BulkInsertWurzel2(dbChannelW2, DBW2, cToken));
                Wurzel2t.Start();
                worker.Add(Wurzel2t);

            }
            {
                StringBuilder connectEuler = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
                connectEuler.Remove(connectEuler.Length - 5, 5);
                connectEuler.Append("Euler.db");
                StringBuilder Euler2 = new StringBuilder(@"URI=file:");
                Euler2.Append(connectEuler);
                string DBEuler = Convert.ToString(Euler2);


                var DBEulert = new Thread(() => BulkInsertEuler(dbChannelEuler, DBEuler, cToken));
                DBEulert.Start();
                worker.Add(DBEulert);

            }
            {
                StringBuilder connectZeta3 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
                connectZeta3.Remove(connectZeta3.Length - 5, 5);
                connectZeta3.Append("Zeta3.db");
                StringBuilder zeta3 = new StringBuilder(@"URI=file:");
                zeta3.Append(connectZeta3);
                string DBZeta3 = Convert.ToString(zeta3);

                var DBZeta3t = new Thread(() => BulkInsertZeta3(dbChannelZeta3, DBZeta3, cToken));
                DBZeta3t.Start();
                worker.Add(DBZeta3t);
            }

        }
    }
}

        
    

//beispiel für Lambda:
//System.Linq.Expressions.Expression<Func<int, int>> e = x => x * x;
//Console.WriteLine(e);
//// Output:
//// x => (x * x)