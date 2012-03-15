using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using System.IO;
using System.Diagnostics;

namespace BundleFinder
{
    class Program
    {
        static uint TotalFiles = 0;
        static uint TotalProcessed = 0;
        static uint UniqueFiles = 0;
        static uint SharedFiles = 0;

        static string[] ignoreGroup = 
        {
            "manifest.dsx",
            "content/runtime/webLinks/daz productions, inc",
            "content/readme",
        };

        static Dictionary<string, List<string>> ProdDatabase = new Dictionary<string, List<string>>();
        static Dictionary<string, List<string>> FormattedDupDict = new Dictionary<string, List<string>>();
        static StringBuilder Report = new StringBuilder();

        static void Main(string[] args)
        {
            for (int i = 0; i < ignoreGroup.Length; ++i)
                ignoreGroup[i] = ignoreGroup[i].ToLower();

            DirectoryInfo d = new DirectoryInfo("Y:\\Zips Project\\Completed Zip Files");
            List<DirectoryInfo> subDirs = d.GetDirectories().ToList<DirectoryInfo>();
            Report.AppendLine("\t\t Zip Files That Could Not Be Read");
            foreach (DirectoryInfo dir in subDirs)
            {
                //Generate Product List
                foreach (FileInfo f in dir.GetFiles())
                {
                    Console.WriteLine("Processing " + f.Name);
                    if (f.Extension != ".zip")
                        continue;
                    else
                    {
                        List<string> fileList = GenerateZipList(f.FullName);
                        //Add product name to dictionary for interection files
                        foreach (string fileName in fileList)
                        {
                            ++TotalProcessed;
                            if (ProdDatabase.ContainsKey(fileName))
                                ProdDatabase[fileName].Add(f.Name);
                            else
                            {
                                ProdDatabase[fileName] = new List<string>();
                                ProdDatabase[fileName].Add(f.Name);
                            }
                        }
                    }
                }
            }

            //Generate common file list
            foreach (string file in ProdDatabase.Keys)
            {
                if (ProdDatabase[file].Count > 1)
                {
                    
                    List<string> prod = ProdDatabase[file];
                    SharedFiles += (uint)prod.Count;

                    //generate unique key
                    string concatKey;
                    concatKey = prod[0];
                    for (int i = 1; i < prod.Count; ++i)
                        concatKey += "|" + prod[i];

                    if (FormattedDupDict.ContainsKey(concatKey))
                    {
                        FormattedDupDict[concatKey].Add(file);
                    }
                    else
                    {
                        FormattedDupDict[concatKey] = new List<string>();
                        FormattedDupDict[concatKey].Add(file);
                    }
                }
                else
                {
                    ++UniqueFiles;
                }
            }

            ProdDatabase.Clear();
            Report.AppendLine("\t\t\t DUPLICATE FILES REPORT");
            Report.AppendLine("Total Files: " + TotalFiles);
            Report.AppendLine("Files Processed: " + TotalProcessed);
            Report.AppendLine("Unique Files: " + UniqueFiles);
            Report.AppendLine("Shared Files: " + SharedFiles);
            Report.AppendLine();

            foreach (string s in FormattedDupDict.Keys)
            {
                Report.AppendLine();
                Report.AppendLine("========== Shared Files =========");
                string[] products = s.Split('|');
                
                foreach (string p in products)
                    Report.AppendLine(p);

                foreach (string f in FormattedDupDict[s])
                    Report.AppendLine("\t" + f);
            }







            using (StreamWriter outfile = new StreamWriter(d.FullName + @"\DuplicateFiles.txt"))
            {
                outfile.Write(Report.ToString());
            }

            Console.WriteLine("List Complete");
            Console.ReadLine();
        }


        static List<string> GenerateZipList(string filePath)
        {
           
            List<string> entryList = new List<string>();
            ZipFile testZip = null;
            try
            {
                testZip = new ZipFile(filePath);
            }
            catch
            {
                Report.AppendLine(filePath);
            }

            if (testZip == null)
                return entryList;

            TotalFiles += (uint)testZip.EntryFileNames.Count;

            entryList = (testZip.EntryFileNames.Where(entry => entry.Contains(".")).Select(name => name.ToLower())).ToList<string>();

            for (int i = entryList.Count -1; i >=0; --i)
            {
                for(int j = 0; j < ignoreGroup.Length ; ++j)
                {
                    if (entryList[i].Contains(ignoreGroup[j]))
                    {
                        entryList.Remove(entryList[i]);
                        break;
                    }
                }
            }

            return entryList;
        }
    }
}
