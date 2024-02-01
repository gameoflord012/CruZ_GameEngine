﻿using CruZ.Components;
using CruZ.Systems;
using CruZ.UI;
using MonoGame.Extended;
using System.Diagnostics;

namespace CruZ.Editor.UI
{
    public class EntityControl : UIControl
    {
        public EntityControl(TransformEntity e)
        {
            _e = e;
            _sp = e.GetComponent<SpriteComponent>();

            if (_e.HasComponent(typeof(TileComponent))) return;

            _sp.DrawBegin += Sprite_DrawBegin;
            _sp.DrawEnd += Sprite_DrawEnd;
        }

        private void Sprite_DrawEnd(object? sender, DrawEndEventArgs e)
        {
            //var coord = _args.Position;
            //var rect = _args.SourceRectangle;
            //var point = Camera.Main.CoordinateToPoint(coord);

            //Width       = (int)(rect.Width * Camera.Main.WorldToScreenScale().X);
            //Height      = (int)(rect.Height * Camera.Main.WorldToScreenScale().Y);
            //Location    = new(point.X - Width / 2, point.Y - Height / 2);

            _bounds = _args.BoundRect();
        }

        private void Sprite_DrawBegin(object? sender, DrawBeginEventArgs args)
        {
            _args = args;
        }

        public override void Update(UIArgs args)
        {
            base.Update(args);

            CalcBounds();

            if (args.InputInfo.CurMouse.LeftButton == XNA.Input.ButtonState.Pressed)
            {
                _showBorder ^= true;
            }
        }

        private void CalcBounds()
        {
            Location = Camera.Main.CoordinateToPoint(new(_bounds.X, _bounds.Y));

            var size = new Size2
                (_bounds.Width * Camera.Main.WorldToScreenScale().X,
                _bounds.Height * Camera.Main.WorldToScreenScale().Y);

            Width = (int)size.Width;
            Height = (int)size.Height;

        }

        public override void Draw(UIArgs args)
        {
            if (_e.HasComponent(typeof(TileComponent))) return;

            base.Draw(args);
        }

        TransformEntity _e;
        SpriteComponent _sp;
        DrawBeginEventArgs _args;
        RectangleF _bounds;
        bool _showBorder = false;
    }
}