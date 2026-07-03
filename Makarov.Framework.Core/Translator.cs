using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace Makarov.Framework.Core
{

    public sealed class Translator: ICloneable, IDisposable, IComparable<Translator>
    {
        #region Exceptions

        public class TranslatorException : MakarovFrameworkException
        {

            public TranslatorException(string message)
                : base(message)
            { }
        }

        public sealed class WordNotExistsExcetion : TranslatorException
        {

            public WordNotExistsExcetion(string word)
                : base(string.Format("Word '{0}' not exists.", word))
            { }
        }

        public sealed class DictionaryNotLoadedFromFileException : TranslatorException
        {

            public DictionaryNotLoadedFromFileException()
                : base("Dictionary not loaded from file.")
            { }
        }

        public sealed class InvalidNodeTypeException : TranslatorException
        {

            public InvalidNodeTypeException(XmlNodeType t)
                : base(string.Format("Invalid node '{0}'.", t))
            { }
        }

        public sealed class InvalidNodeNameException : TranslatorException
        {

            public InvalidNodeNameException(string nodeName)
                : base(string.Format("Invalid node name '{0}'", nodeName))
            { }
        }

        public sealed class IncompatibleLanguagesException : TranslatorException
        {

            public IncompatibleLanguagesException()
                : base("Incompatible languages.")
            { }
        }
        #endregion

        #region Private members

        private readonly List<KeyValuePair<string, string>> _words = new List<KeyValuePair<string, string>>();
        #endregion

        #region Properties

        public string Language
        {
            get; private set;
        }

        public int Count
        {
            get { return _words.Count; }
        }

        public bool IsLoadedFromFile
        {
            get; private set;
        }

        public string FileName
        {
            get; private set;
        }

        public string this[string word]
        {
            get { return GetTranslation(word); }
            set { SetTranslation(word, value); }
        }
        #endregion

        #region Public methods

        public void Clear()
        {
            _words.Clear();
        }

        public void Add(string word, string translation)
        {
            if (word is null)
                throw new NullStringValueException("word");

            if (translation is null)
                throw new NullStringValueException("translation");

            foreach (var kvp in _words)
                if (kvp.Key == word)
                {

                    _words.Remove(kvp);
                    break;
                }

            _words.Add(new KeyValuePair<string, string>(word, translation));
        }

        public bool IsWordExists(string word)
        {
            if (word == null)
                throw new NullStringValueException("word");

            return _words.Any(kvp => kvp.Key == word);
        }

        public void Delete(string word)
        {
            if (word is null)
                throw new NullStringValueException("word");

            foreach (var kvp in _words)
                if (kvp.Key == word)
                {
                    _words.Remove(kvp);
                    return;
                }

            throw new WordNotExistsExcetion(word);
        }

        public string GetTranslation(string word)
        {
            if (word is null)
                throw new NullStringValueException("word");

            foreach (var kvp in _words)
                if (kvp.Key == word)
                    return kvp.Value;

            return "+ " + word;
        }

        public void SetTranslation(string word, string translation)
        {
            Add(word, translation);
        }

        public void Reload(string schemaFile)
        {
            if (!IsLoadedFromFile)
                throw new DictionaryNotLoadedFromFileException();

            Load(FileName, schemaFile);
        }

        public void Load(string fileName, string schemaFile)
        {

            var doc = new XmlDocument();
            doc.Load(fileName);

            if (!Settings.Environment.IsMono)
            {

                doc.Schemas.Add(Settings.Xml.Namespaces.Base, schemaFile);
                doc.Validate(delegate(object sender, ValidationEventArgs args) { throw args.Exception; });
            }

            var mainNode = doc.GetElementsByTagName(Settings.Xml.Tags.Translation)[0];

            string lang = mainNode.Attributes[Settings.Xml.Attributes.Language].Value;
            if (string.IsNullOrEmpty(Language))
                Language = lang;
            else if (Language != lang)
                throw new IncompatibleLanguagesException();
            else
                Language = lang;

            foreach (XmlNode node in mainNode)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (node.Name == Settings.Xml.Tags.Word)
                        Add(node.Attributes[Settings.Xml.Attributes.Key].Value,
                            node.InnerText);
                }
                else if (node.NodeType != XmlNodeType.Comment)
                    throw new InvalidNodeTypeException(node.NodeType);
            }

            IsLoadedFromFile = true;
            FileName = fileName;
        }

        public void Save(string fileName)
        {
            var doc = new XmlDocument();

            var mainNode = doc.CreateElement(Settings.Xml.Tags.Translation);
            var langAttrib = doc.CreateAttribute(Settings.Xml.Attributes.Language);
            langAttrib.Value = Language;
            mainNode.Attributes.Append(langAttrib);

            foreach (var kvp in _words)
            {
                var node = doc.CreateElement(Settings.Xml.Tags.Word);
                var wordAttrib = doc.CreateAttribute(Settings.Xml.Attributes.Key);
                wordAttrib.Value = kvp.Key;
                node.Attributes.Append(wordAttrib);
                node.InnerText = kvp.Value;
                mainNode.AppendChild(node);
            }

            doc.AppendChild(mainNode);
            doc.Save(fileName);
        }

        public object Clone()
        {
            return new Translator(this);
        }

        public void Dispose()
        {
            _words.Clear();
            IsLoadedFromFile = false;
            FileName = null;
            Language = null;
        }

        public int CompareTo(Translator other)
        {
            if (((object)other) == null) return 1;

            if (Language != other.Language)
                return (Language ?? "").CompareTo(other.Language ?? "");

            if (other._words.Count != _words.Count)
                return _words.Count - other._words.Count;

            if (other._words.Any(kvp => !IsWordExists(kvp.Key) || this[kvp.Key] != kvp.Value))
                return 1;

            return 0;
        }

        public static Translator Compose(Translator t1, Translator t2)
        {
            if (t1.Language != t2.Language)
                throw new IncompatibleLanguagesException();

            var tmp = CreateEmptyTranslator(t1.Language);

            foreach (var kvp in t1._words)
                tmp.Add(kvp.Key, kvp.Value);

            foreach (var kvp in t2._words)
                tmp.Add(kvp.Key, kvp.Value);

            return tmp;
        }

        public static Translator CreateEmptyTranslator(string language)
        {
            if (language is null)
                throw new NullStringValueException("language");

            return new Translator {Language = language};
        }

        public override bool Equals(Object o)
        {
            if (o.GetType() != GetType()) return false;
            return CompareTo((Translator) o) == 0;
        }

        public override int GetHashCode()
        {
            return Language.GetHashCode();
        }
        #endregion

        #region Constructors

        private Translator()
        { }

        public Translator(string fileName, string schemaFile)
        {
            Load(fileName, schemaFile);
        }

        public Translator(Translator other)
        {
            foreach (var kvp in other._words)
                _words.Add(kvp);

            IsLoadedFromFile = other.IsLoadedFromFile;
            FileName = other.FileName;
            Language = other.Language;
        }
        #endregion

        #region Operators

        public static Translator operator +(Translator t1, Translator t2)
        {
            return Compose(t1, t2);
        }

        public static Translator operator +(Translator t, KeyValuePair<string, string> kvp)
        {
            t.Add(kvp.Key, kvp.Value);
            return t;
        }

        public static Translator operator +(KeyValuePair<string, string> kvp, Translator t)
        {
            t.Add(kvp.Key, kvp.Value);
            return t;
        }

        public static Translator operator -(Translator t, string word)
        {
            t.Delete(word);
            return t;
        }

        public static bool operator ==(Translator t1, Translator t2)
        {
            if (((object)t1) == null && ((object)t2) == null) return true;
            if (((object)t1) == null || ((object)t2) == null) return false;
            return t1.CompareTo(t2) == 0;
        }

        public static bool operator !=(Translator t1, Translator t2)
        {
            return !(t1 == t2);
        }
        #endregion
    }
}
