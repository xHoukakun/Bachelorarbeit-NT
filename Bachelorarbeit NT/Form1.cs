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

        static int cpus = 4;
        static double n1 = 10 * 10e17;
        static ulong n = Convert.ToUInt64(n1);
        CancellationTokenSource ctsrc = new CancellationTokenSource();
        public static bool saved = false;
        static List<ulong> StatisticRootOfTwo = new List<ulong>();
        static List<ulong> StatisticEuler = new List<ulong>();
        static List<ulong> StatisticZeta3 = new List<ulong>();
        public Form1()
        {
            InitializeComponent();
            var Start = new Starter(cpus, 40, 1_000,ctsrc.Token);
    

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
        public static void Liste_RootOfTwo(List<ulong> Liste)
        {
            ulong Zähle = 0;
            for (int i = 0; i<Liste.Count();i++)
            {
                StatisticRootOfTwo.Add(Liste[i]);
                Zähle = Zähle + Liste[i];
            }
           
            Console.WriteLine("RootOfTwo gesamt: {0}", Zähle);
        }
        public static void Liste_Euler(List<ulong> Liste)
        {
            ulong Zähle = 0;
            for (int i = 0; i < Liste.Count(); i++)
            {
                StatisticEuler.Add(Liste[i]);
                Zähle = Zähle + Liste[i];
            }
            
            Console.WriteLine("Euler gesamt: {0}", Zähle);
        }
        public static void Liste_Zeta3(List<ulong> Liste)
        {
            ulong Zähle = 0;
            for (int i = 0; i < Liste.Count(); i++)
            {
                StatisticZeta3.Add(Liste[i]);
                Zähle = Zähle + Liste[i];
            }
         
            Console.WriteLine("Zeta3 gesamt: {0}", Zähle);
        }
        private void Cancel()
        {
            
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
