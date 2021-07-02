using CPWGI.MathHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWGI.MultiphaseCalculate
{
    abstract public class calculateMultiphaseBaseClass
    {
        /// <summary>
        /// 设置摩阻系数计算精度，默认0.0001
        /// </summary>
        public double frictionFactorError { get; set; } = 0.0001;
        /// <summary>
        /// 设置含气率计算精度，默认0.0001
        /// </summary>
        public double holdupQasError { get; set; } = 0.0001;

        /// <summary>
        /// 设置摩阻系数计算方法，默认为Corebook公式
        /// </summary>
        public CalFrictionMethods CalFMethod { get; set; } = CalFrictionMethods.Corebook;

        /// <summary>
        /// 当前环空流型
        /// </summary>
        public rheologys rheology { get; set; } = rheologys.unknown;
        public double outterDiameter { get; set; }
        public double innerDiameter { get; set; }
        /// <summary>
        /// 含气率
        /// </summary>
        public double holdupGas { get; set; }
        /// <summary>
        /// 持液率，与含气率同步更新
        /// </summary>
        public double holdupLiquid { get { return 1 - holdupGas; } }
        /// <summary>
        /// 单位长度压降
        /// </summary>
        public double dPdZ { get; set; }
        public double dPdZf { get; set; }
        protected const double gravity = 9.81;

        /// <summary>
        /// 设置环空内外径并更新相关参数
        /// </summary>
        abstract public void setAnnular(double outterDiameter, double innerDiameter);

        /// <summary>
        /// 判断当前条件下环空流态并返回含气率（需要先设置环空内外径）
        /// </summary>
        abstract public double calculateHoldupGas(double volocitySuperficalGas, double volocitySuperficalLiquid, double densityGas, double densityLiquid, double viscosityGas, double viscosityLiquid, double tensionInterface);

        /// <summary>
        /// 计算当前条件下环空单位压降（请先调用calculateHoldupGas计算含气率）
        /// </summary>
        abstract public double calculatedPdZ(double volocitySuperficalGas, double volocitySuperficalLiquid, double densityGas, double densityLiquid, double viscosityGas, double viscosityLiquid);
    }

    /// <summary>
    /// 流型枚举
    /// </summary>
    public enum rheologys { singleMud,disperedBubble, bubble, slug, churn, annular, unknown };
    
    /// <summary>
    /// 摩阻系数计算方法枚举
    /// </summary>
    public enum CalFrictionMethods{ Fanning, Corebook,Simple }
}
