using System;
using System.Drawing;
using System.Drawing.Imaging;
using Makarov.Framework.Core;

namespace Makarov.Framework.Graphics
{

    public unsafe sealed class RawBitmap : IDisposable
    {
        #region Pixel data

        public struct Pixel
        {

            public byte Blue;

            public byte Green;

            public byte Red;

            public byte Alpha;

            public Pixel(byte red, byte green, byte blue, byte alpha)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alpha;
            }

            public Pixel(byte red, byte green, byte blue)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = 255;
            }
        }
        #endregion

        #region Exceptions

        public class RawBitmapException : MakarovFrameworkException
        {

            public RawBitmapException(string message)
                : base(message)
            { }
        }

        public sealed class InvalidPixelFormatException : RawBitmapException
        {

            public InvalidPixelFormatException()
                : base("Invalid pixel format.")
            { }
        }
        #endregion

        #region Private members
        private readonly BitmapData _data;
        readonly byte* _pBase = null;
        #endregion

        #region Constructors

        public RawBitmap(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format32bppArgb)
                throw new InvalidPixelFormatException();

            InnerBitmap = bmp;
            Width = bmp.Width;
            Height = bmp.Height;

            _data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            _pBase = (byte*)_data.Scan0;
        }

        public void Dispose()
        {
            InnerBitmap.UnlockBits(_data);
            InnerBitmap = null;
        }
        #endregion

        #region Properties

        public int Width
        {
            get; private set;
        }

        public int Height
        {
            get; private set;
        }

        public Bitmap InnerBitmap
        {
            get; private set;
        }
        #endregion

        #region Random access

        public Color this[int x, int y]
        {
            get
            {
                var pixel = (Pixel*)(_pBase + y * _data.Stride + x * 4);

                return Color.FromArgb(
                    (*pixel).Alpha,
                    (*pixel).Red,
                    (*pixel).Green,
                    (*pixel).Blue);
            }

            set
            {
                var pixel = (Pixel*)(_pBase + y * _data.Stride + x * 4);
                *pixel = new Pixel(value.R, value.G, value.B, value.A);
            }
        }
        #endregion
    }
}
