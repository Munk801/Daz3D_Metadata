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
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;
using System.Reflection;

namespace MetaData_Verifier
{
    public partial class Form1 : Form
    {
        public List<FileInfo> myFiles;
        public string[] paths;
        public XDocument mdFile;
        public DirectoryInfo parentDir;
        public List<string> fileNames;
        public string mdPath;
        
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
            
            // Contains all the files of metadata to process
            myFiles = new List<FileInfo>();

            // Grab all the files into a string array
            paths = e.Data.GetData(DataFormats.FileDrop) as string[];

            // We must check to make sure they are metadata files and that they exist
            foreach (string path in paths)
            {
                try
                {
                    FileInfo f = new FileInfo(Path.GetFullPath(path));
                    if (f.Extension != ".dsx")
                    {
                        MessageBox.Show(f.Name + " is not a Metadata file.  It must be a .dsx file.");
                        return;
                    }
                    else
                    {
                        myFiles.Add(f);
                    }
                }
                catch (FileNotFoundException f)
                {
                    MessageBox.Show(f.ToString() + " does not exist.  Error Code: " + f.Message);
                }
            }

            foreach (FileInfo filePath in myFiles)
            {
                // Build error string to print to txt or message box or whatever
                XElement metadata = LoadMetaData(filePath);
                if (metadata != null)
                {
                    // Add the product information to error report
                    Error.Report("ERROR REPORT FOR: " + metadata.FirstAttribute.Value.ToString());
                    mdPath = filePath.FullName;

                    // Get parent dir path.  We will need it later
                    // TO DO: CHECK ALL SUB PATHS FOR SUPPORT ASSETS****
                    DirectoryInfo getPD = new DirectoryInfo(Path.GetDirectoryName(mdPath));
                    parentDir = new DirectoryInfo(GetParentDir(getPD));
                    fileNames = GenerateFileList();


                    DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(mdPath));

                    CheckManifestAndMetaData(dir, metadata);

                    // Load the schema from embedded resources.  This will check with the current metadata file
                    XmlSchemaSet schema = new XmlSchemaSet();
                    schema.Add("", "ContentMetadata.xsd");

                    // Validate the document with the schema
                    mdFile = XDocument.Load(filePath.FullName);
                    Error.Report("\n SCHEMA COMPARISON VERIFICATION: ");
                    mdFile.Validate(schema, (o, g) =>
                    {
                        Error.Report(g.Message);
                    });


                    List<string> assetValues = GetAllAssetValuesFromMetaData(metadata, mdFile);

                    CheckSupportAssets(metadata);

                    CheckAssetsInDirectory(assetValues);

                    // Verify Information from the store
                    Error.Report("\n STORE INFORMATION VERIFICATION: ");
                    StoreInfoVerifier(metadata, mdFile, filePath);


                    // Verify values from assets
                    Error.Report("\n ASSET VALUES VERIFICATION: ");
                    // TO DO: VERIFY ALL THE ASSETS IN METADATA ARE IN FILE


                    Error.Print(errorReport);
                }
            }
        }

        private List<string> GenerateFileList()
        {
            //Stack for directory hierarchy
            Stack<DirectoryInfo> directoryStack = new Stack<DirectoryInfo>();
            List<FileInfo> fileList = new List<FileInfo>();
            List<string> outPutList = new List<string>();

            //Prime the stack
            fileList.Add(parentDir.GetFiles());
            directoryStack.Push(parentDir.GetDirectories());

            //grab files
            while (directoryStack.Count != 0)
            {
                DirectoryInfo dir = directoryStack.Pop();
                if(dir.Name == "Support" || dir.Name == "Uninstallers") continue;
                fileList.Add(dir.GetFiles());
                directoryStack.Push(dir.GetDirectories());
            }

            //generate output list
            foreach (FileInfo file in fileList)
            {
                string s = file.FullName;
                int parentCount = parentDir.FullName.Length;
                s = s.Remove(0, parentCount);
                s = s.ToLower().Replace("\\", "/");
                outPutList.Add(s);
            }

            return outPutList;
        }

        private void CheckAssetsInDirectory(List<string> assets)
        {
            Error.Report("\nCHECKING ALL ASSETS IN METADATA WITH DIRECTORY:");
            foreach (string curAsset in assets)
            {
                if(!fileNames.Contains(curAsset.ToLower()))
                    Error.Report(curAsset + " is not found in your directory.");
            }
        }

        private void CheckSupportAssets(XElement metadata)
        {
            Error.Report("\nCHECKING SUPPORT ASSETS:");
            XElement supAssets = metadata.Element("SupportAssets");
            List<XElement> allSupAssets = supAssets.Elements("SupportAsset").ToList<XElement>();
            List<string> supportAssets = new List<string>(fileNames);

            // Checks all the support assets in the metadata against the directories
            foreach (XElement sa in allSupAssets)
            {
                string s = sa.FirstAttribute.Value.ToLower();
                if (!supportAssets.Contains(s))
                    Error.Report(sa.FirstAttribute.Value + " is not found in the directory.");
                else supportAssets.Remove(s);

            }

            if(fileNames.Count != 0)
                foreach (string name in supportAssets)
                {
                    XElement thisAsset = new XElement("SupportAsset", new XAttribute("VALUE", name));
                    //metadata.Save(mdPath, SaveOptions.OmitDuplicateNamespaces);
                    Error.Report(name + " is missing from the metadata.");
                }
        }

        static void CheckManifestAndMetaData(DirectoryInfo dir, XElement metadata)
        {
            DirectoryInfo root = dir.Root;
            FileInfo[] manifest = dir.GetFiles("manifest.dsx");
            // Continually go up til we find the manifest.  The first manifest is our guy!
            while (dir != root)
            {
                manifest = dir.GetFiles("manifest.dsx");
                if (manifest.Length == 1)
                {
                    break;
                }
                dir = dir.Parent;
            }
            if (manifest.Length != 1)
            {
                MessageBox.Show("Manifest could not be found");
                return;
            }

            // Files in which we are checking the GID for
            XElement manifestFile = XElement.Load(manifest[0].FullName);
            // These files will write out the GID.  We may not need both but this works.
            XDocument manTest = XDocument.Load(manifest[0].FullName);
            XElement gidToWrite = manTest.Root.Element("GlobalID");

            // Global IDs must be the same
            Error.Report("\n GLOBAL ID VERIFICATION: ");
            //if (!IsSameGlobalID(metadata, manifestFile)) errorReport.AppendLine("Manifest GlobalID not the same with Metadata GlobalID");

            // Write this back to the XML.  XML Header gets added.  Issue?
            string gid = ChangeGlobalID(metadata, ref manifestFile);
            if (gidToWrite.FirstAttribute.Value != gid)
            {
                gidToWrite.FirstAttribute.Value = gid;
                manTest.Save(manifest[0].FullName, SaveOptions.OmitDuplicateNamespaces);
                Error.Report("Different Global IDs.  Resaved metadata global ID");
            }
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

        static string ChangeGlobalID(XElement metadata, ref XElement manifest)
        {

            // TO DO: CHANGE MANIFEST GID TO METADATA GID
            XElement mdGID = metadata.Element("GlobalID");
            XElement manifestGID = manifest.Element("GlobalID");

            //manifestGID.FirstAttribute.Value = mdGID.FirstAttribute.Value;
            manifest.SetAttributeValue("GlobalID", mdGID.FirstAttribute.Value);
            return mdGID.FirstAttribute.Value;
        }

        /// <summary>
        /// Because we are under the assumption that Content is going to be the Base, this will grab that directory
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        static string GetParentDir(DirectoryInfo dir)
        {
            DirectoryInfo d = dir;
            while (d != d.Root)
            {
                if (d.Name == "Content") return d.FullName;

                d = d.Parent;
            }
            return null;
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
                XElement metadata;

                if (!isMetaData(data))
                {
                    MessageBox.Show("Not a metadata file.  Only works on .dsx metadata files.");
                    return null;
                }
                else
                {
                    xmldoc = XElement.Load(data.FullName);

                    if (xmldoc.Name != "ContentDBInstall")
                    {
                        MessageBox.Show("File is a .DSX file but is not a compatible metadata file.  Please use a compatible metadata file.");
                        return null;
                    }

                    else
                    {
                        XElement products = xmldoc.Element("Products");
                        metadata = products.Element("Product");
                    }
                    return metadata;
                }
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
        /// Checks Product, StoreID, GlobalID, ProductToken, and Artists to ensure that they all have values.  If they dont,
        /// it will inform the user that a value is missing.
        /// </summary>
        /// <param name="metadata"></param>
        static void StoreInfoVerifier(XElement metadata, XDocument md, FileInfo filePath)
        {
            // All the info to check
            string[] storeInfo = new string[] { "StoreID", "GlobalID", "ProductToken", "Artists" };
            XElement[] itemsToCheck = new XElement[4];

            // Check all elements and see if it is there. If not, add to the error message that its not there
            for (int i = 0; i < itemsToCheck.Length; i++)
            {
                if (metadata.Element(storeInfo[i]) == null)
                {
                    Error.Report(storeInfo[i] + " is not found in Metadata.");
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
                        
                        if (artist.FirstAttribute.Value == "")
                        {
                            Error.Report(item.Name + " has an empty Artist.  Removing from metadata...");
                            artist.Remove();
                            // Not properly removing artist anymore
                            md.Save(filePath.FullName);
                        }
                    }
                    if (!item.HasElements)
                        Error.Report(item.Name + " does not contain a value");
                }

                // ALL OTHER ELEMENTS
                else if (item.FirstAttribute.Value == "")
                {
                    Error.Report(item.Name + " does not contain a value");
                }
            }
        }

        /// <summary>
        /// Grabs all the asset values *IE the file location* and stores them in a string list.  The input assumes that you 
        /// have used the LoadMetaData() method to parse through some of the data.  This list can be used to check with the actual directories to make sure
        /// they have all the files.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        static List<string> GetAllAssetValuesFromMetaData(XElement metadata, XDocument md)
        {
            XElement assets = metadata.Element("Assets");
            List<XElement> allAssets = assets.Elements("Asset").ToList<XElement>();
            List<string> assetValues = new List<string>();
            foreach (XElement asset in allAssets)
            {
                if (asset.FirstAttribute.Value == "")
                {
                    Error.Report("No value assigned to asset.");
                }

                else
                {
                    CheckValuesofAssets(asset, md);
                    assetValues.Add("/" + asset.FirstAttribute.Value);
                }
            }

            return assetValues;
        }


        /// <summary>
        /// Processes through an asset element *Assuming you have an Xelement that contains an asset* and checks all the nodes in that asset.  Make sure we have 
        /// compatability, tag, audience, content type, categories, and compatability bases.  Categories, Tags, and Compatabilities all have sub elements
        /// </summary>
        /// <param name="asset"></param>
        static void CheckValuesofAssets(XElement asset, XDocument md)
        {
            
            // FIX: WHEN REMOVING THE ENTIRE ELEMENT, DOESNT REPORT ERROR
            // Check to make sure we have all these for every asset
            var results = asset.Descendants().ToList<XElement>();
            List<string> descNames = new List<string>();
            foreach (XElement desc in results)
            {
                descNames.Add(desc.Name.ToString());
            }

            if (!descNames.Contains("ContentType")) Error.Report(asset.FirstAttribute.Value.ToString() + " is missing a ContentType");
            else if (!descNames.Contains("Audience")) Error.Report(asset.FirstAttribute.Value.ToString() + " is missing an Audience");
            else if (!descNames.Contains("Categories")) Error.Report(asset.FirstAttribute.Value + "is missing Categories");
            else if (!descNames.Contains("Tags")) Error.Report(asset.FirstAttribute.Value.ToString() + " is missing Tags");


            foreach (XElement node in asset.Nodes())
            {
                if (node.Name == "ContentType" || node.Name == "Audience")
                {
                    // Check for value
                    if (node.FirstAttribute.Value == "")
                        Error.Report(node.Name + " does not contain a value");
                }

                if (node.Name == "Categories" || node.Name == "Tags" || node.Name == "Compatabilities")
                {
                    // Process through all categories
                    // Maybe just check if it has all elements??
                    if (!node.HasElements)
                    {
                        Error.Report(node.ToString() + " contains no elements.  Should have at least one element.");
                        continue;
                    }

                    foreach (XElement value in node.Elements())
                    {
                        if (!value.HasAttributes)
                        {
                            Error.Report(value.ToString() + " in " + node.ToString() + " does not contain a value.");
                        }
                    }
                }

            }
        }

        private void errorReport_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
