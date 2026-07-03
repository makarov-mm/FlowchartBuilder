using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace Makarov.Framework.Core.Managers
{

    public sealed class WindowsManager : IDisposable
    {
        #region Exceptions

        public abstract class WindowsManagerException : MakarovFrameworkException
        {
            protected WindowsManagerException(string message)
                : base(message ?? string.Empty)
            { }
        }

        public sealed class WindowsManagerAlreadyDisposedException : WindowsManagerException
        {
            public WindowsManagerAlreadyDisposedException()
                : base(@"Windows manager already disposed exception.")
            { }
        }

        public sealed class WindowTypeNotFoundException : WindowsManagerException
        {

            public WindowTypeNotFoundException(Type type)
                : base($@"Window type '{type.Name}' not found.")
            { }

            public WindowTypeNotFoundException(Type type, string details)
                : base($@"Window type '{type.Name}' not found. {details}")
            { }
        }
        #endregion

        #region Private members

        private bool _disposed;

        private List<Type> _formTypes;

        private List<Form> _windows;

        private string _scannedAssembly;
        #endregion

        #region Constructors

        public WindowsManager(Assembly assembly)
        {
            _formTypes = new List<Type>();
            _windows = new List<Form>();
            _scannedAssembly = assembly.GetName().Name;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = Array.FindAll(ex.Types, t => t != null);
            }

            foreach (Type type in types)
                if (type.IsClass && type.IsSubclassOf(typeof(Form)))
                    _formTypes.Add(type);
        }
        #endregion

        #region Properties

        public IEnumerable<Type> Forms
        {
            get
            {
                if (_disposed)
                    throw new WindowsManagerAlreadyDisposedException();

                return _formTypes;
            }
        }

        public IEnumerable<Form> Windows
        {
            get
            {
                if (_disposed)
                    throw new WindowsManagerAlreadyDisposedException();

                return _windows;
            }
        }
        #endregion

        #region Public methods

        public T GetWindow<T>() where T: Form
        {
            if (_disposed)
                throw new WindowsManagerAlreadyDisposedException();

            foreach (Form form in _windows)
                if (form is T)
                    return (T) form;

            foreach (Type type in _formTypes)
                if (type == typeof(T))
                {
                    _formTypes.Remove(type);
                    var frm = (T) Activator.CreateInstance(type);
                    _windows.Add(frm);
                    return frm;
                }

            var known = _formTypes.Count == 0
                ? "(none)"
                : string.Join(", ", _formTypes.ConvertAll(t => t.Name));
            throw new WindowTypeNotFoundException(
                typeof(T),
                $"Scanned assembly: '{_scannedAssembly}'. Registered form types: {known}.");
        }

        public void Dispose()
        {
            if (_disposed)
                throw new WindowsManagerAlreadyDisposedException();

            foreach (Form wnd in _windows)
                wnd.Dispose();

            _windows.Clear();
            _windows = null;

            _formTypes.Clear();
            _formTypes = null;

            _disposed = true;
        }
        #endregion
    }
}
