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

namespace Bachelorarbeit_NT
{
    public partial class Form1 : Form
    {
        string connectDB = Application.StartupPath;

        static int cpus = 12;
        static ulong x = 1;
        static ulong y = 1;
        static double n1 = 10 * 10e17;
        static ulong n = Convert.ToUInt64(n1);
        CancellationTokenSource ctsrc = new CancellationTokenSource();
        public static bool saved = false;

        public Form1()
        {
            InitializeComponent();
            var Start = new Starter(cpus, n, ctsrc.Token);
           

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
