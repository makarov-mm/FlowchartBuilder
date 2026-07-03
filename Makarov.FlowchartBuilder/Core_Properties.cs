using System;
using System.Collections.Generic;
using System.Drawing;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.Framework.Core;
using Makarov.Framework.Core.SettingStorages;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Core
    {

        public MainForm MainWindow
        {
            get { return Windows.GetWindow<MainForm>(); }
        }

        public Document CurrentDocument
        {
            get
            {

                if (!IsDocumentsExists)
                    throw new CurrentDocumentNotExistsException();

                return _documents[_currDocIndex];
            }
        }

        public bool IsDocumentsExists
        {
            get { return _documents.Count > 0; }
        }

        public bool IsFirstRun
        {
            get;
            private set;
        }

        public List<AbstractGlyph> Buffer
        {
            get { return _buffer; }
        }

        public Type CurrentGlyphType
        {
            get;
            set;
        }

        public bool CurrGlyphTypeSelected
        {
            get { return CurrentGlyphType != null; }
        }

        public bool NeedRedraw
        {
            get;
            set;
        }

        public Loader PluginsLoader
        {
            get
            {
                return _pluginsLoader ??
                    (_pluginsLoader = new Loader(Settings.Directories.Plugins));
            }
        }

        public Translator CurrentTranslator
        {
            get { return Translators[Settings.Environment.Languages.CurrentLanguage]; }
        }

        private IDict<string> SettingsStorage
        {
            get
            {
                if (_settingsStorage != null)
                    return _settingsStorage;

                if (Framework.Core.Settings.Environment.IsMono)
                {
                    try
                    {
                        return (_settingsStorage = new XmlStorage(
                            Settings.Files.XmlSettingsFile));
                    }
                    catch
                    {
                        return (_settingsStorage = new FakeStorage());
                    }
                }

                try
                {
                    return (_settingsStorage = new RegistryStorage(
                        Settings.RegistryKeys.ProgramSettingsKey));
                }
                catch
                {
                    try
                    {
                        return (_settingsStorage = new XmlStorage(
                            Settings.Files.XmlSettingsFile));
                    }
                    catch
                    {
                        return (_settingsStorage = new FakeStorage());
                    }
                }
            }
        }

        public Cache<string> CachedSettingsStorage
        {
            get
            {
                return _cachedSettingsStorage ?? (_cachedSettingsStorage = new Cache<string>(SettingsStorage));
            }
        }

        public Cache<Pen> CachedPens
        {
            get { return _pens; }
        }

        public Cache<Brush> CachedBrushes
        {
            get { return _brushes; }
        }
    }
}
