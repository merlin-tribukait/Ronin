   M S B A M L   `   `   `  ����B  >Ronin, Version=1.0.0.180, Culture=neutral, PublicKeyToken=nullclr-namespace:RoninRonin  5   	   1clr-namespace:Ronin.UtilitiesRonin.Utilities  5   	       Ronin.MainWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   %localclr-namespace:Ronin     5   	   T PMahApps.Metro, Version=1.3.0.0, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f;controls,http://metro.mahapps.com/winfx/xaml/controls  5   	   -	utilitiesclr-namespace:Ronin.Utilities  5   	   -   5        (  "MahApps.Metro.Controls.MetroWindow    TitleForeground
  �   � � 
ResizeMode$ CanMinimize=�5   	    � Title$ L2 Ronin��5   	   $	��550��6   $	��725��6'      TitleCharacterCasing$ Normal=�5   	    � 	Resources 5      *   $Ronin.Utilities.BoolToBrushConverter  5   
      KeybrushConverter  6)   +   %Ronin.Utilities.BoolToStringConverter  5   
   stringConverter  6*   *   $Ronin.Utilities.InverseBoolConverter  5   
   inverseBoolConverter  6)   .��5      � .+�5   
   �  � ColumnDefinitions 5      �� 5      $	��160 �6#   61   �� 5      6%   .+�5      �� $��1q�6   
�������$��0,0,0,0q�6<   ._�5      �� -   5         ��Name$ BotsList��5      ��6,   �� *Bots+ �� SelectedItem 6I   �� *SelectedBot+ �� Mode$ TwoWay=���5      �� 5      .R�5      � $��70��6    5      �� 5   "   $��20 �63   6@   �� 5    "   65   	 � RowDefinitions	 5"      �� 5#   "   $��25 �60   �� 5$   "   �� 5%   "   .+�5'      �� $��0q�6%   
���   �$��0��6M   $��0��6]   $��Center=�6j   ._�5(   "   �� $��0��6,   $��0��6<   $��13(�6I   $
��Bold"�6W   ��6i   �� *PlayerData.MainHero.Level+5)      �� 5*      $��1��6(   $��0��68   $��Center=�6E   $��14(�6b   $
��Bold"�6p   $��Italic$�6�   ��6�   �� *PlayerData.MainHero.Name+�� 5+      $��0��6(   $��1��68   $��Center=�6E   $��14(�6b   HP6t   �� 5,      $��0��6(   $��2��68   $��Center=�6E   $��14(�6b   MP6t   �� 5-   "   $��1q�6)   
���   �$��1��6Q   $��1��6a   $��13��6n   $	��100��6{   ._�5.   "   -  'MahApps.Metro.Controls.MetroProgressBar  
���  ��$��Center=�6M   $��1��6j   $��1��6z   
 � Minimum$
 0Q�6�    � Maximum$	 100Q�6�   $��13��6�   $	��100��6�   $
��Left=�6�    � Value 6�   �� *#!PlayerData.MainHero.HealthPercent+$ OneWay=�6  5/   #   �� 50   "   $��1q�6)   
���   �$��1��6Q   $��2��6a   $��13��6n   $	��100��6{   ._�51   &     
����  �$��Center=�6R   $��1��6o   $��2��6   $
 0Q�6�   $	 100Q�6�   $��13��6�   $	��100��6�   $
��Left=�6�    6�   �� *!PlayerData.MainHero.ManaPercent+$ OneWay=�6"  52   #   53      54      56      57      �� 58      $��1��6   $��1q�6%   
�������$��-1,0,0,0q�6L   ._�59      � 	 5:      �� 5;      $��3* �6$   �� 5<      $��2* �6$   .+�5>      � $��0��6   	 5?      �� 5@      $��1* �6(   �� 5A      $��1* �6(   �� 5B      �� 5C      �� 5D      �� 5E      .+�5G      � $��1��6   	 5H      �� 5I      �� 5J      .+�5L      �� $��Bottom=�6$   $��Center=�6?   $��18(�6\   $
��Bold"�6j   ��6|   �� *&$SelectedBot.PlayerData.MainHero.Name+ �� FallbackValue$ Unknown���� 5M      $��1��6%    �� Orientation$ 
Horizontal=�62   $��Center=�6L   .+�5N      �� $��1��6(   $��14(�65   ��6C   �� *'%SelectedBot.PlayerData.MainHero.Level+$ 0���� 5O      $��1��6(   $��14(�65   $��3,0,0,0q�6C   lv.6[   �� 5P      $��1��6(   $��14(�65   $��7,0,0,0q�6C   ��6T   �� *)'SelectedBot.PlayerData.MainHero.ClassId+$
 None��5Q      5R      �� 5S      $ 
Horizontal=�6!   $��2��6:   $��Center=�6G   $	��Top=�6d   .+�5T      � .+�5U      �� $��1q�6%   
���   �$��2��6M   $��22��6Z   $	��150��6g   ._�5V   "     $��Center=�6<   
��� ���$��2��6m   $	��150��6z   $��22��6�    6�   �� *53SelectedBot.PlayerData.MainHero.CombatPointsPercent+$ OneWay=�5W      �� 5X      $ 
Horizontal=�6)   $��2��6B   $��Center=�6O   .+�5Y   "   �� $��Center=�6,   ��6H   �� *.,SelectedBot.PlayerData.MainHero.CombatPoints+$ OneWay=�$ 0��6�   �� 5Z   "   $��Center=�6,   $��Center=�6G   /6h   �� 5[   "   $��Center=�6,   $��Center=�6G   ��6d   �� *1/SelectedBot.PlayerData.MainHero.MaxCombatPoints+$ OneWay=�$ 0��6�   5\      5]      � 5_      $��10,0,0,0q�6   .+�5`      �� $��1q�6%   
���   �$��2��6M   $��22��6Z   $	��150��6g   ._�5a   "     $��Center=�6<   
���<��$��2��6n   $	��150��6{   $��22��6�    6�   �� */-SelectedBot.PlayerData.MainHero.HealthPercent+$ OneWay=�5b      �� 5c      $ 
Horizontal=�6)   $��2��6B   $��Center=�6O   .+�5d   "   �� $��Center=�6,   ��6H   �� *(&SelectedBot.PlayerData.MainHero.Health+$ OneWay=�$ 0��6�   �� 5e   "   $��Center=�6,   $��Center=�6G   /6h   �� 5f   "   $��Center=�6,   $��Center=�6G   ��6d   �� *+)SelectedBot.PlayerData.MainHero.MaxHealth+$ OneWay=�$ 0��6�   5g      5h      � 5j      $��10,0,0,0q�6   .+�5k      �� $��1q�6%   
���   �$��2��6M   $��22��6Z   $	��150��6g   ._�5l   "     $��Center=�6<   
������$��2��6q   $	��150��6~   $��22��6�    6�   �� *-+SelectedBot.PlayerData.MainHero.ManaPercent+$ OneWay=�5m      �� 5n      $ 
Horizontal=�6)   $��2��6B   $��Center=�6O   .+�5o   "   �� $��Center=�6,   ��6H   �� *&$SelectedBot.PlayerData.MainHero.Mana+$ OneWay=�$ 0��6�   �� 5p   "   $��Center=�6,   $��Center=�6G   /6h   �� 5q   "   $��Center=�6,   $��Center=�6G   ��6d   �� *)'SelectedBot.PlayerData.MainHero.MaxMana+$ OneWay=�$ 0��6�   5r      5s      5t      �� 5u      $��0��6!   $��Center=�6.   .+�5v      �� -   5        $	��350��5v   �   $��40��6�   
�������$��18(�62  ��6!   �� *SelectedBot.Engine.Running+  stringConverter �� 	Converter# [ $ Start����6�   �� *SelectedBot.Engine.Running+  brushConverter# [ $ 	DarkGreen��6@  5w      �� 5x      -   5        $	��300��5x      $��30��6*   $��5��66   $��14(�6C   $	��Top=�6Q   .��6~   CONFIGURATION6�   �� 5y      -   5        $��versionLabel��5y   s   $��5��6    $��Bottom=�6-   $��Center=�6H   $��16(�6e   6s   5z      �� 5{      -   5         ��Name$ Scroller��5{      $��1��6X   .��5|      �� -   5         ��Name$ 
textBoxLog��5|   "   $
��Left=�66   $��Stretch=�6Q   $��14(�6m    �� 
IsReadOnly .  �� 
LineHeight$ 1��6�   6�   5}      5~      5      5�      5�      5�      