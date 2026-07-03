using System;
using System.Drawing;
using System.IO;

namespace Makarov.Framework.Core.Managers
{

    public sealed class GraphicsManager
    {
        #region Exceptions

        public class GraphicsManagerException : MakarovFrameworkException
        {

            public GraphicsManagerException(string message)
                : base(message)
            { }
        }

        public sealed class ImageNotFoundException : GraphicsManagerException
        {

            public ImageNotFoundException(string name)
                : base($"Image '{name}' not found.")
            { }
        }
        #endregion

        #region Constructors

        public GraphicsManager(string directory)
        {
            if (!System.IO.Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Directory '{directory}' not found.");

            Directory = directory;
        }
        #endregion

        #region Properties

        public string Directory
        {
            get;
        }

        public Bitmap this[string name]
        {
            get
            {
                Requested++;

                var arr = new[] { ".bmp", ".png", ".jpg" };
                Bitmap result = null;

                try
                {
                    if (_images.ContainsKey(name))
                    {
                        if (_images[name].IsAlive)
                        {
                            result = (Bitmap) ((Bitmap) _images[name].Target).Clone();
                            Resurrected++;
                        }
                        else
                        {
                            ResurrectionFailures++;
                        }
                    }
                }
                catch
                {
                    result = null;
                    ResurrectionFailures++;
                }

                if (result is null)
                    foreach (string ext in arr)
                    {
                        string filename = Path.Combine(Directory, name + ext);

                        if (File.Exists(filename))
                        {
                            var bmp = (Bitmap)Image.FromFile(filename);
                            Loaded++;

                            if (_images.ContainsKey(name))
                                _images.Remove(name);

                            _images.Add(name, new WeakReference(bmp));
                            result = (Bitmap)bmp.Clone();
                        }
                    }

                if (result == null)
                    throw new ImageNotFoundException(name);

                return result;
            }
        }

        public int Resurrected
        {
            get; private set;
        }

        public int ResurrectionFailures
        {
            get; private set;
        }

        public int Requested
        {
            get; private set;
        }

        public int Loaded
        {
            get; private set;
        }
        #endregion

        #region Private members

        private readonly StringDictionary<WeakReference> _images = new StringDictionary<WeakReference>();
        #endregion

        #region Public methods

        public void Clear()
        {
            _images.Clear();
        }
        #endregion
    }
}
