using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace CPWGI.Model
{
    /// <summary>
    /// 井身结构类，可完成井身结构的创建、修改及网格划分
    /// </summary>
    public class WellBoreClass
    {
        //保存井的名字
        public string wellName { get; set; }
        //保存井身网格，外部只有读权限
        public List<WellGrid> wellBoreGrid { get; private set; }
        public double meshLength { get; set; }
        //保存井身段信息，用于在界面上展示
        public ObservableCollection<WellSection> wellSectionList { get; set; }

        public double groundTemperature { get; set; }
        public double temperaturePerMeter { get; set; }

        public WellBoreClass(string wellName="well") {
            this.wellName = wellName;
            wellSectionList = new ObservableCollection<WellSection>();
        }

        public void addSection(double H, double D, double d)
        {
            wellSectionList.Add(new WellSection(H, D, d));
        }

        public void deleteSection(int index)
        {
            wellSectionList.RemoveAt(index);
        }

        /// <summary>
        /// 该方法完成井身结构的网格划分
        /// </summary>
        /// <param name="meshLength">网格长度</param>
        public List<WellGrid> meshWell()
        {
            wellBoreGrid = new List<WellGrid>();
            wellBoreGrid.Add(new WellGrid());
            WellGrid item = null;
            int counter = 1;
            double layerLength = 0,temperature=0;
            foreach (WellSection section in wellSectionList)
            {
                for (int i = 1; i < section.sectionLength / meshLength + 1; i++)
                {
                    //判断网格是否达到了段长，分别计算网格长度
                    if (i * meshLength <= section.sectionLength)
                        layerLength = wellBoreGrid[counter - 1].wellDepth + meshLength;
                    else
                        layerLength = wellBoreGrid[counter - 1].wellDepth + section.sectionLength - (i - 1) * meshLength;
                    //每次在此处新建对象，则List显示正常
                    //（如果仅在此处更改项的值，则List内所有子项指向同一个对象，错误）
                    temperature = groundTemperature + layerLength * temperaturePerMeter;
                    item = new WellGrid(layerLength,section.outterDiameter,section.innerDiameter,temperature);
                    wellBoreGrid.Add(item);
                    counter++;
                }
            }
            return wellBoreGrid;
        }
    }

    /// <summary>
    /// 井身结构的基本组成部分，包含段长、外径、内径三个参数
    /// 注意初始化时输入的参数单位：段长（m）和内外直径（in）
    /// </summary>
    public class WellSection
    {
        public double sectionLength { get; set; }
        public double outterDiameter { get; set; }
        public double innerDiameter { get; set; }
        public WellSection(double length, double outterDiameter, double innerDiameter)
        {
            sectionLength = length;
            this.outterDiameter = outterDiameter ;
            this.innerDiameter = innerDiameter ;
        }
    }

    /// <summary>
    /// 展示井身单个网格参数，包括深度(m)，外径(mm)，内径(mm)，环空面积(m2),温度(K）
    /// </summary>
    public class WellGrid
    {
        public double wellDepth { get;private set; }
        public double outterDiameter { get;private set; }
        public double innerDiameter { get;private set; }
        public double GridArea { get;private set; }
        public double temperature { get; set; }

        public WellGrid() { }

        public WellGrid(double depth,double outterD,double innerD,double temperature)
        {
            wellDepth = depth;
            outterDiameter = outterD*2.54/100;
            innerDiameter = innerD*2.54/100;
            GridArea = Math.PI * (outterDiameter * outterDiameter - innerDiameter * innerDiameter)/4;
            this.temperature = temperature+273.15;
        }
    }
}
