   M S B A M L   `   `   `  ���� B  >Ronin, Version=1.0.0.180, Culture=neutral, PublicKeyToken=nullclr-namespace:RoninRonin  1clr-namespace:Ronin.UtilitiesRonin.Utilities      Ronin.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     %localclr-namespace:Ronin     T PMahApps.Metro, Version=1.3.0.0, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4f;controls,http://metro.mahapps.com/winfx/xaml/controls  -	utilitiesclr-namespace:Ronin.Utilities  -   (  "MahApps.Metro.Controls.MetroWindow    TitleForeground
  �   � � 
ResizeMode$ CanMinimize=� � Title$ L2 Ronin��$	��550��$	��725��   TitleCharacterCasing$ Normal=� � 	Resources *   $Ronin.Utilities.BoolToBrushConverter     KeybrushConverter  +   %Ronin.Utilities.BoolToStringConverter  stringConverter  *   $Ronin.Utilities.InverseBoolConverter  inverseBoolConverter  .��� .+��  � ColumnDefinitions �� $	��160 ��� .+��� $��1q�
�������$��0,0,0,0q�._��� -    ��Name$ BotsList������ *Bots+ �� SelectedItem �� *SelectedBot+ �� Mode$ TwoWay=����� .R�� $��70�� �� $��20 ��� 	 � RowDefinitions	 �� $��25 ��� �� .+��� $��0q�
���   �$��0��$��0��$��Center=�._��� $��0��$��0��$��13(�$
��Bold"����� *PlayerData.MainHero.Level+�� $��1��$��0��$��Center=�$��14(�$
��Bold"�$��Italic$����� *PlayerData.MainHero.Name+�� $��0��$��1��$��Center=�$��14(�HP�� $��0��$��2��$��Center=�$��14(�MP�� $��1q�
���   �$��1��$��1��$��13��$	��100��._�-  'MahApps.Metro.Controls.MetroProgressBar  
���  ��$��Center=�$��1��$��1��
 � Minimum$
 0Q� � Maximum$	 100Q�$��13��$	��100��$
��Left=� � Value �� *#!PlayerData.MainHero.HealthPercent+$ OneWay=��� $��1q�
���   �$��1��$��2��$��13��$	��100��._�  
����  �$��Center=�$��1��$��2��$
 0Q�$	 100Q�$��13��$	��100��$
��Left=� �� *!PlayerData.MainHero.ManaPercent+$ OneWay=��� $��1��$��1q�
�������$��-1,0,0,0q�._�� 	 �� $��3* ��� $��2* �.+�� $��0��	 �� $��1* ��� $��1* ��� �� �� �� .+�� $��1��	 �� �� .+��� $��Bottom=�$��Center=�$��18(�$
��Bold"����� *&$SelectedBot.PlayerData.MainHero.Name+ �� FallbackValue$ Unknown���� $��1�� �� Orientation$ 
Horizontal=�$��Center=�.+��� $��1��$��14(����� *'%SelectedBot.PlayerData.MainHero.Level+$ 0���� $��1��$��14(�$��3,0,0,0q�lv.�� $��1��$��14(�$��7,0,0,0q����� *)'SelectedBot.PlayerData.MainHero.ClassId+$
 None���� $ 
Horizontal=�$��2��$��Center=�$	��Top=�.+�� .+��� $��1q�
���   �$��2��$��22��$	��150��._�  $��Center=�
��� ���$��2��$	��150��$��22�� �� *53SelectedBot.PlayerData.MainHero.CombatPointsPercent+$ OneWay=��� $ 
Horizontal=�$��2��$��Center=�.+��� $��Center=����� *.,SelectedBot.PlayerData.MainHero.CombatPoints+$ OneWay=�$ 0���� $��Center=�$��Center=�/�� $��Center=�$��Center=����� *1/SelectedBot.PlayerData.MainHero.MaxCombatPoints+$ OneWay=�$ 0��� $��10,0,0,0q�.+��� $��1q�
���   �$��2��$��22��$	��150��._�  $��Center=�
���<��$��2��$	��150��$��22�� �� */-SelectedBot.PlayerData.MainHero.HealthPercent+$ OneWay=��� $ 
Horizontal=�$��2��$��Center=�.+��� $��Center=����� *(&SelectedBot.PlayerData.MainHero.Health+$ OneWay=�$ 0���� $��Center=�$��Center=�/�� $��Center=�$��Center=����� *+)SelectedBot.PlayerData.MainHero.MaxHealth+$ OneWay=�$ 0��� $��10,0,0,0q�.+��� $��1q�
���   �$��2��$��22��$	��150��._�  $��Center=�
������$��2��$	��150��$��22�� �� *-+SelectedBot.PlayerData.MainHero.ManaPercent+$ OneWay=��� $ 
Horizontal=�$��2��$��Center=�.+��� $��Center=����� *&$SelectedBot.PlayerData.MainHero.Mana+$ OneWay=�$ 0���� $��Center=�$��Center=�/�� $��Center=�$��Center=����� *)'SelectedBot.PlayerData.MainHero.MaxMana+$ OneWay=�$ 0���� $��0��$��Center=�.+��� -   $	��350��$��40��
�������$��18(����� *SelectedBot.Engine.Running+  stringConverter �� 	Converter# [ $ Start������ *SelectedBot.Engine.Running+  brushConverter# [ $ 	DarkGreen���� -   $	��300��$��30��$��5��$��14(�$	��Top=�.��CONFIGURATION�� -   $��versionLabel��$��5��$��Bottom=�$��Center=�$��16(��� -    ��Name$ Scroller��$��1��.���� -    ��Name$ 
textBoxLog��$
��Left=�$��Stretch=�$��14(� �� 
IsReadOnly .  �� 
LineHeight$ 1��