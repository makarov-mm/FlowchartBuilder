using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace Makarov.Framework.Core
{

    public sealed class Loader
    {
        #region Constructors

        public Loader(string directory)
        {

            try
            {
                string[] files = Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories);

                _assemblies = new Dictionary<string, string>(files.Length);
                foreach (string file in files)
                {

                    string name = Path.GetFileNameWithoutExtension(file) ?? string.Empty;

                    if (_assemblies.ContainsKey(name))
                    {
                        int i = -1;
                        string uniqName;
                        do
                        {
                            uniqName = name + "_" + (++i);
                        } while (_assemblies.ContainsKey(uniqName));

                        name = uniqName;
                    }

                    _assemblies.Add(name, file);
                }
            }
            catch
            {
            }
        }
        #endregion

        #region Private members

        private Dictionary<string, string> _assemblies;

        private Dictionary<string, Assembly> _loadedAssemblies;
        #endregion

        #region Public methods

        public bool IsAssemblyExists(string name)
        {
            return _assemblies.ContainsKey(name ?? string.Empty);
        }

        public bool IsAssemblyLoaded(string name)
        {
            return _loadedAssemblies.ContainsKey(name ?? string.Empty);
        }

        public Assembly GetAssembly(string name)
        {
            if (_loadedAssemblies.ContainsKey(name))
                return _loadedAssemblies[name];

            if (_assemblies.ContainsKey(name))
            {
                if (_loadedAssemblies is null)
                    _loadedAssemblies = new Dictionary<string, Assembly>(_assemblies.Count);

                Assembly asm = Assembly.LoadFile(name);
                _loadedAssemblies.Add(name, asm);
                return asm;
            }

            return null;
        }

        public IEnumerable<string> Assemblies
        {
            get { return _assemblies.Select(kvp => kvp.Key); }
        }

        public IDictionary<string, Assembly> LoadedAssemblies
        {
            get
            {
                if (_assemblies is null)
                {
                    _assemblies = new Dictionary<string, string>();
                }

                if (_loadedAssemblies is null)
                {

                    _loadedAssemblies = new Dictionary<string, Assembly>(_assemblies.Count);
                }

                foreach (KeyValuePair<string, string> kvp in _assemblies)
                    if (!_loadedAssemblies.ContainsKey(kvp.Key))
                        _loadedAssemblies.Add(kvp.Key, Assembly.LoadFile(kvp.Value));

                return _loadedAssemblies;
            }
        }

        public IEnumerable<Type> GetTypes<T>()
        {
            return from kvp in LoadedAssemblies
                   from type in kvp.Value.GetTypes()
                   where type is T || type.IsSubclassOf(typeof(T))
                   select type;
        }
        #endregion
    }
}
