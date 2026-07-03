using System;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Makarov.Framework.Graphics
{

    public sealed class TempSmoothingMode : IDisposable
    {
        private readonly SmoothingMode _smootingMode;
        private System.Drawing.Graphics _gfx;

        public TempSmoothingMode(System.Drawing.Graphics gfx, SmoothingMode smoothingMode)
        {
            if (gfx.SmoothingMode == smoothingMode)
                return;

            _smootingMode = gfx.SmoothingMode;
            gfx.SmoothingMode = smoothingMode;
            _gfx = gfx;
        }

        public void Dispose()
        {
            if (_gfx == null)
                return;

            _gfx.SmoothingMode = _smootingMode;
            _gfx = null;
        }
    }

    public sealed class TempTextRenderingHint : IDisposable
    {
        private readonly TextRenderingHint _textRenderingHint;
        private System.Drawing.Graphics _gfx;

        public TempTextRenderingHint(System.Drawing.Graphics gfx, TextRenderingHint textRenderingHint)
        {
            if (gfx.TextRenderingHint == textRenderingHint)
                return;

            _textRenderingHint = gfx.TextRenderingHint;
            gfx.TextRenderingHint = textRenderingHint;
            _gfx = gfx;
        }

        public void Dispose()
        {
            if (_gfx == null)
                return;

            _gfx.TextRenderingHint = _textRenderingHint;
            _gfx = null;
        }
    }
}
