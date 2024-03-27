using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _006Control
{
    public partial class ChatLabel : UserControl
    {
        public ChatLabel()
        {
            InitializeComponent();
            this.Width = 138;
            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            using (StringFormat sf = new StringFormat())
            {
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;
                
                g.DrawString($"{ChatName}({ChatIdStr})", Font, new SolidBrush(ForeColor), new Rectangle(0,0, Width-1, Height - 1), sf);
            }

            
        }
        public string ChatId { get; set; }
        public string ChatName { get; set; }

        public string ChatIdStr { get; set; }
    }
}
