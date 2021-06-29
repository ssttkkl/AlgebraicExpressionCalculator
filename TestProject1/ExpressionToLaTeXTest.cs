using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using MathematicalExpressionCalculator;

namespace TestProject1
{
    [TestClass]
    public class ExpressionToLaTeXTest
    {
        [TestMethod]
        public void Test1()
        {
            var context = new ExpressionContext();
            var ep = ExpressionParser.Parse("e^(x^2/a^2+y^2/b^2)", context);
            var latex = ep.ToLaTeX();
            Console.WriteLine(latex);
        }
    }
}
