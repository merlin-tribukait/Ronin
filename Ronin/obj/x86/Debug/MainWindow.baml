   M S B A M L   `   `   `  ����@  <Ronin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:RoninRonin  5   	       Ronin.MainWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   %localclr-namespace:Ronin     5   	   T PMahApps.Metro, Version=1.3.0.0, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f;controls,http://metro.mahapps.com/winfx/xaml/controls  5   	     � Title$  L2 Ronin��5
   	   $	��650��6   $	��825��6'   (  "MahApps.Metro.Controls.MetroWindow   TitleCharacterCasing$ Normal=�5   	   .��5      � .+�5   
   �  � ColumnDefinitions 5      �� 5      $	��160 �6#   61   �� 5      6%   .+�5      �� $��1q�6   
�������$��0,-1,0,0q�6<   ._�5      �� ��6   �� *Bots+ �� SelectedItem 67   �� *SelectedBot+ �� Mode$ TwoWay=���5      �� 5      .R�5      � $��70��6    5      �� 5   "   $��20 �63   6@   �� 5   "   65    � RowDefinitions 5      �� 5   "   $��25 �60   �� 5   "   �� 5   "   .+�5       �� $��0q�6%   
���   �$��0��6M   $��0��6]   $��Center=�6j   ._�5!   "   �� $��0��6,   $��0��6<   $��13(�6I   $
��Bold"�6W   ��6i   �� *PlayerData.MainHero.Level+5"      �� 5#      $��1��6(   $��0��68   $��Center=�6E   $��14(�6b   $
��Bold"�6p   $��Italic$�6�   ��6�   �� *PlayerData.MainHero.Name+�� 5$      $��0��6(   $��1��68   $��Center=�6E   $��14(�6b   HP6t   �� 5%      $��0��6(   $��2��68   $��Center=�6E   $��14(�6b   MP6t   �� 5&   "   $��1q�6)   
���   �$��1��6Q   $��1��6a   $��13��6n   $	��100��6{   ._�5'   "   -  'MahApps.Metro.Controls.MetroProgressBar  
���  ��$��Center=�6M   $��1��6j   $��1��6z    � Minimum$ 0Q�6�    � Maximum$	 100Q�6�   $��13��6�   $	��100��6�   $
��Left=�6�    � Value 6�   �� *#!PlayerData.MainHero.HealthPercent+$ OneWay=�6  5(   #   �� 5)   "   $��1q�6)   
���   �$��1��6Q   $��2��6a   $��13��6n   $	��100��6{   ._�5*   &     
����  �$��Center=�6R   $��1��6o   $��2��6   $ 0Q�6�   $	 100Q�6�   $��13��6�   $	��100��6�   $
��Left=�6�    6�   �� *!PlayerData.MainHero.ManaPercent+$ OneWay=�6"  5+   #   5,      5-      5/      50      � 51      $��1��6    52      �� 53      $��3* �6$   �� 54      $��2* �6$   .+�56      � $��0��6    57      �� 58      $��1* �6(   �� 59      $��1* �6(   �� 5:      �� 5;      �� 5<      �� 5=      .+�5?      � $��1��6    5@      �� 5A      �� 5B      .+�5D      �� $��Bottom=�6$   $��Center=�6?   $��18(�6\   $
��Bold"�6j   ��6|   �� *&$SelectedBot.PlayerData.MainHero.Name+	 �� FallbackValue$	 Unknown���� 5E      $��1��6%   
 �� Orientation$
 
Horizontal=�62   $��Center=�6L   .+�5F      �� $��1��6(   $��14(�65   ��6C   �� *'%SelectedBot.PlayerData.MainHero.Level+$	 0���� 5G      $��1��6(   $��14(�65   $��3,0,0,0q�6C   lv.6[   �� 5H      $��1��6(   $��14(�65   $��7,0,0,0q�6C   ��6T   �� *)'SelectedBot.PlayerData.MainHero.ClassId+$
	 None��5I      5J      �� 5K      $
 
Horizontal=�6!   $��2��6:   $��Center=�6G   $	��Top=�6d   .+�5L      � .+�5M      �� $��1q�6%   
���   �$��2��6M   $��22��6Z   $	��150��6g   ._�5N   "     $��Center=�6<   
��� ���$��2��6k   $	��150��6x   $��22��6�    6�   �� *53SelectedBot.PlayerData.MainHero.CombatPointsPercent+$ OneWay=�5O      �� 5P      $
 
Horizontal=�6)   $��2��6B   $��Center=�6O   .+�5Q   "   �� $��Center=�6,   ��6H   �� *.,SelectedBot.PlayerData.MainHero.CombatPoints+$ OneWay=�$	 0��6�   �� 5R   "   $��Center=�6,   $��Center=�6G   /6h   �� 5S   "   $��Center=�6,   $��Center=�6G   ��6d   �� *1/SelectedBot.PlayerData.MainHero.MaxCombatPoints+$ OneWay=�$	 0��6�   5T      5U      � 5W      $��10,0,0,0q�6   .+�5X      �� $��1q�6%   
���   �$��2��6M   $��22��6Z   $	��150��6g   ._�5Y   "     $��Center=�6<   
���  ��$��2��6j   $	��150��6w   $��22��6�    6�   �� */-SelectedBot.PlayerData.MainHero.HealthPercent+$ OneWay=�5Z      �� 5[      $
 
Horizontal=�6)   $��2��6B   $��Center=�6O   .+�5\   "   �� $��Center=�6,   ��6H   �� *(&SelectedBot.PlayerData.MainHero.Health+$ OneWay=�$	 0��6�   �� 5]   "   $��Center=�6,   $��Center=�6G   /6h   �� 5^   "   $��Center=�6,   $��Center=�6G   ��6d   �� *+)SelectedBot.PlayerData.MainHero.MaxHealth+$ OneWay=�$	 0��6�   5_      5`      � 5b      $��10,0,0,0q�6   .+�5c      �� $��1q�6%   
���   �$��2��6M   $��22��6Z   $	��150��6g   ._�5d   "     $��Center=�6<   
������$��2��6q   $	��150��6~   $��22��6�    6�   �� *-+SelectedBot.PlayerData.MainHero.ManaPercent+$ OneWay=�5e      �� 5f      $
 
Horizontal=�6)   $��2��6B   $��Center=�6O   .+�5g   "   �� $��Center=�6,   ��6H   �� *&$SelectedBot.PlayerData.MainHero.Mana+$ OneWay=�$	 0��6�   �� 5h   "   $��Center=�6,   $��Center=�6G   /6h   �� 5i   "   $��Center=�6,   $��Center=�6G   ��6d   �� *)'SelectedBot.PlayerData.MainHero.MaxMana+$ OneWay=�$	 0��6�   5j      5k      5l      �� 5m      $��0��6!   $��Center=�6.   .+�5n      �� -   5        $	��350��5n   !   $��40��6-   
��� ��
�������$��18(�6d   .��6�   Start6�   5o      �� 5p      -   5        $	��300��5p      $��30��6*   $��5��67   $��14(�6D   .��6g   configuration6v   5q      �� 5r      -   5         ��Name$ Scroller��5r      $��1��6X   .��5s      �� -   5         ��Name$ 
textBoxLog��5s   "   $
��Left=�66   $��Stretch=�6Q   $��14(�6m    �� 
IsReadOnly .  �� 
LineHeight$ 1��6�   6�   5t      5u      5v      5w      5x      