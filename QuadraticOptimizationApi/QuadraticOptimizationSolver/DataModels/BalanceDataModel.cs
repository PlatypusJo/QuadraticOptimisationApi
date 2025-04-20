using QuadraticOptimizationApi.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadraticOptimizationSolver.DataModels
{
    public class BalanceDataModel : BaseDataModel
    {
        #region Properties

        public double[,] MatrixA { get; set; }
        public double[] VectorY { get; set; }
        public double[] Tolerance { get; set; }
        public double[] VectorI { get; set; }
        public double[] VectorX0 { get; set; }
        public (RangeDto metrologicRange, RangeDto technologicRange)[] FlowRanges { get; set; }
        public bool[] FlowMeasured { get; set; }

        #endregion

        #region Constructors

        public BalanceDataModel() { }

        public BalanceDataModel(double[,] matrixA, double[] vectorY, double[] tolerance, double[] vectorI, double[] vectorX0)
        {
            MatrixA = matrixA;
            VectorY = vectorY;
            Tolerance = tolerance;
            VectorI = vectorI;
            VectorX0 = vectorX0;
        }

        #endregion
    }
}
