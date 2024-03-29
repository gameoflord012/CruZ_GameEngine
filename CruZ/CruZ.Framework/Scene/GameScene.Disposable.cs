﻿using System;

namespace CruZ.Common.Scene
{
    public partial class GameScene : IDisposable
    {
        public void Dispose()
        {
            SetActive(false);

            foreach (var e in _entities)
            {
                e.Dispose();
            }
        }
    }
}