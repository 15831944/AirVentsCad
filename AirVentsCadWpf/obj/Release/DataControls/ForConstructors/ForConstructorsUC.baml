   M S B A M L   `   `   `  ����   AirVentsCadC    =AirVentsCadWpf.DataControls.ForConstructors.ForConstructorsUc   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     ���	#FFFFFEFE.���   � RowDefinitions  �� $
��Auto ��� $��15 ��� $
��Auto ��� $��15 ��� $
��Auto ��� $��15 ��� $��67 ��� $��15 ��� $	��150 �.+�� $��Stretch=�$��1��$��Stretch=����	#FFF3E7E7� $��Stretch=�$��3��$��Stretch=����	#FFF3E7E7� -   $��0��$��Stretch=�$��Stretch=�.+�� -    �Name$ 	Temporary��$
��Left=�$��0,0,0,20q�$��Bottom=�$	��450�� � ColumnDefinitions �� $
��Auto ��� $��60 ��� $
��Auto ��� .+��� $��Тип��$��Center=�$��Stretch=��� -    ��Name$ TypeOfDumper��$��1��$��Center=�$��Stretch=� �� SelectedIndex$ 0�� �� 
IsReadOnly . $��1��$��24��.@��� $��20���� $��30��� $��Center=�$��3��$
��Left=� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� -    ��Name$ HeightDamper��$��5�� �� 	MaxLength$ 4��$��Center=�$��Center=�$��3��$��40��$��24��$��Center=�$��Center=��� -    ��Name$ HeightLabel��$��Высота��$��Center=�$	��Top=�$��2���� -   $ WidthDamper��$��3��$ 4��$��Center=�$��40��$��24��$��Center=�$��Center=�$��Center=�$��1���� -   $ 
WidthLabel��$��0��$��Ширина��$
��Left=�$��Center=��� -   	 ��Name$	 BuildDamper��$��0��$��7��$��Построить��$
��Left=�$��30��$��0q�$��Bottom=�$	��100����Z� �� $ ��Конструкторам��$
��Left=�$��	44,0,0,80q�$��Center=�$��Center=�$��24(�$��42��$��Bottom=�� $��Stretch=�$��5��$��Stretch=����	#FFF3E7E7� $��Stretch=�$��7��$��Stretch=����	#FFF3E7E7�� $��
SolidWorks��$
��Left=�$��5q�$��4��.��� $
��Left=�$	��Top=�$��5q�  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� -	   $@��:Обозначения текущего документа��$��5q�$	��Top=�$	��5,1q���Z� �� -
   $6��0Закрыть активный документ��$��5q�$	��Top=�$��1��$	��5,1q���Z� �� -   $��Закрыть SolidWorks��$��5q�$	��Top=�$��2��$	��5,1q���Z� �� -   $.��(Закрыть процесс SolidWorks��$��5q�$��Bottom=�$��3��$	��5,1q���Z� �� -   $Z��TВнести массу по всем к-ям в таблицу параметров��$��5q�$��Bottom=�$��4��$	��5,1q���Z� �� -   $7��1Получение детали и свойств��$��5q�$��Bottom=�$��5��$	��5,1q���Z� �� $,��&Таблицы для чертежей��$
��Left=�$��5q�$��2��.��� $
��Left=�$	��Top=�$��5q�  �� $
��Auto ��� $
��Auto �.+��� -   $.	 (ДобавитьТаблицуВидов��$0��*Добавить таблицу видов��$��5q�$	��Top=�$	��5,1q���Z� �� -   $4	 .ДобавитьТаблицуРазмеров��$6��0Добавить таблицу размеров��$��5q�$��1��$	��Top=�$	��5,1q���Z� &   System.Windows.Controls.DataGrid  -   
  Name$
 ТаблицаВидов��$
��Left=�$��
213,10,0,0q�$��2��$	��Top=�$��87��$	��218��