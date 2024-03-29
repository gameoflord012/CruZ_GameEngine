﻿using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using System.Drawing.Design;
using CruZ.Framework.UI;
using CruZ.Framework.Resource;
using CruZ.Framework.DataType;
using CruZ.Framework;
using CruZ.Framework.GameSystem.Render;
using CruZ.Framework.Utility;

namespace CruZ.Common.ECS
{
    /// <summary>
    /// Game component loaded from specify resource
    /// </summary>
    public partial class SpriteRendererComponent : RendererComponent, IHasBoundBox
    {
        public event EventHandler<DrawArgs>? DrawLoopBegin;
        public event EventHandler<DrawLoopEndEventArgs>? DrawLoopEnd;
        public event Action? DrawBegin;
        public event Action? DrawEnd;
        public event Action<UIBoundingBox> BoundingBoxChanged;

        #region Properties
        public bool SortByY { get; set; } = false;
        public bool Flip { get; set; }

        [JsonIgnore, Browsable(false)]
        public Texture2D? Texture { get => _texture; set => _texture = value; }

        [TypeConverter(typeof(Vector2TypeConverter))]
        public Vector2 NormalizedOrigin { get; set; } = new(0.5f, 0.5f);

        [Editor(typeof(FileUITypeEditor), typeof(UITypeEditor))]
        public string TexturePath
        {
            get => _spriteResInfo != null ? _spriteResInfo.ResourceName : "";
            set => LoadTexture(value);
        }
        #endregion

        public SpriteRendererComponent()
        {
            _resource = GameContext.GameResource;
            InitBoundingBoxEventHandlers();
        }

        private void InitBoundingBoxEventHandlers()
        {
            DrawBegin += () =>
            {
                _boundingBox.WorldOrigins.Clear();
                _hasBoundingBox = false;
            };

            DrawLoopEnd += (sender, args) =>
            {
                _boundingBox.WorldOrigins.Add(args.DrawArgs.GetWorldOrigin());

                if (!_hasBoundingBox)
                {
                    _boundingBox.WorldBounds = args.DrawArgs.GetWorldBounds();
                    _hasBoundingBox = true;
                }
                else
                {
                    var bounds = args.DrawArgs.GetWorldBounds();

                    _boundingBox.WorldBounds.X = MathF.Min(_boundingBox.WorldBounds.X, bounds.X);
                    _boundingBox.WorldBounds.Y = MathF.Min(_boundingBox.WorldBounds.Y, bounds.Y);
                    _boundingBox.WorldBounds.Width = _boundingBox.WorldBounds.Right < bounds.Right ? bounds.Right - _boundingBox.WorldBounds.X : _boundingBox.WorldBounds.Width;
                    _boundingBox.WorldBounds.Height = _boundingBox.WorldBounds.Bottom < bounds.Bottom ? bounds.Bottom - _boundingBox.WorldBounds.Y : _boundingBox.WorldBounds.Height;
                }
            };

            DrawEnd += () => BoundingBoxChanged?.Invoke(_hasBoundingBox ? _boundingBox : UIBoundingBox.Default);
        }

        public void LoadTexture(string texturePath)
        {
            if (!string.IsNullOrEmpty(texturePath))
            {
                _spriteResInfo = _resource.RetriveResourceInfo(texturePath);

                try
                {
                    Texture = _resource.Load<Texture2D>(_spriteResInfo);
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Failed to load texture with path \"{texturePath}\"", e);
                }
            }
        }

        public int CompareLayer(SpriteRendererComponent other)
        {
            return SortingLayer == other.SortingLayer ?
                CalculateLayerDepth().CompareTo(other.CalculateLayerDepth()) :
                SortingLayer.CompareTo(other.SortingLayer);
        }

        public override void Render(GameTime gameTime, SpriteBatch spriteBatch, Matrix viewProjectionMatrix)
        {
            if(Texture == null) return;

            var fx = EffectManager.NormalSpriteRenderer;
            fx.Parameters["view_projection"].SetValue(viewProjectionMatrix);

            DrawBegin?.Invoke();
            spriteBatch.Begin(
                effect: EffectManager.NormalSpriteRenderer,
                sortMode: SpriteSortMode.FrontToBack,
                samplerState: SamplerState.PointClamp);

            while (true)
            {
                #region Before Drawloop
                DrawArgs drawArgs = new();
                drawArgs.Apply(AttachedEntity);
                drawArgs.Apply(Texture);
                drawArgs.LayerDepth = CalculateLayerDepth();
                drawArgs.NormalizedOrigin = NormalizedOrigin;
                drawArgs.Color = Color.White;
                drawArgs.Flip = Flip;
                DrawLoopBegin?.Invoke(this, drawArgs); 
                #endregion

                spriteBatch.Draw(drawArgs);

                #region After Drawloop
                var drawEndArgs = new DrawLoopEndEventArgs(drawArgs);
                DrawLoopEnd?.Invoke(this, drawEndArgs);
                if (!drawEndArgs.KeepDrawing) break; 
                #endregion
            }

            spriteBatch.End();
            DrawEnd?.Invoke();
        }

        private float CalculateLayerDepth()
        {
            return SortByY ? AttachedEntity.Transform.Position.Y / 2 : LayerDepth;
        }

        public override string ToString()
        {
            return _spriteResInfo != null ? _spriteResInfo.ResourceName : "<None>";
        }

        Texture2D? _texture;
        [JsonProperty]
        ResourceInfo? _spriteResInfo;
        ResourceManager _resource;
        UIBoundingBox _boundingBox = new();
        bool _hasBoundingBox;
    }
}
