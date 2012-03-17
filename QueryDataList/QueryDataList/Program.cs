using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace QueryDataList
{
    class Program
    {
        static Dictionary<int, List<string>> DataList = new Dictionary<int,List<string>>();
        static Dictionary<string, List<int>> dependencyCollection = new Dictionary<string, List<int>>();

        static void Main(string[] args)
        {
            GenerateMasterList();
            Console.WriteLine("Finding Bundles");
            FindBundles();
            Console.WriteLine("Found Bundles");

            Console.ReadLine();
        }

        private static void FindBundles()
        {
            StringBuilder bundleFormat = new StringBuilder();
            bundleFormat.AppendLine("Product ID,Intaller Files");
            //Generate dependency collection
            foreach (int key in DataList.Keys)
            {
                foreach (string installer in DataList[key])
                {
                    if (dependencyCollection.ContainsKey(installer))
                    {
                        dependencyCollection[installer].Add(key);
                    }
                    else
                    {
                        dependencyCollection[installer] = new List<int>();
                        dependencyCollection[installer].Add(key);
                    }
                }
            }

            DataList.Clear();

            Dictionary<string, List<string>> bundleCollection = new Dictionary<string, List<string>>();

            foreach (string installer in dependencyCollection.Keys)
            {
                List<int> uniqueList = dependencyCollection[installer].Distinct().ToList();
                if ( uniqueList.Count > 1)
                {
                    string dependency ="";
                    dependency = uniqueList[0] + "";

                    for (int i = 1; i < uniqueList.Count; ++i)
                        dependency += "|" + uniqueList[i];

                    if (bundleCollection.ContainsKey(dependency))
                    {
                        bundleCollection[dependency].Add(installer);
                    }
                    else
                    {
                        bundleCollection[dependency] = new List<string>();
                        bundleCollection[dependency].Add(installer);
                    }
                }
            }

            foreach (string dependency in bundleCollection.Keys)
            {
                string[] pidList = dependency.Split('|');
                string[] installerList = bundleCollection[dependency].ToArray();

                string line = "";
                int max = Math.Max(pidList.Length, installerList.Length);

                for (int i = 0; i < max; ++i)
                {
                    if (i > pidList.Length -1)
                        line = "" + ',';
                    else
                        line = pidList[i] + ',';

                    if (i > installerList.Length -1)
                        line += "";
                    else
                        line += installerList[i];

                    bundleFormat.AppendLine(line);
                }

                bundleFormat.AppendLine("" + ',' + "");
            }

            string filepath = @"bundleList.csv";
            File.WriteAllText(filepath, bundleFormat.ToString());
        }

        private static void GenerateMasterList()
            {
                using (StreamReader r = new StreamReader("daz_storephront-3-16-12-target.csv"))
                {
                    r.ReadLine();
                    while (!r.EndOfStream)
                    {
                        string entireLine = r.ReadLine();
                        string[] lines = entireLine.Split(',');
                        lines[3] = lines[3].Replace("\"", "");
                        if (lines[3] == "")
                            continue;

                        if (lines[2].Contains("Poses"))
                        {
                            int key = Int32.Parse(lines[4]);

                            if (key == 0)
                                continue;

                            if (DataList.ContainsKey(key))
                                DataList[key].Add(lines[5]);
                            else
                            {
                                DataList[key] = new List<string>();
                                DataList[key].Add(lines[5]);
                            }
                            Console.WriteLine(lines[4] + " | " + lines[5]);
                        }
                        else
                        {
                            int key = Int32.Parse(lines[2]);

                            if (key == 0)
                                continue;

                            if (DataList.ContainsKey(key))
                                DataList[key].Add(lines[3]);
                            else
                            {
                                DataList[key] = new List<string>();
                                DataList[key].Add(lines[3]);
                            }
                            Console.WriteLine(lines[2] + " | " + lines[3]);
                        }
                    }
                }
                    List<int> SortedKeys = DataList.Keys.ToList<int>();
                    SortedKeys.Sort();
                    StringBuilder output = new StringBuilder();
                    char delim = ',';
                    string filepath = @"EntireProductFiles.csv";

                    foreach (int key in SortedKeys)
                    {
                        output.AppendLine(key + "," + DataList[key][0]);
                        for (int i = 1; i < DataList[key].Count; i++)
                        {
                            output.AppendLine("" + delim + DataList[key][i]);
                        }
                    }
                    File.WriteAllText(filepath, output.ToString());
                    Console.WriteLine("DONE WITH MASTER");
        }
    }
}
