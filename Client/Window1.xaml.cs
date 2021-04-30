using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.Samples.Kinect.DepthBasics
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            string path = "C:\\KinectData\\rangeData.txt";
            StreamReader sr = new StreamReader(path, System.Text.Encoding.Default);
            String line;
            for (int i = 0; i < 3; i++)
            {
                line = sr.ReadLine();
                string[] temp = line.Split(' ');
                if (i == 0)
                {
                    txtBX1.Text = temp[0];
                    txtBX2.Text = temp[1];
                    txtBY1.Text = temp[2];
                    txtBY2.Text = temp[3];
                    txtBZ1.Text = temp[4];
                    txtBZ2.Text = temp[5];
                }
                else if (i == 1)
                {
                    txtLX1.Text = temp[0];
                    txtLX2.Text = temp[1];
                    txtLY1.Text = temp[2];
                    txtLY2.Text = temp[3];
                    txtLZ1.Text = temp[4];
                    txtLZ2.Text = temp[5];
                }

                else
                {
                    txtRX1.Text = temp[0];
                    txtRX2.Text = temp[1];
                    txtRY1.Text = temp[2];
                    txtRY2.Text = temp[3];
                    txtRZ1.Text = temp[4];
                    txtRZ2.Text = temp[5];
                }
  
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            string path = "C:\\KinectData\\rangeData.txt";
            if (!(string.IsNullOrEmpty(txtBX1.Text) || string.IsNullOrEmpty(txtBY1.Text) || string.IsNullOrEmpty(txtBZ1.Text) ||
                string.IsNullOrEmpty(txtLX1.Text) || string.IsNullOrEmpty(txtLY1.Text) || string.IsNullOrEmpty(txtLZ1.Text) ||
                    string.IsNullOrEmpty(txtRX1.Text) || string.IsNullOrEmpty(txtRY1.Text) || string.IsNullOrEmpty(txtRZ1.Text) ||
                    string.IsNullOrEmpty(txtBX2.Text) || string.IsNullOrEmpty(txtBY2.Text) || string.IsNullOrEmpty(txtBZ2.Text) ||
                string.IsNullOrEmpty(txtLX2.Text) || string.IsNullOrEmpty(txtLY2.Text) || string.IsNullOrEmpty(txtLZ2.Text) ||
                    string.IsNullOrEmpty(txtRX2.Text) || string.IsNullOrEmpty(txtRY2.Text) || string.IsNullOrEmpty(txtRZ2.Text)))
            {

                string content = txtBX1.Text + " " + txtBX2.Text + " " + txtBY1.Text + " " + txtBY2.Text + " " + txtBZ1.Text + " " + txtBZ2.Text + "\r\n" +
                    txtLX1.Text + " " + txtLX2.Text + " " + txtLY1.Text + " " + txtLY2.Text + " " + txtLZ1.Text + " " + txtLZ2.Text + "\r\n" +
                    txtRX1.Text + " " + txtRX2.Text + " " + txtRY1.Text + " " + txtRY2.Text + " " + txtRZ1.Text + " " + txtRZ2.Text + "\r\n";
                FileStream fs = new FileStream(path, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(content);
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();

                MessageBox.Show("保存成功", "提示", MessageBoxButton.OKCancel);
            }
            else
            {
                MessageBox.Show("保存失败，请检查是否将所有摄像区域信息填写完毕", "提示", MessageBoxButton.OKCancel);
            }
        }
    }
}
