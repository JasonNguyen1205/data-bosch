using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Melsec;

namespace data_bosch
{
    public partial class PCBInjection : Form, INotifyPropertyChanged
    {
        FX3U plc = new FX3U("com5",9600,7,System.IO.Ports.Parity.Even,System.IO.Ports.StopBits.One);
        string HU { get; set; } = "";
        float pos1 { get; set; } = 0;
        float pos2 { get; set; } = 0;
        double demen1 { get; set; } =  0;
        double demen2 { get; set; } =  0;
        public double HiLimit { get; private set; }
        public double LoLimit { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        PropertyChangedEventArgs PropertyChangedEventArgs;
        public PCBInjection()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (plc.Open()) 
            {
                status.Text=("connect");
                Timer timer= new Timer();
            }
            else
            {
                status.Text = ("disconnect");
            }

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
         
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            plc.Close();
        }

        private async void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Enter)
            {
                lblPos1.ForeColor=Color.Black; lblPos2.ForeColor=Color.Black;
                lblPos1.Text = "Done";
                lblPos2.Text = "Done";
                HiLimit = 7.22;
                LoLimit = 7.0;
                plc.WriteDWord(62, 0);
                HU = textBox1.Text;
                pos1 = 0;
                label1.Text = pos1.ToString("#.##");
                pos2 = 0;
                label2.Text = pos2.ToString("#.##");
                pos1 = 0; 
                pos2 = 0;
                demen1 = 0;
                demen2 = 0;
                textBox1.Text = "";
                textBox1.Enabled = false;
                textBox1.BackColor = Color.Gray;
                lblPos1.Text = "Processing";
                lblPos2.Text = "Processing";

                PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(demen1));

                plc.WriteDWord(60, 1);
                do
                {
                    Debug.WriteLine("Processing");
                    pos1 = plc.ReadFloat(12);
                    label1.Text = pos1.ToString("#.##");
                    pos2 = plc.ReadFloat(6);
                    label2.Text = pos2.ToString("#.##");
                    Debug.WriteLine(pos1);
                    Debug.WriteLine(pos2);
                    await Task.Delay(1000);
                }
                while (pos1==0||pos2==0);

                demen1 = pos1;
                demen2 = pos2;
                lblPos1.Text = demen1 < HiLimit ? "Done": "Fail";
                lblPos1.ForeColor = demen1 < HiLimit ? Color.Green : Color.Red;
                lblPos2.Text = demen2 < HiLimit ? "Done": "Fail";
                lblPos2.ForeColor = demen2 < HiLimit ? Color.Green : Color.Red;

                PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(demen1));

                plc.WriteDWord(60, 0);
                plc.WriteDWord(62, 1);
                textBox1.Text = "";
                textBox1.Enabled = true;
                textBox1.BackColor = Color.White;
                textBox1.Focus();
            }
        }


    }
}
