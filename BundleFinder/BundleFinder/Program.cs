using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using System.IO;
using System.Diagnostics;

/*
 * ================================================ Daz Bundle Finder ====================================================
 * Queries through all the products and looks for products with similar information.  Because Bundles will always contain
 * one or more products, the directory can be queried and return a list of products with simliar files in it.  A dictionary is
 * created with the key as the file and the value as the product.  The dictionary can then be reduced to keys with more than one
 * value.  This allows for easy matching of products.
 * */
namespace BundleFinder
{
    class Program
    {
        static uint TotalFiles = 0;
        static uint TotalProcessed = 0;
        static uint UniqueFiles = 0;
        static uint SharedFiles = 0;

        // Products will have these files in common, no need to include them
        static string[] ignoreGroup = 
        {
            "manifest.dsx",
            "content/runtime/webLinks/daz productions, inc",
            "content/readme",
        };

        // Dictionary of product files as keys and products as values
        static Dictionary<string, List<string>> ProdDatabase = new Dictionary<string, List<string>>();
        // This dictionary will be formated to give all the products as the key and the files as the value for output
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
                // Remove delimiter
                string[] products = s.Split('|');
                
                foreach (string p in products)
                    Report.AppendLine(p);

                foreach (string f in FormattedDupDict[s])
                    Report.AppendLine("\t" + f);
            }






            // Write to file
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
            // We may not always be able to access the zips
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

            // Avoid case and directories and add files to lsit
            entryList = (testZip.EntryFileNames.Where(entry => entry.Contains(".")).Select(name => name.ToLower())).ToList<string>();


            // If any of the above files are found, remove from our entry. 
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
