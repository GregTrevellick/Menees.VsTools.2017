   M S B A M L   `   `   `  ����i  eMicrosoft.VisualStudio.Shell.15.0, Version=15.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a�Zclr-namespace:Microsoft.VisualStudio.PlatformUI;assembly=Microsoft.VisualStudio.Shell.15.0!Microsoft.VisualStudio.PlatformUI  5       Menees.VsTools)  #Menees.VsTools.BaseConverterControl   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      ivsZclr-namespace:Microsoft.VisualStudio.PlatformUI;assembly=Microsoft.VisualStudio.Shell.15.0     5      -   5        9   3Microsoft.VisualStudio.PlatformUI.EnvironmentColors#    ToolWindowBackgroundBrushKey#���   5
       � 	Resources 5         defaultStyle �� Property �� Value   ToolWindowTextBrushKey �� 
TargetType �� BasedOn"   ComboBoxPopupBorderBrushKey��%!  &	        '�� ^     7   '�� �     7   '�� �     7   �� 5      .�5      ��  � ��# �   6*   �� 5       � ��# �  6*   5      �� 5      " ��6
   8   6&   �� 5      " ��6
   8   6(   �� 5      " ��6
   8   .�5      ��  � ��# �  6#   5      5      .��5      �  � RowDefinitions 5      �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5       	 � ColumnDefinitions	 5"      �� 5#      $
��Auto �6   �� 5$      .+�5&      �� $��Byte Order:��6
   �� 5'      -   5        
 ��Name$
 	byteOrder��5'      $��1��6    $��4q�60    �� SelectedIndex$ 0��6;   .@�5)      �� $��Numeric��6   �� 5*      $��Little Endian��6   �� 5+      $��
Big Endian��6   5,      �� 5-      $��
Data Type:��6
   $��1��6   �� 5.      -   5        $
 
numberType��5.      $��1��6!   $��1��61   $��4q�6>   $ 4��6I   .@�50      �� $��SByte - 1 byte��6   �� 51      $��Byte - 1 byte��6   �� 52      $��Int16 - 2 bytes��6   �� 53      $��UInt16 - 2 bytes��6   �� 54      $��Int32 - 4 bytes��6   �� 55      $��UInt32 - 4 bytes��6   �� 56      $��Int64 - 8 bytes��6   �� 57      $��UInt64 - 8 bytes��6   �� 58      $��Single - 4 bytes��6   �� 59      $��Double - 8 bytes��6   �� 5:      $��Decimal - 16 bytes��6   5;      �� 5<      $
��Hex:��6
   $��2��6   �� 5=      -   5         ��Name$ hexEdit��5=      $��1��6   $��2��6-   $��4q�6:   6E   �� 5>      $��Decimal:��6
   $��3��6   �� 5?      -   5        $ decimalEdit��5?      $��1��6!   $��3��61   $��4q�6>   6I   �� 5@      $��Binary:��6
   $��4��6   �� 5A      -   5        $ 
binaryEdit��5A      $��1��6    $��4��60   $��4q�6=    �� TextWrapping$
 Wrap=�6H   " �� VerticalScrollBarVisibility$
 Auto=�6\   6   5B      5C      