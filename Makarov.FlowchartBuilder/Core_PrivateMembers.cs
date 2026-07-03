using System.Collections.Generic;
using System.Drawing;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Core
    {

        private int _currDocIndex;

        private bool _disposed;

        private Loader _pluginsLoader;

        private Cache<string> _cachedSettingsStorage;

        private IDict<string> _settingsStorage;

        private Cache<Brush> _brushes =
            new Cache<Brush>(new DisposableStringDictionary<Brush>());

        private Cache<Pen> _pens =
            new Cache<Pen>(new DisposableStringDictionary<Pen>());

        private readonly List<AbstractGlyph> _buffer = new List<AbstractGlyph>();

        private readonly List<Document> _documents = new List<Document>();
    }
}
