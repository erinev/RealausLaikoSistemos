using System;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using Erikas_MSP430.Properties;

namespace Erikas_MSP430
{
    public partial class MainForm : Form
    {
        private readonly SerialPort _serialPort;

        public MainForm()
        {
            InitializeComponent();
            _serialPort = new SerialPort();

            _serialPort.DataReceived += ReceivedMSP430Data;
        }

        private delegate void SetTextDeleg(string text);

        private void btnConnect_Click(object sender, EventArgs e)
        {
            SetPortSettings();

            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                }
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Unable to open serial port. port: {0}", _serialPort.PortName));
            }
        }

        private void ReceivedMSP430Data(object sender, SerialDataReceivedEventArgs e)
        {
            const int startIndex = 2;
            string data = _serialPort.ReadLine();
            data.Remove(startIndex);

            if (StringContains(data, "ok", StringComparison.InvariantCultureIgnoreCase))
            {
                ShowSuccessPopUp();
            }
            else
            {
                BeginInvoke(new SetTextDeleg(ProcessRecievedData), new object[] {data});
            }
        }

        private void ProcessRecievedData(string data)
        {
            TemperatureTextBox.Text = data;

            smallDBDataSet.temp_logRow newRow = smallDBDataSet1.temp_log.Newtemp_logRow();
            newRow.temperature = Convert.ToDouble(data);
            newRow.time = DateTime.Now;

            smallDBDataSet1.temp_log.Rows.Add(newRow);

            temp_logTableAdapter.Update(smallDBDataSet1.temp_log);

            chart2.Series["temp"].Points.AddXY(newRow.time, newRow.temperature);
            if (chart2.Series["temp"].Points.Count() > 10)
                chart2.Series["temp"].Points.RemoveAt(0);
            chart2.ResetAutoValues();
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            temp_logTableAdapter.Fill(smallDBDataSet1.temp_log);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        private void btnRedrawChart_Click(object sender, EventArgs e)
        {
            chart1.DataBind();
        }

        private void SetIntervalButton_Click(object sender, EventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            SetPortSettings();

            _serialPort.Open();
            _serialPort.Write(string.Format("LOOP_INTV:{0};", NewIntervalTextBox.Text));
        }

        private void SetPortSettings()
        {
            _serialPort.PortName = PortTextBox.Text;
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataBits = 8;
        }

        private void ShowSuccessPopUp()
        {
            MessageBox.Show(Resources.MainForm_ShowSuccessPopUp_Naujas_laiko_intervalas_nustatytas);
            NewIntervalTextBox.Text = String.Empty;
        }

        private bool StringContains(string source, string wordToFind, StringComparison comparison)
        {
            return source.IndexOf(wordToFind, comparison) >= 0;
        }
    }
}