﻿using CruZ.Common.ECS;
using CruZ.Framework;
using CruZ.Framework.GameSystem.ECS;
using CruZ.Framework.GameSystem.Render;
using CruZ.Framework.Resource;
using CruZ.Framework.Serialization;

using Microsoft.Xna.Framework;

using MonoGame.Extended.Sprites;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CruZ.Framework.GameSystem.Animation
{
    /// <summary>
    /// Playing animations in SpriteSheet
    /// </summary>
    public class AnimationPlayer
    {
        public AnimationPlayer(SpriteSheet spriteSheet)
        {
            _animatedSprite = new AnimatedSprite(spriteSheet);
        }

        public void Play(string animationName)
        {
            try
            {
                _animatedSprite.Play(animationName);
            }
            catch (KeyNotFoundException e)
            {
                throw new(string.Format("Cant found animation with key {0}", animationName), e);
            }
        }

        public void Update(GameTime gameTime)
        {
            _animatedSprite.Update(gameTime);
        }

        public void Load(SpriteRendererComponent sprite)
        {
            if (_sprite == sprite) return;

            UnLoad();
            _sprite = sprite;

            _sprite.DrawLoopBegin += Sprite_DrawLoopBegin;
            _sprite.DrawLoopEnd += Sprite_DrawLoopEnd;
        }

        public void UnLoad()
        {
            if (_sprite != null)
            {
                _sprite.DrawLoopBegin -= Sprite_DrawLoopBegin;
                _sprite.DrawLoopEnd -= Sprite_DrawLoopEnd;
            }

            _sprite = null;
        }

        private void Sprite_DrawLoopBegin(object? sender, DrawArgs e)
        {
            e.Texture = _animatedSprite.TextureRegion.Texture;
            e.SourceRectangle = _animatedSprite.TextureRegion.Bounds;
            e.NormalizedOrigin =
                new(
                _animatedSprite.OriginNormalized.X - 0.5f + e.NormalizedOrigin.X,
                _animatedSprite.OriginNormalized.Y - 0.5f + e.NormalizedOrigin.Y);
        }

        private void Sprite_DrawLoopEnd(object? sender, DrawLoopEndEventArgs e)
        {

        }

        AnimatedSprite _animatedSprite;
        SpriteRendererComponent? _sprite;
    }
}

namespace CruZ.Framework.GameSystem.Animation
{
    public class AnimationComponent : Component, ICustomSerializable
    {
        public AnimationComponent()
        {
            _resource = GameContext.GameResource;
        }

        public void LoadSpriteSheet(string resourcePath, string animationPlayerKey)
        {
            var spriteSheet = _resource.Load<SpriteSheet>(resourcePath);

            _getAnimationPlayer[animationPlayerKey] = new AnimationPlayer(spriteSheet);
            _loadedResources.Add(new(resourcePath, animationPlayerKey));
        }

        internal void Update(GameTime gameTime)
        {
            _currentAnimationPlayer?.Update(gameTime);
        }

        public AnimationPlayer SelectPlayer(string key)
        {
            if (_currentAnimationPlayer == GetPlayer(key))
                return _currentAnimationPlayer;

            _currentAnimationPlayer?.UnLoad();
            _currentAnimationPlayer = GetPlayer(key);
            _currentAnimationPlayer.Load(_sprite);

            return _currentAnimationPlayer;
        }

        private AnimationPlayer GetPlayer(string key)
        {
            if (!_getAnimationPlayer.ContainsKey(key))
                throw new ArgumentException($"No animation with key {key}");

            return _getAnimationPlayer[key];
        }

        protected override void OnComponentChanged(ComponentCollection comps)
        {
            comps.TryGetComponent(out _sprite);
        }

        public object ReadJson(JsonReader reader, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            foreach (var player in jObject["animation-players"])
            {
                string? uri = player["resource-uri"].Value<string>();
                string? playerKey = player["animation-player-key"].Value<string>();

                if (string.IsNullOrEmpty(uri)) continue;
                Trace.Assert(playerKey != null);

                LoadSpriteSheet(uri, playerKey);
            }

            return this;
        }

        public void WriteJson(JsonWriter writer, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("animation-players");
            writer.WriteStartArray();
            foreach (var resource in _loadedResources)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("resource-uri");
                writer.WriteValue(resource.Key);
                writer.WritePropertyName("animation-player-key");
                writer.WriteValue(resource.Value);
                writer.WriteEndObject();
            }
            writer.WriteEnd();
            writer.WriteEnd();
        }

        AnimationPlayer? _currentAnimationPlayer;
        SpriteRendererComponent? _sprite;
        ResourceManager _resource;
        Dictionary<string, AnimationPlayer> _getAnimationPlayer = new();
        List<KeyValuePair<string, string>> _loadedResources = [];
    }
}