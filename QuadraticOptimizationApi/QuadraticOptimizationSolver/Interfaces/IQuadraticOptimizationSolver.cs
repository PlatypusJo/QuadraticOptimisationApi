using QuadraticOptimizationSolver.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadraticOptimizationSolver.Interfaces
{
    public interface IQuadraticOptimizationSolver<T> where T: BaseDataModel
    {
        public double[] Solve(T data);
    }
}
