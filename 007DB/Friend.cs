using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _007DB
{
    public class Friend
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        public int UserId1 { get; set; }
        public int UserId2 { get; set; }
        public int Status { get; set; } // 1表示UserId1向UserId2谁申请
    }
}
