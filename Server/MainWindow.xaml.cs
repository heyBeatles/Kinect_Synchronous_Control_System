//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Microsoft.Kinect;

    public partial class MainWindow : Window
    {

        private KinectSensor kinectSensor = null;

        private ColorFrameReader colorFrameReader = null;

        //以下分别对应三个窗口的彩色位图，只需写方法传以下三组值过来即可
        //其中BasicColorBitmap作为对照组，基准面可通过摄像头来获取位图，以确保程序本身可通过位图显示图像
        //做传输实验时，应以左右两个摄像头的位图为基准，即传左右摄像头位图过来本程序
        //界面所对应摄像头分别为，上左为基准摄像头，上右为左面摄像头，下左为右面摄像头
        private WriteableBitmap BasicColorBitmap = null;
        private WriteableBitmap LeftColorBitmap = null;
        private WriteableBitmap RightColorBitmap = null;

        public MainWindow()
        {
            delete("5000");
            delete("5001");
            delete("6666");
            //以下代码为基准摄像头获取Kinect位图语句
            this.kinectSensor = KinectSensor.GetDefault();
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.BasicColorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.kinectSensor.Open();

            //关键代码，不可删去
            this.DataContext = this;

            //初始化窗口
            this.InitializeComponent();


            //确认保存kinect数据的文件夹是否存在，不存在则提前生成
            if (!Directory.Exists(upPath))
            {
                Directory.CreateDirectory(upPath);
            }
            if (!Directory.Exists(rightPath))
            {
                Directory.CreateDirectory(rightPath);
            }
            if (!Directory.Exists(leftPath))
            {
                Directory.CreateDirectory(leftPath);
            }
            if (!Directory.Exists(upHeadPath))
            {
                Directory.CreateDirectory(upHeadPath);
            }
            if (!Directory.Exists(rightHeadPath))
            {
                Directory.CreateDirectory(rightHeadPath);
            }
            if (!Directory.Exists(leftHeadPath))
            {
                Directory.CreateDirectory(leftHeadPath);
            }
        }

        public ImageSource ImageSourceBasic
        {
            get
            {
                return this.BasicColorBitmap;
            }
        }
        public ImageSource ImageSourceLeft
        {
            get
            {
                return this.LeftColorBitmap;
            }
        }
        public ImageSource ImageSourceRight
        {
            get
            {
                return this.RightColorBitmap;
            }
        }

        //彩色帧到达，后序实现了左右摄像头的视频传输，可将此方法删去
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
                        this.BasicColorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this.BasicColorBitmap.PixelWidth) && (colorFrameDescription.Height == this.BasicColorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this.BasicColorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this.BasicColorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.BasicColorBitmap.PixelWidth, this.BasicColorBitmap.PixelHeight));
                        }

                        this.BasicColorBitmap.Unlock();
                    }
                }
            }
        }

        //将matlab生成工具包导入，在以下方法体中处理
        //所生成值需要显示在窗口的，可直接查看xaml文档，对应文本框皆有命名

        //三维显示按钮处理方法
        private void Correct_Click(object sender, RoutedEventArgs e)
        {

        }

        //一键校准按钮处理方法
        private void ViewPoint_Click(object sender, RoutedEventArgs e)
        {

        }

        //计算参数按钮处理方法
        private void Caculate_Click(object sender, RoutedEventArgs e)
        {

        }






        //以下为主控通信部分的代码
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        ////以下为主控通信部分的代码


        private string upPath = "C:\\KinectData\\PointData\\BasicPoint";
        private string leftPath = "C:\\KinectData\\PointData\\LeftPoint";
        private string rightPath  = "C:\\KinectData\\PointData\\RightPoint";
        private string upHeadPath = "C:\\KinectData\\PointData\\BasicHeadPoint";
        private string leftHeadPath = "C:\\KinectData\\PointData\\LeftHeadPoint";
        private string rightHeadPath = "C:\\KinectData\\PointData\\RightHeadPoint";


        private static byte[] message = new byte[1024];

        private Socket serverSocket;

        private Socket clientSocket;

        private IPAddress initIP;

        public int initPort;

        private List<socketUser> listSocket = new List<socketUser>();

        private int length = 0;

        private string filename = "hhh";


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //初始化主控设备的IP地址和监听的端口
            InitIPandPort();
            //创建监听socket对象并启动监听线程
            InitNetWork();
            //开始监听文件接收的线程
            init();
        }


        /// <summary>
        /// 初始化主控设备的IP地址和监听的端口
        /// </summary>
        private void InitIPandPort()
        {
            string hostName = Dns.GetHostName();
            this.servername.Content = "主控设备" + hostName;
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            foreach (IPAddress ipaddress in hostEntry.AddressList)
            {
                if (ipaddress.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    this.ipAddTextbox.Text = ipaddress.ToString();
                    break;
                }
            }
            this.initIP = IPAddress.Parse(this.ipAddTextbox.Text.ToString().Trim());
            //this.initPort = Convert.ToInt32(this.duankou.Text.ToString().Trim());
            this.initPort = 5000;
            if (initPort != -1)
            {
                portTextbox.Text = initPort.ToString();
            }
            else
            {
                portTextbox.Text = "无可用端口,请检查";
            }
        }



        //以下三个函数GetFirstAvailablePort、PortIsUsed()、PortIsAvailable仅为了获取一个空闲的可用端口号，可略过不看


        /// <summary>
        /// 获取第一个可用的端口号
        /// </summary>
        /// <returns></returns>
        public static int GetFirstAvailablePort()
        {
            int MAX_PORT = 6000; //系统tcp/udp端口数最大是65535            
            int BEGIN_PORT = 5000;//从这个端口开始检测

            for (int i = BEGIN_PORT; i < MAX_PORT; i++)
            {
                if (PortIsAvailable(i)) return i;
            }

            return -1;
        }

        /// <summary>
        /// 获取操作系统已用的端口号
        /// </summary>
        /// <returns></returns>
        public static IList PortIsUsed()
        {
            //获取本地计算机的网络连接和通信统计数据的信息
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            //返回本地计算机上的所有Tcp监听程序
            IPEndPoint[] ipsTCP = ipGlobalProperties.GetActiveTcpListeners();

            //返回本地计算机上的所有UDP监听程序
            IPEndPoint[] ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

            //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            IList allPorts = new ArrayList();
            foreach (IPEndPoint ep in ipsTCP) allPorts.Add(ep.Port);
            foreach (IPEndPoint ep in ipsUDP) allPorts.Add(ep.Port);
            foreach (TcpConnectionInformation conn in tcpConnInfoArray) allPorts.Add(conn.LocalEndPoint.Port);

            return allPorts;
        }

        /// <summary>
        /// 检查指定端口是否已用
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool PortIsAvailable(int port)
        {
            bool isAvailable = true;

            IList portUsed = PortIsUsed();

            foreach (int p in portUsed)
            {
                if (p == port)
                {
                    isAvailable = false; break;
                }
            }

            return isAvailable;
        }

        /// <summary>
        /// 创建监听socket对象并启动监听线程
        /// </summary>
        private void InitNetWork()
        {
            IPEndPoint localEP = new IPEndPoint(this.initIP, this.initPort);
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.serverSocket.Bind(localEP);
            this.stateInfo.Content = "监听中...";
            this.serverSocket.Listen(10);
            this.stateInfo.Content = this.listSocket.Count + " 个连接.";


            //移植过来后会报错，不明原因暂不处理
            //Control.CheckForIllegalCrossThreadCalls = false;




            new Thread(new ThreadStart(this.ListeningClientConnect))
            {
                IsBackground = true
            }.Start();
        }




        private void B_SendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendCollectOrder("doCollect");
        }


        /// <summary>
        /// 1、接收到客户端的连接，用 socket 对像的 Accept() 方
        ///    法创建一个新的用于和客户端进行通信的 socket 对像clientSocket
        /// 2、将socket对像clientSocket加入集合listSocket中
        /// 3、更新listview将新客户加入显示
        /// 4、更新连接数label显示信息，对话框添加连接信息
        /// 5、通过新的socket向新连接发送打招呼信息
        /// 6、创建新线程实时接受新客户socket发来的信息并显示到对话框中
        /// </summary>
        private void ListeningClientConnect()
        {
            for (; ; )
            {
                this.clientSocket = this.serverSocket.Accept();




                clientSocket.SetSocketOption(SocketOptionLevel.Tcp,SocketOptionName.NoDelay,true);
                socketUser user = new socketUser(clientSocket);
                this.listSocket.Add(user);
                //this.listSocket.Add(this.clientSocket);



                this.stateInfo.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this.stateInfo.Content = this.listSocket.Count + " 个连接."));
                //this.clientSocket.Send(Encoding.UTF8.GetBytes("你好，我是服务器！"));
                user.bw.Write("你好，我是服务器！来自CHARBW");
                new Thread(new ParameterizedThreadStart(this.ReceiveMessage))
                {
                    IsBackground = true
                }.Start(user);
            }
        }

        

        private void SendCollectOrder(string collectType)
        {
            if (this.listSocket.Count == 0)
            {
                //MessageBox.Show("没有连接采集设备！", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                MessageBox.Show("没有连接采集设备！");
            }
            else
            {
                //for (int i = 0; i < this.L_ClientList.Items.Count; i++)
                for (int i = 0; i < listSocket.Count; i++)
                {
                    try
                    {
                        //listSocket[i].Send(Encoding.UTF8.GetBytes(collectType));
                        listSocket[i].bw.Write(collectType);
                        listSocket[i].bw.Flush();
                        this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += listSocket[i].remoteClient.ToString()+"发送"+collectType+"消息类型的时间为:"+DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff")+"\r\n"; }));
                    }
                    catch (Exception ex)
                    {
                        this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += ex.ToString() + "\r\n"; }));
                        //txtmsg.Text += ex.ToString() + "\r\n";
                        //listSocket[i].Shutdown(SocketShutdown.Both);
                        //listSocket[i].Close();
                        listSocket[i].Close();
                    }
                }
            }
        }




        /// <summary>
        /// 与某一客户socket通信的线程方法
        /// 1、将receive方法接收到的信息存储到缓冲数组message中
        /// 2、对字节信息进行解码并显示到相应textbox对话框中
        /// 3、如果客户端断开，则receive方法抛出异常，并将相应
        ///    异常信息更新显示到textbox和状态label中
        /// </summary>
        /// <param name="clientSocket"></param>
        private void ReceiveMessage(object newAcceptedUser)
        {
            socketUser user = (socketUser)newAcceptedUser;
            //Socket socket = (Socket)clientSocket;
            for (; ; )
            {
                try
                {

                    //int count = socket.Receive(MainWindow.message);
                    string message = user.br.ReadString();
                    string collecorEndPoint = Convert.ToString(user.remoteClient.RemoteEndPoint.ToString());
                    //string message = Encoding.UTF8.GetString(MainWindow.message, 0, count);
                    string[] messageInfo = message.Split(new char[] { '|' });
                    if (messageInfo.Length > 1)
                    {
                        switch (messageInfo[0])
                        {
                            case "iWantCollect":
                                txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text += messageInfo[1] + "(" + collecorEndPoint + ")向该主控发起采集确认\r\n"));
                                txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text += "主控设别向三台采集设备发起采集命令\r\n"));
                                SendCollectOrder("doCollect");
                                break;
                            case "SendFile":
                                string currentCollector = messageInfo[3];
                                filename = currentCollector + Path.GetFileName(messageInfo[1]);
                                length = Convert.ToInt32(messageInfo[2]);
                                txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text += "提前接收文件描述信息:来自[" + currentCollector + "],文件名[" + filename + "]，大小[" + length + "]字节" + "\r\n"));
                                break;
                            case "Login":
                                this.L_ClientList.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this.L_ClientList.Items.Add(Convert.ToString("[" + messageInfo[2] + "]:" + messageInfo[1]))));
                                this.txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text = this.listSocket.Count + " 个连接来自[" + messageInfo[2] + "]:" + messageInfo[1] + "\r\n"));
                                break;
                            case "iWantCollectFace":
                                txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text += messageInfo[1] + "(" + collecorEndPoint + ")向该主控发起抓脸采集确认\r\n"));
                                txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text += "主控设别向三台采集设备发起抓脸采集命令\r\n"));
                                SendCollectOrder("doCollectFace");
                                break;
                            default:
                                break;
                        }
                    }
                    else if (message != "")
                    {

                        this.txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text += "未知格式消息：" + message + "hhh"));
                        //txtmsg.Text += "未知格式消息：" + message;
                    }

                }
                catch (Exception ex)
                {
                    this.txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text += user.remoteClient.ToString()));
                    this.txtmsg.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => txtmsg.Text += " 断开连接...\r\n" + ex.ToString()));
                    this.listSocket.Remove(user);

                    for (int i = 0; i < L_ClientList.Items.Count; i++)
                    {
                        string str = "";
                        this.L_ClientList.Dispatcher.Invoke(new Action(() => { str = L_ClientList.Items[i].ToString(); }));
                        if (str.Contains(user.remoteClient.ToString()))
                            this.L_ClientList.Dispatcher.Invoke(new Action(() => { L_ClientList.Items.RemoveAt(i); }));

                    }
                    this.stateInfo.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => this.stateInfo.Content = this.listSocket.Count + " 个连接."));
                    //socket.Shutdown(SocketShutdown.Both);
                    //socket.Close();
                    user.Close();
                    break;
                }
            }
        }



        private void ClearBtn_Click_1(object sender, RoutedEventArgs e)
        {
            txtmsg.Text = "";
        }



        //以上为主控通信部分的代码
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //以上为主控通信部分的代码



        //以下为视频传输部分的代码
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        ////以下为视频传输部分的代码


        int ReceiveLength;//一帧图片的长度
        Thread threadListen;//接受连接请求
        Thread thredRece;//接收视频信息
        int i = 0;
        int port ;

        private void init()
        {
            try
            {
                TcpListener listener = new TcpListener(initIP, 5001);
                listener.Start();

                threadListen = new Thread(ReceiveConnect);
                threadListen.Start(listener);
                threadListen.IsBackground = true;

                TcpListener Filelistener = new TcpListener(initIP, 6666);
                Filelistener.Start();

                threadListen = new Thread(ReceiveConnect);
                threadListen.Start(Filelistener);
                threadListen.IsBackground = true;

            }
            catch (Exception ex)
            {

            }
        }

        private void ReceiveConnect(object obj)//监听连接函数 等候客户端的连接
        {
            TcpListener listener = obj as TcpListener;
            bool isExit = false;//是否停止接收
            while (!isExit)
            {
                TcpClient remoteClient = listener.AcceptTcpClient(); 
                string message = remoteClient.Client.RemoteEndPoint.ToString();
                    
                isExit = false;
                User user = new User(remoteClient);
                Console.WriteLine(remoteClient.ToString());
                thredRece = new Thread(new ParameterizedThreadStart(ReceiveMessages));//新建线程接收图片
                thredRece.IsBackground = true;
                thredRece.Start(user);
            }
        }

        private void ReceiveMessages(object obj)
        {
            User user = obj as User;
            MemoryStream receiveMS;//保存一帧图片
            bool isExit = false;//是否停止接收
            while (!isExit)
            {
                string receiveString = null;
                string[] splitString = null;
                byte[] receiveBytes = null;
                try
                {
                    receiveString = user.br.ReadString();                    
                    splitString = receiveString.Split(':');
                    if (splitString[1] == "true")
                    {
                        ReceiveLength = user.br.ReadInt32();
                        receiveBytes = user.br.ReadBytes(ReceiveLength);
                       

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    init();
                    break;
                }
                if (receiveString == null)
                {
                    if (isExit == false)
                    {
                        // this.textBox3.Invoke(setTextCallBack, "receive ERROR!");
  
                    }
                    break;
                }
                //this.textBox3.Invoke(setTextCallBack, "收到：" + receiveString);

                switch (splitString[0])
                {
                    case "Top":
                        try
                        {

                            BitmapImage bitmapImage = ByteArrayToBitmapImage(receiveBytes);
                            this.imageTop.Dispatcher.Invoke(new Action(() => { this.imageTop.Source = bitmapImage; }));
                        }
                        catch (Exception ez)
                        {
                            MessageBox.Show("Receive ERROR!");
                            Console.WriteLine(ez.ToString());
                            break;
                        }
                        break;

                    case "Left":
                        try
                        {
                            BitmapImage bitmapImage = ByteArrayToBitmapImage(receiveBytes);
                            this.imageLeft.Dispatcher.Invoke(new Action(() => { this.imageLeft.Source = bitmapImage; }));


                        }
                        catch (Exception ez)
                        {
                            MessageBox.Show("Receive ERROR!");
                            Console.WriteLine(ez.ToString());
                            break;
                        }
                        break;

                    case "Right":
                        try
                        {
                            BitmapImage bitmapImage = ByteArrayToBitmapImage(receiveBytes);
                            this.imageRight.Dispatcher.Invoke(new Action(() => { this.imageRight.Source = bitmapImage; }));

                        }
                        catch (Exception ez)
                        {
                            MessageBox.Show("Receive ERROR!");
                            Console.WriteLine(ez.ToString());
                            break;
                        }
                        break;

                    default:
                        {
                            this.txtmsg.Dispatcher.Invoke(new Action(() => { txtmsg.Text += "文件接收socket连接建立，开始启动线程接收大小为" + ReceiveLength + "的文件。" + "\r\n"; }));
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + "  " + receiveString);
                            string[] filename = splitString[0].Split(',');
                            string text = "";
                            string text2 = "";
                            if (filename[0] .Equals("上"))
                            {
                                if (filename[1].Equals("头"))
                                {
                                    text = upHeadPath + "\\B" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + ".txt";
                                    text2 = "C:\\KinectData\\TempData\\basicHeadPoint.txt";
                                }
                                else
                                {
                                    text = upPath + "\\B" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + ".txt";
                                    text2 = "C:\\KinectData\\TempData\\basicPoint.txt";
                                }
                            }
                            else if (filename[0].Equals("右"))
                            {
                                if (filename[1].Equals("头"))
                                {
                                    text = rightHeadPath + "\\R" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + ".txt";
                                    text2 = "C:\\KinectData\\TempData\\rightHeadPoint.txt";
                                }
                                else
                                {
                                    text = rightPath + "\\R" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + ".txt";
                                    text2 = "C:\\KinectData\\TempData\\rightPoint.txt";
                                }
                            }
                            else
                            {
                                if (filename[1].Equals("头"))
                                {
                                    text = leftHeadPath + "\\L" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + ".txt";
                                    text2 = "C:\\KinectData\\TempData\\leftHeadPoint.txt";
                                }
                                else
                                {
                                    text = leftPath + "\\L" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff") + ".txt";
                                    text2 = "C:\\KinectData\\TempData\\leftPoint.txt";
                                }
                            }
                            string str = System.Text.Encoding.Default.GetString(receiveBytes);
                            FileWrite(str, text);
                            FileWrite(str, text2);
                            this.txtmsg.Dispatcher.Invoke(new Action(() => {
                                txtmsg.Text += string.Concat(new object[]
            {
                            "文件( ",
                            this.length,
                            " )字节,成功保存到：",
                            text,
                            "\r\n"
            });
                            }));
                        }
                        break;

                }

            }

        }

        public static void FileWrite(string strs,string path)
        {
            Console.WriteLine(path);
            Console.WriteLine(strs);
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);       
            //获得字节数组
            byte[] data = System.Text.Encoding.Default.GetBytes(strs);
            //开始写入
            fs.Write(data, 1, data.Length-1);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }


        public static BitmapImage ByteArrayToBitmapImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }








        //以上为视频传输部分的代码
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //////★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        ////以上为视频传输部分的代码

        private void delete(string port)
        {
            Process pro = new Process();

            // 设置命令行、参数
            pro.StartInfo.FileName = "cmd.exe";
            pro.StartInfo.UseShellExecute = false;
            pro.StartInfo.RedirectStandardInput = true;
            pro.StartInfo.RedirectStandardOutput = true;
            pro.StartInfo.RedirectStandardError = true;
            pro.StartInfo.CreateNoWindow = true;
            // 启动CMD
            pro.Start();
            // 运行端口检查命令
            pro.StandardInput.WriteLine("netstat -ano");
            pro.StandardInput.WriteLine("exit");

            // 获取结果
            Regex reg = new Regex(@"\s ", RegexOptions.Compiled);
            string line = null;
            while ((line = pro.StandardOutput.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                {
                    line = reg.Replace(line, ",");

                    string[] arr = line.Split(',');

                    string[] arr1 = arr[2].Split(':');

                    if (arr[2].EndsWith(":" + port))
                    {

                        KillProcess(Int32.Parse(arr[arr.Length - 1]));
                    }

                }
            }
        }

        public void KillProcess(int processName) //调用方法，传参
        {
            try
            {
                //    //  Process[] thisproc = Process.GetProcessesByName(processName);
                Process thisproc = Process.GetProcessById(processName);
                if (!thisproc.CloseMainWindow()) //尝试关闭进程 释放资源
                {
                    thisproc.Kill(); //强制关闭
                }
                Console.WriteLine("进程 {0}关闭成功", processName);


            }
            catch //出现异常，表明 kill 进程失败
            {
                Console.WriteLine("结束进程{0}出错！", processName);
            }

        }

        private void PortTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void CollectFaceBtn_Click(object sender, RoutedEventArgs e)
        {
            SendCollectOrder("doCollectFace");
        }
    }
}
