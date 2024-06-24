using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Net.Http;
using System.Security.Policy;

namespace Loader_Github
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string KEY = "Secret";
        string LINK_GITHUB_RAW = "https://raw.githubusercontent.com/E0x00000/Loader-Git-Dependencies/main/BD.txt";

        
        string Request(string url)
        {
            WebClient client = new WebClient();
            string BD = client.DownloadString(url);
            return BD;
        }

        string encode(string text, string key)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] encryptedBytes = new byte[textBytes.Length];
            for (int i = 0; i < textBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(textBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return Convert.ToBase64String(encryptedBytes);
        }
        string decode(string encryptedText, string key)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] textBytes = new byte[encryptedBytes.Length];
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                textBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return Encoding.UTF8.GetString(textBytes);
        }

        string get_hwid()
        {
            var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
            ManagementObjectCollection mbsList = mbs.Get();
            string id = "";
            foreach (ManagementObject mo in mbsList)
            {
                id = mo["ProcessorId"].ToString();
                break;
            }
            return id;
        }
        string get_date(string baseLogin)
        {
            DateTime date;
            if (!DateTime.TryParse(baseLogin, out date))
            {
                return "Invalid";
            }
            string getnowdatestring_request = Request("http://worldtimeapi.org/api/ip");
            int startIndex = getnowdatestring_request.IndexOf("\"datetime\":\"") + "\"datetime\":\"".Length;
            int endIndex = getnowdatestring_request.IndexOf("\"", startIndex);
            string getnowdateString = getnowdatestring_request.Substring(startIndex, endIndex - startIndex);
            DateTime nowdate;
            if (!DateTime.TryParse(getnowdateString, out nowdate))
            {
                return "Invalid";
            }
            if (date <= nowdate)
            {
                return "expired";
            }
            else
            {
                return "sub";
            }
        }

        string get_password(string text)
        {
            int indexPassword = text.IndexOf("password:");
            if (indexPassword != -1)
            {
                int indexEnd = text.IndexOf(":", indexPassword + 9);
                if (indexEnd == -1)
                {
                    indexEnd = text.Length;
                }
                string passwordSubstring = text.Substring(indexPassword + 9, indexEnd - indexPassword - 9);
                return passwordSubstring.Trim();
            }
            return null;
        }

        string replace(string text, string encryptedPassword, string decryptedPassword)
        {
            return text.Replace($"password:{encryptedPassword}", $"password:{decryptedPassword}");
        }

        string decryptedPassword;
        bool Login(string username, string password, bool checkbox)
        {
            
            string hwid = get_hwid();
            string encryptedText = encode(password, KEY);
            string base_login_partial = $"{{username:{username}:password:{password}:hwid:{hwid}";
            string base_login_partial_ec = $"{{username:{username}:password:{encryptedText}:hwid:{hwid}";
            if(checkbox)
            {
                MessageBox.Show(base_login_partial_ec);
                return false;
            }

            


            bool isAuthenticated = false;
            try
            {
                string valid = Request(LINK_GITHUB_RAW);
                string encryptedPassword = get_password(valid);
                if (encryptedPassword != "")
                {
                    string decryptedPassword = decode(encryptedPassword, KEY);
                    valid = replace(valid, encryptedPassword, decryptedPassword);
                }
                if (valid.Contains(base_login_partial))
                {
                    int indexExpires = valid.IndexOf("expires:");
                    if (indexExpires != -1)
                    {
                        int indexEnd = valid.IndexOf("}", indexExpires);
                        string expiresSubstring = valid.Substring(indexExpires + 8, indexEnd - indexExpires - 8);
                        if (get_date(expiresSubstring) == "expired")
                        {
                            MessageBox.Show("expired");
                        }
                        else
                        {
                            MessageBox.Show("Logged");
                            isAuthenticated = true;
                        }
                    }
                }
                else
                { 
                    if (!valid.Contains($"username:{username}"))
                    {
                        MessageBox.Show("Invalid Username");
                    }
                    else if (!valid.Contains($"password:{decryptedPassword}"))
                    {
                        MessageBox.Show("Invalid Password");
                    }
                    else if (!valid.Contains($"hwid:{hwid}"))
                    {
                        MessageBox.Show("Invalid HWID");
                    }
                    else
                    {

                        MessageBox.Show("Invalid Credentials");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return isAuthenticated;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Login(textBox1.Text, textBox2.Text, checkBox1.Checked);

        }
    }
}
