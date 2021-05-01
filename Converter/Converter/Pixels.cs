using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Converter
{
    /// <summary>
    /// pixel render format. works for 20 frames, after that we're out of memory -_-
    /// </summary>
    class Pixels
    {
        const string osRoot = @"..\..\..\..\..\";
        const string framesDir = osRoot + @"video\frames500";
        const string outPath = osRoot + @"src\frames.h";
        const int SKIP_FRAMES = 200;
        const int MAX_FRAMES = 20;

        public static void Convert()
        {
            // prepare c jagged array for frames
            StringBuilder jaggedArrray = new StringBuilder()
                .AppendLine(@"
// jagged array of all frames
const u8 *frames[] = {");

            // prepare c code
            StringBuilder cppBody = new StringBuilder()
                .AppendLine(@"
// generated using Pixels Converter
#include ""util.h""
");

            // for every frame
            int fn = 0;
            foreach (string frameFile in Directory.EnumerateFiles(framesDir, "*.png", SearchOption.TopDirectoryOnly))
                using (Bitmap frame = new Bitmap(frameFile))
                {
                    fn++;
                    if (fn > (SKIP_FRAMES + MAX_FRAMES))
                        break;
                    if (fn < SKIP_FRAMES)
                        continue;

                    // get frame name (comment to array)
                    string frameName = Path.GetFileNameWithoutExtension(frameFile);
                    string memberName = $"f_{frameName}";
                    Console.WriteLine($"Process {frameName} as {memberName}");

                    // append the frame as own array to the file
                    cppBody.AppendLine($@"
// {frameName}
const u8 {memberName}[] = {ConvertFrameCpp(frame)};");

                    // append entry in jagged frames array
                    jaggedArrray.Append(memberName).AppendLine(",");
                }

            // finish frames array
            jaggedArrray.AppendLine("f_endcondition };");

            // finish c code body
            cppBody.AppendLine(@"
// end condition
const u8 f_endcondition[] = {0x0, 0x0};
")
                .AppendLine(jaggedArrray.ToString());


            //write to file
            File.WriteAllText(outPath, cppBody.ToString());

        }

        static string ConvertFrameCpp(Bitmap frame)
        {
            StringBuilder data = new StringBuilder()
                .AppendLine("{");

            void AppendPx(string px, int repeats)
            {
                data.Append($@"{px},{Hex(repeats)},");
            }

            // compress equal pixels
            string lastPx = null;
            int pxRepeats = 0;
            for (int y = 0; y < frame.Height; y++)
                for (int x = 0; x < frame.Width; x++)
                {
                    string px = ConvertPixel(frame.GetPixel(x, y));

                    // correct first pixel
                    if (lastPx == null)
                        lastPx = px;

                    if (px.Equals(lastPx) && pxRepeats < 255)
                    {
                        // same pixel and limit of 255 not exceeded
                        // just add another pixel
                        pxRepeats++;
                    }
                    else
                    {
                        // a different pixel OR limit of 255 exceeded, append pixel data
                        // as [pixel, repeats]
                        AppendPx(lastPx, pxRepeats);

                        // and reset with new pixel data
                        lastPx = null;
                        pxRepeats = 1;
                    }
                }

            // add remaining pixels
            if (lastPx != null)
                AppendPx(lastPx, pxRepeats);

            // add end condition
            data.Append("0x0,0x0 }");
            return data.ToString();
        }

        static string ConvertPixel(Color px)
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

        static string Hex(int b)
        {
            return "0x" + b.ToString("X");
        }
    }
}
