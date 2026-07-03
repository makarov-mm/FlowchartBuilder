using System.Reflection;
using Makarov.Framework.Core.Managers;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Core
    {

        public GraphicsManager Images => _graphicsManager ?? (_graphicsManager = new GraphicsManager(Settings.Directories.Graphics));

        public GraphicsManager Icons => _iconsManager ?? (_iconsManager = new GraphicsManager(Settings.Directories.Icons));

        public WindowsManager Windows => _windowsManager ?? (_windowsManager = new WindowsManager(typeof(Core).Assembly));

        public TranslatorsManager Translators =>
            _translatorsManager ?? (_translatorsManager = TranslatorsManager.LoadFromFolder(
                Settings.Directories.Translations,
                Settings.Files.TranslationsSchema));

        public SerializerManager Serializers =>
            _serializer
            ??
            (_serializer = new SerializerManager(Assembly.LoadFrom(Settings.Files.SerializationDll)));

        public UnitsManager Units => _unitsManager ?? (_unitsManager = new UnitsManager());

        private GraphicsManager _graphicsManager;

        private GraphicsManager _iconsManager;

        private WindowsManager _windowsManager;

        private TranslatorsManager _translatorsManager;


        private SerializerManager _serializer;

        private UnitsManager _unitsManager;
    }
}
