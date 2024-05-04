﻿using CruZ.GameEngine;
using CruZ.GameEngine.GameSystem;
using CruZ.GameEngine.GameSystem.Animation;
using CruZ.GameEngine.GameSystem.ECS;
using CruZ.GameEngine.GameSystem.Physic;
using CruZ.GameEngine.GameSystem.Scene;
using CruZ.GameEngine.GameSystem.Script;
using CruZ.GameEngine.GameSystem.StateMachine;

using Genbox.VelcroPhysics.Collision.ContactSystem;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace NinjaAdventure
{
    internal class NinjaCharacter : IDisposable
    {
        public NinjaCharacter(GameScene scene, SpriteRendererComponent spriteRenderer)
        {
            _spriteRenderer = spriteRenderer;
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

            _physic = new PhysicBodyComponent();
            {
                FixtureFactory.AttachCircle(0.5f, 1, _physic.Body);
                _physic.BodyType = BodyType.Dynamic;
                _physic.IsSensor = true;
                _physic.OnCollision += Physic_OnCollision;
                _physic.OnSeperation += Physic_OnSeperation; ;
            }
            Entity.AddComponent(_physic);

            _health = new HealthComponent(30, spriteRenderer);
            {

            }
            Entity.AddComponent(_health);

            _machine = new StateMachineComponent();
            {
                _machine.SetData("PhysicComponent", _physic);
                _machine.SetData("AnimationComponent", _animationComponent);
                _machine.SetData("SpriteRenderer", spriteRenderer);
                _machine.SetData("GameScene", _gameScene);
                _machine.SetData("NinjaCharacter", this);
                _machine.Add(new NinjaAttackState());
                _machine.Add(new NinjaMovingState());
            }
            Entity.AddComponent(_machine);

            _machine.SetNextState(typeof(NinjaMovingState));
        }

        internal void SpawnSuriken(Vector2 direction)
        {
            var suriken = new Suriken(_gameScene, _spriteRenderer, Entity.Position, direction);
            suriken.BecomeUseless += () => uselessSurikens.Add(suriken);
        }

        private void Physic_OnSeperation(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if(IsMonster(fixtureB))
            {
                _monsterCount--;
            }
        }

        private void Physic_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (IsMonster(fixtureB))
            {
                _monsterCount++;
            }
        }

        private static bool IsMonster(Fixture fixtureB)
        {
            return fixtureB.Body.UserData is LarvaMonster;
        }

        private void Script_Updating(GameTime gameTime)
        {
            ClearUselessSuriken();

            //_isAttackAnimationPlaying =
            //    _animationComponent.IsAnimationPlaying() &&
            //    _animationComponent.CurrentAnimationName().StartsWith("attack");

            // movement update
            //if (!_isAttackAnimationPlaying) MovingLogic(); // don't move when attacking

            //// check can firing new suriken
            
            //if (!_isAttackAnimationPlaying) // we don't want moving animation playing when player attacking
            //{
            //}

        }

        //private string CurrentFacingDir()
        //{
        //    return AnimationHelper.GetFacingDirectionString(_ninjaInput.Movement);
        //}

        private void ClearUselessSuriken()
        {
            foreach (var useless in uselessSurikens)
            {
                useless.Dispose();
            }

            uselessSurikens.Clear();
        }

        PhysicBodyComponent _physic;
        ScriptComponent _scriptComponent;
        HealthComponent _health;
        StateMachineComponent _machine;
        SpriteRendererComponent _spriteRenderer;

        AnimationComponent _animationComponent;
        bool _isAttackAnimationPlaying;

        int _monsterCount = 0;
        List<Suriken> uselessSurikens = [];

        GameScene _gameScene;

        public TransformEntity Entity;

        public void Dispose()
        {
            _physic.OnCollision -= Physic_OnCollision;
            _scriptComponent.Updating -= Script_Updating;

            foreach (var uselessSuriken in uselessSurikens)
            {
                uselessSuriken.Dispose();
            }
        }
    }
}
