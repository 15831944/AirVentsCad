using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows.Forms;
using VentsCadServiceLibrary;

namespace VentsCadService
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button2.Enabled = false;
            //MessageBox.Show(localComputerName);

            IPAddress[] localIPs = Dns.GetHostAddresses(localComputerName);
            List<string> Data = new List<string>();
            foreach (var item in localIPs)
            {
                if (!item.IsIPv6LinkLocal)
                {
                    Data.Add(item.ToString());
                }                     
            }
            localHostIps.DataSource = Data;

        }

        string localComputerName = Dns.GetHostName();        

        public static bool IsLocalIpAddress(string host)
        {
            try
            { 
                // get host IP addresses
                IPAddress[] hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (IPAddress hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;          

        }       

        ServiceHost host { get; set; }

        Uri baseAddress { get; set; } = new Uri("http://localhost:8000/hello");//Uri("http://192.168.14.86:8000/hello");

        private void button1_Click(object sender, EventArgs e)
        {
            var baseAddress = new Uri("http://" + localHostIps.Text);// + ":/hello");

            host = new ServiceHost(typeof(VentsService), baseAddress);            

            var smb = new ServiceMetadataBehavior
            {
                HttpGetEnabled = true,
                MetadataExporter = { PolicyVersion = PolicyVersion.Policy15 }
            };
            host.Description.Behaviors.Add(smb);

            try
            {
                host.Open();
                Status.Text = $"The service is ready at {baseAddress}";
                button1.Enabled = false;
                button2.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            host.Close();
            Status.Text = $"The service is stopped";
            button1.Enabled = true;
            button2.Enabled = false;
        }
    }
}
