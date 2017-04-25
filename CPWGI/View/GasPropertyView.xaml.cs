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
using CPWGI.Model;

namespace CPWGI.View
{
    /// <summary>
    /// GasPropertyView.xaml 的交互逻辑
    /// </summary>
    public partial class GasPropertyView : UserControl
    {
        public GasClass Gas { get; set; }
        public GasPropertyView()
        {
            Gas = new GasClass(0.97);
            InitializeComponent();
            DataContext = this;
        }
    }
}
