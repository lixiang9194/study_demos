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
using CPWGI.MultiphaseCalculate;
using CPWGI.Control;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;

namespace CPWGI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public WellBoreClass WellBore { get; set; }
        public DrillMudClass Mud { get; set; }
        public GasClass Gas { get; set; }
        public CalculateMultiphaseHasanKabir Calculate { get; set; }
        public ResultMatrix Result { get; set; }
        //todo井口回压在这里添加并不合适
        public double PressureWellHead { get; set; }
        public double ErrorWell = 10000, ErrorSegment = 1000, ErrorHoldup = 0.0001;
        public ControlCalculateClass CC { get; set; }
        public SeriesCollection SC { get; set; }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ChartValues<ObservablePoint> CV = new ChartValues<ObservablePoint>();
            double X = 0, Y = 0;
            for (int i = 1; i < WellBore.wellBoreGrid.Count; i++)
            {
                X = WellBore.wellBoreGrid[i].wellDepth;
                Y = Result.wellborePressure[0][i]/1000;
                CV.Add(new ObservablePoint(X, Y));
            }

            SC = new SeriesCollection
            {
                new LineSeries
                {
                    Values=CV,
                },
            };
            lvcChart.Series = SC;
            MessageBox.Show("画图完成！");
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Result = new ResultMatrix();
            Calculate = new CalculateMultiphaseHasanKabir();
            WellBore = wellBoreView.wellBore;
            Mud = mudPropertyView.Mud;
            Gas = gasPropertyView.Gas;
            WellBore.addSection(1000, 8, 3.5);
            WellBore.meshWell();
            CC = new ControlCalculateClass(WellBore, Mud, Gas, Calculate, Result);
            CC.CalculateSteady(50, 10);
            MessageBox.Show("计算完成！");
        }
    }
}
