using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Makarov.Framework.Core.Managers
{

    public sealed class TranslatorsManager : ICloneable, IDisposable, IComparable<TranslatorsManager>
    {
        #region Exceptions

        public class TranslatorsManagerException : MakarovFrameworkException
        {

            public TranslatorsManagerException(string message)
                : base(message)
            { }
        }

        public sealed class LanguageNotExistsException : TranslatorsManagerException
        {

            public LanguageNotExistsException(string language)
                : base($"Language '{language}' not exists.")
            { }
        }

        public sealed class LanguageAlreadyExistsException : TranslatorsManagerException
        {

            public LanguageAlreadyExistsException(string language)
                : base($"Language '{language}' already exists.")
            { }
        }
        #endregion

        #region Private members

        private readonly List<Translator> _translators = new List<Translator>();
        #endregion

        #region Constructors

        public TranslatorsManager()
        { }

        public TranslatorsManager(Translator t)
        {
            Add(t);
        }

        public TranslatorsManager(string folder, string schemaFile)
        {
            Task.WaitAll((from file in Directory.GetFiles(folder)
                          let ext = Path.GetExtension(file)
                          where ext != null && ext == ".xml"
                          select Task.Factory.StartNew(() => Add(new Translator(file, schemaFile)))).ToArray());
        }

        public TranslatorsManager(TranslatorsManager other)
        {
            foreach (var t in other._translators)
                Add(t);
        }
        #endregion

        #region Public methods

        public object Clone()
        {
            return new TranslatorsManager(this);
        }

        public void Dispose()
        {
            lock (_translators)
                _translators.Clear();
        }

        public int CompareTo(TranslatorsManager tm)
        {
            if (((object) tm) == null) return 1;
            if (Count != tm.Count) return Count - tm.Count;

            lock (_translators)
                foreach (var t in _translators)
                    if (!tm.IsLanguageExists(t.Language))
                        return 1;
                    else
                    {
                        int diff = tm[t.Language].CompareTo(t);
                        if (diff != 0)
                            return diff;
                    }

            return 0;
        }

        public static TranslatorsManager LoadFromFolder(string folder, string schemaFile)
        {
            return new TranslatorsManager(folder, schemaFile);
        }

        public bool IsLanguageExists(string language)
        {
            lock (_translators)
                if (_translators.Any(t => t.Language == language))
                    return true;

            return false;
        }

        public Translator GetTranslator(string language)
        {
            if (language == null)
                throw new NullStringValueException("language");

            lock (_translators)
                foreach (var t in _translators)
                    if (t.Language == language)
                        return t;

            throw new LanguageNotExistsException(language);
        }

        public void Add(Translator t)
        {
            if (IsLanguageExists(t.Language))
                throw new LanguageAlreadyExistsException(t.Language);

            lock (_translators)
                _translators.Add(t);
        }

        public void Delete(string language)
        {
            lock (_translators)
                foreach (var t in _translators)
                    if (t.Language == language)
                    {
                        _translators.Remove(t);
                        return;
                    }

            throw new LanguageNotExistsException(language);
        }

        public override bool Equals(Object o)
        {
            if (o == null) return false;
            if (o.GetType() != GetType()) return false;
            return CompareTo((TranslatorsManager)o) == 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                lock (_translators)
                    return _translators.Sum(t => t.GetHashCode());
            }
        }

        public void Clear()
        {
            lock (_translators)
                _translators.Clear();
        }

        #endregion

        #region Properties

        public int Count
        {
            get
            {
                lock (_translators)
                    return _translators.Count;
            }
        }

        public Translator this[string language]
        {
            get { return GetTranslator(language); }
        }

        public IEnumerable<string> Languages
        {
            get { return Translators.Select(t => t.Language); }
        }

        public IEnumerable<Translator> Translators
        {
            get
            {
                lock (_translators)
                    foreach (var t in _translators)
                        yield return t;
            }
        }
        #endregion

        #region Operators

        public static bool operator ==(TranslatorsManager tm1, TranslatorsManager tm2)
        {
            if (((object)tm1) == null && ((object)tm2) == null) return true;
            if (((object)tm1) == null || ((object)tm2) == null) return false;
            return tm1.CompareTo(tm2) == 0;
        }

        public static bool operator !=(TranslatorsManager tm1, TranslatorsManager tm2)
        {
            return !(tm1 == tm2);
        }

        public static TranslatorsManager operator +(TranslatorsManager tm, Translator t)
        {
            tm.Add(t);
            return tm;
        }
        #endregion
    }
}
