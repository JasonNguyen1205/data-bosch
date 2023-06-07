using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Melsec;

namespace data_bosch
{
    public partial class PCBInjection : Form, INotifyPropertyChanged
    {
        FX3U plc = new FX3U("com5", 9600, 7, System.IO.Ports.Parity.Even, System.IO.Ports.StopBits.One);
        string HU { get; set; } = "";
        float pos1 { get; set; } = 0;
        float pos2 { get; set; } = 0;
        double demen1 { get; set; } = 0;
        double demen2 { get; set; } = 0;
        public double HiLimit { get; private set; }
        public double LoLimit { get; private set; }
        private static Regex re2 = new Regex("^\\d{8}([-])\\d{7}([-])\\d{7}([-])\\d{3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public event PropertyChangedEventHandler PropertyChanged;
        PropertyChangedEventArgs PropertyChangedEventArgs;
        public PCBInjection()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbxCom.Items.Clear();
            List<String> tList = new List<String>();
            foreach (string s in SerialPort.GetPortNames())
            {
                tList.Add(s);
            }
            tList.Sort();
            
            cbxCom.Items.AddRange(tList.ToArray());
            txtBarcode.Enabled = false;
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
                lblStatus.Text = "";
                var checkBarcode = false;
                txtBarcode.Enabled = false;
                txtBarcode.BackColor = Color.Gray;
                checkBarcode = re2.IsMatch(txtBarcode.Text);
                if (checkBarcode==false)
                {
                    lblStatus.Text = "Wrong Barcode!";
                    txtBarcode.Text = "";
                    txtBarcode.Enabled = true;
                    txtBarcode.BackColor = Color.White;
                    txtBarcode.Focus();
                    plc.WriteDWord(60, 0);
                    plc.WriteDWord(62, 1);
                    return;

                }
                lblPos1.ForeColor = Color.Black; lblPos2.ForeColor = Color.Black;
                //lblPos1.Text = "Done";
                //lblPos2.Text = "Done";
                HiLimit = 7.8;
                LoLimit = 7.0;
                plc.WriteDWord(62, 0);
                HU = txtBarcode.Text;
                pos1 = 0;
                label1.Text = pos1.ToString("#.###");
                pos2 = 0;
                label2.Text = pos2.ToString("#.###");
                pos1 = 0;
                pos2 = 0;
                demen1 = 0;
                demen2 = 0;
                lblPos1.Text = "Processing";
                lblPos2.Text = "Processing";
                string apiStatus = "Fail";
                PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(demen1));

                plc.WriteDWord(60, 1);
                do
                {
                    Debug.WriteLine("Processing");
                    pos1 = plc.ReadFloat(12);
                    label1.Text = pos1.ToString("#.###");
                    pos2 = plc.ReadFloat(6);
                    label2.Text= pos2.ToString("#.###");
                    Debug.WriteLine(pos1);
                    Debug.WriteLine(pos2);
                    await Task.Delay(10);
                }
                while (pos1 == 0 || pos2 == 0);

                demen1 = pos1;
                demen2 = pos2;
                lblPos1.Text =  demen1 < HiLimit ? "Pass" : "Fail";
                lblPos1.ForeColor = demen1 < HiLimit ? Color.Green : Color.Red;
                if (lblPos1.Text.Equals("Pass")) {
                    lblPos1.Text = demen1 >= LoLimit ? "Pass" : "Fail";
                    lblPos1.ForeColor = demen1 >= LoLimit ? Color.Green : Color.Red;
                }
                lblPos2.Text = demen2 < HiLimit ? "Pass" : "Fail";
                lblPos2.ForeColor = demen2 < HiLimit ? Color.Green : Color.Red;
                if (lblPos2.Text.Equals("Pass")) {
                    lblPos2.Text = demen2 >= LoLimit ? "Pass" : "Fail";
                    lblPos2.ForeColor = demen2 >= LoLimit ? Color.Green : Color.Red;
                }
                plc.WriteDWord(60, 0);
                plc.WriteDWord(62, 1);
                if (lblPos1.Text.Equals("Pass") && lblPos2.Text.Equals("Pass"))
                {
                    apiStatus = await InsertHeightCheckData(txtBarcode.Text, 1, System.Environment.MachineName, HiLimit + "-" + demen1.ToString("#.###") + "-" + demen2.ToString("#.###"));
                }
                else {
                    apiStatus = await InsertHeightCheckData(txtBarcode.Text, 0, System.Environment.MachineName, HiLimit + "-" + demen1.ToString("#.###") + "-" + demen2.ToString("#.###"));
                }
                if (apiStatus.Equals("OK")) {
                    lblStatus.Text = "Insert Success!";
                    lblStatus.ForeColor = Color.Green;
                }
                else
                { 
                    lblStatus.Text = "FAIL!!!";
                    lblStatus.ForeColor = Color.Red;
                }
                PropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(demen1));

                
                txtBarcode.Text = "";
                txtBarcode.Enabled = true;
                txtBarcode.BackColor = Color.White;
                txtBarcode.Focus();
            }
        }

        HttpClient _httpClient;
        private async Task<string> InsertHeightCheckData(string barcode, int status, string machine, string resultTest)
        {
            _httpClient = new HttpClient();
            string apiStatus = "Fail";
            var rq = new HttpRequestMessage();
            rq.Method = HttpMethod.Post;
            var requestStr = $"http://fvn-s-web01:5000/api/ProcessLock/FA/InsertPCBHeightCheckAsync/" + barcode.ToString() + "/" + status + "/" + machine + "/" + resultTest;
            Debug.WriteLine(requestStr);
            rq.RequestUri = new Uri(requestStr);
            var rs = await _httpClient.SendAsync(rq);
            if (rs.StatusCode == System.Net.HttpStatusCode.OK)
            {
                apiStatus = "OK";
            }
            else {
                apiStatus = "Fail";
            }
            return apiStatus;
        }

        private void lblPos1_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void cbxCom_SelectedIndexChanged(object sender, EventArgs e)
        {
            plc = new FX3U(cbxCom.SelectedItem.ToString(), 9600, 7, System.IO.Ports.Parity.Even, System.IO.Ports.StopBits.One);
            lblPos1.Text = string.Empty;
            lblPos2.Text = string.Empty;
            cbxCom.Enabled = false;
            if (plc.Open())
            {
                lblStatus.Text = ("Connected PLC");
                Timer timer = new Timer();
                txtBarcode.Enabled = true;
                txtBarcode.BackColor = Color.White;
                txtBarcode.Focus();
                //try {
                //    plc.WriteDWord(60, 0);
                //    plc.WriteDWord(62, 1);
                //} catch (Exception ex) {
                //    lblStatus.Text = ("Can't Connected PLC");
                //    txtBarcode.Enabled = false;
                //}
            }
            else
            {
                lblStatus.Text = ("Can't Connected PLC");
                txtBarcode.Enabled = false;
            }
        }
    }
}
