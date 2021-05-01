using Converter.Pain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converter
{
    class Rects
    {
        const string osRoot = @"..\..\..\..\..\";
        const string framesDir = osRoot + @"video\frames";
        const string outPath = osRoot + @"src\frames.h";
        static readonly Color primary = Color.Black;
        static readonly Color secondary = Color.White;

        public static void Convert()
        {
            // prerender frames into rects, parallel
            List<RenderedFrame> renderedFrames = new List<RenderedFrame>();
            Parallel.ForEach(Directory.EnumerateFiles(framesDir, "*.png", SearchOption.TopDirectoryOnly), (frameFile) =>
            {
                using (Bitmap frame = new Bitmap(frameFile))
                {
                    // get frame name (comment to array)
                    string frameName = Path.GetFileNameWithoutExtension(frameFile);
                    Console.WriteLine($"Prerender {frameName}");

                    // render frame
                    RenderedFrame rf = new Frame(frame, primary, secondary)
                        .SwapPrimaryAndSecondary()
                        .Render();

                    // and add to list
                    rf.Comment = frameName;
                    renderedFrames.Add(rf);
                }
            });

            // as we prerendered in parallel, the list may not be in order. so we have to order it
            // luckily, the Comment field contains the filename, which would have been the order anyway
            renderedFrames = renderedFrames
                .OrderBy(f => f.Comment)
                .ToList();

            // repeat to tune values quickly
            while (true)
            {
                // get input
                Console.WriteLine();
                Console.WriteLine($"total frames available: {renderedFrames.Count}");
                Console.WriteLine("Enter configuration, CTRL+C to exit");
                Console.Write("frame_skip [200]: ");
                string framesSkipStr = Console.ReadLine();
                Console.Write("frame_count [20]: ");
                string framesCountStr = Console.ReadLine();
                Console.Write("every_nth_frame [0]: ");
                string frameIntervalStr = Console.ReadLine();
                Console.Write("min_rect_size (width * height) [100]: ");
                string minRectSizeStr = Console.ReadLine();
                Console.Write("max_rect_count [50]: ");
                string maxRectCountStr = Console.ReadLine();

                // parse inputs
                int framesSkip, framesCount, frameInterval, minRectSize, maxRectCount;
                if (string.IsNullOrWhiteSpace(framesSkipStr) || !int.TryParse(framesSkipStr, out framesSkip))
                    framesSkip = 200;
                if (string.IsNullOrWhiteSpace(framesCountStr) || !int.TryParse(framesCountStr, out framesCount))
                    framesCount = 20;
                if (string.IsNullOrWhiteSpace(frameIntervalStr) || !int.TryParse(frameIntervalStr, out frameInterval))
                    frameInterval = 20;
                if (string.IsNullOrWhiteSpace(minRectSizeStr) || !int.TryParse(minRectSizeStr, out minRectSize))
                    minRectSize = 100;
                if (string.IsNullOrWhiteSpace(maxRectCountStr) || !int.TryParse(maxRectCountStr, out maxRectCount))
                    maxRectCount = 50;

                // write source
                int sizeEst = 0;
                WriteSourceFile(renderedFrames, framesSkip, framesCount, frameInterval, minRectSize, maxRectCount, ref sizeEst);

                Console.WriteLine($"est. array binary size: {sizeEst} bytes");
            }
        }

        static void WriteSourceFile(List<RenderedFrame> renderedFrames, int skip, int count, int everyNth, int minRectSize, int maxRectCount, ref int estBinSize)
        {
            // prepare c source body
            StringBuilder cSrc = new StringBuilder().AppendLine(@$"
// Generated using Rects converter
// frame_count: {count}
// frame_skip: {skip}
// every_nth: {everyNth}
// min_rect_size (w*h): {minRectSize}
// max_rect_count : {maxRectCount}

#include ""util.h""");
            StringBuilder cArr = new StringBuilder()
                .AppendLine(@"
const u8 rd_end_condition[] = {0x00, 0x00};

const u8 *rectData[] = {");

            // append rectangles of all frames to c source body
            int fs = 0;
            int fc = 0;
            int nth = 0;
            foreach (RenderedFrame frame in renderedFrames)
            {
                // handle skip
                fs++;
                if (fs < skip)
                    continue;

                // handle frame divider
                nth++;
                if (nth < everyNth)
                    continue;
                nth = 0;

                // hanle frame count limit
                fc++;
                if (fc > count)
                    break;

                // append frame
                AppendFrameToC(cSrc, cArr, frame, minRectSize, maxRectCount, ref estBinSize);
            }

            // finish c source
            cArr.AppendLine("rd_end_condition };");
            cSrc.AppendLine(cArr.ToString());

            // add end condition to estimated binary array size
            estBinSize += 2;

            // write to file
            File.WriteAllText(outPath, cSrc.ToString());
        }

        static void AppendFrameToC(StringBuilder cSrc, StringBuilder cArr, RenderedFrame frame, int minRectSize, int maxRectCount, ref int estBinSize)
        {
            /*
             * format of the rects data is as follows:
             * - first two bytes are the screen and rectangle colors
             * - following that are 1..n rectangles:
             *  - 5 bytes per rectangle, with x,y,w,h each with 9 bit and 4 bit flags
             *  - like so: ABCDxxxx xxxxxyyy yyyyyyww wwwwwwwh hhhhhhhh
             *  - flag A indicates that this is the last rectangle, and following it is the start of a new frame
             *  - flags BCD are not used
             * - end condition: screen and rect color are equal
             */

            // prepare rectangle list: sort by size descending, then filter out all rectangles below the minimum size
            List<Rectangle> rects = frame.Rectangles
                .OrderByDescending((r) => r.Width * r.Height)
                .Where((r) => (r.Width * r.Height) > minRectSize)
                .Take(maxRectCount)
                .ToList();

            // prepare member name and add pointer to c array
            string memberName = "rd_" + frame.Comment;
            cArr.Append(memberName).AppendLine(",");

            // write start: comment, then screen color (secondary) and rectangle color (primary)
            cSrc.Append(@$"/* {frame.Comment} ({rects.Count} rects) */
const u8 {memberName}[] = {{
{ConvertColor(frame.Secondary)}, {ConvertColor(frame.Primary)}, 
");

            // add color information to size estimate
            estBinSize += 2;

            // if we have 0 rectangles, add a dummy rectangle
            if (rects.Count <= 0)
            {
                // append rect
                cSrc.Append(ConvertRect(0, 0, 1, 1, true))
                    .AppendLine(",");

                // add to size estimation
                estBinSize += 5;
            }
            else
            {
                // append all rectangles
                for (int i = 0; i < rects.Count; i++)
                {
                    // get rect
                    Rectangle rect = rects[i];
                    bool isLast = i + 1 == rects.Count;

                    // append rect
                    cSrc.Append(ConvertRect((uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height, isLast))
                        .AppendLine(",");

                    // add to size estimation
                    estBinSize += 5;
                }
            }

            // add end condition
            cSrc.AppendLine("};");
        }

        static string ConvertRect(ulong x, ulong y, ulong w, ulong h, bool lastRect,
            bool flagB = false, bool flagC = false, bool flagD = false)
        {
            /*
             * format of the rects data is as follows:
             * - first two bytes are the screen and rectangle colors
             * - following that are 1..n rectangles:
             *  - 5 bytes per rectangle, with x,y,w,h each with 9 bit and 4 bit flags
             *  - like so: ABCDxxxx xxxxxyyy yyyyyyww wwwwwwwh hhhhhhhh
             *  - flag A indicates that this is the last rectangle, and following it is the start of a new frame
             *  - flags BCD are not used
             * - end condition: screen and rect color are equal
             */

            // build flags
            ulong flags = 0;
            if (lastRect)
                flags |= 1 << 3;
            if (flagB)
                flags |= 1 << 2;
            if (flagC)
                flags |= 1 << 1;
            if (flagD)
                flags |= 1;

            //generate data value
            ulong data = 0;
            data |= (flags & 0xF) << 36;
            data |= (x & 0x1FF) << 27;
            data |= (y & 0x1FF) << 18;
            data |= (w & 0x1FF) << 9;
            data |= (h & 0x1FF);

            // split data back into 5 bytes and append to c style array content
            // like this: 0xaa, 0xbb, 0xcc, 0xdd, 0xee
            return new StringBuilder()
                .Append(Hex((byte)((data >> 32) & 0xFF))).Append(", ")
                .Append(Hex((byte)((data >> 24) & 0xFF))).Append(", ")
                .Append(Hex((byte)((data >> 16) & 0xFF))).Append(", ")
                .Append(Hex((byte)((data >> 8) & 0xFF))).Append(", ")
                .Append(Hex((byte)((data) & 0xFF)))
                .ToString();
        }

        static string ConvertColor(Color px)
        {
            byte r = px.R;
            byte g = px.G;
            byte b = px.B;

            // based on COLOR(_r, _g, _b) macro
            byte c = (byte)(0
                | (r & 0x7) << 5
                | (g & 0x7) << 2
                | (b & 0x3) << 0);

            // convert to hex string
            return Hex(c);
        }

        static string Hex(byte b)
        {
            return "0x" + b.ToString("X");
        }
    }
}
