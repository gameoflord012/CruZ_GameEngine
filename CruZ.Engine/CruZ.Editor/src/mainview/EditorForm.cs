﻿using CruZ.Editor.Controls;
using CruZ.Editor.Systems;
using CruZ.Resource;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CruZ.Editor
{
    public partial class EditorForm : Form
    {
        public PropertyGrid Inspector_PropertyGrid { get => inspector_PropertyGrid; }
        //TODO: public EditorApplication EditorApplication    { get => _editorApp; }

        private EditorForm()
        {
            KeyPreview = true;

            InitializeComponent();

            //TODO: _editorApp.OnSelectedEntityChanged += WorldViewControl_OnSelectedEntityChanged;
            //TODO: _editorApp.SceneLoadEvent += WorldViewControl_SceneLoadEvent;
            entities_ComboBox.SelectedIndexChanged += Entities_ComboBox_SelectedIndexChanged;
            entities_ComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            _editorApp = new();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z))
            {
                UndoService.Undo();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Shift | Keys.Z))
            {
                UndoService.Redo();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Entities_ComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            //TODO: _editorApp.SelectEntity((TransformEntity)entities_ComboBox.SelectedItem);
        }

        private void WorldViewControl_SceneLoadEvent(object? sender, GameScene e)
        {
            entities_ComboBox.Items.Clear();

            for (int i = 0; i < e.Entities.Count(); i++)
            {
                entities_ComboBox.Items.Add(e.Entities[i]);
            }
        }

        private void WorldViewControl_OnSelectedEntityChanged(object? sender, Components.TransformEntity e)
        {
            Inspector.Instance.DisplayEntity(e);
            Trace.Assert(entities_ComboBox.Items.Contains(e));
            entities_ComboBox.SelectedItem = e;
        }

        private void OpenScene_Clicked(object sender, EventArgs e)
        {
            var files = DialogHelper.SelectSceneFile(false);
            if (files.Count() == 0) return;

            string sceneFile = files[0];

            _editorApp.LoadSceneFromFile(sceneFile);
        }

        private void SaveScene_Clicked(object sender, EventArgs args)
        {
            //TODO: if (_editorApp.CurrentGameScene == null) return;

            try
            {
                //TODO: ResourceManager.SaveResource(_editorApp.CurrentGameScene);
            }
            catch (System.Exception e)
            {
                ShowExceptionDialog(e);
            }

        }

        private void File_Menu_Clicked(object sender, EventArgs e)
        {

        }

        private void SaveAsScene_Clicked(object sender, EventArgs e)
        {
            var savePath = DialogHelper.GetSaveScenePath();
            if (savePath == null) return;

            ResourceManager.CreateResource(
                savePath,
                //TODO: _editorApp.CurrentGameScene, 
                true);
        }

        private void LoadScene_Clicked(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter Scene name to load", "Load scene Prompt");

            if (string.IsNullOrWhiteSpace(input)) return;

            try
            {
                //TODO: _editorApp.LoadScene(SceneManager.GetSceneAssets(input));
            }
            catch (SceneAssetNotFoundException ex)
            {
                ShowExceptionDialog(ex);
            }
        }

        private void ShowExceptionDialog(System.Exception ex)
        {
            MessageBox.Show(
                $"{ex}\nInner Error: {ex.InnerException}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CacheService.CallWriteCaches();
            _editorApp.ExitApp();
        }

        EditorApplication _editorApp;
    }

    public partial class EditorForm
    {
        static EditorForm? _instance;
        public static EditorForm Instance => _instance ??= new EditorForm();
    }
}
