using System;
using System.Collections.Generic;
using System.Linq;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder
{

    public sealed class PluginsManager
    {
        #region Exceptions

        public class PluginsManagerException : FlowchartBuilderException
        {

            public PluginsManagerException(string message)
                : base(message)
            { }
        }

        public sealed class PluginsManagerAlreadyExistsException : SingletonObjectAlreadyExistsException
        {

            public PluginsManagerAlreadyExistsException()
                : base("PluginsManager")
            { }
        }
        #endregion

        #region Constructors

        private static PluginsManager _instance;

        private PluginsManager()
        {

            if (_instance != null)
                throw new PluginsManagerAlreadyExistsException();

            Refresh();
        }
        #endregion

        #region Properties

        public static PluginsManager Instance
        {
            get { return _instance ?? (_instance = new PluginsManager()); }
        }

        public IEnumerable<IPlugin> Plugins
        {
            get { return _plugins; }
        }

        public IEnumerable<Type> Glyphs
        {
            get { return _glyphs; }
        }

        public IEnumerable<Type> VisibleGlyphs
        {
            get
            {
                foreach (Type type in Glyphs)
                {
                    object[] visibleAttribs =
                        type.GetCustomAttributes(typeof(VisibleGlyphAttribute), true);

                    if (visibleAttribs.Length > 0)
                    {
                        if (((VisibleGlyphAttribute)visibleAttribs[visibleAttribs.Length - 1]).IsVisible)
                            yield return type;
                    }
                    else
                        yield return type;
                }
            }
        }

        public IEnumerable<string> GlyphsLibraries
        {
            get { return _glyphsLibs; }
        }
        #endregion

        #region Private members

        private readonly List<IPlugin> _plugins = new List<IPlugin>();

        private readonly List<Type> _glyphs = new List<Type>();

        private readonly List<string> _glyphsLibs = new List<string>();
        #endregion

        #region Public methods

        public void Refresh()
        {

            _plugins.Clear();
            _glyphs.Clear();

            var classes =
                (from type in Core.Instance.PluginsLoader.GetTypes<object>()
                where type.IsClass && !type.IsAbstract
                select type).ToArray();

            foreach (Type cls in classes)
            {

                Type[] ifaces = cls.GetInterfaces();

                foreach (Type iface in ifaces)
                {

                    if (iface == typeof (IPlugin))
                    {

                        _plugins.Add((IPlugin) Activator.CreateInstance(cls));
                        break;
                    }
                }

            }

            var glyphs =
                from type in classes
                where type.IsSubclassOf(typeof (AbstractGlyph))
                select type;

            foreach (Type cls in glyphs)
            {

                _glyphs.Add(cls);

                foreach (var nameAttr in cls.GetCustomAttributes(typeof (GlyphFamilyAttribute), true))
                {
                    var name = ((GlyphFamilyAttribute)nameAttr).Family;
                    if (!_glyphsLibs.Contains(name))
                        _glyphsLibs.Add(name);
                }
            }
        }
        #endregion
    }
}
