using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Security.Permissions;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using ConecctorOneC;
using EdmLib;
using HostingWindowsForms.EPDM;
using Microsoft.Win32;
using Timer = System.Windows.Forms.Timer;

namespace HostingWindowsForms.Host
{
    public partial class HostingForm : Form
    {
        readonly RegistryKey _rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        //private Thread myThread;
        //Initialize
        public HostingForm()
        {
            InitializeComponent();

            _rkApp.SetValue("Vents service", Application.ExecutablePath);

            try
            {
                var perm = new SqlClientPermission(PermissionState.Unrestricted);
                perm.Demand();
            }
            catch
            {
                throw new ApplicationException("No permission");
            }

            CheckPdmVault();

            Program.HostForm = this;
        }
        #region Fields

        private ServiceHost _host;
        private ClassOfTasks _classOdTasks;
        public Epdm Epdm;

        static public IEdmVault5 Vault1;
        static public IEdmVault7 Vault2;

        //public static string VaultName = @"Tets_debag";
        public static string VaultName = @"Vents-PDM";

        #endregion
        private void HostingForm_Load(object sender, EventArgs e)
        {
            try
            {
                _host = new ServiceHost(typeof(VentsService));
                _host.Open();

                Connection.ConnectionString();

                labelRun.Text = @"Работает...";

                toolStripMenuRunService.Enabled = false;
                BtnStart.Enabled = false;

                _classOdTasks = new ClassOfTasks();
                _classOdTasks.OnNewMessageChataData += OnNewMessage;

                LoadMessages();

                //myTimer.Tick += new EventHandler(TimerEventProcessor);

                ////myTimer.Interval = 5000;
                //myTimer.Start();

                //// Runs the timer
                //while (exitFlag == false)
                //{
                //    Application.DoEvents();
                //}
                //myTimer.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + @": " + ex.TargetSite +@": " + ex.Source);
            }
        }
        #region Timer

        static Timer _myTimer = new Timer();
        static bool _exitFlag;
        // This is the method to run when the timer is raised.
        private static void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {

            (new Thread(delegate ()
            {
                const int hour = 8;
                const int minute = 04;

                m:
                while (true)
                {
                    // Displays a message box asking whether to continue running the timer.
                    //if (MessageBox.Show("Continue running?", "Count is: " + alarmCounter, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    if ((hour == DateTime.Now.Hour) && (minute == DateTime.Now.Minute))
                    {
                    // Restarts the timer and increments the counter.
                    //alarmCounter += 1;
                    //myTimer.Enabled = true;

                    //return;
                    goto m;
                    }
                    else
                    {
                        // Stops the timer.
                        _exitFlag = true;
                    }

                }

            })).Start();       
        }

        private void SetTimer()
        {
            //timer1.Stop();
            //var timeToAlarm = DateTime.Now.Date.AddHours(12).AddMinutes(03);
            //if (timeToAlarm == DateTime.Now)
            //{
            //    MessageBox.Show("1");
            //}
            //    timeToAlarm.AddDays(1);
            //timer1.Interval = (int)(timeToAlarm - DateTime.Now).TotalMilliseconds;
            //timer1.Start();

        }
        #endregion
        #region Tray icon menu

        private void toolStripMenuRunService_Click(object sender, EventArgs e)
        {
            _host = new ServiceHost(typeof(VentsService));
            _host.Open();
            toolStripMenuRunService.Enabled = false;
            labelRun.Text = @"Работает...";
        }

        private void toolStripMenuStopService_Click(object sender, EventArgs e)
        {
            _host.Close();
            toolStripMenuRunService.Enabled = true;
            labelRun.Text = @"Служба остановлена";
        }

        private void mynotifyicon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();

            this.WindowState = FormWindowState.Normal;

            this.ShowInTaskbar = true;
        }

        private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            _host.Close();
            this.Close();
        }

        #endregion
        public void OnNewMessage()
        {
            try
            {
                var i = (ISynchronizeInvoke)this;

                // Check if the event was generated from another
                // thread and needs invoke instead
                if (i.InvokeRequired)
                {
                    var tempDelegate = new ClassOfTasks.NewMessage(OnNewMessage);
                    i.BeginInvoke(tempDelegate, null);

                    return;
                }

                // If not coming from a seperate thread
                // we can access the Windows form controls
                LoadMessages();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + @": " + ex.TargetSite);
            }
        }
        public void LoadMessages()
        {
            try
            {
                dataGridView1.Rows.Clear();

                var dt = _classOdTasks.GetTaskListSql();

                foreach (DataRow r in dt.Rows)
                {
                    dataGridView1.Rows.Add(r["FileName"].ToString().Replace(".SLDPRT", ""));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + @": " + ex.TargetSite);
            }
        }
        #region Epdm
        //private const string VaultName = @"Vents-PDM";
        public void CheckPdmVault()
        {
            try
            {
                if (Vault1 == null)
                {
                    Vault1 = new EdmVault5();
                }

                Vault2 = (IEdmVault7)Vault1;

                var ok = Vault1.IsLoggedIn;

                if (!ok)
                {
                    Vault1.LoginAuto(VaultName, 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + @"; " + ex.StackTrace);
            }
        }
        #endregion
        #region Buttons

        private void BtnStart_Click(object sender, EventArgs e)
        {
            try
            {
                _host.Close();

                _host = new ServiceHost(typeof(VentsService));

                _host.Open();

                _classOdTasks = new ClassOfTasks();
                _classOdTasks.OnNewMessageChataData += new ClassOfTasks.NewMessage(OnNewMessage);

                LoadMessages();

                labelRun.Text = @"Работает...";

                toolStripMenuRunService.Enabled = false;

                BtnStart.Enabled = false;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message); }
        }
        
        private void BtnStopService_Click(object sender, EventArgs e)
        {
            _host.Close();
            toolStripMenuRunService.Enabled = true;

            //if (myThread != null)
            //{ myThread.Abort(); }

            labelRun.Text = @"Служба остановлена";

            BtnStart.Enabled = true;
        }
        #endregion
        // scroll always in botom
        private void richTextBoxLog_TextChanged_1(object sender, EventArgs e)
        {
            richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length; //Set the current caret position at the end
            richTextBoxLog.ScrollToCaret(); //Now scroll it automatically
        }
    }
}