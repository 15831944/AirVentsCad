   M S B A M L   `   `   `  ����   AirVentsCad2    ,AirVentsCadWpf.DataControls.Loggers.LoggerUc   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     .��� ���	#FFFFFEFE$	��500��$	��500��  � ColumnDefinitions  �� $	��700�� � RowDefinitions �� $
��Auto ��� $��15 ��� $
��Auto ��� $
��Auto ��� $��19 ��� $
��Auto �.+�� $��Stretch=�$��1��$��Stretch=����	#FFF3E7E7$��2��� $��Stretch=�$��4��$��Stretch=����	#FFF3E7E7 J� RenderTransformOrigin$ 0.5,3.2&�$��2���� $��0��$#��Логгер (события)��$
��Left=�$��
5, 15,5,15q�$��Center=�$��Center=�$��24(�$��42��$��Bottom=��� -    ��Name$ NLogGrid��$
��NLog��$
��Left=�$��0q�$��2��$��Center=�.��� $
��Left=�$	��200��$��5q�$	��Top=�$
��Auto��  �� $	��165 ���  �� �� $
��Auto �.+�&   System.Windows.Controls.DataGrid  -     Name$* $СобытийТаблицаDataGrid��   
IsReadOnly .    AutoGenerateColumns .  $��2q�   CanUserReorderColumns .     CanUserResizeColumns . 	   CanUserSortColumns	 . 
   SelectionUnit$
 FullRow=�   SelectionMode$ Single=�$��2������ *LogStucture+   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Дата��   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto    MaxWidth$	 150Q�1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *
DataTime+  $ Уровень��$
 Auto $	 100Q� �� *Level+  $ Имя класса��$
 Auto $	 170Q� �� *Place+  $ Сообщение��$
 Auto  �� *Text+� $
��Left=�$��2q�$��1��$	��Top=�$��1��  �� $
��Auto ��� $
��Auto �.+��� -    ��Name$ УровеньКомбо��$��Center=�$��2q�$��Center=�$	��120��$��1����Z� �� $��0��$��Уровень��$��Center=�$��2q�$��Center=�� $
��Left=�$��2q�$��1��$��0��$��Center=�  �� $
��Auto ��� $
��Auto �.+��� -   $ ДатаComboBox��$��Center=�$��2q�$��Center=�$	��120��$��1������ *LogStucture+��Z� �� $��0��$��Дата��$��Center=�$��2q�$��Center=��� -   $ SqlLog��$	��SQL��$��2q�$��5��.��� $��2q� �� $
��Auto ��� $
��Auto �.+�� $��2q�$��1��$��Center=�$
��Left=�  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+�� $��Center=�$	��0,2q�$��Center=� �� $	��26* ��� $	��27* �.+��� -   $ Имя��$
��Left=�$��2q�$��1��$	��Top=�$	��100��{�.  ��Z� �� -    ��Name$$ ВсеПользователи��$��0��$��Пользователь��$
��Left=�$	��2,5q�$��Center=�� -    �Name$ ДатаВыборка��$��Center=�$��2q�$��Center=�$��1�� �� $	��26* ��� $	��27* �.+��� -	   $ Дата��$
��Left=�$��2q�$��1��$	��Top=�$	��100����Z� �� $��0��$��Дата��$
��Left=�$��2q�$��Center=�$��24��� -
   $ Классы��$��Center=�$��2q�$��Center=�$��2�� �� $	��26* ��� $	��27* �.+��� -   $ ИмяКласса��$
��Left=�$��2q�$��1��$	��Top=�$	��100����Z� �� $��0��$��Имя класса��$
��Left=�$��2q�$	��Top=�$��24��� -   $ УровеньGrid��$��Center=�$	��5,2q�$��Center=�$��3�� �� $	��26* ��� $	��27* �.+��� -   $ УровеньCombo��$
��Left=�$��2q�$��1��$	��Top=�$	��100����Z� �� $��0��$��Уровень��$
��Left=�$��2q�$	��Top=�$��24���� -    ��Name$" ОбновитьДанные��$#��Обновить данные��$��5��$��Center=�$��5q�$	��5,1q���Z� � $
��Left=�$��2q�$	��Top=�$��4�� �� $
��Auto ��� $
��Auto �.+��� -   $ Информация��$��Информация��$
��Left=�$��5q�$	��Top=� d� 	IsChecked$
 TrueY��� -   $ Ошибки��$��Только ошибки��$
��Left=�$��5q�$	��Top=�$��1��$��Visible=�  -   $  ТаблицаДанных��$��0�� .  $
��Auto��$��0,0,4,4q�$
��Left=�$	��400��$	��100������ *SqlLogStucture+   $ Дата��$	 120 $	 150Q� �� *	ErrorTime+  $ Уровень��$ 60 $	 100Q� �� *
ErrorState+  $ Имя класса��$	 120 $	 170Q� �� *ErrorModule+  $ Сообщение��$	 500  �� *ErrorMessage+  $ Функция��$	 100  �� *ErrorFunction+  $ Пользователь��$	 150  �� *
UserName+  $ Код��$ 80  �� *	ErrorCode+� $��0��$
��Left=�$
��Auto��$��
701,15,0,0q�$	��Top=�$
��Auto��  �� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��0��$��2q�����  �� Path$ CurrentPage� �� StringFormat$ Current Page: {0}���� -   $��0��$��Вперед��$
��Left=�$��2q�$	��Top=�$��75��$��2����Z� �� -   $��0��$��
Назад��$
��Left=�$��2q�$	��Top=�$��75��$��1����Z�   -   $! ТаблицаДанных2��$��3�� .  $
��Auto��$��2q�$
��Left=�$	��700��$	��100������    $ Дата��$	 120 $	 150Q� �� *	ErrorTime+  $ Уровень��$ 60 $	 100Q� �� *
ErrorState+  $ Имя класса��$	 120 $	 170Q� �� *ErrorModule+  $ Сообщение��$	 500  �� *ErrorMessage+  $ Функция��$	 100  �� *ErrorFunction+  $ Пользователь��$	 150  �� *
UserName+  $ Код��$ 80  �� *	ErrorCode+�� -   $ button��$��0��$��Button��$
��Left=�$��
970,30,0,0q�$	��Top=�$��75��