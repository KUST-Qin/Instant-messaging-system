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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _004ChatClient
{

    public partial class Login : Form
    {

       
        private static Socket socket;
        public Login()
        {
            InitializeComponent();


            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(address, 8887);
            socket.Connect(endPoint);

        }

       
        
        private void button1_Click(object sender, EventArgs e)
        {

            
            // 封装request请求
            byte[] bytes = new Byte[1024];
            Req req = new Req() { Type = ReqType.LOGIN, O = new User() { IdStr = this.textBox1.Text, Password = MD5.GetMD5Hash(this.textBox2.Text) } };
            String str = JsonConvert.SerializeObject(req);
           
            byte[] msg = Encoding.UTF8.GetBytes(str); 
            int bytesSent = socket.Send(msg);
            int bytesRec = socket.Receive(bytes);
            string result = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            //MessageBox.Show(result);
            Rep rep = JsonConvert.DeserializeObject<Rep>(result);
            if (rep.Status == RepStatus.ERROR)
            {
                MessageBox.Show(rep.Msg);
                
            } else

            {
                User user = JsonConvert.DeserializeObject<User>(rep.O.ToString());

                new Main(socket, user, this).Show();

                this.Visible = false;
                //this.Dispose();
                
            }
            
        }

        private void Login_Load(object sender, EventArgs e)
        {
            
            

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Register reg = new Register(socket);
            reg.ShowDialog();

        }
    }
}
