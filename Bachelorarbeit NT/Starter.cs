using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;

namespace Bachelorarbeit_NT
{/// <summary>
/// die Starter Klasse kümmert sich um organisation der Form wie alles berechnet werden soll. Es wurde sehr viel Abstrahiert. 
/// </summary>
    public class Starter
    {
        public List<Thread> worker = new List<Thread>(); //Ich erstelle eine Liste von Threads die Aktiv sind.
        public Starter(int workerNum)
        {

            Channel<Coordinate> jobChannel = Channel.CreateBounded<Coordinate>(33554432);  //erstelle den Job Queue
            Channel<Result> resultChannel = Channel.CreateBounded<Result>(33554432) ;   //erstelle die result queue
                                                                             
            CancellationTokenSource ctsrc = new CancellationTokenSource();   //der CancellationToken.

            var Ausgabe = new Thread(() => DbWorker(resultChannel, "connection", ctsrc.Token));// lambda ausdruck um den thread zu initialisieren.
            Ausgabe.Start();   
            worker.Add(Ausgabe);

            for (int i = 0; i < workerNum; i++)
            {
                var trh = new Thread(() => Worker(jobChannel, resultChannel, ctsrc.Token)); //hier werden Worker erstellt mittels lambda ausdruck man braucht hier eine Anonyme Funktion der man diese Parameter übergibt
                trh.Start();                                                                    //beispiel für einen "verständlicheren" Lambda findet man unten
                worker.Add(trh);                                                                //ich kenne den Worker ja sogesehen nicht. Der bekommt erst später einen genauen Typen.
            }

            var JobProd = new Thread(() => JobProducer(Term.TermType.QuadraticTwo, jobChannel, 1, 10000000000000000000)); //hier startet der Job Producer
            JobProd.Start();
            worker.Add(JobProd);
        }

        public async void Worker(ChannelReader<Coordinate> jobChan, ChannelWriter<Result> resultChan, CancellationToken cToken)
        {
            while (await jobChan.WaitToReadAsync()) //Warte bis irgendwas in der queue ist um es zu berechnen
            {
                while (jobChan.TryRead(out var jobItem)) //wenn etwas in der Queue ist versuche es zu nehmen
                {
                    var s = jobItem.Alpha(); // finde raus welches Item du gerade berechnet hast ( alpha)
                    
                    
                    decimal result = jobItem.Calc();   //Berechne es.
                    await resultChan.WriteAsync(new Result(s, result));  //warte bis du das Ergebnis in die Queue schreiben kannst.
                }

                if (cToken.IsCancellationRequested == true)
                {
                    Console.WriteLine("Worker Fertig");
                    return; }
               

            }
          
        }
        public async void JobProducer(Term.TermType typ, ChannelWriter<Coordinate> jobChan, decimal start, decimal ende)
        {
            Term t = null;  //standart wert des Typen (Welches alpha soll berechnet werden
            switch (typ) //switch case für den Typen Man muss den Workern ja den Typ der Rechnung übergeben
            {
                case Term.TermType.QuadraticTwo:
                    t = new RootOfTwo();
                    break;
                case Term.TermType.Zeta3:
                    t = new Zeta3();
                    break;
                case Term.TermType.Euler:
                    t = new Euler();
                    break;
                default: throw new ArgumentException();
            }
            decimal x = 1, y = 1;

            while (x * x <= ende) //gehe diagonal über das Feld um schneller Ergebnisse zu bekommen
            {
                decimal x2 = x;
                y = 1M;
                while (x2 >= 1)
                {
                   
                    await jobChan.WriteAsync(new Coordinate(t, x2, y)); //schreibe einen Job in die "Queue" der bearbeitet werden soll 
                    x2 = x2 - 1;
                    y++;
                }
                x++;
                

            }
            Console.WriteLine("Producer Fertig");

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

        public async void DbWorker(ChannelReader<Result> resultChan, string connectionString, CancellationToken cToken) //Das ist der DB worker der schreibt alles  in die Datenbank
        {
            StringBuilder connectDB1 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
            connectDB1.Remove(connectDB1.Length - 5, 5);
            connectDB1.Append("Values.db");        //Datenbank heißt Values;
            StringBuilder hilfsstring = new StringBuilder(@"URI=file:");
            hilfsstring.Append(connectDB1);
            string connectDB = Convert.ToString(hilfsstring); //ConnectDB ist der String mit dem ich die DB öffne.

            Create_Database(connectDB);
         
            using (var connection = new SQLiteConnection(connectDB))
            { //warte bis etwas in dem Channel ist
                while (await resultChan.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
                { //versuche es rauszulesen
                    while (resultChan.TryRead(out var jobItem))
                    {
                        // if(jobItem.type)
                        string s = jobItem.type;
                        if (s == "RootOfTwo")
                        {
                            connection.Open(); //nicht performant benutze transaction:  https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert Die Idee wäre hier: Ers
                            var cmd = new SQLiteCommand(connection);
                            cmd.CommandText = @"INSERT INTO Wurzel2(Value) VALUES(@Value)";
                            cmd.Parameters.AddWithValue("@Value", jobItem.result);
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                            connection.Close();
                            
                        }
                        //Console.WriteLine(jobItem);
                        //@TODO insertCREATE TABLE "Produkt" ()
                   }

                    if (cToken.IsCancellationRequested == true)
                    {
                        Console.WriteLine("DB Worker Fertitg"); 
                        return;
                        
                    }
                    
                }
                

            }

        
        }/// <summary>
        /// Hier werden die Tables erst gedroppt und dann neu erstellt. Somit fängt die Berechnung von neu an. Man hat keine Inkonsistenten Daten
        /// </summary>
        /// <param name="connect">Connection String für die database</param>
        public  void Create_Database(string connect)
        {
            using (var connection = new SQLiteConnection(connect))
            {
                connection.Open();
                var cmd =  new SQLiteCommand(connection);
                cmd.CommandText = "DROP TABLE IF EXISTS Wurzel2";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DROP TABLE IF EXISTS Eulersche_Zahl";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DROP TABLE IF EXISTS Zeta3";
                cmd.ExecuteNonQuery();
                
                //SQL befehle für das ERstellen der Datenbank
                cmd.CommandText = @"CREATE TABLE Wurzel2 (

                    ID    INTEGER NOT NULL UNIQUE,

                    Value NUMERIC NOT NULL UNIQUE,
                    PRIMARY KEY(ID)
                    )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Eulersche_Zahl (

                     ID    INTEGER NOT NULL UNIQUE,

                    Value NUMERIC UNIQUE NOT NULL,
                    PRIMARY KEY(ID)
                )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Zeta3 (

                     ID    INTEGER NOT NULL UNIQUE,

                    Value NUMERIC UNIQUE NOT NULL,
                    PRIMARY KEY(ID)
                )";
                cmd.ExecuteNonQuery();
                //Hier werden nun Indexe auf die Values gesetzt damit man einfacher drauf zugreifen kann ( Sortierung)
                //Dies hat einen sehr großen Einfluss auf die Laufzeit des Programmes. 
                //Da ich nicht weiß in welcher Reihenfolge die Werte rein geschrieben werden 
                //Sorge ich nachher mit Select befehl das die sehr schnell aufsteigend sortiert werden
                cmd.CommandText= @"CREATE INDEX Sortiert_Euler ON Eulersche_Zahl(
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

    }
}
//beispiel für Lambda:
//System.Linq.Expressions.Expression<Func<int, int>> e = x => x * x;
//Console.WriteLine(e);
//// Output:
//// x => (x * x)