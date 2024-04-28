﻿using CruZ.GameEngine;
using CruZ.GameEngine.GameSystem;
using CruZ.GameEngine.GameSystem.Animation;
using CruZ.GameEngine.GameSystem.ECS;
using CruZ.GameEngine.GameSystem.Scene;
using CruZ.GameEngine.GameSystem.Script;
using CruZ.GameEngine.Input;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NinjaAdventure
{
    internal class NinjaCharacter
    {
        public NinjaCharacter(GameScene scene, SpriteRendererComponent spriteRenderer)
        {
            _gameScene = scene;
            Entity = scene.CreateEntity("Ninja");

            _animationComponent = new AnimationComponent(spriteRenderer);
            {
                _animationComponent.FitToWorldUnit = true;
                _animationComponent.LoadAnimationFile("art\\NinjaAnim.aseprite");
            }
            Entity.AddComponent(_animationComponent);

            _scriptComponent = new ScriptComponent();
            {
                _scriptComponent.Updating += Script_Updating;
            }
            Entity.AddComponent(_scriptComponent);

            _surikenRenderer = new SpriteRendererComponent();
            {

            }
            Entity.AddComponent(_surikenRenderer);

            InputManager.KeyStateChanged += Input_KeyStateChanged;
        }

        private void Script_Updating(GameTime gameTime)
        {
            //
            // movement update
            //
            if(!_isAttackAnimationPlaying) // don't move when attacking
                Entity.Transform.Position += _inputMovement * gameTime.GetElapsedSeconds() * _speed;

            //
            // spawning suriken
            //
            if (_inputFireSuriken)
            {
                var suriken = new Suriken(_gameScene, _surikenRenderer, Entity.Position, _inputMovement);
                suriken.BecomeUseless += suriken.Dispose;
            }

            //
            // animations
            //
            var facingString = AnimationHelper.GetFacingDirectionString(_inputMovement);
            
            if(_inputFireSuriken)
            {
                _animationComponent.PlayAnimation($"attack-{facingString}", 1);
                _isAttackAnimationPlaying = true;
                _animationComponent.CurrentAnimation!.OnAnimationEnd = 
                    (animation) => _isAttackAnimationPlaying = false;
            }
            
            if(!_isAttackAnimationPlaying) // we don't want moving animation playing when player attacking
            {
                _animationComponent.PlayAnimation($"walk-{facingString}");
            }

            if (_inputFireSuriken) _inputFireSuriken = false;
        }


        private void Input_KeyStateChanged(IInputInfo inputInfo)
        {
            _inputMovement = Vector2.Zero;
            _inputFireSuriken = false;

            if (inputInfo.IsKeyHeldDown(Keys.A))
            {
                _inputMovement += new Vector2(-1, 0);
            }
            if (inputInfo.IsKeyHeldDown(Keys.D))
            {
                _inputMovement += new Vector2(1, 0);
            }
            if (inputInfo.IsKeyHeldDown(Keys.W))
            {
                _inputMovement += new Vector2(0, 1);
            }
            if (inputInfo.IsKeyHeldDown(Keys.S))
            {
                _inputMovement += new Vector2(0, -1);
            }

            if (inputInfo.IsKeyJustDown(Keys.Space))
            {
                _inputFireSuriken = true;
            }
        }

        List<Suriken> surikens = [];

        Vector2 _inputMovement;

        bool _inputFireSuriken;
        bool _isAttackAnimationPlaying;
        float _speed = 4;

        AnimationComponent _animationComponent;
        ScriptComponent _scriptComponent;
        SpriteRendererComponent _surikenRenderer;

        GameScene _gameScene;

        public TransformEntity Entity;
    }
}
