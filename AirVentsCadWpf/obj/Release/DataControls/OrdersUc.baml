   M S B A M L   `   `   `  ����   AirVentsCad*    $AirVentsCadWpf.DataControls.OrdersUc   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     $	��Top=���. $��Visible=�.���   � RowDefinitions  �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��67 ��� $
��Auto ��� $��66 ��� $
��Auto ��� $��66 ��� $
��Auto ��� $
��Auto ��� $��70 ��� $��0* �.+��� $��
Заказ��$
��Left=�$��
50,10,0,50q�$��Center=�$��Center=�$��Center=�$��24(�$��42��� $��Center=�$��26��$��10,5,0,5q�$��1��$
��Left=� � ColumnDefinitions �� $
��Auto ��� $
��Auto ��� $	��100 �.+��� $��Типоразмер��$��Center=�$��Stretch=�$	��2,0q�$��0,5,5,5q��� -    ��Name$ 
SizeOfUnit��$��Center=�$��Center=� �� SelectedIndex$ 0�� �� 
IsReadOnly . $��1��$��Center=�$��Center=�$	��4,2q�$��3���� -    ��Name$ Nonstandard��$��Нестандарт��$��2��$��Center=�$��Center=�$
��10,5q�$��4��� $��Stretch=�$��2��$��Stretch=�$��0q�$��15���� -    ��Name$ 
SaveButton��$��7��$��Зберегти��$��Right=�$��30��$��	0,0,48,10q�$��Bottom=�$	��100��$��15����Z� � $��Stretch=�$��4��$��Stretch=�$��0q�$��15��� $��Stretch=�$��6��$��Stretch=�$��0q�$��15��� $��Stretch=�$��9��$��Stretch=�$��0q�$��15��� $��0��$��Center=�$
��Left=�$��
14,61,0,15q� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� -    ��Name$ OrderTextBox��$��003021��$��1�� �� 	MaxLength$ 8��$��Center=�$��Center=�$��1��$��Center=�$��Center=�$��40���� -   	 ��Name$	 WidthLabel2��$��0��$��
Заказ��$
��Left=�$��Center=�� $
��Left=�$��10,10,-12,0q�$��3��$	��Top=�$��4�� �� $
��Auto ��� $
��Auto �  �� $
��Auto ��� $
��Auto �.+��� $��Дата старта��$
��Left=�$��5q�$	��Top=��� $!��Дата окончания��$
��Left=�$��5q�$	��Top=�$��1��$��0��(  "System.Windows.Controls.DatePicker  -   
  Name$
 StartDay��$��0��$��1��$��Center=�$��5q�$��Center=�   FirstDayOfWeek$ Monday=�   IsTodayHighlighted .    -   $
 	FinishDay��$��1��$��Center=�$��5q�$��Center=�$��1��$ Monday=�   Text$ Введите дату�� .  