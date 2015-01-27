using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverageXML2CSV
{
    public class AnalyzedModule : AnalyzedBase
    {
        public IEnumerable<AnalyzedNamespace> Namespaces { get; set; }
    }
}
