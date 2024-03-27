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

namespace _003ChatServer
{
    public partial class Form1 : Form
    {
        List<Socket> sockets = new List<Socket>();
        private Socket serverSocket;
        private Dictionary<int, Socket> clientSockets = new Dictionary<int, Socket>();
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();


            
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(address, 8887);

            serverSocket.Bind(endPoint);
            serverSocket.Listen(int.MaxValue);
            Thread thread = new Thread(startServer);
            thread.IsBackground = true;
            thread.Start(serverSocket);

            this.textBox1.Text += "\n启动成功！";
            this.button1.Enabled = false;
        }

        public void startServer(Object o)
            
        {
            Socket serverSocket = o as Socket;
                
           
            while (true)
            {
                    Socket clientSocket = serverSocket.Accept();
                    Thread thread = new Thread(ReceiveMsg);
                    thread.IsBackground = true;
                    thread.Start(clientSocket);
                   

                

            }
        }

        private Req get(Socket clientSocket)
        {
            byte[] bytes = new byte[1024 * 1024 * 2];
            int bytesRec = clientSocket.Receive(bytes);

            string data = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            //MessageBox.Show(data);
            return JsonConvert.DeserializeObject<Req>(data);
        }
        private void ReceiveMsg(Object o )

        {
            Socket clientSocket = o as Socket;
            while (true)
            {
                Req req = get(clientSocket);
            
                if (req.Type == ReqType.LOGIN)
                {
                    User reqU = JsonConvert.DeserializeObject<User>(req.O.ToString());
                    User u = DBHelper.db.Queryable<User>().First(it => it.IdStr == reqU.IdStr && it.Password == reqU.Password);
                    if (u == null)
                    {
                        //Send(clientSocket, JsonConvert.SerializeObject( new Rep() { Status = RepStatus.ERROR, Msg = "用户名或者密码错误！", O = null }));
                        send(clientSocket, new Rep() { Status = RepStatus.ERROR, Msg = "用户名或者密码错误！", Type = ReqType.LOGIN });
                    } 
                    else
                    {
                        send(clientSocket, new Rep() { Status = RepStatus.SUCCESS,  Type = ReqType.LOGIN, O = u });
                        clientSockets.Add(u.Id, clientSocket);
                    }

                    //todo
                } 
                else if (req.Type == ReqType.REG)
                {
                    User user = JsonConvert.DeserializeObject<User>(req.O.ToString());
                    DBHelper.db.Insertable<User>(new User { Name = user.Name, Password = user.Password}).ExecuteCommand();
                    user = DBHelper.db.Queryable<User>().First(item=>item.Name == user.Name && item.Password == user.Password);
                    user.IdStr = user.Id.ToString().PadLeft(3, '0');
                    DBHelper.db.Updateable<User>(user).ExecuteCommand();
                    send(clientSocket, new Rep {  Status = RepStatus.SUCCESS, Type=ReqType.REG, O = user.IdStr} );
                }
                else if (req.Type == ReqType.GETFRIENDS)
                {
                    int userId = JsonConvert.DeserializeObject<User>(req.O.ToString()).Id;
                    List<User> users = GetFriendsById(userId);
                    send(clientSocket, new Rep() { Status = RepStatus.SUCCESS, O = users, Type=req.Type });
                }
                else if (req.Type == ReqType.MSG)
                {
                    Msg msg = JsonConvert.DeserializeObject<Msg>(req.O.ToString());

                    if (clientSockets.ContainsKey(msg.ToUser.Id))
                    {
                        send(clientSockets[msg.ToUser.Id], new Rep() { Status = RepStatus.SUCCESS, Type = ReqType.MSG, O = msg });
                    }
                }
                else if (req.Type == ReqType.SEARCH)
                {
                    Search search = JsonConvert.DeserializeObject<Search>(req.O.ToString());

                    /*
                    List<User> userList = DBHelper.db.Queryable<User>().Where(item => item.IdStr.Contains(search.SearchIdStr) && item.Id != search.FromUser.Id).ToList();

                    send(clientSocket, new Rep() { Type = ReqType.SEARCH, Status = RepStatus.SUCCESS, O = userList });
                    */

                    var users = DBHelper.db.Queryable<User>().Where(item => item.IdStr.Contains(search.SearchIdStr) && item.Id != search.FromUser.Id).Select(it => new SearchEntity { Id = it.Id, IdStr = it.IdStr, Name = it.Name, Type = "用户" });
                    var groups = DBHelper.db.Queryable<Group>().Where(item => item.IdStr.Contains(search.SearchIdStr)).Select(it => new SearchEntity { Id = it.Id, IdStr = it.IdStr, Name = it.Name, Type = "群组" });
                    var list = DBHelper.db.UnionAll(users, groups).ToList();
                    send(clientSocket, new Rep() { Type = ReqType.SEARCH, Status = RepStatus.SUCCESS, O = list });
                }
                else if (req.Type == ReqType.ADDFRIEND)
                {
                    AddFriend addFriend = JsonConvert.DeserializeObject<AddFriend>(req.O.ToString());
                    DBHelper.db.Insertable<Friend>(new Friend() { UserId1 = addFriend.FromUser.Id, UserId2 = addFriend.FriendId, Status = 1 }).ExecuteCommand();

                   
                    if (clientSockets.ContainsKey(addFriend.FriendId))
                    {
                        send(clientSockets[addFriend.FriendId], new Rep { Type=ReqType.ADDFRIEND, Status=RepStatus.SUCCESS, O = addFriend.FromUser});
                    }
                }
                else if (req.Type == ReqType.EXIT)
                {
                    send(clientSocket, new Rep() { Type = ReqType.EXIT });
                    User fromUser = JsonConvert.DeserializeObject<User>(req.O.ToString());
                    clientSockets.Remove(fromUser.Id);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    break;
                }
                else if (req.Type == ReqType.ADDGROUP)
                {
                    AddGroup addGroup = JsonConvert.DeserializeObject<AddGroup>(req.O.ToString());
                    DBHelper.db.Insertable<GroupUser>(new GroupUser { GroupId = addGroup.GroupId, UserId = addGroup.FromUser.Id }).ExecuteCommand();
                }   
                else if (req.Type == ReqType.GROUPCHAT)
                {
                    GroupChat groupChat = JsonConvert.DeserializeObject<GroupChat>(req.O.ToString());
                    List<int> list = DBHelper.db.Queryable<GroupUser>().Where(item => item.GroupId == groupChat.Id).Select(g => g.UserId).ToList();
                    foreach (int id in list)
                    {
                        if (clientSockets.ContainsKey(id))
                        {
                            send(clientSockets[id], new Rep {  Type = ReqType.GROUPCHAT, O = groupChat.Content, Status = RepStatus.SUCCESS });
                            
                        }
                    }
                }
                else if (req.Type == ReqType.GETGROUPS)
                {
                    int userId = JsonConvert.DeserializeObject<User>(req.O.ToString()).Id;
                    List<Group> groups = DBHelper.db.Queryable<GroupUser>().InnerJoin<Group>((gu, g) => gu.GroupId == g.Id).Where(gu => gu.UserId == userId).Select((gu, g) => g).ToList();
                    send(clientSocket, new Rep { Type = ReqType.GETGROUPS, O = groups, Status = RepStatus.SUCCESS });
                }
                else if (req.Type == ReqType.APPROVE)
                {
                    Approve approve = JsonConvert.DeserializeObject<Approve>(req.O.ToString());
                    Friend f = DBHelper.db.Queryable<Friend>().First(item => item.UserId1 == approve.FromUser.Id && item.UserId2 == approve.ToUser.Id);
                    f.Status = 2;
                    DBHelper.db.Updateable<Friend>(f).ExecuteCommand();
                    DBHelper.db.Insertable<Friend>(new Friend { UserId1 = f.UserId2, UserId2 = f.UserId1, Status = 2 }).ExecuteCommand();
                    if (clientSockets.ContainsKey(approve.FromUser.Id))
                    {
                        send(clientSockets[approve.FromUser.Id], new Rep { Status = RepStatus.SUCCESS, Type = ReqType.APPROVE, O = approve });
                    }
                }
            }
            
        }

        private List<User> GetFriendsById(int userId)
        {
            //Friend表userId1,userId2 
            return DBHelper.db.Queryable<User>().InnerJoin<Friend>((u, f) => f.UserId2 == u.Id && f.UserId1 == userId && (f.Status == 2 || f.Status == 1)).OrderBy((u,f)=>f.Status, SqlSugar.OrderByType.Desc).Select(u=>new User() { Id=u.Id, IdStr = u.IdStr, Password="", Name = u.Name}).ToList();
        }

        private User getUser(int id)
        {
            return DBHelper.db.Queryable<User>().First(it => it.Id == id);
        }
        private string getFriends(int id)
        {
            string result = "";
            HashSet<int> userIds = new HashSet<int>();
            List< Friend> friends = DBHelper.db.Queryable<Friend>().Where(it => it.UserId1 == id || it.UserId2 == id).ToList();
            foreach(Friend friend in friends)
            {
                userIds.Add(friend.UserId1);
                userIds.Add(friend.UserId2);
            }
            userIds.Remove(id);
            List<User> users = DBHelper.db.Queryable<User>().ToList();
            foreach (User user in users)
            {
                if (userIds.Contains(user.Id))
                {
                    result +=$"{user.Id}|{user.Name}|{user.IdStr},";
                }
            }
            if (result != "")
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }

     
        private void handleLogin(Socket clientSocket, string idStr, string password)
        {
            
            User u = DBHelper.db.Queryable<User>().First(it => it.IdStr == idStr && it.Password == password);
            if (u != null )
            {
                //Send(clientSocket, u.Id + "," + u.Name);
                clientSockets.Add(u.Id, clientSocket);
            } else
            {
                //Send(clientSocket, "ERROR");
            }
           
        }

        private void send(Socket socket, Rep req)
        {
            socket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(req)));
        }

    }
}
