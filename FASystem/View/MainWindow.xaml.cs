using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using FASystem.Model;

namespace FASystem
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinect;
        private FrameDescription colorFrameDescription;
        private ColorImageFormat colorImageFormat;
        private ColorFrameReader colorFrameReader;

        public TrainingInfo TrainingInfo { get; set; }
        //private List<TrainingInfo> trainingList;

        public MainWindow()
        {
            InitializeComponent();

            // Init Kinect
            this.kinect = KinectSensor.GetDefault();

            this.colorImageFormat = ColorImageFormat.Bgra;
            this.colorFrameDescription
                = this.kinect.ColorFrameSource.CreateFrameDescription(this.colorImageFormat);
            this.colorFrameReader = this.kinect.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;

            this.kinect.Open();

            // this.trainingList = new List<TrainingInfo>();

        }

        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
         
            ColorFrame colorFrame = e.FrameReference.AcquireFrame();

            // フレームが上手く取得出来ない場合がある。
            if (colorFrame == null) return;

            //画素情報を確保するバッファを用意する。
            //高さ * 幅 * 画素あたりのデータ量だけ保存できれば良い
            byte[] colors = new byte[this.colorFrameDescription.Width
                                     * this.colorFrameDescription.Height
                                     * this.colorFrameDescription.BytesPerPixel];

            colorFrame.CopyConvertedFrameDataToArray(colors, this.colorImageFormat);

            //用意した領域に画素情報を複製する
            BitmapSource bitmapSource = BitmapSource.Create(this.colorFrameDescription.Width,
                                                            this.colorFrameDescription.Height,
                                                            96,
                                                            96,
                                                            PixelFormats.Bgra32,
                                                            null,
                                                            colors,
                                                            this.colorFrameDescription.Width * (int)this.colorFrameDescription.BytesPerPixel);

            //キャンバスに表示する
            this.cameraCanvas.Background = new ImageBrush(bitmapSource);

            //取得したフレームを放棄する
            colorFrame.Dispose();
        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //カラー画像の取得を中止して、関連するリソースを放棄する
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            //Kinectを停止いて、関連するリソースを放棄する。
            if (this.kinect != null)
            {
                this.kinect.Close();
                this.kinect = null;
            }
        }


        private void showSettingWindow()
        {
            Window settingWindow = new SettingWindow();
            settingWindow.Show();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (TrainingInfo == null)
            {
                this.showSettingWindow();
            }
        }

        public void initTeachChart()
        {
            Console.WriteLine(this.TrainingInfo.TrainingName);
            Console.WriteLine(this.TrainingInfo.RangeTrackingTargets.Count);
            this.ChartLeft.DataContext = this.TrainingInfo.RangeTrackingTargets.First().generateBindingGraphCollection();
        }
    }
}
