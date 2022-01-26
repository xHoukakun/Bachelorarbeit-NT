using System.Threading;
using System;
using System.IO;
namespace Bachelorarbeit_NT
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            string[] del = { "Zeta3", "Euler", "RootOfTwo" };
            ctsrc.Cancel();//Der Cancelation Token wird gesetzt damit werden die Threads beendet
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            while (!saved)  //solange kein Sicherers Beenden möglich ist passiert erstmal nichts
            {
                Thread.Sleep(1000);
                
            }
            speichern();
            if (delete) //Wenn zusätzlich alles Gelöscht werden soll wird das hier ausgeführt
            {
                
                if (File.Exists("Merken.txt"))
                {
                    File.Delete("Merken.txt");
                }
                if(File.Exists("Fertig.txt"))
                {
                    File.Delete("Fertig.txt");
                }
                foreach (string f in del)  
                {
                    if (File.Exists(f + "Statistik.txt"))
                    {
                        File.Delete(f + "Statistik.txt");
                    }
                    if (File.Exists(f + "Abstände.txt"))
                    {
                        File.Delete(f + "Abstände.txt");
                    }
                    if (File.Exists(f + "Min.txt"))
                    {
                        File.Delete(f + "Min.txt");
                    }
                    if (File.Exists(f + "Max.txt"))
                    {
                        File.Delete(f + "Max.txt");
                    }
                }
            }
            if (disposing && (components != null)) //Code von Visual Studio
            {
                components.Dispose();
            }
            Console.WriteLine("Sicher beendet");
            base.Dispose(disposing);
        }
        private void speichern()
        {
            StreamWriter sw1 = new StreamWriter("EulerMin" + ".txt");
            for (int i = 0; i < DelMinE.Count; i++)
            {
                sw1.WriteLine(DelMinE[i]);
            }
            sw1.Close();
            StreamWriter sw2 = new StreamWriter("EulerMax" + ".txt");
            for(int i=0;i<DelMaxE.Count;i++)
            {
                sw2.WriteLine(DelMaxE[i]);
            }
            sw2.Close();
            StreamWriter sw3 = new StreamWriter("Zeta3Max" + ".txt");
            for(int i=0;i<DelMaxZ.Count;i++)
            {
                sw3.WriteLine(DelMaxZ[i]);
            }
            sw3.Close();
            StreamWriter sw4 = new StreamWriter("Zeta3Min" + ".txt");
            for (int i = 0; i < DelMinZ.Count; i++)
            {
                sw4.WriteLine(DelMinZ[i]);
            }
            sw4.Close();
            StreamWriter sw5 = new StreamWriter("RootOfTwoMin" + ".txt");
            for(int i = 0;i< DelMinR.Count; i++)
            {
                sw5.WriteLine(DelMinR[i]);
            }
            sw5.Close();
            StreamWriter sw6 = new StreamWriter("RootOfTwoMax" + ".txt");
            for (int i = 0; i < DelMaxR.Count; i++)
            {
                sw6.WriteLine(DelMaxR[i]);
            }
            sw6.Close();
        }
        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SaveButton = new System.Windows.Forms.Button();
            this.bDelete = new System.Windows.Forms.Button();
            this.DelRootOfTwo = new System.Windows.Forms.Button();
            this.DelZeta3 = new System.Windows.Forms.Button();
            this.DelEuler = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.bRMin = new System.Windows.Forms.Button();
            this.dMinZ = new System.Windows.Forms.Button();
            this.bEMin = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(12, 12);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(1279, 573);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.Click += new System.EventHandler(this.chart1_Click);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(109, 591);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(93, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "RootOfTwo";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(208, 591);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(93, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Zeta3 ";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Enabled = false;
            this.button3.Location = new System.Drawing.Point(307, 591);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(95, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "Euler";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(1171, 554);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(90, 23);
            this.SaveButton.TabIndex = 10;
            this.SaveButton.Text = "SaveAsImage";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // bDelete
            // 
            this.bDelete.Location = new System.Drawing.Point(1082, 590);
            this.bDelete.Name = "bDelete";
            this.bDelete.Size = new System.Drawing.Size(75, 23);
            this.bDelete.TabIndex = 11;
            this.bDelete.Text = "DeleteAll";
            this.bDelete.UseVisualStyleBackColor = true;
            this.bDelete.Click += new System.EventHandler(this.bDelete_Click);
            // 
            // DelRootOfTwo
            // 
            this.DelRootOfTwo.Location = new System.Drawing.Point(109, 621);
            this.DelRootOfTwo.Name = "DelRootOfTwo";
            this.DelRootOfTwo.Size = new System.Drawing.Size(93, 23);
            this.DelRootOfTwo.TabIndex = 12;
            this.DelRootOfTwo.Text = "DelRootOfTwo";
            this.DelRootOfTwo.UseVisualStyleBackColor = true;
            this.DelRootOfTwo.Click += new System.EventHandler(this.DelRootOfTwo_Click);
            // 
            // DelZeta3
            // 
            this.DelZeta3.Location = new System.Drawing.Point(209, 621);
            this.DelZeta3.Name = "DelZeta3";
            this.DelZeta3.Size = new System.Drawing.Size(92, 23);
            this.DelZeta3.TabIndex = 13;
            this.DelZeta3.Text = "DelZeta3";
            this.DelZeta3.UseVisualStyleBackColor = true;
            this.DelZeta3.Click += new System.EventHandler(this.DelZeta3_Click);
            // 
            // DelEuler
            // 
            this.DelEuler.Location = new System.Drawing.Point(307, 621);
            this.DelEuler.Name = "DelEuler";
            this.DelEuler.Size = new System.Drawing.Size(95, 23);
            this.DelEuler.TabIndex = 14;
            this.DelEuler.Text = "DelEuler";
            this.DelEuler.UseVisualStyleBackColor = true;
            this.DelEuler.Click += new System.EventHandler(this.DelEuler_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 599);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Daten:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 626);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "Delta Max:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(34, 656);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Delta Min:";
            // 
            // bRMin
            // 
            this.bRMin.Location = new System.Drawing.Point(109, 651);
            this.bRMin.Name = "bRMin";
            this.bRMin.Size = new System.Drawing.Size(93, 23);
            this.bRMin.TabIndex = 18;
            this.bRMin.Text = "DelRootOfTwo";
            this.bRMin.UseVisualStyleBackColor = true;
            this.bRMin.Click += new System.EventHandler(this.bRMin_Click);
            // 
            // dMinZ
            // 
            this.dMinZ.Location = new System.Drawing.Point(209, 651);
            this.dMinZ.Name = "dMinZ";
            this.dMinZ.Size = new System.Drawing.Size(92, 23);
            this.dMinZ.TabIndex = 19;
            this.dMinZ.Text = "DelZeta3";
            this.dMinZ.UseVisualStyleBackColor = true;
            this.dMinZ.Click += new System.EventHandler(this.dMinZ_Click);
            // 
            // bEMin
            // 
            this.bEMin.Location = new System.Drawing.Point(307, 651);
            this.bEMin.Name = "bEMin";
            this.bEMin.Size = new System.Drawing.Size(95, 23);
            this.bEMin.TabIndex = 20;
            this.bEMin.Text = "DelEuler";
            this.bEMin.UseVisualStyleBackColor = true;
            this.bEMin.Click += new System.EventHandler(this.bEMin_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1386, 712);
            this.Controls.Add(this.bEMin);
            this.Controls.Add(this.dMinZ);
            this.Controls.Add(this.bRMin);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DelEuler);
            this.Controls.Add(this.DelZeta3);
            this.Controls.Add(this.DelRootOfTwo);
            this.Controls.Add(this.bDelete);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.chart1);
            this.Name = "Form1";
            this.Text = "BinäreQuadratischeFormen";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button bDelete;
        private System.Windows.Forms.Button DelRootOfTwo;
        private System.Windows.Forms.Button DelZeta3;
        private System.Windows.Forms.Button DelEuler;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bRMin;
        private System.Windows.Forms.Button dMinZ;
        private System.Windows.Forms.Button bEMin;
    }
}

