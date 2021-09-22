using System;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading;


namespace Hier_sind_Testinstanzen
{
    class Program
    {
        static void Main(string[] args)
        {




            Channel<ulong> jobChannel = Channel.CreateUnbounded<ulong>();
            var cr = new Thread(() => Schreiber(jobChannel));
            var cd = new Thread(() => Producer(jobChannel));
            cr.Start();
            cd.Start();
            /*

            StringBuilder connectDB1 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            connectDB1.Remove(connectDB1.Length - 6, 6);
            connectDB1.Append("Values.db");
            Console.WriteLine(connectDB1);
            Console.ReadKey();
            StringBuilder hilfsstring = new StringBuilder(@"URI=file:");
            hilfsstring.Append(connectDB1);


            List<Decimal> a = new List<Decimal>();
            int b = a.Count;


            string connectDB = Convert.ToString(hilfsstring);
            using var con = new SQLiteConnection(connectDB);
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = "DROP TABLE IF EXISTS data";

            cmd.CommandText = @"CREATE TABLE data (

                    ID    INTEGER NOT NULL UNIQUE,

                    VALUE NUMERIC,
                    PRIMARY KEY(ID)
                    )";
            cmd.ExecuteNonQuery();




            using (var transaction = con.BeginTransaction())
            {
                var command = con.CreateCommand();
                command.CommandText = @"INSERT INTO data(Value) VALUES(@Value) ";
                command.Prepare();

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@Value";
                command.Parameters.Add(parameter);

                // Insert a lot of data
                List<Decimal> Values = new List<decimal>();
                var random = new Random();
                for (var i = 0; i < 150_000; i++)
                {
                    Values.Add(random.Next());
                }
                Values.ForEach(delegate (Decimal value)
                {

                    command.Parameters.AddWithValue("@Value", value);
                    command.PrepareAsync();
                    command.ExecuteNonQueryAsync();


                });

                Values.Clear();



                transaction.CommitAsync();
                con.CloseAsync();
            }*/
        }

        public static async void Producer(ChannelWriter<ulong> jobProducer)
        {
            for (int i = 0; i < 1000; i++)
            {
                await jobProducer.WriteAsync(Convert.ToUInt64(i));
            }
            jobProducer.Complete();
            Console.WriteLine("Prod fertig");
            return;
        }
        public static async void Schreiber(ChannelReader<ulong> resultChan)
        {
            while (await resultChan.WaitToReadAsync())
            {
                while (resultChan.TryRead(out var jobItem))
                {
                    Console.WriteLine(jobItem);
                    Thread.Sleep(10);
                }
            }
            Console.WriteLine("Return");
            return;
        }
    }
}

