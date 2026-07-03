using System;
using System.Drawing;
using System.Windows.Forms;
using Makarov.Framework.Core;

namespace Makarov.Framework.Graphics
{

    public class Renderer : IDisposable
    {
        #region Exceptions

        public abstract class RendererException : MakarovFrameworkException
        {

            protected RendererException(string message)
                : base(message ?? string.Empty)
            { }
        }

        public sealed class RendererAlreadyDisposedException : RendererException
        {
            public RendererAlreadyDisposedException()
                : base(@"Renderer already disposed.")
            { }
        }
        #endregion

        #region Private members

        private readonly Form _wnd;

        private bool _disposed;
        #endregion

        #region Constructors

        public Renderer(Form window)
        {
            _wnd = window;

            _wnd.Resize += (sender, e) => Redraw();
            _wnd.Paint += (sender, e) => Redraw();
        }
        #endregion

        #region Properties

        public Form Window
        {
            get
            {
                if (_disposed)
                    throw new RendererAlreadyDisposedException();

                return _wnd;
            }
        }

        public int Width
        {
            get
            {
                if (_disposed)
                    throw new RendererAlreadyDisposedException();

                return _wnd.ClientSize.Width;
            }
        }

        public int Height
        {
            get
            {
                if (_disposed)
                    throw new RendererAlreadyDisposedException();

                return _wnd.ClientSize.Height;
            }
        }
        #endregion

        #region Public methods
        public void Dispose()
        {
            if (_disposed)
                throw new RendererAlreadyDisposedException();

            _disposed = true;
        }

        public virtual void Redraw()
        {
            using (System.Drawing.Graphics gfx = _wnd.CreateGraphics())
                gfx.Clear(Color.Blue);
        }
        #endregion
    }
}
