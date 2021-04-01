using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace MeaningSolution___Proxy_Changer
{

    public partial class Form1 : Form
    {
        private DataSet ds = new DataSet();
        private DataTable proxyInfo = new DataTable();

        public Form1()
        {
            InitializeComponent();

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            NetworkChange.NetworkAddressChanged += new
            NetworkAddressChangedEventHandler(AddressChangedCallback);

            proxyInfo.Columns.Add("NetworkName");
            //proxyInfo.Columns.Add("NetworkType");
            proxyInfo.Columns.Add("ProxyIPAddress");
            proxyInfo.Columns.Add("ProxyPort");
            ds.Tables.Add(proxyInfo);
            if (File.Exists("proxy.xml")) ds.ReadXml("proxy.xml");
            dataGridViewProxy.DataSource = proxyInfo;
            
            notifyIconMain.Text = showConnectedId();
        }

        void AddressChangedCallback(object sender, EventArgs e)
        {
            Boolean isFound = false;
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && ni.OperationalStatus == OperationalStatus.Up)
                    {
                        Invoke(new Action(() =>
                        {
                            notifyIconMain.Text = showConnectedId();
                        }));

                        isFound = true;
                        break;
                    }

            if (!isFound)
            {
                Invoke(new Action(() =>
                {
                    notifyIconMain.Text = showConnectedId();
                }));
            }

        }

        // Show SSID and Signal Strength
        string showConnectedId()
        {

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = "netsh.exe",
                Arguments = "wlan show interfaces",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            };

            Process p = Process.Start(startInfo);
            string s = p.StandardOutput.ReadToEnd();
            string s1 = "N/A";
            p.WaitForExit();

            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);

            var isFound = false;
            if (s.Contains("SSID"))
            {
                s1 = s.Substring(s.IndexOf("SSID"));
                s1 = s1.Substring(s1.IndexOf(":"));
                s1 = s1.Substring(2, s1.IndexOf("\n")).Trim();
                
                proxyInfo.Rows.OfType<DataRow>().ToList().ForEach(r =>
                {
                    if (s1.Contains(r["NetworkName"].ToString()) && !isFound)
                    {
                        registry.SetValue("ProxyEnable", 1);
                        registry.SetValue("ProxyServer", r["ProxyIPAddress"] + ":" + r["ProxyPort"]);
                        s1 += " " + r["ProxyIPAddress"] + ":" + r["ProxyPort"];
                        isFound = true;
                    }
                });


            }

            if (!isFound)
            {
                registry.SetValue("ProxyEnable", 0);
                registry.SetValue("ProxyServer", "");
            }

            return "Proxy Changer : " + s1;
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.TopMost = true;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            ds.WriteXml("proxy.xml");
            this.Hide();
        }
    }
}
