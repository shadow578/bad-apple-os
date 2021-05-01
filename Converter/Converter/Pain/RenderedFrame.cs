using System.Collections.Generic;
using System.Drawing;

namespace Converter.Pain
{
    public class RenderedFrame
    {
        public List<Rectangle> Rectangles { get; set; }
        public Color Primary { get; set; }
        public Color Secondary { get; set; }
        public string Comment { get; set; }
    }
}
