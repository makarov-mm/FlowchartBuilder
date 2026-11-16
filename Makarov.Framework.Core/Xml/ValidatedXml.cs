using System.Xml;

namespace Makarov.Framework.Core.Xml;

public static class ValidatedXml
{
    /// <summary>
    /// Loads <paramref name="fileName"/> and validates it against <paramref name="schemaFile"/>;
    /// any validation error or warning is thrown as an <see cref="System.Xml.Schema.XmlSchemaException"/>.
    /// </summary>
    public static XmlElement Load(string fileName, string targetNamespace, string schemaFile)
    {
        var document = new XmlDocument();
        document.Load(fileName);
        document.Schemas.Add(targetNamespace, schemaFile);
        document.Validate((_, e) => throw e.Exception);

        return document.DocumentElement ?? throw new InvalidDataException($"'{fileName}' has no root element.");
    }
}
