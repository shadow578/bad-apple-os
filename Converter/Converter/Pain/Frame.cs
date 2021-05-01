using System;
using System.Collections.Generic;
using System.Drawing;

namespace Converter.Pain
{
    public class Frame
    {
        private int PRIMARY = 0,
            SECONDARY = 1,
            IGNORE = 2;

        /// <summary>
        /// frame buffer
        /// </summary>
        int[,] frame;

        /// <summary>
        /// frame dimensions
        /// </summary>
        int width, height;

        /// <summary>
        /// primary and secondary colors rgb values
        /// </summary>
        Color primaryRGB, secondaryRGB;

        /// <summary>
        /// init a new frame with Black/White primary/secondary color
        /// </summary>
        /// <param name="of">the image</param>
        public Frame(Bitmap of) : this(of, Color.Black, Color.White) { }

        /// <summary>
        /// init a new frame
        /// </summary>
        /// <param name="of">the image</param>
        /// <param name="primary">primary color</param>
        /// <param name="secondary">secondary color</param>
        public Frame(Bitmap of, Color primary, Color secondary) : this(of, null, primary, secondary)
        {
        }

        /// <summary>
        /// init a new frame
        /// </summary>
        /// <param name="of">the image</param>
        /// <param name="diff">previous image. only differences are added</param>
        /// <param name="primary">primary color</param>
        /// <param name="secondary">secondary color</param>
        public Frame(Bitmap of, Bitmap diff, Color primary, Color secondary)
        {
            width = of.Width;
            height = of.Height;
            primaryRGB = primary;
            secondaryRGB = secondary;

            //build frame
            frame = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    // get deltaE to primary and secondary
                    Color c = of.GetPixel(x, y);
                    float deltaPrimary = c.DeltaETo(primary);
                    float deltaSecondary = c.DeltaETo(secondary);
                    int state = (deltaPrimary < deltaSecondary) ? PRIMARY : SECONDARY;

                    // get deltaE to primary and secondary for previous frame
                    if (diff != null)
                    {
                        Color cd = diff.GetPixel(x, y);
                        float deltaPrimaryDiff = cd.DeltaETo(primary);
                        float deltaSecondaryDiff = cd.DeltaETo(secondary);
                        int stateDiff = (deltaPrimaryDiff < deltaSecondaryDiff) ? PRIMARY : SECONDARY;

                        // set as already drawn if equal state
                        if (state == stateDiff)
                            state = IGNORE;
                    }

                    // set state in frame
                    frame[x, y] = state;
                }
        }

        public RenderedFrame Render()
        {
            return new RenderedFrame
            {
                Rectangles = GetAsRectangles(),
                Primary = GetPrimary(),
                Secondary = GetSecondary()
            };
        }

        /// <summary>
        /// swap the primary and secondary color if the secondary color is more common than the primary
        /// </summary>
        /// <returns>the frame reference</returns>
        public Frame SwapPrimaryAndSecondary()
        {
            // count how many primary and secondary pixels we have
            int p = 0, s = 0;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (frame[x, y] == PRIMARY)
                        p++;
                    else if (frame[x, y] == SECONDARY)
                        s++;

            // if we have more secondary than primary, swap the colors
            if (s < p)
            {
                Color tc = primaryRGB;
                primaryRGB = secondaryRGB;
                secondaryRGB = tc;

                int ti = PRIMARY;
                PRIMARY = SECONDARY;
                SECONDARY = ti;
            }

            return this;
        }

        /// <summary>
        /// primary color rgb value
        /// </summary>
        /// <returns></returns>
        public Color GetPrimary()
        {
            return primaryRGB;
        }

        /// <summary>
        /// secondary color rgb value
        /// </summary>
        /// <returns></returns>
        public Color GetSecondary()
        {
            return secondaryRGB;
        }

        /// <summary>
        /// get this frame as a list of rectangles of the primary color
        /// </summary>
        /// <returns>the rectangle list</returns>
        public List<Rectangle> GetAsRectangles()
        {
            List<Rectangle> rects = new List<Rectangle>();
            Rectangle? rect;
            while ((rect = GetNextRect()).HasValue)
                rects.Add(rect.Value);

            return rects;
        }

        /// <summary>
        /// get the next rect in the primary color
        /// </summary>
        /// <returns>the rect, or null if none left</returns>
        Rectangle? GetNextRect()
        {
            // find the first pixel that is primary color and not yet drawn
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (frame[x, y] == PRIMARY)
                        return TraverseFrom(x, y);

            return null;
        }

        /// <summary>
        /// traverse the next biggest rectangle in the primary color, starting at the start coords.
        /// marks drawn primary color pixels as drawn
        /// </summary>
        /// <param name="startX">the start coord, x</param>
        /// <param name="startY">the start coord, y</param>
        /// <returns>the rectangle</returns>
        Rectangle? TraverseFrom(int startX, int startY)
        {
            int x = startX + 1;
            int y = startY + 1;
            //Console.WriteLine($"i: {startX} j: {startY}");
            frame[startX, startY] = IGNORE;
            bool xLimitHit = false;
            bool yLimitHit = false;
            while ((!xLimitHit || !yLimitHit)
                && x > startX
                && y > startY)
            {
                // hit limit on image bounds
                if (x >= width)
                    xLimitHit = true;

                if (y >= height)
                    yLimitHit = true;

                //find x and y limits
                if (x < width && !xLimitHit)
                    for (int yy = startY; yy < y; yy++)
                        if (frame[x, yy] == SECONDARY)
                        {
                            xLimitHit = true;
                            x--;
                            break;
                        }

                if (y < height && !yLimitHit)
                    for (int xx = startX; xx < x; xx++)
                        if (frame[xx, y] == SECONDARY)
                        {
                            yLimitHit = true;
                            y--;
                            break;
                        }

                // mark as drawn
                if (x < width && !xLimitHit)
                    for (int yy = startY; yy < y; yy++)
                        frame[x, yy] = IGNORE;

                if (y < height && !yLimitHit)
                    for (int xx = startX; xx < x; xx++)
                        frame[xx, y] = IGNORE;

                if (x < width && y < height && !xLimitHit && !yLimitHit)
                    frame[x, y] = IGNORE;

                // increment coords
                if (!xLimitHit)
                    x++;

                if (!yLimitHit)
                    y++;

                //Console.WriteLine($"X: {x}  Y: {y}");
            }

            // calc width and height
            int w = Math.Min(x - startX, width);
            int h = Math.Min(y - startY, height);

            // ensure all are marked drawn
            for (int xx = startX; xx < startX + w; xx++)
                for (int yy = startY; yy < startY + h; yy++)
                    if (xx < width && yy < height)
                        frame[xx, yy] = IGNORE;

            //Console.WriteLine($"R: X: {startX}  Y: {startY}  W: {w}  H: {h}   ({w + startX} / {h + startY})");
            //Console.WriteLine("----------");

            //if (w <= 0 || h <= 0)
            //    return null;

            return new Rectangle(startX, startY, w, h);
        }
    }
}
