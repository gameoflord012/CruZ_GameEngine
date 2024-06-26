﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using CruZ.GameEngine;

//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//namespace CruZ.Experiment.Filter
//{
//    internal class BloomFilterExperiment : GameWrapper
//    {
//        public BloomFilterExperiment()
//        {
//            Content.RootDirectory = ".\\Content\\bin\\";
//        }

//        protected override void OnInitialize()
//        {
//            base.OnInitialize();
//            _sp = new SpriteBatch(GraphicsDevice);
            
//            GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.DiscardContents;
//        }

//        protected override void LoadContent()
//        {
//            base.LoadContent();

//            _filter = new BloomFilter();
//            _filter.Load(GraphicsDevice, Content);
//            _filter.BloomPreset = BloomFilter.BloomPresets.Wide;
//            _filter.BloomThreshold = 1f;
//            _tex = Content.Load<Texture2D>("homelander");
//        }

//        protected override void OnDrawing(GameTime gameTime)
//        {
//            base.OnDrawing(gameTime); 
          
//            var filtered = _filter.DrawRequest(_tex, _tex.Width, _tex.Height);

//            GraphicsDevice.SetRenderTarget(null);
//            _sp.Begin(transformMatrix: Matrix.CreateScale(0.5f));
//            _sp.DrawRequest(filtered, Vector2.Zero, Color.White);
//            _sp.End();

//        }

//        protected override void OnExiting(object sender, EventArgs args)
//        {
//            base.OnExiting(sender, args);
//            _filter.Dispose();
//        }

//        SpriteBatch _sp;
//        Texture2D _tex;
//        BloomFilter _filter;
//    }
//}
