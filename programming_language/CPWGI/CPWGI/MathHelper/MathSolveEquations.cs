using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPWGI.MathHelper
{
   public static class MathSolveEquations
    {
        /// <summary>
        /// 牛顿下山法
        /// </summary>
        /// <param name="f">待求解函数</param>
        /// <param name="df">待求解函数的导数</param>
        /// <param name="root">根的初始值，默认为0</param>
        /// <param name="error">解的误差，默认为0.001</param>
        /// <returns></returns>
        public static double SolveEquationsWithNewtonDownHillMothod(Func<double, double> f, Func<double, double> df, double root = 0, double error = 0.0001)
        {
            double temp = root - 10;
            double fx, fxNew, dfx;
            double w;
            while (Math.Abs(root - temp) > error)
            {
                temp = root;
                fx = f(root);
                dfx = df(root);
                //牛顿下山因子，保证收敛，数值分析299页
                w = 1.0;
                root = temp - fx / dfx;
                fxNew = f(root);
                while (Math.Abs(fxNew) > Math.Abs(fx))
                {
                    w = w / 2;
                    root = temp - w * fx / dfx;
                    fxNew = f(root);
                }
            }
            return root;
        }
    }
}
