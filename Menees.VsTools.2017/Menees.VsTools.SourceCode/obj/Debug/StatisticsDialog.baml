   M S B A M L   `   `   `  ����i  eMicrosoft.VisualStudio.Shell.15.0, Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a�Zclr-namespace:Microsoft.VisualStudio.PlatformUI;assembly=Microsoft.VisualStudio.Shell.15.0!Microsoft.VisualStudio.PlatformUI  5       Menees.VsTools%  Menees.VsTools.StatisticsDialog   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      iuiZclr-namespace:Microsoft.VisualStudio.PlatformUI;assembly=Microsoft.VisualStudio.Shell.15.0     5        � Title$  
Statistics��5      $	��460��6   $	��300��6&   $	��600��65    � SizeToContent$ WidthAndHeight=�6E    � WindowStartupLocation$ CenterOwner=�6    � 
ResizeMode$ NoResize=�5   D    � ShowInTaskbar .   +� FocusedElement 6   ��  �� ElementName$ 	countGrid��.��5      � $��6,6,6,0q�6    � ColumnDefinitions 5      �� 5	      $
��Auto �6   �� 5
       � RowDefinitions 5      �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      �� 5      $
��Auto �6   .+�5      �� $��Lines:��6
   �� 5      -   5        	 ��Name$	 lineCountLabel��5   
   $��0��6"   $��1��6.   �� 5      $��Characters:��6
   $��1��6    �� 5      -   5        $	 characterCountLabel��5   
   $��0��6'   $��1��63   $��1��6A   ]� 5      $��2��6   $��2��6   .+�5      �� $��Counts:��6   $	��Top=�6   &   System.Windows.Controls.DataGrid  5      -   5        
  Name$
 	countGrid��5      $	��5,0q�6!      
IsReadOnly .    EnableRowVirtualization .     HorizontalGridLinesBrush# � ��6`      VerticalGridLinesBrush# � ��6�    �� AlternationCount$ 1��6     AlternatingRowBackground# � ��6'     AutoGenerateColumns .  #��� ��6�     Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ Dec. Num��6   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6,   ��  �� Path$ Number�  5      $ Hex. Num��6    6,   �� $ Number� �� StringFormat$ 0x{0:X}��  5      $ 	Character��6    6-   �� $ 	Character�  5      $ Count��6    6)   �� $ Count�5      5       �� 5!      $��OK��6   $��3��6   $��2��6%   $��75��69   $
��0,10q�6G    �� 	IsDefault .  �� IsCancel . $��Center=�6v   5"      5#      