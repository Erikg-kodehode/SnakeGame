using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SnakeGame
{
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Draws a rounded rectangle with the specified pen and corner radius
        /// </summary>
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (pen == null)
                throw new ArgumentNullException(nameof(pen));

            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Draws a rounded rectangle with the specified pen and corner radius
        /// </summary>
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF bounds, float cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (pen == null)
                throw new ArgumentNullException(nameof(pen));

            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified brush and corner radius
        /// </summary>
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        /// <summary>
        /// Fills a rounded rectangle with the specified brush and corner radius
        /// </summary>
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF bounds, float cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException(nameof(graphics));
            if (brush == null)
                throw new ArgumentNullException(nameof(brush));

            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        /// <summary>
        /// Creates a GraphicsPath for a rounded rectangle with the specified bounds and corner radius
        /// </summary>
        private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int cornerRadius)
        {
            return CreateRoundedRectanglePath(
                new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height),
                cornerRadius);
        }

        /// <summary>
        /// Creates a GraphicsPath for a rounded rectangle with the specified bounds and corner radius
        /// </summary>
        private static GraphicsPath CreateRoundedRectanglePath(RectangleF bounds, float cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = cornerRadius * 2;
            
            // Ensure the corner radius doesn't exceed half the rectangle width or height
            diameter = Math.Min(diameter, bounds.Width);
            diameter = Math.Min(diameter, bounds.Height);
            cornerRadius = diameter / 2;

            // If the corner radius is 0 or less, draw a regular rectangle
            if (cornerRadius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            RectangleF arc = new RectangleF(bounds.X, bounds.Y, diameter, diameter);
            
            // Top left arc
            path.AddArc(arc, 180, 90);
            
            // Top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            
            // Bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            
            // Bottom left arc
            arc.X = bounds.X;
            path.AddArc(arc, 90, 90);
            
            // Close the path
            path.CloseFigure();
            
            return path;
        }
    }
}

