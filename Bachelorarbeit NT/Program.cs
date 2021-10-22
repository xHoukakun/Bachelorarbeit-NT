using System;
using System.Windows.Forms;

namespace Bachelorarbeit_NT
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); //ich arbeite aufgrund der besseren Sichtbarkeit gerne mit Windows Forms.
        }
    }
}
