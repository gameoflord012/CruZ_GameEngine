﻿using CruZ.Framework.Input;
using CruZ.Framework.Utility;

using Microsoft.Xna.Framework;


using System;
using System.Collections.Generic;

namespace CruZ.Common.UI
{
    public partial class UIControl : IDisposable
    {
        static readonly int BOUND_THICKNESS = 3;
        static readonly Color DEFAULT_BACKGROUND_COLOR = XNA.Color.Red;

        #region Properties
        public UIControl? Parent { get => _parent; }
        public UIControl[] Childs => _childs.ToArray();

        public Point Location
        {
            get => new((int)_location.X, (int)_location.Y);
            set { _location.X = value.X; _location.Y = value.Y; }
        }
        public int Width { get => (int)_size.X; set => _size.X = value; }
        public int Height { get => (int)_size.Y; set => _size.Y = value; }

        public Color BackgroundColor = DEFAULT_BACKGROUND_COLOR;

        public object? Tag { get; set; } = null;
        #endregion

        public void AddChild(UIControl child)
        {
            _childs.Add(child);

            child._parent = this;
            child.OnParentChanged(this);
        }

        public DRAW.RectangleF GetRect()
        {
            return new DRAW.RectangleF(_location.X, _location.Y, _size.X, _size.Y);
        }

        public void RemoveChild(UIControl child)
        {
            if(!_childs.Remove(child))
            {
                throw new ArgumentException($"Fail to remove {child} ui control from {this}");
            }

            child._parent = null;
            child.OnParentChanged(null);
        }

        internal void InternalUpdate(UIInfo args)
        {
            _args = args;

            // dragging
            ProcessDragging(args);
            
            // update mouse events
            if (IsMouseHover())
            {
                if (args.InputInfo.MouseClick && !Dragging())
                {
                    OnMouseClick(args);
                }

                if (args.InputInfo.MouseStateChanges)
                {
                    OnMouseStateChange(args);
                }
            }

            // call update
            OnUpdate(args);
        }

        internal void InternalDraw(UIInfo args)
        {
            _args = args;
            OnDraw(args);
        }

        protected bool IsMouseHover()
        {
            return GetRect().Contains(_args.MousePos().X, _args.MousePos().Y);
        }

        protected void ReleaseDrag()
        {
            s_GlobalDragObject = null;
        }

        protected virtual void OnMouseClick(UIInfo args)
        {
        }
        protected virtual void OnMouseStateChange(UIInfo args) { }
        protected virtual void OnParentChanged(UIControl? parent) { }
        protected virtual void OnUpdate(UIInfo args) { }
        protected virtual void OnDraw(UIInfo args)
        {
            args.SpriteBatch.DrawRectangle(GetRect(), BackgroundColor, BOUND_THICKNESS);
        }

        #region Dragging
        protected virtual object? OnStartDragging(UIInfo args) => null;
        protected virtual void OnUpdateDragging(UIInfo args) { }
        protected virtual bool OnReleaseDragging() => true;

        private void ProcessDragging(UIInfo args)
        {
            if (!Dragging() &&
                IsMouseHover() &&
                args.InputInfo.IsMouseHeldDown(MouseKey.Left) &&
                args.InputInfo.MouseMoving)
            {
                _dragObject = OnStartDragging(args);
                s_GlobalDragObject = _dragObject;
            }

            
            if(Dragging() && s_GlobalDragObject == _dragObject)
            {
                OnUpdateDragging(args);

                if(args.InputInfo.IsMouseHeldUp(MouseKey.Left))
                {
                    if(OnReleaseDragging())
                    {
                        _dragObject = null;
                        s_GlobalDragObject = null;
                    }
                }
            }
        }

        public void Dispose()
        {
            if(Parent != null)
            {
                Parent.RemoveChild(this);
            }
        }
        #endregion

        #region Private_Variables
        List<UIControl> _childs = [];
        UIControl? _parent;

        Vector2 _location = new(0, 0);
        Vector2 _size = new(0, 0);

        object? _dragObject;

        UIInfo _args;
        #endregion
    }

    public partial class UIControl
    {
        private static object? s_GlobalDragObject = null;
        public static bool Dragging() { return s_GlobalDragObject != null; }
    }
}