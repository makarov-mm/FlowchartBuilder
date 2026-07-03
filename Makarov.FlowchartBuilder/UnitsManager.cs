using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace Makarov.FlowchartBuilder
{

    public sealed class UnitsManager
    {
        #region Exceptions

        public class UnitsManagerException : FlowchartBuilderException
        {

            public UnitsManagerException(string message)
                : base(message)
            { }
        }

        public sealed class UnitManagerAlreadyExistsException : SingletonObjectAlreadyExistsException
        {

            public UnitManagerAlreadyExistsException()
                : base("UnitsManagerException")
            { }
        }

        public sealed class UnitsNotFoundException : UnitsManagerException
        {

            public UnitsNotFoundException(string name)
                : base(string.Format("Units '{0}' not found.", name))
            { }
        }

        public sealed class UnitsAlreadyExistsException : UnitsManagerException
        {

            public UnitsAlreadyExistsException(string name)
                : base(string.Format("Units '{0}' already exists.", name))
            { }
        }
        #endregion

        #region Constructors

        public UnitsManager()
        {
            var doc = new XmlDocument();
            doc.Load(Settings.Files.UnitsDefinition);
            doc.Schemas.Add(Settings.Xml.Namespaces.Base, Settings.Files.UnitsSchema);
            doc.Validate(delegate(object sender, ValidationEventArgs args) { throw args.Exception; });
            var mainNode = doc.GetElementsByTagName(Settings.Xml.Tags.UnitsTypes)[0];

            var nfi = new NumberFormatInfo { NumberDecimalSeparator = "." };
            const NumberStyles nstyle = NumberStyles.Float;

            foreach (XmlNode node in mainNode)
            {
                string name = node.Attributes[Settings.Xml.Attributes.Name].Value;
                string shortName = node.Attributes[Settings.Xml.Attributes.ShortName].Value;
                float InMM = float.Parse(node.Attributes[Settings.Xml.Attributes.InMM].Value, nstyle, nfi);
                int BigStep = int.Parse(node.Attributes[Settings.Xml.Attributes.BigStep].Value, nstyle, nfi);
                int SmallStep = int.Parse(node.Attributes[Settings.Xml.Attributes.SmallStep].Value, nstyle, nfi);

                AddUnits(new Units(
                    name,
                    shortName,
                    InMM,
                    BigStep,
                    SmallStep));
            }
        }
        #endregion

        #region Private members

        private readonly Dictionary<string, Units> _units =
            new Dictionary<string, Units>();
        #endregion

        #region Properties

        public Units this[string name]
        {
            get { return GetUnits(name); }
        }

        public int Count
        {
            get { return _units.Count; }
        }

        public IEnumerable<string> LoadedUnits
        {
            get { return _units.Select(kvp => kvp.Key); }
        }
        #endregion

        #region Public methods

        public Units GetUnits(string name)
        {
            if (!_units.TryGetValue(name, out var units))
                throw new UnitsNotFoundException(name);

            return units;
        }

        public void AddUnits(Units units)
        {
            if (_units.ContainsKey(units.Name))
                throw new UnitsAlreadyExistsException(units.Name);

            _units.Add(units.Name, units);
        }

        public void Clear()
        {
            _units.Clear();
        }
        #endregion
    }
}
