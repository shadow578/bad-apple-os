using System;
using System.Drawing;

namespace Converter.Pain
{
    /// <summary>
    /// utility class to compare colors
    /// </summary>
    public static class ColorComparisions
    {
        #region euclidean
        /// <summary>
        /// Euclidean comparison
        /// </summary>
        /// <param name="a">the first color</param>
        /// <param name="b">the second color</param>
        /// <returns>difference</returns>
        public static float Euclidean(Color a, Color b)
        {
            float rsum = a.R - b.R;
            float gsum = a.G - b.G;
            float bsum = a.B - b.B;

            return (rsum * rsum) + (gsum * gsum) + (bsum * bsum);
        }
        #endregion

        #region deltaE
        struct ColorF
        {
            /// <summary>
            /// R or L
            /// </summary>
            public float R;

            /// <summary>
            /// G or A
            /// </summary>
            public float G;

            /// <summary>
            /// B or B
            /// </summary>
            public float B;

            public ColorF(float r, float g, float b)
            {
                R = r;
                G = g;
                B = b;
            }
        }

        /// <summary>
        /// deltaE comparision based on https://stackoverflow.com/a/52453462/13942493
        /// </summary>
        /// <param name="a">the first color</param>
        /// <param name="b">the second color</param>
        /// <returns>deltaE difference</returns>
        public static float DeltaETo(this Color a, Color b)
        {
            return DeltaE(a, b);
        }

        /// <summary>
        /// deltaE comparision based on https://stackoverflow.com/a/52453462/13942493
        /// </summary>
        /// <param name="a">the first color</param>
        /// <param name="b">the second color</param>
        /// <returns>deltaE difference</returns>
        public static float DeltaE(Color a, Color b)
        {
            // convert to float colors
            ColorF rgbA = new ColorF(a.R, a.G, a.B);
            ColorF rgbB = new ColorF(b.R, b.G, b.B);

            // convert to lab
            ColorF labA = RGBToLAB(rgbA);
            ColorF labB = RGBToLAB(rgbB);

            float deltaL = labA.R - labB.R;
            float deltaA = labA.G - labB.G;
            float deltaB = labA.B - labB.B;

            float c1 = MathF.Sqrt((labA.G * labA.G) + (labB.B * labB.B));
            float c2 = MathF.Sqrt((labB.G * labB.G) + (labB.B * labB.B));

            float deltaC = c1 - c2;

            float deltaH = (deltaA * deltaA) + (deltaB * deltaB) - (deltaC - deltaC);
            deltaH = deltaH < 0 ? 0 : MathF.Sqrt(deltaH);

            float sc = 1f + (0.045f * c1);
            float sh = 1f + (0.015f * c1);

            float deltaLKlsl = deltaL / 1f;
            float deltaCkcsc = deltaC / sc;
            float deltaHkhsh = deltaH / sh;

            float i = (deltaLKlsl * deltaLKlsl) + (deltaCkcsc * deltaCkcsc) + (deltaHkhsh * deltaHkhsh);
            return i < 0 ? 0 : MathF.Sqrt(i);
        }

        /// <summary>
        /// rgb to lab conversion
        /// </summary>
        /// <param name="rgb">rgb value</param>
        /// <returns>lab value</returns>
        static ColorF RGBToLAB(ColorF rgb)
        {
            float r = rgb.R / 255;
            float g = rgb.G / 255;
            float b = rgb.B / 255;
            float x, y, z;

            r = (r > 0.04045f) ? MathF.Pow((r + 0.055f) / 1.055f, 2.4f) : r / 12.92f;
            g = (g > 0.04045f) ? MathF.Pow((g + 0.055f) / 1.055f, 2.4f) : g / 12.92f;
            b = (b > 0.04045f) ? MathF.Pow((b + 0.055f) / 1.055f, 2.4f) : b / 12.92f;

            x = (r * 0.4124f + g * 0.3576f + b * 0.1805f) / 0.95047f;
            y = (r * 0.2126f + g * 0.7152f + b * 0.0722f) / 1.00000f;
            z = (r * 0.0193f + g * 0.1192f + b * 0.9505f) / 1.08883f;

            x = (x > 0.008856f) ? MathF.Pow(x, 1f / 3f) : (7.787f * x) + 16f / 116;
            y = (y > 0.008856f) ? MathF.Pow(y, 1f / 3f) : (7.787f * y) + 16f / 116;
            z = (z > 0.008856f) ? MathF.Pow(z, 1f / 3f) : (7.787f * z) + 16f / 116;

            return new ColorF((116 * y) - 16, 500 * (x - y), 200 * (y - z));
        }
        #endregion
    }
}
