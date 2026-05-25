// <copyright file="SheetAttributes.cs" company="Michael Makarov">
// Copyright Michael Makarov (c) 2011 All Right Reserved
// </copyright>
// <author>Michael Makarov</author>
// <email>m.m.makarov@gmail.com</email>
// <date>2011-II-18</date>
// <summary>Атрибуты листа.</summary>

using System;

namespace Makarov.FlowchartBuilder.API.Attributes
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Аттрибут листа.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public abstract class SheetAttribute : Attribute
    { }

    /// <summary>
    /// Ширина листа.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SheetWidthAttribute : SheetAttribute
    {
        /// <param name="width">Ширина в миллиметрах.</param>
        public SheetWidthAttribute(float width)
        {
            WidthInMM = width;
        }

        /// <summary>
        /// Ширина в миллиметрах.
        /// </summary>
        public float WidthInMM { get; }
    }

    /// <summary>
    /// Высота листа.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SheetHeightAttribute : SheetAttribute
    {
        /// <param name="width">Ширина в миллиметрах.</param>
        public SheetHeightAttribute(float width)
        {
            HeightInMM = width;
        }

        /// <summary>
        /// Ширина в миллиметрах.
        /// </summary>
        public float HeightInMM { get; }
    }

    /// <summary>
    /// Имя листа.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SheetNameAttribute : SheetAttribute
    {
        /// <param name="name">Имя.</param>
        public SheetNameAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Имя.
        /// </summary>
        public string Name { get; }
    }

    /// <summary>
    /// Семейство листа.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SheetFamilyAttribute : SheetAttribute
    {
        /// <param name="family">Семейство.</param>
        public SheetFamilyAttribute(string family)
        {
            Family = family;
        }

        /// <summary>
        /// Семейство.
        /// </summary>
        public string Family { get; }
    }

    // ReSharper restore InconsistentNaming
}