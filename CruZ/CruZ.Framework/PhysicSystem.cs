﻿//using Box2D.NetStandard.Dynamics.World;
//using CruZ.ECSManager;
//using Microsoft.Xna.Framework;
//using MonoGame.Extended.Entities;
//using MonoGame.Extended.Entities.GameSystem;

//namespace CruZ.GameSystem
//{
//    class PhysicSystem : EntityUpdateSystem
//    {
//        public PhysicSystem() : base(Aspect.All(typeof(PhysicBody)))
//        {
//            _physicWorld = new();
//        }

//        public override void CreateContext(IComponentMapperService mapperService)
//        {
//            _physicBodyMapper = mapperService.GetMapper<PhysicBody>();

//            foreach(var entityId in ActiveEntities)
//            {
//                PhysicBody body = _physicBodyMapper.Get(entityId);
//                if(!body.IsInitialize)
//                {
//                    body.CreateContext(_physicWorld);
//                }
//            }
//        }

//        public override void OnUpdate(GameTime gameTime)
//        {
//            _physicWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds, 6, 2);
//        }

//        Box2D.NetStandard.Dynamics.World.World _physicWorld;
//        ComponentMapper<PhysicBody> _physicBodyMapper;
//    }
//}