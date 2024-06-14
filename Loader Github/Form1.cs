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

        
        string Request(string url)
        {
            WebClient client = new WebClient();
            string BD = client.DownloadString(url);
            return BD;
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


        bool Login(string username, string password)
        {
            string hwid = get_hwid();
            string base_login_partial = $"{{username:{username}:password:{password}:hwid:{hwid}";

            bool isAuthenticated = false;
            try
            {
                string valid = Request("https://raw.githubusercontent.com/E0x00000/Loader-Git-Dependencies/main/BD.txt");
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
                    else if (!valid.Contains($"password:{password}"))
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
            Login(textBox1.Text, textBox2.Text);
        }
    }
}
