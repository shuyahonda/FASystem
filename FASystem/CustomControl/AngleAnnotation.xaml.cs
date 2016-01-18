using FASystem.Model;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FASystem.CustomControl
{
    /// <summary>
    /// 角度を表示するカスタムコントロール
    /// </summary>
    public partial class AngleAnnotation : UserControl,INotifyPropertyChanged
    {
        private static readonly Color PERMISSIBLE_COLOR = Colors.GreenYellow;
        private static readonly Color PROHIBITED_COLOR = Colors.OrangeRed;
        private static readonly Color NORMAL_COLOR = Colors.LightGray;

        public event PropertyChangedEventHandler PropertyChanged;

        private int angle;

        /// <summary>
        /// 角度を保持するプロパティ
        /// </summary>
        public int Angle
        {
            get
            {
                return this.angle;
            }
            set
            {
                this.angle = value;
                this.checkAngle(this.angle);
                OnPropertyChanged("Angle");
            }
        }

        /// <summary>
        /// 線の座標を管理するプロパティ
        /// 骨格の座標になる
        /// </summary>
        public int EdgeCoordinate {
            get
            {
                return EdgeCoordinate;
            }
            set
            {
                this.EdgeCoordinate = value;
            }
        }

        /// <summary>
        /// トラッキングターゲット
        /// 原点となる関節と、許容範囲・非許容範囲を利用する
        /// </summary>
        public RangeTrackingTarget trackingTarget { get; set; }


        public AngleAnnotation()
        {
            InitializeComponent();

            this.AngleText.DataContext = this;
        }

        public AngleAnnotation(RangeTrackingTarget target) : this()
        {
            this.trackingTarget = target;
        }

        /// <summary>
        /// アノテーションのBackgroundColorを変更する
        /// </summary>
        /// <param name="color"></param>
        private void setBackColor(Color color)
        {
            SolidColorBrush colorBrush = new SolidColorBrush(color);
            colorBrush.Opacity = 0.9;
            this.AnnotationBack.Background = colorBrush;
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void checkAngle(int angle)
        {
            if (this.trackingTarget.PermissibleRangeInTop.inRangeAngle(angle) || this.trackingTarget.PermissibleRangeInBottom.inRangeAngle(angle))
            {
                this.setBackColor(PERMISSIBLE_COLOR);
            } else if (this.trackingTarget.ProhibitedRangeInTop.inRangeAngle(angle) || this.trackingTarget.ProhibitedRangeInBottom.inRangeAngle(angle))
            {
                this.setBackColor(PROHIBITED_COLOR);
            } else
            {
                this.setBackColor(NORMAL_COLOR);
            }
        }
    }
}
