using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathematicalExpressionCalculator;

namespace TestProject1
{
    [TestClass]
    public class RationalNumberTest
    {
        [TestMethod]
        public void TestSimplified()
        {
            RationalNumber a, b;
            a = new RationalNumber(114, 514);
            b = a.Simplified;
            Assert.AreEqual(new RationalNumber(57, 257), b);

            a = new RationalNumber(-114, 514);
            b = a.Simplified;
            Assert.AreEqual(new RationalNumber(-57, 257), b);

            a = new RationalNumber(1919810);
            b = a.Simplified;
            Assert.AreEqual(new RationalNumber(1919810), b);

            a = new RationalNumber(-1919810);
            b = a.Simplified;
            Assert.AreEqual(new RationalNumber(-1919810), b);

            a = new RationalNumber(1, 1919810);
            b = a.Simplified;
            Assert.AreEqual(new RationalNumber(1, 1919810), b);

            a = new RationalNumber(1, -1919810);
            b = a.Simplified;
            Assert.AreEqual(new RationalNumber(1, -1919810), b);

            a = new RationalNumber(0);
            b = a.Simplified;
            Assert.AreEqual(new RationalNumber(0), b);
        }

        [TestMethod]
        public void TestIsZero()
        {
            RationalNumber a;
            a = new RationalNumber(0);
            Assert.IsTrue(a.IsZero);

            a = new RationalNumber(10, 24);
            Assert.IsFalse(a.IsZero);

            a = new RationalNumber(-1, 2);
            Assert.IsFalse(a.IsZero);
        }

        [TestMethod]
        public void TestIsOne()
        {
            RationalNumber a;
            a = new RationalNumber(1);
            Assert.IsTrue(a.IsOne);

            a = new RationalNumber(0);
            Assert.IsFalse(a.IsOne);

            a = new RationalNumber(-1);
            Assert.IsFalse(a.IsOne);

            a = new RationalNumber(10, 24);
            Assert.IsFalse(a.IsOne);
        }

        [TestMethod]
        public void TestIsMinusOne()
        {
            RationalNumber a;
            a = new RationalNumber(-1);
            Assert.IsTrue(a.IsMinusOne);

            a = new RationalNumber(0);
            Assert.IsFalse(a.IsMinusOne);

            a = new RationalNumber(1);
            Assert.IsFalse(a.IsMinusOne);

            a = new RationalNumber(-10, 24);
            Assert.IsFalse(a.IsMinusOne);

            a = new RationalNumber(10, 24);
            Assert.IsFalse(a.IsMinusOne);
        }

        [TestMethod]
        public void TestIsInteger()
        {
            RationalNumber a;
            a = new RationalNumber(-1);
            Assert.IsTrue(a.IsInteger);

            a = new RationalNumber(0);
            Assert.IsTrue(a.IsInteger);

            a = new RationalNumber(1);
            Assert.IsTrue(a.IsInteger);

            a = new RationalNumber(10, 10);
            Assert.IsTrue(a.IsInteger);

            a = new RationalNumber(-16, 8);
            Assert.IsTrue(a.IsInteger);

            a = new RationalNumber(10, 20);
            Assert.IsFalse(a.IsInteger);

            a = new RationalNumber(-20, 22);
            Assert.IsFalse(a.IsInteger);
        }

        [TestMethod]
        public void TestPlus()
        {
            RationalNumber a, b, c;
            a = new RationalNumber(1, 2);
            b = new RationalNumber(2, 3);
            c = a + b; // 1/2+2/3=3/6+4/6=7/6
            Assert.AreEqual(new RationalNumber(7, 6), c);

            a = new RationalNumber(-1, 2);
            b = new RationalNumber(2, 3);
            c = a + b; // -1/2+2/3=-3/6+4/6=1/6
            Assert.AreEqual(new RationalNumber(1, 6), c);

            a = new RationalNumber(1, 2);
            b = new RationalNumber(-2, 3);
            c = a + b; // 1/2-2/3=3/6-4/6=-1/6
            Assert.AreEqual(new RationalNumber(-1, 6), c);

            a = new RationalNumber(-1, 2);
            b = new RationalNumber(-2, 3);
            c = a + b; // -1/2-2/3=-3/6-4/6=-7/6
            Assert.AreEqual(new RationalNumber(-7, 6), c);
        }

        [TestMethod]
        public void TestMinus()
        {
            RationalNumber a, b, c;
            a = new RationalNumber(1, 2);
            b = new RationalNumber(2, 3);
            c = a - b; // 1/2-2/3=3/6-4/6=-1/6
            Assert.AreEqual(new RationalNumber(-1, 6), c);

            a = new RationalNumber(-1, 2);
            b = new RationalNumber(2, 3);
            c = a - b; // -1/2-2/3=-3/6-4/6=-7/6
            Assert.AreEqual(new RationalNumber(-7, 6), c);

            a = new RationalNumber(1, 2);
            b = new RationalNumber(-2, 3);
            c = a - b; // 1/2-(-2/3)=3/6+4/6=7/6
            Assert.AreEqual(new RationalNumber(7, 6), c);

            a = new RationalNumber(-1, 2);
            b = new RationalNumber(-2, 3);
            c = a - b; // -1/2-(-2/3)=-3/6+4/6=1/6
            Assert.AreEqual(new RationalNumber(1, 6), c);
        }

        [TestMethod]
        public void TestMinus2()
        {
            RationalNumber a, b;
            a = new RationalNumber(1, 2);
            b = -a;
            Assert.AreEqual(new RationalNumber(-1, 2), b);

            a = new RationalNumber(-1, 2);
            b = -a;
            Assert.AreEqual(new RationalNumber(1, 2), b);

            a = new RationalNumber(0);
            b = -a;
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void TestTimes()
        {
            RationalNumber a, b, c;
            a = new RationalNumber(10, 11);
            b = new RationalNumber(17, 18);
            c = a * b; // (10/11)*(17/18)=170/198=85/99
            Assert.AreEqual(new RationalNumber(85, 99), c);

            a = new RationalNumber(-10, 11);
            b = new RationalNumber(17, 18);
            c = a * b; // (-10/11)*(17/18)=-170/198=-85/99
            Assert.AreEqual(new RationalNumber(-85, 99), c);

            a = new RationalNumber(-10, 11);
            b = new RationalNumber(17, -18);
            c = a * b; // (-10/11)*(-17/18)=-170/198=-85/99
            Assert.AreEqual(new RationalNumber(85, 99), c);
        }

        [TestMethod]
        public void TestDivide()
        {
            RationalNumber a, b, c;
            a = new RationalNumber(10, 11);
            b = new RationalNumber(17, 18);
            c = a / b; // (10/11)/(17/18)=(10/11)*(18/17)=180/187
            Assert.AreEqual(new RationalNumber(180, 187), c);

            a = new RationalNumber(-10, 11);
            b = new RationalNumber(17, 18);
            c = a / b; // (-10/11)/(17/18)=(-10/11)*(18/17)=-180/187
            Assert.AreEqual(new RationalNumber(-180, 187), c);

            a = new RationalNumber(-10, 11);
            b = new RationalNumber(17, -18);
            c = a / b; // (-10/11)/(-17/18)=(-10/11)*(-18/17)=180/187
            Assert.AreEqual(new RationalNumber(180, 187), c);
        }

        [TestMethod]
        public void TestParse()
        {
            RationalNumber a;

            a = RationalNumber.Parse("0");
            Assert.AreEqual(0, a);

            a = RationalNumber.Parse("10");
            Assert.AreEqual(10, a);

            a = RationalNumber.Parse("14/20");
            Assert.AreEqual(new RationalNumber(14, 20), a);

            a = RationalNumber.Parse("114514.1919810");
            Assert.AreEqual(114514 + new RationalNumber(1919810, 10000000), a);

            a = RationalNumber.Parse("-10");
            Assert.AreEqual(-10, a);

            a = RationalNumber.Parse("-14/20");
            Assert.AreEqual(-new RationalNumber(14, 20), a);

            a = RationalNumber.Parse("-114514.1919810");
            Assert.AreEqual(-114514 - new RationalNumber(1919810, 10000000), a);
        }

        [TestMethod]
        public void TestEquals()
        {
            RationalNumber a, b, c, d;
            a = new RationalNumber(10, 20);
            b = new RationalNumber(1, 2);
            c = new RationalNumber(-6, 12);
            d = new RationalNumber(6, 8);
            Assert.IsTrue(a.Equals(a));
            Assert.IsTrue(a.Equals(b));
            Assert.IsFalse(a.Equals(c));
            Assert.IsFalse(a.Equals(d));
            Assert.IsTrue(b.Equals(b));
            Assert.IsFalse(b.Equals(c));
            Assert.IsFalse(b.Equals(d));
            Assert.IsTrue(c.Equals(c));
            Assert.IsFalse(c.Equals(d));
            Assert.IsTrue(d.Equals(d));

            c = new RationalNumber(0, 8);
            d = new RationalNumber(0, -4);
            Assert.IsTrue(c.Equals(c));
            Assert.IsTrue(d.Equals(d));
            Assert.IsTrue(c.Equals(d));
            Assert.IsFalse(a.Equals(c));
            Assert.IsFalse(a.Equals(d));
        }

        [TestMethod]
        public void TestAbsolutelyEquals()
        {
            RationalNumber a, b, c, d;
            a = new RationalNumber(10, 20);
            b = new RationalNumber(1, 2);
            c = new RationalNumber(-6, 12);
            d = new RationalNumber(6, 8);
            Assert.IsTrue(a.AbsolutelyEquals(a));
            Assert.IsFalse(a.AbsolutelyEquals(b));
            Assert.IsFalse(a.AbsolutelyEquals(c));
            Assert.IsFalse(a.AbsolutelyEquals(d));
            Assert.IsTrue(b.AbsolutelyEquals(b));
            Assert.IsFalse(b.AbsolutelyEquals(c));
            Assert.IsFalse(b.AbsolutelyEquals(d));
            Assert.IsTrue(c.AbsolutelyEquals(c));
            Assert.IsFalse(c.AbsolutelyEquals(d));
            Assert.IsTrue(d.AbsolutelyEquals(d));

            c = new RationalNumber(0, 8);
            d = new RationalNumber(0, -4);
            Assert.IsTrue(c.AbsolutelyEquals(c));
            Assert.IsTrue(d.AbsolutelyEquals(d));
            Assert.IsFalse(c.AbsolutelyEquals(d));
            Assert.IsFalse(a.AbsolutelyEquals(c));
            Assert.IsFalse(a.AbsolutelyEquals(d));
        }

        [TestMethod]
        public void TestCompareTo()
        {
            RationalNumber a, b, c, d;
            a = new RationalNumber(10, 20);
            b = new RationalNumber(1, 2);
            c = new RationalNumber(-6, 12);
            d = new RationalNumber(0);

            Assert.AreEqual(0, a.CompareTo(a));
            Assert.AreEqual(0, a.CompareTo(b));
            Assert.AreEqual(1, a.CompareTo(c));
            Assert.AreEqual(1, a.CompareTo(d));
            Assert.AreEqual(0, b.CompareTo(a));
            Assert.AreEqual(0, b.CompareTo(b));
            Assert.AreEqual(1, b.CompareTo(c));
            Assert.AreEqual(1, b.CompareTo(d));
            Assert.AreEqual(-1, c.CompareTo(a));
            Assert.AreEqual(-1, c.CompareTo(b));
            Assert.AreEqual(0, c.CompareTo(c));
            Assert.AreEqual(-1, c.CompareTo(d));
            Assert.AreEqual(-1, d.CompareTo(a));
            Assert.AreEqual(-1, d.CompareTo(b));
            Assert.AreEqual(1, d.CompareTo(c));
            Assert.AreEqual(0, d.CompareTo(d));
        }
    }
}
