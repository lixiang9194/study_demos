using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Threading;
using System.Collections;

namespace WpfAppAlgorithm
{
    public static class SortClass
    {
        /// <summary>
        /// 直接插入排序
        /// </summary>
        public static void DirectInsertSort(ChartValues<ObservableValue> originList,int pauseTime)
        {
            int length = originList.Count;
            ObservableValue temp;int index;
            for (int i = 1; i < length; i++)
            {
                temp = originList[i];
                index = i;
                for (int j = i-1; j >-1 ; j--)
                {
                    if (originList[j].Value > temp.Value)
                    {
                        originList[j + 1] = originList[j];
                        index = j;
                    }
                    else
                        break;//如果j处小于temp，则j-1必小于temp，不必再循环
                }
                Thread.Sleep(pauseTime);
                originList[index] = temp;
            }
        }

        /// <summary>
        /// 希尔插入排序
        /// </summary>
        public static void ShellSort(ChartValues<ObservableValue> originList, int pauseTime)
        {
            int length = originList.Count;
            int step = (int)Math.Round(length / 2.0);
            ObservableValue temp; int j;
            while (step>0)
            {
                for (int i = step; i < length; i++)
                {
                    temp = originList[i];
                    j = i;
                    while(j>=step && originList[j-step].Value>temp.Value)
                    {
                        originList[j] = originList[j - step];
                        j = j - step;
                    }
                    originList[j] = temp;
                    Thread.Sleep(pauseTime);
                }
                step = (int)Math.Round(step / 2.0);
            }
        }

        /// <summary>
        /// 直接选择排序
        /// </summary>
        public static void SelectSort(ChartValues<ObservableValue> originList, int pauseTime)
        {
            int length = originList.Count;
            ObservableValue temp; int min;
            for (int i = 0; i < length; i++)
            {
                min = i;
                for (int j = i + 1; j < length; j++)
                {
                    if (originList[j].Value < originList[min].Value)
                        min = j;
                }
                Thread.Sleep(pauseTime);
                temp = originList[i];
                originList[i] = originList[min];
                originList[min] = temp;
            }
        }

        /// <summary>
        /// 堆排序
        /// </summary>
        public static void HeapSort(ChartValues<ObservableValue> originList, int pauseTime)
        {
            int length = originList.Count;
            int first = (int)Math.Floor(length / 2.0);
            ObservableValue temp;
            //构造大根堆
            for (int start = first; start > -1; start--)
                MaxHeap(originList, start, length - 1,pauseTime);
            
            //堆排，将大根堆转换成有序数组
            for (int end = length-1; end >0; end--)
            {
                temp = originList[end];
                originList[end] = originList[0];
                originList[0] = temp;
                MaxHeap(originList, 0, end - 1,pauseTime);
            }
        }
        //最大堆调整：将堆的末端子节点作调整，使得子节点永远小于父节点
        private static void MaxHeap(ChartValues<ObservableValue> originList, int start, int end, int pauseTime)
        {
            int root = start;
            int child;
            ObservableValue temp;
            while (true)
            {
                child = root * 2 + 1;
                if (child > end) break;
                if (child + 1 <= end && originList[child].Value < originList[child + 1].Value)
                    child = child + 1;
                if (originList[root].Value < originList[child].Value)
                {
                    temp = originList[root];
                    originList[root] = originList[child];
                    originList[child] = temp;
                    root = child;
                    Thread.Sleep(pauseTime);
                }
                else
                    break;
            }
        }

        /// <summary>
        /// 冒泡排序
        /// </summary>
        public static void BubbleSort(ChartValues<ObservableValue> originList, int pauseTime)
        {
            int length = originList.Count;
            ObservableValue temp;
            bool swapFlag = false;
            int lastSwapIndex=0,lastIndex=length;
            for (int i = 0; i < length; i++)
            {
                for (int j = 1; j < lastIndex; j++)
                {
                    if (originList[j-1].Value>originList[j].Value)
                    {
                        temp = originList[j];
                        originList[j] = originList[j - 1];
                        originList[j - 1] = temp;
                        lastSwapIndex = j;
                        swapFlag = true;  
                    }  
                }
                lastIndex = lastSwapIndex;
                Thread.Sleep(pauseTime);
                //如果某次遍历没有发生交换，则排序已经完成
                if (!swapFlag) break;
            }
        }

        /// <summary>
        /// 快速排序
        /// </summary>
        public static void QuickSort(ChartValues<ObservableValue> originList, int pauseTime)
        {
            QSort(originList, 0, originList.Count - 1,pauseTime);
        }
        //快排函数，ary为待排序数组，left为待排序的左边界，right为右边界
        private static void QSort(ChartValues<ObservableValue> originList,int left,int right,int pauseTime)
        {
            if (left >= right) return;
            double key = originList[left].Value;
            ObservableValue temp;
            int lp = left, rp = right;
            while (lp<rp)
            {
                while (originList[rp].Value >= key && lp < rp)
                    rp -= 1;
                while (originList[lp].Value <= key && lp < rp)
                    lp += 1;
                temp = originList[lp];
                originList[lp] = originList[rp];
                originList[rp] = temp;
                Thread.Sleep(pauseTime);
            }
            temp = originList[lp];
            originList[lp] = originList[left];
            originList[left] = temp;
            QSort(originList, left, lp - 1,pauseTime);
            QSort(originList, rp + 1, right,pauseTime);
        }
        
        /// <summary>
        /// 快速排序(多线程版)
        /// </summary>
        public static void QuickSortUseThreads(ChartValues<ObservableValue> originList, int pauseTime)
        {
            QSortUseThreads(originList, 0, originList.Count - 1, pauseTime);
        }
        //快排函数(多线程版)，ary为待排序数组，left为待排序的左边界，right为右边界
        private static void QSortUseThreads(ChartValues<ObservableValue> originList, int left, int right, int pauseTime)
        {
            if (left >= right) return;
            double key = originList[left].Value;
            ObservableValue temp;
            int lp = left, rp = right;
            while (lp < rp)
            {
                while (originList[rp].Value >= key && lp < rp)
                    rp -= 1;
                while (originList[lp].Value <= key && lp < rp)
                    lp += 1;
                temp = originList[lp];
                originList[lp] = originList[rp];
                originList[rp] = temp;
                Thread.Sleep(pauseTime);
            }
            temp = originList[lp];
            originList[lp] = originList[left];
            originList[left] = temp;
            Task.Run(() => QSort(originList, left, lp - 1, pauseTime));
            Task.Run(() => QSort(originList, rp + 1, right, pauseTime));
        }

        /// <summary>
        /// 归并排序方法
        /// </summary>
        public static void MergeSort(ChartValues<ObservableValue> originList, int pauseTime)
        {
            int length = originList.Count;
            MergeSortMain(originList, 0, length - 1, pauseTime);
        }
        //归并排序主程序（目标数组，子表的起始位置，子表的终止位置）
        private static void MergeSortMain(ChartValues<ObservableValue> originList,int first,int last, int pauseTime)
        {
            if (first >= last) return;
            int mid = (first + last) / 2;
            //对划分出来的子表进行递归划分
            MergeSortMain(originList, first, mid,pauseTime);
            MergeSortMain(originList, mid + 1, last, pauseTime);
            //对左右子表进行有序的整合（归并排序的核心部分）
            Merge(originList, first, mid, last,pauseTime);
        }
        //归并排序的核心部分：将两个有序的左右子表（以mid区分），合并成一个有序的表
        private static void Merge(ChartValues<ObservableValue> originList,int first,int mid,int last,int pauseTime)
        {
            ChartValues<ObservableValue> tempList = new ChartValues<ObservableValue>();
            int lp = first, rp = mid + 1;
            
            //进行左右子表的遍历，如果其中有一个子表遍历完，则跳出循环
            while (lp<=mid && rp<=last)
            {
                if(originList[lp].Value<=originList[rp].Value)
                    tempList.Add(originList[lp++]);
                else
                    tempList.Add(originList[rp++]);
            }
            
            //有一侧子表遍历完后，跳出循环，将另外一侧子表剩下的数一次放入暂存数组中（有序）
            for (int i = lp; i <= mid ; i++)
                tempList.Add(originList[i]);
            for (int i = rp; i <= last ; i++)
                tempList.Add(originList[i]);
            
            //将暂存数组中有序的数列写入目标数组的制定位置，使进行归并的数组段有序
            int tempIndex = 0;
            for (int i = first; i <= last; i++)
                originList[i] = tempList[tempIndex++];
            Thread.Sleep(pauseTime);
        }

    }
}
