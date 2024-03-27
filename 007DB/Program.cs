using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _007DB
{
    public class DBHelper
    {
        public static SqlSugarScope db = new SqlSugarScope(new ConnectionConfig()
            {
                ConnectionString = "server=localhost;user id=root;password=123456;database=test",//连接符字串
                DbType = DbType.MySql,//数据库类型
                IsAutoCloseConnection = true, //不设成true要手动close
            },
            db => {
                //(A)全局生效配置点
                //调试SQL事件，可以删掉
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
                        Console.WriteLine(sql);//输出sql,查看执行sql
                    };
                }
        );
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            db.Insertable<User>(new User() { Id = 1, Name = "张三", Password = "149098" }).ExecuteCommand();
        }
    }
}
