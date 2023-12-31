﻿using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CruZ.Components
{
    public static class EntityExtension
    {
        public static IEnumerable<T> GetAllComponents<T>(this EntitySystem system, ComponentMapper<T> mapper) where T : class
        {
            foreach(var e in system.GetActiveEntities())
            {
                yield return mapper.Get(e);
            }

            yield break;
        }

        public static void Attach(this Entity e, object component, Type ty)
        {
            var mapper = e.ComponentManager.GetMapper(ty);
            mapper.Put(e.Id, component);
        }

        public static TransformEntity CreateTransformEntity(this World world)
        {
            var e = world.CreateEntity();
            TransformEntity t_e = new(e);
            return t_e;
        }

        public static int[] GetActiveEntities(this EntitySystem es)
        {
            return es.ActiveEntities.
                Where(e => TransformEntity.GetTransformEntity(e).IsActive).
                ToArray();
        }
    }
}