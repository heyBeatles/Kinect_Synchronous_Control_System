   M S B A M L   `   `   `  ����  DepthBasics-WPF5    /Microsoft.Samples.Kinect.DepthBasics.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6F   -   5          � Title$(  "Kinect三维点云校准-客户端��5   	   $��614.61��5   	   $	��700��6   ���	#FFF0F0F0 � 	Resources 5         MediumGreyBrush �� Color  KinectPurpleBrush  KinectBlueBrush �� 
TargetType �� Property �� Value  ScreenshotButton �� 
TargetType �� Orientation ��Name	 �� TextAlignment
 ��Name �� Triggers P� Property P� Value �� 
TargetName��%Q  &	        &	 #     &	 F     '�� Y     &	 �     7  7  �� 5	   
   $ 	#ff6e6e6e��62   �� 5
   
   $ 	#ff52318f��64   �� 5   
   6   �� 5   
   " ��6   .�5      ��  �@J�SnapsToDevicePixels . 64   5      �� 5   
   " ��6   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� $ 
Horizontal=�6)   
������ .+�5   "   �� $ ScreenshotText��6,   #��z��6D   $
	 Left=�6e   $��Center=�6{   8��  $��15(�6�   � 5   "   $��9,0,0,0q�6'   .+�5   &   �� $
 ScreenshotNormal��6,   $!��Images\ScreenshotNormal.png��6F   $
��None=�6k   $��Center=�6z   �� 5   &   $
 ScreenshotHover��6,   $ ��Images\ScreenshotHover.png��6E   $
��None=�6i   $��Center=�6x   $y�	Collapsed=�6�   5   #   5      5       5      P� 5       �@J�IsMouseOver . . �5   "   �� $ ScreenshotNormal��6Q    � y�$ 	Collapsed=�6?   6Q   �� 5   "   $ ScreenshotHover��6O    � y�$ Visible=�6?   6O   �� 5    "   8  $ ScreenshotText��6h    � ��5!      5#      5%      5&      5'      .��5(      � $��-18,-1,-264.4,10.4q�6   $	��561��6'   $��Bottom=�64   $��Visible=�6O    � ColumnDefinitions 5)   
   �� 5*      $
��335* �6   �� 5+      $	��68* �6   �� 5,      $
��573* �6    � RowDefinitions 5.   
   �� 5/      $
��Auto �6   �� 50      $��* �6   �� 51      $
��Auto �6   �� 52      $
��Auto �6   .+�55   
   (� $��1��6   $��Center=�6   $��3��6<   $��294,0,294,10q�6P   $	��386��6f   $��2��6r   .��56      �� $��UniformToFill=�63   $	��100��6K   $	��114��6X   ��6   �� *ImageSource+6X   57      �� 58   
   -   5         ��Name$ txtmsg��58       �� 
IsReadOnly . " �� VerticalScrollBarVisibility$
 Auto=�65    �� TextWrapping$
 Wrap=�6X    �� AcceptsReturn . ���	#FF8DA3AE$	��139��6�   $	��Top=�6�   $��38,25,294,0q�6  $��3��6  $��3��6/  ��6l   �� *test+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�.��6<   J� InputBindings 59      �� 5:      
 �� Key$ Return��6    �� 	Modifiers$ Shiftf�6*   �� 5;      $ Return��6    �� Command 6*   �� *	command+.��5<   %   5=      �� 5>   
   ���	#FF52318F$��1q�6)   $
��Left=�6=   $��1��6X   $	��Top=�6c   $	��644��6{   ���	#FF52318F J� OpacityMask
 �   �$��	36,11,0,0q�6�   $��3��6�   $��3��6�   �� 5?   
   -   5         ��Name$ getBasic��5?      $��手动采集��6#   $
��Left=�62   $��38,256,0,75q�6M   $��1��6b   $	��100��6o   6{   �� 5@   
   -   5         ��Name$ 
findkinect��5@      $��提示文本��6$   $
��Left=�63   $��
36,177,0,0q�6N   $	��Top=�6b   ���	#FF52318F���	#FF52318F$
��Bold"�6�   $��10(�6�   $��3��6�   $��3��6�   $	��540��6�   �� 5A   
   -   5        $ setRange��5A      $��设置范围��6#   $
��Left=�62   $��168,256,0,0q�6M   $��1��6b   $	��Top=�6o   $	��100��6�   $��30��6�    J� RenderTransformOrigin$ 0.575,1.383&�6�   �� 5B   
   $��连接主控IP:��6   $
��Left=�6"   $	��Top=�6=   $��	37,12,0,0q�6U   $��2��6h   �� 5C   
   $��连接主控端口:��6   $
��Left=�6"   $	��Top=�6=   $��	36,57,0,0q�6U   $��2��6h   $ 
0.506,1.36&�6y   �� 5D   
   -   5        $ 
T_IPAdress��5D      $
��Left=�6&   $��25��6A   $
 Wrap=�6M   $��192.168.3.18��6a   $	��Top=�6u   $	��114��6�   $��
155,14,0,0q�6�   $��2��6�   $ 
0.494,-0.5&�6�   �� 5E   
   -   5        $ T_Port��5E      $
��Left=�6"   $��25��6=   $
 Wrap=�6I   $
��5000��6]   $	��Top=�6i   $	��115��6�   $��
154,57,0,0q�6�   $��2��6�   �� 5F   
   $!��当前采集设备名称：��6   $
��Left=�6$   $	��Top=�6?   $��
38,146,0,0q�6W   $��2��6k   $ 
0.483,1.88&�6|   �� 5G   
   -   5          ��Name$  currentCollectot��5G      $
��Left=�6-   $	��Top=�6H   $	��114��6`   $��156,146,0,0q�6l   $��25��6�   $��2��6�   ! �� SelectedIndex$! 0��6�   .@�5H      �� $��	上KINECT��6   �� 5I      $��	左KINECT��6   �� 5J      $��	右KINECT��6   5K      �� 5L   
   -	   5        $ L_Information��5L      $��待连接...��6'   $
��Left=�68   $	��Top=�6S   $��
36,326,0,0q�6k   $��2��6   $	��232��6�   �� 5M   
   -
   5        $ connectMain��5M      $��连接主控��6&   $
��Left=�65   $	��Top=�6P   $	��100��6h   $��168,195,0,0q�6t   $��2��6�   $��30��6�   �� 5N   
   -   5        $ clearBtn��5N      $��清空��6#   $
��Left=�60   $	��Top=�6K   $��83��6c   $��2��6n   $��179,177,0,0q�6~   $��3��6�   $��21��6�   �� 5O   
   -   5        $��手动传输��5O      $
��Left=�6    $	��Top=�6;   $	��100��6S   $��
38,195,0,0q�6_   $ 1.235,0.625&�6�   $��2��6�   $��30��6�   �� 5P   
   $��连接视频端口：��6   $
��Left=�6"   $��	38,99,0,0q�6=   $��1��6P   $	��Top=�6]   �� 5Q   
   -   5        $ textBox2��5Q      $
��Left=�6$   $��23��6?   $��
154,99,0,0q�6K   $��1��6_   $
 Wrap=�6l   $
��5001��6�   $	��Top=�6�   $	��115��6�   {�. 6�   5R      5S      