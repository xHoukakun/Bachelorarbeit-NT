using System;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading;
using System.Linq;

namespace Hier_sind_Testinstanzen
{
    class Program
    {
        static void Main(string[] args)
        {




            List<int> Test = new List<int>();
            Test.Add(1);
            Test.Add(2);
            foreach (int element in Test)
            {
                Console.WriteLine(element);
            }
            Console.WriteLine(Test.Max());
            Console.WriteLine("For schleife:");
            for(int i=0;i<Test.Count;i++)
            {
                Console.WriteLine(Test[i]);
            }
            Console.WriteLine("Hallo");
            Test.RemoveAt(0);
            Console.WriteLine(Test[0]);

           
            Test.Add(1);
            Console.WriteLine("ollah");
            foreach(int element in Test)
            {
                Console.WriteLine(element);
            }
           
            Console.ReadKey();

           
           
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

      
    }
}

