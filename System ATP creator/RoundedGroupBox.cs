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

    /// <summary>
    /// A CheckBox that always renders its text using ForeColor, even when disabled.
    /// This prevents Windows from overriding the text color with its blue/grey system disabled color.
    /// The checkbox square itself is still greyed out when Enabled = false (unclickable).
    /// </summary>
    public class GreyableCheckBox : CheckBox
    {
        public GreyableCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);

            // Draw the standard checkbox square using the system renderer (respects Enabled state)
            int boxSize = 13;
            int boxY = (Height - boxSize) / 2;
            Rectangle boxRect = new Rectangle(0, boxY, boxSize, boxSize);

            System.Windows.Forms.VisualStyles.CheckBoxState state;
            if (!Enabled)
                state = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled;
            else if (Checked)
                state = System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal;
            else
                state = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;

            System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(boxRect.X, boxRect.Y), state);

            // Draw text manually using our ForeColor (ignores Enabled state)
            int textX = boxSize + 4;
            Rectangle textRect = new Rectangle(textX, 0, Width - textX, Height);
            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(Text, Font, brush, textRect, sf);
            }
        }
    }
}
