using BelugaBox_Stream.Constants;
using BelugaBox_Stream.Interfaces;
using BelugaBox_Stream.Models;
using BelugaBox_Stream.Properties;
using WebSocketSharp;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;


namespace BelugaBox_Stream
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            languageComboBox.DataSource = cultures;
            languageComboBox.DisplayMember = "Name";
        }


        public class LoginBody
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


        public class LoginInfo
        {
            public string OperatorUsername { get; set; }
            public string CustomerID { get; set; }
            public string CustomerName { get; set; }
            public string KeyValues { get; set; }
            public string ApiKey { get; set; }
            public bool rememberMe { get; set; }
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

        }


        private void loginButton_Click(object sender, EventArgs e)
        {
            string operatorUsernameVal = textBox1.Text;
            string customerIdVal = textBox2.Text;
            string customerNameVal = textBox3.Text;
            string keyValueVal = textBox4.Text;
            string apiKeyVal = textBox5.Text;
            

            bool apiKeyValid = false;

            if (
                textBox5.Text == "" || textBox5.Text == String.Empty ||
                apiKeyVal == "" || apiKeyVal == String.Empty ||

                textBox4.Text == "" || textBox4.Text == String.Empty ||
                keyValueVal == "" || keyValueVal == String.Empty ||

                textBox3.Text == "" || textBox3.Text == String.Empty ||
                customerNameVal == "" || customerNameVal == String.Empty ||

                textBox2.Text == "" || textBox2.Text == String.Empty ||
                customerIdVal == "" || customerIdVal == String.Empty ||

                textBox1.Text == "" || textBox1.Text == String.Empty ||
                operatorUsernameVal == "" || operatorUsernameVal == String.Empty
                )
            {
                MessageBox.Show(Resources.TextBoxFilled);
            }
            else
            {
                apiKeyValid = true;
            }

            if (apiKeyValid)
            {
                // For request body
                LoginBody loginBody = new LoginBody();
                loginBody.OperatorUsername = operatorUsernameVal;
                loginBody.CustomerID = customerIdVal;
                loginBody.CustomerName = customerNameVal;
                loginBody.KeyValues = keyValueVal;
                loginBody.ApiKey = apiKeyVal;


                // For user info local data
                LoginInfo loginInfo = new LoginInfo();
                loginInfo.OperatorUsername = loginBody.OperatorUsername;
                loginInfo.CustomerID = loginBody.CustomerID;
                loginInfo.CustomerName = loginBody.CustomerName;
                loginInfo.KeyValues = loginBody.KeyValues;
                loginInfo.ApiKey = loginBody.ApiKey;
                loginInfo.rememberMe = checkBox1.Checked;



                this.Hide();
                RecordForm recordForm = new RecordForm(operatorUsernameVal,customerIdVal,customerNameVal,keyValueVal,apiKeyVal);
                recordForm.Show();

            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
