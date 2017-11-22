using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace SmartMeter_P1
    {
    public partial class Form1 : Form
    {
        SerialPort ComPort = new SerialPort();
        
        internal delegate void SerialDataReceivedEventHandlerDelegate(object sender, SerialDataReceivedEventArgs e);
        internal delegate void SerialPinChangedEventHandlerDelegate(object sender, SerialPinChangedEventArgs e);
        private SerialPinChangedEventHandler SerialPinChangedEventHandler1;
        delegate void SetTextCallback(string text);
        string InputData = String.Empty;

        List<string> MeterData = new List<string>();
        long messagesReceived = 0;

        mySettings settings = new mySettings();
        mySQL mySQL = new mySQL();
        myFunctions func = new myFunctions();

        int iTime = 0;

        decimal i1_7_0_total = 0;
        int i1_7_0_count = 0;

        public Form1()
        {
            InitializeComponent();
            SerialPinChangedEventHandler1 = new SerialPinChangedEventHandler(PinChanged);
            ComPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(port_DataReceived_1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //settings
            settings.settingsFile = "settings.ini";

            //mySQL
            //mysql @ FitPC2
            mySQL.server = settings.getSetting("Server");
            mySQL.uid = settings.getSetting("User");
            mySQL.pwd = settings.getSetting("Password");
            mySQL.database = settings.getSetting("Database");

            lblMySQLserver.Text = mySQL.server;
            lblMySQLdatabase.Text = mySQL.database;

            mySQL.connect();
            lblMySQLconnection.Text = mySQL.conn.State.ToString();

            val_1_8_1.Text = "0.000";
            val_1_8_2.Text = "0.000";
            val_1_7_0.Text = "0.000";
            val_96_14_0.Text = "0.000";
            val_24_2_1.Text = "0.000";

            //serial communication settings
            string[] ArrayComPortsNames = null;
            int index = -1;
            string ComPortName = null;

            //Com Ports
            ArrayComPortsNames = SerialPort.GetPortNames();

            if (ArrayComPortsNames.Count() > 0)
            {
                do
                {
                    index += 1;
                    cboPorts.Items.Add(ArrayComPortsNames[index]);
                } while (!((ArrayComPortsNames[index] == ComPortName) || (index == ArrayComPortsNames.GetUpperBound(0))));

                Array.Sort(ArrayComPortsNames);

                if (index == ArrayComPortsNames.GetUpperBound(0))
                {
                    ComPortName = ArrayComPortsNames[0];
                }
                //xxx get first item print in text
                cboPorts.Text = "COM4";
            }

            //Baud Rate
            cboBaudRate.Items.Add(300);
            cboBaudRate.Items.Add(600);
            cboBaudRate.Items.Add(1200);
            cboBaudRate.Items.Add(2400);
            cboBaudRate.Items.Add(9600);
            cboBaudRate.Items.Add(14400);
            cboBaudRate.Items.Add(19200);
            cboBaudRate.Items.Add(38400);
            cboBaudRate.Items.Add(57600);
            cboBaudRate.Items.Add(115200);
            cboBaudRate.Items.ToString();
            //get first item print in text
            cboBaudRate.Text = cboBaudRate.Items[9].ToString();
            //Data Bits
            cboDataBits.Items.Add(7);
            cboDataBits.Items.Add(8);
            //get the first item print it in the text 
            cboDataBits.Text = cboDataBits.Items[1].ToString();

            //Stop Bits
            cboStopBits.Items.Add("One");
            cboStopBits.Items.Add("OnePointFive");
            cboStopBits.Items.Add("Two");
            //get the first item print in the text
            cboStopBits.Text = cboStopBits.Items[0].ToString();
            //Parity 
            cboParity.Items.Add("None");
            cboParity.Items.Add("Even");
            cboParity.Items.Add("Mark");
            cboParity.Items.Add("Odd");
            cboParity.Items.Add("Space");
            //get the first item print in the text
            cboParity.Text = cboParity.Items[0].ToString();
            //Handshake
            cboHandShaking.Items.Add("None");
            cboHandShaking.Items.Add("XOnXOff");
            cboHandShaking.Items.Add("RequestToSend");
            cboHandShaking.Items.Add("RequestToSendXOnXOff");
            //get the first item print it in the text 
            cboHandShaking.Text = cboHandShaking.Items[0].ToString();


            //CONNECT
            if (btnPortState.Text == "Closed")
            {
                btnPortState.Text = "Open";
                ComPort.PortName = Convert.ToString(cboPorts.Text);
                ComPort.BaudRate = Convert.ToInt32(cboBaudRate.Text);
                ComPort.DataBits = Convert.ToInt16(cboDataBits.Text);
                ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cboStopBits.Text);
                ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), cboHandShaking.Text);
                ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), cboParity.Text);
                ComPort.Open();
            }

            lblConnectionStatus.Text = btnPortState.Text;

        }




        private void port_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            InputData = ComPort.ReadExisting();
            if (InputData != String.Empty)
            {
                this.BeginInvoke(new SetTextCallback(SetText), new object[] { InputData });
            }
        }
       
        private void SetText(string text)
        {
            try
            {
                tmr1sec.Enabled = true;

                //crc16 crc = new crc16();
                //bool checksum = crc.Check(this.rtbIncoming.Text, "0C97");

                //rtbIncoming.Text += text;

                //Work on the received data
                //http://www.yellownote.nl/blog/diy-monitor-smartmeter/
                string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    Match match;

                    //Meter Reading electricity delivered to client (low tariff)
                    //1-0:1.8.1(000687.138*kWh)
                    match = Regex.Match(line, @"[0-9]-[0-9]:1.8.1\([0]{1,}(.*)\*kWh\)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string valuepart = line.Split(new char[] { '(', ')' })[1];
                        string value = valuepart.Split(new char[] { '*' })[0];
                        val_1_8_1.Text = Convert.ToDecimal(value.Replace(".",",")).ToString();

                        messagesReceived++;
                        lblMessages.Text = messagesReceived.ToString();
                    }

                    //Meter Reading electricity delivered to client (normal tariff)
                    //1-0:1.8.2(000523.849*kWh)
                    match = Regex.Match(line, @"[0-9]-[0-9]:1.8.2\([0]{1,}(.*)\*kWh\)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string valuepart = line.Split(new char[] { '(', ')' })[1];
                        string value = valuepart.Split(new char[] { '*' })[0];
                        val_1_8_2.Text = Convert.ToDecimal(value.Replace(".", ",")).ToString();
                    }

                    //Actual electricity power delivered to client (+P)
                    //1-0:1.7.0(00.428*kW)
                    match = Regex.Match(line, @"[0-9]-[0-9]:1.7.0\([0]{1,}(.*)\*kW\)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string valuepart = line.Split(new char[] { '(', ')' })[1];
                        string value = valuepart.Split(new char[] { '*' })[0];
                        decimal val = Math.Round(Convert.ToDecimal(value.Replace(".", ",")) * 1000,0);
                        val_1_7_0.Text = val.ToString();

                        i1_7_0_total += val;
                        i1_7_0_count++;

                    }

                    //Tariff indicator electricity
                    //0-0:96.14.0(0001)
                    match = Regex.Match(line, @"[0-9]-[0-9]:96.14.0\([0]{1,}", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string valuepart = line.Split(new char[] { '(', ')' })[1];
                        string value = valuepart.Split(new char[] { '*' })[0];
                        val_96_14_0.Text = Convert.ToDecimal(value.Replace(".", ",")).ToString();
                    }

                    //
                    //0-1:24.2.1(141227170000W)(00435.247*m3)
                    match = Regex.Match(line, @"[0-9]-[0-9]:24.2.1\([0-9]{12}[S,W]\)\([0]{1,}(.*)\*m3\)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        string valuepart = line.Split(new char[] { '(', ')' })[3];
                        string value = valuepart.Split(new char[] { '*' })[0];
                        val_24_2_1.Text = Convert.ToDecimal(value.Replace(".", ",")).ToString();

                        mySQL.insertMeasurementLIVE(val_1_8_1.Text, val_1_8_2.Text, val_1_7_0.Text, val_96_14_0.Text, val_24_2_1.Text);
                    }

                    
                }
            }
            catch (Exception ex)
            {
                string message = ex.ToString();
                message = message;
            }

        }
        internal void PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            SerialPinChange SerialPinChange1 = 0;
            bool signalState = false;

            SerialPinChange1 = e.EventType;
            //lblCTSStatus.BackColor = Color.Green;
            //lblDSRStatus.BackColor = Color.Green;
            //lblRIStatus.BackColor = Color.Green;
            //lblBreakStatus.BackColor = Color.Green;
            switch (SerialPinChange1)
            {
                case SerialPinChange.Break:
                    //lblBreakStatus.BackColor = Color.Red;
                    //MessageBox.Show("Break is Set");
                    break;
                case SerialPinChange.CDChanged:
                    signalState = ComPort.CtsHolding;
                  //  MessageBox.Show("CD = " + signalState.ToString());
                    break;
                case SerialPinChange.CtsChanged:
                    signalState = ComPort.CDHolding;
                    //lblCTSStatus.BackColor = Color.Red;
                    //MessageBox.Show("CTS = " + signalState.ToString());
                    break;
                case SerialPinChange.DsrChanged:
                    signalState = ComPort.DsrHolding;
                    //lblDSRStatus.BackColor = Color.Red;
                    // MessageBox.Show("DSR = " + signalState.ToString());
                    break;
                case SerialPinChange.Ring:
                    //lblRIStatus.BackColor = Color.Red;
                    //MessageBox.Show("Ring Detected");
                    break;
            }
        }

        private void btnPortState_Click(object sender, EventArgs e)
        {
            if (btnPortState.Text == "Closed")
            {
                btnPortState.Text = "Open";
                ComPort.PortName = Convert.ToString(cboPorts.Text);
                ComPort.BaudRate = Convert.ToInt32(cboBaudRate.Text);
                ComPort.DataBits = Convert.ToInt16(cboDataBits.Text);
                ComPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cboStopBits.Text);
                ComPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), cboHandShaking.Text);
                ComPort.Parity = (Parity)Enum.Parse(typeof(Parity), cboParity.Text);
                ComPort.Open();
            }
            else if (btnPortState.Text == "Open")
            {
                btnPortState.Text = "Closed";
                ComPort.Close();
               
            }

            tmr1sec.Enabled = false;

            lblConnectionStatus.Text = btnPortState.Text;
        }

        private void tmr1sec_Tick(object sender, EventArgs e)
        {
            iTime++;

            if (iTime >= 60)
            {
                //Store the collected data into the database

                //val_1_8_1   -> `p1_meterreading_in_1` decimal(10,3),
                //val_1_8_2   -> `p1_meterreading_in_2` decimal(10,3),
                //val_1_7_0   -> `p1_current_power_in` decimal(10,3),
                //val_96_14_0 -> `p1_current_tariff` integer,
                //val_24_2_1  -> `p1_channel_1_meterreading` decimal(10,3),

                string i1_7_0_average;
                if (i1_7_0_count > 0)
                {
                    i1_7_0_average = Convert.ToInt32(i1_7_0_total / i1_7_0_count).ToString();
                }
                else
                {
                    i1_7_0_average = val_1_7_0.Text;
                }
                i1_7_0_total = 0;
                i1_7_0_count = 0;
                
                mySQL.insertMeasurement(val_1_8_1.Text, val_1_8_2.Text, i1_7_0_average, val_96_14_0.Text, val_24_2_1.Text);

                iTime = 0;
            }
        }
               

        

      
    }
}
