using System.Xml;
using Makarov.Framework.Core.Xml;

namespace Makarov.Framework.Core;

/// <summary>
/// One UI language loaded from an XML dictionary (see Translations.xsd).
/// </summary>
public sealed class Translator
{
    private const string XmlNamespace = "http://flowchartbuilder.blogspot.com";
    private const string WordTag = "fb:Word";
    private const string KeyAttribute = "Key";
    private const string LanguageAttribute = "Language";

    private readonly Dictionary<string, string> _words;

    private Translator(string language, Dictionary<string, string> words)
    {
        Language = language;
        _words = words;
    }

    public string Language { get; }

    public int Count => _words.Count;

    /// <summary>
    /// Returns the translation of <paramref name="word"/>; a missing word comes back as "+ word"
    /// so that untranslated keys stay visible in the UI.
    /// </summary>
    public string this[string word] => _words.TryGetValue(word, out string? translation) ? translation : "+ " + word;

    public bool Contains(string word) => _words.ContainsKey(word);

    public static Translator Load(string fileName, string schemaFile)
    {
        XmlElement root = ValidatedXml.Load(fileName, XmlNamespace, schemaFile);

        var words = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (XmlElement word in root.ChildNodes.OfType<XmlElement>().Where(e => e.Name == WordTag))
            words[word.GetAttribute(KeyAttribute)] = word.InnerText;

        return new Translator(root.GetAttribute(LanguageAttribute), words);
    }
}
