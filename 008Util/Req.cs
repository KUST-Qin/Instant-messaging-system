using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _008Util
{
    public class Req
    {
        public ReqType Type { get; set; }
        public Object O;
    }
    public enum ReqType
    {
        LOGIN, REG, GETFRIENDS,MSG,SEARCH,ADDFRIEND,EXIT,ADDGROUP,GROUPCHAT,GETGROUPS,APPROVE
    }
}
