using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace _004ChatClient
{
    public class ClientSocket
    {
        public static string Send(string msg)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(address, 8888);
            socket.Connect(endPoint);
            byte[] bytes = new Byte[1024];






            //byte[] msg = Encoding.UTF8.GetBytes($"LOGIN,{this.textBox1.Text},{this.textBox2.Text}");
            byte[] bytesMsg = Encoding.UTF8.GetBytes(msg);
            // Send the data through the socket.  
            int bytesSent = socket.Send(bytesMsg);

            // Receive the response from the remote device.  
            int bytesRec = socket.Receive(bytes);
            

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            return Encoding.UTF8.GetString(bytes, 0, bytesRec);
        }
    }
}
