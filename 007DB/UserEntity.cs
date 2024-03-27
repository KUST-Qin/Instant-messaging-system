using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _007DB
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IdStr { get; set; }

        // 1表示Id向你申请好友 2表示已经是好友了
        public int Status { get; set; }
    }
}
