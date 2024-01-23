﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace CruZ.Components
{
    public class TileComponent : IComponent, IComponentCallback
    {
        public Type ComponentType => typeof(TileComponent);

        public int TileSize = 16;

        public void OnComponentAdded(TransformEntity entity)
        {
            if(_sp != null)
            {
                _sp.OnDrawBegin -= Sprite_OnDrawBegin;
                _sp.OnDrawEnd -= Sprite_OnDrawEnd;
            }

            _e = entity;
            _sp = entity.GetComponent<SpriteComponent>();

            _sp.OnDrawBegin += Sprite_OnDrawBegin;
            _sp.OnDrawEnd += Sprite_OnDrawEnd;
        }

        private void Sprite_OnDrawBegin(object? sender, EventArgs e)
        {
            if((_idX + _idY) % 2 == 0) return;
            _sp.SourceRectangle = new(_idX * TileSize, _idY * TileSize, TileSize, TileSize);
        }

        private void Sprite_OnDrawEnd(object? sender, DrawEndEventArgs e)
        {
            var bounds = _sp.Texture.Bounds;

            if(_idX * TileSize < bounds.Width)
            {
                _idX++;
            }
            else if(_idY * TileSize < bounds.Height)
            {
                _idX = 0;
                _idY++;
            }
            else
            {
                return;
            }

            e.KeepDrawing = true;
        }

        int _idX, _idY;

        TransformEntity _e;
        SpriteComponent? _sp;
    }
}
