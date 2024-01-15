﻿using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace CruZ.Components
{
    public class Transform
    {
        public event Action<Vector3> OnPositionChanged;

        public Transform()
        {
            _position = Vector3.Zero;
            _scale = Vector3.One;
        }

        [JsonIgnore]
        public Matrix TotalMatrix { get => ScaleMatrix * TranslateMatrix; }
        [JsonIgnore]
        public Matrix TranslateMatrix { get => Matrix.CreateTranslation(_position); }
        [JsonIgnore]
        public Matrix ScaleMatrix { get => Matrix.CreateScale(_scale); }
        
        public Vector3 Scale { get => _scale; set => _scale = value; }
        
        public Vector3 Position 
        { 
            get => _position;
            set { _position = value; OnPositionChanged?.Invoke(_position); }
        }

        Vector3 _position;
        Vector3 _scale;
    }
}
