using System;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.IO;
using System.Text;


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

            cmd.CommandText = "DROP TABLE IF EXISTS cars";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE cars(id INTEGER PRIMARY KEY,
            name TEXT, price INT)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Audi',52642)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Mercedes',57127)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Skoda',9000)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Volvo',29000)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Bentley',350000)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Citroen',21000)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Hummer',41400)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "INSERT INTO cars(name, price) VALUES('Volkswagen',21600)";
            cmd.ExecuteNonQuery();

            con.Close();
            Console.WriteLine("Table cars created");
            
            con.Open();

            string stm = "SELECT * FROM cars LIMIT 5";

            
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Console.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetInt32(2)}");
            }

            Console.ReadLine();
        }
    }
}

