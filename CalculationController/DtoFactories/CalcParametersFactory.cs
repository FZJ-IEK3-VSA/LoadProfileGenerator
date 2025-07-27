using Common.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationController.DtoFactories
{
    public class CalcParametersFactory
    {
        public static CalcParameters MakeGoodDefaults()
        {
            return CalcParameters.CreateDefaultParamsForTesting();
        }
    }
}
