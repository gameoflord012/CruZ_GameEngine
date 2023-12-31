﻿using CruZ.Resource;
using CruZ.Systems;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using System;

namespace CruZ
{
    using Microsoft.Xna.Framework;

    public partial class GameCore : Game
    {

        public event Action?                    InitializeEvent;
        public event Action?                    InitializeSystemEvent;
        public event Action?                    LoadContentEvent;
        public event Action?                    EndRunEvent;
        public event Action<object, EventArgs>? ExitEvent;
        public event Action<GameTime>?          UpdateEvent;
        public event Action<GameTime>?          DrawEvent;
        public event Action<GameTime>?          LateDrawEvent;
        

        public GameCore()
        {
            Content.RootDirectory = ".";
            IsMouseVisible = true;

            _graphics = new GraphicsDeviceManager(this);
        }

        protected override void EndRun()
        {
            base.EndRun();
            EndRunEvent?.Invoke();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            ExitEvent?.Invoke(sender, args);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            LoadContentEvent?.Invoke();
        }

        protected override void Initialize()
        {
            base.Initialize();

            InitalizeSystem();
            InitializeEvent?.Invoke();
        }

        private void InitalizeSystem()
        {
            InitializeSystemEvent?.Invoke();
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            UpdateEvent?.Invoke(gameTime);
            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            InternalDraw(gameTime);
        }

        private void InternalDraw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);
            DrawEvent?.Invoke(gameTime);
            LateDrawEvent?.Invoke(gameTime);
        }

        public void ChangeWindowSize(int width, int height)
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.ApplyChanges();
        }

        private GraphicsDeviceManager _graphics;
    }
}
