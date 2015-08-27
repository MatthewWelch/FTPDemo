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

namespace ScaleableImageControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ScaleableImageCtrl : UserControl
    {
        public ScaleableImageCtrl()
        {
            InitializeComponent();
        }
        public void SetSource(string fileName)
        {
            img.Source = new BitmapImage(new Uri(fileName));
        }

        //public void SetSourceFromBitmap(bitmap bmp)
        //{
        //    img.Source = new BitmapImage(new Uri(fileName));
        //}
        public void SetOpacity(double opacity)
        {
            img.Opacity = opacity;
        }

    }
}
