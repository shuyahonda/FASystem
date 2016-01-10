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

namespace FASystem.CustomControl
{
    /// <summary>
    /// 角度を表示するカスタムコントロール
    /// </summary>
    public partial class AngleAnnotation : UserControl
    {
        /// <summary>
        /// 角度を保持するプロパティ
        /// </summary>
        public int Angle
        {
            get
            {
                return Angle;
            }
            set
            {
                this.AngleText.Text = value.ToString() + "°";
                this.Angle = value;
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

        public AngleAnnotation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// アノテーションのBackgroundColorを変更する
        /// </summary>
        /// <param name="color"></param>
        private void setBackColor(Color color)
        {
            this.AnnotationBack.Background = new SolidColorBrush(color);
        }
    }
}
