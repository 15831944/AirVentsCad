   M S B A M L   `   `   `  ����  AirVentsCad0    *AirVentsCadWpf.DataControls.MontageFrameUc   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .��5      �   � RowDefinitions  5   
   �� 5	      $
��Auto �6   �� 5
      $��15 �6   �� 5      $��66 �6   �� 5      $��15 �6   �� 5      $��66 �6   �� 5      $��15 �6   �� 5      $��66 �6   �� 5      $��15 �6   �� 5      $	��150 �6   .+�5   
   � $��Stretch=�6   $��1��6-   $��Stretch=�6:   � 5   
   $��Stretch=�6   $��2��6-   $��Stretch=�6:   � 5   
   $��Stretch=�6   $��3��6-   $��Stretch=�6:   � 5   
   $��0��6   $��Stretch=�6   $��Stretch=�6:   $��3q�6V    � ColumnDefinitions 5      �� 5      $
��Auto �6#   �� 5      $
��Auto �6#     5       �� 5!      �� 5"      �� 5#      .+�5%      � $
��Left=�6   $��Bottom=�6.   $��2q�6I   $��1��6T   $��0��6a    5&      �� 5'      $
��Auto �6'   �� 5(      $
��Auto �6'   �� 5)      $
��Auto �6'   �� 5*      $
��Auto �6'   �� 5+      $
��Auto �6'   �� 5,      $��50 �6'   �� 5-      $
��Auto �6'   �� 5.      $��50 �6'   �� 5/      $��90 �6'   .+�51      �� $��Тип рамы��6   $��3q�6+   $��Center=�66   $��Center=�6Q   $��2,5,5,5q�6n   �� 52      -   5         ��Name$ TypeOfFrame��52      $��Center=�60    �� SelectedIndex$ 0��6K    �� 
IsReadOnly . $��Stretch=�6o   $��1��6�   $��2q�6�   $��1��6�   $��Center=�6  $��35��68  .@�53      �� $��0��6#    � ToolTip$ Обычный блок��6/   �� 54      $��1��6#   $% Блок рекупертора��6/   �� 55      $��2��6#   $* $С продольной балкой��6/   �� 56      $��3��6#   $? 9Со смещенной поперечной балкой��6/   57      �� 58      -   5         ��Name$ FrameOffsetLabel��58      $��Смещение��62   $��Center=�6E   $��3q�6b   $��Center=�6m   $��2��6�   �� 59      -   5         ��Name$ FrameOffset��59      $��Stretch=�6/   $��Center=�6z   $��3��6�    �� TextAlignment$ Center=�6�   $��40��6�   $��Center=�6�   $	��0,5q�6�   $��2��6  {�.  6&  �� 5:      -   5        $ LenghtBaseFrameLabel��5:      $��
Длина��66   $��Center=�6F   $��0q�6c   $��Center=�6n   $��6��6�   $��Center=�6�   $��Center=�6�   �� 5;      -   5        $ LenghtBaseFrame��5;      $��Center=�63   $��Center=�6}   $��7��6�   $��Center=�6�   $��0q�6�   $��40��6�   $��5��6  �� 5<      -   5        $ WidthBaseFramelabel��5<      $��Ширина��65   $��Center=�6F   $��0q�6c   $��Center=�6n   $��4��6�   $��Center=�6�   $��40��6�   �� 5=      -   5        $ WidthBaseFrame��5=      $��Center=�62   $��Center=�6z   $��5��6�   $��Center=�6�   $��0q�6�   $��40��6�   $��4��6  �� 5>      -   5        	 ��Name$	 Build��5>      $��Построить��6(   $��Right=�6<   $��25��6X   $��5q�6d   $��Bottom=�6o   $��80��6�   $��8��5?   F   $	��6,1q�6V   ��60   Z� 6V   5@      �� 5A      -	   5        
 ��Name$
 	PictureMf��5A      $��Stretch=�6'   $	��100��6E   $	��200��6U   $��2q�6e   $��Stretch=�6p   $	��200��6�   $	��300��6�   $��1��6�   $��0��5B      $��3��6!   � 5C      -
   5         �Name$ 	PanelGrid��5C   #   $��0��6   $��2q�66   $	��Top=�6A   $
��Left=�6Y   $
��Auto��6t   $��2��6�     5D      �� 5E      $
��Auto �6$    5G      �� 5H      $
��Auto �6'   �� 5I      $
��Auto �6'   �� 5J      $
��Auto �6'   �� 5K      $
��Auto �6'   �� 5L      $
��Auto �6'   �� 5M      $
��Auto �6'   �� 5N      $
��Auto �6'   .+�5P      �� $��Материал��6   $��0��6+   $��0��68   $��Center=�6H   $��0q�6e   $��Center=�6p   $��2q�6�   $��70��6�   �� 5Q      -   5        $ MaterialMontageFrame��5Q      $��23��69   $��Center=�6F   $��Center=�6c   $ 0��6~    . $��1��6�   $��Center=�6�   $	��2,6q�6�   $��2��6�   $��5,3,2,3q�6�   $��0��6  $	��100��6  6  �� 5R      $��Толщина��6   $��0��6*   $��2��67   $
��Left=�6G   $��2q�6b   $��Center=�6m   $��2q�6�   �� 5S      -   5        $
 Ral1��5S      $��23��6)   $
��Left=�66   $��Center=�6Q   $ 0��6l    . $��4��6�   $��Center=�6�   $��2q�6�   $��2��6�   $��5,3,2,3q�6�   $��0��6�   $	��100��6�   6  �� 5T      -   5        $ CoatingType1��5T      $��23��61   $
��Left=�6>   $��Center=�6Y   $ 0��6t    . $��5��6�   $��Center=�6�   $��2q�6�   $��2��6�   $��5,3,2,3q�6�   $��0��6�   $��80��6  �� 5U      -   5        $ CoatingClass1��5U      $��23��62   $
��Left=�6?   $��Center=�6Z   $ 0��6u    . $��6��6�   $��Center=�6�   $��2q�6�   $��2��6�   $��5,3,2,3q�6�   $��0��6�   $��35��6  �� 5V      -   5        $ Thikness��5V      $��Stretch=�6-   $��Center=�6K   $ 0��6f    . $��3��6�   $��2q�6�   $��3��6�   $��40��6�   $��Center=�6�   5Z      5[      �� 5\      $!��Монтажная рама��6   $
��Left=�6-   $��2q�6H   $	��Top=�6S   $��Center=�6k   $��Center=�6�   $��24(�6�   $��0��6�   $��0��5]      5^      � 5_   
   $��Stretch=�6   $��5��6-   $��Stretch=�6:   � 5`   
   $��Stretch=�6   $��7��6-   $��Stretch=�6:   � 5a   
   -   5        $ GridTypeOfUnit50��5a      $��0q�6)   $��8��64   5b      5f      