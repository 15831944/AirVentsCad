   M S B A M L   `   `   `  ����   AirVentsCad%    AirVentsCadWpf.DataControls.Bom   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35J 9http://schemas.microsoft.com/winfx/2006/xaml/presentation       R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     .���   � ColumnDefinitions  �� $
��Auto ��� $
��Auto ��� $
��Auto � � RowDefinitions �� $��40 ��� $
��Auto ��� $
��Auto ��� .+�� -    �Name$ AsmGrid��$��2q�$��1��$��0��$	��Top=�$
��Left=� �� $
��Auto ��� �� $��50 �.+��� $'��!Сборочные единицы��$��Center=�$��2q�$��Center=�$��20(�&   System.Windows.Controls.DataGrid  -     Name$ BomTableAsm��$��5q�   AutoGenerateColumns .  $��1��$��0�� �� SelectedIndex$ 0��$
��Auto��$
��Auto��   
IsReadOnly .    CanUserReorderColumns .     CanUserResizeColumns .  	   CanUserSortColumns	 .  
   SelectionUnit$
 FullRow=�$��Center=�$	��Top=�$	��550��   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Обозначение��   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Обозначение+ �� StringFormat$ {0:C}��  $ Конфигурация��$
 Auto  �� *Конфигурация+   ElementStyle �� .���  �� Property �@��TextAlignment �� Value$ Center=�  $ Наименование��$
 Auto  �� *Наименование+  $ Кол-во��$
 Auto  �� *Количество+ �� .���  �@��TextAlignment$ Center=�  $ Версия��$
 Auto  �� * ПоследняяВерсия+ �� .���  �@��TextAlignment$ Center=�� -   $ PrtGrid��$��2q�$��1��$	��Top=�$��1��$
��Left=� �� $
��Auto ��� $
��Auto ��� $
��Auto �.+�  -   $ BomTablePrt��$��5q�$��1�� .    CanUserResizeRows .   .  $ 0��$
��Auto��$
��Auto��$	��800��$	��Top=� .   . 	 . $
 FullRow=�$	��550��$��Center=�$��2��   SelectionMode$ Single=� � ToolTip$t nДвойной щелчек для занесения в список деталей для изменений��   $ Обозначение��$
 Auto    MinWidth$ 70Q� �� *Обозначение+$ {0:C}��  $ Конфигурация��$
 Auto  �� *Конфигурация+ �� .���  �@��TextAlignment$ Center=�  $ Наименование��$
 Auto  �� *Наименование+  $ Материал��$
 Auto    MaxWidth$	 150Q� �� *Материал+  $ Материал ЦМИ��$
 Auto $	 150Q� �� *МатериалЦми+  $ Толщина��$
 Auto  �� *ТолщинаЛиста+  $ Кол-во��$
 Auto  �� *Количество+ �� .���  �@��TextAlignment$ Center=�  $ Версия��$
 Auto  �� * ПоследняяВерсия+ �� .���  �@��TextAlignment$ Center=��� $��0��$��Детали��$��Center=�$��2q�$��Center=�$��20(�� -   $ 
PrtMessage��$��2q�$��2��$��25��� $
��Left=�$��17,9,0,0q�$	��Top=�  �� $
��Auto ��� $
��Auto �.+��� -    ��Name$ FilterTextBox��$��0��$��1��$��Center=�$��2q� �� TextWrapping$
 Wrap=�$�� ��$��Center=�$��19��$	��100���� $��0��$��Фильтр��$��Center=�$��2q�$��Center=� J� RenderTransformOrigin$ 0.579,0.115&�9� -    9�Name$ 	Expander1��$��2��$��0��$+��%Полная спецификация��$��2q�$��4��$
��1000��$
��Left=�$	��Top=�.��  -   $ BomTable��$ 0��$	��500��$��2q��� -	    ��Name$ СборкаLbl��$��0��$��0��$�� ��$��Center=�$��10,3,0,0q�$��Center=�$��20(�$��2��$��50��� $
��Left=�$��	28,0,0,10q�$��Bottom=�$��1��$��0��  �� $
��Auto ��� $
��Auto �.+��� -
   $ ConnectToVault��$��0��$��Label��$
��Left=�$��2q�$	��Top=�$ 0.579,0.115&�$��1���� -    ��Name$ Loggin��$��0��$!��Выбрать сборку��$��Center=�$��2q�$��Center=�$ 	0.628,0.5&���Z� �� -   $ HelpSelectedPrtGrid��$g��aПерестроить, сохранить и закрыть все открытые детали��$��2��$��0��$
��Left=�$��
10,105,0,0q�$	��Top=�� -   $ SelectedPrtGrid��$��2q�$��1��$��2��$
��Left=�$��2�� �� $
��Auto ��� $
��Auto ��� $
��Auto �.+�  -   $ BomTableSelectedPrt��$��5q�$��1�� .  .   .  $ 0��$
��Auto��$
��Auto��$	��Top=� .   . 	 . $
 FullRow=�$	��250��$��Center=�$��2��$ Single=�   $ Обозначение��$
 Auto $ 70Q� �� *Обозначение+$ {0:C}��  $ Наименование��$
 Auto  �� *Наименование+�� $��0��$,��&Детали для изменений��$��Center=�$��2q�$��Center=�$��20(�� -   $ PrtMessage1��$��2q�$��2��$��50��$��Center=� �� $	��40* ��� $	��69* ��� $	��37* �  �� $
��Auto �.+��� -   $ MakeDxfButton��$#��Очистить список��$��2q�$��Center=�$
��Auto��$��1��$��30����Z� �� -   $ MaceCutLists��$#��Выгрузить_CutList_ы��$��2q�$��Center=�$��0��$��2��$��30����Z� 