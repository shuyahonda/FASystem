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
using FASystem.CustomControl;
using System.Collections.Generic;
using System.Windows.Controls;

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

        private TrainingInfo trainingInfo;
        /// <summary>
        /// SettingWindowから送られてくるトレーニング情報
        /// FBに必要な情報はすべてこの中に入っている
        /// </summary>
        public TrainingInfo TrainingInfo
        {
            get
            {
                return this.trainingInfo;
            }
            set
            {
                this.trainingInfo = value;
                initAngleAnnotaions();
            }
        }


        /// <summary>
        /// ユーザーの関節角度を管理するコレクション
        /// Chartへ反映される
        /// </summary>
        private ObservableCollection<GraphPoint> UserAngleCollection { get; set; } = new ObservableCollection<GraphPoint>();

        /// <summary>
        /// 角度表示用のアノテーションへの参照を保持する
        /// </summary>
        private List<AngleAnnotation> AngleAnnotations { get; set; } = new List<AngleAnnotation>();

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

            if (kinect == null)
            {
                this.showCloseDialog("Kinectが接続されていないか、利用できません。アプリケーションを終了します。");
            }

            this.colorImageFormat = ColorImageFormat.Bgra;
            this.colorFrameDescription = this.kinect.ColorFrameSource.CreateFrameDescription(this.colorImageFormat);
            this.colorFrameReader = this.kinect.ColorFrameSource.OpenReader();
            this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            bodyFrameReader = kinect.BodyFrameSource.OpenReader();
            bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            this.kinect.Open();
            this.bodies = this.bodies = new Body[kinect.BodyFrameSource.BodyCount];
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

            this.cropBitmap(ref bitmapSource);

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
                        // 角度アノテーションの表示・座標の調整
                        AngleAnnotation annotation = this.AngleAnnotations.Where(ant => ant.trackingTarget.Origin == trackingTarget.Origin).First();
                        ColorSpacePoint colorPoint = this.kinect.CoordinateMapper.MapCameraPointToColorSpace(body.Joints[trackingTarget.Origin].Position);
                        Canvas.SetLeft(annotation, colorPoint.X / 2 - annotation.ActualWidth);
                        Canvas.SetTop(annotation, colorPoint.Y / 2 - annotation.ActualHeight);


           
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


                                if (trackingTarget.isManageTempo == true)
                                {
                                    this.UserAngleCollection.Add(graphPoint);

                                }

                                //this.AngleAnnotations.First().Angle = (int)Utility.radToDegree(angle);

                                var angleAnnotation = this.AngleAnnotations.Where(an => an.trackingTarget.Origin == trackingTarget.Origin).First();
                                angleAnnotation.Angle = (int)Utility.radToDegree(angle);

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

                    //固定点
                    foreach (var fixTarget in this.TrainingInfo.FixTrackingTargets)
                    {

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
            //Window settingWindow = new SettingWindow();
            //settingWindow.Show();
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
            const int GRAPH_MARGIN = 20;
            const int TEACH_BORDER_THICKNESS = 8;
            const int USER_BORDER_THICKNESS = 6;

            // Init Chart
            plotter.Children.RemoveAll((typeof(LineGraph)));

            //教則
            var managedTarget = this.TrainingInfo.RangeTrackingTargets.Where(tar => tar.isManageTempo == true).First();
            var teachSeries = new EnumerableDataSource<GraphPoint>(managedTarget.generateBindingGraphCollection());

            teachSeries.SetXMapping(x => x.X);
            teachSeries.SetYMapping(y => y.Y);
            plotter.AddLineGraph(teachSeries, Colors.Red, TEACH_BORDER_THICKNESS);

            //ユーザ
            var userAngleSeries = new EnumerableDataSource<GraphPoint>(this.UserAngleCollection);

            userAngleSeries.SetXMapping(x => x.X);
            userAngleSeries.SetYMapping(y => y.Y);
            plotter.AddLineGraph(userAngleSeries, Colors.LightGreen, USER_BORDER_THICKNESS);

            // 高さ固定
            this.setYAxisRange((int)managedTarget.PermissibleRangeInTop.calcAverage() - GRAPH_MARGIN, (int)managedTarget.PermissibleRangeInBottom.calcAverage() + GRAPH_MARGIN);
            plotter.LegendVisible = false;
        }


        /// <summary>
        ///　映像表示部の切取り処理
        /// </summary>
        private void cropBitmap(ref BitmapSource source)
        {
            //リサイズ
            //ScaleTransform scale = new ScaleTransform((this.cameraCanvas.ActualWidth / source.PixelWidth), (this.cameraCanvas.ActualHeight / source.PixelHeight));
            //TransformedBitmap tbBitmap = new TransformedBitmap(source, scale);

            source = new CroppedBitmap(source, new Int32Rect(this.colorFrameDescription.Width / 2 - (int)this.cameraCanvas.ActualWidth / 2,
                                                             this.colorFrameDescription.Height / 2 - (int)this.cameraCanvas.ActualHeight / 2,
                                                             (int)this.cameraCanvas.ActualWidth,
                                                             (int)this.cameraCanvas.ActualHeight));
        }

        /// <summary>
        /// アプリ起動時にKinectが利用できない場合に表示するダイアログ
        /// </summary>
        /// <param name="message"></param>
        private void showCloseDialog(string message)
        {
            string caption = "メッセージ";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(message, caption, button, icon);

            if (result == MessageBoxResult.OK)
            {
                Environment.Exit(0);
            }
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

        /// <summary>
        /// トレーニングセレクトボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trainingSelectButton_Click(object sender, RoutedEventArgs e)
        {
            this.showTrainingSelectWindow();
        }

        /// <summary>
        /// 設定ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settingButton_Click(object sender, RoutedEventArgs e)
        {
            this.showSettingWindow();
        }


        /// <summary>
        /// 角度表示用アノテーションの初期化
        /// </summary>
        private void initAngleAnnotaions()
        {
            foreach (var target in this.TrainingInfo.RangeTrackingTargets)
            {
                AngleAnnotation annotation = new AngleAnnotation(target);
                this.AngleAnnotations.Add(annotation);
                this.cameraCanvas.Children.Add(annotation);
            }
        }
    }
}
