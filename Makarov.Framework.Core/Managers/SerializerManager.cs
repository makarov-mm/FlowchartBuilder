using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Makarov.Framework.Core.Managers
{

    public sealed class SerializerManager
    {
        #region Exceptions

        public abstract class SerializerManagerException : MakarovFrameworkException
        {

            protected SerializerManagerException(string message)
                : base(message)
            { }
        }

        public sealed class SerializerAlreadyExistsException : SerializerManagerException
        {

            public SerializerAlreadyExistsException(string type)
                : base($@"Serializer for type '{type ?? string.Empty}' already exists.")
            { }

            public SerializerAlreadyExistsException(Type type)
                : this(type.Name)
            { }
        }

        public sealed class SerializerNotExistsException : SerializerManagerException
        {

            public SerializerNotExistsException(string type)
                : base($@"Serializer for type '{type ?? string.Empty}' not exists.")
            { }

            public SerializerNotExistsException(Type type)
                : this(type.Name)
            { }
        }
        #endregion

        #region Private members

        private readonly List<AbstractSerializer> _serializers = new List<AbstractSerializer>();
        #endregion

        #region Constructors

        public SerializerManager(IEnumerable<Assembly> assemblies)
        {
            AddFromAssemblies(assemblies);
        }

        public SerializerManager(Assembly assembly)
        {
            AddFromAssembly(assembly);
        }
        #endregion

        #region Public methods

        public string Serialize(object value)
        {
            return Serialize(value.GetType(), value);
        }

        public string Serialize(Type type, object value)
        {
            foreach (AbstractSerializer serializer in _serializers)
                if (serializer.SerializerType == type)
                    return serializer.Serialize(value);

            throw new SerializerNotExistsException(type);
        }

        public string Serialize<T>(T value)
        {
            Type targetType = typeof(T);

            foreach (AbstractSerializer serializer in _serializers)
                if (serializer.SerializerType == targetType)
                    return serializer.Serialize(value);

            throw new SerializerNotExistsException(targetType);
        }

        public T Deserialize<T>(string str)
        {
            Type targetType = typeof(T);

            foreach (AbstractSerializer serializer in _serializers)
                if (serializer.SerializerType == targetType)
                    return (T)serializer.Deserialize(str);

            throw new SerializerNotExistsException(targetType);
        }

        public object Deserialize(Type type, string str)
        {
            foreach (AbstractSerializer serializer in _serializers)
                if (serializer.SerializerType == type)
                    return serializer.Deserialize(str);

            throw new SerializerNotExistsException(type);
        }

        public void AddSerializer(AbstractSerializer serializer, bool throwOnExists = true)
        {
            if (!IsExists(serializer.SerializerType) || !throwOnExists)
                _serializers.Add(serializer);
            else
                throw new SerializerAlreadyExistsException(serializer.SerializerType);
        }

        public void AddFromAssembly(Assembly assembly)
        {
            AddFromAssemblies(new[] { assembly });
        }

        public void AddFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                    if (type.IsClass && type.IsSubclassOf(typeof (AbstractSerializer)))
                        AddSerializer((AbstractSerializer) Activator.CreateInstance(type));
            }
        }

        public bool IsExists<T>()
        {
            return IsExists(typeof (T));
        }

        public bool IsExists(Type type)
        {
            return _serializers.Any(serializer => serializer.SerializerType == type);
        }

        public void Clear()
        {
            _serializers.Clear();
        }
        #endregion
    }
}
