using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bachelorarbeit_NT
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string connectDB=Application.StartupPath;

            int cpus = 20;
            ulong x = 1;
            ulong y = 1;
            double n1 = 10 * 10e17;
            ulong n = Convert.ToUInt64(n1);
            var asdf = new Starter(cpus,n);



        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
