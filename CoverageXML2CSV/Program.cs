using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CoverageXML2CSV
{
    class Program
    {
        static void Main(string[] args)
        {
            AnalysisDepth depth = AnalysisDepth.Method;
            string sourceFile = null;
            string outputFile = null;
            bool proceed = true;

            try
            {
                if (args != null && args.Length == 6)
                {
                    for (int argIndex = 0; argIndex < args.Length; argIndex += 2)
                    {
                        switch (args[argIndex].ToLower())
                        {
                            case "-d":
                            case "/d":
                                depth = (AnalysisDepth)Enum.Parse(typeof(AnalysisDepth), args[argIndex + 1]);
                                break;
                            case "-x":
                            case "/x":
                                sourceFile = args[argIndex + 1];
                                if (string.IsNullOrEmpty(sourceFile))
                                    throw new ArgumentException("No specified coverage XML file");
                                else if (!File.Exists(sourceFile))
                                    throw new ArgumentException("Invalid coverage XML file");
                                break;
                            case "-o":
                            case "/o":
                                outputFile = args[argIndex + 1];
                                if (string.IsNullOrEmpty(outputFile))
                                    throw new ArgumentException("No specified output CSV file path");
                                break;
                        }
                    }

                    if (proceed && !string.IsNullOrEmpty(sourceFile) && !string.IsNullOrEmpty(outputFile))
                    {
                        // do stuff
                        XDocument coverageXml = ReadCoverageXml(sourceFile);
                        if (coverageXml != null)
                        {
                            IEnumerable<AnalyzedModule> modules = ExtractAnalyzedModules(coverageXml);
                            WriteOutputFile(modules, depth, outputFile);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Invalid data, or invalid source/output files");
                    }
                }
                else
                {
                    throw new ArgumentException("Incorrect arguments");
                }
            }
            catch (Exception ex)
            {
                PrintUsage();
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("CoverageXML2CSV.exe : Convert Visual Studio coverage XML files to CSV");
            Console.WriteLine();
            Console.WriteLine("CoverageXML2CSV.exe -d [DEPTH] -x [COVERAGE XML FILE] -o [OUTPUT CSV FILE]");
            Console.WriteLine("\t-d\tDEPTH");
            Console.WriteLine("\t  \tModule|Namespace|Class|Method");
            Console.WriteLine("\t-x\tCOVERAGE XML FILE");
            Console.WriteLine("\t  \tFull path and filename of .coveragexml file");
            Console.WriteLine("\t-o\tOUTPUT CSV FILE");
            Console.WriteLine("\t  \tFull path and filename of CSV file to generate");

        }

        static XDocument ReadCoverageXml(string sourceFile)
        {
            XDocument xdoc = null;

            using (TextReader reader = new StreamReader(sourceFile))
            {
                xdoc = XDocument.Parse(reader.ReadToEnd());
            }

            return xdoc;
        }

        static IEnumerable<AnalyzedModule> ExtractAnalyzedModules(XDocument coverageXml)
        {
            IEnumerable<AnalyzedModule> extracted = from module in coverageXml.Descendants("Module")
                                                    select new AnalyzedModule()
                                                    {
                                                        Name = module.Element("ModuleName").Value,
                                                        LinesCovered = int.Parse(module.Element("LinesCovered").Value),
                                                        LinesPartiallyCovered = int.Parse(module.Element("LinesPartiallyCovered").Value),
                                                        LinesNotCovered = int.Parse(module.Element("LinesNotCovered").Value),
                                                        BlocksCovered = int.Parse(module.Element("BlocksCovered").Value),
                                                        BlocksNotCovered = int.Parse(module.Element("BlocksNotCovered").Value),
                                                        Namespaces = from ns in module.Descendants("NamespaceTable")
                                                                     select new AnalyzedNamespace()
                                                                     {
                                                                         Name = ns.Element("NamespaceName").Value,
                                                                         LinesCovered = int.Parse(ns.Element("LinesCovered").Value),
                                                                         LinesPartiallyCovered = int.Parse(ns.Element("LinesPartiallyCovered").Value),
                                                                         LinesNotCovered = int.Parse(ns.Element("LinesNotCovered").Value),
                                                                         BlocksCovered = int.Parse(ns.Element("BlocksCovered").Value),
                                                                         BlocksNotCovered = int.Parse(ns.Element("BlocksNotCovered").Value),
                                                                         Classes = from cl in ns.Descendants("Class")
                                                                                   select new AnalyzedClass()
                                                                                   {
                                                                                       Name = cl.Element("ClassName").Value,
                                                                                       LinesCovered = int.Parse(cl.Element("LinesCovered").Value),
                                                                                       LinesPartiallyCovered = int.Parse(cl.Element("LinesPartiallyCovered").Value),
                                                                                       LinesNotCovered = int.Parse(cl.Element("LinesNotCovered").Value),
                                                                                       BlocksCovered = int.Parse(cl.Element("BlocksCovered").Value),
                                                                                       BlocksNotCovered = int.Parse(cl.Element("BlocksNotCovered").Value),
                                                                                       Methods = from meth in cl.Descendants("Method")
                                                                                                 select new AnalyzedMethod()
                                                                                                 {
                                                                                                     Name = meth.Element("MethodName").Value,
                                                                                                     LinesCovered = int.Parse(meth.Element("LinesCovered").Value),
                                                                                                     LinesPartiallyCovered = int.Parse(meth.Element("LinesPartiallyCovered").Value),
                                                                                                     LinesNotCovered = int.Parse(meth.Element("LinesNotCovered").Value),
                                                                                                     BlocksCovered = int.Parse(meth.Element("BlocksCovered").Value),
                                                                                                     BlocksNotCovered = int.Parse(meth.Element("BlocksNotCovered").Value),
                                                                                                 }
                                                                                   }
                                                                     }
                                                    };
            return extracted;
        }

        static void WriteOutputFile(IEnumerable<AnalyzedModule> modules, AnalysisDepth depth, string outputFile)
        {

            using (FileStream fileStream = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    // Write header
                    streamWriter.Write("ModuleName,ModuleLinesCovered,ModuleLinesCoveredPct,ModuleLinesPartiallyCovered,ModuleLinesPartiallyCoveredPct,ModuleLinesNotCovered,ModuleLinesNotCoveredPct,ModuleLinesTotal,ModuleBlocksCovered,ModuleBlocksCoveredPct,ModuleBlocksNotCovered,ModuleBlocksNotCoveredPct,ModuleBlocksTotal");
                    if (depth > AnalysisDepth.Module)
                    {
                        streamWriter.Write(",NamespaceName,NamespaceLinesCovered,NamespaceLinesCoveredPct,NamespaceLinesPartiallyCovered,NamespaceLinesPartiallyCoveredPct,NamespaceLinesNotCovered,NamespaceLinesNotCoveredPct,NamespaceLinesTotal,NamespaceBlocksCovered,NamespaceBlocksCoveredPct,NamespaceBlocksNotCovered,NamespaceBlocksNotCoveredPct,NamespaceBlocksTotal");
                        if (depth > AnalysisDepth.Namespace)
                        {
                            streamWriter.Write(",ClassName,ClassLinesCovered,ClassLinesCoveredPct,ClassLinesPartiallyCovered,ClassLinesPartiallyCoveredPct,ClassLinesNotCovered,ClassLinesNotCoveredPct,ClassLinesTotal,ClassBlocksCovered,ClassBlocksCoveredPct,ClassBlocksNotCovered,ClassBlocksNotCoveredPct,ClassBlocksTotal");
                            if (depth > AnalysisDepth.Class)
                            {
                                streamWriter.Write(",MethodName,MethodLinesCovered,MethodLinesCoveredPct,MethodLinesPartiallyCovered,MethodLinesPartiallyCoveredPct,MethodLinesNotCovered,MethodLinesNotCoveredPct,MethodLinesTotal,MethodBlocksCovered,MethodBlocksCoveredPct,MethodBlocksNotCovered,MethodBlocksNotCoveredPct,MethodBlocksTotal");
                            }
                        }
                    }
                    streamWriter.WriteLine();

                    // Write contents
                    foreach (AnalyzedModule module in modules)
                    {
                        if (depth > AnalysisDepth.Module)
                        {
                            // Process each namespace in module
                            foreach (AnalyzedNamespace ns in module.Namespaces)
                            {
                                if (depth > AnalysisDepth.Namespace)
                                {
                                    // Process each class in namespace
                                    foreach (AnalyzedClass cl in ns.Classes) 
                                    {
                                        if (depth > AnalysisDepth.Class)
                                        {
                                            // Process each method in class
                                            foreach (AnalyzedMethod meth in cl.Methods)
                                            {
                                                streamWriter.Write(GetAnalyzedItemOutput(module));
                                                streamWriter.Write(",");
                                                streamWriter.Write(GetAnalyzedItemOutput(ns));
                                                streamWriter.Write(",");
                                                streamWriter.Write(GetAnalyzedItemOutput(cl));
                                                streamWriter.Write(",");
                                                streamWriter.WriteLine(GetAnalyzedItemOutput(meth));
                                            }
                                        }
                                        else
                                        {
                                            streamWriter.Write(GetAnalyzedItemOutput(module));
                                            streamWriter.Write(",");
                                            streamWriter.Write(GetAnalyzedItemOutput(ns));
                                            streamWriter.Write(",");
                                            streamWriter.WriteLine(GetAnalyzedItemOutput(cl));
                                        }
                                    }
                                }
                                else
                                {
                                    streamWriter.Write(GetAnalyzedItemOutput(module));
                                    streamWriter.Write(",");
                                    streamWriter.WriteLine(GetAnalyzedItemOutput(ns));
                                }
                            }
                        }
                        else
                        {
                            streamWriter.WriteLine(GetAnalyzedItemOutput(module));
                        }
                    }

                    // Wrap it up
                    streamWriter.Close();
                }
                fileStream.Close();
            }

        }

        static string GetAnalyzedItemOutput(AnalyzedBase item)
        {
            return string.Format("\"{0}\",{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
                item.Name,
                item.LinesCovered,
                item.LinesCoveredPct.ToString("P2"),
                item.LinesPartiallyCovered,
                item.LinesPartiallyCoveredPct.ToString("P2"),
                item.LinesNotCovered,
                item.LinesNotCoveredPct.ToString("P2"),
                item.LinesTotal,
                item.BlocksCovered,
                item.BlocksCoveredPct.ToString("P2"),
                item.BlocksNotCovered,
                item.BlocksNotCoveredPct.ToString("P2"),
                item.BlocksTotal
                );
        }

    }
}
