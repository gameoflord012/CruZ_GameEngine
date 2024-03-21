﻿using System;

namespace CruZ.Common.GameSystem.Resource
{
    public class ResourceInfo
    {
        private ResourceInfo() { }

        private ResourceInfo(Guid guid, string resourceName)
        {
            Guid = guid;
            ResourceName = resourceName;
        }

        public static ResourceInfo Create(Guid guid, string resourceName)
        {
            return new ResourceInfo(guid, resourceName);
        }

        public Guid Guid { get; private set; }
        public string ResourceName { get; private set; }
    }
}