using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualsTS
{
    public partial class AnimationTS : Form
    {
        /// <summary>
        /// Button upon clicking which the visualization starts.
        /// </summary>
        Button btn;

        /// <summary>
        /// Input to the csv file is provided here.
        /// </summary>
        RichTextBox inputPath;

        /// <summary>
        /// It stores the created tenant shards(shown by a square).
        /// </summary>
        List<RichTextBox> tenantShards = new List<RichTextBox>();

        /// <summary>
        /// The number of minutes simulation was ruun.
        /// </summary>
        int numberOfMinutes = 0;

        /// <summary>
        /// Stores data for every TS wether it contains fresh data or stale data at that time.
        /// </summary>
        string[,] data;

        public AnimationTS()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Text = "Seconds:";
            richTextBox2.Text = "0";
            richTextBox3.BackColor = Color.Black;
            richTextBox4.BackColor = Color.Red;
            richTextBox5.BackColor = Color.Green;
            richTextBox6.Text = "Stale/No data";
            richTextBox7.Text = "Fresh data";
            richTextBox8.Text = "Simulation over";
            btn = new Button();
            btn.Text = "Click here";
            btn.Height = 40;
            btn.Width = 60;
            btn.Left = 700;
            inputPath = new RichTextBox();
            inputPath.Height = 40;
            inputPath.Width = 500;
            inputPath.Left = 200;
            inputPath.Text = "Enter location of file";
            inputPath.Enter += new EventHandler(inputPath_Enter);
            inputPath.Leave += new EventHandler(inputPath_Leave);
            this.Controls.Add(btn);
            this.Controls.Add(inputPath);
            btn.Click += new EventHandler(button1_Click);
        }

        private void inputPath_Enter(object sender, EventArgs e)
        {
            if (inputPath.Text == "Enter location of file")
            {
                inputPath.Text = "";
            }
        }

        private void inputPath_Leave(object sender, EventArgs e)
        {
            if (inputPath.Text == "")
            {
                inputPath.Text = "Enter location of file";
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            string path;
            path = inputPath.Text;
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                string[] col = lines[1].Split(',');
                int numberOfTS = Convert.ToInt32(col[4]);   //col[4] contains number of tenant shards.
                numberOfMinutes = Convert.ToInt32(col[9]) + 1;  //col[9] contains the config parameter: number of minutes
                int fanout = Convert.ToInt32(col[24]); //col[24] contains config param: fanout
                data = new string[numberOfTS, numberOfMinutes * 6];
                int flag = 0;
                foreach (string line in lines)
                {
                    if (flag == 0)
                    {
                        flag = 1;
                        continue;
                    }
                    string[] columns = line.Split(',');
                    data[Convert.ToInt32(columns[2]), Convert.ToInt32(columns[1])] = columns[3];
                }

                int numberOfLevels = 1;
                int totalTSTillThisLevel = 1;
                while (totalTSTillThisLevel < numberOfTS)
                {
                    totalTSTillThisLevel += (int)Math.Pow(fanout, numberOfLevels);
                    numberOfLevels++;
                }

                int currentTS = 1;
                for (int i = 0; i < numberOfLevels; i++)
                {
                    int numberOfTSAtThisLevel = (int)Math.Pow(fanout, i);
                    int currentNumberOfTSAtThisLevel = 0;
                    while (currentTS <= numberOfTS && currentNumberOfTSAtThisLevel < numberOfTSAtThisLevel)
                    {
                        RichTextBox r = new RichTextBox();
                        r.Height = 20;
                        r.Width = 20;
                        r.Left = currentNumberOfTSAtThisLevel * 21;
                        r.Top = (i + 1) * 21;
                        tenantShards.Add(r);
                        this.Controls.Add(r);
                        currentTS++;
                        currentNumberOfTSAtThisLevel++;
                    }
                }
                Thread t = new Thread(new ThreadStart(SayHiThread));
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                inputPath.Text = ex.Message;
            }

        }

        private void SayHiThread()
        {
            for (int i = 0; i < numberOfMinutes * 6; i++)
            {
                Invoke(new SayHiDelegate(SayHi), i);
                Thread.Sleep(1000);
            }
        }

        private void SayHi(int timeInterval)
        {
            richTextBox2.Text = Convert.ToString(timeInterval * 10);
            for (int i = 0; i < tenantShards.Count; i++)
            {
                if (data[i, timeInterval] == null)
                    tenantShards[i].BackColor = Color.Green;
                else if (data[i, timeInterval].Equals("true", StringComparison.OrdinalIgnoreCase))
                    tenantShards[i].BackColor = Color.Red;
                else if (data[i, timeInterval].Equals("false", StringComparison.OrdinalIgnoreCase))
                    tenantShards[i].BackColor = Color.Black;
                else
                    tenantShards[i].BackColor = Color.Green;
            }
        }
    }
    public delegate void SayHiDelegate(int minute);
}