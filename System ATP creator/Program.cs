using System;
using System.Windows.Forms;
using System.IO;

namespace System_ATP_creator
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Create logo file if it doesn't exist
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (!File.Exists(logoPath))
            {
                try
                {
                    LogoCreator.CreateCILogo(logoPath);
                }
                catch
                {
                    // If logo creation fails, the app will use the text-based logo
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
