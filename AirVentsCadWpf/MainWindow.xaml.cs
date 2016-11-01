using System;
using System.Windows;
using System.Windows.Controls;
using AirVentsCadWpf.AdminkaWindows;
using AirVentsCadWpf.DataControls;
using AirVentsCadWpf.MenuControls;
using System.Deployment.Application;
using AirVentsCadWpf.Properties;


namespace AirVentsCadWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Switcher.PageSwitcher = this;
                Switcher.Switch(new AuthenticatedUc());

                CurrentState.Content = State;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        /// <summary>
        /// Navigates the menu.
        /// </summary>
        /// <param name="nextPage">The next page.</param>
        public void NavigateMenu(UserControl nextPage)
        {
            MenuControlsGrid.Children.Clear();
            MenuControlsGrid.Children.Add(nextPage);
        }

        /// <summary>
        /// Navigates the data.
        /// </summary>
        /// <param name="nextPage">The next page.</param>
        public void NavigateData(UserControl nextPage)
        {
            DataControlsGrid.Children.Clear();
            DataControlsGrid.Children.Add(nextPage);
        }

        /// <summary>
        /// Navigates the button.
        /// </summary>
        /// <param name="nextPage">The next page.</param>
        public void NavigateButton(UserControl nextPage)
        {
            DataControlsGrid.Children.Clear();
            DataControlsGrid.Children.Add(nextPage);
        }

      // private readonly GroupUserControl _groupUser = new GroupUserControl();

        private readonly SettingsW _settings = new SettingsW();

        //   readonly LogViewer _nlogViewer = new LogViewer();

        private void Label_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
          //  _groupUser.Visibility = Visibility.Visible;
            _settings.Visibility = Visibility.Visible;
            _settings.Activate();
        }

        void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //OwnedWindow ownedWindow = new OwnedWindow();
            //ownedWindow.Owner = this;
            //ownedWindow.Closing += new System.ComponentModel.CancelEventHandler(ownedWindow_Closing);
            //ownedWindow.Show();
            // this.OwnedWindows.Count;
            Application.Current.Shutdown();
        }


        void Label_MouseLeftButtonDown_2(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //  _nlogViewer.Visibility = Visibility.Visible;
        }


        //private static Version AssemblyVersion
        //{
        //    get { return ApplicationDeployment.CurrentDeployment.CurrentVersion; }
        //}

        /// <summary>
        /// Gets the publish version.
        /// </summary>
        /// <value>
        /// The publish version.
        /// </value>
        public string PublishVersion
        {
            get
            {
                if (!ApplicationDeployment.IsNetworkDeployed) return  "Not Published";
                var ver = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                return $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
            }
        }

        private void CurrentVersion_Initialized(object sender, EventArgs e)
        {
            Title = " AirVentsCAD v." + PublishVersion;
            CurrentVersion.Content = Title;
            //CurrentVersion.Content = AssemblyVersion.Major + "." + AssemblyVersion.Minor + "." + AssemblyVersion.Build + "." + AssemblyVersion.Revision;
        }

        private void Label_MouseLeftButtonDown_3(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Settings.Default.UserName = "";
            Settings.Default.Password = "";
            Settings.Default.Reset();
           // Properties.Settings.Save();
            Switcher.Switch(new AuthenticatedUc());
            Switcher.SwitchData(new UnitElement());
        }
        
        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CurrentState.Content = State;
        }

        static string State => $"Хранилище: {Settings.Default.TestPdmBaseName}     База данных: {Settings.Default.ConnectionToSQL.Split(';')[1].Split('=')[1]}";

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var content = "";
            if (App.ProductsInWork.List == null) return;
            
            foreach (var item in App.ProductsInWork.List)
            {
                content = content + " | " + item;
            }
            label.Content = content;
        }
    }
}
