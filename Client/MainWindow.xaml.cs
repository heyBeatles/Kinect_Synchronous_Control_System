//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using MathWorks.MATLAB.NET.Arrays;//系统dll文件  
    using UseAdjust;//自己生成的dll文件  
    using UseAdjustNative;
    using MathWorks.MATLAB.NET.Utility;
    using UseAdjustToShow;
    using UseAdjustToShowNative;
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;
    using System.Text;
    using System.Windows.Forms;
    using MessageBox = System.Windows.Forms.MessageBox;
    using System.Windows.Automation.Peers;
    using System.Windows.Threading;
    using System.Drawing;


    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private DepthFrameReader depthFrameReader = null; //深度源
        private ColorFrameReader colorFrameReader = null; //彩色源
        /// <summary>
        /// Description of the data contained in the depth frame
        /// </summary>
        private FrameDescription depthFrameDescription = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap depthBitmap = null; //深度位图
        private WriteableBitmap colorBitmap = null; //彩色位图
        private UInt16[] depthFrameData1 = new UInt16[512 * 424];
        bool isBasicClick = false;
        bool isLeftClick = false;
        bool isRightClick = false;
        bool sendM = false;

        double leftZ1 = 0.74;   //左面Z轴较摄像头近端点
        double leftZ2 = 1.2;    //左面Z轴较远端点
        double rightZ1 = 0.89;  //右面Z轴较近端点
        double rightZ2 = 1.32;  //右面Z轴较远端点
        double basicZ1 = 0;     //上面Z轴的较近端点
        double basicZ2 = 1.35;     //上面Z轴的较远端点
        double basicY1 = -0.25;   //上面Y轴的负方向端点
        double basicY2 = 0.25;   //上面Y轴的正方向端点
        double leftY1 = -0.4;   //左面Y轴的负方向端点
        double leftY2 = 0.4;   //左面Y轴的正方向端点
        double rightY1 = -0.4;   //右面Y轴的负方向端点
        double rightY2 = 0.4;   //右面Y轴的正方向端点
        double basicX1 = -0.5;   //上面X轴的负方向端点
        double basicX2 = 0.5;   //上面X轴的正方向端点
        double leftX1 = -0.5;   //左面X轴的负方向端点
        double leftX2 = 0.5;   //左面X轴的正方向端点
        double rightX1 = -0.5;   //右面X轴的负方向端点
        double rightX2 = 0.5;   //右面X轴的正方向端点
        //double leftAndRightHigh = 0.4;   //左面Y轴的高度
        //double halfWidth = 0.5;         //三个面X轴最大宽度的一半
        public bool catch_Time=true;
        public bool catch_head_Time = true;

        //string accuracyPath = "C:\\KinectData\\accuracy.txt";
        string basicPath = "C:\\KinectData\\TempData\\basicPoint.txt";
        string leftPath = "C:\\KinectData\\TempData\\leftPoint.txt";
        string rightPath = "C:\\KinectData\\TempData\\rightPoint.txt";
        string returnPath = "C:\\KinectData\\TempData\\returnData.txt";
        string rangePath = "C:\\KinectData\\rangeData.txt";
        string tempPath;


        private CameraSpacePoint[] cameraSpacePoints = new CameraSpacePoint[512 * 424];
        private CameraSpacePoint[] faceCameraSpacePoints = new CameraSpacePoint[512 * 424];
        private String timeText;
        private Boolean faceFlag = false;

        CameraSpacePoint[] highPoint = new CameraSpacePoint[512 * 424];
        public struct spacePoint
        {
            public double x;
            public double y;
            public double z;
        }

        //抓取函数
        public bool pointCloud_catch(CameraSpacePoint[] s)
        {
            bool b = false; int m = 300;
            int flagL = 0, flagR = 0, flagM = 0;
            
            for (int i = 1; i < 512 * 424; ++i)
            {
                if (s[i].Y >= -0.36 && s[i].Y <= 0.36 && s[i].Z < 1.5)
                {
                    if (s[i].X <= 0.01 && s[i].X >= -0.01) flagM++;
                    if (s[i].X <= 0.46 && s[i].X >= 0.44) flagL++;
                    if (s[i].X <= -0.44 && s[i].X >= -0.46) flagR++;
                }

            }
            if (flagL >= m && flagR >= m && flagM >= m && this.catch_Time && currentCollectot.Text == "上KINECT")
            {
                b = true;
                this.catch_Time = false;
            }
            if (flagL < m && flagR < m && flagM < m && !(this.catch_Time) && currentCollectot.Text == "上KINECT")
            {
                this.catch_Time = true;
            }

            return b;
        }
        public bool pointCloud_catch_head(CameraSpacePoint[] s)
        {
            bool b = false; int m = 300;
            int flagL = 0, flagR = 0, flagM = 0;

            for (int i = 1; i < 512 * 424; ++i)
            {
                if (s[i].Y >= -0.36 && s[i].Y <= 0.36 && s[i].Z < 1.5)
                {
                    if (s[i].X <= 0.01 && s[i].X >= -0.01) flagM++;
                    if (s[i].X <= 0.46 && s[i].X >= 0.44) flagL++;
                    if (s[i].X <= -0.44 && s[i].X >= -0.46) flagR++;
                }

            }
            if (flagL >= m  &&flagR<m && flagM >= m && this.catch_head_Time && currentCollectot.Text == "上KINECT")
            {
                b = true;
                this.catch_head_Time = false;
            }
            if (flagL < m &&flagR<m && flagM < m && !(this.catch_head_Time) && currentCollectot.Text == "上KINECT")
            {
                this.catch_head_Time = true;
            }

            return b;
        }


        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private byte[] depthPixels = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            //用于初始化计时器的相关设置，用于实时检测canCollect信号是否为TRUE
            //为TRUE则开始抓拍，并将抓拍后生成的文件发往主控
            //计时器绑定的方法为dispatcherTimer_Tick
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            dispatcherTimer.Start();



            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the depth frames
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.depthFrameReader.FrameArrived += this.Reader_FrameArrived;
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
            //

            // get FrameDescription from DepthFrameSource
            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // allocate space to put the pixels being received and converted
            this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

            // create the bitmap to display
            this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.depthBitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }
            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.depthFrameReader != null)
            {
                // DepthFrameReader is IDisposable
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>

        /// <summary>
        /// Handles the depth frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {

                        depthFrame.CopyFrameDataToArray(depthFrameData1);
                        CoordinateMapper CorMap = kinectSensor.CoordinateMapper;
                        CorMap.MapDepthFrameToCameraSpace(depthFrameData1, cameraSpacePoints);



                        if (pointCloud_catch_head(cameraSpacePoints))
                        {
                            if (connected == true)
                            {
                                sendConfirmMessage("iWantCollectFace");
                            }
                            else
                            {
                                txtmsg.AppendText("可以抓取头部数据，但是未与服务端进行连接，请先建立连接！！！" + "\r\n");
                            }
                        }
                        else Console.WriteLine("未检测到猪脸可采！");


                        //上端KINECT检测到可以抓拍的时机后向主控发送确认信息，主控收到后会发送
                        //回响应信息将客户端的canCollect信号置一，客户端发现信号置一后即开始采集
                        //并在采集后将生成的TXT文件自动发送给主控，对于canCollect信号的处理请见
                        if (pointCloud_catch(cameraSpacePoints)||testSignal==true)
                        {
                            if (connected==true)
                            {
                                sendConfirmMessage("iWantCollect");
                                testSignal = false;

                            }
                            else
                            {
                                txtmsg.AppendText("可以抓取数据，但是未与服务端进行连接，请先建立连接！！！"  + "\r\n");
                                testSignal = false;
                            }
                        }
                      //  else Console.WriteLine("未检测到！");


                        // verify data and write the color data to the display bitmap
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)) &&
                            (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight))
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance




                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);



                            depthFrameProcessed = true;

                            if (flag)
                            {
                                MemoryStream ms = new MemoryStream();
                                BitmapImage bmp = ConvertWriteableBitmapToBitmapImage(depthBitmap);
                                Bitmap bitmap = BitmapImage2Bitmap(bmp);
                                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                byte[] arrImage = ms.GetBuffer();
                                send(arrImage,"null");
                            }
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                this.RenderDepthPixels();
            }
        }

        //彩色帧到达
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this.colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                        }

                        this.colorBitmap.Unlock();
                       
 
                            

                    }
                }
            }
        }

        //读取点云一帧的数据
        private void Reader_OnlyFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {

                        depthFrame.CopyFrameDataToArray(depthFrameData1);
                        CoordinateMapper CorMap = kinectSensor.CoordinateMapper;
                        CorMap.MapDepthFrameToCameraSpace(depthFrameData1, cameraSpacePoints);

                        


                        // verify data and write the color data to the display bitmap
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)) &&
                            (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight))
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance




                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);



                            depthFrameProcessed = true;
                        }
                    }

                    GetRange();   //获取读取区域

                    if (isLeftClick)
                    {
                        isLeftClick = false;
                        //将左配准面点云数据写入文件
                        System.DateTime currentTime = new System.DateTime();
                        currentTime = System.DateTime.Now;

                        String time = currentTime.Month.ToString() + '-' + currentTime.Day.ToString() + '-' +
                                currentTime.Hour.ToString() + "-" + currentTime.Minute.ToString() + "-" +
                                currentTime.Second.ToString();


                        string filePath = "C:\\KinectData\\PointData\\LeftPoint\\" + time + ".txt";

                        if (canCollectHead==true&&canCollect==false)
                        {
                            timeText = time;
                            faceCameraSpacePoints = cameraSpacePoints;
                            
                        }
                        else if (canCollectHead == false && canCollect == true)
                        {
                            write(filePath,cameraSpacePoints);
                            write("C:\\KinectData\\TempData\\leftPoint.txt",cameraSpacePoints);
                            txtmsg.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  "));
                            txtmsg.AppendText("左面点云获取成功，已保存至" + filePath + "\r\n \r\n");
                            sendFileWithPath(filePath);
                            canCollect = false;

                            string headFilePath = "C:\\KinectData\\PointData\\LeftHeadPoint\\";
                            if (!Directory.Exists(headFilePath))
                                Directory.CreateDirectory(headFilePath);
                            filePath = headFilePath + timeText + ".txt";
                            write(filePath,faceCameraSpacePoints);
                            write("C:\\KinectData\\TempData\\leftHeadPoint.txt", faceCameraSpacePoints);
                            sendFileWithPath(filePath);;
                        }
                     

                        if (IsVerticalScrollBarAtBottom)
                        {
                            this.txtmsg.ScrollToEnd();
                        }
                    }

                    if (isRightClick)
                    {
                        isRightClick = false;
                        //将右配准面点云数据写入文件
                        System.DateTime currentTime = new System.DateTime();
                        currentTime = System.DateTime.Now;

                        String time = currentTime.Month.ToString() + '-' + currentTime.Day.ToString() + '-' +
                                currentTime.Hour.ToString() + "-" + currentTime.Minute.ToString() + "-" +
                                currentTime.Second.ToString();
                        string filePath = "C:\\KinectData\\PointData\\RightPoint\\" + time + ".txt";

                        if (canCollectHead == true && canCollect == false)
                        {
                            timeText = time;
                            faceCameraSpacePoints = cameraSpacePoints;

                        }
                        else if (canCollectHead == false && canCollect == true)
                        {
                            write(filePath, cameraSpacePoints);
                            write("C:\\KinectData\\TempData\\rightPoint.txt", cameraSpacePoints);
                            txtmsg.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  "));
                            txtmsg.AppendText("右面点云获取成功，已保存至" + filePath + "\r\n \r\n");
                            sendFileWithPath(filePath);
                            canCollect = false;

                            string headFilePath = "C:\\KinectData\\PointData\\rightHeadPoint\\";
                            if (!Directory.Exists(headFilePath))
                                Directory.CreateDirectory(headFilePath);
                            filePath = headFilePath + timeText + ".txt";
                            write(filePath, faceCameraSpacePoints);
                            write("C:\\KinectData\\TempData\\rightHeadPoint.txt", faceCameraSpacePoints);
                            sendFileWithPath(filePath); ;
                        }
                        if (IsVerticalScrollBarAtBottom)
                        {
                            this.txtmsg.ScrollToEnd();
                        }
                    }
                    
                    if (isBasicClick)
                    {
                        isBasicClick = false;
                        //将基准面点云数据写入文件
                        System.DateTime currentTime = new System.DateTime();
                        currentTime = System.DateTime.Now;

                        String time = currentTime.Month.ToString() + '-' + currentTime.Day.ToString() + '-' +
                                currentTime.Hour.ToString() + "-" + currentTime.Minute.ToString() + "-" +
                                currentTime.Second.ToString();
                        string filePath = "C:\\KinectData\\PointData\\BasicPoint\\" + time + ".txt";
                        if (canCollectHead == true && canCollect == false)
                        {
                            timeText = time;
                            faceCameraSpacePoints = cameraSpacePoints;

                        }
                        else if (canCollectHead == false && canCollect == true)
                        {
                            write(filePath, cameraSpacePoints);
                            write("C:\\KinectData\\TempData\\basicPoint.txt", cameraSpacePoints);
                            txtmsg.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  "));
                            txtmsg.AppendText("基准点云获取成功，已保存至" + filePath + "\r\n \r\n");
                            sendFileWithPath(filePath);
                            canCollect = false;

                            string headFilePath = "C:\\KinectData\\PointData\\basicHeadPoint\\";
                            if (!Directory.Exists(headFilePath))
                                Directory.CreateDirectory(headFilePath);
                            filePath = headFilePath + timeText + ".txt";
                            write(filePath, faceCameraSpacePoints);
                            write("C:\\KinectData\\TempData\\basicHeadPoint.txt", faceCameraSpacePoints);
                            sendFileWithPath(filePath); ;
                        }

                        if (IsVerticalScrollBarAtBottom)
                        {
                            this.txtmsg.ScrollToEnd();
                        }


                    }
                }
            }

            if (depthFrameProcessed)
            {
                this.RenderDepthPixels();
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                // Console.WriteLine(depth);

                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);

            }
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /// 

        //kinect找不到的提示语
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            //txtmsg.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss  "));
            findkinect.Content = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
            //txtmsg.AppendText(" \r\n \r\n");
            if (IsVerticalScrollBarAtBottom)
            {
                this.txtmsg.ScrollToEnd();
            }
        }

        public bool IsVerticalScrollBarAtBottom
        {
            get
            {
                bool atBottom = false;

                this.txtmsg.Dispatcher.Invoke((Action)delegate
                {
                    //if (this.txtLog.VerticalScrollBarVisibility != ScrollBarVisibility.Visible)
                    //{
                    //    atBottom= true;
                    //    return;
                    //}
                    double dVer = this.txtmsg.VerticalOffset;       //获取竖直滚动条滚动位置
                    double dViewport = this.txtmsg.ViewportHeight;  //获取竖直可滚动内容高度
                    double dExtent = this.txtmsg.ExtentHeight;      //获取可视区域的高度

                    if (dVer + dViewport >= dExtent)
                    {
                        atBottom = true;
                    }
                    else
                    {
                        atBottom = false;
                    }
                });

                return atBottom;
            }
        }

        private void GetBasic(object sender, RoutedEventArgs e)
        {
            if (currentCollectot.Text == "上KINECT")
            {
                isBasicClick = true;
                
            }
            else if (currentCollectot.Text == "左KINECT")
            {
                isLeftClick = true;
                
            }
            else if (currentCollectot.Text == "右KINECT")
            {
                isRightClick = true;
                
            }
            else isLeftClick = true;
            this.depthFrameReader.FrameArrived += this.Reader_OnlyFrameArrived;

        }

        private void SetRange_Click(object sender, RoutedEventArgs e)
        {
            Window1 setRange = new Window1();
            setRange.Show();
        }

        private void GetRange()
        {
            string path = "C:\\KinectData\\rangeData.txt";
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Default);
            String line;
            for (int i = 0; i < 3; i++)
            {
                line = sr.ReadLine();
                string[] temp = line.Split(' ');
                if (i == 0)
                {
                    basicX1 = Convert.ToDouble(temp[0]);
                    basicX2 = Convert.ToDouble(temp[1]);
                    basicY1 = Convert.ToDouble(temp[2]);
                    basicY2 = Convert.ToDouble(temp[3]);
                    basicZ1 = Convert.ToDouble(temp[4]);
                    basicZ2 = Convert.ToDouble(temp[5]);
                }
                else if (i == 1)
                {
                    leftX1 = Convert.ToDouble(temp[0]);
                    leftX2 = Convert.ToDouble(temp[1]);
                    leftY1 = Convert.ToDouble(temp[2]);
                    leftY2 = Convert.ToDouble(temp[3]);
                    leftZ1 = Convert.ToDouble(temp[4]);
                    leftZ2 = Convert.ToDouble(temp[5]);
                }

                else
                {
                    rightX1 = Convert.ToDouble(temp[0]);
                    rightX2 = Convert.ToDouble(temp[1]);
                    rightY1 = Convert.ToDouble(temp[2]);
                    rightY2 = Convert.ToDouble(temp[3]);
                    rightZ1 = Convert.ToDouble(temp[4]);
                    rightZ2 = Convert.ToDouble(temp[5]);
                }

            }
            sr.Close();
        }






        //以下为客户端通信部分的代码
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        ////以下为客户端通信部分的代码


        private NetworkStream chatNS;
        private BinaryReader chatBR;
        private BinaryWriter chatBW;



        private bool canCollect = false;
        private bool connected = false;
        private Socket clientSocket;
        private IPAddress initIP;
        private int initPort;
        private Thread receiveThread;
        private static byte[] message = new byte[1024];
        private Boolean testSignal = false;
        private Boolean canCollectHead = false;

        //用于循环监听canCollect信号的计时器
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        



        private void SendMessage(string message)
        {
            try
            {
                //this.clientSocket.Send(Encoding.UTF8.GetBytes(message));
                chatBW.Write(message);
            }
            catch (Exception ex)
            {
                this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += ex.ToString(); }));
                this.clientSocket.Shutdown(SocketShutdown.Both);
                this.clientSocket.Close();
            }
        }



        private void InitNetWork()
        {
            /// 1、创建用于连接服务端的socket对象
            IPEndPoint remoteEP = new IPEndPoint(this.initIP, this.initPort);
            this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            /// while循环尝试连接服务端
            while (!this.connected)
            {
                try
                {
                    this.L_Information.Dispatcher.Invoke(new Action(() => { this.L_Information.Content = "连接中..."; }));
                    this.clientSocket.Connect(remoteEP);
                    this.L_Information.Dispatcher.Invoke(new Action(() => { this.L_Information.Content = "连接成功..."; }));


                    chatNS = new NetworkStream(clientSocket);
                    chatBW = new BinaryWriter(chatNS);
                    chatBR = new BinaryReader(chatNS);


                    this.connectMain.Dispatcher.Invoke(new Action(() => { connectMain.IsEnabled = false; }));
                    this.connected = true;
                    this.currentCollectot.Dispatcher.Invoke(new Action(() => { this.SendMessage("Login|" + clientSocket.LocalEndPoint.ToString() + "|" + currentCollectot.Text); }));
                }
                catch (Exception ex)
                {
                    this.L_Information.Dispatcher.Invoke(new Action(() => { this.L_Information.Content = "服务器已断开，尝试重新连接."; }));
                    //this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += ex.ToString(); }));
                }
                Thread.Sleep(1000);
            }
            /// 连接成功，启动接收服务端信息的线程
            this.receiveThread = new Thread(new ThreadStart(this.ReceiveMessage));
            this.receiveThread.IsBackground = true;
            this.receiveThread.Start();
        }


        

        private void ReceiveMessage()
        {
            bool flag = true;
            while (flag)
            {
                /// 使用socket的receive方法将接受字节存储到message缓冲区中
                try
                {
                    //int count = this.clientSocket.Receive(message);
                    /////将缓冲区字节编码并显示到对话框中
                    //string receiMessage = Encoding.UTF8.GetString(message, 0, count);
                    string receiMessage = chatBR.ReadString();
                    if (receiMessage == "doCollect")
                    {
                        this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "主控端命令：主控端发来采集命令，开始采集......(设置可采信号为TRUE)\r\n"; }));
                        this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "收集到采身体信号时间：" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + "\r\n"; }));
                        canCollect = true;
                    }else if (receiMessage == "doCollectFace")
                    {
                        this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "主控端命令：主控端发来采集头部命令，开始采集......(设置可采头部信号为TRUE)\r\n"; }));
                        canCollectHead = true;
                        this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "收集到采头信号时间："+DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff")+"\r\n"; }));
                    }
                    else
                    {
                        //this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "主控端通知：" + Encoding.UTF8.GetString(message, 0, count) + "\r\n"; }));
                        this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "主控端通知：" + receiMessage + "\r\n"; }));
                    }

                }
                /// 服务器断开时对抛出异常的处理
                catch (Exception ex)
                {
                    this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "服务器已关闭.\r\n尝试重新连接 ..." + "\r\n"; }));
                    this.clientSocket.Shutdown(SocketShutdown.Both);
                    flag = false;
                    this.connected = false;
                    this.InitNetWork();
                }
            }
        }

        private void ConnectMain_Click(object sender, RoutedEventArgs e)
        {
            if (currentCollectot.Text == "")
            {
                System.Windows.MessageBox.Show("请指定当前采集器名称！");
            }
            else
            {
                this.initIP = IPAddress.Parse(this.T_IPAdress.Text.ToString().Trim());
                this.initPort = Convert.ToInt32(this.T_Port.Text.ToString().Trim());
                Thread thread = new Thread(new ThreadStart(this.InitNetWork));
                thread.Start();
                init();
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            txtmsg.Text = "";
        }

        private void Btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            sendConfirmMessage("iWantCollect");
        }



        //参数为确认采集的类型，为"iWantCollectFace"表示确认可采集头部，
        //为"iWantCollect"时表示确认可采集全身
        private void sendConfirmMessage(string confirmType)
        {
            if (this.connected)
            {
                if (currentCollectot.Text == "")
                    System.Windows.MessageBox.Show("请指定当前采集器名称和点云文件所在目录");
                else
                {
                    string s = confirmType+"|" + currentCollectot.Text;
                    this.SendMessage(s);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("没有检测到要连接的主控设备...", "Notice");
            }
        }





        private void sendFileInfoBeforeSendFile(string text, long fileLength)
        {
            this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "本机提前发送文件描述信息到主控......" + "(" + Path.GetFileName(text) + "-大小" + fileLength + "字节)\r\n"; }));
            try
            {
                if (this.connected)
                {
                    string collectorName = "";
                    this.currentCollectot.Dispatcher.Invoke(new Action(() => { collectorName= currentCollectot.Text; }));
                    string s = "SendFile|" + text + "|" + fileLength + "|" + collectorName;
                    this.SendMessage(s);
                }
                else
                {
                    System.Windows.MessageBox.Show("没有检测到要连接的主控设备...", "Notice");
                    return;
                }
                //睡一下，确保主控已收到文件描述信息再发送文件
                Thread.Sleep(40);
            }
            catch (Exception ex)
            {
                this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "【异常】准备发送文件描述信息阶段出错: " + ex.ToString(); }));
            }
        }

        private void sendFile(string text, long fileLength)
        {
            try
            {
                using (FileStream fileStream = new FileStream(text, FileMode.Open))
                {
                    ;
                    byte[] array5 = new byte[20971520];
                    int num = fileStream.Read(array5, 0, array5.Length);
                    if (num > 0)
                    {
                        byte[] array6 = new byte[num + 1];
                        array6[0] = 2;
                        Buffer.BlockCopy(array5, 0, array6, 1, num);
                        send(array6, "test");
                        this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "发送完毕！(" + num + "字节)"+"\r\n"; }));
                    }
                }
            }
            catch (Exception ex)
            {
                this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "【异常】发送文件出错: " + ex.ToString()+"\r\n"; }));
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            txtmsg.Text = "";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (testSignal==false)
            {
                testSignal = true;
            }
            else
            {
                testSignal = false;
            }
        }



        private void sendFileWithPath(string path)
        {
            FileInfo info = new FileInfo(path);
            string absolutePath = info.FullName.ToString();
            //根据设备所属让其各自先睡一会避免文件发送堵塞
            if (currentCollectot.Text == "左KINECT")
            {
                Thread.Sleep(1000);
            }
            else if (currentCollectot.Text == "右KINECT")
            {
                Thread.Sleep(2000);
            }
            sendFileInfoBeforeSendFile(absolutePath, info.Length);
            sendFile(absolutePath, info.Length);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (canCollect==true ||canCollectHead == true)
            {
                string path = "";
                if (currentCollectot.Text == "上KINECT")
                {
                    isBasicClick = true;
                    this.depthFrameReader.FrameArrived += this.Reader_OnlyFrameArrived;
                }
                else if (currentCollectot.Text == "左KINECT")
                {
                    isLeftClick = true;
                    this.depthFrameReader.FrameArrived += this.Reader_OnlyFrameArrived;
                }
                else if (currentCollectot.Text == "右KINECT")
                {
                    isRightClick = true;
                    this.depthFrameReader.FrameArrived += this.Reader_OnlyFrameArrived;
                }
            }
        }





        //以上为客户端通信部分的代码
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //以上为客户端通信部分的代码


        // 以下为客户端视频传输代码
        // **************************************
        // **************************************
        // **************************************
        // **************************************
        // 以下为客户端视频传输代码
        private int Location;
        TcpClient LocalClient;
        BinaryReader localBR;
        BinaryWriter localBW;
        NetworkStream localNS;

        TcpClient FileClient;
        BinaryReader FileBR;
        BinaryWriter FileBW;
        NetworkStream FileNS;
        Boolean flag = false;

        private void init()
        {
            try
            {
                LocalClient = new TcpClient(this.T_IPAdress.Text.Trim(), Convert.ToInt32(this.textBox2.Text.Trim()));
                localNS = LocalClient.GetStream();
                localBW = new BinaryWriter(localNS);
                localBR = new BinaryReader(localNS);
                flag = true;

                FileClient = new TcpClient(this.T_IPAdress.Text.Trim(), 6666);
                FileNS = FileClient.GetStream();
                FileBW = new BinaryWriter(FileNS);
                FileBR = new BinaryReader(FileNS);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                flag = false;
            }
        }


        private void send(byte[] array, string name)
        {
            try
            {
                if (name.Equals("null"))
                {
                    if (currentCollectot.Text == "上KINECT")
                    {
                        localBW.Write("Top:true");
                        localBW.Write(array.Length);
                        localBW.Write(array);
                        localBW.Flush();
                    }
                    else if (currentCollectot.Text == "左KINECT")
                    {
                        localBW.Write("Left:true");
                        localBW.Write(array.Length);
                        localBW.Write(array);
                        localBW.Flush();
                    }
                    else if (currentCollectot.Text == "右KINECT")
                    {
                        localBW.Write("Right:true");
                        localBW.Write(array.Length);
                        localBW.Write(array);
                        localBW.Flush();
                    }
                }
                else
                {
                    if (canCollectHead==true)
                    {
                        if (currentCollectot.Text == "上KINECT")
                        {
                            Console.WriteLine("上KINECT");
                            FileBW.Write("上,头," + name + ":true");
                        }
                        else if (currentCollectot.Text == "左KINECT")
                        {
                            Console.WriteLine("左KINECT");
                            FileBW.Write("左,头," + name + ":true");
                        }
                        else if (currentCollectot.Text == "右KINECT")
                        {
                            Console.WriteLine("右KINECT");
                            FileBW.Write("右,头," + name + ":true");
                        }
                        canCollectHead = false;
                    }
                    else
                    {
                        if (currentCollectot.Text == "上KINECT")
                        {
                            Console.WriteLine("上KINECT");
                            FileBW.Write("上," + name + ":true");
                        }
                        else if (currentCollectot.Text == "左KINECT")
                        {
                            Console.WriteLine("左KINECT");
                            FileBW.Write("左," + name + ":true");
                        }
                        else if (currentCollectot.Text == "右KINECT")
                        {
                            Console.WriteLine("右KINECT");
                            FileBW.Write("右," + name + ":true");
                        }   
                    }
                    FileBW.Write(array.Length);
                    FileBW.Write(array);
                    FileBW.Flush();
                }
            }
            catch (Exception ez)
            {
                flag = false; 
                Console.WriteLine(ez.ToString());
                init();
            }
        }
        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            //using ()
            //{
            MemoryStream stream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wbm));
            ;
            encoder.Save(stream);
            bmImage.BeginInit();
            bmImage.CacheOption = BitmapCacheOption.OnLoad;
            bmImage.StreamSource = stream;
            bmImage.EndInit();
            bmImage.Freeze();
            // }

            return bmImage;
        }
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)

        {
            return new Bitmap(bitmapImage.StreamSource);
        }

        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
        {
           // MessageBox.Show("  begin ");
            byte[] bytearray = null;
            try
            {
                Stream smarket = bmp.StreamSource;
                if (smarket != null && smarket.Length > 0)
                {
                    //设置当前位置
                    smarket.Position = 0;

                    using (BinaryReader br = new BinaryReader(smarket))
                    {
                        bytearray = br.ReadBytes((int)smarket.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("   " + ex);
                Console.WriteLine(ex);
            }
            return bytearray;
        }

        private void CheckBox1_Checked_1(object sender, RoutedEventArgs e)
        {

        }

        private void CurrentCollectot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void txtmsg_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        // 以上为客户端视频传输代码
        // **************************************
        // **************************************
        // **************************************
        // **************************************
        // 以上为客户端视频传输代码

        private void write(string filePath, CameraSpacePoint[] cameraSpacePoints)
        {
            FileStream nfile = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(nfile);

            for (int i = 2; i < 512 * 424; i++)
            {
                if (cameraSpacePoints[i].X < leftX2 && cameraSpacePoints[i].X > leftX1 &&
                    cameraSpacePoints[i].Y < leftY2 && cameraSpacePoints[i].Y > leftY1 &&
                    cameraSpacePoints[i].Z < leftZ2 && cameraSpacePoints[i].Z > leftZ1)
                    sw.WriteLine(cameraSpacePoints[i].X + " " + cameraSpacePoints[i].Y + " " + cameraSpacePoints[i].Z);
            }
            sw.Close();
            nfile.Close();
        }
    }


}
