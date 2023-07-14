using BelugaBox_Stream.Models;
using BelugaBox_Stream.Constants;
using BelugaBox_Stream.Interfaces;
using System.Net.WebSockets;
using BelugaBox_Stream.Properties;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json;
using DotNetEnv;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;




namespace BelugaBox_Stream
{


    public partial class RecordForm : Form
    {

        private const int CHUNK = 1024;
        private const int RATE = 11025;
        private const int RECORD_SECONDS = 75;
        private int STREAM_STEREO_DEVICE_ID;
        private const int WEBSOCKET_TIMEOUT_SECOND = 30;
        private const int WEBSOCKET_TIMEOUT_RETRY = 5;

        private WaveInEvent waveIn;
        private ClientWebSocket webSocket;
        private bool isRecording;
        private Task receivingTask;
        private CancellationTokenSource cancellationTokenSource;

        private static string host = Resources.Host;


        public string OperatorUsernameVal;
        public string CustomerIdVal;
        public string CustomerNameVal;
        public string KeyValueVal;
        public string ApiKeyVal;





        public RecordForm(string operatorUsernameVal, string customerIdVal, string customerNameVal, string keyValueVal, string apiKeyVal)
        {
            InitializeComponent();
            LoadDevices();
            languageComboBox.DataSource = cultures;
            languageComboBox.DisplayMember = "Name";
            OperatorUsernameVal = operatorUsernameVal;
            CustomerIdVal = customerIdVal;
            CustomerNameVal = customerNameVal;
            KeyValueVal = keyValueVal;
            ApiKeyVal = apiKeyVal;
            
        }


        private void LoadDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            OutputDeviceComboBox.Items.AddRange(devices.ToArray());
            OutputDeviceComboBox.SelectedIndex = 0; 
        }



        private async void StartButton_Click(object sender, EventArgs e)
        {

            if (!isRecording)
            {
                try
                {
                    webSocket = new ClientWebSocket();
                    Uri serverUri = new Uri(host);
                    await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                    Console.WriteLine("WebSocket connected");

                    // Send connection string
                    string connStr = GetConnectionString();
                    await SendWebSocketMessage(connStr);

                    // Wait for response
                    string response = await ReceiveWebSocketMessage();
                    Console.WriteLine(response);


                    if (response == "proceed")
                    {

                        waveIn = new WaveInEvent();
                        STREAM_STEREO_DEVICE_ID = OutputDeviceComboBox.SelectedIndex;
                        waveIn.DeviceNumber = STREAM_STEREO_DEVICE_ID;
                        waveIn.WaveFormat = new WaveFormat(RATE, 16, 2); // 16-bit, 2 channels

                        waveIn.DataAvailable += WaveIn_DataAvailable;
                        waveIn.StartRecording();

                        // Start receiving WebSocket messages
                        cancellationTokenSource = new CancellationTokenSource();
                        receivingTask = Task.Run(() => ReceiveWebSocketMessages(cancellationTokenSource.Token));

                        isRecording = true;
                        StartButton.Text = Resources.StopButtonText;
                        StartButton.BackColor = Color.IndianRed;

                    }

                    else
                    {
                        MessageBox.Show("Authentication failure. Exiting...");
                        webSocket.Dispose();

                    }

                }

                catch (Exception ex)
                {
                    MessageBox.Show($"An error occured: {ex.Message}");
                    webSocket.Dispose();
                }

            }

            else
            {

                // stop recording

                waveIn.StopRecording();
                waveIn.Dispose();


                cancellationTokenSource.Cancel();
                await receivingTask;
                await SendWebSocketMessage("END_CH001");

                webSocket.Dispose();

                isRecording = false;
                StartButton.Text = Resources.StartButtonText;
                StartButton.BackColor = Color.LightGray;

            }


            
        }







        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Send audio data via WebSocket

            byte[] audioData = new byte[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, audioData, 0, e.BytesRecorded);
            
            string streamFlag = "STREAM_001";
            byte[] flagBytes = Encoding.ASCII.GetBytes(streamFlag);
            byte[] dataToSend = flagBytes.Concat(audioData).ToArray();

            Task.Run(() => SendWebSocketMessage(dataToSend));
        }



        private async Task SendWebSocketMessage(byte[] data)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        private async Task SendWebSocketMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await SendWebSocketMessage(data);
        }



        private async Task<string> ReceiveWebSocketMessage()
        {
            byte[] buffer = new byte[1024];
            var receivedData = new ArraySegment<byte>(buffer);

            WebSocketReceiveResult result = await webSocket.ReceiveAsync(receivedData, CancellationToken.None);
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        private async Task ReceiveWebSocketMessages(CancellationToken cancellationToken)
        {

            try

            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    var buffer = new byte[1024];
                    var receivedData = new ArraySegment<byte>(buffer);
                    var message = new StringBuilder();

                    do
                    {
                        result = await webSocket.ReceiveAsync(receivedData, CancellationToken.None);
                        message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    string receivedMessage = message.ToString();
                    DisplayWebSocketMessage(receivedMessage);
                }
            }
            //{
            //    while (!cancellationToken.IsCancellationRequested)
            //    {
            //        string message = await ReceiveWebSocketMessage();
            //        // Process and display the received WebSocket message
            //        DisplayWebSocketMessage(message);
            //    }
            //}
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving WebSocket message: {ex.Message}");
            }
        }

        private void DisplayWebSocketMessage(string message)
        {


            //Invoke on UI thread to update the text box
            if (textBoxResponse.InvokeRequired)
            {
                textBoxResponse.Invoke((MethodInvoker)delegate
                {
                    textBoxResponse.AppendText(message + System.Environment.NewLine);
                });
            }


            else
            {
                textBoxResponse.AppendText(message + System.Environment.NewLine);
            }
        }

        private string GetConnectionString()
        {


            LoginBody loginBody = new LoginBody
            {

                OperatorUsername = OperatorUsernameVal,
                CustomerID = CustomerIdVal,
                CustomerName = CustomerNameVal,
                KeyValues = KeyValueVal,
                ApiKey = ApiKeyVal,
                ChannelCount = 2,
                Mode = modeComboBox.SelectedItem.ToString(),
                ChunkSecond = 0,
                SampleRate = RATE
            };

            if (codecComboBox.SelectedItem.ToString() == "PCM")
            {
                loginBody.Codec = 1;
            }

            else if (codecComboBox.SelectedItem.ToString() == "MU-LAW")
            {
                loginBody.Codec = 7;
            }
            

            return JsonConvert.SerializeObject(loginBody);
        }

        private class LoginBody
        {

            public string OperatorUsername { get; set; }
            public string CustomerID { get; set; }
            public string CustomerName { get; set; }
            public string KeyValues { get; set; }
            public string ApiKey { get; set; }
            public int ChannelCount { get; set; }
            public int ChunkSecond { get; set; }
            public string Mode { get; set; }
            public int SampleRate { get; set; }
            public int Codec { get; set; }
        }



        private CultureItem[] cultures = new CultureItem[]
        {
            new CultureItem() { Name = "日本語", CultureInfo = new CultureInfo("ja-JP")},
            new CultureItem(){ Name = "English", CultureInfo = new CultureInfo("en-US") }
        };



        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = languageComboBox.SelectedItem as CultureItem;

            if (selected != null)
            {
                Thread.CurrentThread.CurrentUICulture = selected.CultureInfo;
                ApplyLocalization();
            }
        }


        public void ApplyLocalization()
        {
            label1.Text = Resources.OutputDeviceLabelText;
            label4.Text = Resources.ModeLabelText;
            label5.Text = Resources.StreamCodecLabelText;
            StartButton.Text = Resources.StartButtonText;
            groupBox1.Text = Resources.RecordLabelText;
            groupBox2.Text = Resources.ResultsLabelText;
        }



    }
}
