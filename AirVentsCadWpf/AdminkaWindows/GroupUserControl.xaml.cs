using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AirVentsCadWpf.AirVentsClasses;

namespace AirVentsCadWpf.AdminkaWindows
{
    /// <summary>
    /// Interaction logic for GroupUserControl.xaml
    /// </summary>
    /// 
    public partial class GroupUserControl
    {
        readonly Adminka _adminka = new Adminka();
        readonly SqlBaseData _sqlBaseData = new SqlBaseData();
        
        public GroupUserControl()
        {
            InitializeComponent();
            Closing += (MainWindow_Closing);
        }

        private void Window_Activated_1(object sender, EventArgs e)
        {
            LoadGroup();
            LoadUsersInGroup(_groupNameStr);
        }

        private string _groupNameStr;// = "Administrator";

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            //sqlBaseData.AddUserToSqlBase("NewName", "FullName");
            _adminka.GruopName = _groupNameStr;
            _adminka.Visibility = Visibility.Visible;
            LoadUsersInGroup(_groupNameStr);
        }

        static void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
        private void Window_Closing_2(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            LoadGroup();
            LoadUsersInGroup("Administrator");
        }

         void LoadGroup()
        {
            Groups.ItemsSource = _sqlBaseData.GroupsTable().AsDataView();
            Groups.Columns[0].Width = 150;
        }


        void LoadUsersInGroup(string groupname)
        {
            GroupUsers.ItemsSource = _sqlBaseData.GroupUsers(groupname).AsDataView();
            GroupUsers.Columns[0].Width = 110;
            GroupUsers.Columns[1].Width = 180;
            GroupUsers.VerticalContentAlignment =  VerticalAlignment.Center;
            GroupUsers.HorizontalContentAlignment = HorizontalAlignment.Center;
        }
         
        private void Groups_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           //LoadUsersInGroup(NewGroupName.Text); //NewGroupName.Text);
        }
        
        #region temp

        void Kohn()
        {
            Groups.Loaded += (s, e) => Groups.Columns.AsParallel().ForAll(column =>
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            });
        }

        public static Style NewCellStyle()
        {
            //And here is the C# code to achieve the above
            var style = new Style(typeof(DataGridCell));
            style.Setters.Add(new System.Windows.Setter
            {
                Property = HorizontalAlignmentProperty,
                Value = HorizontalAlignment.Center
            });
            return style;
        }
        #endregion

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < GroupUsers.Items.Count; i++)
            {
                try
                {
                    object item = i;
                    GroupUsers.SelectedItem = item;
                    GroupUsers.ScrollIntoView(item);
                    break;
                }
                catch (Exception)
                { }
            }


            DataRowView dataRow = (DataRowView)GroupUsers.SelectedItem;

            if (dataRow == null){return;}

            //int index = GroupUsers.CurrentCell.Column.DisplayIndex;

            string userNameStr = dataRow.Row.ItemArray[0].ToString();

            MessageBox.Show(userNameStr);

            if (_sqlBaseData.DeleteUserFromGroup(userNameStr, _groupNameStr))
            {
                MessageBox.Show("Пользователь " + userNameStr + " удален из группы " + _groupNameStr);
            }
            else
            {
                MessageBox.Show("Не удалось удалить пользователя " + userNameStr + " из группы " + _groupNameStr);
            }

            LoadUsersInGroup(_groupNameStr);
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewGroupName.Text == "")
            {
                MessageBox.Show("Введите имя группы");
                return;
            }
            _sqlBaseData.AddGroupToSqlBase(NewGroupName.Text, "");
            LoadGroup();
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (_sqlBaseData.DeleteGroupToSqlBase(NewGroupName.Text))
            {
                MessageBox.Show("Группа " + _groupNameStr + " успешно удалена.");
            }
            else if (NewGroupName.Text == "")
            {
                MessageBox.Show("Выберете группу для удаления.");
            }
            else
            {
                MessageBox.Show("Не удалось удалить группу  " + _groupNameStr + ".");
            }
            LoadGroup();
        }
        

        private void Groups_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (var item in e.AddedCells)
            {
                var col = item.Column as DataGridColumn;
                var fc = col.GetCellContent(item.Item);

                if (fc is CheckBox)
                {
                    MessageBox.Show("Values - " + (fc as CheckBox).IsChecked);
                }
                else if (fc is TextBlock)
                {
                    NewGroupName.Text = (fc as TextBlock).Text;
                    LoadUsersInGroup(NewGroupName.Text);
                    _groupNameStr = NewGroupName.Text;
                    // MessageBox.Show("Values - " + (fc as TextBlock).Text);
                }
            }
        }
        

        private void GroupUsers_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (var item in e.AddedCells)
            {
                var col = item.Column as DataGridColumn;
                var fc = col.GetCellContent(item.Item);

                if (fc is CheckBox)
                {
                   // MessageBox.Show("Values - " + (fc as CheckBox).IsChecked);
                }
                else if (fc is TextBlock)
                {
                    
                    //GroupNameStr =  (fc as TextBlock).Text;
                    //NewGroupName.Text = GroupNameStr;
                }
            }
        }

       

        
    }
}
