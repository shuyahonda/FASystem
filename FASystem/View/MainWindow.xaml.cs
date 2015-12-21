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
using System.Collections.ObjectModel;

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
        private BodyFrameReader bodyFrameReader;

        private Body[] bodies;

        public TrainingInfo TrainingInfo { get; set; }
        private ObservableCollection<GraphPoint> UserAngleCollection { get; set; }

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

            bodyFrameReader = kinect.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            this.kinect.Open();

            this.UserAngleCollection = new ObservableCollection<GraphPoint>();
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

        /* フレームが来る度に呼び出される.距離の単位はメートル.*/
        private void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                {
                    return;
                }
                // ボディデータを取得する

                bodyFrame.GetAndRefreshBodyData(bodies);

                //ボディがトラッキングできている
                foreach (var body in bodies.Where(b => b.IsTracked))
                {
                    // TrainingInfoを利用して原点と２つのベクトルから角度を求める

                    // テスト用に右肘を原点、右手首と右肩をベクトルとして角度を求める
                    // 角度を求めて,UserAngleCollectionへAdd


                    // 右手首の座標を取得
                    var rightWristX = body.Joints[JointType.WristRight].Position.X;
                    var rightWristY = body.Joints[JointType.WristRight].Position.Y;
                    // 右肘の座標を取得 （原点 - B )
                    var rightElbowX = body.Joints[JointType.ElbowRight].Position.X;
                    var rightElbowY = body.Joints[JointType.ElbowRight].Position.Y;
                    // 右肩の座標を取得
                    var rightShoulderX = body.Joints[JointType.ShoulderRight].Position.X;
                    var rightShoulderY = body.Joints[JointType.ShoulderRight].Position.Y;

                    var wristVectorX = rightWristX - rightElbowX;
                    var wristVectorY = rightWristY - rightElbowY;

                    var shoulderVectorX = rightShoulderX - rightElbowX;
                    var shoulderVectorY = rightShoulderY - rightElbowY;

                    var cos = (wristVectorX * wristVectorY + shoulderVectorX * shoulderVectorY) / 
                        ((Math.Sqrt(Math.Pow(wristVectorX,2) + Math.Pow(wristVectorY,2)) * Math.Sqrt(Math.Pow(shoulderVectorX,2) + Math.Pow(shoulderVectorY,2))));

                    var angle = Math.Acos(cos);
                    Console.WriteLine(angle);

                    
                }
            }
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

        public void initChart()
        {
            Console.WriteLine(this.TrainingInfo.TrainingName);
            Console.WriteLine(this.TrainingInfo.RangeTrackingTargets.Count);
            //this.ChartLeft.DataContext = this.TrainingInfo.RangeTrackingTargets.First().generateBindingGraphCollection();

            // 教則用とユーザーのラインを用意
            this.ChartLeft.Series.First().DataContext = this.TrainingInfo.RangeTrackingTargets.First().generateBindingGraphCollection();
            this.ChartLeft.Series.Last().DataContext = this.UserAngleCollection;
        }
    }
}
