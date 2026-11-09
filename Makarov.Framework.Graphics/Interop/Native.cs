using System.Runtime.InteropServices;

namespace Makarov.Framework.Graphics.Interop;

/// <summary>Entry points, identifiers, constants and structures of Direct2D, DirectWrite and WIC.</summary>
internal static partial class Native
{
    public const int D2DERR_RECREATE_TARGET = unchecked((int)0x8899000C);

    public const int D2D1_FACTORY_TYPE_MULTI_THREADED = 1;
    public const int DWRITE_FACTORY_TYPE_SHARED = 0;
    public const uint CLSCTX_INPROC_SERVER = 1;

    public const int D2D1_RENDER_TARGET_TYPE_DEFAULT = 0;
    public const uint DXGI_FORMAT_B8G8R8A8_UNORM = 87;
    public const int D2D1_ALPHA_MODE_PREMULTIPLIED = 1;
    public const int D2D1_ALPHA_MODE_IGNORE = 3;
    public const int D2D1_PRESENT_OPTIONS_IMMEDIATELY = 2;

    public const int D2D1_ANTIALIAS_MODE_PER_PRIMITIVE = 0;
    public const int D2D1_ANTIALIAS_MODE_ALIASED = 1;
    public const int D2D1_TEXT_ANTIALIAS_MODE_CLEARTYPE = 1;
    public const int D2D1_TEXT_ANTIALIAS_MODE_GRAYSCALE = 2;
    public const int D2D1_TEXT_ANTIALIAS_MODE_ALIASED = 3;

    public const int D2D1_DASH_STYLE_SOLID = 0;
    public const int D2D1_DASH_STYLE_CUSTOM = 5;
    public const int D2D1_GAMMA_2_2 = 0;
    public const int D2D1_EXTEND_MODE_CLAMP = 0;
    public const int D2D1_FILL_MODE_ALTERNATE = 0;
    public const int D2D1_FIGURE_BEGIN_FILLED = 0;
    public const int D2D1_FIGURE_BEGIN_HOLLOW = 1;
    public const int D2D1_FIGURE_END_OPEN = 0;
    public const int D2D1_FIGURE_END_CLOSED = 1;
    public const int D2D1_SWEEP_DIRECTION_COUNTER_CLOCKWISE = 0;
    public const int D2D1_SWEEP_DIRECTION_CLOCKWISE = 1;
    public const int D2D1_ARC_SIZE_SMALL = 0;
    public const int D2D1_ARC_SIZE_LARGE = 1;
    public const int D2D1_DRAW_TEXT_OPTIONS_CLIP = 2;

    public const int DWRITE_FONT_WEIGHT_NORMAL = 400;
    public const int DWRITE_FONT_WEIGHT_BOLD = 700;
    public const int DWRITE_FONT_STYLE_NORMAL = 0;
    public const int DWRITE_FONT_STYLE_ITALIC = 2;
    public const int DWRITE_FONT_STRETCH_NORMAL = 5;
    public const int DWRITE_WORD_WRAPPING_WRAP = 0;
    public const int DWRITE_WORD_WRAPPING_NO_WRAP = 1;

    public const int WICBitmapCacheOnLoad = 2;

    public static readonly Guid IID_ID2D1Factory = new("06152247-6f50-465a-9245-118bfd3b6007");
    public static readonly Guid IID_IDWriteFactory = new("b859ee5a-d838-4b5b-a2e8-1adc7d93db48");
    public static readonly Guid CLSID_WICImagingFactory = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
    public static readonly Guid IID_IWICImagingFactory = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
    public static readonly Guid GUID_WICPixelFormat32bppPBGRA = new("6fddc324-4e03-4bfe-b185-3d77768dc910");

    [LibraryImport("d2d1.dll")]
    public static partial int D2D1CreateFactory(int factoryType, in Guid riid, nint factoryOptions, out nint factory);

    [LibraryImport("dwrite.dll")]
    public static partial int DWriteCreateFactory(int factoryType, in Guid iid, out nint factory);

    [LibraryImport("ole32.dll")]
    public static partial int CoCreateInstance(in Guid clsid, nint outer, uint context, in Guid iid, out nint instance);

    /// <summary>D2D1_POINT_2F (also used for D2D1_SIZE_F). A plain struct: it is passed by value to native code.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Point2F(float x, float y)
    {
        public float X = x;
        public float Y = y;
    }

    /// <summary>D2D1_COLOR_F: straight (not premultiplied) alpha.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ColorF(float r, float g, float b, float a)
    {
        public float R = r;
        public float G = g;
        public float B = b;
        public float A = a;
    }

    /// <summary>D2D1_RECT_F.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RectF(float left, float top, float right, float bottom)
    {
        public float Left = left;
        public float Top = top;
        public float Right = right;
        public float Bottom = bottom;
    }

    /// <summary>D2D1_PIXEL_FORMAT.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelFormat
    {
        public uint Format;
        public int AlphaMode;
    }

    /// <summary>D2D1_RENDER_TARGET_PROPERTIES.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RenderTargetProperties
    {
        public int Type;
        public PixelFormat PixelFormat;
        public float DpiX;
        public float DpiY;
        public int Usage;
        public int MinLevel;
    }

    /// <summary>D2D1_HWND_RENDER_TARGET_PROPERTIES (pixelSize is a D2D1_SIZE_U).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct HwndRenderTargetProperties
    {
        public nint Hwnd;
        public uint PixelWidth;
        public uint PixelHeight;
        public int PresentOptions;
    }

    /// <summary>D2D1_SIZE_U.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SizeU(uint width, uint height)
    {
        public uint Width = width;
        public uint Height = height;
    }

    /// <summary>D2D1_STROKE_STYLE_PROPERTIES.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StrokeStyleProperties
    {
        public int StartCap;
        public int EndCap;
        public int DashCap;
        public int LineJoin;
        public float MiterLimit;
        public int DashStyle;
        public float DashOffset;
    }

    /// <summary>D2D1_ELLIPSE.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Ellipse(Point2F center, float radiusX, float radiusY)
    {
        public Point2F Center = center;
        public float RadiusX = radiusX;
        public float RadiusY = radiusY;
    }

    /// <summary>D2D1_GRADIENT_STOP.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GradientStop(float position, ColorF color)
    {
        public float Position = position;
        public ColorF Color = color;
    }

    /// <summary>D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LinearGradientBrushProperties(Point2F start, Point2F end)
    {
        public Point2F Start = start;
        public Point2F End = end;
    }

    /// <summary>D2D1_BEZIER_SEGMENT.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BezierSegment(Point2F point1, Point2F point2, Point2F point3)
    {
        public Point2F Point1 = point1;
        public Point2F Point2 = point2;
        public Point2F Point3 = point3;
    }

    /// <summary>D2D1_ARC_SEGMENT (size is a D2D1_SIZE_F).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ArcSegment
    {
        public Point2F Point;
        public Point2F Size;
        public float RotationAngle;
        public int SweepDirection;
        public int ArcSize;
    }

    /// <summary>DWRITE_TEXT_RANGE.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TextRange(uint startPosition, uint length)
    {
        public uint StartPosition = startPosition;
        public uint Length = length;
    }

    /// <summary>DWRITE_TEXT_METRICS.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TextMetrics
    {
        public float Left;
        public float Top;
        public float Width;
        public float WidthIncludingTrailingWhitespace;
        public float Height;
        public float LayoutWidth;
        public float LayoutHeight;
        public uint MaxBidiReorderingDepth;
        public uint LineCount;
    }
}
