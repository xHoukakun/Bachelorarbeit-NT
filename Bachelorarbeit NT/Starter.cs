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
{
    public class Starter
    {
        public List<Thread> worker = new List<Thread>();
        public Starter(int workerNum)
        {

            Channel<Coordinate> jobChannel = Channel.CreateBounded<Coordinate>(2^31);  //erstelle den Job Queue
            Channel<Result> resultChannel = Channel.CreateBounded<Result>(2^31);   //erstelle die result queue
                                                                                 //datenbank verarbeiter muss ähnlivh gema<yvht werden  

            CancellationTokenSource ctsrc = new CancellationTokenSource();

            var asdfasfd = new Thread(() => JobProducer(Term.TermType.QuadraticTwo, jobChannel, 1, 10000000000000000000)); //hier startet der Job Producer
            asdfasfd.Start();
            worker.Add(asdfasfd);
            var Ausgabe = new Thread(() => DbWorker(resultChannel, "connection", ctsrc.Token));// lampda ausdruck um den thread zu initialisieren.
            Ausgabe.Start();
            worker.Add(Ausgabe);

            for (int i = 0; i < workerNum; i++)
            {
                var trh = new Thread(() => Worker(jobChannel, resultChannel, ctsrc.Token));
                trh.Start();
                worker.Add(trh);
            }
            
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
                { return; }


            }
        }
        public async void JobProducer(Term.TermType typ, ChannelWriter<Coordinate> jobChan, decimal start, decimal ende)
        {
            Term t = null;  //standart wert des Typen (Welches alpha soll berechnet werden
            switch (typ)
            {
                case Term.TermType.QuadraticTwo:
                    t = new RootOfTwo();
                    break;
                default: throw new ArgumentException();
            }
            decimal x = 1, y = 1;

            while (x * x <= ende)
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

            jobChan.Complete();  //wenn die Jobs fertig geladen wurden.
        }
        public class Result
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
            StringBuilder connectDB1 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //Ich erfahre wo die datenbank liegt.
            connectDB1.Remove(connectDB1.Length - 5, 5);
            connectDB1.Append("Values.db");        //Datenbank heißt Values;
            StringBuilder hilfsstring = new StringBuilder(@"URI=file:");
            hilfsstring.Append(connectDB1);
            string connectDB = Convert.ToString(hilfsstring); //ConnectDB ist der String mit dem ich die DB öffne.

            Create_Database(connectDB);
         
            using (var connection = new SQLiteConnection(connectDB))
            {
                while (await resultChan.WaitToReadAsync())
                {
                    while (resultChan.TryRead(out var jobItem))
                    {
                        // if(jobItem.type)
                        string s = jobItem.type;
                        if (s == "RootOfTwo")
                        {
                            connection.Open();
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
                        Console.WriteLine("Ich habe Fertig"); 
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
