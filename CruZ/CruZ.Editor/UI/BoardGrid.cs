﻿using Microsoft.Xna.Framework.Graphics;

using System.Drawing;

using Color = Microsoft.Xna.Framework.Color;

namespace CruZ.Editor.UI;

using System.Numerics;

using CruZ.GameEngine;
using CruZ.GameEngine.GameSystem;
using CruZ.GameEngine.GameSystem.UI;
using CruZ.GameEngine.Utility;

public class BoardGrid : UIControl
{
    public BoardGrid()
    {
        ShoudDisplay = true;
    }

    protected override void OnDraw(DrawUIEventArgs args)
    {
        if(!ShoudDisplay) return;

        var sp = args.SpriteBatch;
        var vp_Width = GameApplication.GetGraphicsDevice().Viewport.Width;
        var vp_Height = GameApplication.GetGraphicsDevice().Viewport.Height;
        
        DrawAxis(args.SpriteBatch);
        //
        // draw cross-hair
        //
        var center = new PointF(vp_Width / 2f, vp_Height / 2f);
        sp.DrawLine(center.X, center.Y - 10, center.X, center.Y + 10, Color.Black);
        sp.DrawLine(center.X - 10, center.Y, center.X + 10, center.Y, Color.Black);
    }

    public bool ShoudDisplay
    {
        get;
        set;
    }

    private void DrawAxis(SpriteBatch spriteBatch)
    {
        const int MAX_LINE_IN_SCREEN = 25;

        float minBoardScreenSize = Camera.Current.ViewPortWidth / MAX_LINE_IN_SCREEN;
        //
        // x2 if board size is smaller than minBoardScreenSize
        //
        int boardWorldSize = 1;
        while (boardWorldSize * Camera.Current.ScreenToWorldRatio().X < minBoardScreenSize)
            boardWorldSize *= 2;

        var boardColor = boardWorldSize == 1 ?
            EditorConstants.BoardUnitColor :
            EditorConstants.BoardNormalColor;

        DrawBoard(boardWorldSize, boardColor);

        void DrawBoard(int boardSize, Color boardColor)
        {
            var center = Camera.Current.CameraOffset;

            var x_distance = Camera.Current.ViewPortWidth / Camera.Current.ScreenToWorldRatio().X;
            var y_distance = Camera.Current.ViewPortHeight / -Camera.Current.ScreenToWorldRatio().Y;

            var min_x = center.X - x_distance;
            var max_x = center.X + x_distance;

            var min_y = center.Y - y_distance;
            var max_y = center.Y + y_distance;

            for (float x = (int)min_x / boardSize * boardSize; x < max_x; x += boardSize)
            {
                var p1 = Camera.Current.CoordinateToPoint(new Vector2(x, min_y));
                var p2 = Camera.Current.CoordinateToPoint(new Vector2(x, max_y));

                spriteBatch.DrawLine(p1.X, p1.Y, p2.X, p2.Y, boardColor);
            }

            for (float y = (int)min_y / boardSize * boardSize; y < max_y; y += boardSize)
            {
                var p1 = Camera.Current.CoordinateToPoint(new(min_x, y));
                var p2 = Camera.Current.CoordinateToPoint(new(max_x, y));

                spriteBatch.DrawLine(p1.X, p1.Y, p2.X, p2.Y, boardColor);
            }
        }
    }
}
