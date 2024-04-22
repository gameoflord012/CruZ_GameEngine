﻿using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using CruZ.GameEngine.GameSystem.UI;
using CruZ.GameEngine.Resource;
using CruZ.GameEngine.Service;

namespace CruZ.Editor.UI
{
    internal class LoggingWindow : UIControl
    {
        Dictionary<string, string> TextInfo = [];

        public LoggingWindow()
        {
            Location = new(5, 3);

            _fontScale = 0.5f;
            _resource = EditorContext.EditorResource;
            _font = _resource.Load<SpriteFont>(".resourceref\\Internal\\font\\editorfont.spritefont");
            _lineSpacing = _font.LineSpacing * _fontScale;
            _curRow = 0;
        }

        protected override void OnDraw(UIInfo info)
        {
            _sb = info.SpriteBatch;
            _curRow = 0;
            DrawString(LogManager.GetMsg("Fps"));
            _curRow++;
            DrawString(LogManager.GetMsg("Scene"));
            _curRow++;
            DrawString(LogManager.GetMsg("CursorCoord"));
            _curRow++;
            DrawString(LogManager.GetMsg("Default"));
        }

        private void DrawString(string s)
        {
            _sb?.DrawString(
                _font, s,
                new Vector2(
                    Location.X, Location.Y + _curRow * _lineSpacing),
                Color.Black
                , 0, new Vector2(0, 0), _fontScale, SpriteEffects.None, 0
                );
        }

        SpriteFont _font;
        SpriteBatch? _sb;
        float _lineSpacing;
        float _curRow;
        float _fontScale;

        ResourceManager _resource;
    }
}
