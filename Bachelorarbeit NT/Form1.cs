using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace Bachelorarbeit_NT
{
    public partial class Form1 : Form
    {
        string connectDB = Application.StartupPath;

        static int cpus = 12;
        static double n1 = 10 * 10e17;
        static ulong n = Convert.ToUInt64(n1);
        CancellationTokenSource ctsrc = new CancellationTokenSource();
        public static bool saved = false;

        public Form1()
        {
            InitializeComponent();
            // var Start = new Starter(cpus, n, ctsrc.Token);
            StringBuilder connectEuler = new StringBuilder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)); //man erfährt wo die DB liegt. Insbesondere sorgen die Nächsten Zeilen code für die DB
            connectEuler.Remove(connectEuler.Length - 5, 5);
            connectEuler.Append("Euler.db");
            StringBuilder Euler2 = new StringBuilder(@"URI=file:");
            Euler2.Append(connectEuler);
            string DBEuler = Convert.ToString(Euler2);
            var connection = new SQLiteConnection(DBEuler);
            connection.Open();
            connection.LoadExtension("mod_spatialite");

            var command = connection.CreateCommand();
            command.CommandText =
            @"
    SELECT spatialite_version()
";
            var version = (string)command.ExecuteScalar();

            Console.WriteLine($"Using SpatiaLite {version}");


            command.CommandText =
            @"
                            SELECT Max(Value)
                            FROM Eulersche_Zahl
                           
                            ";

            var fromdb = command.ExecuteScalar();
           
            connection.Close();



        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
            

        }
        public static void Change_Text()
        {

        }
        public static void save()
        {
            saved = true;

        }
        private void Cancel()
        {
            
        }
    }
}
