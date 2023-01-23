using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SwapWallet.Models;
using VicWeb.Misc;

namespace SwapWallet.Services
{
    public interface IFile
    {
        string FileName { get; }
    }

    public interface IFileService
    {
        string GetDirectory<T>() where T : IFile;
        string GetFilePath<T>(T item) where T : IFile;
        void Persist<T>(T item, JsonSerializerSettings settings = null) where T : IFile;
        IEnumerable<T> RetrieveAll<T>() where T : IFile;
        T Retrieve<T>(string fileName) where T : IFile;
        bool IsSuppressed { get; set; }
    }
    public class FileService : IFileService
    {
        public bool IsSuppressed { get; set; }
        private Dictionary<Type, string> _paths = new Dictionary<Type, string>();
        private Dictionary<Type, string> _ext = new Dictionary<Type, string>();

        private JsonSerializerSettings _defaultSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        private string _root;
        

        public FileService()
        {
            _root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            RegisterPath<User>("Users", "user");
            RegisterPath<MetaBase>("", "meta");
            RegisterPath<AssetHistory>("Balances", "bal");
        }

        private void RegisterPath<T>(string localPath, string ext) where T : IFile
        {
            var path = Path.Combine(_root, localPath);
            _paths.Add(typeof(T), path);
            _ext.Add(typeof(T), ext);
        }

        public string GetDirectory<T>() where T : IFile
        {
            Insist.IsTrue(_paths.ContainsKey(typeof(T)));

            return _paths[typeof(T)];
        }

        public string GetFilePath<T>(T item) where T : IFile
        {
            Insist.IsTrue(_paths.ContainsKey(typeof(T)));

            return Path.Combine(GetDirectory<T>(), item.FileName +"."+ _ext[typeof(T)]);
        }

        public void Persist<T>(T item, JsonSerializerSettings settings = null) where T : IFile
        {
            if (IsSuppressed)
                return;
            Insist.IsTrue(_paths.ContainsKey(typeof(T)));

            Directory.CreateDirectory(GetDirectory<T>());
            var json = JsonConvert.SerializeObject(item, settings ?? _defaultSettings);
            File.WriteAllText(GetFilePath(item), json);
        }

        public T Retrieve<T>(string name) where T : IFile
        {
            Insist.IsTrue(_paths.ContainsKey(typeof(T)));

            var dir = GetDirectory<T>();

            if (Directory.Exists(dir))
            {
                var path = Directory.EnumerateFiles(dir, "*." + _ext[typeof(T)])
                    .FirstOrDefault(file => file.Contains(name));

                if (path != null)
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            }

            return default(T);
        }

        public IEnumerable<T> RetrieveAll<T>() where T : IFile
        {
            Insist.IsTrue(_paths.ContainsKey(typeof(T)));

            var dir = GetDirectory<T>();

            if (Directory.Exists(dir))
                return Directory.EnumerateFiles(dir, "*." + _ext[typeof(T)])
                    .Select(item => JsonConvert.DeserializeObject<T>(File.ReadAllText(item)));

            return Enumerable.Empty<T>();
        }
    }
}
