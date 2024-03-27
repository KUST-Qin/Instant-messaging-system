using _006Control;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _004ChatClient
{
    public partial class Main : Form
    {
        User FromUser;
        Socket socket;
        int friendId;
        public Thread t { get; set; }
        public bool Exit { get; set; }
        List<ChatLabel> cllist = new List<ChatLabel>();
        public User ToUser { get; set; }
        public Dictionary<User, string> ChatHistory { get; set; }
        public Dictionary<int, Group> groups { get; set; }
        List<ChatLabel> clGroup = new List<ChatLabel>();
        public Dictionary<int, string> groupChatHistory { get; set; }
        Form2 form2 = new Form2();
        public Form LoginForm { get; set; }

        public Group ToGroup { get; set; }

        public Main(Socket socket1, User u, Form form)
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            socket = socket1;
            this.FromUser = u;
            this.Text = $"{u.Name}({u.IdStr})";
            this.chatLabelPanel1.Visible = true;
            //this.chatMainPanel1.button1.Click += new EventHandler(SendMsg);
            
            t = new Thread(ReceiveMsg);
            t.IsBackground = true;
            t.Start();
            this.LoginForm = form;
            this.chatMainPanel1.button1.Click += new EventHandler(Send_Click);
            ChatHistory = new Dictionary<User, string>(new UserIdEqualityComparer());
            groups = new Dictionary<int, Group>();
            form2.Show();
            form2.Visible = false;
            form2.button1.Click += new EventHandler(Search_Click);

            this.form2.dataGridView1.CellContentClick += (o, e) =>
            {
                DataGridView dgv = this.form2.dataGridView1;
                if (dgv.Columns[e.ColumnIndex].Name != "Add")
                {
                    return;
                }
                if (dgv.CurrentCell is DataGridViewButtonCell)
                {
                    DataGridViewButtonCell buttonCell = dgv.CurrentCell as DataGridViewButtonCell;
                    if (buttonCell.Value.ToString() != "添加")
                    {
                        return;
                    }
                }
                
                if (dgv.Rows[e.RowIndex].Cells["Type"].Value.ToString() == "用户")
                {
                    DataGridViewRow dgr = dgv.Rows[e.RowIndex];
                    Send(new Req { Type = ReqType.ADDFRIEND, O = new AddFriend { FromUser = this.FromUser, FriendId = Convert.ToInt32(dgr.Cells["Id"].Value.ToString()) } });
                    Send(new Req { Type = ReqType.SEARCH, O = new Search() { FromUser = this.FromUser, SearchIdStr = this.form2.textBox1.Text } });
                    ChatLabel cl = new ChatLabel() { ChatId = dgr.Cells["Id"].Value.ToString(), ChatIdStr = dgr.Cells["IdStr"].Value.ToString(), ChatName = dgr.Cells["Name1"].Value.ToString() };
                    this.chatLabelPanel4.Controls.Add(cl);
                    //this.chatLabelPanel1.Controls.Add(cl);
                    //cl.DoubleClick += new EventHandler(ChatLabel_DoubleClick);
                    //ChatHistory.Add(new User { Id = Convert.ToInt32(cl.ChatId), IdStr = cl.ChatIdStr, Name = cl.ChatName }, "");
                    //cllist.Add(cl);
                } else if (dgv.Rows[e.RowIndex].Cells["Type"].Value.ToString() == "群组")
                {
                    DataGridViewRow dgr = dgv.Rows[e.RowIndex];
                    Group g = new Group { Id = Convert.ToInt32(dgr.Cells["Id"].Value.ToString()), Name = dgr.Cells["Name1"].Value.ToString(), IdStr = dgr.Cells["IdStr"].Value.ToString() };
                    Send(new Req { Type = ReqType.ADDGROUP, O = new AddGroup { FromUser = this.FromUser, GroupId = Convert.ToInt32(dgr.Cells["Id"].Value.ToString()) } });
                    groups.Add(g.Id, g);
                    Send(new Req { Type = ReqType.SEARCH, O = new Search() { FromUser = this.FromUser, SearchIdStr = this.form2.textBox1.Text } });
                    
                    ChatLabel cl = new ChatLabel() { ChatId = g.Id+"", ChatIdStr = g.IdStr, ChatName = g.Name };
                    this.chatLabelPanel2.Controls.Add(cl);

                    
                    cl.DoubleClick += (oo, ee) =>
                    {
                        this.ToUser = null;
                        this.ToGroup = g;
                        this.chatMainPanel1.ChatTitle = $"{g.Name}({g.IdStr})";
                        this.chatMainPanel1.textBox1.Text += groupChatHistory[g.Id];
                    };
                }

            };


            // 获取当前朋友信息
            Send(new Req() { Type = ReqType.GETFRIENDS, O = this.FromUser });
            Send(new Req() { Type = ReqType.GETGROUPS, O = this.FromUser });
            this.form2.FormClosing += (o, e) =>
            {
                e.Cancel = true;
                this.form2.Visible = false;
            };

            groupChatHistory = new Dictionary<int, string>();
            this.chatMainPanel1.Visible = false;
        }
        

        private void Send_Click(object sender, EventArgs e)
        {
            Send(new Req() {Type=ReqType.MSG, O = new Msg() { FromUser = this.FromUser, Content = this.chatMainPanel1.textBox2.Text, ToUser = this.ToUser } });
            this.chatMainPanel1.textBox1.Text += $"\n{this.FromUser.Name}:{this.chatMainPanel1.textBox2.Text}";
            this.chatMainPanel1.textBox2.Text = "";
        }

        private Rep get()
        {
            byte[] bytes = new Byte[1024];
            
            

            int bytesRec = socket.Receive(bytes);
            string result = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            return JsonConvert.DeserializeObject<Rep>(result);
            
            
        }

        private void ReceiveMsg()
        {
            
            while (true)
            {


                Rep rep = get();
                
                if (rep.Type == ReqType.EXIT)
                {
                    this.socket.Shutdown(SocketShutdown.Both);
                    this.socket.Close();
                    this.LoginForm.Dispose();
                    break;
                }
                else if (rep.Type == ReqType.GETFRIENDS)
                {
                    if (this.IsHandleCreated)
                    {
                        this.Invoke(new MethodInvoker(delegate 
                        {

                            List<User> friends = JsonConvert.DeserializeObject<List<User>>(rep.O.ToString());

                            foreach (User item in friends)
                            {
                                ChatLabel cl = new ChatLabel() { ChatId = item.Id + "", ChatIdStr = item.IdStr, ChatName = item.Name };
                                this.chatLabelPanel1.Controls.Add(cl);
                                cl.DoubleClick += new EventHandler(ChatLabel_DoubleClick);
                                ChatHistory.Add(item, "");
                                cllist.Add(cl);
                                
                            }
                        }));
                    }
                        
                    
                } 
                else if (rep.Type == ReqType.MSG)
                {
                    Msg msg = JsonConvert.DeserializeObject<Msg>(rep.O.ToString());
                    if (this.ToUser.Id != msg.FromUser.Id)
                    {
                        ChatHistory[msg.ToUser] += msg.Content;
                    }
                    else
                    {
                        this.Invoke(new MethodInvoker(delegate 
                        {
                            chatMainPanel1.textBox1.Text += $"\r\n{msg.FromUser.Name}:{msg.Content}";
                        }));
                    }
                }
                else if (rep.Type == ReqType.SEARCH)
                {
                    List<SearchEntity> searchEntities = JsonConvert.DeserializeObject<List<SearchEntity>>(rep.O.ToString());
                    //MessageBox.Show(rep.O.ToString());
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.form2.dataGridView1.Rows.Clear();
                        for (int i = 0; i < searchEntities.Count; i++)
                        //foreach (SearchEntity searchEntity in searchEntities)
                        {
                            int index = this.form2.dataGridView1.Rows.Add();
                            SearchEntity searchEntity = searchEntities[i];
                            this.form2.dataGridView1.Rows[index].Cells["SerialNumber"].Value = i+1;
                            this.form2.dataGridView1.Rows[index].Cells["IdStr"].Value = searchEntity.IdStr;
                            this.form2.dataGridView1.Rows[index].Cells["Name1"].Value = searchEntity.Name;
                            this.form2.dataGridView1.Rows[index].Cells["Id"].Value = searchEntity.Id;
                            this.form2.dataGridView1.Rows[index].Cells["Type"].Value = searchEntity.Type;
                            DataGridViewButtonCell button = this.form2.dataGridView1.Rows[index].Cells["Add"] as DataGridViewButtonCell;
                            if ((searchEntity.Type == "用户" && cllist.Find(item => item.ChatId == searchEntity.Id + "") == null)
                                    || (searchEntity.Type == "群组" && !groups.ContainsKey(searchEntity.Id))
                            )
                            {

                                button.Value = "添加";

                            }
                            else
                            {
                                button.Value = "已添加";
                            }

                        }
                    }));
                    
                    

                }
                else if (rep.Type == ReqType.GROUPCHAT)
                {
                    GroupChat groupChat = JsonConvert.DeserializeObject<GroupChat>(rep.O.ToString());
                    groupChatHistory[groupChat.Id] += groupChat.Content;
                }
                else if (rep.Type == ReqType.GETGROUPS)
                {
                    List<Group> groups = JsonConvert.DeserializeObject<List<Group>>(rep.O.ToString());

                    foreach (Group group in groups)
                    {
                        ChatLabel cl = new ChatLabel { ChatId = group.Id + "", ChatName = group.Name, ChatIdStr = group.IdStr };
                        this.chatLabelPanel2.Controls.Add(cl);
                        groupChatHistory.Add(group.Id, "");
                        this.groups.Add(group.Id, group);
                        cl.DoubleClick += (o, e) =>
                        {
                            ClearBackColor();

                            ChatLabel chatLabel = o as ChatLabel;
                            chatLabel.BackColor = Color.Blue;
                            chatLabel.ForeColor = Color.White;

                            this.ToUser = null;
                            this.ToGroup = group;
                            this.chatMainPanel1.ChatTitle = $"{group.Name}({group.IdStr})";
                            this.chatMainPanel1.textBox1.Text = "";
                            this.chatMainPanel1.Visible = true;
                        };
                    }
                }
                else if (rep.Type == ReqType.ADDFRIEND)
                {
                    if (this.IsHandleCreated)
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            User user = JsonConvert.DeserializeObject<User>(rep.O.ToString());
                            ChatLabel cl = new ChatLabel { ChatId = user.Id + "", ChatName = user.Name, ChatIdStr = user.IdStr };
                            this.chatLabelPanel3.Controls.Add(cl);
                            cl.DoubleClick += (o, e) =>
                            {
                                DialogResult dr = MessageBox.Show($"确定添加{user.Name}({user.IdStr})", "确认添加", MessageBoxButtons.OKCancel);
                                if (dr == DialogResult.Cancel) return;
                                Send(new Req { Type = ReqType.APPROVE, O = new Approve { FromUser = user, ToUser = this.FromUser } });
                                this.chatLabelPanel3.Controls.Remove(cl);
                              
                                ChatHistory.Add(user, "");
                                cl = new ChatLabel { ChatId = user.Id + "", ChatName = user.Name, ChatIdStr = user.IdStr };
                                this.chatLabelPanel1.Controls.Add(cl);
                                cl.DoubleClick += (oo, ee)=> 
                                {
                                    ClearBackColor();
                                    cl.BackColor = Color.Blue;
                                    cl.ForeColor = Color.White;
                                    this.ToUser = user;
                                    this.chatMainPanel1.Visible = true;
                                    this.chatMainPanel1.ChatTitle = $"{user.Name}({user.IdStr})";
                                }; 
                                cllist.Add(cl);
                            };
                            
                        }));
                    }
                    
                }
                else if (rep.Type == ReqType.APPROVE)
                {
                    if (this.IsHandleCreated)
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            
                            ChatLabel cl = null;
                            Approve approve = JsonConvert.DeserializeObject<Approve>(rep.O.ToString());
                            foreach (Control item in this.chatLabelPanel4.Controls)
                            {
                                cl = item as ChatLabel;
                                if (cl.ChatIdStr == approve.ToUser.IdStr)
                                {
                                    break;
                                }
                            }
                            this.chatLabelPanel1.Controls.Add(cl);
                            this.chatLabelPanel4.Controls.Remove(cl);
                            
                            
                            ChatHistory.Add(approve.ToUser, "");
                            cllist.Add(cl);
                            cl.DoubleClick += (oo, ee) =>
                            {
                                ClearBackColor();
                                cl.BackColor = Color.Blue;
                                cl.ForeColor = Color.White;
                                this.ToUser = approve.ToUser;
                                this.chatMainPanel1.Visible = true;
                                this.chatMainPanel1.ChatTitle = $"{this.ToUser.Name}({this.ToUser.IdStr})";
                            };
                            cllist.Add(cl);

                        }));
                    }

                          
                }
            }
            this.form2.Dispose();
            this.Dispose();
        }

        private void ChatLabel_DoubleClick(object sender, EventArgs e)
        {
            ClearBackColor();

            ChatLabel cl = sender as ChatLabel;
            cl.BackColor = Color.Blue;
            cl.ForeColor = Color.White;
            this.chatMainPanel1.ChatTitle = $"{cl.ChatName}({cl.ChatIdStr})";
            ToUser = new User() { Id = Convert.ToInt32(cl.ChatId), IdStr = cl.ChatIdStr, Name = cl.ChatName };
            this.chatMainPanel1.Visible = true;
        }

        public void Send(Req req)
        {
            socket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
        }
        // FRIENDS
        
       
        private void ClearBackColor()
        {
            foreach (ChatLabel cltemp in cllist)
            {
                cltemp.BackColor = Color.White;
                cltemp.ForeColor = Color.Black;
            }

            foreach (ChatLabel cltemp in clGroup)
            {
                cltemp.BackColor = Color.White;
                cltemp.ForeColor = Color.Black;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            form2.Visible = true;
        }

        private void Search_Click(object sender, EventArgs e)
        {
            Send(new Req() { Type = ReqType.SEARCH, O = new Search() { FromUser=this.FromUser, SearchIdStr = this.form2.textBox1.Text} });
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Send(new Req() { Type = ReqType.EXIT, O = this.FromUser });
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
