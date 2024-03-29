﻿using System;
using SimpleTcp;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Server_manage
{   
    public partial class Form1 : Form
    {
        SimpleTcpServer server;
        List<string> listClient = new List<string>();
        
        //Xử lý Socket serve
        IPEndPoint IP;
        Socket Server1;
        List<Socket> ClientList;
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            btnClose.Enabled = false;
        }

        #region connect
        private void coneect()
        {
            string textP = ""; string textPort = "";
            int Index = TextIP.Text.IndexOf(':');

            textP = TextIP.Text.Substring(0, Index - 1);
            textPort = TextIP.Text.Substring(Index + 1);

            IP = new IPEndPoint(IPAddress.Any, Int32.Parse(textPort));
            Server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Server1.Bind(IP); textIFO.Text += "Starting.............";
            Thread Listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        Server1.Listen(100);
                        Socket clien = Server1.Accept();
                        ClientList.Add(clien);
                        textIFO.Text += $"{clien.RemoteEndPoint.ToString()}:Connected{Environment.NewLine}";
                        listClientText.Text += $"{clien.RemoteEndPoint.ToString()}{Environment.NewLine}";
                        Thread rec = new Thread(Receive);
                        rec.IsBackground = true;
                        rec.Start(clien);
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Parse(textP), Int32.Parse(textPort));
                    Server1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
            });
            Listen.IsBackground = true;
            Listen.Start();
        }

        //Hàm Kiểm tra client disconnect 
        bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        #endregion

        #region LoadForm
        private void ThreadUpdataData()
        {
            sql_manage.updateData();
            timer1.Start();
        }

        //Tạo data tự động lấy từ json mỗi khi mở app
        private void Form1_Load(object sender, EventArgs e)
        {
            //sql_manage.updateData(); // Gọi hàm update dữ liệu
            ClientList = new List<Socket>();
            listClient = new List<string>();
            Thread trd = new Thread(new ThreadStart(ThreadUpdataData));
            trd.Start();
            trd.Join();
        }

        #endregion

        #region event click 
        private void CreateClient_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(TextIP.Text)) {
                coneect();
                openServer.Enabled = false;
                btnClose.Enabled = true;
            }
            else
                MessageBox.Show("textIp is NULL", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            openServer.Enabled = true;
            btnClose.Enabled = false;
            listClientText.Text = string.Empty;
            textIFO.Text = string.Empty;
        }
        #endregion

        #region send and rec data
        //Hàm nhận dữ liệu từ client
        private void Receive(object obj)
        {
            Socket clien = (Socket)obj;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024 * 5000];
                    clien.Receive(data);
                    string mess = (string)Deserialize(data);
                    textIFO.Text += $"{clien.RemoteEndPoint.ToString()}:{mess}{Environment.NewLine}";
                    checkString1(mess, clien);
                }
            }
            catch { }
        }
        //Hàm gửi Dữ liệu cho client 
        private void sendString(Socket clien, string s)
        {
            foreach (Socket item in ClientList)
            {
                if (item.RemoteEndPoint.ToString() == clien.RemoteEndPoint.ToString())
                {
                    clien.Send(Serialize(s));
                    break;
                }
            }
        }
        #endregion

        #region checkString
        //Hàm kiểm tra dữ liệu để server lấy dữ liệu và gửi cho client phù hợp với yêu cầu của client
        private void checkString1(string s,Socket clien) {
            sql_manage f = new sql_manage();
            if (s[0] == '1') {
                if (f.checkLogin(s) == -1) {
                    sendString(clien, "1Success");//Đăng nhập thành công
                }
                else {
                    sendString(clien, "2Invalid");//Đăng nhập không thành công
                }
            }
            else if (s[0] == '2') {
                if (f.checkLogin(s) == -1) {
                    sendString(clien, "3Invalid");//Đăng ký không thành công
                }
                else {
                    f.Insert_Account(s);
                    sendString(clien, "4Success");//Đăng ký thành công
                }
            }
            else if (s[0] == '3'){ //In ra toàn bộ giá trị của bảng 
                string sendString1 = f.GetDataFromDatabase("", "");
                sendString(clien, sendString1);
            }
            else if(s[0]=='4')//In ra giá trị của bảng có điều kiện
            {
                string currency = "";string date_time = "";
                int Index = s.IndexOf('@');
                currency = s.Substring(1, Index - 1);
                date_time = s.Substring(Index + 1);
                string sendString1 = "";
                if (currency == "All")
                    sendString1 = f.GetDataFromDatabase("", date_time);
                else
                    sendString1 = f.GetDataFromDatabase(currency, date_time);
                sendString(clien, sendString1);
            }
            else if (s[0] == '5') { 
                listClientText.Text = string.Empty;
                foreach(Socket item in ClientList) {
                    if (SocketConnected(item))
                        listClientText.Text += $"{item.RemoteEndPoint.ToString()}{Environment.NewLine}";
                }
            }
        }
        #endregion

        #region change byte array to string and string to byte array
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);
            return stream.ToArray();// stream tra ra 1 day byte
        }
        // gom manh
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }
        #endregion

        #region timer tick
        int count_time = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            count_time++;
            textBox1.Text = count_time.ToString();
            if(count_time %1200 == 0)
            {
                sql_manage.updateData();
            }
        }
        #endregion

    }
}
//https://stackoverflow.com/questions/41683798/convert-json-from-get-request-into-text-boxes-in-c-sharp-winforms-application

