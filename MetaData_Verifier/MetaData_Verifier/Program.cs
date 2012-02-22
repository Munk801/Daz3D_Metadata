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
            XElement test = XElement.Load(file.FullName);
            XElement products = test.Element("Product");
            StoreInfoVerifier(test);

        }

        static XElement LoadMetaData(FileInfo data)
        {

            // Element will get overwritten
            XElement metadata = XElement.Load(data.FullName) ;
            if (!isMetaData(data))
            {
                FileInfo newData = new FileInfo(Console.ReadLine());
                LoadMetaData(newData);
            }
            else
            {
                metadata = XElement.Load(data.FullName);
            }

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
            StringBuilder errorString = new StringBuilder();
            string[] storeInfo = new string[] { "Product", "StoreID", "GlobalID", "ProductToken", "Artists" };
            XElement products = metadata.Element("Products");
            XElement[] itemsToCheck = new XElement[5];
            if (products.Element("Product") == null)
            {
                errorString.AppendLine("No Product Found");
            }
            itemsToCheck[0] = products.Element("Product");
            for (int i = 1; i < itemsToCheck.Length; i++)
            {
                if (itemsToCheck[0].Element(storeInfo[i]) == null)
                {
                    errorString.AppendLine(storeInfo[i] + " is not found in Metadata.");
                    continue;
                }
                else
                {
                    itemsToCheck[i] = itemsToCheck[0].Element(storeInfo[i]);
                }
            }

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
            
            if (errorString.Length == 0)
            {
                MessageBox.Show("ALL IS WELL WITH STORE INFO");
            }
            else
            {
                MessageBox.Show(errorString.ToString());
            }
        }
    }
}
