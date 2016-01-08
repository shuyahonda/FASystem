using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using FASystem.Model;
using FASystem.Enum;
using System.Collections.ObjectModel;
using FASystem.Helper;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using FASystem.View;

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
        /// フレームカウント
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

            if (this.kinect == null)
            {
                this.showCloseDialog("Kinectが接続されていません。アプリケーションを終了します。");
            }

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

            //リサイズ
            //ScaleTransform scale = new ScaleTransform((this.cameraCanvas.ActualWidth / bitmapSource.PixelWidth), (this.cameraCanvas.ActualHeight / bitmapSource.PixelHeight));
            //TransformedBitmap tbBitmap = new TransformedBitmap(bitmapSource, scale);

            CroppedBitmap croppedBitmap = new CroppedBitmap(bitmapSource, new Int32Rect(this.colorFrameDescription.Width / 2 - this.colorFrameDescription.Width / 2,
                                                                                       this.colorFrameDescription.Height / 2 - this.colorFrameDescription.Height / 2,
                                                                                       (int)this.cameraCanvas.ActualWidth,
                                                                                       (int)this.cameraCanvas.ActualHeight));

            //ScaleTransform scale = new ScaleTransform((this.cameraCanvas.ActualWidth / croppedBitmap.PixelWidth), (this.cameraCanvas.ActualHeight / bitmapSource.PixelHeight));



            Console.WriteLine(this.cameraCanvas.ActualWidth);
            Console.WriteLine(this.cameraCanvas.ActualHeight);

            //キャンバスに表示する
            //this.cameraCanvas.Background = new ImageBrush(bitmapSource);
            this.cameraCanvas.Background = new ImageBrush(croppedBitmap);

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

            if (count == this.TrainingInfo.RangeTrackingTargets.First().Tempo.getAllFrame())
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

                        Vector vector1 = new Vector();
                        Vector vector2 = new Vector();
                        double cos;
                        double angle;
                        GraphPoint graphPoint;

                        switch (trackingTarget.PlaneType)
                        {                            
                            case PlaneType.CoronalPlane:
                                // X,Y
                                vector1.X = position1.X - origin.X;
                                vector1.Y = position1.Y - origin.Y;
                                vector2.X = position2.X - origin.X;
                                vector2.Y = position2.Y - origin.Y;

                                cos = (vector1.X * vector2.X + vector1.Y * vector2.Y) /
                                    ((Math.Sqrt(Math.Pow(vector1.X, 2) + Math.Pow(vector1.Y, 2)) * Math.Sqrt(Math.Pow(vector2.X, 2) + Math.Pow(vector2.Y, 2))));
                                angle = Math.Acos(cos);
                                graphPoint = new GraphPoint(count, (int)Utility.radToDegree(angle));
                                this.UserAngleCollection.Add(graphPoint);
#if DEBUG 
                                Console.WriteLine("CoronalPlane...Angle ->" + Utility.radToDegree(angle) + "°");
#endif

                                break;
                            case PlaneType.SagittalPlane:
                                // Y,Z
                                vector1.X = position1.Y - origin.Y;
                                vector1.Y = position1.Z - origin.Z;
                                vector2.X = position2.Y - origin.Y;
                                vector2.Y= position2.Z - origin.Z;

                                cos = (vector1.X * vector2.X + vector1.Y * vector2.Y) /
                                    ((Math.Sqrt(Math.Pow(vector1.X, 2) + Math.Pow(vector1.Y, 2)) * Math.Sqrt(Math.Pow(vector2.X, 2) + Math.Pow(vector2.Y, 2))));
                                angle = Math.Acos(cos);
                                graphPoint = new GraphPoint(count, (int)Utility.radToDegree(angle));
                                this.UserAngleCollection.Add(graphPoint);
#if DEBUG
                                Console.WriteLine("SagittalPlane...Angle ->" + Utility.radToDegree(angle) + "°");
#endif
                                
                                break;
                            case PlaneType.TransversePlane:
                                // X,Z
                                vector1.X = position1.Y - origin.Y;
                                vector1.Y = position1.Z - origin.Z;
                                vector2.X = position2.Y - origin.Y;
                                vector2.Y = position2.Z - origin.Z;

                                cos = (vector1.X * vector2.X + vector1.Y * vector2.Y) /
                                                                    ((Math.Sqrt(Math.Pow(vector1.X, 2) + Math.Pow(vector1.Y, 2)) * Math.Sqrt(Math.Pow(vector2.X, 2) + Math.Pow(vector2.Y, 2))));
                                angle = Math.Acos(cos);
                                graphPoint = new GraphPoint(count, (int)Utility.radToDegree(angle));
                                this.UserAngleCollection.Add(graphPoint);

#if DEBUG
                                Console.WriteLine("TransversePlane...Angle ->" + Utility.radToDegree(angle) + "°");
#endif
                                break;
                            default:
                                break;
                        }
                    }
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
        private void showTrainingSelectWindow()
        {
            Window selectWindow = new TrainingListWindow();
            selectWindow.Show();
        }

        /// <summary>
        /// 設定ウィンドウを表示する
        /// </summary>
        private void showSettingWindow()
        {
            Window settingWindow = new SettingWindow();
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
                this.showTrainingSelectWindow();
            }
        }

        /// <summary>
        /// グラフ表示部の初期化処理
        /// </summary>
        public void initChart()
        {
            // Init Chart - Data Binding
            plotter.Children.RemoveAll((typeof(LineGraph)));
            
            var teachSeries = new EnumerableDataSource<GraphPoint>(this.TrainingInfo.RangeTrackingTargets.First().generateBindingGraphCollection());
            teachSeries.SetXMapping(x => x.X);
            teachSeries.SetYMapping(y => y.Y);
            plotter.AddLineGraph(teachSeries, Colors.Red, 2);

            var userAngleSeries = new EnumerableDataSource<GraphPoint>(this.UserAngleCollection);
            userAngleSeries.SetXMapping(x => x.X);
            userAngleSeries.SetYMapping(y => y.Y);
            plotter.AddLineGraph(userAngleSeries, Colors.Green, 2);


            // Init Chart - Height
            RangeTrackingTarget target = this.TrainingInfo.RangeTrackingTargets.First();
            this.setYAxisRange((int)target.PermissibleRangeInTop.calcAverage() - 20, (int)target.PermissibleRangeInBottom.calcAverage() + 20);
        }

        private void trainingSelectButton_Click(object sender, RoutedEventArgs e)
        {
            this.showTrainingSelectWindow();
        }

        private void cropBitmap()
        {

        }

        private void settingButton_Click(object sender, RoutedEventArgs e)
        {
            this.showSettingWindow();
        }

        private void showCloseDialog(string message)
        {
            Console.WriteLine("showCloseDialog");
            string caption = "";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBox.Show(message, caption, button, icon);
        }

        /// <summary>
        /// ChartのY軸を固定する
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private void setYAxisRange(int min, int max)
        {
            ViewportAxesRangeRestriction restr = new ViewportAxesRangeRestriction();
            restr.YRange = new DisplayRange(min, max);
            plotter.Viewport.Restrictions.Add(restr);
        }
    }
}
