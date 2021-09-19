using System;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace Hier_sind_Testinstanzen
{
    class Program
    {
        static void Main(string[] args)
        {


            StringBuilder connectDB1 = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            connectDB1.Remove(connectDB1.Length - 6, 6);
            connectDB1.Append("Values.db");
            Console.WriteLine(connectDB1);
            Console.ReadKey();
            StringBuilder hilfsstring = new StringBuilder(@"URI=file:");
            hilfsstring.Append(connectDB1);

         
       

            
            string connectDB = Convert.ToString(hilfsstring);
            using var con = new SQLiteConnection(connectDB);
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = "DROP TABLE IF EXISTS data";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DROP TABLE IF EXISTS cars";
            cmd.ExecuteNonQuery();
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
                
                Values.ForEach(delegate (Decimal value)
                {
                    
                    command.Parameters.AddWithValue("@Value", value);
                    command.Prepare();
                    command.ExecuteNonQuery();

                });
                  
                Values.Clear();
                    
                

                transaction.Commit();
            }
        }
    }
}

