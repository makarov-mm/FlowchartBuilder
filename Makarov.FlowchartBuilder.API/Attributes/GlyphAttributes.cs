using System;

namespace Makarov.FlowchartBuilder.API.Attributes
{

    [AttributeUsage(AttributeTargets.Class)]
    public abstract class GlyphAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlyphDefaultWidthAttribute : GlyphAttribute
    {

        public GlyphDefaultWidthAttribute(float width)
        {
            WidthInMM = width;
        }

        public float WidthInMM { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlyphDefaultHeightAttribute : GlyphAttribute
    {

        public GlyphDefaultHeightAttribute(float height)
        {
            HeightInMM = height;
        }

        public float HeightInMM { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlyphMinimalWidthAttribute : GlyphAttribute
    {

        public GlyphMinimalWidthAttribute(float width)
        {
            WidthInMM = width;
        }

        public float WidthInMM { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlyphMinimalHeightAttribute : GlyphAttribute
    {

        public GlyphMinimalHeightAttribute(float height)
        {
            HeightInMM = height;
        }

        public float HeightInMM { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlyphNameAttribute : GlyphAttribute
    {

        public GlyphNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlyphFamilyAttribute : GlyphAttribute
    {

        public GlyphFamilyAttribute(string family)
        {
            Family = family;
        }

        public string Family { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlyphGroupOrderAttribute : GlyphAttribute
    {
        private readonly int _order;

        public GlyphGroupOrderAttribute(int order)
        {
            _order = order;
        }

        public int Order
        {
            get { return _order; }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlyphOrderAttribute : GlyphAttribute
    {

        public GlyphOrderAttribute(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ActivePropertyAttribute : GlyphAttribute
    { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandGlyphAttribute : GlyphAttribute
    {

        public CommandGlyphAttribute(string cmdName)
        {
            CommandName = cmdName;
        }

        public string CommandName { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class VisibleGlyphAttribute : GlyphAttribute
    {

        public VisibleGlyphAttribute(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public bool IsVisible { get; }
    }

}
