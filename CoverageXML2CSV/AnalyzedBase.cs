using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverageXML2CSV
{
    public abstract class AnalyzedBase
    {
        public string Name { get; set; }
        public int LinesCovered { get; set; }
        public int LinesPartiallyCovered { get; set; }
        public int LinesNotCovered { get; set; }
        public int LinesTotal { get { return LinesCovered + LinesPartiallyCovered + LinesNotCovered; } }
        public double LinesCoveredPct { get { return (double)LinesCovered / (double)LinesTotal; } }
        public double LinesPartiallyCoveredPct { get { return (double)LinesPartiallyCovered / (double)LinesTotal; } }
        public double LinesNotCoveredPct { get { return (double)LinesNotCovered / (double)LinesTotal; } }
        public int BlocksCovered { get; set; }
        public int BlocksNotCovered { get; set; }
        public int BlocksTotal { get { return BlocksCovered + BlocksNotCovered; } }
        public double BlocksCoveredPct { get { return (double)BlocksCovered / (double)BlocksTotal; } }
        public double BlocksNotCoveredPct { get { return (double)BlocksNotCovered / (double)BlocksTotal; } }
    }
}
