   M S B A M L   `   `   `  ����  AirVentsCad*    $AirVentsCadWpf.DataControls.OrdersUc   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      $	��Top=�5      ��. $��Visible=�6c   .��5      �   � RowDefinitions  5	   
   �� 5
      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��67 �6   �� 5      $
��Auto �6   �� 5      $��66 �6   �� 5      $
��Auto �6   �� 5      $��66 �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��70 �6   �� 5      $��0* �6   .+�5   
   �� $��
Заказ��6   $
��Left=�6    $��
50,10,0,50q�6;   $��Center=�6O   $��Center=�5      $��Center=�64   $��24(�6V   $��42��6d   � 5   
   $��Center=�6   $��26��6*   $��10,5,0,5q�66   $��1��6H   $
��Left=�6U    � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $
��Auto �6#   �� 5      $	��100 �6#   .+�5      �� $��Типоразмер��6   $��Center=�6)   $��Stretch=�6F   $	��2,0q�6b   $��0,5,5,5q�6o   �� 5       -   5         ��Name$ 
SizeOfUnit��5       $��Center=�6+   $��Center=�6H    �� SelectedIndex$ 0��6c    �� 
IsReadOnly . $��1��6�   $��Center=�6�   $��Center=�6�   $	��4,2q�6�   $��3��6�   5"      �� 5#      -   5         ��Name$ Nonstandard��5#      $��Нестандарт��6,   $��2��6A   $��Center=�6Q   $��Center=�6n   $
��10,5q�6�   $��4��6�   5$      � 5%   
   $��Stretch=�6   $��2��6-   $��Stretch=�6:   $��0q�6V   $��15��6a   �� 5&   
   -   5         ��Name$ 
SaveButton��5&      $��7��6   $��Зберегти��62   $��Right=�6E   $��30��6a   $��	0,0,48,10q�6m   $��Bottom=�6�   $	��100��6�   $��15��6�   ��6�   Z� 6�   � 5'   
   $��Stretch=�6   $��4��6-   $��Stretch=�6:   $��0q�6V   $��15��6a   � 5(   
   $��Stretch=�6   $��6��6-   $��Stretch=�6:   $��0q�6V   $��15��6a   � 5)   
   $��Stretch=�6   $��9��6-   $��Stretch=�6:   $��0q�6V   $��15��6a   � 5*   
   $��0��6   $��Center=�6   $
��Left=�67   $��
14,61,0,15q�6R    5+      �� 5,      $
��Auto �6#   �� 5-      $
��Auto �6#   �� 5.      $
��Auto �6#   �� 5/      $
��Auto �6#   .+�51      �� -   5         ��Name$ OrderTextBox��51      $��003021��6,   $��1��6:    �� 	MaxLength$ 8��6G   $��Center=�6V   $��Center=�6s   $��1��6�   $��Center=�6�   $��Center=�6�   $��40��6�   �� 52      -   5        	 ��Name$	 WidthLabel2��52      $��0��6)   $��
Заказ��6:   $
��Left=�6J   $��Center=�6e   53      � 54   
   $
��Left=�6   $��10,10,-12,0q�6*   $��3��6?   $	��Top=�6P   $��4��6h    55      �� 56      $
��Auto �6#   �� 57      $
��Auto �6#     59      �� 5:      $
��Auto �6    �� 5;      $
��Auto �6    .+�5=      �� $��Дата старта��6   $
��Left=�6*   $��5q�6E   $	��Top=�6P   �� 5>      $!��Дата окончания��6   $
��Left=�6-   $��5q�6H   $	��Top=�6S   $��1��6k   $��0��5?      (  "System.Windows.Controls.DatePicker  5@      -   5        
  Name$
 StartDay��5@      $��0��6+   $��1��68   $��Center=�6H   $��5q�6e   $��Center=�6p      FirstDayOfWeek$ Monday=�6�      IsTodayHighlighted .  6�     5A      -   5        $
 	FinishDay��5A      $��1��6,   $��Center=�6<   $��5q�6Y   $��Center=�6d   $��1��6   $ Monday=�6�      Text$ Введите дату��6�    .  6�   5B      5C      5D      