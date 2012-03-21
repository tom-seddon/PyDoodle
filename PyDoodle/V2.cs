using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PyDoodle
{
    public struct V2
    {
        public static readonly V2 V00 = new V2(0, 0);

        public double x, y;

        public V2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public static V2 FromAngle(double angle)
        {
            return new V2(Math.Sin(angle), Math.Cos(angle));
        }

        internal PointF AsPointF()
        {
            return new PointF((float)this.x, (float)this.y);
        }

        //         public static V2 FromPoint(Point p)
        //         {
        //             return new V2((double)p.X, (double)p.Y);
        //         }

        public static V2 operator -(V2 a)
        {
            return new V2(-a.x, -a.y);
        }

        public static V2 operator +(V2 a, V2 b)
        {
            return new V2(a.x + b.x, a.y + b.y);
        }

        public static V2 operator -(V2 a, V2 b)
        {
            return new V2(a.x - b.x, a.y - b.y);
        }

        public static V2 operator *(double a, V2 b)
        {
            return new V2(a * b.x, a * b.y);
        }

        public static V2 operator *(V2 a, double b)
        {
            return new V2(a.x * b, a.y * b);
        }

        public static V2 operator /(V2 a, double b)
        {
            return new V2(a.x / b, a.y / b);
        }

        public static double Dot(V2 a, V2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static bool operator ==(V2 a, V2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(V2 a, V2 b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == (V2)obj;//???
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode();//???
        }

        public double GetLengthSq()
        {
            return Dot(this, this);
        }

        public double GetLength()
        {
            return Math.Sqrt(GetLengthSq());
        }

        public V2 GetNormalised()
        {
            double len = GetLength();
            if (len == 0.0)
                return this;
            else
                return this / len;
        }

        public V2 GetPerp()
        {
            return new V2(-y, x);
        }

        //         public PointF GetPointF()
        //         {
        //             return new PointF((float)x, (float)y);
        //         }

        //         public SizeF GetSizeF()
        //         {
        //             return new SizeF((float)x, (float)y);
        //         }

        public static V2 Min(V2 a, V2 b)
        {
            return new V2(Math.Min(a.x, b.y), Math.Min(a.y, b.y));
        }

        public static V2 Max(V2 a, V2 b)
        {
            return new V2(Math.Max(a.x, b.y), Math.Max(a.y, b.y));
        }

        public override string ToString()
        {
            return string.Format("V2({0},{1})", x, y);
        }
    }
}
