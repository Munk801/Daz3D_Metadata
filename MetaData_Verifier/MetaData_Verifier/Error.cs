using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MetaData_Verifier
{
    public static class Error
    {
        private static StringBuilder log = new StringBuilder();

        public static void Report(string error)
        {
            log.AppendLine(error);
        }

        public static void Print()
        {
            MessageBox.Show(log.ToString());
            log.Clear();
        }

        public static void Print(RichTextBox errorbox)
        {
            errorbox.Text = log.ToString();
            log.Clear();
        }
    }
}
