using System;

namespace Makarov.FlowchartBuilder.API.Attributes
{

    [AttributeUsage(AttributeTargets.Class)]
    public abstract class SheetAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SheetWidthAttribute : SheetAttribute
    {

        public SheetWidthAttribute(float width)
        {
            WidthInMM = width;
        }

        public float WidthInMM { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SheetHeightAttribute : SheetAttribute
    {

        public SheetHeightAttribute(float width)
        {
            HeightInMM = width;
        }

        public float HeightInMM { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SheetNameAttribute : SheetAttribute
    {

        public SheetNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SheetFamilyAttribute : SheetAttribute
    {

        public SheetFamilyAttribute(string family)
        {
            Family = family;
        }

        public string Family { get; }
    }

}
