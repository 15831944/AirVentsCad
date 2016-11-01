using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AirVentsCadWpf.Properties;

namespace AirVentsCadWpf.DataControls.Loggers
{
    /// <summary>
    /// Interaction logic for HardwareUc.xaml
    /// </summary>
    public partial class LoggerUc
    {

        private PagingCollectionView _cview;

        /// <summary>
        /// 
        /// </summary>
        public class PagingCollectionView : CollectionView
        {
            private readonly IList _innerList;

            private int _currentPage = 1;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="innerList"></param>
            /// <param name="itemsPerPage"></param>
            public PagingCollectionView(IList innerList, int itemsPerPage)
                : base(innerList)
            {
                _innerList = innerList;
                ItemsPerPage = itemsPerPage;
            }

            /// <summary>
            /// 
            /// </summary>
            public override int Count
            {
                get
                {
                    if (_currentPage < PageCount) // page 1..n-1
                    {
                        return ItemsPerPage;
                    }
                    var itemsLeft = _innerList.Count % ItemsPerPage;
                    return 0 == itemsLeft ? ItemsPerPage : itemsLeft;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public int CurrentPage
            {
                get { return _currentPage; }
                set
                {
                    _currentPage = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CurrentPage"));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public int ItemsPerPage { get; }

            /// <summary>
            /// 
            /// </summary>
            public int PageCount => (_innerList.Count + ItemsPerPage - 1) / ItemsPerPage;

            private int EndIndex
            {
                get
                {
                    var end = _currentPage * ItemsPerPage - 1;
                    return (end > _innerList.Count) ? _innerList.Count : end;
                }
            }

            private int StartIndex => (_currentPage - 1) * ItemsPerPage;

            public override object GetItemAt(int index)
            {
                var offset = index % (ItemsPerPage);
                return _innerList[StartIndex + offset];
            }

            /// <summary>
            /// 
            /// </summary>
            public void MoveToNextPage()
            {
                if (_currentPage < PageCount)
                {
                    CurrentPage += 1;
                }
                Refresh();
            }

            /// <summary>
            /// 
            /// </summary>
            public void MoveToPreviousPage()
            {
                if (_currentPage > 1)
                {
                    CurrentPage -= 1;
                }
                Refresh();
            }

            /// <summary>
            /// 
            /// </summary>
            public void MoveToLastPage()
            {
                CurrentPage = PageCount;
                
                Refresh();
            }

        }

        void UpdateContext(IList list)
        {
            _cview = null;

            _cview = new PagingCollectionView(list, 25);
            DataContext = _cview;

            _cview.MoveToLastPage();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerUc"/> class.
        /// </summary>
        public LoggerUc()
        {

            InitializeComponent();

            UpdateContext(SqlLogStucture.LogStuctures().ToList());

            УровеньКомбо.ItemsSource = new[] { "ALL", "INFO", "DEBUG", "ERROR", "TRACE", "WARN" };
            УровеньКомбо.SelectedIndex = 0;

            App.ElementVisibility.ForUiElementsList(
                new List<UIElement> {Информация, УровеньGrid, Классы, ДатаВыборка, NLogGrid, SqlLog }, 
                Visibility.Collapsed);
        }

        
        private IEnumerable<LogStucture> _logList;

        private IEnumerable<LogStucture> _logListFiltered;

        private class LogStucture
        {
            public DateTime DataTime { get; private set; }
            public string Level { get; private set; }
            private string Place { get; set; }
            private string Text { get; set; }

            /// <summary>
            /// Logs the stuctures.
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<LogStucture> LogStuctures()
            {
                var listLog = new List<LogStucture>();
                
                try
                {
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AirVentsCadLog.log");
                    using (var reader = new StreamReader(logPath, Encoding.Default))
                    {
                        string readLine;
                        while ((readLine = reader.ReadLine()) != null)
                        {
                            var row = readLine.Split('|');
                            if (row.Count() != 4) continue;
                            try
                            {
                                listLog.Add(new LogStucture
                                {
                                    DataTime = Convert.ToDateTime(row[0]),
                                    Level = row[1],
                                    Place = row[2],
                                    Text = row[3]
                                });
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                        }
                    }
                    return listLog;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка при получении списка! \n" + e.Message);
                    return null;
                }
            }
        }

        private static IList<SqlLogStucture> SqlLogStuctures { get; set; }

        private void СобытийТаблицаDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            _sqlLogStuctures = SqlLogStucture.LogStuctures();

            var sqlLogStuctures = _sqlLogStuctures as IList<SqlLogStucture> ?? _sqlLogStuctures.ToList();

            SqlLogStuctures = sqlLogStuctures;
            ТаблицаДанных.ItemsSource = SqlLogStucture.LogStuctures();

            SqlLog.Header = "SQL " + SqlLogStucture.LogStuctures().Count() + " записи(ей)";

            Имя.ItemsSource = sqlLogStuctures.GroupBy(i => i.UserName).Where(i => i.Count() > 1).ToList();
            Имя.SelectedValuePath = "UserName";
            Имя.DisplayMemberPath = "UserName";
            Имя.SelectedIndex = -1;

            Дата.ItemsSource = sqlLogStuctures.GroupBy(i => i.ErrorTime.ToShortDateString()).Where(i => i.Count() > 1).ToList();
            Дата.SelectedValuePath = "ErrorTime";
            Дата.DisplayMemberPath = "ErrorTime";
            Дата.ItemStringFormat = "dd-MM-yyyy";
            Дата.SelectedIndex = -1;
            
            ИмяКласса.ItemsSource = sqlLogStuctures.GroupBy(i => i.ErrorModule).Where(i => i.Count() > 1).ToList();
            ИмяКласса.SelectedValuePath = "ErrorModule";
            ИмяКласса.DisplayMemberPath = "ErrorModule";
            ИмяКласса.SelectedIndex = -1;

            УровеньCombo.ItemsSource = sqlLogStuctures.GroupBy(i => i.ErrorState).Where(i => i.Count() > 1).ToList();
            УровеньCombo.SelectedValuePath = "ErrorState";
            УровеньCombo.DisplayMemberPath = "ErrorState";
            УровеньCombo.SelectedIndex = -1;
       
        }

        private void ScrollDown()
        {
            if (СобытийТаблицаDataGrid.Items.Count <= 0) return;
            var border = VisualTreeHelper.GetChild(СобытийТаблицаDataGrid, 0) as Decorator;
            var scroll = border?.Child as ScrollViewer;
            scroll?.ScrollToEnd();
        }

        private void УровеньКомбо_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                if (УровеньКомбо != null)
                {
                    switch (УровеньКомбо.SelectedValue.ToString())
                    {
                        case "ALL":
                            _logListFiltered = _logList;
                            break;
                        case "INFO":
                            _logListFiltered = _logList.Where(x => x.Level == "INFO");
                            break;
                        case "DEBUG":
                            _logListFiltered = _logList.Where(x => x.Level == "DEBUG");
                            break;
                        case "ERROR":
                            _logListFiltered = _logList.Where(x => x.Level == "ERROR");
                            break;
                        case "TRACE":
                            _logListFiltered = _logList.Where(x => x.Level == "TRACE");
                            break;
                        case "WARN":
                            _logListFiltered = _logList.Where(x => x.Level == "WARN");
                            break;
                    }
                }

                СобытийТаблицаDataGrid.ItemsSource = _logListFiltered;
                ScrollDown();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ДатаComboBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            try
            {
                if (ДатаComboBox != null)
                {

                    var selectedDate = (DateTime)ДатаComboBox.SelectedValue;

                    if (_logListFiltered != null)
                    {
                        _logListFiltered =
                       _logListFiltered.Where(x => x.DataTime.ToShortDateString().ToString() == selectedDate.ToShortDateString().ToString());
                    }
                    else
                    {
                        _logListFiltered =
                      _logList.Where(x => x.DataTime.ToShortDateString().ToString() == selectedDate.ToShortDateString().ToString());
                    }


                }

                СобытийТаблицаDataGrid.ItemsSource = _logListFiltered;
                ScrollDown();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            
        }

        #region DataTable From SQL to List<Class>

        private IEnumerable<SqlLogStucture> _sqlLogStuctures;

        /// <summary>
        /// 
        /// </summary>
        private class SqlLogStucture
        {

            /// <summary>
            /// 
            /// </summary>
            public DateTime ErrorTime { get;  set; }
            public string UserName { get;  set; }
            public string ErrorModule { get;  set; }
            public string ErrorMessage { get; set; }
            public string ErrorState { get;  set; }
            public string ErrorFunction { get; set; }
            public string ErrorCode { get; set; }


            /// <summary>
            /// Logs the stuctures.
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<SqlLogStucture> LogStuctures()
            {
                if (SqlLogStuctures != null) return SqlLogStuctures;

                try
                {
                    var listLog = (from DataRow row in ErrorLog.Rows
                                   select new SqlLogStucture
                                   {
                                       ErrorTime = Convert.ToDateTime(row["ErrorTime"]),
                                       UserName = row["UserName"].ToString(),
                                       ErrorModule = row["ErrorModule"].ToString(),
                                       ErrorMessage = row["ErrorMessage"].ToString(),
                                       ErrorState = row["ErrorState"].ToString(),
                                       ErrorFunction = row["ErrorFunction"].ToString(),
                                       ErrorCode = row["ErrorCode"].ToString()
                                   }).ToList();
                    return listLog;
                }
                catch (Exception e)
                {
                    MessageBox.Show("Ошибка при получении списка! \n" + e.Message);
                    return null;
                }
            }

            private static DataTable ErrorLog { get; } = ErrorLogTable();

            static DataTable ErrorLogTable()
            {
                if (ErrorLog != null) return ErrorLog;
                var errorLog = new DataTable();

                using (var sqlConnection = new SqlConnection(Settings.Default.ConnectionToSQL))
                {
                    try
                    {
                        sqlConnection.Open();
                        var sqlCommand = new SqlCommand(@"SELECT * FROM ErrorLog",// where ErrorTime Like '%" +//DateTime.Now.ToString("dd.mm.yyyy") "2015"+"%'", 
                            sqlConnection);
                        var sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                        sqlDataAdapter.Fill(errorLog);
                        //sqlDataAdapter.Dispose();
                        //ordersList.Columns["LastName"].ColumnName = "Фамилия";
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Ошибка выгрузки данных из базы");
                    }
                    finally
                    {
                        sqlConnection.Close();
                    }
                }
                return errorLog;
            }
           
        }

        #endregion

        private void ТаблицаДанных_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var style = new Style(typeof(DataGridCell));
            style.Setters.Add(new Setter(ContentTemplateProperty, Resources["templ"]));
            e.Column.CellStyle = style;
        }

        private void ОбновитьДанные_Click(object sender, RoutedEventArgs e)
        {
            СобытийТаблицаDataGrid_Loaded(sender, null);
        }

        private string Пользователь { get; set; }
        private string Класс { get; set; }
        private DateTime Время { get; set; }
        private string Уровень { get; set; }

        private IList<SqlLogStucture> _tempList;

        void ФильтрацияТаблициДанных()
        {
            //http://answerstop.org/question/77212/wpf-datagrid-is-filled-except-when-i-use-linq-to-filter-its-items
            if (SqlLogStuctures == null)return;

            //var _tempList = SqlLogStuctures;
            _tempList = SqlLogStuctures;


            if (Пользователь != null)
            {
                _tempList = SqlLogStuctures.Where(x => x.UserName == Пользователь).ToList();
            }
            
            //if (Класс != null)
            //{
            //    _tempList = _tempList.Where(x => x.errorModule == Класс).ToList();
            //}
            
            //if (Уровень != null)
            //{
            //    _tempList = _tempList.Where(x => x.errorState == Уровень).ToList();
            //}

            //_tempList = Информация.IsChecked == true ? _tempList.Where(x => x.errorState == "Info").ToList() : _tempList.ToList();

            if (Ошибки.IsChecked == true)
            {
                _tempList = _tempList.Where(x => x.ErrorState == "Error").ToList();
            }

            //MessageBox.Show("Уровень " + Уровень + SqlLogStuctures.Count + "  " + _tempList.Count);

            //if (Время.ToShortDateString() != "")
            //{
            //    _tempList = _tempList.Where(x => x.errorTime.ToShortDateString() == Время.ToShortDateString()).ToList();
            //}

            //ТаблицаДанных.ItemsSource = SqlLogStuctures;
            //MessageBox.Show("_tempList");

            ТаблицаДанных.ItemsSource = _tempList;

            if (ТаблицаДанных.Items.Count <= 0) return;
            var border = VisualTreeHelper.GetChild(ТаблицаДанных, 0) as Decorator;
            if (border == null) return;
            var scroll = border.Child as ScrollViewer;
            scroll?.ScrollToEnd();


            SqlLog.Header = "SQL " + _tempList.Count + " записи(ей)";

            //ТаблицаДанных.ItemsSource = SqlLogStucture.LogStuctures().
            //    Where(x => x.userName == Пользователь).
            //    Where(x => x.errorModule == Класс).
            //    Where(x => x.errorFunction == Уровень).
            //    Where(x => x.errorTime.ToShortDateString() == Время.ToShortDateString());
        }

        private void Имя_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Имя.SelectedValue == null) return;
            Пользователь = Имя.SelectedValue.ToString();
            ФильтрацияТаблициДанных();
        }

        private void Дата_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Дата.SelectedValue == null) return;
            Время = Convert.ToDateTime(Дата.SelectedValue);
            _tempList = SqlLogStuctures.Where(x => x.ErrorTime == Время).ToList();
            ФильтрацияТаблициДанных();
        }

        private void Ошибки_Click(object sender, RoutedEventArgs e)
        {
            ФильтрацияТаблициДанных();
        }

        private void Информация_Click(object sender, RoutedEventArgs e)
        {
            ФильтрацияТаблициДанных();
        }

        private void ВсеПользователи_Click(object sender, RoutedEventArgs e)
        {
            if (ВсеПользователи.IsChecked == true)
            {
                Имя.IsEnabled = true;
                ФильтрацияТаблициДанных();
            }
            else
            {
                ТаблицаДанных.ItemsSource = SqlLogStuctures;
                SqlLog.Header = "SQL " + SqlLogStuctures.Count() + " записи(ей)";
                Имя.IsEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _cview.MoveToNextPage();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _cview.MoveToPreviousPage();
        }

        private void button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = SqlLogStucture.LogStuctures().
                    Where(x => x.UserName.ToLower().Contains("kb81")).
                    ToList();
                if (list.Count == 0) return;
                
                UpdateContext(list);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }            
        }
    }
}
