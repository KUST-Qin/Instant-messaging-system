using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _007DB
{
    public class User
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public string IdStr { get; set; }
       
    }

    public class UserIdEqualityComparer : EqualityComparer<User>
    {
             public override bool Equals(User x, User y)
        {
            return x.Id == y.Id;
        }

        public override int GetHashCode(User obj)
        {
            return obj.Id;
        }
}
}
