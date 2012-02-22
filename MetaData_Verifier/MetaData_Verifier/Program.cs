using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Linq;
using System.Xml.Linq;

namespace MetaData_Verifier
{
    class Program
    {
        static void Main(string[] args)
        {
            FileInfo file = new FileInfo(args[0]);
            XElement metadata = LoadMetaData(file);
            List<string> assetValues = GetAllAssetValuesFromMetaData(metadata);
            StoreInfoVerifier(metadata);

        }


        /// <summary>
        /// LoadMetaData will load the xml from file and parse through some unnecessary items to reach the elements.
        /// The return value will have already checked the product name and the root node will be down at Product.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static XElement LoadMetaData(FileInfo data)
        {

            // Element will get overwritten
            XElement xmldoc = XElement.Load(data.FullName) ;
            if (!isMetaData(data))
            {
                FileInfo newData = new FileInfo(Console.ReadLine());
                LoadMetaData(newData);
            }
            else
            {
                xmldoc = XElement.Load(data.FullName);
            }
            XElement products = xmldoc.Element("Products");
            

            if (products.Element("Product") == null)
            {
                MessageBox.Show("No Product Found");
            }
            XElement metadata = products.Element("Product");
            return metadata;

        }

        /// <summary>
        /// Returns true if the file exists and is a metadata file.  Only checks file extension and not actual data.
        /// It is up to the verifier to check certain pieces of info inside for consistency.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static bool isMetaData(FileInfo data)
        {
            if (!data.Exists || data.Extension != ".dsx")
            {
                MessageBox.Show("Error:  This is not a MetaData File.  MetaData must have a .dsx extension");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks Product, StoreID, GlobalID, ProductToken, and Artists to ensure that they all have values.  If they dont,
        /// it will inform the user that a value is missing.
        /// </summary>
        /// <param name="metadata"></param>
        static void StoreInfoVerifier(XElement metadata)
        {
            // Prints out at the end with results
            StringBuilder errorString = new StringBuilder();

            // All the info to check
            string[] storeInfo = new string[] {"StoreID", "GlobalID", "ProductToken", "Artists" };
            XElement[] itemsToCheck = new XElement[4];

            // Check all elements and see if it is there. If not, add to the error message that its not there
            for (int i = 0; i < itemsToCheck.Length; i++)
            {
                if (metadata.Element(storeInfo[i]) == null)
                {
                    errorString.AppendLine(storeInfo[i] + " is not found in Metadata.");
                    continue;
                }
                else
                {
                    itemsToCheck[i] = metadata.Element(storeInfo[i]);
                }
            }

            // Now that we know the elements are there, check to see if they have values.
            foreach (XElement item in itemsToCheck)
            {

                if (item == null) continue;
                // ARTISTS ARE THE ONLY THING THAT CAN HAVE MORE THAN ONE ELEMENT
                if (item.Name == "Artists")
                {
                    foreach (XElement artist in item.Elements())
                    {
                        if (artist.FirstAttribute.Value == null)
                        {
                            errorString.AppendLine(item.Name + " does not contain a value");
                        }
                    }
                }

                // ALL OTHER ELEMENTS
                else if (item.FirstAttribute.Value == "")
                {
                    errorString.AppendLine(item.Name + " does not contain a value");
                }
            }
            
            // Nothing in error, GREAT!  We are good.  Message box may need to change to something else for usability.
            if (errorString.Length == 0)
            {
                MessageBox.Show("ALL IS WELL WITH STORE INFO");
            }
            else
            {
                MessageBox.Show(errorString.ToString());
            }
        }

        /// <summary>
        /// Grabs all the asset values *IE the file location* and stores them in a string list.  The input assumes that you 
        /// have used the LoadMetaData() method to parse through some of the data.  This list can be used to check with the actual directories to make sure
        /// they have all the files.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        static List<string> GetAllAssetValuesFromMetaData(XElement metadata)
        {
            XElement assets = metadata.Element("Assets");
            List<XElement> allAssets = assets.Elements("Asset").ToList<XElement>();
            List<string> assetValues = new List<string>();
            foreach (XElement asset in allAssets)
            {
                if (asset.FirstAttribute.Value == "")
                {
                    Console.WriteLine("No value assigned to asset");
                }

                else
                {
                    assetValues.Add(asset.FirstAttribute.Value);
                }
            }

            return assetValues;
        }
    }
}
