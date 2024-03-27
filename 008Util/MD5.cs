using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace _008Util
{
    public class MD5
    {

        public static string GetMD5Hash(string str)
        {
            
            StringBuilder sb = new StringBuilder();
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

              
                int length = data.Length;
                for (int i = 0; i < length; i++)
                    sb.Append(data[i].ToString("X2"));

            }
            return sb.ToString();
        }
    }
}
