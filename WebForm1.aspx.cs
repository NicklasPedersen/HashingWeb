using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.Text;
using System.Threading;

namespace HashingWeb
{
    public partial class WebForm1 : System.Web.UI.Page
    {

        static string connStr = "Server=localhost;Database=hashingtest;Uid=root;Pwd=testingpass;";
        static byte[] GenerateSalt()
        {
            using (var crypto = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[32];
                crypto.GetBytes(salt);
                return salt;
            }
        }

        static byte[] CombineArrays(byte[] left, byte[] right)
        {
            byte[] newArray = new byte[left.Length + right.Length];
            Array.Copy(left, newArray, left.Length);
            Array.Copy(right, 0, newArray, left.Length, right.Length);
            return newArray;
        }

        static byte[] HashWithSalt(string pwd, byte[] salt)
        {
            using (var hash = new MD5CryptoServiceProvider())
            {
                byte[] pwd_bytes = Encoding.ASCII.GetBytes(pwd);
                byte[] salted_bytes = CombineArrays(pwd_bytes, salt);
                for (int i = 0; i < 1; i++)
                {
                    salted_bytes = hash.ComputeHash(salted_bytes);
                }
                return salted_bytes;
            }
        }

        static bool CompareBytes(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        static bool UserExists(string name)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM users WHERE @user = username;";
                cmd.Parameters.AddWithValue("user", name);
                using (var reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
        }
        static void CreateUser(string name, string pwd)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                byte[] salt = new byte[0];//GenerateSalt();
                byte[] hash = HashWithSalt(pwd, salt);
                conn.Open();
                MySqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO users (username, hash, salt) VALUES (@user, @hash, @salt)";
                cmd.Parameters.AddWithValue("user", name);
                cmd.Parameters.AddWithValue("hash", Convert.ToBase64String(hash));
                cmd.Parameters.AddWithValue("salt", Convert.ToBase64String(salt));
                cmd.ExecuteNonQuery();
            }
        }
        static bool Login(string name, string pwd)
        {
            using (var conn = new MySqlConnection(connStr))
            {
                conn.Open();
                byte[] salt;
                byte[] hash;
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM users WHERE @user = username";
                    cmd.Parameters.AddWithValue("user", name);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return false;
                        }
                        salt = Convert.FromBase64String(reader.GetString("salt"));
                        hash = Convert.FromBase64String(reader.GetString("hash"));
                    }
                }

                byte[] new_hash = HashWithSalt(pwd, salt);
                return CompareBytes(new_hash, hash);
            }
        }
        string tries = "tries";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[tries] == null)
            {
                Session[tries] = 0;
            }
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            if (Session["nextattempt"] is DateTime time)
            {
                if (time > DateTime.Now)
                {
                    Label1.Text = $"Please wait {(time - DateTime.Now).Seconds} seconds before trying again";
                    return;
                }
            }
            
            string name = this.user.Text;
            string pwd = this.pwd.Text;

            if (!UserExists(name))
            {
                Label1.Text = "User does not exist, creating new user";
                CreateUser(name, pwd);
            }
            else
            {
                bool login = Login(name, pwd);
                Session[tries] = (int)Session[tries] + 1;
                if (!login)
                {
                    if ((int)Session[tries] < 3)
                    {
                        Label1.Text = "Wrong username or password, try again";
                    }
                    else
                    {
                        Label1.Text = "Too many attemps wait a while before trying again";
                        Session["nextattempt"] = DateTime.Now.AddSeconds(5);
                        Session[tries] = 0;
                    }
                }
                else
                {
                    // You would probably generate a session token and use that here
                    // though only over https
                    Session["user"] = name;
                    Response.Redirect("WebForm2.aspx");
                }
            }
        }
    }
}