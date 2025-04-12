using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadraticOptimizationSolver.DataModels
{
    public class FunctionMinimizationDataModel : BaseDataModel
    {
        #region Properties

        public string Function { get; set; }

        public double FirstLimitX { get; set; }

        public double FirstLimitY { get; set; }

        public double FirstLimit { get; set; }

        public double SecondLimitX { get; set; }

        public double SecondLimitY { get; set; }

        public double SecondLimit { get; set; }

        #endregion

        #region Constructors

        public FunctionMinimizationDataModel() { }

        public FunctionMinimizationDataModel(string function, double firstLimitX, double firstLimitY, double firstLimit, double secondLimitX, double secondLimitY, double secondLimit)
        {
            Function = function;
            FirstLimitX = firstLimitX;
            FirstLimitY = firstLimitY;
            FirstLimit = firstLimit;
            SecondLimitX = secondLimitX;
            SecondLimitY = secondLimitY;
            SecondLimit = secondLimit;
        }

        #endregion
    }
}
