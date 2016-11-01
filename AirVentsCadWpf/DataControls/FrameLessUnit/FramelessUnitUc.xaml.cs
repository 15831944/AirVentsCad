using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AirVentsCadWpf.AirVentsClasses;
using AirVentsCadWpf.Properties;
using AirVentsCadWpf.Логирование;
using VentsCadLibrary;
using VentsMaterials;
using ModelSw = AirVentsCadWpf.AirVentsClasses.UnitsBuilding.ModelSw;


namespace AirVentsCadWpf.DataControls.FrameLessUnit
{
    /// <summary>
    /// Interaction logic for FramelessUnitUc.xaml
    /// </summary>
    public partial class FramelessUnitUc
    {
        readonly SqlBaseData _sqlBaseData = new SqlBaseData();
        readonly SetMaterials _setMaterials = new SetMaterials();
        readonly ToSQL _toSql = new ToSQL();

        bool _isDeveloper = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FramelessUnitUc"/> class.
        /// </summary>
        public FramelessUnitUc()
        {
            InitializeComponent();

            ToSQL.Conn = Settings.Default.ConnectionToSQL;

            #region Профили промежуточные

            ШиринаПрофиля1.MaxLength = 3;
            ШиринаПрофиля2.MaxLength = 3;
            ШиринаПрофиля3.MaxLength = 3;
            ШиринаПрофиля4.MaxLength = 3;

            #endregion

            Усиления.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "Отсутствует", Content = "-"},
                new ComboBoxItem {ToolTip = "Нижней", Content = "Н"},
                new ComboBoxItem {ToolTip = "Нижней\nВерхней", Content = "НВ"},
                new ComboBoxItem {ToolTip = "Нижней\nЗадней", Content = "НЗ"},
                new ComboBoxItem {ToolTip = "Нижней\nВерхней\nЗадней", Content = "НВЗ"},
                new ComboBoxItem {ToolTip = "Нижней\nВерхней\nЗадней\nСъемной(ых)", Content = "НВЗС"},
                new ComboBoxItem {ToolTip = "Нижней\nВерхней\nЗадней\nСъемной(ых)\nТорцевой(ых)", Content = "НВЗСТ"},
                new ComboBoxItem {ToolTip = "Торцевой(ых)", Content = "Т"}
            };

            ТипПанелиПоиск.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {Content = "Несъемная", Name = "n2"},
                new ComboBoxItem {Content = "Съемная", Name = "n2"},
                new ComboBoxItem {Content = "Нажняя", Name = "n1"},
                new ComboBoxItem {Content = "Верхняя", Name = "n3"},
                new ComboBoxItem {Content = "Торцевая", Name = "n2"}
            };

            ТипПанелиПоиск.SelectedIndex = 0;


            ОпорнаяЧастьТипСдвоенойПанели.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "Разрез вдоль", Content = "Вдоль", Name = "n22"},
                new ComboBoxItem {ToolTip = "Разрез поперек", Content = "Разрез поперек", Name = "n11"},
                new ComboBoxItem {ToolTip = "Верхняя вдоль\nНижняя цельная", Content = "Верхняя вдоль\nНижняя цельная", Name = "n20"},
                new ComboBoxItem {ToolTip = "Верхняя поперек\nНижняя цельная", Content = "Верхняя поперек\nНижняя цельная", Name = "n10"}
            };

            ОпорнаяЧастьТипСдвоенойПанели.ToolTip = "Тип сдвоеной панели";


            КришаТипСдвоенойПанели.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "Разрез вдоль", Content = "Вдоль", Name = "n22"},
                new ComboBoxItem {ToolTip = "Разрез поперек", Content = "Разрез поперек", Name = "n11"},
                new ComboBoxItem {ToolTip = "Верхняя вдоль\nНижняя цельная", Content = "Верхняя вдоль\nНижняя цельная", Name = "n20"},
                new ComboBoxItem {ToolTip = "Верхняя поперек\nНижняя цельная", Content = "Верхняя поперек\nНижняя цельная", Name = "n10"}
            };

            КришаТипСдвоенойПанели.ToolTip = "Тип сдвоеной панели";

            НесъемнаяПанельСдвоенойПанели.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "Разрез вдоль", Content = "Вдоль", Name = "n22"},
                new ComboBoxItem {ToolTip = "Разрез поперек", Content = "Разрез поперек", Name = "n11"},
                new ComboBoxItem {ToolTip = "Верхняя вдоль\nНижняя цельная", Content = "Верхняя вдоль\nНижняя цельная", Name = "n20"},
                new ComboBoxItem {ToolTip = "Верхняя поперек\nНижняя цельная", Content = "Верхняя поперек\nНижняя цельная", Name = "n10"}
            };

            НесъемнаяПанельСдвоенойПанели.ToolTip = "Тип сдвоеной панели";

            DataGrid1.Visibility = Visibility.Collapsed; 

            var images =
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Image1", @"\AHU Selection\br_icon_teaser_cad_sr_pos_150(1).ico"),
                    new KeyValuePair<string, string>("Image2", @"D:\Photos\tn-36.jpg"),
                    new KeyValuePair<string, string>("Image3", @"D:\Photos\tn-37.jpg")
                };

            DataGrid1.ItemsSource = images;

            КришаТипПанели.ItemsSource = Totals.TypeOfCutFramelessPanel;
            ОпорнаяЧастьТипПанели.ItemsSource = Totals.TypeOfCutFramelessPanel;
            НесъемнаяПанельТипПанели1.ItemsSource = Totals.TypeOfCutFramelessPanel;

            НаличиеТорцевойПанели.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Отсутствуют"},
                new ComboBoxItem {ToolTip = "1", Content = "На входе"},
                new ComboBoxItem {ToolTip = "2", Content = "На выходе"},
                new ComboBoxItem {ToolTip = "3", Content = "Две торцевых панели"}
            };
            НаличиеТорцевойПанели.ToolTip = "Торцевые панели";

            var смещениеПоГоризонтали = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Центр"},
                new ComboBoxItem {ToolTip = "1", Content = "Влево"},
                new ComboBoxItem {ToolTip = "2", Content = "Вправо"}
            };

            СмещениеПоГоризонтали.ItemsSource = смещениеПоГоризонтали;
            СмещениеПоГоризонтали1.ItemsSource = смещениеПоГоризонтали;
            СмещениеПоГоризонтали3.ItemsSource = смещениеПоГоризонтали;

            СмещениеПоГоризонтали4.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Центр"},
                new ComboBoxItem {ToolTip = "1", Content = "Вверх"},
                new ComboBoxItem {ToolTip = "2", Content = "Вниз"}
            };

            СмещениеПоВертикали.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Центр"},
                new ComboBoxItem {ToolTip = "1", Content = "Вверх"},
                new ComboBoxItem {ToolTip = "2", Content = "Вниз"}
            };

            СмещениеПоВертикали8.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Центр"},
                new ComboBoxItem {ToolTip = "1", Content = "Вперед"},
                new ComboBoxItem {ToolTip = "2", Content = "Назад"}
            };
            СмещениеПоВертикали12.ItemsSource = СмещениеПоВертикали8.ItemsSource;
            СмещениеПоВертикали16.ItemsSource = СмещениеПоВертикали8.ItemsSource;


            Фланец.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "20"},
                new ComboBoxItem {ToolTip = "1", Content = "30"}
            };

            СмещениеПоГоризонтали2.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Центр"},
                new ComboBoxItem {ToolTip = "1", Content = "Влево"},
                new ComboBoxItem {ToolTip = "2", Content = "Вправо"}
            };

            СмещениеПоВертикали4.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Центр"},
                new ComboBoxItem {ToolTip = "1", Content = "Вверх"},
                new ComboBoxItem {ToolTip = "2", Content = "Вниз"}
            };

            Фланец2.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "20"},
                new ComboBoxItem {ToolTip = "1", Content = "30"}
            };

            Фланец1.ItemsSource = Фланец2.ItemsSource;
            Фланец3.ItemsSource = Фланец2.ItemsSource;
            Фланец4.ItemsSource = Фланец2.ItemsSource;

            ТипУстановкиПромежуточныеВставки.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Без усиливающих панелей"},
                new ComboBoxItem {ToolTip = "1", Content = "Две усиливающие панели"},
                new ComboBoxItem {ToolTip = "2", Content = "Первая усиливающая панель"},
                new ComboBoxItem {ToolTip = "3", Content = "Вторая усиливающая панель"}
            };

            ТипУстановкиПромежуточныеВставки.ToolTip = "Усиливающие панели";

            ТипПанели.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "Верхняя (для внутренней установки)", Content = "21"},
                new ComboBoxItem {ToolTip = "Верхняя (для установки с односкатной крышей)", Content = "22"},
                new ComboBoxItem {ToolTip = "Верхняя (для установки с двускатной крышей)", Content = "23"},
                new ComboBoxItem {ToolTip = "Съемная (панель простая)", Content = "04"},
                new ComboBoxItem {ToolTip = "Съемная (панель теплообменника)", Content = "05"},
                new ComboBoxItem {ToolTip = "Несъемная (панель простая)", Content = "01"},
                new ComboBoxItem {ToolTip = "Нижняя (для внутренней установки)", Content = "30"},
                new ComboBoxItem {ToolTip = "Нижняя (для установки с монтажной рамой)", Content = "31"},
                new ComboBoxItem {ToolTip = "Нижняя (для установки с монтажными ножками)", Content = "32"}
            }.OrderBy(x=>x.Content);

            ТипУстановки.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "01", Content = "Одна съемная панель"},
                new ComboBoxItem {ToolTip = "02", Content = "Две съемных панели"},
                new ComboBoxItem {ToolTip = "03", Content = "Три съемных панели"}
            };

            КолвоПанелей.ItemsSource = new List<ComboBoxItem>
            {
                new ComboBoxItem {Content = "1"},
                new ComboBoxItem {Content = "2"},
                new ComboBoxItem {Content = "3"}
            };

            ТолщинаВнешней.ItemsSource = Totals.SheetMetalThikness;                
            ТолщинаВнешней.SelectedIndex = 2;

            ТолщинаВннутренней.ItemsSource = Totals.SheetMetalThikness;
            ТолщинаВннутренней.SelectedIndex = 2;

            SectionTextBox.ItemsSource = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList();

            Totals.SetMaterial(MaterialP1);
            Totals.SetMaterial(MaterialP2);

            Ral1.ItemsSource = ((IListSource)_toSql.RalTable()).GetList();
            Ral1.DisplayMemberPath = "RAL";
            Ral1.SelectedValuePath = "Hex";
            Ral1.SelectedIndex = 0;

            Ral2.ItemsSource = ((IListSource)_toSql.RalTable()).GetList();
            Ral2.DisplayMemberPath = "RAL";
            Ral2.SelectedValuePath = "Hex";
            Ral2.SelectedIndex = 0;

            Ral1.Visibility = Visibility.Hidden;
            Ral2.Visibility = Visibility.Hidden;


            Table.ItemsSource = ((IListSource)_toSql.RalTable()).GetList();

            CoatingType1.ItemsSource = ((IListSource)_setMaterials.CoatingTypeDt()).GetList();
            CoatingType1.DisplayMemberPath = "Name";
            CoatingType1.SelectedValuePath = "Code";
            CoatingType1.SelectedIndex = 0;

            CoatingType2.ItemsSource = ((IListSource)_setMaterials.CoatingTypeDt()).GetList();
            CoatingType2.DisplayMemberPath = "Name";
            CoatingType2.SelectedValuePath = "Code";
            CoatingType2.SelectedIndex = 0;

            CoatingClass1.ItemsSource = _setMaterials.CoatingListClass();
            CoatingClass2.ItemsSource = _setMaterials.CoatingListClass();
           

            UpdateCombo(Шумоизоляция, "23");
            UpdateCombo(КришаТип, "3");
            UpdateCombo(ОпорнаяЧастьТип, "1");

            UpdateCombo(ТипСъемнойПанели1, "2");
            UpdateCombo(ТипСъемнойПанели2, "2");
            UpdateCombo(ТипСъемнойПанели3, "2");

            UpdateCombo(ПрименениеСкотча, "25");

            ТипНесъемнойПанели.ItemsSource = ((IListSource)_sqlBaseData.PanelsTable("2")).GetList();
            ТипНесъемнойПанели.DisplayMemberPath = "PanelTypeName";
            ТипНесъемнойПанели.SelectedValuePath = "PanelTypeCode";
            ТипНесъемнойПанели.SelectedIndex = 3;

            UpdateCombo(ТипУсилПанели1, "4");
            UpdateCombo(ТипУсилПанели2, "4");

            #region UnitFramelessFULL

            Lenght.MaxLength = 5;
            WidthU.MaxLength = 5;
            WidthU.IsReadOnly = true;
            HeightU.MaxLength = 5;
            HeightU.IsReadOnly = true;
            Lenght.MaxLength = 5;
            
            SizeOfUnit.ItemsSource = ((IListSource) _sqlBaseData.AirVentsStandardSize()).GetList();
            SizeOfUnit.DisplayMemberPath = "Type";
            SizeOfUnit.SelectedValuePath = "Type";
            SizeOfUnit.SelectedIndex = 0;

            #endregion

            WidthU.IsEnabled = false;
            HeightU.IsEnabled = false;

            App.ElementVisibility.ForUiElementsList(new List<UIElement>
            {
                ExistingParts,
                Table,
                ПанелиУстановки,
                ДлинаLabel,

                //Кол-во панелей
                Ширина2, ШиринаПанели2,
                Ширина3, ШиринаПанели3,
                Отступ2, Отступ2L,
                Отступ3, Отступ3L,
                
                КолвоПанелей11,
                КолвоПанелей1,

                PartsGeneration
            },
            Visibility.Collapsed);

            Quary.Visibility = Visibility.Collapsed;

            Левая.IsChecked = true;

            FindParts.IsEnabled = false;
        }

        void UpdateCombo(Selector comboBox, string query)
        {
            comboBox.ItemsSource = ((IListSource)_sqlBaseData.PanelsTable(query)).GetList();
            comboBox.DisplayMemberPath = "PanelTypeName";
            comboBox.SelectedValuePath = "PanelTypeCode";
            comboBox.SelectedIndex = 0;
        }
      

        void SizeOfUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SizeOfUnit?.SelectedItem == null) return;
            
            var id = Convert.ToInt32(((DataRowView) SizeOfUnit.SelectedItem)["SizeID"].ToString());
            var sqlBaseData = new SqlBaseData();
            var standartUnitSizes = sqlBaseData.StandartSize(id, 6);

            if (WidthU == null || HeightU == null)
            {
                return;
            }
            WidthU.Text = standartUnitSizes[0];
            HeightU.Text = standartUnitSizes[1];
        }

        void nonstandard_Checked(object sender, RoutedEventArgs e)
        {
            WidthU.IsReadOnly = false;
            HeightU.IsReadOnly = false;
            WidthU.IsEnabled = true;
            HeightU.IsEnabled = true;
            WidthLabel.Visibility = Visibility.Visible;
            WidthU.Visibility = Visibility.Visible;
            HeightLabel.Visibility = Visibility.Visible;
            HeightU.Visibility = Visibility.Visible;
        }

        void nonstandard_Unchecked(object sender, RoutedEventArgs e)
        {
            SizeOfUnit_SelectionChanged(null,null);
            WidthU.IsReadOnly = true;
            HeightU.IsReadOnly = true;
            WidthU.IsEnabled = false;
            HeightU.IsEnabled = false;
        }

        #region BUILDING

        /// <summary>
        /// 
        /// </summary>
        public class AddingConstructParameters
        {
            /// <summary>
            /// 
            /// </summary>
            public double ScrewsByHeight { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ScrewsByWidth { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ScrewsByLenght { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<string> PanelsToDelete { get; set; }
        }

        void BUILDING_Click(object sender, RoutedEventArgs args)
        {
            PartsPdmTable.ItemsSource = null;
            if (!Totals.RemPanWidthCheck(ШиринаСъемнойПанели2, 2)) return;
            if (!Totals.RemPanWidthCheck(ШиринаСъемнойПанели3, 3)) return;
            if (!Totals.RemPanWidthCheck(ШиринаСъемнойПанели1, 1)) return;       

            Логгер.Информация("Начато построение бескаркасной установки","",
                "Сохранение настроек программы",
                "FramelessUnitUc");

            try
            { 
                var modelSw = new ModelSw();

                #region PathToBlock

                if (onlySearch.IsChecked != true)
                {
                    var framelessBlockNewName = $"{SizeOfUnit.Text} {OrderTextBox.Text}B Section {SectionTextBox.Text}";
                    var orderFolder =
                        $@"{Settings.Default.DestinationFolder}\{ModelSw.UnitFramelessOreders}\{SizeOfUnit.Text}\{SizeOfUnit.Text} {OrderTextBox.Text}B";
                    var framelessBlockNewPath = new FileInfo($@"{orderFolder}\{framelessBlockNewName}.SLDASM").FullName;

                    if (onlySearch.IsChecked != true  && 
                        ModelSw.OpenIfExist(framelessBlockNewPath, VaultSystem.VentsCadFile.Type.Assembly, null)) return;                    
                }

                #endregion

                var width = Convert.ToDouble(WidthU.Text);
                var height = Convert.ToDouble(HeightU.Text);
                var lenght = Convert.ToDouble(Lenght.Text);
                
                #region Панели

                var materialP1 = new[] {MaterialP1.SelectedValue.ToString(), ТолщинаВнешней.Text};
                var materialP2 = new[]
                {
                    MaterialP2.SelectedValue.ToString(), ТолщинаВннутренней.Text,
                    Шумоизоляция.SelectedValue.ToString(),
                    ПрименениеСкотча.SelectedValue.ToString()
                };

                string panelUp = null;
                string panelDown = null;
                string panelFixed = null;

                var config = Левая.IsChecked == true ? "01" : "02";
                var configDown = Левая.IsChecked == true ? "02" : "01";
                var leftSide = Левая.IsChecked == true;
                
                var stickyTape = ПрименениеСкотча.Text == "Со скотчем";


                var addingConstructParameters = new AddingConstructParameters();

                var typeOfDoubleКриша = (ComboBoxItem) КришаТипСдвоенойПанели.SelectedItem;
                var typeOfDoubleStrКриша = КришаТипСдвоенойПанели.Visibility == Visibility.Visible
                    ? typeOfDoubleКриша.Name.Replace("n", "") : "00";

                var typeOfDoubleОпорнаяЧасть = (ComboBoxItem) ОпорнаяЧастьТипСдвоенойПанели.SelectedItem;
                var typeOfDoubleStrОпорнаяЧасть = ОпорнаяЧастьТипСдвоенойПанели.Visibility == Visibility.Visible
                    ? typeOfDoubleОпорнаяЧасть.Name.Replace("n", "") : "00";

                var typeOfDoubleНесъемнаяПанель = (ComboBoxItem) НесъемнаяПанельСдвоенойПанели.SelectedItem;
                var typeOfDoubleStrНесъемнаяПанель = НесъемнаяПанельСдвоенойПанели.Visibility == Visibility.Visible
                    ? typeOfDoubleНесъемнаяПанель.Name.Replace("n", "") : "00";

                var ironwares = new ModelSw.Ironwares
                {
                    Height = height, Width = width, Lenght = lenght,
                    TypeOfDownPanel = typeOfDoubleStrОпорнаяЧасть,
                    TypeOfUnremPanel = typeOfDoubleStrНесъемнаяПанель,
                    TypeOfUpPanel = typeOfDoubleStrКриша
                };

                modelSw.PartsToDeleteList = new List<KeyValuePair<string, string>>();

                #region Панели Торцевые

                string panelBack1 = null;string panelBack2 = null;

                List<string> partsToDeleteList;
                var existingAsmsAndParts2 = new List<ModelSw.ExistingAsmsAndParts>();

                const string noBrush = "Без покрытия";
                var покрытие = new[]
                {
                    Ral1.Text, Ral1.Text != noBrush ? CoatingType1.Text : "0",
                    Ral1.Text != noBrush ? CoatingClass1.Text : "0",
                    Ral2.Text, Ral2.Text != noBrush ? CoatingType2.Text : "0",
                    Ral2.Text != noBrush ? CoatingClass2.Text : "0",
                    Ral1.SelectedValue?.ToString() ?? noBrush,
                    Ral2.SelectedValue?.ToString() ?? noBrush
                };

                var screwsBackPanel =
                    new ModelSw.Screws
                    {
                        ByWidth = ironwares.ScrewsByHeight - 1000,
                        ByHeight = ironwares.ScrewsByWidth - 1000,

                        ByWidthInnerUp = ironwares.ScrewsByHeight,
                        ByHeightInnerUp = ironwares.ScrewsByWidth
                    };

                var typeOfBackPanel =
                    new[]
                    {
                        "35",
                        "Торцевая панель",
                        Convert.ToString(SqlBaseData.PanelsTypeId("35"))                      
                    };

                 

                if (НаличиеТорцевойПанели.SelectedIndex == 1 || НаличиеТорцевойПанели.SelectedIndex == 3)
                {
                    try
                    {
                        if (!(PartsGenerationChk.IsChecked == true & BackPanels.IsChecked != true))
                        {
                            //const string paneltype = "Панель торцевая первая";
                            List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                            panelBack1 = modelSw.PanelsFramelessStr(
                                typeOfPanel: typeOfBackPanel,
                                width: Convert.ToString(height - 40), height: Convert.ToString(width - 40),
                                materialP1: materialP1, materialP2: materialP2, 
                                скотч: stickyTape,
                                усиление: _panelBack,
                                config: config,
                                расположениеПанелей: "",
                                покрытие: покрытие,
                                расположениеВставок: "",
                                первыйТип: null,
                                типТорцевой: ParmsOfBackPanel1(),
                                типДвойной: "00",
                                screws: screwsBackPanel,
                                partsToDeleteList: out partsToDeleteList,
                                existingAsmsAndParts: out existingAsmsAndParts,
                                onlySearch: onlySearch.IsChecked == true);

                            existingAsmsAndParts2.AddRange(existingAsmsAndParts);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        Логгер.Ошибка(ex.Message, ex.StackTrace, ex.TargetSite.ToString(),"FramelessUnitUc");
                    }
                }

                if (НаличиеТорцевойПанели.SelectedIndex == 2 || НаличиеТорцевойПанели.SelectedIndex == 3)
                {
                    try
                    {
                        if (!(PartsGenerationChk.IsChecked == true & BackPanels.IsChecked != true))
                        {
                            List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                            panelBack2 = modelSw.PanelsFramelessStr(
                                typeOfPanel: typeOfBackPanel,
                                width: Convert.ToString(height - 40), height: Convert.ToString(width - 40),
                                materialP1: materialP1, materialP2: materialP2,
                                скотч: stickyTape,
                                усиление: _panelBack,
                                config: config,
                                расположениеПанелей: "",
                                покрытие: покрытие,
                                расположениеВставок: "",
                                первыйТип: null,
                                типТорцевой: ParmsOfBackPanel2(),
                                типДвойной: "00",
                                screws: screwsBackPanel,
                                partsToDeleteList: out partsToDeleteList,
                                existingAsmsAndParts: out existingAsmsAndParts,
                                onlySearch: onlySearch.IsChecked == true);

                            existingAsmsAndParts2.AddRange(existingAsmsAndParts);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                        Логгер.Ошибка(
                            ex.Message,
                            ex.StackTrace,
                            ex.TargetSite.ToString(),
                            "FramelessUnitUc");
                    }
                }
               
                #endregion
                
                var panelBack = new[] {panelBack1, panelBack2};

                try
                {
                    if (!(PartsGenerationChk.IsChecked == true & UpPanels.IsChecked != true))
                    {
                        List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                        panelUp = modelSw.PanelsFramelessStr(
                            typeOfPanel:
                                new[]
                                {
                                    ТипВерхнейПанели.Text, КришаТип.Text,
                                    Convert.ToString(SqlBaseData.PanelsTypeId(ТипВерхнейПанели.Text))
                                },
                            width: Convert.ToString(lenght), height: Convert.ToString(width),
                            materialP1: materialP1, materialP2: materialP2,
                            скотч: stickyTape,
                            усиление: PanelUpChk.IsChecked == true,
                            config: config,
                            расположениеПанелей: PanelsConfig(),
                            покрытие: покрытие,
                            расположениеВставок: ProfilsConfig(),
                            первыйТип: null,
                            типТорцевой: ParmsOfRoofPanelFlange(),
                            типДвойной: typeOfDoubleStrКриша,
                            screws: new ModelSw.Screws
                            {
                                ByWidth = ironwares.ScrewsByLenght,
                                ByHeight = ironwares.ScrewsByWidth,
                                ByWidthInner =
                                    typeOfDoubleStrНесъемнаяПанель.Contains("1")
                                        ? ironwares.ScrewsByLenght - 1000 > 2000
                                            ? ironwares.ScrewsByLenght - 2000
                                            : ironwares.ScrewsByLenght - 1000
                                        : ironwares.ScrewsByLenght - 1000,
                                ByHeightInner = ironwares.ScrewsByWidth - 1000
                            },
                            partsToDeleteList: out partsToDeleteList,
                            existingAsmsAndParts: out existingAsmsAndParts,
                            onlySearch: onlySearch.IsChecked == true);

                        existingAsmsAndParts2.AddRange(existingAsmsAndParts);

                        foreach (var partName in partsToDeleteList)
                        {
                            modelSw.PartsToDeleteList.Add(new KeyValuePair<string, string>(@panelUp, partName));
                        }
                        addingConstructParameters.PanelsToDelete = new List<string>();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());

                    Логгер.Ошибка(
                        ex.Message,
                        ex.StackTrace,
                        ex.TargetSite.ToString(),
                        "FramelessUnitUc");
                }

                #region Панель нижняя под монтажные ножки

                try
                {
                    if (!(PartsGenerationChk.IsChecked == true & DownPanels.IsChecked != true))
                    {
                        //const string paneltype = "Панель нижняя под монтажные ножки";
                        List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                        panelDown = modelSw.PanelsFramelessStr(
                            typeOfPanel:
                                new[]
                                {
                                    ТипПанелиНижней.Text, ОпорнаяЧастьТип.Text,
                                    Convert.ToString(SqlBaseData.PanelsTypeId(ТипПанелиНижней.Text))
                                },
                            width: Convert.ToString(lenght), height: Convert.ToString(width),
                            materialP1: materialP1, materialP2: materialP2,
                            скотч: stickyTape,// ПрименениеСкотча.Text, ПрименениеСкотча.Text,
                            усиление: PanelDownChk.IsChecked == true,
                            config: configDown,  // REVERSE
                            расположениеПанелей: PanelsConfig(),
                            покрытие: покрытие,
                            расположениеВставок: ProfilsConfig(),
                            первыйТип: null,
                            типТорцевой: ParmsOfMontagePanelFlange(),
                            типДвойной: typeOfDoubleStrОпорнаяЧасть,
                            screws: new ModelSw.Screws
                            {
                                ByWidth = ironwares.ScrewsByLenght,
                                ByHeight = ironwares.ScrewsByWidth,

                                ByWidthInner =
                                    typeOfDoubleStrНесъемнаяПанель.Contains("1")
                                        ? ironwares.ScrewsByLenght - 1000 > 2000
                                            ? ironwares.ScrewsByLenght - 2000
                                            : ironwares.ScrewsByLenght - 1000
                                        : ironwares.ScrewsByLenght - 1000,
                                ByHeightInner = ironwares.ScrewsByWidth - 1000
                            },
                            partsToDeleteList: out partsToDeleteList,
                            existingAsmsAndParts: out existingAsmsAndParts,
                            onlySearch: onlySearch.IsChecked == true);

                        existingAsmsAndParts2.AddRange(existingAsmsAndParts);

                        foreach (var partName in partsToDeleteList)
                        {
                            modelSw.PartsToDeleteList.Add(new KeyValuePair<string, string>(@panelDown, partName));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Логгер.Ошибка(ex.Message,
                        ex.StackTrace,
                        ex.TargetSite.ToString(),
                        "FramelessUnitUc");
                }

                #endregion

                #region Съемная и усиливающие панели

                var panelRemovable = new[] {""};
                string panelRemovable1 = null;
                string panelRemovable2 = null;
                string panelRemovable3 = null;

                try
                {
                    //"Съемная";
                    try
                    {
                        if (!(PartsGenerationChk.IsChecked == true & RemPanels.IsChecked != true))
                        {
                            List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                            panelRemovable1 = modelSw.PanelsFramelessStr(
                                typeOfPanel: new[]
                                {
                                    ТипСъемнойПанели1.SelectedValue.ToString(),
                                    ТипСъемнойПанели1.Text,
                                    Convert.ToString(SqlBaseData.PanelsTypeId(ТипСъемнойПанели1.SelectedValue.ToString()))
                                },
                                width: Convert.ToString(ШиринаСъемнойПанели1.Text), height: Convert.ToString(height - 40),
                                materialP1: materialP1, materialP2: materialP2,
                                скотч: stickyTape,// ПрименениеСкотча.Text, ПрименениеСкотча.Text,
                                усиление: УсилениеСъемнойПанели1.IsChecked == true,
                                config: "00",
                                расположениеПанелей: "",
                                покрытие: покрытие,
                                расположениеВставок: null,
                                первыйТип: null,
                                типТорцевой: null,
                                типДвойной: "00",
                                screws: null,
                                partsToDeleteList: out partsToDeleteList,
                                existingAsmsAndParts: out existingAsmsAndParts,
                                onlySearch: onlySearch.IsChecked == true);

                            existingAsmsAndParts2.AddRange(existingAsmsAndParts);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.ToString());
                    }
                    try
                    {
                        if (!(PartsGenerationChk.IsChecked == true & RemPanels.IsChecked != true))
                        {
                            List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                            panelRemovable2 = modelSw.PanelsFramelessStr(
                                typeOfPanel: new[]
                                {
                                    ТипСъемнойПанели2.SelectedValue.ToString(),
                                    ТипСъемнойПанели2.Text,
                                    Convert.ToString(SqlBaseData.PanelsTypeId(ТипСъемнойПанели2.SelectedValue.ToString()))
                                },
                                width: Convert.ToString(ШиринаСъемнойПанели2.Text), height: Convert.ToString(height - 40),
                                materialP1: materialP1, materialP2: materialP2,
                                скотч: stickyTape,// ПрименениеСкотча.Text, ПрименениеСкотча.Text,
                                усиление: УсилениеСъемнойПанели2.IsChecked == true,
                                config: "00",
                                расположениеПанелей: "",
                                покрытие: покрытие,
                                расположениеВставок: null,
                                первыйТип: null,
                                типТорцевой: null,
                                типДвойной: "00",
                                screws: null,
                                partsToDeleteList: out partsToDeleteList,
                                existingAsmsAndParts: out existingAsmsAndParts,
                                onlySearch: onlySearch.IsChecked == true);

                            existingAsmsAndParts2.AddRange(existingAsmsAndParts);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.ToString());
                    }

                    try
                    {
                        if (!(PartsGenerationChk.IsChecked == true & RemPanels.IsChecked != true))
                        {
                            List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                            panelRemovable3 = modelSw.PanelsFramelessStr(
                                typeOfPanel: new[]
                                {
                                    ТипСъемнойПанели3.SelectedValue.ToString(),
                                    ТипСъемнойПанели3.Text,
                                    Convert.ToString(SqlBaseData.PanelsTypeId(ТипСъемнойПанели3.SelectedValue.ToString()))
                                },
                                width: Convert.ToString(ШиринаСъемнойПанели3.Text), height: Convert.ToString(height - 40),
                                materialP1: materialP1, materialP2: materialP2,
                                скотч: stickyTape,// ПрименениеСкотча.Text, ПрименениеСкотча.Text,
                                усиление: УсилениеСъемнойПанели3.IsChecked == true,
                                config: "00",
                                расположениеПанелей: "",
                                покрытие: покрытие,
                                расположениеВставок: null,
                                первыйТип: null,
                                типТорцевой: null,
                                типДвойной: "00",
                                screws: null,
                                partsToDeleteList: out partsToDeleteList,
                                existingAsmsAndParts: out existingAsmsAndParts,
                                onlySearch: onlySearch.IsChecked == true);

                            existingAsmsAndParts2.AddRange(existingAsmsAndParts);
                        }
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.ToString());
                    }

                    #region Усиливющие панели

                    string panelReinforcing1 = null;
                    string panelReinforcing2 = null;

                    if (ТипУсилПанели1.Visibility == Visibility.Visible)
                    {
                        try
                        {
                            if (!(PartsGenerationChk.IsChecked == true & StrenghtPanels.IsChecked != true))
                            {
                                var typeOfPanel = ТипУсилПанели1.SelectedItem as ComboBoxItem;
                                List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                                panelReinforcing1 = modelSw.PanelsFramelessStr(
                                    typeOfPanel: new[]
                                    {
                                        ТипУсилПанели1.SelectedValue.ToString(),
                                        ТипУсилПанели1.Text,
                                        Convert.ToString(SqlBaseData.PanelsTypeId(ТипУсилПанели1.SelectedValue.ToString()))
                                    },

                                    width: Convert.ToString(130), height: Convert.ToString(height - 40),
                                    materialP1: materialP1, materialP2: materialP2,
                                    скотч: stickyTape,// ПрименениеСкотча.Text, ПрименениеСкотча.Text,
                                    усиление: false,
                                    config: "00",
                                    расположениеПанелей: "",
                                    покрытие: покрытие,
                                    расположениеВставок: null,
                                    первыйТип: new[]
                                    {
                                        ТипНесъемнойПанели.SelectedValue.ToString(),
                                        ТипНесъемнойПанели.Text,
                                        Convert.ToString(SqlBaseData.PanelsTypeId(ТипНесъемнойПанели.SelectedValue.ToString()))
                                    }, 
                                    типТорцевой: null,
                                    типДвойной: "00",
                                    screws: null,
                                    partsToDeleteList: out partsToDeleteList,
                                    existingAsmsAndParts: out existingAsmsAndParts,
                                    onlySearch: onlySearch.IsChecked == true);

                                existingAsmsAndParts2.AddRange(existingAsmsAndParts);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }

                    if (ТипУсилПанели2.Visibility == Visibility.Visible)
                    {
                        try
                        {
                            if (!(PartsGenerationChk.IsChecked == true & StrenghtPanels.IsChecked != true))
                            {
                                var typeOfPanel = ТипУсилПанели2.SelectedItem as ComboBoxItem;

                                List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                                panelReinforcing2 = modelSw.PanelsFramelessStr(
                                    typeOfPanel: new[]
                                    {
                                        ТипУсилПанели2.SelectedValue.ToString(),
                                        ТипУсилПанели2.Text,
                                        Convert.ToString(SqlBaseData.PanelsTypeId(ТипУсилПанели2.SelectedValue.ToString()))
                                    },

                                    width: Convert.ToString(130), height: Convert.ToString(height - 40),
                                    materialP1: materialP1, materialP2: materialP2,
                                    скотч: stickyTape,// ПрименениеСкотча.Text, ПрименениеСкотча.Text,
                                    усиление: false,
                                    config: "00",
                                    расположениеПанелей: "",
                                    покрытие: покрытие,
                                    расположениеВставок: null,
                                    первыйТип: new[]
                                    {
                                        ТипНесъемнойПанели.SelectedValue.ToString(),
                                        ТипНесъемнойПанели.Text,
                                        Convert.ToString(SqlBaseData.PanelsTypeId(ТипНесъемнойПанели.SelectedValue.ToString()))
                                    },  
                                    типТорцевой: null,
                                    типДвойной: "00",
                                    screws: null,
                                    partsToDeleteList: out partsToDeleteList,
                                    existingAsmsAndParts: out existingAsmsAndParts,
                                    onlySearch: onlySearch.IsChecked == true);

                                existingAsmsAndParts2.AddRange(existingAsmsAndParts);
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.ToString());
                        }
                    }

                    #endregion

                    panelRemovable = new[]
                    {
                        panelRemovable1,
                        panelRemovable2,
                        panelRemovable3,
                        ПрименениеСкотча.Text,
                        panelReinforcing1,
                        panelReinforcing2
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());

                    Логгер.Ошибка(
                        logText: ex.Message,
                        код: ex.StackTrace,
                        функция: ex.TargetSite.ToString(),
                        className: "FramelessUnitUc");
                }
                #endregion

                try
                {
                    if (!(PartsGenerationChk.IsChecked == true & UnremPanels.IsChecked != true))
                    {
                        //"Несъемная";
                        List<ModelSw.ExistingAsmsAndParts> existingAsmsAndParts;
                        panelFixed = modelSw.PanelsFramelessStr(
                            typeOfPanel: new[]
                            {
                                ТипНесъемнойПанели.SelectedValue.ToString(),
                                ТипНесъемнойПанели.Text,
                                Convert.ToString(SqlBaseData.PanelsTypeId(ТипНесъемнойПанели.SelectedValue.ToString()))
                            },
                            width: Convert.ToString(lenght), height: Convert.ToString(height - 40),
                            materialP1: materialP1, materialP2: materialP2,
                            скотч: stickyTape,
                            усиление: PanelUnremChk.IsChecked == true,
                            config: "00",
                            расположениеПанелей: "",
                            покрытие: покрытие,
                            расположениеВставок: ProfilsConfig(),
                            первыйТип: null,
                            типТорцевой: ParmsOfUnRemPanelFlange(),
                            типДвойной: typeOfDoubleStrНесъемнаяПанель,
                            screws: new ModelSw.Screws
                            {
                                ByWidth =
                                    typeOfDoubleStrНесъемнаяПанель.Contains("1")
                                        ? ironwares.ScrewsByLenght - 1000 > 2000
                                            ? ironwares.ScrewsByLenght - 2000
                                            : ironwares.ScrewsByLenght - 1000
                                        : ironwares.ScrewsByLenght - 1000,
                                ByHeight = ironwares.ScrewsByHeight,

                                ByWidthInner = ironwares.ScrewsByLenght,
                                ByHeightInner = ironwares.ScrewsByHeight - 1000,
                                ByWidthInnerUp = ironwares.ScrewsByLenght
                            },
                            partsToDeleteList: out partsToDeleteList,
                            existingAsmsAndParts: out existingAsmsAndParts,
                            onlySearch: onlySearch.IsChecked == true);

                        existingAsmsAndParts2.AddRange(existingAsmsAndParts);

                        foreach (var partName in partsToDeleteList)
                        {
                            modelSw.PartsToDeleteList.Add(new KeyValuePair<string, string>(@panelFixed, partName));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    Логгер.Ошибка(
                        ex.Message,
                        ex.StackTrace,
                        ex.TargetSite.ToString(),
                        "FramelessUnitUc");
                }

                #endregion

                if (ТипПроф1.Visibility != Visibility.Visible){ТипПроф1.Text = "-";}
                if (ТипПроф2.Visibility != Visibility.Visible){ТипПроф2.Text = "-";}
                if (ТипПроф3.Visibility != Visibility.Visible){ТипПроф3.Text = "-";}
                if (ТипПроф4.Visibility != Visibility.Visible){ТипПроф4.Text = "-";}

                string[] profils = null;
                {
                    try
                    {
                        profils = new[]
                        {
                            ТипПроф1.Text == "-"
                                ? "-"
                                : modelSw.Profil(Convert.ToDouble(HeightU.Text) - 80, ТипПроф1.Text, ironwares.ScrewsByHeight + 1000),
                            ТипПроф2.Text == "-"
                                ? "-"
                                : modelSw.Profil(Convert.ToDouble(HeightU.Text) - 80, ТипПроф2.Text, ironwares.ScrewsByHeight + 1000),
                            ТипПроф3.Text == "-"
                                ? "-"
                                : modelSw.Profil(Convert.ToDouble(HeightU.Text) - 80, ТипПроф3.Text, ironwares.ScrewsByHeight + 1000),
                            ТипПроф4.Text == "-"
                                ? "-"
                                : modelSw.Profil(Convert.ToDouble(HeightU.Text) - 80, ТипПроф4.Text, ironwares.ScrewsByHeight + 1000)
                        };
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                    }
                }
                
                try
                {
                    if (OnlyFind)
                    {
                        PartsPdmTable.ItemsSource = existingAsmsAndParts2;
                        var asmsAndParts = new ModelSw.ExistingAsmsAndParts();
                        foreach (var existingAsmsAndPartse in existingAsmsAndParts2)
                        {
                            if (existingAsmsAndPartse == null) return;

                            existingAsmsAndPartse.PartPath = asmsAndParts.GetPath(existingAsmsAndPartse?.PartName, Settings.Default.PdmBaseName);
                            existingAsmsAndPartse.ExistInSistem = string.IsNullOrEmpty(existingAsmsAndPartse?.PartPath)
                                ? "Нет"
                                : "Есть";
                        }
                        PartsPdmTable.ItemsSource = existingAsmsAndParts2;
                        return;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                var dimensions = new ModelSw.BlockDimensions
                {
                    Width = width,
                    Height = height,
                    Lenght = lenght,
                    DimO1 = Отступ1.Visibility == Visibility.Visible ? (Convert.ToDouble(Отступ1.Text) - 46) : 0,
                    DimO2 = Отступ2.Visibility == Visibility.Visible ? Convert.ToDouble(Отступ2.Text) - 132 : 0,
                    DimO3 = Отступ3.Visibility == Visibility.Visible ? Convert.ToDouble(Отступ3.Text) - 132 : 0,
                    Planel1Width = ШиринаПанели1.Visibility == Visibility.Visible ? Convert.ToDouble(ШиринаПанели1.Text) : 0,
                    Planel2Width = ШиринаПанели2.Visibility == Visibility.Visible ? Convert.ToDouble(ШиринаПанели2.Text) : 0,
                    Planel3Width = ШиринаПанели3.Visibility == Visibility.Visible ? Convert.ToDouble(ШиринаПанели3.Text) : 0
                };

                if (PartsGenerationChk.IsChecked == true)
                {
                    modelSw.DeleteAllPartsToDelete();
                    return;
                }
                
                modelSw.FramelessBlock(
                    size: ((DataRowView) SizeOfUnit.SelectedItem)["Type"].ToString(),
                    order: OrderTextBox.Text,
                    leftSide: leftSide,
                    section: SectionTextBox.Text,
                    pDown: panelDown,
                    pFixed: panelFixed,
                    pUp: panelUp,
                    съемныеПанели: panelRemovable,
                    промежуточныеСтойки: profils,
                    dimensions: dimensions, 
                    троцевыеПанели: panelBack);                

                modelSw.DeleteAllPartsToDelete();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

                Логгер.Ошибка(logText: ex.Message,
                    код: ex.StackTrace,
                    функция: ex.TargetSite.ToString(),
                    className: "FramelessUnitUc");
            }

            Логгер.Информация($"Построение бескаркасной уставновки завершено - {""}",
                "", "", "FramelessUnitUc");
        }

        void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void EnterKeyDown(object sender, KeyEventArgs e)
        {
            StartBuildingBlock(e);
        }
       
        void StartBuildingBlock(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BUILDING_Click(this, new RoutedEventArgs());
            }
        }
      
        #endregion

        #region Построить панель

        void ПостроитьПанель_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //var добавитьПанель = _sqlBaseData.AirVents_AddPanel(
                //    ТипСъемнойПанели1.SelectedValue.ToString(),
                //    Convert.ToInt32(WidthU.Text),
                //    Convert.ToInt32(HeightU.Text),
                //    40,
                //    Convert.ToInt32(MaterialP1.SelectedValue),
                //    Convert.ToInt32(MaterialP2.SelectedValue),
                //    Convert.ToDouble(ТолщинаВнешней.Text.Replace('.',',')),
                //    Convert.ToDouble(ТолщинаВннутренней.Text.Replace('.', ',')),
                //    Convert.ToInt32(Шумоизоляция.SelectedValue),
                //    ПрименениеСкотча.Text == "Со скотчем",
                //    УсилениеСъемнойПанели1.IsChecked == true,
                //    Ral1.Text,
                //    CoatingType1.Text,
                //    Convert.ToInt32(CoatingClass1.Text));

                //modelSw.PanelsFramelessStr(
                //        "01",
                //        Convert.ToString(lenght),
                //        Convert.ToString(lenght - 40),
                //        materialP1,
                //        materialP2,
                //        скотч,
                //        "00" + strengthUnrem,
                //        "",
                //        new[] {Ral1.Text, CoatingType1.Text, CoatingClass1.Text});

              //  MessageBox.Show(добавитьПанель.ToString());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            
            try
            {
                //var modelSw = new ModelSw();

                //string Config;

                //switch (ТипПанели.Text)
                //{
                //    case "01":
                //    case "04":
                //    case "05":
                //        Config = "00";
                //        break;
                //    default:
                //        Config = "01";
                //        break;
                //}

                //var strengthUp = УсилениеПанели.IsChecked == true;

                //modelSw.PanelsFramelessStr(
                //    ТипПанели.Text,
                //    ШиринаПанели.Text,
                //    ВысотаПанели.Text,
                //    new[] { МатериалВнешней.Text, ТолщинаВнешней.Text },
                //    new[] { МатериалВнутренней.Text, ТолщинаВннутренней.Text },
                //    TypeOfPanelF1.Text == "Со скотчем" ? true : false,
                //    strengthUp,
                //    Config,
                //    PanelsConfig(),
                //    new[] { Ral1.Text, CoatingType1.Text, CoatingClass1.Text });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        void ПотроитьПанель(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ПостроитьПанель_Click(this, new RoutedEventArgs());
            }
        }

        void ШиринаПанели_KeyDown(object sender, KeyEventArgs e)
        {
            ПотроитьПанель(e);
        }

        void ВысотаПанели_KeyDown(object sender, KeyEventArgs e)
        {
            ПотроитьПанель(e);
        }

        #endregion

        #region Строки для передачи параметров в метод построения панелей (Съемные панели и конфигурации)

        string PanelsConfig()
        {
            return
                $"{(Отступ1.Visibility == Visibility.Visible ? Отступ1.Text : "")}{(ШиринаПанели1.Visibility == Visibility.Visible ? ";" + ШиринаПанели1.Text : "")}{(Отступ2.Visibility == Visibility.Visible ? ";" + Отступ2.Text : "")}{(ШиринаПанели2.Visibility == Visibility.Visible ? ";" + ШиринаПанели2.Text : "")}{(Отступ3.Visibility == Visibility.Visible ? ";" + Отступ3.Text : "")}{(ШиринаПанели3.Visibility == Visibility.Visible ? ";" + ШиринаПанели3.Text : "")}";
        }


        public class ProfilsConfiguration
        {
            public Profil Profil1 { get; set; }
            public Profil Profil2 { get; set; }
            public Profil Profil3 { get; set; }
            public Profil Profil4 { get; set; }

            ProfilsConfiguration(Profil profil1, Profil profil2, Profil profil3, Profil profil4)
            {
                Profil1 = profil1;
                Profil2 = profil2;
                Profil3 = profil3;
                Profil4 = profil4;
            }

            public enum TypeOfProf
            {                
                p00,
                p01,
                p02,
                p05
            }

            public class Profil
            {
                TypeOfProf type { get; set; }

                double width { get; set; }
            }               

        }

        string ProfilsConfig()
        {
            #region To delete

            //MessageBox.Show($" ТипПроф1- {ТипПроф1.Text}\nТипПроф2- {ТипПроф2.Text}\nТипПроф3- {ТипПроф3.Text}\nТипПроф4 - {ТипПроф4.Text}"\nШиринаПрофиля1- {ШиринаПрофиля1.Text}\nШиринаПрофиля2- {ШиринаПрофиля2.Text}\nШиринаПрофиля3- {ШиринаПрофиля3.Text}\nШиринаПрофиля4- {ШиринаПрофиля4.Text}\n");

            //if (ТипПроф1.Text == "-" & ТипПроф2.Text == "-" &
            //    ТипПроф3.Text == "-" & ТипПроф4.Text == "-" 
            //    &
            //    ТипУсилПанели1.Visibility != Visibility.Visible &
            //    ТипУсилПанели2.Visibility != Visibility.Visible)
            //{
            //    return null;
            //}

            #endregion

            return $"{ТипПроф1.Text + "_" + ШиринаПрофиля1.Text};{ТипПроф2.Text + "_" + ШиринаПрофиля2.Text};{ТипПроф3.Text + "_" + ШиринаПрофиля3.Text};{ТипПроф4.Text + "_" + ШиринаПрофиля4.Text}";
        }

        string ParmsOfBackPanel1()
        {
            return ModelSw.BackPanelConfig(
                Convert.ToDouble(string.IsNullOrEmpty(HeightF.Text) ? "9" : HeightF.Text),
                Convert.ToDouble(string.IsNullOrEmpty(WidthF.Text) ? "9" : WidthF.Text),

                Convert.ToDouble(string.IsNullOrEmpty(byHeight.Text)
                    ? "0" : СмещениеПоВертикали.SelectedIndex == 1 ? "-" + byHeight.Text : byHeight.Text),
                Convert.ToDouble(string.IsNullOrEmpty(byWidth.Text)
                    ? "0" : СмещениеПоГоризонтали.SelectedIndex == 1 ? byWidth.Text : "-" + byWidth.Text),

                Фланец.Text == "20" ? null : "true",
                1);
        }              

        string ParmsOfBackPanel2()
        {
            return ModelSw.BackPanelConfig(
                Convert.ToDouble(string.IsNullOrEmpty(HeightF2.Text) ? "9" : HeightF2.Text),
                Convert.ToDouble(string.IsNullOrEmpty(WidthF2.Text) ? "9" : WidthF2.Text),

                Convert.ToDouble(string.IsNullOrEmpty(byHeight3.Text) ? "0" : СмещениеПоВертикали4.SelectedIndex == 1 ? byHeight3.Text : "-" + byHeight3.Text),
                Convert.ToDouble(string.IsNullOrEmpty(byWidth1.Text) ? "0" : СмещениеПоГоризонтали2.SelectedIndex == 1 ? byWidth1.Text : "-" + byWidth1.Text),

                Фланец2.Text == "20" ? null : "true",
                1);
        }

        string ParmsOfRoofPanelFlange()
        {
            return ModelSw.BackPanelConfig(
                Convert.ToDouble(string.IsNullOrEmpty(HeightF1.Text) ? "9" : HeightF1.Text),
                Convert.ToDouble(string.IsNullOrEmpty(WidthF1.Text) ? "9" : WidthF1.Text),

                Convert.ToDouble(string.IsNullOrEmpty(byHeight1.Text)
                    ? "0"
                    : СмещениеПоВертикали8.SelectedIndex == 1 ? "-" + byHeight1.Text : byHeight1.Text),
                Convert.ToDouble(string.IsNullOrEmpty(byWidth2.Text)
                    ? "0"
                    : СмещениеПоГоризонтали1.SelectedIndex == 1 ? byWidth2.Text : "-" + byWidth2.Text),

                Фланец1.Text == "20" ? null : "true",
                3);
        }

        string ParmsOfMontagePanelFlange()
        {
            return ModelSw.BackPanelConfig(
                Convert.ToDouble(string.IsNullOrEmpty(HeightF3.Text) ? "9" : HeightF3.Text),
                Convert.ToDouble(string.IsNullOrEmpty(WidthF3.Text) ? "9" : WidthF3.Text),

                Convert.ToDouble(string.IsNullOrEmpty(byHeight2.Text) ? "0" : СмещениеПоВертикали12.SelectedIndex == 1 ? "-" + byHeight2.Text : byHeight2.Text),
                Convert.ToDouble(string.IsNullOrEmpty(byWidth3.Text) ? "0" : СмещениеПоГоризонтали3.SelectedIndex == 1 ? "-" + byWidth3.Text : byWidth3.Text),

                Фланец3.Text == "20" ? null : "true",

                4);
        }

        string ParmsOfUnRemPanelFlange()
        {
            var signForСмещениеПоГоризонтали4 = Левая.IsChecked == true ? "-" : "";
            var signForСмещениеПоГоризонтали41 = Правая.IsChecked == true ? "-" : "";

            return ModelSw.BackPanelConfig(
                Convert.ToDouble(string.IsNullOrEmpty(WidthF4.Text) ? "9" : WidthF4.Text),
                Convert.ToDouble(string.IsNullOrEmpty(HeightF4.Text) ? "9" : HeightF4.Text),

                Convert.ToDouble(string.IsNullOrEmpty(byHeight4.Text) ? "0" : СмещениеПоВертикали16.SelectedIndex == 1 ? signForСмещениеПоГоризонтали4 + byHeight4.Text : signForСмещениеПоГоризонтали41 + byHeight4.Text),

                Convert.ToDouble(string.IsNullOrEmpty(byWidth4.Text) ? "0" : СмещениеПоГоризонтали4.SelectedIndex == 1 ? "-" + byWidth4.Text : byWidth4.Text),

                Фланец4.Text == "20" ? null : "true",
                
                5);
        }        

        void ПостроитьПанель1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(PanelsConfig());
        }

        #endregion
        
        void КолвоПанелейS(object sender, SelectionChangedEventArgs e)
        {
            КолВоПанелей();
        }

        void КолвоПанелей_LayoutUpdated(object sender, EventArgs e)
        {
            КолВоПанелей();
        }

        void КолВоПанелей()
        {
            if (Ширина2 == null || Ширина3 == null || ШиринаПанели2 == null || ШиринаПанели3 == null ||
                Отступ2L == null || Отступ3L == null || Отступ2 == null || Отступ3 == null) return;

            App.ElementVisibility.ForUiElementsList(
                       new List<UIElement>
                       {
                            Ширина2, ШиринаПанели2, Отступ2L, Отступ2,
                            Ширина3, ШиринаПанели3, Отступ3L, Отступ3
                       }, Visibility.Collapsed);

            ШиринаПанели1.IsEnabled = true;
            ШиринаПанели2.IsEnabled = true;
            ШиринаПанели3.IsEnabled = true;

            var item = КолвоПанелей.SelectedValue as ComboBoxItem;

            if (item == null) return;
            switch (item.Content.ToString())
            {
                case "1":
                    ШиринаПанели1.IsEnabled = false;
                    ШиринаПанели3.Text = "";
                    ШиринаПанели2.Text = "";
                    break;
                case "2":
                    ШиринаПанели2.IsEnabled = false;
                    ШиринаПанели3.Text = "";
                    App.ElementVisibility.ForUiElementsList(
                       new List<UIElement>
                       {
                            Ширина2, ШиринаПанели2, Отступ2L, Отступ2
                       }, Visibility.Visible);
                    break;
                case "3":
                    ШиринаПанели3.IsEnabled = false;
                    App.ElementVisibility.ForUiElementsList(
                        new List<UIElement>
                        {
                            Ширина2, ШиринаПанели2, Отступ2L, Отступ2,
                            Ширина3, ШиринаПанели3, Отступ3L, Отступ3
                        }, Visibility.Visible);
                    break;
                default:
                    App.ElementVisibility.ForUiElementsList(
                        new List<UIElement>
                        {
                            Ширина3, ШиринаПанели3, Отступ3L, Отступ3
                        }, Visibility.Visible);
                    break;
            }
        }

        void RiznPanel(TextBox общаяДлина, TextBox панель1, TextBox панель2, TextBox панель3,
             TextBox профиль1, TextBox профиль2, TextBox профиль3, TextBox профиль4)
        {            
            панель1.SelectionChanged += ШиринаПанели1_SelectionChanged;
            панель2.SelectionChanged += ШиринаПанели2_SelectionChanged;
            панель3.SelectionChanged += ШиринаПанели3_SelectionChanged;
            
            var посадочнаяШиринаПанели = общаяДлина.Text != "" ? Convert.ToInt32(общаяДлина.Text) : 0;
            var посадочнаяШиринаПанели1 = панель1.Text != "" ? Convert.ToInt32(панель1.Text) : 0;
            var посадочнаяШиринаПанели2 = панель2.Text != "" ? Convert.ToInt32(панель2.Text) : 0;
          
            #region Профили

            int prof1;
            int.TryParse(профиль1.Text, out prof1);
            int prof2;
            int.TryParse(профиль2.Text, out prof2);
            int prof3;
            int.TryParse(профиль3.Text, out prof3);
            int prof4;
            int.TryParse(профиль4.Text, out prof4);

            var sumOfProfilsLenght = prof1 + prof2 + prof3 + prof4;
                
            #endregion

            switch (КолвоПанелей.Text)
            {
                case "1":
                    панель1.Text = Convert.ToString(посадочнаяШиринаПанели - sumOfProfilsLenght);
                    панель1.SelectionChanged -= ШиринаПанели1_SelectionChanged;
                    break;
                case "2":
                    панель2.Text = Convert.ToString(посадочнаяШиринаПанели - посадочнаяШиринаПанели1 - sumOfProfilsLenght);
                    панель2.SelectionChanged -= ШиринаПанели2_SelectionChanged;
                    break;
                case "3":
                    панель3.Text = Convert.ToString(посадочнаяШиринаПанели - посадочнаяШиринаПанели1 - посадочнаяШиринаПанели2 - sumOfProfilsLenght);
                    панель3.SelectionChanged -= ШиринаПанели3_SelectionChanged;
                    break;
            }
        }

        void Sum()
        {
            RiznPanel(ШиринаПанели, ШиринаПанели1, ШиринаПанели2, ШиринаПанели3, ШиринаПрофиля1, ШиринаПрофиля2, ШиринаПрофиля3, ШиринаПрофиля4);
        }
        
        void ШиринаПанели_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Sum();
        }

        void ШиринаПанели1_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ШиринаСъемнойПанели1.Text = ШиринаПанели1.Text;
            Sum();
        }

        void ШиринаПанели2_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ШиринаСъемнойПанели2.Text = ШиринаПанели2.Text;
            if (!ШиринаПанели2.IsEnabled) return;
            Sum();
        }

        void ШиринаПанели3_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ШиринаСъемнойПанели3.Text = ШиринаПанели3.Text;
            if (!ШиринаПанели3.IsEnabled) return;
            Sum();
        }

        void ШиринаПрофиля1_TextChanged(object sender, TextChangedEventArgs e)
        {
            Sum();
            int width;
            int.TryParse(ШиринаПрофиля1.Text, out width);
            Отступ1.Text = Convert.ToString(46 + width);
        }

        void ШиринаПрофиля2_TextChanged(object sender, TextChangedEventArgs e)
        {
            Sum();
            int width;
            int.TryParse(ШиринаПрофиля2.Text, out width);
            Отступ2.Text = Convert.ToString(132 + width);
        }

        void ШиринаПрофиля3_TextChanged(object sender, TextChangedEventArgs e)
        {
            Sum();
            int width;
            int.TryParse(ШиринаПрофиля3.Text, out width);
            Отступ3.Text = Convert.ToString(132 + width);
        }

        void ШиринаПрофиля4_TextChanged(object sender, TextChangedEventArgs e)
        {
            Sum();
        }

        void RemovablePanels_Initialized(object sender, EventArgs e)
        {
            ШиринаПанели1.IsEnabled = false;
        }

        #region Картинка установки

        void ТипУстановки_LayoutUpdated(object sender, EventArgs e)
        {
            const string picturePath = @"\DataControls\Pictures\Бескаркасная установка\";

            var item = (ComboBoxItem) ТипУстановки?.SelectedItem;
            if (item == null) return;
            var pictureName = "01 - Frameless Design 40mm - " + item.ToolTip + ".jpg";
            
            var mapLoader = new BitmapImage();
            mapLoader.BeginInit();
            mapLoader.UriSource = new Uri(picturePath + pictureName, UriKind.RelativeOrAbsolute);

            mapLoader.EndInit();
            if (PicturePanel != null) PicturePanel.Source = mapLoader;
        }

        void Левая_Checked(object sender, RoutedEventArgs e)
        {
            PicturePanel.RenderTransformOrigin = new Point(0.5, 0.5);
            var flipTrans = new ScaleTransform {ScaleX = 1};
            //  flipTrans.ScaleY = -1;
            PicturePanel.RenderTransform = flipTrans;
        }

      

        // Перенос текстбоксов с размерами на рисунок
        void Grid_Loaded_2(object sender, RoutedEventArgs e)
        {
            // Длина
            if (Lenght.Parent != null)
            {
                var parent = (Panel)Lenght.Parent;
                parent.Children.Remove(Lenght);
            }
            MyPanel.Children.Add(Lenght);
            Grid.SetRow(Lenght, 8);

            // Панель 1
            if (ШиринаСъемнойПанели1.Parent != null)
            {
                var parent = (Panel)ШиринаСъемнойПанели1.Parent;
                parent.Children.Remove(ШиринаСъемнойПанели1);
            }
            MyPanel.Children.Add(ШиринаСъемнойПанели1);
            Grid.SetRow(ШиринаСъемнойПанели1, 0);


            // Панель 2
            if (ШиринаСъемнойПанели2.Parent != null)
            {
                var parent = (Panel)ШиринаСъемнойПанели2.Parent;
                parent.Children.Remove(ШиринаСъемнойПанели2);
            }
            MyPanel.Children.Add(ШиринаСъемнойПанели2);
            Grid.SetRow(ШиринаСъемнойПанели2, 0);

            // Панель 3
            if (ШиринаСъемнойПанели3.Parent != null)
            {
                var parent = (Panel)ШиринаСъемнойПанели3.Parent;
                parent.Children.Remove(ШиринаСъемнойПанели3);
            }
            MyPanel.Children.Add(ШиринаСъемнойПанели3);
            Grid.SetRow(ШиринаСъемнойПанели3, 0);

            // Профиль 2
            if (ТипПроф2.Parent != null)
            {
                var parent = (Panel)ТипПроф2.Parent;
                parent.Children.Remove(ТипПроф2);
            }
            MyPanel.Children.Add(ТипПроф2);
            Grid.SetRow(ТипПроф2, 0);

            // Профиль 3
            if (ТипПроф3.Parent != null)
            {
                var parent = (Panel)ТипПроф3.Parent;
                parent.Children.Remove(ТипПроф3);
            }
            MyPanel.Children.Add(ТипПроф3);
            Grid.SetRow(ТипПроф3, 0);

            // Тип Усил Панели1 
            if (ТипУсилПанели1.Parent != null)
            {
                var parent = (Panel)ТипУсилПанели1.Parent;
                parent.Children.Remove(ТипУсилПанели1);
            }
            MyPanel.Children.Add(ТипУсилПанели1);
            Grid.SetRow(ТипУсилПанели1, 5);

            // Профиль 1
            if (ТипПроф1.Parent != null)
            {
                var parent = (Panel)ТипПроф1.Parent;
                parent.Children.Remove(ТипПроф1);
            }
            MyPanel.Children.Add(ТипПроф1);
            Grid.SetRow(ТипПроф1, 0);
            
            // Тип Усил Панели2 
            if (ТипУсилПанели2.Parent != null)
            {
                var parent = (Panel)ТипУсилПанели2.Parent;
                parent.Children.Remove(ТипУсилПанели2);
            }
            MyPanel.Children.Add(ТипУсилПанели2);
            Grid.SetRow(ТипУсилПанели2, 5);

            // Профиль 4
            if (ТипПроф4.Parent != null)
            {
                var parent = (Panel)ТипПроф4.Parent;
                parent.Children.Remove(ТипПроф4);
            }
            MyPanel.Children.Add(ТипПроф4);
            Grid.SetRow(ТипПроф4, 0);

            ТипПроф4.Visibility = Visibility.Collapsed;
            ТипПроф1.Visibility = Visibility.Collapsed;

        }

        #endregion

        void КришаТип_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ТипВерхнейПанели.Text = КришаТип.SelectedValue.ToString();
        }

        void ОпорнаяЧастьТип_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ТипПанелиНижней.Text = ОпорнаяЧастьТип.SelectedValue.ToString();
        }

        void ШиринаСъемнойПанели1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (ТипУстановки == null) return;
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void ШиринаСъемнойПанели2_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        void Lenght_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (ТипУстановки == null) return;
            
            var item = (ComboBoxItem)ТипУстановки.SelectedValue;
            
            if (item.ToolTip.ToString() == "01")
            {
                ШиринаПанели.Text = Lenght.Text;
                ШиринаСъемнойПанели1.Text = Lenght.Text;
                ШиринаПанели1.Text = Lenght.Text;
            }
            else
            {
                ШиринаПанели.Text = Lenght.Text;
            }
        }

        void ТипСъемнойПанели1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ТипСъемнойПанели1.ToolTip = ТипСъемнойПанели1.SelectedValue;
        }

        void ТипУстановки_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ТипУстановки == null) return;
            var item = (ComboBoxItem)ТипУстановки.SelectedItem;

            ШиринаСъемнойПанели1.IsEnabled = false;
            ШиринаСъемнойПанели2.IsEnabled = false;
            ШиринаСъемнойПанели3.IsEnabled = false;

            switch (item.ToolTip.ToString())
            {
                case "01":
                    КолвоПанелей.Text = "1";
                    ШиринаСъемнойПанели1.IsEnabled = false;
                    ШиринаСъемнойПанели1.Text = Lenght.Text;

                    Grid.SetColumn(Lenght, 5);
                    Grid.SetColumn(ШиринаСъемнойПанели1, 5);
                    ШиринаСъемнойПанели1.Visibility = Visibility.Visible;
                    ШиринаСъемнойПанели2.Visibility = Visibility.Collapsed;
                    ШиринаСъемнойПанели3.Visibility = Visibility.Collapsed;

                    ТипПроф2.SelectedIndex = 0;
                    ТипПроф3.SelectedIndex = 0;
                    
                    ТипПроф2.Visibility = Visibility.Collapsed;
                    ТипПроф3.Visibility = Visibility.Collapsed;

                    break;

                case "02":
                    КолвоПанелей.Text = "2";
                    ШиринаСъемнойПанели1.IsEnabled = true;
                    Grid.SetColumn(Lenght, 6);
                    Grid.SetColumn(ШиринаСъемнойПанели1, 4);
                    
                    Grid.SetColumn(ТипПроф2, 5);

                    Grid.SetColumn(ШиринаСъемнойПанели2, 7);
                    Grid.SetColumn(ШиринаСъемнойПанели3, 9);
                    ШиринаСъемнойПанели1.Visibility = Visibility.Visible;
                    ШиринаСъемнойПанели2.Visibility = Visibility.Visible;
                    ШиринаСъемнойПанели3.Visibility = Visibility.Collapsed;
                    
                    ТипПроф3.SelectedIndex = 0;
                    ТипПроф2.Visibility = Visibility.Visible;
                    ТипПроф3.Visibility = Visibility.Collapsed;

                    break;

                case "03":
                    КолвоПанелей.Text = "3";
                    ШиринаСъемнойПанели1.IsEnabled = true;
                    ШиринаСъемнойПанели2.IsEnabled = true;
                    Grid.SetColumn(Lenght, 5);
                    Grid.SetColumn(ШиринаСъемнойПанели1, 3);
                    Grid.SetColumn(ТипПроф2, 4);
                    Grid.SetColumn(ШиринаСъемнойПанели2, 5);
                    Grid.SetColumn(ТипПроф3, 7);
                    Grid.SetColumn(ШиринаСъемнойПанели3, 8);
                    ШиринаСъемнойПанели1.Visibility = Visibility.Visible;
                    ШиринаСъемнойПанели2.Visibility = Visibility.Visible;
                    ШиринаСъемнойПанели3.Visibility = Visibility.Visible;
                    
                    ТипПроф2.Visibility = Visibility.Visible;
                    ТипПроф3.Visibility = Visibility.Visible;

                    break;
            }
        }

        void ТипУстановкиПромежуточныеВставки_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ТипУстановкиПромежуточныеВставки == null) return;
            var item = (ComboBoxItem)ТипУстановкиПромежуточныеВставки.SelectedItem;

            switch (item.ToolTip.ToString())
            {
                case "0":

                    ТипУсилПанели1.Visibility = Visibility.Collapsed;
                    ТипУсилПанели2.Visibility = Visibility.Collapsed;

                    ТипУсилПанели1.Text = "-";
                    ТипУсилПанели2.Text = "-";

                    break;

                case "1":
                    
                    ТипУсилПанели1.Visibility = Visibility.Visible;
                    ТипУсилПанели2.Visibility = Visibility.Visible;
                    ТипУсилПанели1.SelectedIndex = 0;
                    ТипУсилПанели2.SelectedIndex = 0;

                    Grid.SetColumn(ТипУсилПанели1, 2);
                    Grid.SetColumn(ТипУсилПанели2, 9);

                    break;

                case "2":
                    
                    ТипУсилПанели1.Visibility = Visibility.Visible;
                    ТипУсилПанели2.Visibility = Visibility.Collapsed;
                    ТипУсилПанели1.SelectedIndex = 0;
                    ТипУсилПанели2.Text = "-";

                    Grid.SetColumn(ТипУсилПанели1, 2);
                    Grid.SetColumn(ТипУсилПанели2, 9);

                    break;

                case "3":

                    ТипУсилПанели1.Visibility = Visibility.Collapsed;
                    ТипУсилПанели2.Visibility = Visibility.Visible;
                    ТипУсилПанели1.Text = "-";
                    ТипУсилПанели2.SelectedIndex = 0;

                    Grid.SetColumn(ТипУсилПанели1, 2);
                    Grid.SetColumn(ТипУсилПанели2, 9);

                    break;
            }

            ШиринаПрофиля1.Text = ТипУсилПанели1.Visibility == Visibility.Visible ? "130" : "";
            ШиринаПрофиля4.Text = ТипУсилПанели2.Visibility == Visibility.Visible ? "130" : "";
        }

        void ШиринаСъемнойПанели1_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ШиринаПанели1.Text = ШиринаСъемнойПанели1.Text;
        }

        void ШиринаСъемнойПанели2_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ШиринаПанели2.Text = ШиринаСъемнойПанели2.Text;
            ШиринаСъемнойПанели2.Foreground = ШиринаСъемнойПанели2.Text.Contains("-") ? Brushes.Red : Brushes.Black;
        }

        void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            if (!Settings.Default.Developer)
            {
                _isDeveloper = false;
            }
            if (_isDeveloper)
            {
                App.ElementVisibility.ForUiElementsList(
                    new List<UIElement>
                    {
                        НесъемныеПанели, ПанельБескаркаснойУстановки, Parts, УсилениеСъемнойПанели1Chk,
                        УсилениеСъемнойПанели2Chk, УсилениеСъемнойПанели2Chk, ПанелиУстановки
                    }, Visibility.Collapsed);
            }
            else
            {
                App.ElementVisibility.ForUiElementsList(
                        new List<UIElement>
                        {
                            НесъемныеПанели, ПанельБескаркаснойУстановки, Parts, УсилениеСъемнойПанели1Chk,
                            УсилениеСъемнойПанели2Chk, УсилениеСъемнойПанели2Chk
                        }, Visibility.Collapsed);
            }
        }
        
        void Усиления_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)Усиления.SelectedItem;
            if (item == null) return;

            УсилениеСъемнойПанели1.IsChecked = false;
            УсилениеСъемнойПанели2.IsChecked = false;
            УсилениеСъемнойПанели3.IsChecked = false;
            PanelUpChk.IsChecked = false;
            PanelUnremChk.IsChecked = false;
            PanelDownChk.IsChecked = false;
            _panelBack = false;

            switch (item.Content.ToString())
            {
                case "":
                    break;
                case "Н":
                    PanelDownChk.IsChecked = true;
                    break;
                case "НВ":
                    PanelDownChk.IsChecked = true;
                    PanelUpChk.IsChecked = true;
                    break;
                case "НЗ":
                    PanelDownChk.IsChecked = true;
                    PanelUnremChk.IsChecked = true;
                    break;
                case "НВЗ":
                    PanelDownChk.IsChecked = true;
                    PanelUnremChk.IsChecked = true;
                    PanelUpChk.IsChecked = true;
                    break;
                case "НВЗС":
                    УсилениеСъемнойПанели1.IsChecked = true;
                    УсилениеСъемнойПанели2.IsChecked = true;
                    УсилениеСъемнойПанели3.IsChecked = true;
                    PanelDownChk.IsChecked = true;
                    PanelUnremChk.IsChecked = true;
                    PanelUpChk.IsChecked = true;
                    break;
                case "НВЗСТ":
                    УсилениеСъемнойПанели1.IsChecked = true;
                    УсилениеСъемнойПанели2.IsChecked = true;
                    УсилениеСъемнойПанели3.IsChecked = true;
                    PanelDownChk.IsChecked = true;
                    PanelUnremChk.IsChecked = true;
                    PanelUpChk.IsChecked = true;
                    _panelBack = true;
                    break;
                case "Т":
                    _panelBack = true;
                    break;
            }
        }

        private bool _panelBack;

        void ШиринаСъемнойПанели3_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ШиринаСъемнойПанели3.Foreground = ШиринаСъемнойПанели3.Text.Contains("-") ? Brushes.Red : Brushes.Black;
        }

        #region Покраска

        void Ral1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Ral1.ToolTip = Ral1.SelectedValue;
        }

        void Ral2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Ral2.ToolTip = Ral2.SelectedValue;
        }

        void Ral1_LayoutUpdated(object sender, EventArgs e)
        {
            if (Ral1.Text == "Без покрытия")
            {
                CoatingType1.Visibility = Visibility.Collapsed;
                CoatingClass1.Visibility = Visibility.Collapsed;
            }
            else
            {
                CoatingType1.Visibility = Visibility.Visible;
                CoatingClass1.Visibility = Visibility.Visible;
            }
        }

        void Ral2_LayoutUpdated(object sender, EventArgs e)
        {

            if (Ral2.Text == "Без покрытия")
            {
                CoatingType2.Visibility = Visibility.Collapsed;
                CoatingClass2.Visibility = Visibility.Collapsed;
            }
            else
            {
                CoatingType2.Visibility = Visibility.Visible;
                CoatingClass2.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Материал

        void MaterialP1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialP1.SelectedIndex == 0)
            {
                ТолщинаВнешнейLbl.Visibility = Visibility.Hidden;
                ТолщинаВнешней.Visibility = Visibility.Hidden;
            }
            else
            {
                ТолщинаВнешнейLbl.Visibility = Visibility.Visible;
                ТолщинаВнешней.Visibility = Visibility.Visible;
            }
        }

        void MaterialP2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MaterialP2.SelectedIndex == 0)
            {
                ТолщинаВннутреннейLbl.Visibility = Visibility.Hidden;
                ТолщинаВннутренней.Visibility = Visibility.Hidden;
            }
            else
            {
                ТолщинаВннутреннейLbl.Visibility = Visibility.Visible;
                ТолщинаВннутренней.Visibility = Visibility.Visible;
            }
        }

        #endregion
        
        void Button_Click(object sender, RoutedEventArgs e)
        {
            var modelSw = new ModelSw();
            modelSw.Profil(Convert.ToDouble(HeightU.Text) - 40, "03", null);
        }

        void ТипУсилПанели1_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ТипУсилПанели1 == null) return;

            if (ТипУсилПанели1.Visibility == Visibility.Visible)
            {
                ТипПроф1.Visibility = Visibility.Visible;
                Grid.SetColumn(ТипПроф1, 2);
            }
            else
            {
                ТипПроф1.Visibility = Visibility.Collapsed;
            }
        }

        void ТипУсилПанели2_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ТипУсилПанели2 == null) return;

            if (ТипУсилПанели2.Visibility == Visibility.Visible)
            {
                ТипПроф4.Visibility = Visibility.Visible;
                Grid.SetColumn(ТипПроф4, 9);
            }
            else
            {
                ТипПроф4.Visibility = Visibility.Collapsed;
            }
        }

        void НаличиеТорцевойПанели_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (НаличиеТорцевойПанели == null) return;

            switch (НаличиеТорцевойПанели.SelectedIndex)
            {
                case 0:
                    Back.Visibility =  Visibility.Collapsed;
                    Back2.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    Back.Visibility = Visibility.Visible;
                    Back2.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    Back.Visibility = Visibility.Collapsed;
                    Back2.Visibility = Visibility.Visible;
                    break;
                case 3:
                    Back.Visibility = Visibility.Visible;
                    Back2.Visibility = Visibility.Visible;
                    break;
                default:
                    Back.Visibility = Visibility.Visible;
                    Back2.Visibility = Visibility.Visible;
                    break;
            }
        }

        void СмещениеПоВертикали_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоВертикали == null) return;
            byHeight.Visibility = СмещениеПоВертикали.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void СмещениеПоГоризонтали_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоГоризонтали == null) return;
            byWidth.Visibility = СмещениеПоГоризонтали.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void СмещениеПоГоризонтали1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоГоризонтали2 == null) return;
            byWidth1.Visibility = СмещениеПоГоризонтали2.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void СмещениеПоВертикали4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоВертикали4 == null) return;
            byHeight3.Visibility = СмещениеПоВертикали4.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void КришаТипПанели_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (КришаТипПанели == null) return;
            RoofFlangeType.Visibility = КришаТипПанели.SelectedIndex == 0 || КришаТипПанели.SelectedIndex == 1 ? Visibility.Collapsed : Visibility.Visible;
            КришаТипСдвоенойПанели.Visibility = КришаТипПанели.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        void ОпорнаяЧастьТипПанели_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ОпорнаяЧастьТипПанели == null) return;
            MontageFlangeTypeGrid.Visibility = ОпорнаяЧастьТипПанели.SelectedIndex == 0 || ОпорнаяЧастьТипПанели.SelectedIndex == 1 ? Visibility.Collapsed : Visibility.Visible;
            ОпорнаяЧастьТипСдвоенойПанели.Visibility = ОпорнаяЧастьТипПанели.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        void СмещениеПоГоризонтали1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоГоризонтали1 == null) return;
            byWidth2.Visibility = СмещениеПоГоризонтали1.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void СмещениеПоВертикали8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоВертикали8 == null) return;
            byHeight1.Visibility = СмещениеПоВертикали8.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void СмещениеПоГоризонтали3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоГоризонтали3 == null) return;
            byWidth3.Visibility = СмещениеПоГоризонтали3.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void СмещениеПоВертикали12_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоВертикали12 == null) return;
            byHeight2.Visibility = СмещениеПоВертикали12.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void НесъемнаяПанельТипПанели1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (НесъемнаяПанельТипПанели1 == null) return;
            НесъемнаяПанельGrid.Visibility = НесъемнаяПанельТипПанели1.SelectedIndex == 0 || НесъемнаяПанельТипПанели1.SelectedIndex == 1 ? Visibility.Collapsed : Visibility.Visible;
            НесъемнаяПанельСдвоенойПанели.Visibility = НесъемнаяПанельТипПанели1.SelectedIndex != 1? Visibility.Collapsed : Visibility.Visible;
        }

        void СмещениеПоГоризонтали4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоГоризонтали4 == null) return;
            byWidth4.Visibility = СмещениеПоГоризонтали4.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void СмещениеПоВертикали16_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (СмещениеПоВертикали16 == null) return;
            byHeight4.Visibility = СмещениеПоВертикали16.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        void RoofFlangeType_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (RoofFlangeType.IsVisible) return;
            WidthF1.Clear();
            HeightF1.Clear();
        }

        void MontageFlangeTypeGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MontageFlangeTypeGrid.IsVisible) return;
            WidthF3.Clear();
            HeightF3.Clear();
        }

        void НесъемнаяПанельGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (НесъемнаяПанельGrid.IsVisible) return;
            WidthF4.Clear();
            HeightF4.Clear();
        }

        void ОпорнаяЧастьТипСдвоенойПанели_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ОпорнаяЧастьТипСдвоенойПанели == null) return;
                var item = ОпорнаяЧастьТипСдвоенойПанели.SelectedItem as ComboBoxItem;
        }

        void ТипПанелиПоиск_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ТипПанелиПоиск?.SelectedItem as ComboBoxItem;

            if (item == null) return;

            try
            {
                ПодтипПанелиПоиска.ItemsSource =
                ((IListSource)_sqlBaseData.PanelsTable(item.Name.Replace("n", ""))).GetList();
                ПодтипПанелиПоиска.DisplayMemberPath = "PanelTypeName";
                ПодтипПанелиПоиска.SelectedValuePath = "PanelTypeCode";
                ПодтипПанелиПоиска.SelectedIndex = 0;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
                ПодтипПанелиПоиска.ItemsSource = null;
            }
        }

        void ПоискПанели_Click(object sender, RoutedEventArgs e)
        {
            
        }

        void OpenFile(object sender, RoutedEventArgs e)
        {
            var item = PartsPdmTable.SelectedItem as ModelSw.ExistingAsmsAndParts;
            try
            {
                if (string.IsNullOrEmpty(item?.PartPath)) return;

                VaultSystem.GetLastVersionOfFile(item.PartPath);
                Process.Start(@item.PartPath);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        void GoToFile(object sender, RoutedEventArgs e)
        {
            var item = PartsPdmTable.SelectedItem as ModelSw.ExistingAsmsAndParts;
            try
            {
                VaultSystem.GoToFile(@item?.PartPath);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        void onlySearch_Checked(object sender, RoutedEventArgs e)
        {
            ExistingParts.Visibility = onlySearch?.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
            FindParts.IsEnabled = true;
            Build.IsEnabled = false;
        }

        void onlySearch_Unchecked(object sender, RoutedEventArgs e)
        {
            ExistingParts.Visibility = onlySearch?.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
        }

        bool OnlyFind { get; set; }

        void FindParts_Click(object sender, RoutedEventArgs e)
        {
            OnlyFind = true;
            try
            {
                BUILDING_Click(null, null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            OnlyFind = false;
        }

        void ExistingParts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = PartsPdmTable?.SelectedItem as ModelSw.ExistingAsmsAndParts;
            OpenMenuItem.IsEnabled = item?.ExistInSistem != "Нет";
            GoToFileMenuItem.IsEnabled = item?.ExistInSistem != "Нет";
        }
                
        readonly SolidColorBrush _orangeColorBrush = new SolidColorBrush(Colors.LavenderBlush);
        readonly SolidColorBrush _lightCyanColorBrush = new SolidColorBrush(Colors.LightCyan);
        void ExistingParts_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var item = (ModelSw.ExistingAsmsAndParts)e.Row.DataContext;
            e.Row.Background = item?.ExistInSistem == "Нет" ? _lightCyanColorBrush : _orangeColorBrush;
        }

        void PartsGenerationChk_Checked(object sender, RoutedEventArgs e)
        {
            PartsGeneration.Visibility = Visibility.Visible;
        }

        void PartsGenerationChk_Unchecked(object sender, RoutedEventArgs e)
        {
            PartsGeneration.Visibility = Visibility.Collapsed;
        }

        void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            Quary.Visibility = Visibility.Visible;
        }

        void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Quary.Visibility = Visibility.Collapsed;
        }

        void BuildRb_Checked(object sender, RoutedEventArgs e)
        {
            FindParts.IsEnabled = false;
            Build.IsEnabled = true;
        }

        private void BUILDNG_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

