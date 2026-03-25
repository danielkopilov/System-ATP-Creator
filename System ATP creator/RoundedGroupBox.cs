using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace System_ATP_creator
{
    public class RoundedGroupBox : GroupBox
    {
        private int borderRadius = 12;
        private Color borderColor = Color.FromArgb(128, 0, 0, 0); // 50% black

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius
        {
            get => borderRadius;
            set
            {
                borderRadius = value;
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public RoundedGroupBox()
        {
            SetStyle(ControlStyles.UserPaint | 
                     ControlStyles.ResizeRedraw | 
                     ControlStyles.SupportsTransparentBackColor | 
                     ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.FromArgb(250, 251, 253);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent.BackColor);

            // Measure the text
            SizeF textSize = g.MeasureString(Text, Font);
            int textHeight = (int)textSize.Height;

            // Create rounded rectangle path for the border
            Rectangle borderRect = new Rectangle(0, textHeight / 2, Width - 1, Height - textHeight / 2 - 1);
            using (GraphicsPath path = GetRoundedRectPath(borderRect, borderRadius))
            {
                // Fill the background
                using (SolidBrush bgBrush = new SolidBrush(BackColor))
                {
                    g.FillPath(bgBrush, path);
                }

                // Draw the border
                using (Pen borderPen = new Pen(borderColor, 1))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Draw the text with background
            if (!string.IsNullOrEmpty(Text))
            {
                int textX = 15;
                int textY = 0;

                // Create a small rectangle for text background
                Rectangle textBackRect = new Rectangle(textX - 5, textY, (int)textSize.Width + 10, textHeight);
                
                // Fill text background
                using (SolidBrush textBackBrush = new SolidBrush(Parent.BackColor))
                {
                    g.FillRectangle(textBackBrush, textBackRect);
                }

                // Draw the text
                using (SolidBrush textBrush = new SolidBrush(ForeColor))
                {
                    g.DrawString(Text, Font, textBrush, textX, textY);
                }
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            // Top-left corner
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            
            // Top-right corner
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            
            // Bottom-right corner
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            
            // Bottom-left corner
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            
            path.CloseFigure();
            return path;
        }
    }
}
