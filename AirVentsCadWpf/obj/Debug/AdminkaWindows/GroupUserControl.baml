   M S B A M L   `   `   `  ����  AirVentsCad4    .AirVentsCadWpf.AdminkaWindows.GroupUserControl   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$-  'Группы пользователей��5   	   $	��347��6&   $	��500��63    � Icon$9 3/AHU Selection/br_icon_teaser_cad_sr_pos_150(1).ico��6?    � WindowStartupLocation$ CenterScreen=�6z    � Topmost . .��5      �  � RowDefinitions 5   
   �� 5      $
��241* �6   �� 5      $	��29* �6    � ColumnDefinitions 5
   
   �� 5      $��80 �6   �� 5      $��80 �6   �� 5      $��5 �6   �� 5      .+�5   
   � $��Stretch=�6   $��Stretch=�6-   $��2��6I   .+�5      �� -   5         ��Name$ NewGroupName��5      $
��Left=�6,   $��
10,252,0,0q�6G    �� TextWrapping$
 Wrap=�6[   $	��Top=�6o   $	��140��6�   5      &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ Groups��5      $��0��6#   $��0��60   $��Stretch=�6@   $	��Top=�6^   $��2��6v   	   
IsReadOnly	 . $��0q�6�   $	��240��6�   
   SelectionMode$
 Single=�6   �� SelectedIndex$ 0��6)  � 5   
   $��0��6   $��3��6   $��Stretch=�6,   $��Stretch=�6J   $��5,0,-5,0q�6f   .+�5        -   5        $ 
GroupUsers��5      $
��Left=�6+   	 . $	��Top=�6�   $	��101��6�   $
 Single=�6�   $	��240��6�   $	��317��6�   5      �� 5   
   -   5         ��Name$ DeleteGroupButton��5      $��0��6,   $��Удалить��6<   $
��Left=�6N   $��1��6i   $	��Top=�6v   $��5,5,0,0q�6�   $��22��6�   $��50��6�   6�   �� 5   
   -   5        $ AddGroupButton��5      $��Добавить��6)   $��right=�6<   $��1��6X   $	��Top=�6e   $��1��6}   $��0,5,11,0q�6�   $��22��6�   $��59��6�   6�   �� 5   
   -   5        $��Удалить��5      $��right=�6#   $��1��6?   $	��Top=�6L   $��3��6d   $��0,5,10,0q�6t   $��22��6�   $��59��6�   6�   �� 5   
   -   5        $ AddUser��5      $��Добавить��6"   $��3��65   $
��Left=�6E   $��20,5,0,0q�6`   $��1��6r   $	��Top=�6   $��75��6�   6�   5      5      