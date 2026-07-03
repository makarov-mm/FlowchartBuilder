using System;

namespace Makarov.FlowchartBuilder
{

    public sealed class DonNotRedraw : IDisposable
    {

        public DonNotRedraw()
        {

            Core.Instance.NeedRedraw = false;

            _disposed = false;
        }

        public void Dispose()
        {

            if (_disposed) return;

            Core.Instance.NeedRedraw = true;

            _disposed = true;
        }

        private bool _disposed;
    }
}
