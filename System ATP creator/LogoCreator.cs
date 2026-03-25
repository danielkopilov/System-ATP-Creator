using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace System_ATP_creator
{
    public static class LogoCreator
    {
        public static void CreateCILogo(string outputPath)
        {
            int width = 500;
            int height = 120;

            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Define CI red color
                Color eciRed = Color.FromArgb(227, 6, 19);
                
                int startX = 20;
                int startY = 30;

                // Draw three horizontal lines (stripes)
                using (Brush redBrush = new SolidBrush(eciRed))
                {
                    int stripeWidth = 45;
                    int stripeHeight = 10;
                    int stripeSpacing = 15;

                    g.FillRectangle(redBrush, startX, startY, stripeWidth, stripeHeight);
                    g.FillRectangle(redBrush, startX, startY + stripeSpacing, stripeWidth, stripeHeight);
                    g.FillRectangle(redBrush, startX, startY + (stripeSpacing * 2), stripeWidth, stripeHeight);
                }

                // Draw arrow/chevron shape
                using (Pen redPen = new Pen(eciRed, 8))
                {
                    redPen.StartCap = LineCap.Round;
                    redPen.EndCap = LineCap.Round;
                    
                    int arrowX = startX + 55;
                    int arrowY = startY + 20;
                    
                    // Draw chevron/arrow pointing right
                    PointF[] arrowPoints = new PointF[]
                    {
                        new PointF(arrowX, arrowY - 10),
                        new PointF(arrowX + 20, arrowY),
                        new PointF(arrowX, arrowY + 10)
                    };
                    
                    g.DrawLines(redPen, arrowPoints);
                }

                // Draw "CI SYSTEMS" text
                using (Font font = new Font("Arial", 36, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(eciRed))
                {
                    string text = "CI SYSTEMS";
                    int textX = startX + 100;
                    int textY = startY + 5;
                    
                    g.DrawString(text, font, textBrush, textX, textY);
                }

                // Save the image
                bitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
