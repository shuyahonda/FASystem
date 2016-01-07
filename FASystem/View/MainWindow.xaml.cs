using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using FASystem.Model;
using System.Collections.ObjectModel;
using FASystem.Helper;

namespace FASystem
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Kinect関連
        /// </summary>
        private KinectSensor kinect;
        private FrameDescription colorFrameDescription;
        private ColorImageFormat colorImageFormat;
        private ColorFrameReader colorFrameReader;
        private BodyFrameReader bodyFrameReader;
        private Body[] bodies;

        /// <summary>
        /// SettingWindowから送られてくるトレーニング情報
        /// FBに必要な情報はすべてこの中に入っている
        /// </summary>
        public TrainingInfo TrainingInfo { get; set; }

        /// <summary>
        /// ユーザーの関節角度を管理するコレクション
        /// Chartへ反映される
        /// </summary>
        private ObservableCollection<GraphPoint> UserAngleCollection { get; set; }

        /// <summary>
        /// こいつは別のところで管理したい
        /// </summary>
        private int count = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Init Kinect Sensors
            this.kinect = KinectSensor.GetDefault();
            this.colorImageFormat = ColorImageFormat.Bgra;
            this.colorFrameDescription = this.kinect.ColorFrameSource.CreateFrameDescription(this.colorImageFormat);
            this.colorFrameReader = this.kinect.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            bodyFrameReader = kinect.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;
            this.kinect.Open();
            this.bodies = this.bodies = new Body[kinect.BodyFrameSource.BodyCount];

            this.UserAngleCollection = new ObservableCollection<GraphPoint>();
        }


        /// <summary>
        /// Kinectセンサーからのカラー画像のハンドリング
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Kinectセンサーからの骨格情報のハンドリング
        /// 単位はメートル
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            if (this.TrainingInfo == null) return;

            count++;

            if (count == 210)
            {
                this.UserAngleCollection.Clear();
                count = 0;
            }

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
                    foreach (var trackingTarget in this.TrainingInfo.RangeTrackingTargets)
                    {
                        CameraSpacePoint origin = new CameraSpacePoint();
                        CameraSpacePoint position1 = new CameraSpacePoint();
                        CameraSpacePoint position2 = new CameraSpacePoint();

                        // ベクトルが一つであれば、単位ベクトルを利用して計算する
                        if (trackingTarget.Vector.Count == 1)
                        {
                            origin = body.Joints[trackingTarget.Origin].Position;
                            position1 = body.Joints[trackingTarget.Vector[0]].Position;
                            position2 = body.Joints[trackingTarget.Vector[1]].Position;

                        }
                        else if (trackingTarget.Vector.Count == 2)
                        {
                            origin = body.Joints[trackingTarget.Origin].Position;
                            position1 = body.Joints[trackingTarget.Vector[0]].Position;
                            position2 = body.Joints[trackingTarget.Vector[1]].Position;
                        }

                        if (trackingTarget.PlaneType == Enum.PlaneType.CoronalPlane)
                        {
                            // use X,Y
                            var vectorX1 = position1.X - origin.X;
                            var vectorY1 = position1.Y - origin.Y;
                            var vectorX2 = position2.X - origin.X;
                            var vectorY2 = position2.Y - origin.Y;

                            var cos = (vectorX1 * vectorY1 + vectorX2 * vectorY2) /
                                ((Math.Sqrt(Math.Pow(vectorX1, 2) + Math.Pow(vectorY1, 2)) * Math.Sqrt(Math.Pow(vectorX2, 2) + Math.Pow(vectorY2,2))));
                            var angle = Math.Acos(cos);
                            Console.WriteLine("Angle ->" + angle + "°");
                        }
                        else if (trackingTarget.PlaneType == Enum.PlaneType.SagittalPlane)
                        {
                            // use Y,Z
                            var vectorX1 = position1.Y - origin.Y;
                            var vectorY1 = position1.Z - origin.Z;
                            var vectorX2 = position2.Y - origin.Y;
                            var vectorY2 = position2.Z - origin.Z;

                            var cos = (vectorX1 * vectorY1 + vectorX2 * vectorY2) /
                                ((Math.Sqrt(Math.Pow(vectorX1, 2) + Math.Pow(vectorY1, 2)) * Math.Sqrt(Math.Pow(vectorX2, 2) + Math.Pow(vectorY2, 2))));
                            var angle = Math.Acos(cos);
                            Console.WriteLine("Angle ->" + angle + "°");
                        }
                        else if (trackingTarget.PlaneType == Enum.PlaneType.TransversePlane)
                        {
                            // use X,Z
                            var vectorX1 = position1.X - origin.X;
                            var vectorY1 = position1.Z - origin.Z;
                            var vectorX2 = position2.X - origin.X;
                            var vectorY2 = position2.Z - origin.Z;

                            var cos = (vectorX1 * vectorY1 + vectorX2 * vectorY2) /
                                ((Math.Sqrt(Math.Pow(vectorX1, 2) + Math.Pow(vectorY1, 2)) * Math.Sqrt(Math.Pow(vectorX2, 2) + Math.Pow(vectorY2, 2))));
                            var angle = Math.Acos(cos);
                            Console.WriteLine("Angle ->" + angle + "°");
                        }
                    
                        /*
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
                            ((Math.Sqrt(Math.Pow(wristVectorX, 2) + Math.Pow(wristVectorY, 2)) * Math.Sqrt(Math.Pow(shoulderVectorX, 2) + Math.Pow(shoulderVectorY, 2))));

                        var angle = Math.Acos(cos);
                        Console.WriteLine(angle);
                        */

                    }

                   
                    /*
                    var angle = Utility.radToPI(Math.Acos(cos));
                    
                    GraphPoint point = new GraphPoint(count, (int)angle);
                    this.UserAngleCollection.Add(point);
                    */
                }
            }
        }

        /// <summary>
        /// Windowを閉じたときの処理
        /// Kinectを閉じる
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //カラー画像の取得を中止して、関連するリソースを放棄する
            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            //Kinectを停止して、関連するリソースを放棄する
            if (this.kinect != null)
            {
                this.kinect.Close();
                this.kinect = null;
            }
        }

        /// <summary>
        /// トレーニング情報を設定するためのウィンドウを表示する
        /// </summary>
        private void showSettingWindow()
        {
            Window settingWindow = new TrainingListWindow();
            settingWindow.Show();
        }

        /// <summary>
        /// Windowが描画されたときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (TrainingInfo == null)
            {
                this.showSettingWindow();
            }
        }

        /// <summary>
        /// グラフ表示部の初期化処理
        /// </summary>
        public void initChart()
        {
            // Init Chart - Data Binding
            this.ChartLeft.Series.First().DataContext = this.TrainingInfo.RangeTrackingTargets.First().generateBindingGraphCollection();
            this.ChartLeft.Series.Last().DataContext = this.UserAngleCollection;

            // Init Chart Widthdd
            RangeTrackingTarget target = this.TrainingInfo.RangeTrackingTargets.First();
            this.ChartLeft.MaxWidth = target.Tempo.getAllFrame();
        }
    }
}
