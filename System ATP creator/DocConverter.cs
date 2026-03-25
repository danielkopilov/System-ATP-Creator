using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System_ATP_creator
{
    public static class DocConverter
    {
        public static bool ConvertDocToDocx(string docPath, string docxPath)
        {
            try
            {
                // Try using Word Interop if available
                Type? wordType = Type.GetTypeFromProgID("Word.Application");
                if (wordType == null)
                    return false;

                dynamic? wordApp = Activator.CreateInstance(wordType);
                if (wordApp == null)
                    return false;

                try
                {
                    wordApp.Visible = false;
                    dynamic doc = wordApp.Documents.Open(docPath);
                    
                    // Save as .docx format (16 = wdFormatXMLDocument)
                    doc.SaveAs2(docxPath, FileFormat: 16);
                    doc.Close();
                    
                    return true;
                }
                finally
                {
                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
