   M S B A M L   `   `   `  ����   AirVentsCad4    .AirVentsCadWpf.AdminkaWindows.GroupUserControl   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$-  'Группы пользователей��$	��347��$	��500�� � Icon$9 3/AHU Selection/br_icon_teaser_cad_sr_pos_150(1).ico�� � WindowStartupLocation$ CenterScreen=� � Topmost . .���  � RowDefinitions �� $
��241* ��� $	��29* � � ColumnDefinitions �� $��80 ��� $��80 ��� $��5 ��� .+�� $��Stretch=�$��Stretch=�$��2��.+��� -    ��Name$ NewGroupName��$
��Left=�$��
10,252,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��140��&   System.Windows.Controls.DataGrid  -     Name$ Groups��$��0��$��0��$��Stretch=�$	��Top=�$��2��	   
IsReadOnly	 . $��0q�$	��240��
   SelectionMode$
 Single=� �� SelectedIndex$ 0��� $��0��$��3��$��Stretch=�$��Stretch=�$��5,0,-5,0q�.+�  -   $ 
GroupUsers��$
��Left=�	 . $	��Top=�$	��101��$
 Single=�$	��240��$	��317���� -    ��Name$ DeleteGroupButton��$��0��$��Удалить��$
��Left=�$��1��$	��Top=�$��5,5,0,0q�$��22��$��50���� -   $ AddGroupButton��$��Добавить��$��right=�$��1��$	��Top=�$��1��$��0,5,11,0q�$��22��$��59���� -   $��Удалить��$��right=�$��1��$	��Top=�$��3��$��0,5,10,0q�$��22��$��59���� -   $ AddUser��$��Добавить��$��3��$
��Left=�$��20,5,0,0q�$��1��$	��Top=�$��75��