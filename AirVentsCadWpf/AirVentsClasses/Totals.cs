using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VentsMaterials;

namespace AirVentsCadWpf.AirVentsClasses
{
    public class Totals
    {
        public static bool RemPanWidthCheck(TextBox combo, int panelNumber)
        {
            var check = true;
            if (combo.Visibility == Visibility.Visible)
            {
                if (combo.Text == "")
                {
                    check = false;
                    MessageBox.Show($"Введите ширину {panelNumber}-й съемной панели"); return check;
                }
                if (combo.Text.Contains("-"))
                {
                    check = false;
                    MessageBox.Show($"Ширина {panelNumber}-й съемной панели не может быть отрицательной!"); return check;
                }
                if (Convert.ToInt32(combo.Text) < 100)
                {
                    check = false;
                    MessageBox.Show($"Ширина {panelNumber}-й съемной панели не может быть меньше 100 мм!"); return check;
                }
            }
            return check;
        }



        public static List<ComboBoxItem> SheetMetalThikness = new List<ComboBoxItem>
            {
                new ComboBoxItem {Content = "0.5"},
                new ComboBoxItem {Content = "0.6"},
                new ComboBoxItem {Content = "0.8"},
                new ComboBoxItem {Content = "1.0"},
                new ComboBoxItem {Content = "1.2"},
                new ComboBoxItem {Content = "1.5"}
            };

        public static List<ComboBoxItem> TypeOfCutFramelessPanel =
            new List<ComboBoxItem>
            {
                new ComboBoxItem {ToolTip = "0", Content = "Стандартная"},
                new ComboBoxItem {ToolTip = "3", Content = "Сшитая"},
                new ComboBoxItem {ToolTip = "1", Content = "Прямоугольный вырез"},
                new ComboBoxItem {ToolTip = "2", Content = "Круглый вырез"}
            };

        public static List<char> SectionLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToList();

        //static readonly SqlBaseData _sqlBaseData = new SqlBaseData();
        public static void SetMaterial(ComboBox item)
        {
            item.ItemsSource = ((IListSource)SqlBaseData.MaterialsTable()).GetList();
            item.DisplayMemberPath = "MaterialsName";
            item.SelectedValuePath = "LevelID";
            item.SelectedIndex = 0;
        }

        public static void SetMontageFrameMaterial(ComboBox item)
        {
            item.ItemsSource = ((IListSource)SqlBaseData.MaterialsForMontageFrame()).GetList();
            item.DisplayMemberPath = "MaterialsName";
            item.SelectedValuePath = "LevelID";
            item.SelectedIndex = 0;
        }

    public static void SetCoatingType(ComboBox item)
        {
            item.ItemsSource = ((IListSource)_setMaterials.CoatingTypeDt()).GetList();
            item.DisplayMemberPath = "Name";
            item.SelectedValuePath = "Code";
            item.SelectedIndex = 0;
        }

        public static void SetCoatingClass(ComboBox item)
        {
            item.ItemsSource = _setMaterials.CoatingListClass();
        }

         public static void SetPanelType(ComboBox item)
        {
            item.ItemsSource = ((IListSource)SqlBaseData.PanelsTable()).GetList();
            item.DisplayMemberPath = "PanelTypeName";
            item.SelectedValuePath = "PanelTypeCode";
            item.SelectedIndex = 0;}

        public static void SetRal(ComboBox item)
        {
            ToSQL.Conn = Properties.Settings.Default.ConnectionToSQL;
            item.ItemsSource = ((IListSource)_toSql.RalTable()).GetList();
            item.DisplayMemberPath = "RAL";
            item.SelectedValuePath = "Hex";
            item.SelectedIndex = 0;
        }
               
        public static readonly SetMaterials _setMaterials = new SetMaterials();
        public static readonly ToSQL _toSql = new ToSQL();
        
    }
   
}