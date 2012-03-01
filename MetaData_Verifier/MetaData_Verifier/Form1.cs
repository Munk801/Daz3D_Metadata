using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;

namespace MetaData_Verifier
{
    public partial class Form1 : Form
    {

        public string[] myFiles;

        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
                    
        }


        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            myFiles = e.Data.GetData(DataFormats.FileDrop) as string[];


            // Build error string to print to txt or message box or whatever
            StringBuilder errorReport = new StringBuilder();

            string diff = "\\Content\\Runtime\\Support\\" + Path.GetFileName(myFiles[0]);
            string manifestPath = Path.GetFullPath(myFiles[0]).Replace(diff, "\\manifest.dsx");
            //  TO DO: ADD A TRY CATCH
            XElement manifest = XElement.Load(manifestPath);

            FileInfo file = new FileInfo(myFiles[0]);
            XElement metadata = LoadMetaData(file);
            XDocument md = XDocument.Load(myFiles[0]);

            // Add the product information to error report
            errorReport.AppendLine("ERROR REPORT FOR: " + metadata.FirstAttribute.Value.ToString());

            // Global IDs must be the same
            errorReport.AppendLine("\n GLOBAL ID VERIFICATION: ");
            if (!IsSameGlobalID(metadata, manifest)) errorReport.AppendLine("Manifest GlobalID not the same with Metadata GlobalID");

            // Load the schema from embedded resources.  This will check with the current metadata file
            XmlSchemaSet schema = new XmlSchemaSet();
            //schema.Add("", System.Xml.XmlReader.Create("ContentMetadata.xsd"));
            schema.Add("", "ContentMetadata.xsd");

            // Validate the document with the schema
            errorReport.AppendLine("\n SCHEMA COMPARISON VERIFICATION: ");
            md.Validate(schema, (o, g) =>
            {
                errorReport.AppendLine(g.Message);
            });

            // Verify Information from the store
            errorReport.AppendLine("\n STORE INFORMATION VERIFICATION: ");
            StoreInfoVerifier(metadata, errorReport);


            // Verify values from assets
            errorReport.AppendLine("\n ASSET VALUES VERIFICATION: ");
            List<string> assetValues = GetAllAssetValuesFromMetaData(metadata, errorReport);

            // Some way of indicating user of the results
            MessageBox.Show(errorReport.ToString());

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
            try
            {
                XElement xmldoc = XElement.Load(data.FullName);


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
            catch (XmlException e)
            {
                MessageBox.Show(" There is an issue with the metadata file: ", e.ToString());
                return null;
            }

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
        /// Checks the Global ID values of the metadata and the manifest xml files.  If they are different, return false
        /// </summary>
        /// <param name="metadata"> XElement should be retrieved from LoadMetaData function</param>
        /// <param name="manifest"> Manifest is loaded from initial start</param>
        /// <returns></returns>
        static bool IsSameGlobalID(XElement metadata, XElement manifest)
        {
            XElement mdGID = metadata.Element("GlobalID");
            XElement manifestGID = manifest.Element("GlobalID");

            if (mdGID.FirstAttribute.Value == manifestGID.FirstAttribute.Value)
                return true;

            else return false;
        }

        /// <summary>
        /// Checks Product, StoreID, GlobalID, ProductToken, and Artists to ensure that they all have values.  If they dont,
        /// it will inform the user that a value is missing.
        /// </summary>
        /// <param name="metadata"></param>
        static void StoreInfoVerifier(XElement metadata, StringBuilder errorReport)
        {
            // All the info to check
            string[] storeInfo = new string[] { "StoreID", "GlobalID", "ProductToken", "Artists" };
            XElement[] itemsToCheck = new XElement[4];

            // Check all elements and see if it is there. If not, add to the error message that its not there
            for (int i = 0; i < itemsToCheck.Length; i++)
            {
                if (metadata.Element(storeInfo[i]) == null)
                {
                    errorReport.AppendLine(storeInfo[i] + " is not found in Metadata.");
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
                            errorReport.AppendLine(item.Name + " does not contain a value");
                        }
                    }
                }

                // ALL OTHER ELEMENTS
                else if (item.FirstAttribute.Value == "")
                {
                    errorReport.AppendLine(item.Name + " does not contain a value");
                }
            }

            // Nothing in error, GREAT!  We are good.  Message box may need to change to something else for usability.
            if (errorReport.Length == 0)
            {
                errorReport.AppendLine("ALL IS WELL WITH STORE INFO");
            }
        }

        /// <summary>
        /// Grabs all the asset values *IE the file location* and stores them in a string list.  The input assumes that you 
        /// have used the LoadMetaData() method to parse through some of the data.  This list can be used to check with the actual directories to make sure
        /// they have all the files.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        static List<string> GetAllAssetValuesFromMetaData(XElement metadata, StringBuilder errorReport)
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
                    CheckValuesofAssets(asset, errorReport);
                    assetValues.Add(asset.FirstAttribute.Value);
                }
            }

            return assetValues;
        }


        /// <summary>
        /// Processes through an asset element *Assuming you have an Xelement that contains an asset* and checks all the nodes in that asset.  Make sure we have 
        /// compatability, tag, audience, content type, categories, and compatability bases.  Categories, Tags, and Compatabilities all have sub elements
        /// </summary>
        /// <param name="asset"></param>
        static void CheckValuesofAssets(XElement asset, StringBuilder errorReport)
        {
            // Check to make sure we have all these for every asset
            if (asset.Elements("ContentType") == null) errorReport.AppendLine(asset.ToString() + " is missing a ContentType");
            if (asset.Elements("Audience") == null) errorReport.AppendLine(asset.ToString() + " is missing an Audience");
            if (asset.Elements("Tags") == null) errorReport.AppendLine(asset.ToString() + " is missing a Tag");
            if (asset.Elements("Categories") == null) errorReport.AppendLine(asset.ToString() + " is missing a Category");
            if (asset.Elements("Compatabilities") == null) errorReport.AppendLine(asset.ToString() + " is missing a Compatability");

            foreach (XElement node in asset.Nodes())
            {
                if (node.Name == "ContentType" || node.Name == "Audience")
                {
                    // Check for value
                    if (node.FirstAttribute.Value == "")
                        errorReport.AppendLine(node.Name + " does not contain a value");
                }

                if (node.Name == "Categories" || node.Name == "Tags" || node.Name == "Compatabilities")
                {
                    // Process through all categories
                    // Maybe just check if it has all elements??
                    if (!node.HasElements)
                    {
                        errorReport.AppendLine(node.ToString() + " contains no elements.  Should have at least one element.");
                        continue;
                    }

                    foreach (XElement value in node.Elements())
                    {
                        if (!value.HasAttributes)
                        {
                            errorReport.AppendLine(value.ToString() + " in " + node.ToString() + " does not contain a value.");
                        }
                    }
                }

            }
        }

    }
}
