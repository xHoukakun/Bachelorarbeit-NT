﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;


namespace Hier_sind_Testinstanzen
{
    class Program
    {
        static int workernum = 0;
        static void Main(string[] args)
        {

            Channel<int> JobChan = Channel.CreateBounded<int>(1024);
            Channel<int> ResultChan = Channel.CreateBounded<int>(1024);

            /*
            var a = new Thread(() => ender(ResultChan));
            a.Start();
            var b = new Thread(() => worker(ResultChan, JobChan, ResultChan));
            b.Start();
            var c = new Thread(() => worker(ResultChan, JobChan, ResultChan));
            c.Start();
            var b1 = new Thread(() => worker(ResultChan, JobChan, ResultChan));
            b1.Start();
            var c1 = new Thread(() => worker(ResultChan, JobChan, ResultChan));
            c1.Start();
            var b2 = new Thread(() => worker(ResultChan, JobChan, ResultChan));
            b2.Start();
            var c3 = new Thread(() => worker(ResultChan, JobChan, ResultChan));
            c3.Start();
            var d = new Thread(() => producer(100000, JobChan));
            d.Start();
            Console.ReadKey();


            */
            List<decimal> hey = new List<decimal>();
            hey.Add(1);
            hey.Add(2);
            hey.Add(3);
            hey.Add(4);
            hey.Add(5);
            hey.Add(6);
            hey.Add(7);
            hey.Add(8);
            hey.Add(9);
            hey.Add(10);

            for (int i = 0; i < hey.Count(); i++)
            {
                Console.WriteLine("Hey");
            }
            Console.WriteLine(hey[hey.Count-1]);


            /*


                        List<decimal> Test = new List<decimal>();
                        /*   Test = InsertionSort(1, Test);
                           Test = InsertionSort(1.5M, Test);
                           Test = InsertionSort(0, Test);
                           Test = InsertionSort(2, Test);
                           Test = InsertionSort(5, Test);
                           Test = InsertionSort(3, Test);
                           Test = InsertionSort(-1, Test);
                           Test = InsertionSort(2.5M, Test);
                           Test = InsertionSort(4, Test);
                           Test = InsertionSort(2.4M, Test);  
                        Random zufall = new Random();
                        int k = 0;
                        for (int j = 0; Test.Count() <100000 ;)
                        {
                            decimal y = Convert.ToDecimal(100 * zufall.NextDouble());
                            if(Test.Contains(y))
                            {
                                Console.WriteLine("Fehler beim Einfügen");
                                j++;
                                k = j;
                            }
                            else
                            {
                                Console.WriteLine("Fügte {0} hinzu", y);
                                Test.Add(y);

                            }

                        }
                        Console.WriteLine(k);
                        Console.WriteLine(Test.Count());
                        Console.ReadKey();

                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        Test.Sort();
                        watch.Stop();
                        Console.Write("Sortieren: ");
                        Console.WriteLine(watch.Elapsed);
                        Test.Add(0);
                        //  for (int i = 0; i < Test.Count(); i++) 
                        //{
                        // Console.Write(i);
                        //  Console.Write(" ");
                        //   Console.WriteLine(Test[i]);

                        // }
                        Stopwatch watch2 = new Stopwatch();
                        decimal x = 0;
                        watch2.Start();
                        for (int i = 0; i < Test.Count(); i++)
                        {
                            if (i - 1 < 100)
                            {
                                if (Test[i] > Test[i + 1])
                                {

                                    Console.WriteLine("Fehler {0}", i);

                                }
                                x = Math.Max(x, Test[i]);
                            }
                            else
                            {
                                if (Test[i] < x)
                                {
                                    Console.WriteLine("Fehler Maximum");
                                }
                            }
                        }
                        watch2.Stop();


                        Console.Write("Zum Kontrollieren: ");
                        Console.WriteLine(watch2.Elapsed);
                        Console.ReadKey();

                        */

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
        public static async void producer(int n, ChannelWriter<int> jobChan)
        {
            for(int i = 0; i < n; i++)
            {
                await jobChan.WriteAsync(i);
            }
            jobChan.TryComplete();
            return;
        }
        public static async void worker(ChannelWriter<int> ResultChan, ChannelReader<int> jobChan, ChannelReader<int> ResultChanRead)
        {
            while (await jobChan.WaitToReadAsync()) //Warte bis irgendwas in der queue ist um es zu berechnen
            {
                while (jobChan.TryRead(out var jobItem)) //wenn etwas in der Queue ist versuche es zu nehmen
                {
                    int u = jobItem;
                    await ResultChan.WriteAsync(u);
                }
            }
            workernum++;
            while(!ResultChanRead.Completion.IsCompleted)
            {
                Thread.Sleep(1000);
                if(workernum>=6)
                {
                    Console.WriteLine("Fertig");
                    ResultChan.TryComplete();
                }
            }
            return;
        }
        public static async void ender(ChannelReader<int> jobChan)
        {
            while (await jobChan.WaitToReadAsync()) //Warte bis irgendwas in der queue ist um es zu berechnen
            {
                while (jobChan.TryRead(out var jobItem))
                {
                    Console.WriteLine(jobItem);
                }
            }
            Console.ReadKey();
            return;
        }
        public static List<decimal> InsertionSort(decimal insert, List<decimal> Liste)
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

