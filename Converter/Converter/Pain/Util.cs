using System;
using System.Collections.Generic;
using System.Drawing;

namespace Converter.Pain
{
    public static class Util
    {
        static Random rnd = new Random();

        /// <summary>
        /// shuffle a list
        /// based on https://stackoverflow.com/a/1262619
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">the lsit to shuffle</param>
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T x = list[k];
                list[k] = list[n];
                list[n] = x;
            }
        }

        /// <summary>
        /// resize a bitmap. the original bitmap is disposed
        /// </summary>
        /// <param name="org">the original bitmap</param>
        /// <param name="w">new width</param>
        /// <param name="h">new height</param>
        /// <returns>scaled bitmap</returns>
        public static Bitmap Resize(this Bitmap org, int w, int h)
        {
            Bitmap scaled = new Bitmap(w, h);

            // get graphics from the scaled bitmap
            // and draw bitmap scaled
            using (Graphics g = Graphics.FromImage(scaled))
                g.DrawImage(org, 0, 0, w, h);

            org.Dispose();
            return scaled;
        }
    }
}
