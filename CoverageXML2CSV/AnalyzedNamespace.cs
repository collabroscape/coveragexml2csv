﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverageXML2CSV
{
    public class AnalyzedNamespace : AnalyzedBase
    {
        public IEnumerable<AnalyzedClass> Classes { get; set; }
    }
}
