﻿using FASystem.Model;
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using Newtonsoft.Json;
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

namespace FASystem
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        private List<TrainingInfo> trainingInfos;


        public SettingWindow()
        {
            InitializeComponent();

            // Init Property
            this.trainingInfos = new List<TrainingInfo>();
            trainingList.ItemsSource = trainingInfos;

            this.loadTrainingInfos();

            KinectRegion.SetKinectRegion(this, kinectRegion);
            this.kinectRegion.KinectSensor = KinectSensor.GetDefault();

        }
          



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

        private void trainingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrainingInfo trainingInfo = (TrainingInfo)trainingList.SelectedItem;
            Console.WriteLine("selectedItem ->" + trainingInfo.TrainingName);
            Console.WriteLine(trainingInfo.RangeTrackingTargets.First().PermissibleRangeInTop.calcAverage());
            // MainWindosにtrainingInfoを渡したい
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.TrainingInfo = trainingInfo;
            mainWindow.initChart();
            this.Close();
        }
    }
    
}
