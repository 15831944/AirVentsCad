   M S B A M L   `   `   `  ���� p  lSystem.Windows.Controls.Input.Toolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35pTclr-namespace:System.Windows.Controls;assembly=System.Windows.Controls.Input.ToolkitSystem.Windows.Controls   AirVentsCad?   9AirVentsCadWpf.DataControls.Specification.SpecificationUc   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     htoolkitTclr-namespace:System.Windows.Controls;assembly=System.Windows.Controls.Input.Toolkit     J	 FWpfAnimatedGif, Version=1.4.14.0, Culture=neutral, PublicKeyToken=null,gif"http://wpfanimatedgif.codeplex.com 	 ���	#FFFFFEFE  � 	Resources  *  $System.Windows.Controls.DataGridCell �� 
TargetType �� Triggers P� Property P� Value �� Property �� Value��%Q   '        �� "   P�  �@J�IsMouseOver . . ���  � ��$ 13(�.���  � ColumnDefinitions �� $	��97* � � RowDefinitions �� $
��Auto ��� $��15 ��� $
��Auto ��� $��15 ��� $
��Auto ��� $��15 �.+�� $��Stretch=�$��1��$��Stretch=����	#FFF3E7E7$��2��� $��Stretch=�$��3��$��Stretch=����	#FFF3E7E7$��2��� $��0��$
��Left=�$��2q�$��Center=� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� -   	 ��Name$	 Status��$��0��$��Спецификация��$
��Left=�$��0,-1,0,0q�$��Center=�$��Center=�$��24(�$��42��$	��Top=�$��0���� $#��Выгрузка данных��$��2q�$��1��$��3��$��0��$
��Left=�$��Center=�.��� $��2q�$��Center=�$��Center=�$
��Auto�� �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto ��� $
��Auto �.+�� $
��Left=�$��2q�$��Center=�$��0�� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� -   $��GetList��$6��0Получить перечень деталей��$��3��$��3q�$��Center=�$��3q�$��Center=���Z� �� $��0��$��
Поиск��$��Center=�$	��3,4q�$��Center=�-   'System.Windows.Controls.AutoCompleteBox  -   
  Name$
 AutoCompleteTextBox1��$��0��$��1��$��2q�$��Center=�$��Center=�$	��100���� -    ��Name$ 
Найти��$��
Найти��$��Right=�$��3q�$��Center=�$��3q�$��2��$
��Auto��{�.  ��Z� � -    �Name$
 COnf��$
��Left=�$
��Auto��$��2q�$��1��$��Center=�$	��255�� �� $
��Auto ��� $
��Auto �.+��� -   $	 label��$��Конфигурация��$
��Left=�$��2q�$��Center=��� -    ��Name$ ConfigsCombo��$��1��$
��Left=�$��2q�$��Center=�$
��Auto�� �� SelectedIndex$ 0����Z� �� -    ��Name$) #ПереченьДеталей_Copy��$��65��$%��Перечень деталей��$
��Left=�$��5q�$��2��$	��Top=�.��� $
��Left=�$	��Top=�$��2q� �� $
��Auto ��� $
��Auto � �� $
��Auto ��� $
��Auto �.+�&   System.Windows.Controls.DataGrid  -	     Name$ PartsListXml2sDataGrid�� %� VirtualizationMode$ 	Recycling=�   CanUserAddRows .  $	��700��$��0��$
��Left=�$��2q�$��0��   SelectionMode$ Single=�   CanUserResizeRows .     RowDetailsVisibilityMode$ Visible=�   AutoGenerateColumns .  $ 0��$
��Auto��$
��Auto��$	��250������ *PartsListXml2+   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Наименование��   
IsReadOnly .    Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto    MinWidth$	 130Q�1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Наименование+  $ Конфигурация��$
 Auto  . $ 50Q� �� *Конфигурация+4  .System.Windows.Controls.DataGridCheckBoxColumn  $	 Xml�� .  $ 40    MaxWidth$	 200Q� �� *Xml+4	  .System.Windows.Controls.DataGridTemplateColumn	  $	 DXF��$ 33  .    CanUserSort .  	  CellTemplate �� .R��� -
   $��20������   �� Path$  ImageSrc�  $	 Ver�� .  $ 40 $	 200Q� �� *CurrentVersion+� $
��Auto��$	��2,0q�$��1��$��0��$	��Top=� �� $
��Auto ��� $
��Auto �  -   $! PartsListXml2sDataGrid_Copy��$ 	Recycling=� .  $��2��$
��Left=�$��5,5,0,0q�$	��Top=�$ Extended=� .  $ Visible=� .  $ 0��$
��Auto��$
��Auto��$	��275��$��0��$��3������ *PartsListXml2+   $ Наименование�� . $
 Auto $	 130Q� �� *42НаименованиеБезРасширения+  $ Конфиг.�� .  $ 60 $	 200Q� �� *Конфигурация+  $ Ширина заг.�� .  $ 80 $	 200Q� �� * ЗаготовкаШирина+  $ Высота заг.�� .  $ 70 $	 200Q� �� * ЗаготовкаВысота+  $ Гибы�� .  $ 40 $	 200Q� �� *
Гибы+  $ Толщина�� .  $ 60 $	 200Q� �� *Толщина+  $ Материал�� .  $	 100 $	 200Q� �� *Материал+  $% Площадь покрытия�� .  $	 120 $	 200Q� �� * ПлощадьПокрытия+  $ Масса �� .  $ 50 $	 200Q� �� *МассаS+�� -   $ XmlParts1_Copy��$��Выделить все��$
��Left=�$��25��$��2,5,0,0q�$��2��$	��Top=�$	��108��$��2����Z� �� -   $ groupBox��$��Выгрузка��$
��Left=�$��2q�$��2��$	��Top=�$
��Auto��$
��Auto��$��4��.��� $��Stretch=�$
��Auto��$��-1,2,5,2q�$��Stretch=�$
��Auto�� �� $
��Auto ��� $
��Auto �.+�9� -   ! 9�Name$! expander��" 9� 
IsExpanded" . $��Данные в 1С��$��Stretch=�$��2q�$��Stretch=�.��� $
��Left=�$
��Auto��$��2q�$
��Auto�� �� $
��Auto ��� $
��Auto � �� $	��200 ��� $
��Auto �.+��� -   # ��Name$# checkBoxOnlyParts��$2��,Выгрузка только деталей��$
��Left=�$��2q�$��0��$	��Top=�$ d� 	IsChecked$$ FalseY��� -   $# checkBoxOnlyAsms��$0��*Выгрузка только сборок��$
��Left=�$��2q�$��1��$��0��$	��Top=�$$ FalseY��� -   $ 	XmlParts1��$��Выгрузить в 1С��$
��Left=�$��25��$��4,6,0,10q�$��Center=�$
��Auto��$��1��$��0��$��2����Z� 9� -   $! expander_Copy��" . $��Развертки DXF��$��Stretch=�$��2q�$��Stretch=�$��1����Z� .��� .+�� $��Stretch=�$
��Auto��$��2q�$��Stretch=�$
��Auto�� �� $	��15* ��� $	��14* � �� $	��200 ��� $
��Auto ��� $
��Auto �.+��� -   $# checkBox��$=��7Только конфигурации из сборки��$��Center=�$��2,6,3,7q�$��0��$��Center=�$
$ TrueY�{�. $��0���� -   $# 	checkBox2��$6��0Включая нелистовые детали��$
��Left=�$��2,6,3,7q�$��0��$��Center=�$
$ TrueY�{�. $��1���� -   $ 	DxfParts1��$��Выгрузить DXF��$
��Left=�$��25��$��2,2,0,2q�$��Center=�$
��Auto��$��2��$��2����Z�   -   $ dataGridAfterGet�� .  $
��Left=�$��0q�$��4��$	��Top=�$	��350��$
��Auto��   $ Наименование�� . $
 Auto $	 130Q� �� *
FileName+  $ VersionBeforeGet�� .  $
 Auto $	 200Q� �� *VersionBeforeGet+  $ VersionToGet�� .  $
 Auto $	 200Q� �� *VersionToGet+  $ VersionAfterGet�� .  $
 Auto $	 200Q� �� *VersionAfterGet+  $ Equal�� .  $
 Auto  �� *Equal+7
  1System.Windows.Forms.Integration.WindowsFormsHost
  -   % 
 Name$% 	FormsHost��$
��Left=�$	��324��$��
512,10,0,0q�$��4��$	��Top=�$	��665��