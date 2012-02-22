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
            StoreInfoVerifier(args[0]);
        }

        static XElement LoadMetaData(FileInfo data)
        {

            // Element will get overwritten
            XElement metadata = new XElement(data.FullName);
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

        static bool isMetaData(FileInfo data)
        {
            if (!data.Exists || data.Extension != ".dsx")
            {
                MessageBox.Show("Error:  This is not a MetaData File.  MetaData must have a .dsx extension");
                return false;
            }
            return true;
        }

        static void StoreInfoVerifier(XElement metadata)
        {
            // Check Product Value

            // Check StoreID

            // Check GlobalID
 
            // 


            
        }
    }
}
