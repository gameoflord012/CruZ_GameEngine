﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruZ.GameEngine.GameSystem
{
    public class TransformEntityHelper
    {
        /// <summary>
        /// parent index alway smaller than its children
        /// </summary>
        public static List<TransformEntity> SortByDepth(IReadOnlyCollection<TransformEntity> entities)
        {
            List<TransformEntity> sorted = [];
            HashSet<TransformEntity> visited = [];

            foreach (var current in entities)
            {
                ClimbUp(visited, sorted, current);
            }

            return sorted;
        }

        private static void ClimbUp(HashSet<TransformEntity> visited, List<TransformEntity> sorted, TransformEntity current)
        {
            visited.Add(current);

            if (current.Parent == null || visited.Contains(current.Parent))
            {
                // ignore
            }
            else
            {
                ClimbUp(visited, sorted, current.Parent);
            }

            sorted.Add(current);
        }
    }
}
