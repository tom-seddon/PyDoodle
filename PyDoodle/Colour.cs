using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PyDoodle
{
    public struct Colour
    {
        public float r, g, b, a;

        public Colour(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        // this is just too confusing when you call it from python...
        //public Colour(int r, int g, int b, int a = 255) : this(r / 255f, g / 255f, b / 255f, a / 255f) { }

        private int GetByteValue(float f)
        {
            if (f < 0f)
                return 0;
            else if (f > 1f)
                return 255;
            else
                return (int)(f * 255f);
        }

        internal Color AsColor()
        {
            return Color.FromArgb(GetByteValue(a), GetByteValue(r), GetByteValue(g), GetByteValue(b));
        }

        public static Colour operator -(Colour a)
        {
            return new Colour(-a.r, -a.g, -a.b, -a.a);
        }

        public static Colour operator +(Colour a, Colour b)
        {
            return new Colour(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
        }

        public static Colour operator -(Colour a, Colour b)
        {
            return new Colour(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
        }

        public static Colour operator *(float a, Colour b)
        {
            return new Colour(a * b.r, a * b.g, a * b.b, a * b.a);
        }

        public static Colour operator *(Colour a, float b)
        {
            return new Colour(a.r * b, a.g * b, a.b * b, a.a * b);
        }

        public static Colour operator /(Colour a, float b)
        {
            return new Colour(a.r / b, a.g / b, a.b / b, a.a / b);
        }

        public static double Dot(Colour a, Colour b)
        {
            return a.r * b.r + a.g * b.g + a.b * b.b + a.a * b.a;
        }

        public static bool operator ==(Colour a, Colour b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

        public static bool operator !=(Colour a, Colour b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return this == (Colour)obj;//???
        }

        public override int GetHashCode()
        {
            return r.GetHashCode() + g.GetHashCode() + b.GetHashCode() + a.GetHashCode();//???
        }

        //         public static Colour Min(Colour a, Colour b)
        //         {
        //             return new Colour(Math.Min(a.x, b.y), Math.Min(a.y, b.y));
        //         }
        // 
        //         public static Colour Max(Colour a, Colour b)
        //         {
        //             return new Colour(Math.Max(a.x, b.y), Math.Max(a.y, b.y));
        //         }

        public override string ToString()
        {
            return string.Format("Colour({0},{1},{2},{3})", r, g, b, a);
        }
    }
}
