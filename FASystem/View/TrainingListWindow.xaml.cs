using FASystem.Model;
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FASystem
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class TrainingListWindow : Window, INotifyPropertyChanged
    {
        private const int DEFAULT_REPS = 8;

        private List<TrainingInfo> trainingInfos;

        private int reps = DEFAULT_REPS;

        public int Reps
        {
            get
            {
                return reps;
            }
            set
            {
                this.reps = value;
                OnPropertyChanged("Reps");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TrainingListWindow()
        {
            InitializeComponent();

            // Init KinectRegion
            KinectRegion.SetKinectRegion(this, kinectRegion);
            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();

            // Init Fields
            this.trainingInfos = new List<TrainingInfo>();
            trainingList.ItemsSource = trainingInfos;

            // トレーニングリストを表示
            this.loadTrainingInfos();

            this.DataContext = this;
        }

        /// <summary>
        /// トレーニング情報を外部ファイル等から初期化する
        /// </summary>
        private void loadTrainingInfos()
        {
            /* 実行ファイルのパスを取得する
             *
            string exePath = Environment.GetCommandLineArgs()[0];
            string exeFullPath = System.IO.Path.GetFullPath(exePath);
            string startupPath = System.IO.Path.GetDirectoryName(exeFullPath);
            */

            /*
            // リソースを取得する
            System.Reflection.Assembly myAssembly =　System.Reflection.Assembly.GetExecutingAssembly();
            
            Stream stream = myAssembly.GetManifestResourceStream("TestTraining");

            Console.WriteLine(myAssembly.GetManifestResourceNames().Count());

            StreamReader reader = new StreamReader(stream);
            String text = reader.ReadToEnd();
            
            Console.WriteLine(text);
            */


            //TODO: あとで修正
            String text = System.Text.Encoding.UTF8.GetString(FASystem.Properties.Resources.TestTraining);

            Console.WriteLine(text);

            text = text.Remove(0, 1);


            //TrainingModelを生成

            var trainingInfo = JsonConvert.DeserializeObject<TrainingInfo>(text);

            this.trainingInfos.Add(trainingInfo);

            Console.WriteLine(this.trainingInfos.First().TrainingName);
        }

        /// <summary>
        /// トレーニングリストからあるトレーニングが選択されたときに呼ばれる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trainingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrainingInfo trainingInfo = (TrainingInfo)trainingList.SelectedItem;
            
            // MainWindosにtrainingInfoを渡す
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.TrainingInfo = trainingInfo;
            mainWindow.initChart();
            this.Close();
        }

        private void repsUpButton_Click(object sender, RoutedEventArgs e)
        {
            this.Reps++;
        }

        private void repsDownButton_Click(object sender, RoutedEventArgs e)
        {
            this.Reps--;
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
