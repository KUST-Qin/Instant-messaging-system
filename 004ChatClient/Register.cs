using _007DB;
using _008Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _004ChatClient
{
    public partial class Register : Form
    {
        public Socket Socket { get; set; }
        public Register(Socket socket)
        {
            InitializeComponent();
            this.Socket = socket;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (this.textBox2.Text != this.textBox3.Text)
            {
                MessageBox.Show("密码与确认密码不一致！");
                return;
            }
            // 封装request请求
            byte[] bytes = new Byte[1024];
            Req req = new Req() { Type = ReqType.REG, O = new User { Name = this.textBox1.Text, Password = MD5.GetMD5Hash( this.textBox2.Text) } };
            String str = JsonConvert.SerializeObject(req);



            byte[] msg = Encoding.UTF8.GetBytes(str);
            int bytesSent = Socket.Send(msg);
            int bytesRec = Socket.Receive(bytes);
            string result = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            
            Rep rep = JsonConvert.DeserializeObject<Rep>(result);
            if (rep.Status == RepStatus.ERROR)
            {
                MessageBox.Show(rep.Msg);
            }
            else
            {
                string IdStr = rep.O.ToString();
                MessageBox.Show($"你的账号是{IdStr}");
                this.Dispose();
            }
                  
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
