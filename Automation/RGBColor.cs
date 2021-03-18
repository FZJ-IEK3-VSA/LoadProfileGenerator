using System.Diagnostics.CodeAnalysis;

namespace Automation
{
    public static class LPGColors {
        public static ColorRGB AliceBlue { get; } = new ColorRGB(240, 248, 255);
        public static ColorRGB Blue { get; } = new ColorRGB(0,0, 255);
        public static ColorRGB Black { get; } = new ColorRGB(0, 0, 0);
    }
    public record ColorRGB
    {
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = R.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ B.GetHashCode();
                return hashCode;
            }
        }
        public ColorRGB() { }

        public ColorRGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            A = 0;
        }

        public ColorRGB(byte a, byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        #region Fields

        #endregion

        /// <summary>
        ///     Gets or sets the Red value.
        /// </summary>
        /// <value> The R. </value>
        public byte R { get; set; }

        public byte A { get; set; }

        /// <summary>
        ///     Gets or sets the Green value.
        /// </summary>
        /// <value> The G. </value>
        public byte G { get; set; }

        /// <summary>
        ///     Gets or sets the Blue value.
        /// </summary>
        /// <value> The B. </value>
        public byte B { get; set; }

        /*/// <summary>
        ///     Implicit conversion of the specified RGB.
        /// </summary>
        /// <param name="rgb"> The RGB. </param>
        /// <returns>color </returns>
        public static implicit operator Color(ColorRGB rgb) => FromColorRGB(rgb);

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static Color FromColorRGB(ColorRGB rgb)
        {
            var c = Color.FromArgb(255, rgb.R, rgb.G, rgb.B);
            return c;
        }*/

        //public override bool Equals(object obj)
        //{
        //    if (!(obj is ColorRGB))
        //    {
        //        return false;
        //    }

        //    return Equals((ColorRGB)obj);
        //}

        //[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        //public bool Equals(ColorRGB other)
        //{
        //    if (R != other.R)
        //    {
        //        return false;
        //    }
        //    if (G != other.G)
        //    {
        //        return false;
        //    }
        //    if (B != other.B)
        //    {
        //        return false;
        //    }
        //    if (A != other.A)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        //public static bool operator ==(ColorRGB point1, ColorRGB point2) => point1.Equals(point2);

        //public static bool operator !=(ColorRGB point1, ColorRGB point2) => !point1.Equals(point2);

    }
}
