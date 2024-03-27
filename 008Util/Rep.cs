using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _008Util
{
    public class Rep
    {
        public Object O { get; set; }
        public RepStatus Status { get; set; }

        public string Msg { get; set; }
        public ReqType Type { get; set; }
    }

   
    public enum RepStatus 
    {
        SUCCESS, ERROR
    }
    
}
