﻿using AsepriteDotNet.Aseprite;

using CruZ.GameEngine.Serialization;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended.BitmapFonts;
using MonoGame.Framework.Content.Pipeline.Builder;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

using PathHelper = CruZ.GameEngine.Utility.PathHelper;

namespace CruZ.GameEngine.Resource
{
    /// <summary>
    /// For external content pipeline to work, the dll must be presented in domain base dir
    /// </summary>
    public class ResourceManager
    {
        public string ResourceRoot
        {
            get => _resourceRoot;
            private set
            {
                _resourceRoot = Path.GetFullPath(value);
            }
        }

        private ResourceManager(string resourceRoot)
        {
            _serializer = new Serializer();
            ResourceRoot = resourceRoot;

            Directory.CreateDirectory(Path.GetDirectoryName(ResourceRoot) ?? throw new ArgumentException("resourceRoot"));
            Directory.CreateDirectory(Path.GetDirectoryName(ContentOutputDir) ?? throw new ArgumentException("resourceRoot"));

            //_serializer.Converters.Add(new TextureAtlasJsonConverter(this));
            _serializer.Converters.Add(new TransformEntityJsonConverter());
            _serializer.Converters.Add(new ComponentJsonConverter());
            _serializer.Converters.Add(new Vector4JsonConverter());
            _serializer.Converters.Add(new Vector2JsonConverter());
            _serializer.Options.MakeReadOnly();

            _pipelineManager = new(ResourceRoot, ContentOutputDir, ContentOutputDir);
            _pipelineManager.Platform = TargetPlatform.Windows;
            AddPipelineAssemblies();

            InitResourceDir();
        }

        private void AddPipelineAssemblies()
        {
            _pipelineManager.AddAssembly(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MonoGame.Aseprite.Content.Pipeline.dll"));
            _pipelineManager.AddAssembly(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MonoGame.Extended.Content.Pipeline.dll"));
            _pipelineManager.AddAssembly(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MonoGame.Extended.dll"));
        }

        private void InitResourceDir()
        {
            //foreach (var filePath in DirectoryHelper.EnumerateFiles(ResourceRoot, [REF_DIR_NAME, ".content"]))
            //{
            //    var extension = Path.GetExtension(filePath);

            //    switch (extension)
            //    {
            //        // remove excess .import files
            //        case ".import":
            //            var resourceFile = filePath.Substring(0, filePath.LastIndexOf(extension));
            //            if (!File.Exists(resourceFile)) File.Delete(filePath);
            //            break;

            //        default:
            //            // initialize resource if filePath is a resource
            //            if (ResourceSupportedExtensions.Contains(Path.GetExtension(filePath).ToLower()))
            //            {
            //                ImportResourcePath(filePath);
            //            }
            //            break;
            //    }
            //}

            // enumerates .xnb and .mgcontent files
            // TEST:
            foreach (var filePath in Directory.EnumerateFiles(ContentOutputDir, "*.*", SearchOption.AllDirectories).
                Where(e =>
                    Path.GetFileName(e) != ".mgcontent" &&
                    (e.EndsWith(".xnb") || e.EndsWith(".mgcontent"))))
            {
                // delete if file name is not guid or resource guid doesn't exists
                var relative = Path.GetRelativePath(ContentOutputDir, filePath);
                var resourceFile = Path.Combine(ResourceRoot, Path.ChangeExtension(filePath, null));
                
                if(!File.Exists(resourceFile))
                {
                    File.Delete(filePath);
                }
            }
        }

        private static readonly string[] ContentSupportedExtensions =
        [
            ".jpg", ".png", // texture file
            ".fx", 
            ".aseprite", 
            ".fnt"
        ];

        private static readonly Type[] ContentSupportedTypes =
        [
            typeof(Texture2D),
            typeof(AsepriteFile),
            typeof(Effect),
            typeof(BitmapFont)
        ];

        private static readonly string[] ResourceSupportedExtensions =
        [
            .. ContentSupportedExtensions,
            .. new string[]
            {
                ".sf", ".scene",
            },
        ];

        //public void Create(string resourcePath, object resObj)
        //{
        //    resourcePath = GetFormattedResourcePath(resourcePath);
        //    _serializer.SerializeToFile(resObj, Path.Combine(ResourceRoot, resourcePath));
        //    InitResourceInstance(resObj, resourcePath, true);
        //}

        //public bool TrySave(IResource resource)
        //{
        //    if (resource.Info == null) return false;
        //    Create(resource.Info.ResourceName, resource);
        //    return true;
        //}

        public T Load<T>(string resourcePath)
        {
            return (T)Load(resourcePath, typeof(T));
        }

        //public T Load<T>(ResourceInfo resourceInfo)
        //{
        //    return GetManagerFromReferencePath(resourceInfo.ReferencePath).Load<T>(resourceInfo.Guid);
        //}

        /// <summary>
        /// Get <see cref="ResourceInfo"/> with given imported resource path
        /// </summary>
        //public ResourceInfo RetriveResourceInfo(string resourcePath)
        //{
        //    resourcePath = GetFormattedResourcePath(resourcePath);
        //    var relative = Path.GetRelativePath(ResourceRoot, resourcePath).Replace("/", "\\").AsSpan();
        //    StringBuilder sb = new();

        //    const string REF_DIR = $"{REF_DIR_NAME}\\";
        //    while (relative.StartsWith(REF_DIR))
        //    {
        //        relative = relative.Slice(REF_DIR.Length);
        //        sb.Append(REF_DIR);

        //        int slashIndex = relative.IndexOf("\\");

        //        sb.Append(relative.Slice(0, slashIndex + 1));
        //        relative = relative.Slice(0, slashIndex + 1);
        //    }

        //    try
        //    {
        //        var refPath = sb.ToString();
        //        ResourceManager manager = GetManagerFromReferencePath(refPath);

        //        return ResourceInfo.Create(manager._guidManager.GetGuid(resourcePath), resourcePath, refPath);

        //    }
        //    catch (InvalidGuidException)
        //    {
        //        throw new ArgumentException($"Resource \"{resourcePath}\" maybe unimported");
        //    }
        //    catch (InvalidGuidValueException)
        //    {
        //        throw new ArgumentException($"Resource \"{resourcePath}\" maybe unimported");
        //    }
        //}

        //private ResourceManager GetManagerFromReferencePath(string referencePath)
        //{
        //    return
        //        string.IsNullOrEmpty(referencePath) ? this :
        //        From(Path.Combine(ResourceRoot, referencePath));
        //}

        //string IGuidValueProcessor<string>.GetProcessedGuidValue(string value)
        //{
        //    return GetFormattedResourcePath(value).ToLower();
        //}

        /// <summary>
        /// Load resource with relative or full path, the resource fileName should within the .resourceInstance folder
        /// </summary>
        /// <returns></returns>
        private object Load(string resourcePath, Type ty)
        {
            resourcePath = GetFormattedResourcePath(resourcePath);

            //ResourceManager manager = GetManagerFromReferencePath(refPath);

            object? returnResource;

            if (ContentSupportedTypes.Contains(ty))
            {
                returnResource = LoadContentNonGeneric(resourcePath, ty);
            }
            else
            {
                returnResource = LoadResource(resourcePath, ty);
            }

            //InitResourceInstance(returnResource, resourceInfo);
            return returnResource;
        }

        private object LoadResource(string resourcePath, Type ty)
        {
            throw new NotImplementedException();

            resourcePath = GetFormattedResourcePath(resourcePath);
            var fullResourcePath = Path.Combine(ResourceRoot, resourcePath);

            object? resObj;
            try
            {
                resObj = _serializer.DeserializeFromFile(fullResourcePath, ty);
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(string.Format("Can't find resource file {0}", fullResourcePath));
            }
            catch (JsonException)
            {
                throw new LoadResourceFailedException($"Can't load resource \"{fullResourcePath}\" due to invalid resource formatting or not available in content");
            }

            return resObj;
        }

        private T LoadContent<T>(string resourcePath)
        {
            resourcePath = GetFormattedResourcePath(resourcePath);
            var content = GameApplication.GetContent();

            // Setup content context
            content.RootDirectory = ContentOutputDir;
            content.AssetNameResolver = ResolveAssetName;
            
            var loaded = content.Load<T>(resourcePath);
            
            // Return content context to default
            content.AssetNameResolver = null;
            content.RootDirectory = ".";
            
            return loaded;
        }

        private string ResolveAssetName(string assetName, Type assetType, ContentManager content)
        {
            var resourcePath = GetFormattedResourcePath(assetName);

            BuildContent(assetType, resourcePath);

            return GetContentPathFromResourcePath(resourcePath);
        }

        private void BuildContent(Type ty, string resourcePath)
        {
            resourcePath = GetFormattedResourcePath(resourcePath);
            var contentPath = GetContentPathFromResourcePath(resourcePath) + ".xnb";
            _pipelineManager.BuildContent(resourcePath, contentPath, processorParameters: GetProcessorParam(ty));
            _pipelineManager.ContentStats.Write(ContentOutputDir);
        }

        private string GetContentPathFromResourcePath(string resourcePath)
        {
            resourcePath = GetFormattedResourcePath(resourcePath);
            var relative = Path.GetRelativePath(ResourceRoot, resourcePath);
            return Path.Combine(ContentOutputDir, relative);
        }

        private object LoadContentNonGeneric(string resourcePath, Type ty)
        {
            try
            {
                return typeof(ResourceManager).
                    GetMethod(nameof(LoadContent), BindingFlags.NonPublic | BindingFlags.Instance)!.
                    MakeGenericMethod(ty).
                    Invoke(this, [resourcePath])!;
            }
            catch (Exception e)
            {
                throw new ContentLoadException($"Cannot load content {resourcePath}", e);
            }
        }

        //private void InitResourceInstance(object resourceInstance, string resourcePath, bool autoImportResourcePath = false)
        //{
        //    if (autoImportResourcePath) ImportResourcePath(resourcePath);
        //    InitResourceInstance(resourceInstance, RetriveResourceInfo(resourcePath));
        //}

        //private void InitResourceInstance(object resourceInstance, ResourceInfo resourceInfo)
        //{
        //    if (resourceInstance is IResource resource)
        //        resource.Info = resourceInfo;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourcePath">a subpath or full subpath of this resource dir</param>
        /// <returns>formatted</returns>
        private string GetFormattedResourcePath(string resourcePath)
        {
            resourcePath = Path.Combine(ResourceRoot, resourcePath);
            if (!PathHelper.IsSubpath(ResourceRoot, resourcePath))
                throw new ArgumentException($"Resource Path \"{resourcePath}\" must be a subpath of resource root \"{ResourceRoot}\"");
            return Path.GetFullPath(resourcePath);
        }

        /// <summary>
        /// Read Guid or auto-generated new Guid and .import files
        /// </summary>
        /// <param name="resourcePath"></param>
        //private Guid ImportResourcePath(string resourcePath)
        //{
        //    resourcePath = GetFormattedResourcePath(resourcePath);
        //    Guid guid;

        //    if (TryReadGuidFromImportFile(resourcePath, out guid)) // if .import exists
        //    {

        //    }
        //    else // if don't
        //    {
        //        // Write new guid to .import
        //        guid = _guidManager.GenerateUniqueGuid();
        //        using (var writer = new StreamWriter(File.Create(resourcePath + ".import")))
        //        {
        //            writer.WriteLine(guid);
        //            writer.Flush();
        //        }
        //    }

        //    _guidManager.ConsumeGuid(guid, resourcePath);
        //    return guid;
        //}

        //public ResourceManager CreateResourceReference(string referencePath)
        //{
        //    DirectoryInfo referenceDir = GetReferenceDir();
        //    referenceDir.MoveTo(referencePath);
        //    if (referenceDir.Exists) throw new InvalidOperationException("Reference already exists");
        //    referenceDir.Create();
        //    return From(referenceDir.FullName);
        //}

        public void CopyResourceData(ResourceManager resourceRef, string relativeDestination)
        {
            if(!PathHelper.IsRelativeASubpath(relativeDestination)) throw new ArgumentException(relativeDestination);

            PathHelper.UpdateFolder(
                resourceRef.ResourceRoot,
                Path.Combine(ResourceRoot, relativeDestination),
                "*", true, true);
        }

        private OpaqueDataDictionary GetProcessorParam(Type ty)
        {
            if (!_processorParams.ContainsKey(ty))
                _processorParams[ty] = [];
            return _processorParams[ty];
        }

        Dictionary<Type, OpaqueDataDictionary> _processorParams = [];

        /// <summary>
        /// Get .import file from normal file
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        private static bool TryReadGuidFromImportFile(string filePath, out Guid guid)
        {
            var dotImport = filePath + ".import";
            guid = default;
            if (!File.Exists(dotImport)) return false;
            return Guid.TryParse(File.ReadLines(dotImport).First(), out guid);
        }

        string ContentOutputDir => $"{_resourceRoot}\\.content\\";
        string _resourceRoot = "res";

        Serializer _serializer;
        PipelineManager _pipelineManager;

        public static ResourceManager From(string resourceDir)
        {
            resourceDir = Path.GetFullPath(resourceDir);

            if (!_managers.ContainsKey(resourceDir))
                _managers[resourceDir] = new ResourceManager(resourceDir);

            return _managers[resourceDir];
        }
        static Dictionary<string, ResourceManager> _managers = [];
    }
}