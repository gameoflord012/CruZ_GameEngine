﻿using CruZ.Common.ECS.Ultility;
using CruZ.Common.Utility;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;

using System.Collections.Generic;
using System.Linq;

namespace CruZ.Common.ECS
{
    internal class RenderSystem : EntitySystem, IUpdateSystem, IDrawSystem
    {
        public RenderSystem() : base(Aspect.One(
            typeof(SpriteComponent), typeof(LightComponent)))
        {
            GameApplication.RegisterWindowResize(GameApp_WindowResize);
        }

        public override void Initialize(IComponentMapperService mapperService)
        {
            _spriteMapper = mapperService.GetMapper<SpriteComponent>();
            _lightMapper = mapperService.GetMapper<LightComponent>();
            _spriteBatch = GameApplication.GetSpriteBatch();
            _gd = GameApplication.GetGraphicsDevice();
        }

        public void Draw(GameTime gameTime)
        {
            List<SpriteComponent> sprites = GetSortedSpriteList();
            List<LightComponent> lights = this.GetAllComponents(_lightMapper);
            List<int> sortingLayers = [];

            // process sprites
            int i = 0;
            while (i < sprites.Count)
            {
                var sortingLayer = sprites[i].SortingLayer;
                sortingLayers.Add(sortingLayer);

                var renderTarget = GetRenderTarget(sortingLayer);

                _gd.SetRenderTarget(renderTarget);
                _gd.Clear(Color.Transparent);

                var fx = EffectManager.NormalSpriteRenderer;
                fx.Parameters["view_projection"].SetValue(GetViewProjectionMatrix());

                // render sprite
                _spriteBatch.Begin(
                    effect: fx,
                    sortMode: SpriteSortMode.FrontToBack,
                    samplerState: SamplerState.PointClamp);
                do
                {
                    sprites[i].InternalDraw(_spriteBatch);
                    i++;
                } while (
                    i < sprites.Count &&
                    sprites[i].SortingLayer == sprites[i - 1].SortingLayer);

                _spriteBatch.End();

                // render lights
                foreach (var light in lights
                    .Where(e => e.SortingLayers.Contains(sortingLayer)))
                {
                    light.InternalDraw(_spriteBatch, GetViewProjectionMatrix());
                }
            }

            // render all renderTargets to back buffer
            _gd.SetRenderTarget(null);
            _gd.Clear(GameConstants.DEFAULT_BACKGROUND_COLOR);
            _spriteBatch.Begin();
            foreach (var sortingLayer in sortingLayers)
            {
                var renderTarget = GetRenderTarget(sortingLayer);
                _spriteBatch.Draw(renderTarget, new Vector2(0, 0), Color.White);
            }
            _spriteBatch.End();
        }

        private Matrix GetViewProjectionMatrix()
        {
            return Camera.Main.ViewMatrix() * Camera.Main.ProjectionMatrix();
        }

        private void GameApp_WindowResize(Viewport viewport)
        {
            foreach (var renderTarget in _renderTargets.Values)
                renderTarget.Dispose();
            _renderTargets.Clear();
        }

        private RenderTarget2D GetRenderTarget(int sortingLayer)
        {
            if (!_renderTargets.ContainsKey(sortingLayer))
                _renderTargets[sortingLayer] = new RenderTarget2D(_gd, _gd.Viewport.Width, _gd.Viewport.Height);
            return _renderTargets[sortingLayer];
        }

        private List<SpriteComponent> GetSortedSpriteList()
        {
            List<SpriteComponent> sprites = this.GetAllComponents(_spriteMapper).ToList();
            sprites.Sort((s1, s2) => { return s1.SortingLayer.CompareTo(s2.SortingLayer); });
            return sprites;
        }

        public virtual void Update(GameTime gameTime) { }

        SpriteBatch _spriteBatch;
        ComponentMapper<LightComponent> _lightMapper;
        ComponentMapper<SpriteComponent> _spriteMapper;
        Dictionary<int, RenderTarget2D> _renderTargets = [];
        GraphicsDevice _gd;
        Effect _lightEffect;
    }
}
