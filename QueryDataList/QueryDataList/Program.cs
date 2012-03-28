using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using ProductInfo = System.Tuple<int, string>;
using Product = System.Tuple;
namespace QueryDataList
{
    class Program
    {
        
        static Dictionary<int, List<string>> DataList = new Dictionary<int,List<string>>();
        static Dictionary<string, List<int>> dependencyCollection = new Dictionary<string, List<int>>();
        
        /// <summary>
        /// Dictionary containing all our Bundles 
        /// </summary>
        static Dictionary<ProductInfo, List<ProductInfo>> FinalBundleList = new Dictionary<ProductInfo, List<ProductInfo>>();

        // List of all the products that share IDX.
        static List<ProductInfo> SharedIDXProducts = new List<ProductInfo>();

        static void Main(string[] args)
        {
            //GenerateMasterList();
            //Console.WriteLine("Finding Bundles");
            //FindBundles();
            //Console.WriteLine("Found Bundles");
            string path = "test.csv";

            GenerateMasterBundleList(path);
            Console.ReadLine();
        }

        /// <summary>
        /// Given a CSV File containing items in the order [ROLE],[IDX],[PRODUCTNAME]
        /// This method will take in the csv file and process through in order to generate a final dictionary value
        /// </summary>
        /// <param name="path">Path to CSV</param>
        private static void GenerateMasterBundleList(string path)
        {
            // List of the csvData.  Gets cleared after finding a ",,"
            List<string[]> csvData = new List<string[]>();
            try
            {
                using (StreamReader readFile = new StreamReader(path))
                {
                    string line;
                    string[] row;
                    // Go until we reach end of line
                    while ((line = readFile.ReadLine()) != null)
                    {
                        if (line != ",,")
                        {
                            // Add these items to our csvdata
                            row = line.Split(',');
                            // TO DO: ENSURE THAT EVERY LINE CONTAINS 3 ELEMENTS

                            csvData.Add(row);
                        }
                        else
                        {
                            Console.WriteLine("End of File Dependency");
                            // Check if all products are labeled Bundle


                            // This is our dictionary of all the items that share files
                            Dictionary<ProductInfo, List<ProductInfo>> miniProduct = new Dictionary<ProductInfo, List<ProductInfo>>();

                            foreach (string[] product in csvData)
                            {
                                string role = product[0];
                                if (role == "Bundle")
                                {
                                    // We found a bundle, so go through every other product in the lsit and find dependencies
                                    ProductInfo itsABundle = Product.Create(Int32.Parse(product[1]), product[2]);
                                    miniProduct.Add(itsABundle, new List<ProductInfo>());
                                    //Search through list for all dependencies
                                    foreach (string[] otherProd in csvData)
                                    {
                                        if (product == otherProd) continue;
                                        else if (otherProd[0] == "Unique")
                                        {
                                            ProductInfo partOfBundle = Product.Create(Int32.Parse(otherProd[1]), otherProd[2]);
                                            miniProduct[itsABundle].Add(partOfBundle);
                                        }
                                        else if (otherProd[0] == "Compound")
                                        {
                                            // COMPOUNDS ARE A PART OF A BUNDLE
                                        }

                                    }
                                }
                                // TO DO: COMPOUND ITEMS
                                else if (role == "Compound")
                                {
                                    //Search through list of all dependencies
                                }

                                // Put all the sharedIDX items in a list
                                else if (role == "Shared IDX")
                                {
                                    ProductInfo sharedIDX = Product.Create(Int32.Parse(product[1]), product[2]);
                                    if (!SharedIDXProducts.Contains(sharedIDX))
                                    {
                                        SharedIDXProducts.Add(sharedIDX);
                                    }
                                }
                                else if (role == "Unique")
                                {
                                    // DONT REALLY  NEED TO DO ANYTHING BUT THESE ARE ALL THE CASES
                                }
                                else
                                {
                                    // CAN CAPTURE MISSPELLINGS AND SUCH
                                }
                            }

                            // We have finished sorting through everything with a file dependency, add them back to our MASTER dictionary
                            foreach (var key in miniProduct.Keys)
                            {
                                if (FinalBundleList.Keys.Contains(key))
                                {
                                    FinalBundleList[key].Union(miniProduct[key]);
                                }
                                else
                                {
                                    FinalBundleList.Add(key, miniProduct[key]);
                                }
                            }
                            // May not need to clear the dictionary but why not be safe
                            miniProduct.Clear();
                            csvData.Clear();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

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
