﻿using SimpleTcp;
using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Client
{
    public partial class Form1 : Form
    {
        int check = 0;
        SimpleTcpClient Client;
        public Form1(){
            InitializeComponent();
        }

        private void guna2Button1_Click(object sender, EventArgs e){
            if (!string.IsNullOrEmpty(Username.Text) && (!string.IsNullOrEmpty(Password.Text)))
            {
                Client.Send($"Login:{Username.Text}%{Password.Text}");
                MessageFromServer.Text += $"Me:LogIn:{Username.Text}%{Password.Text}{Environment.NewLine}";
            }
        }


        private void checkString(string s) {
            this.Invoke(new Action(() => { 
                if (s[0] == '1') {
                    OpenTable();name.Text += Username.Text;
                    Client.Send("3AllData");
                }
                else if (s[0] == '2') {
                    MessageBox.Show("Login unsuccessful", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (s[0] == '3') {
                    MessageBox.Show("Registration failed","Message",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                else if (s[0] == '4') {
                    createPass.Text = string.Empty;
                    createUser.Text = string.Empty;
                    creatRePass.Text = string.Empty;
                    MessageBox.Show("Sign Up Success", "Message", MessageBoxButtons.OK);
                }
                else if (s[0] == '5') {
                    LoadDataGridView(s);
                }
                else if (s[0] == '6') { 
                }
            }));
        }
        private void Events_DataReceived(object sender, DataReceivedEventArgs e){
            string Str_Check = Encoding.UTF8.GetString(e.Data);
            checkString(Str_Check);
        }
        private void Events_Disconnected(object sender, ClientDisconnectedEventArgs e){
            try{
                MessageFromServer.Text += $"Server is disconnected.{Environment.NewLine}";
            }
            catch{
                MessageBox.Show("Server is disconnected", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }

        }

        private void Events_Connected(object sender, ClientConnectedEventArgs e){
            MessageFromServer.Text += $"Server is connected.{Environment.NewLine}";
        }

        private void Form1_Load(object sender, EventArgs e){
            btnCreate.Enabled = false;
            btnSignIn.Enabled = false;
            btnConnect.Enabled = false;
            ((Control)show).Enabled = false;
        }

        private void CreateClient_Click_3(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextIP.Text))
            {
                Client = new SimpleTcpClient($"{TextIP.Text}:{TextPort.Text}");
                Client.Events.Connected += Events_Connected;
                Client.Events.Disconnected += Events_Disconnected;
                Client.Events.DataReceived += Events_DataReceived;
                btnConnect.Enabled = true;
                CreateClient.Enabled = false;
            }
            else
            {
                MessageBox.Show("IP server is null", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            try{
                Client.Connect();
                btnCreate.Enabled = true;
                btnSignIn.Enabled = true;
                btnConnect.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnSignIn_Click_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Username.Text) && !string.IsNullOrEmpty(Password.Text))
            { 
                Client.Send($"1*{Username.Text.Length.ToString()}*{Username.Text}*{Password.Text.Length.ToString()}*{Password.Text}");
                MessageFromServer.Text += $"Me:Login{Username.Text}%{Password.Text}{Environment.NewLine}";
            }
        }

        private void btnCreate_Click_1(object sender, EventArgs e){
            {
                if ((!string.IsNullOrEmpty(createUser.Text)) && (!string.IsNullOrEmpty(createPass.Text)) && (!string.IsNullOrEmpty(creatRePass.Text))){
                    if (creatRePass.Text.ToString() == createPass.Text.ToString()){
                        Client.Send($"2*{createUser.Text.Length.ToString()}*{createUser.Text}*{createPass.Text.Length.ToString()}*{createPass.Text}");
                        MessageFromServer.Text += $"Me:Create:{createUser.Text}%{createPass.Text}{Environment.NewLine}";
                    }
                    else{
                        MessageBox.Show("Re_pass is invalid", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Input is empty", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }        
        /// <summary>
        /// Tải dữ liệu cho bảng  
        /// </summary>
        private void LoadComboBox(List<Coins> data) {
            SearchString.Items.Clear();
            foreach(Coins item in data) {
                SearchString.Items.Add(item.Currency);
            }
            SearchString.Text = data[0].Currency;
        }
        private void LoadDataGridView(string s) {
            List<Coins> data = ListCoins.Instance.Load(s);
            this.Invoke(new Action(() => {
                coinsData.Rows.Clear();
                foreach(Coins item in data) {
                    DataGridViewRow row = (DataGridViewRow)coinsData.Rows[0].Clone();
                    row.Cells[0].Value = item.Currency.ToString();
                    row.Cells[2].Value = item.Buy_cash.ToString();
                    row.Cells[3].Value = item.Buy_transfer.ToString();
                    row.Cells[4].Value = item.Sell.ToString();
                    row.Cells[1].Value = item.Date_time;
                    coinsData.Rows.Add(row);
                }
                LoadComboBox(data);
            }));   
        }
        private void OpenTable() { 
            this.Invoke(new Action(()=> {
                ((Control)show).Enabled = true;
                ((Control)login).Enabled = false;
                ((Control)create).Enabled = false;
                MainControl.SelectedTab = MainControl.TabPages["show"];
            }));
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            ((Control)show).Enabled = false;
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            this.Invoke(new Action(() =>{
                coinsData.Rows.Clear();
                ((Control)login).Enabled = true;
                ((Control)create).Enabled = true;
                ((Control)show).Enabled = false;
                MainControl.SelectedTab = MainControl.TabPages["login"];
                Username.Text = Password.Text = string.Empty;
                name.Text = "Hello:";
            }));
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            Client.Send("3AllData");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

        }
    }
}
