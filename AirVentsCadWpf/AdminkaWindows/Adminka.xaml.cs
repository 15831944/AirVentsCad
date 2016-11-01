using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using AirVentsCadWpf.AirVentsClasses;

namespace AirVentsCadWpf.AdminkaWindows
{
    /// <summary>
    /// Interaction logic for Adminka.xaml
    /// </summary>
    /// 
    public partial class Adminka
    {

        public string GruopName { get; set; }

        readonly SqlBaseData _sqlBase = new SqlBaseData();

        #region Погашение окна

        public Adminka()
        {
            InitializeComponent();
            Closing += (MainWindow_Closing); 
        }
        
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
        }

        #endregion

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            ListUsersDomain();
        }

        private void getUsersList_Click(object sender, RoutedEventArgs e)
        {
            var dataGridCell = GetCell(1, 1);
            var textBlock = dataGridCell.Content as TextBlock;
            if (textBlock != null) MessageBox.Show(textBlock.Text);
        }
        
        #region GetDomainUsers

         class Users
        {
            public bool SelectUser { get; set; }
            public string AccountName { get; set; }
            public string FullUserName { get; set; }
        }


        private List<Users> lstAdUsers;

         void ListUsersDomain()
        {

            //string[] bn = {"otel", "zver"};

           // var
                lstAdUsers = new List<Users>();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, Properties.Settings.Default.Domain))// "vents.local"
                {
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        foreach (var result in searcher.FindAll())
                        {
                            var directoryEntry = result.GetUnderlyingObject() as DirectoryEntry;
                            var objSurveyUsers = new Users();//name   //samAccountName
                            if (directoryEntry != null)
                            {
                                

                                objSurveyUsers.AccountName = (String)directoryEntry.Properties["samAccountName"].Value;
                                objSurveyUsers.FullUserName = (String)directoryEntry.Properties["displayName"].Value;
                                //objSurveyUsers.FullUserName =  Convert.ToInt32( directoryEntry.Properties["userAccountControl"].Value).ToString();
                                //objSurveyUsers.FullUserName = (String)directoryEntry.Properties["Mail"].Value;
                            }
                            lstAdUsers.Add(objSurveyUsers);
                            //comboBoxOfUsers.Items.Add((String)de.Properties["displayName"].Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            // ListBox rr= new ListBox();
            UsersList.ItemsSource = lstAdUsers;
            UsersList.Columns[0].Header = "";
            UsersList.Columns[1].Header = "Имя входа"; UsersList.Columns[1].IsReadOnly = true;
            UsersList.Columns[0].CheckAccess(); // UsersList.Columns[1].SortDirection = ListSortDirection.Ascending;
            UsersList.Columns[2].Header = "Полное имя"; UsersList.Columns[2].IsReadOnly = true;
            UsersList.SelectedIndex = 0;
            
            //rr.ItemsSource = lstAdUsers;
            //rr.C.Columns[0].Header = "";
            //rr.Columns[1].Header = "Имя входа"; UsersList.Columns[1].IsReadOnly = true;
            //rr.Columns[0].CheckAccess(); // UsersList.Columns[1].SortDirection = ListSortDirection.Ascending;
            //rr.Columns[2].Header = "Полное имя"; UsersList.Columns[2].IsReadOnly = true;
            //rr.SelectedIndex = 0;
        }

        void ListUsersBoxDomain()
        {
            var lstAdUsers = new List<Users>();
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, Properties.Settings.Default.Domain))
                {
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        lstAdUsers.AddRange(searcher.FindAll().Select(result => result.GetUnderlyingObject()).OfType<DirectoryEntry>().Select(directoryEntry => new Users
                        {
                            AccountName = (String) directoryEntry.Properties["samAccountName"].Value, FullUserName = (String) directoryEntry.Properties["displayName"].Value
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            UsersList.ItemsSource = lstAdUsers;
            UsersList.Columns[0].Header = "";
            UsersList.Columns[1].Header = "Имя входа"; UsersList.Columns[1].IsReadOnly = true;
            UsersList.Columns[1].Width = 100;
            UsersList.Columns[0].CheckAccess(); // UsersList.Columns[1].SortDirection = ListSortDirection.Ascending;
            UsersList.Columns[2].Header = "Полное имя"; UsersList.Columns[2].IsReadOnly = true;
            UsersList.Columns[2].Width = 100;
            UsersList.SelectedIndex = 0;

          //  UsersListBox.Items.Add( lstADUsers);
        }

        #endregion
        

        private void AddUserToGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var item = UsersList.SelectedItem as Users;
            if (item == null) {return;}
            if (_sqlBase.AddUserToGroupInSqlBase(item.AccountName, item.FullUserName, GruopName))
            {
               // MessageBox.Show("Пользователь " + item.AccountName + " успешно добавлен в группу " + gruopName);
            }
            else
            {
               // MessageBox.Show("Не удалось добавить пользователя " + item.AccountName + " в группу " + gruopName);
            }
            
            //ComboboxValue();
            //var sqlBaseData = new SQLBaseData();
            //var currentItem = UsersList.SelectedItem as object;
            //var currentItem1 = UsersList.CurrentCell;
            //MessageBox.Show(currentItem1.ToString());
            // SearchInDataGridViewV(UsersList);
            // sqlBaseData.AddUserToSqlBase("NewNMN",  "FullName");
        }
        
        private void UsersList_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                var dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        var row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }
        
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        private void UsersList_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UsersList_PreviewMouseLeftButtonDown(sender, e);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DataGridCell cell = GetCell(1, 1);
            TextBlock tb = cell.Content as TextBlock;
            MessageBox.Show(tb.Text);
            //....
        }

       

        DataGridCell GetCell(int row, int column)
        {
            DataGridRow rowContainer = GetRow(row);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    UsersList.ScrollIntoView(rowContainer, UsersList.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        DataGridRow GetRow(int index)
        {
            var row = (DataGridRow)UsersList.ItemContainerGenerator.ContainerFromIndex(index);
            if (row != null) return row;
            UsersList.UpdateLayout();
            UsersList.ScrollIntoView(UsersList.Items[index]);
            row = (DataGridRow)UsersList.ItemContainerGenerator.ContainerFromIndex(index);
            return row;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                var visual = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = visual as T;
                if (child == null)
                {
                    return child = GetVisualChild<T>(visual);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        void UsersList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (var item in e.AddedCells)
            {
                var col = item.Column as DataGridColumn;
                var fc = col.GetCellContent(item.Item);

                if (fc is CheckBox)
                {
                  //  MessageBox.Show("Values - " + (fc as CheckBox).IsChecked);
                }
                else if (fc is TextBlock)
                {
                   // _userName = (fc as TextBlock).Text;
                   // string Str1 = (fc as TextBlock).Text;
                   // string Str2 = Str1 + (fc as TextBlock).Text;
                   //userName = userName + Str1 + Str2;
                    // MessageBox.Show("Values - " + (fc as TextBlock).Text + "  " + fc.ToString());
                }
              //  MessageBox.Show(userName);
            }
        }

        void Window_IsVisibleChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (GruopName == null)
            {
                GruopName = "Administrator";
            }
            AddUserToGroupButton.Content = "Добавить пользователя в группу " + GruopName;
        }

        private void UsersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ВыгрузитьПользователейВБазу_Click(object sender, RoutedEventArgs e)
        {
            if (lstAdUsers != null)
            {
                foreach (var item in lstAdUsers)
                {
                    _sqlBase.AddUserInSqlBase(item.AccountName, item.FullUserName);        
                }
            }
            else
            {
                MessageBox.Show("PINOP");
            }
        }
    }
}
