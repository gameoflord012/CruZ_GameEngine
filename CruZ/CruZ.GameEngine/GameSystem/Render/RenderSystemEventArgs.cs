﻿using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CruZ.GameEngine.GameSystem.Render
{
    public class RenderSystemEventArgs : EventArgs
    {
        public readonly GameTime GameTime;
        public readonly SpriteBatch SpriteBatch;
        public readonly Matrix ViewProjectionMatrix;
        public readonly RenderTarget2D SpriteRenderTarget;

        public RenderSystemEventArgs(GameTime gameTime, SpriteBatch spriteBatch, Matrix viewProjectionMatrix, RenderTarget2D spriteRenderTarget)
        {
            GameTime = gameTime;
            SpriteBatch = spriteBatch;
            ViewProjectionMatrix = viewProjectionMatrix;
            SpriteRenderTarget = spriteRenderTarget;
        }
    }
}
