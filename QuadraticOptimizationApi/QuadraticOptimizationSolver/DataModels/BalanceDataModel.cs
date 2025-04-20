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

        /// <summary>
        /// Матрица баланса
        /// </summary>
        public double[,] MatrixA { get; set; }
        /// <summary>
        /// Вектор Y
        /// </summary>
        public double[] VectorY { get; set; }
        /// <summary>
        /// Допуски
        /// </summary>
        public double[] Tolerance { get; set; }
        /// <summary>
        /// Вектор I
        /// </summary>
        public double[] VectorI { get; set; }
        /// <summary>
        /// Вектор измеренных начальных значений
        /// </summary>
        public double[] VectorX0 { get; set; }
        /// <summary>
        /// Интервальные ограничения потоков
        /// </summary>
        public (RangeDto metrologicRange, RangeDto technologicRange)[] FlowRanges { get; set; }
        /// <summary>
        /// Признак наличия датчика у измеряемого потока
        /// </summary>
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
