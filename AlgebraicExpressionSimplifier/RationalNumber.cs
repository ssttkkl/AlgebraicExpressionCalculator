using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace MathematicalExpressionCalculator
{
    public struct RationalNumber : IComparable<RationalNumber>, IEquatable<RationalNumber>
    {
        private static BigInteger GCD(BigInteger x, BigInteger y)
        {
            return y == 0 ? x : GCD(y, x % y);
        }
        public int Sign { get; set; }
        public BigInteger Numerator { get; set; }
        public BigInteger Denominator { get; set; }
        public RationalNumber(BigInteger num) : this(num, BigInteger.One) { }
        public RationalNumber(BigInteger numerator, BigInteger denominator) : this(numerator, denominator, 1) { }
        private RationalNumber(BigInteger numerator, BigInteger denominator, int flag)
        {
            Sign = numerator.Sign * denominator.Sign * flag;
            if (Sign != 0 && denominator.IsZero)
                throw new DivideByZeroException();
            Numerator = numerator * numerator.Sign;
            Denominator = denominator * denominator.Sign;
        }
        public static implicit operator RationalNumber(int num)
        {
            return new RationalNumber(num);
        }
        public static implicit operator RationalNumber(BigInteger num)
        {
            return new RationalNumber(num);
        }
        public static explicit operator double(RationalNumber num)
        {
            return num.Sign * ((double)num.Numerator / (double)num.Denominator);
        }
        public static explicit operator BigInteger(RationalNumber num)
        {
            return num.Sign * (num.Numerator / num.Denominator);
        }
        public static RationalNumber Parse(string text)
        {
            if (text.Length == 0)
                throw new ArgumentException("Argument is empty");

            if (text[0] == '-')
                return -Parse(text.Substring(1));
            if (text.All(it => char.IsDigit(it)))
                return BigInteger.Parse(text);
            if (text.Count(it => it == '/') == 1)
            {
                var arr = text.Split('/');
                return new RationalNumber(BigInteger.Parse(arr[0]), BigInteger.Parse(arr[1]));
            }
            if (text.Count(it => it == '.') == 1)
            {
                var arr = text.Split('.');
                var ans = (RationalNumber)BigInteger.Parse(arr[0]);

                var k = new RationalNumber(1, 10);
                for (int i = 0; i < arr[1].Length; i++)
                {
                    ans += k * int.Parse(arr[1][i].ToString());
                    k = new RationalNumber(1, k.Denominator * 10);
                }
                return ans;
            }
            throw new ArgumentException("Illegal Argument: " + text);
        }
        public RationalNumber Simplified
        {
            get
            {
                if (Sign == 0)
                    return new RationalNumber(0);
                var gcd = GCD(Numerator, Denominator);
                if (gcd.IsOne)
                    return this;
                else
                    return new RationalNumber(Numerator / gcd, Denominator / gcd, Sign);
            }
        }
        public bool IsZero { get => Sign == 0; }
        public bool IsOne { get => Sign == 1 && Numerator == Denominator; }
        public bool IsMinusOne { get => Sign == -1 && Numerator == Denominator; }
        public bool IsInteger { get => Sign == 0 || Numerator % Denominator == 0; }

        public override bool Equals(object obj)
        {
            return obj is RationalNumber number && Equals(number);
        }

        public bool Equals(RationalNumber other)
        {
            return Simplified.AbsolutelyEquals(other.Simplified);
        }

        public bool AbsolutelyEquals(RationalNumber other)
        {
            return Sign == other.Sign &&
                   Numerator.Equals(other.Numerator) &&
                   Denominator.Equals(other.Denominator);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Sign, Numerator, Denominator);
        }

        public static RationalNumber operator -(RationalNumber x)
        {
            return new RationalNumber(x.Numerator, x.Denominator, -x.Sign).Simplified;
        }
        public static RationalNumber operator +(RationalNumber x, RationalNumber y)
        {
            if (x.Sign * y.Sign == 1)
            {
                var lcm = x.Denominator * y.Denominator / GCD(x.Denominator, y.Denominator);
                var ans = new RationalNumber(x.Numerator * lcm / x.Denominator + y.Numerator * lcm / y.Denominator, lcm, x.Sign).Simplified;
                return ans;
            }
            else if (x.Sign * y.Sign == -1)
            {
                return x.Sign == 1 ? x - (-y) : y - (-x);
            }
            else
            {
                return x.Sign == 0 ? y : x;
            }
        }
        public static RationalNumber operator -(RationalNumber x, RationalNumber y)
        {
            if (x.Sign * y.Sign == 1)
            {
                // x<0, y<0, x-y=(-|x|)-(-|y|)=-(|x|-|y|)
                var lcm = x.Denominator * y.Denominator / GCD(x.Denominator, y.Denominator);
                var ans = new RationalNumber(x.Numerator * lcm / x.Denominator - y.Numerator * lcm / y.Denominator, lcm, x.Sign).Simplified;
                return ans;
            }
            else if (x.Sign * y.Sign == -1)
            {
                // x>0, y<0, x-y=|x|-(-|y|)=|x|+|y|=x+(-y)
                // x<0, y>0, x-y=(-|x|)-|y|=-(|x|+|y|)=-(-x+y)
                return x.Sign == 1 ? x + (-y) : -((-x) + y);
            }
            else
            {
                return x.Sign == 0 ? -y : x;
            }
        }

        public static RationalNumber operator *(RationalNumber x, RationalNumber y)
        {
            return new RationalNumber(x.Numerator * y.Numerator, x.Denominator * y.Denominator, x.Sign * y.Sign).Simplified;
        }
        public static RationalNumber operator /(RationalNumber x, RationalNumber y)
        {
            if (y.IsZero)
                throw new DivideByZeroException();
            return new RationalNumber(x.Numerator * y.Denominator, x.Denominator * y.Numerator, x.Sign * y.Sign).Simplified;
        }

        public static bool operator ==(RationalNumber left, RationalNumber right)
        {
            return EqualityComparer<RationalNumber>.Default.Equals(left, right);
        }

        public static bool operator !=(RationalNumber left, RationalNumber right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            if (Denominator == 1)
            {
                return Sign switch
                {
                    0 => "0",
                    1 => $"{Numerator}",
                    -1 => $"-{Numerator}",
                    _ => throw new InvalidOperationException("Sign must be 0, 1 or -1"),
                };
            }
            else
            {
                return Sign switch
                {
                    0 => "0",
                    1 => $"({Numerator}/{Denominator})",
                    -1 => $"(-{Numerator}/{Denominator})",
                    _ => throw new InvalidOperationException("Sign must be 0, 1 or -1"),
                };
            }
        }

        public int CompareTo(RationalNumber other)
        {
            return (this - other).Sign;
        }

        public static bool operator >(RationalNumber left, RationalNumber right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(RationalNumber left, RationalNumber right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(RationalNumber left, RationalNumber right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(RationalNumber left, RationalNumber right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}