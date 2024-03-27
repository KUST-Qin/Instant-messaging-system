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

namespace _006Control
{
    public partial class ChatMainPanel : UserControl
    {
        public ChatMainPanel()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            using (StringFormat sf = new StringFormat())
            {
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

           
                g.DrawString(this.ChatTitle, Font, Brushes.Black, new Rectangle(0, 0, Width - 1,30), sf);
            }

        }
        private string _chatTitle;
        
        public string ChatTitle
        {
            get { return _chatTitle; }
            set { _chatTitle = value; Invalidate(); }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            
        }
    }
}
