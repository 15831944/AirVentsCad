   M S B A M L   `   `   `  ����   AirVentsCad0    *AirVentsCadWpf.DataControls.MontageFrameUc   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     .���   � RowDefinitions  �� $
��Auto ��� $��15 ��� $��66 ��� $��15 ��� $��66 ��� $��15 ��� $��66 ��� $��15 ��� $	��150 �.+�� $��Stretch=�$��1��$��Stretch=�� $��Stretch=�$��2��$��Stretch=�� $��Stretch=�$��3��$��Stretch=�� $��0��$��Stretch=�$��Stretch=�$��3q� � ColumnDefinitions �� $
��Auto ��� $
��Auto �  �� �� �� .+�� $
��Left=�$��Bottom=�$��2q�$��1��$��0�� �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��50 ��� $
��Auto ��� $��50 ��� $��90 �.+��� $��Тип рамы��$��3q�$��Center=�$��Center=�$��2,5,5,5q��� -    ��Name$ TypeOfFrame��$��Center=� �� SelectedIndex$ 0�� �� 
IsReadOnly . $��Stretch=�$��1��$��2q�$��1��$��Center=�$��35��.@��� $��0�� � ToolTip$ Обычный блок���� $��1��$% Блок рекупертора���� $��2��$* $С продольной балкой���� $��3��$? 9Со смещенной поперечной балкой���� -    ��Name$ FrameOffsetLabel��$��Смещение��$��Center=�$��3q�$��Center=�$��2���� -    ��Name$ FrameOffset��$��Stretch=�$��Center=�$��3�� �� TextAlignment$ Center=�$��40��$��Center=�$	��0,5q�$��2��{�.  �� -   $ LenghtBaseFrameLabel��$��
Длина��$��Center=�$��0q�$��Center=�$��6��$��Center=�$��Center=��� -   $ LenghtBaseFrame��$��Center=�$��Center=�$��7��$��Center=�$��0q�$��40��$��5���� -   $ WidthBaseFramelabel��$��Ширина��$��Center=�$��0q�$��Center=�$��4��$��Center=�$��40���� -   $ WidthBaseFrame��$��Center=�$��Center=�$��5��$��Center=�$��0q�$��40��$��4���� -   	 ��Name$	 Build��$��Построить��$��Right=�$��25��$��5q�$��Bottom=�$��80��$��8��$	��6,1q���Z� �� -	   
 ��Name$
 	PictureMf��$��Stretch=�$	��100��$	��200��$��2q�$��Stretch=�$	��200��$	��300��$��1��$��0��$��3��� -
    �Name$ 	PanelGrid��$��0��$��2q�$	��Top=�$
��Left=�$
��Auto��$��2��  �� $
��Auto � �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto ��� $
��Auto �.+��� $��Материал��$��0��$��0��$��Center=�$��0q�$��Center=�$��2q�$��70���� -   $ MaterialMontageFrame��$��23��$��Center=�$��Center=�$ 0�� . $��1��$��Center=�$	��2,6q�$��2��$��5,3,2,3q�$��0��$	��100���� $��Толщина��$��0��$��2��$
��Left=�$��2q�$��Center=�$��2q��� -   $
 Ral1��$��23��$
��Left=�$��Center=�$ 0�� . $��4��$��Center=�$��2q�$��2��$��5,3,2,3q�$��0��$	��100���� -   $ CoatingType1��$��23��$
��Left=�$��Center=�$ 0�� . $��5��$��Center=�$��2q�$��2��$��5,3,2,3q�$��0��$��80���� -   $ CoatingClass1��$��23��$
��Left=�$��Center=�$ 0�� . $��6��$��Center=�$��2q�$��2��$��5,3,2,3q�$��0��$��35���� -   $ Thikness��$��Stretch=�$��Center=�$ 0�� . $��3��$��2q�$��3��$��40��$��Center=��� $!��Монтажная рама��$
��Left=�$��2q�$	��Top=�$��Center=�$��Center=�$��24(�$��0��$��0��� $��Stretch=�$��5��$��Stretch=�� $��Stretch=�$��7��$��Stretch=�� -   $ GridTypeOfUnit50��$��0q�$��8��