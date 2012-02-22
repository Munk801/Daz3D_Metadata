#define TEMP
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Security.AccessControl;

namespace Metadata_Comparer
{
    class Program
    {
        #region Fields
        /// <summary>
        /// Represents the directory to write the output file to.
        /// </summary>
        private static DirectoryInfo outputDirectory;

        /// <summary>
        /// A collection of information about the input files for comparison.
        /// </summary>
        private static FileInfo[] inputFiles;

        /// <summary>
        /// A collection of products for each inputfile
        /// </summary>
        private static List<XElement>[] metaInputFileProducts;
        #endregion

        /// <summary>
        /// Compares two or more .dsx metadata files for irregularities.
        /// </summary>
        /// <param name="args">inputFile_1 , inputFile_2, ... outputDirectory.</param>
        static void Main(string[] args)
        {
            //Check: We need to have at least 3 arguments passed in (two files and an output directory).
            if (args.Length < 3)
                Error.Throw(ErrorCode.Invalid_Parameters, "INVALID ARGUMENT COUNT- You need to specify at least two metadata files to compare and an output directory to write to.");

            //Validate and initialize the collections of products in the metadata file we need to compare.
            InitializeAndCheckInput(args);

            Console.ReadLine();
        }

        /// <summary>
        /// Validates the input arguments.
        /// </summary>
        /// <param name="args">The files and directory to check</param>
        public static void InitializeAndCheckInput(string[] args)
        {
            #region Directory Checks
            //Check: We need a valid output directory that we can write to.
            outputDirectory = new DirectoryInfo(args[args.Length - 1]);
            if (!outputDirectory.Exists)
                Error.Throw(ErrorCode.Invalid_Parameters, "INVALID OUTPUT DIRECTORY- " + outputDirectory.Name + " does not exist.");

            //Check: We need to see if we can write to this directory.
            //TRY TO CREATE THE OUTPUT FILE IN THIS DIRECTORY
            #endregion

            #region Input File Checks
            //Check: We need valid input files we can read
            inputFiles = new FileInfo[args.Length - 1];
            for (int i = 0; i < (args.Length - 1); ++i)
            {
                inputFiles[i] = new FileInfo(args[i]);

                if (!inputFiles[i].Exists || inputFiles[i].Extension != ".dsx")
                    Error.Throw(ErrorCode.Invalid_Parameters, "INVALID INPUT FILE- " + inputFiles[i].Name + " doesn't exist or isn't a daz metadata file <.dsx>.");
            }

            //Check: We need to compare daz3d meta data files
            XElement[] metaDataFiles = new XElement[inputFiles.Length];
            for (int i = 0; i < inputFiles.Length; ++i)
            {
                metaDataFiles[i] = XElement.Load(inputFiles[i].FullName);
                XElement products = metaDataFiles[i].Element("Products");

                if (metaDataFiles[i].Name != "ContentDBInstall" || products == null)
                    Error.Throw(ErrorCode.Invalid_Parameters, "INVALID INPUT FILE- " + inputFiles[i].Name + " isn't a daz3d metadata file.");

                metaDataFiles[i] = products;
            }

            //Check: we need to validate the metadata file for correct structure.
            metaInputFileProducts = new List<XElement>[inputFiles.Length];
            for (int i = 0; i < metaInputFileProducts.Length; ++i)
            {
                List<XElement> productList = metaDataFiles[i].Elements().ToList<XElement>();

                //Needs at least one product
                if (productList.Count < 1)
                    Error.Throw(ErrorCode.Invalid_Parameters, "INVALID INPUT FILE- " + inputFiles[i].Name + " needs to have at least one product to compare.");

                //Check for valid products
                /* NOTE- We should check for valid meta data file structure here for the whole metadata file.
                 * This would be a very similar to the validator except we don't compare the entries against the product files */
#if TEMP
                foreach (XElement product in productList)
                {
                    if (product.Name != "Product" || product.Attribute("VALUE") == null)
                        Error.Throw(ErrorCode.Invalid_Parameters, "INVALID INPUT FILE- " + inputFiles[i].Name + " has inconsistent metadata file structure.");
                }
#endif
                //Save product list
                metaInputFileProducts[i] = productList;
            }

            //Check: We should only compare metadata for the same products.
            /* NOTE- Can two products have sub products in a different order and still be valid...? */
            for (int i = 1; i < metaInputFileProducts.Length; ++i)
            {
                var results = metaInputFileProducts[0].Zip<XElement, XElement, bool>(metaInputFileProducts[i],
                    (a, b) => a.Attribute("VALUE").Value == b.Attribute("VALUE").Value);

                foreach (bool result in results)
                    if (result == false)
                        Error.Throw(ErrorCode.Invalid_Parameters, "INVALID FILE COMPARISION- " + inputFiles[0].Name + " and " + inputFiles[i].Name + " are for different products or their sub products are in a different order.");
            }
            #endregion
        }

        /// <summary>
        /// Cleans up any objects that need to be manually released
        /// </summary>
        public static void Dispose()
        {

        }
    }
}
