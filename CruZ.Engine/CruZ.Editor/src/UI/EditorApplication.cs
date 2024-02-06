﻿using CruZ.Components;
using CruZ.Editor.UI;
using CruZ.Resource;
using CruZ.Systems;
using CruZ.UI;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace CruZ.Editor.Controls
{
    public partial class EditorApplication
    {
        public event EventHandler<GameScene> SceneLoadEvent;
        public event Action<TransformEntity?> OnSelectedEntityChanged;

        public GameScene? CurrentGameScene => _currentScene;

        public EditorApplication()
        {
            Input.MouseScrolled     += Input_MouseScroll;
            Input.MouseMoved        += Input_MouseMove;
            Input.MouseStateChanged += Input_MouseStateChanged;

            UIManager.MouseClick += UI_MouseClick;

            CacheService.Register(this);
            UpdateCache?.Invoke(this);

            _thisThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        #region PUBLIC_FUNCS
        public void UnloadCurrentScene()
        {
            if (_currentScene == null) return;

            _currentScene.SetActive(false);
            //_currentScene.Dispose();
        }

        public void SelectEntity(TransformEntity? e)
        {
            if (_currentSelect != null && e == _currentSelect.AttachEntity) 
                return;

            if (_currentSelect != null)
                _currentSelect.SelectEntity(false);

            if(e != null)
            {
                _currentSelect = GetEntityControl(e);
                _currentSelect.SelectEntity(true);
            }
            else
            {
                _currentSelect = null;
            }


            OnSelectedEntityChanged?.Invoke(e);
        }

        public void LoadSceneFromFile(string file)
        {
            Check_AppInitialized();

            var scene = ResourceManager.LoadResource<GameScene>(file);
            LoadScene(scene);
        }

        public void ExitApp()
        {
            if (_gameApp != null && !_gameApp.ExitCalled)
            {
                _gameApp.Exit();
                _gameApp.Dispose();
            }

            if (_gameAppThread != null)
                if (!_gameAppThread.Join(5000))
                    throw new System.Exception("Can't exit editor app");

            _gameApp = null;
            _gameAppThread = null;
        }

        #endregion

        #region EVENT_HANDLER

        private void GameApp_WindowResize(Viewport viewport)
        {
            GetMainCamera().ViewPortWidth = viewport.Width;
            GetMainCamera().ViewPortHeight = viewport.Height;
        }
        
        private void GameApp_Intialized()
        {
            Camera.Main = GetMainCamera();

            _appInitalized_Reset.Set();
        }

        private void GameApp_Exit()
        {
            UnloadCurrentScene();
            ExitAppAsync();
        }
        
        private void Input_MouseScroll(IInputInfo info)
        {
            Camera.Main.Zoom = new(
                Camera.Main.Zoom.X - info.SrollDelta * 0.001f * Camera.Main.Zoom.X, 
                Camera.Main.Zoom.Y);
        }

        private void Input_MouseMove(IInputInfo info)
        {
            if (_isMouseDraggingCamera)
            {
                var scale = Camera.Main.ScreenToWorldScale();
                var delt = new Vector3(
                    (info.CurMouse.Position.X - _mouseStartDragPoint.X) * scale.X,
                    (info.CurMouse.Position.Y - _mouseStartDragPoint.Y) * scale.Y);

                Camera.Main.Position = _cameraStartDragCoord - delt;
            }
        }

        private void Input_MouseStateChanged(IInputInfo info)
        {
            if (info.IsMouseJustDown(MouseKey.Middle)
                && !_isMouseDraggingCamera)
            {
                _isMouseDraggingCamera = true;
                _mouseStartDragPoint = info.CurMouse.Position;
                _cameraStartDragCoord = Camera.Main.Position;
            }

            if (info.IsMouseJustUp(MouseKey.Middle))
            {
                _isMouseDraggingCamera = false;
            }
        }

        private void UI_MouseClick(UIInfo info)
        {
            FindEntityToSelect(info);
        }

        #endregion

        #region PRIVATE
        private void FindEntityToSelect(UIInfo info)
        {
            var contains = UIManager.GetContains(info.MousePos().X, info.MousePos().Y);

            var eControl = contains
                .Where(e => e is EntityControl)
                .Select(e => (EntityControl)e).ToList();

            eControl.Sort((e1, e2) =>
            {
                SpriteComponent? sp1 = null, sp2 = null;
                e1.AttachEntity.TryGetComponent(ref sp1);
                e2.AttachEntity.TryGetComponent(ref sp2);

                if (sp1 == sp2) return 0;
                if (sp1 == null) return -1;
                if (sp2 == null) return 1;

                return sp1.CompareLayer(sp2);
            });

            if (eControl.Count() == 0)
            {
                SelectEntity(null);
                return;
            }

            int idx = 0;

            for (int i = 0; i < eControl.Count(); i++)
            {
                if (eControl[i] == _currentSelect)
                {
                    idx = i;
                    break;
                }
            }

            idx = (idx + 1) % eControl.Count();
            SelectEntity(eControl[idx].AttachEntity);
        }

        private EntityControl? GetEntityControl(TransformEntity e)
        {
            foreach (var control in _eControls)
            {
                if (control.AttachEntity == e)
                {
                    return control;
                }
            }

            return null;
        }

        private void Check_AppInitialized()
        {
            if (_gameAppThread != null && _gameAppThread.IsAlive) return;

            _appInitalized_Reset.Reset();
            var newSession = new Thread(StartNewAppSession);
            newSession.Name = "EditorApp session";
            newSession.Start();
            _appInitalized_Reset.WaitOne();

            _gameAppThread = newSession;
        }

        private void StartNewAppSession()
        {
            ExitApp();

            _gameApp = GameApplication.CreateContext();

            _gameApp.WindowResize += GameApp_WindowResize;
            _gameApp.Initializing += GameApp_Intialized;
            _gameApp.Window.AllowUserResizing = true;
            _gameApp.ExitEvent += GameApp_Exit;

            _gameApp.Run();
        }

        private void InitEntityControl()
        {
            if (_currentScene == null) return;

            _eControls.Clear();

            foreach (var e in _currentScene.Entities)
            {
                var eControl = new EntityControl(e);
                UIManager.Root.AddChild(eControl);
                _eControls.Add(eControl);
            }
        }

        private void LoadScene(GameScene scene)
        {
            UnloadCurrentScene();

            _currentScene = scene;
            _currentScene.SetActive(true);
            SceneLoadEvent?.Invoke(this, _currentScene);

            InitEntityControl();
        }

        private Camera GetMainCamera()
        {
            return _mainCamera ??= new Camera(_gameApp.GraphicsDevice.Viewport);
        }
        #endregion

        #region THREADING
        public void ExitAppAsync()
        {
            ThreadPool.QueueUserWorkItem(delegate 
            {
                ExitApp();
            });
        } 
        #endregion


        bool                _isMouseDraggingCamera;
        Vector3             _cameraStartDragCoord;
        XNA.Point           _mouseStartDragPoint;

        GameScene?          _currentScene;
        EntityControl?      _currentSelect;

        GameApplication?    _gameApp;
        Thread?             _gameAppThread;

        Camera?             _mainCamera;

        ManualResetEvent    _appInitalized_Reset = new(false);
        
        List<EntityControl> _eControls = [];
        
        int _thisThreadId;
    }

    public class EditorAsyncResult : IAsyncResult
    {
        public object? AsyncState => null;

        public WaitHandle AsyncWaitHandle => throw new NotImplementedException();

        public bool CompletedSynchronously => true;

        public bool IsCompleted => true;
    }
}
