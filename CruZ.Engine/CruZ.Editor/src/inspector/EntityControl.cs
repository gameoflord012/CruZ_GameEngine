﻿using CruZ.Components;
using CruZ.Systems;
using CruZ.UI;
using MonoGame.Extended;
using System;
using System.Diagnostics;

namespace CruZ.Editor.UI
{
    public class EntityControl : UIControl
    {
        public EntityControl(TransformEntity e)
        {
            _e = e;
            _sp = e.GetComponent<SpriteComponent>();

            _sp.DrawLoopBegin += Sprite_DrawLoopBegin;
            _sp.DrawLoopEnd += Sprite_DrawLoopEnd;
            _sp.DrawBegin += Sprite_DrawBegin;
        }

        private void Sprite_DrawBegin()
        {
            _bounds.X = _e.Position.X;
            _bounds.Y = _e.Position.Y;
            _bounds.Width = 0;
            _bounds.Height = 0;
        }

        private void Sprite_DrawLoopBegin(object? sender, DrawBeginEventArgs args)
        {
            _args = args;
        }

        private void Sprite_DrawLoopEnd(object? sender, DrawEndEventArgs e)
        {
            var rect = _args.BoundRect();
            _bounds.X = MathF.Min(_bounds.X, rect.X);
            _bounds.Y = MathF.Min(_bounds.Y, rect.Y);
            _bounds.Width = _bounds.Right < rect.Right ? rect.Right - _bounds.X : _bounds.Width;
            _bounds.Height = _bounds.Bottom < rect.Bottom ? rect.Bottom - _bounds.Y : _bounds.Height;
        }

        protected override void OnUpdate(UIArgs args)
        {
            base.OnUpdate(args);

            CalcBounds();

            //if (args.InputInfo.CurMouse.LeftButton == XNA.Input.ButtonState.Pressed)
            //{
            //    _showBorder ^= true;
            //}
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

        protected override void OnDraw(UIArgs args)
        {
            base.OnDraw(args);
        }

        TransformEntity _e;
        SpriteComponent _sp;
        DrawBeginEventArgs _args;
        RectangleF _bounds; //World bounds
        bool _showBorder = false;
    }
}