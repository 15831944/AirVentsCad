using System;
using System.Windows;
using System.DirectoryServices;
using System.Windows.Input;
using AirVentsCadWpf.DataControls;


namespace AirVentsCadWpf.MenuControls
{
    /// <summary>
    /// Interaction logic for AuthenticatedUC.xaml
    /// </summary>
    public partial class AuthenticatedUc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticatedUc"/> class.
        /// </summary>
        public AuthenticatedUc()
        {
            try
            {
                InitializeComponent();
                UserName.Text = Properties.Settings.Default.UserName;
                Password.Password = Properties.Settings.Default.Password;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            
        }

        /// <summary>
        /// PDMs the test base.
        /// </summary>
        void PdmTestBase()
        {
            Properties.Settings.Default.Developer = UserName.Text == "kb81";
            Properties.Settings.Default.Save();
        }

        void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PdmTestBase();
            if (AuthenticateUser("vents.local", UserName.Text, Password.Password))
            {
                Switcher.Switch(new MenuTree());
                Switcher.SwitchData(new MonoBlock01Uc50
                {
                    RoofOfUnit50 = { IsChecked = true },
                    InnerOfUnit50 = { IsChecked = true },
                    Panel50 = { IsChecked = true },
                    MontageFrame50 = { IsChecked = true },
                    DumperInMonoblock = { IsChecked = true },
                    Spigot = { IsChecked = true },
                    Panel1 = { SelectionStart = 2 }
                });
                if (RememberMe.IsChecked != true) return;
                Properties.Settings.Default.UserName = UserName.Text;
                Properties.Settings.Default.Password = Password.Password;
                Properties.Settings.Default.Save();
            }
            else
            {
                MessageBox.Show("Направильный пароль или логин!");
            }
        }

        static bool AuthenticateUser(string domainName, string userName, string password)
        {
            bool authenticateUser;
            try
            {
                var directoryEntry = new DirectoryEntry("LDAP://" + domainName, userName, password);
                var directorySearcher = new DirectorySearcher(directoryEntry);
                directorySearcher.FindOne();
                authenticateUser = true;
            }
            catch (Exception)
            {
                //Логгер.Ошибка("Ошибка:" + args.StackTrace, args.ToString(), "AuthenticateUser", "AuthenticatedUC");
                authenticateUser = false;
            }
            return authenticateUser;
        }

         void Grid_Loaded_1(object sender, RoutedEventArgs args)
        {
            try
            {
                PdmTestBase();
                if (!AuthenticateUser("vents.local", UserName.Text, Password.Password)) return;
                Switcher.Switch(new MenuTree());
                Switcher.SwitchData(new MonoBlock01Uc50
                {
                    RoofOfUnit50 = { IsChecked = true },
                    InnerOfUnit50 = { IsChecked = true },
                    Panel50 = { IsChecked = true },
                    MontageFrame50 = { IsChecked = true },
                    DumperInMonoblock = { IsChecked = true },
                    Spigot = { IsChecked = true },
                    Panel1 = {SelectionStart = 2}
                });
            }
            catch (Exception e)
            {
                MessageBox.Show("Во время запуска программы возникла ошибка" + e);
            }
            
        }

         private void UserName_KeyDown(object sender, KeyEventArgs e)
         {
             if (e.Key == Key.Enter)
             {
                 Button_Click_1(this, new RoutedEventArgs());
             }
         }

         private void Password_KeyDown(object sender, KeyEventArgs e)
         {
             if (e.Key == Key.Enter)
             {
                 Button_Click_1(this, new RoutedEventArgs());
             }
         }
    }
}
