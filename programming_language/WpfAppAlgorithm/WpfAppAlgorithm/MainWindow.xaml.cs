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
using System.Threading;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WpfAppAlgorithm
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public int arrayLength { get; set; } = 200;
        public int pauseMillionSeconds { get; set; } = 10;
        private List<double> randomArray=new List<double>();
        private List<ObservableValue> randomList;
        public ChartValues<ObservableValue> originList { get; set; } = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> insertSortList { get; set; } = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> shellSortList { get; set; } = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> selectSortList { get; set; } = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> HeapSortList { get; set; } = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> bubbleSortList { get; set; } = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> quickSortList { get; set; } = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> quickSortUseThreadsList { get; set; } = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> defaultSortList { get; set; } = new ChartValues<ObservableValue>();
        public MainWindow()
        {
            InitializeComponent();
            
            DataContext = this;
        }

        private void GenerateArray_Click(object sender, RoutedEventArgs e)
        {
            randomArray.Clear();
            originList.Clear();
            insertSortList.Clear();
            shellSortList.Clear();
            selectSortList.Clear();
            HeapSortList.Clear();
            bubbleSortList.Clear();
            quickSortList.Clear();
            quickSortUseThreadsList.Clear();
            defaultSortList.Clear();
            Task.Run(
                () =>{
                    Random ran = new Random();
                    for (int i = 0; i < arrayLength; i++)
                    {
                        randomArray.Add(ran.Next(0,1000));
                    }
                    randomList = randomArray.ConvertAll(new Converter<double,ObservableValue>(x=> { return new ObservableValue(x); }));
                    originList.AddRange(randomList);
                    insertSortList.AddRange(randomList);
                    shellSortList.AddRange(randomList);
                    selectSortList.AddRange(randomList);
                    HeapSortList.AddRange(randomList);
                    bubbleSortList.AddRange(randomList);
                    quickSortList.AddRange(randomList);
                    quickSortUseThreadsList.AddRange(randomList);
                    defaultSortList.AddRange(randomList);
                });
        }

        private void SortDirect_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.DirectInsertSort(insertSortList,pauseMillionSeconds)); 
        }

        private void ShellSort_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.ShellSort(shellSortList, pauseMillionSeconds));
        }

        private void SelectSort_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.SelectSort(selectSortList, pauseMillionSeconds));
        }

        private void HeapSort_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.HeapSort(HeapSortList, pauseMillionSeconds));
        }

        private void BubbleSort_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.BubbleSort(bubbleSortList, pauseMillionSeconds));
        }

        private void QuickSort_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.QuickSort(quickSortList, pauseMillionSeconds));
        }

        private void QuickSortUseThreads_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.QuickSortUseThreads(quickSortUseThreadsList, pauseMillionSeconds));
        }

        private void MergeSort_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.MergeSort(defaultSortList, pauseMillionSeconds));
        }

        private void SortArrayUseAllFunction_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SortClass.DirectInsertSort(insertSortList, pauseMillionSeconds));
            Task.Run(() => SortClass.ShellSort(shellSortList, pauseMillionSeconds));
            Task.Run(() => SortClass.SelectSort(selectSortList, pauseMillionSeconds));
            Task.Run(() => SortClass.HeapSort(HeapSortList, pauseMillionSeconds));
            Task.Run(() => SortClass.BubbleSort(bubbleSortList, pauseMillionSeconds));
            Task.Run(() => SortClass.QuickSort(quickSortList, pauseMillionSeconds));
            Task.Run(() => SortClass.QuickSortUseThreads(quickSortUseThreadsList, pauseMillionSeconds));
            Task.Run(() => SortClass.MergeSort(defaultSortList, pauseMillionSeconds));
        }
    }
    
}
