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

        public bool producer_finished = false;
        public bool worker_finished = false;
        public bool binder_finished = false;
        public bool DBConnect_finished = false;
        public int workerNum = 0;
        public int workerFinished = 0;
        public int Binder = 0;
        
        public Starter(int workerNum,ulong N)
        {
            this.workerNum = workerNum;
            Channel<Coordinate> jobChannel = Channel.CreateBounded<Coordinate>(8388608);  //erstelle den Job Queue 8388608
            Channel<Result> resultChannel = Channel.CreateBounded<Result>(33554432);   //erstelle die result queue 33554432
            Channel<Listb> dbChannel = Channel.CreateBounded<Listb>(256); //erstelle einen DB channel

            CancellationTokenSource ctsrc = new CancellationTokenSource();   //der CancellationToken.


            StringBuilder connectDB1 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
            connectDB1.Remove(connectDB1.Length - 5, 5);
            connectDB1.Append("Values.db");        //Datenbank heißt Values;
            StringBuilder hilfsstring = new StringBuilder(@"URI=file:");
            hilfsstring.Append(connectDB1);
            string connectDB = Convert.ToString(hilfsstring); //ConnectDB ist der String mit dem ich die DB öffne.
            Create_Database(connectDB);

            var crDB = new Thread(() => Create_Database(connectDB));
            crDB.Start();
            var DB = new Thread(() => BulkInsert(dbChannel, connectDB, ctsrc.Token));
            DB.Start();
            worker.Add(DB);
            
            {
                var Binder1 = new Thread(() => binder(resultChannel, dbChannel, ctsrc.Token));// lambda ausdruck um den thread zu initialisieren.
                Binder1.Start();
                worker.Add(Binder1);
                var Binder2 = new Thread(() => binder(resultChannel, dbChannel, ctsrc.Token));// lambda ausdruck um den thread zu initialisieren.
                Binder2.Start();
                worker.Add(Binder2);
                var Binder3 = new Thread(() => binder(resultChannel, dbChannel, ctsrc.Token));
                Binder3.Start();
                worker.Add(Binder3);
                var Binder4 = new Thread(() => binder(resultChannel, dbChannel, ctsrc.Token));
                Binder4.Start();
                worker.Add(Binder4);
            }

            for (int i = 0; i < workerNum; i++)
            {
                var trh = new Thread(() => Worker(jobChannel, resultChannel, ctsrc.Token,N)); //hier werden Worker erstellt mittels lambda ausdruck man braucht hier eine Anonyme Funktion der man diese Parameter übergibt
                trh.Start();                                                                    //beispiel für einen "verständlicheren" Lambda findet man unten
                worker.Add(trh);                                                                //ich kenne den Worker ja sogesehen nicht. Der bekommt erst später einen genauen Typen.
            }
            crDB.Join();
            var JobProd = new Thread(() => JobProducer( jobChannel,ctsrc.Token,N)); //hier startet der Job Producer
            JobProd.Start();
            worker.Add(JobProd);
  
        }

        public async void Worker(ChannelReader<Coordinate> jobChan, ChannelWriter<Result> resultChan, CancellationToken cToken,ulong N)
        {
            while (await jobChan.WaitToReadAsync()) //Warte bis irgendwas in der queue ist um es zu berechnen
            {
                while (jobChan.TryRead(out var jobItem)) //wenn etwas in der Queue ist versuche es zu nehmen
                {
                    var s = jobItem.Alpha(); // finde raus welches Item du gerade berechnet hast ( alpha)

                    
                    decimal result = jobItem.Calc();   //Berechne es.
                    if (result < N) {await resultChan.WriteAsync(new Result(s, result)); } //Warte bis du auf die Result schreiben kannst
                     

                }
                if(jobChan.Count == 0&&producer_finished)
                {
                    workerFinished++;
                    

                    if (workerFinished == workerNum)
                    {
                        worker_finished = true;
                        
                    }
                    
                    while (DBConnect_finished == false)
                    {
                        Thread.Sleep(1000);
                    }
                    

                }
            

                if (cToken.IsCancellationRequested == true)
                {
                    Console.WriteLine("Worker Unterbrochen");
                    await resultChan.WaitToWriteAsync(cToken);
                    return;
                }
               

            }
            

        }
        public async void JobProducer(ChannelWriter<Coordinate> jobChan, CancellationToken cToken, ulong ende)
        {
       
            decimal x = 1, y = 1;

            while (x * x <= ende) //gehe diagonal über das Feld um schneller Ergebnisse zu bekommen
            {
                decimal x2 = x;
                y = 1M;
                while (x2 >= 1)
                {

                    await jobChan.WriteAsync(new Coordinate(new RootOfTwo(), x2, y)); //schreibe einen Job in die "Queue" der bearbeitet werden soll 
                    await jobChan.WriteAsync(new Coordinate(new Zeta3(), x2, y));
                    await jobChan.WriteAsync(new Coordinate(new Euler(), x2, y));
                    x2 = x2 - 1;
                    y++;
                }
                x++;


            }
            
            producer_finished = true;
            while(DBConnect_finished == false) 
            {
                Thread.Sleep(1000);
            }
            Console.WriteLine("Alles beendet");
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
            public Listb(List<Decimal> _r, string t)
            {
                for(int i = 0; i < _r.Count; i++)
                {
                    r.Add(_r[i]);
                }
                
                type = t;
                
            }
            public List<decimal> getList()
            {
                return r;
            }
        }

        public async void binder(ChannelReader<Result> resultChan, ChannelWriter<Listb> DBChannel, CancellationToken cToken) //Das ist der DB worker der schreibt alles  in die Datenbank
        {



           //warte bis etwas in dem Channel ist
                List<Decimal> RootOfTwo = new List<Decimal>();
                List<Decimal> Zeta3 = new List<Decimal>();
                List<Decimal> Euler = new List<Decimal>();
                while (await resultChan.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
                { //versuche es rauszulesen
                    while (resultChan.TryRead(out var jobItem))
                    {

                        string s = jobItem.type;
                        decimal u = jobItem.result;
                        switch (s)
                        {
                                case "RootOfTwo":
                                RootOfTwo.Add(u);
                                if (RootOfTwo.Count == 200_000)
                                {
                                    await DBChannel.WriteAsync(new Listb(RootOfTwo, "RootOfTwo"));                                  
                                    RootOfTwo.Clear();
                                    
                                }
                                break;
                            case "Zeta3":
                                Zeta3.Add(u);
                                if (Zeta3.Count == 200_000)
                                {
                                await DBChannel.WriteAsync(new Listb(Zeta3, "Zeta3"));
                                Zeta3.Clear();
                                }
                                break;
                            case "Euler":
                                Euler.Add(u);
                                if (Euler.Count == 200_000)
                                {
                                await DBChannel.WriteAsync(new Listb(Euler, "Euler"));
                                Euler.Clear();
                                }
                                break;
                            default: throw new ArgumentException();
                        }
                        /*if (s == "RootOfTwo")
                        {
                            connection.Open(); //nicht performant benutze transaction:  https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert Die Idee wäre hier: Ers
                            var cmd = new SQLiteCommand(connection);
                            cmd.CommandText = @"INSERT INTO Wurzel2(Value) VALUES(@Value)";
                            cmd.Parameters.AddWithValue("@Value", jobItem.result);
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();
                            connection.Close();

                        }*/

                    }
                    if(worker_finished==true&&resultChan.Count==0)
                    {
                    await DBChannel.WriteAsync(new Listb(RootOfTwo, "RootOfTwo"));
                    await DBChannel.WriteAsync(new Listb(Zeta3, "Zeta3"));
                    await DBChannel.WriteAsync(new Listb(Euler, "Euler"));

                    Binder++;
                    if(Binder==2)
                    {
                        binder_finished = true;
                    }
                    
                    while (DBConnect_finished == false)
                    {
                        Thread.Sleep(1000);
                    }
                    }
                    
                    if (cToken.IsCancellationRequested == true)
                    {
                    await DBChannel.WriteAsync(new Listb(RootOfTwo, "RootOfTwo"));
                    await DBChannel.WriteAsync(new Listb(Zeta3, "Zeta3"));
                    await DBChannel.WriteAsync(new Listb(RootOfTwo, "Euler"));
                    await DBChannel.WaitToWriteAsync(cToken);
                        
                        return;

                    }

                }


            


        }/// <summary>
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
                cmd.CommandText = @"CREATE TABLE Wurzel2 (

                    ID    INTEGER NOT NULL UNIQUE,

                    Value DECIMAL(65,30) NOT NULL UNIQUE,
                    PRIMARY KEY(ID)
                    )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Eulersche_Zahl (

                     ID    INTEGER NOT NULL UNIQUE,

                    Value DECIMAL(65,30) NOT NULL UNIQUE,
                    PRIMARY KEY(ID)
                )";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE Zeta3 (

                     ID    INTEGER NOT NULL UNIQUE,

                    Value DECIMAL(65,30) NOT NULL UNIQUE,
                    PRIMARY KEY(ID)
                )";
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
        public async void BulkInsert(ChannelReader<Listb> dbChannel, string Connect, CancellationToken cToken)
        {
            while (await dbChannel.WaitToReadAsync())    //diese beiden While Schleifen sorgen insbesondere dafür, dass es async ist.  Und man kein Deadlock szenario bekomm ( Auch bekannt als Philosophen Problem)
            { //versuche es rauszulesen
                while (dbChannel.TryRead(out var jobItem))
                {
                    using (var connection = new SQLiteConnection(Connect))
                    {

                        connection.Open();
                        string Tabelname = jobItem.type;
                        List<decimal> Value = jobItem.getList();
                        
                        using (var transaction = connection.BeginTransaction())
                        {
                            var command = connection.CreateCommand();
                            switch (Tabelname)
                            {
                                case "RootOfTwo":
                                    command.CommandText = @"INSERT INTO Wurzel2(Value) VALUES(@Value) ";
                                    command.Prepare();
                                    break;
                                case "Euler":
                                    command.CommandText = @"INSERT INTO Eulersche_Zahl(Value) VALUES(@Value) ";
                                    command.Prepare();
                                    break;
                                case "Zeta3":
                                    command.CommandText = @"INSERT INTO Zeta3(Value) VALUES(@Value) ";
                                    command.Prepare();
                                    break;
                                default: throw new ArgumentException();
                            }
                            var parameter = command.CreateParameter();
                            parameter.ParameterName = "@Value";
                            command.Parameters.Add(parameter);
                      
                            for (int i = 0;i<Value.Count(); i++)
                            {
                                
                                
                                    command.Parameters.AddWithValue("@Value", Value[i]);
                                    command.Prepare();
                                    command.ExecuteNonQuery();
                                
                               
                            }

                            transaction.Commit();
                            connection.Close();
                            var gc = new Thread(() => GC.Collect());
                            gc.Start();

                        }
                    }
                    if(binder_finished==true&&dbChannel.Count==0)
                    {
                       
                        DBConnect_finished = true;
                    }
                    if (cToken.IsCancellationRequested == true)
                    {
                        Console.WriteLine("DB ist fertig");
                        

                    }
                }
            }

        }
    }
}
//beispiel für Lambda:
//System.Linq.Expressions.Expression<Func<int, int>> e = x => x * x;
//Console.WriteLine(e);
//// Output:
//// x => (x * x)