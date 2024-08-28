using System.Collections.Generic;

namespace razor.Components.Models
{
    public class PieData
    {
        public PieData()
        {
            ChartName = "";
            TotalItems = 0;
            Segments = new List<PieSegment>();
        }

        public string ChartName { get; set; }
        public int TotalItems { get; set; }
        public List<PieSegment> Segments { get; set; }
    }

    public class PieSegment
    {
        public PieSegment()
        {
            Name = "";
            Percent = 0;
            Color = "";
        }
        public string Name { get; set; }
        public double Percent { get; set; }
        public string Color { get; set; }
    }

    /// <summary>
    /// Names and counts of segments
    /// </summary>
    public class RawSegment
    {
        public RawSegment()
        {
            Name = "";
            Count = 0;
        }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}

