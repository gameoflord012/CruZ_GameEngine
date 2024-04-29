﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace CruZ.GameEngine.GameSystem
{
    internal class ECSWorld : IDisposable
    {
        public event Action<TransformEntity>? EntityAdded;
        public event Action<TransformEntity>? EntityRemoved;

        public ECSWorld() { }

        public ECSWorld AddSystem(EntitySystem system)
        {
            _systems.Add(system);
            return this;
        }

        internal void AddEntity(TransformEntity e)
        {
            _entitiesToRemove.Remove(e);
            _entitiesToAdd.Add(e);
        }

        internal void RemoveEntity(TransformEntity e)
        {
            _entitiesToAdd.Remove(e);
            _entitiesToRemove.Add(e);
        }

        public void Initialize()
        {
            foreach (var system in _systems)
            {
                system.OnInitialize();
            }
        }

        public void SystemsUpdate(GameTime gameTime)
        {
            ProcessEntitiesChanges();

            foreach (var system in _systems)
            {
                system.Update(new EntitySystemEventArgs(GetActiveEntities(), gameTime));
            }
        }

        public void SystemsDraw(GameTime gameTime)
        {
            ProcessEntitiesChanges();

            foreach (var system in _systems)
            {
                system.Draw(new EntitySystemEventArgs(GetActiveEntities(), gameTime));
            }
        }

        private List<TransformEntity> GetActiveEntities()
        {
            return _entities.Where(e => e.IsActive).ToList();
        }

        private void ProcessEntitiesChanges()
        {
            var removingEntities = _entitiesToRemove.ToImmutableList();
            var addingEntities = _entitiesToAdd.ToImmutableList();
            //
            // update entities
            //
            _entities.ExceptWith(removingEntities);
            _entities.UnionWith(addingEntities);
            //
            // fire events
            //
            foreach (var toRemove in removingEntities)
            {
                _entitiesToRemove.Remove(toRemove);
                EntityRemoved?.Invoke(toRemove);
            }

            foreach (var toAdd in addingEntities)
            {
                _entitiesToAdd.Remove(toAdd);
                EntityAdded?.Invoke(toAdd);
            }
        }

        public void Dispose()
        {
            _systems.ForEach(e => e.Dispose());
            foreach (var e in Entities) e.Dispose();
        }

        public IImmutableList<TransformEntity> Entities { get => _entities.ToImmutableList(); }

        HashSet<TransformEntity> _entitiesToRemove = [];
        HashSet<TransformEntity> _entitiesToAdd = [];
        HashSet<TransformEntity> _entities = [];

        List<EntitySystem> _systems = [];
    }
}
