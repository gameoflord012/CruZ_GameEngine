﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CruZ.Common.Utility
{
    public static class SpriteBatchExtensions
    {
        public static void DrawRectangle(this SpriteBatch spriteBatch, DRAW.RectangleF rectangle, Color color, float thickness = 1f, float layerDepth = 0)
        {
            MonoGame.Extended.ShapeExtensions.DrawRectangle(
                spriteBatch, 
                new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height), 
                color, 
                thickness, 
                layerDepth);
        }
    }
}