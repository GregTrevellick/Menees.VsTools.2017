   M S B A M L   `   `   `  ���� i  eMicrosoft.VisualStudio.Shell.15.0, Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a�Zclr-namespace:Microsoft.VisualStudio.PlatformUI;assembly=Microsoft.VisualStudio.Shell.15.0!Microsoft.VisualStudio.PlatformUI   Menees.VsTools  Menees.VsTools.ListDialog   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     iuiZclr-namespace:Microsoft.VisualStudio.PlatformUI;assembly=Microsoft.VisualStudio.Shell.15.0       � Title$
  List��$	��500��$	��300�� � SizeToContent$ WidthAndHeight=� � WindowStartupLocation$ CenterOwner=� � 
ResizeMode$ NoResize=� � ShowInTaskbar .   +� FocusedElement ��  �� ElementName$ editBox��.��� $��6q� � ColumnDefinitions �� ��  � RowDefinitions �� �� $
��Auto ��� $
��Auto �.+�]� $��2��.+��� -   	 ��Name$	 prompt��$��Prompt:��$	��Top=�&   System.Windows.Controls.DataGrid  -   
  Name$

 list��$	��5,0q�   
IsReadOnly .    EnableRowVirtualization .     HorizontalGridLinesBrush# � ��   VerticalGridLinesBrush# � ��   AutoGenerateColumns .  #��� ��   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ #��1  +System.Windows.Controls.DataGridBoundColumn   Binding ��  �� Path$ Item1�  -   
itemColumn��$
 Item�� �� $ Item2��� -    ��Name$ editBox��$��1��$��2��$	��600��$��6q��� -    ��Name$ 
okayButton��$��2��$��OK��$��75��$��6q� �� 	IsDefault . $��Right=��� $��Cancel��$��2��$��1��$��75��$��6q� �� IsCancel . $
��Left=�