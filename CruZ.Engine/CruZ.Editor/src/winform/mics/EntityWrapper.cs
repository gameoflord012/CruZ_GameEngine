﻿using CruZ.Components;
using System.ComponentModel;

namespace CruZ.Editor
{
    public class EntityWrapper
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TransformEntity TransformEntity { get; }

        public EntityWrapper(TransformEntity e)
        {
            TransformEntity = e;
            RefreshComponents();
        }

        public void RefreshComponents()
        {
            Components = new(TransformEntity.Components);
        }

        public ComponentsWrapper Components { get; set; }
    }

    [TypeConverter(typeof(ComponentsTypeConverter))]
    public class ComponentsWrapper
    {
        public ComponentsWrapper(Components.Component[] comps)
        {
            Components = comps;
        }

        public Components.Component[] Components { get; set; }
    }
}