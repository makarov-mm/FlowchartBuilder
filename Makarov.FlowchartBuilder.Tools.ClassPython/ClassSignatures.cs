namespace Makarov.FlowchartBuilder.Tools.ClassPython;

internal sealed record ClassField(string Name, string Type);
internal sealed record ClassMethod(string Name, IReadOnlyList<ClassField> Parameters, string Result);

// Parse and reprint the supported notation. Raw captions never enter the Python module.
internal sealed class ClassSignatures(string text, IReadOnlySet<string> classes)
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "False", "None", "True", "and", "as", "assert", "async", "await", "break", "class",
        "continue", "def", "del", "elif", "else", "except", "finally", "for", "from", "global",
        "if", "import", "in", "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return",
        "try", "while", "with", "yield", "__debug__"
    };
    private static readonly HashSet<string> Scalars = ["int", "float", "str", "bool", "bytes", "object", "None"];
    private static readonly HashSet<string> Containers = ["list", "set", "dict", "tuple"];
    private int _position;

    public static bool IsIdentifier(string name) =>
        name.Length > 0 && StartsName(name[0]) && name.All(ContinuesName)
        && !Keywords.Contains(name) && !name.StartsWith("__", StringComparison.Ordinal);

    public static bool IsClassName(string name) => IsIdentifier(name) && !Scalars.Contains(name)
        && !Containers.Contains(name) && name is not ("_dataclass" or "NotImplementedError");

    // Python reserves self for the receiver in generated signatures. A dataclass
    // field named mro would pick up type.mro as an accidental field default.
    public static bool IsMemberName(string name) => IsIdentifier(name) && name is not ("self" or "mro");

    public ClassField Field()
    {
        var result = ReadParameter();
        if (!IsMemberName(result.Name)) throw new FormatException();
        RequireEnd();
        return result;
    }

    public ClassMethod Method()
    {
        string name = ReadName();
        if (!IsMemberName(name)) throw new FormatException();
        Expect("(");
        var parameters = new List<ClassField>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (!Take(")"))
        {
            do
            {
                var parameter = ReadParameter();
                if (parameter.Name == "NotImplementedError" || !names.Add(parameter.Name)) throw new FormatException();
                parameters.Add(parameter);
            } while (Take(","));
            Expect(")");
        }
        Expect("->");
        string result = ReadType();
        RequireEnd();
        return new(name, parameters, result);
    }

    private ClassField ReadParameter()
    {
        string name = ReadName();
        if (!IsIdentifier(name) || name == "self") throw new FormatException();
        Expect(":");
        return new(name, ReadType());
    }

    private string ReadType(int depth = 0)
    {
        if (depth >= 32) throw new FormatException();
        var options = new List<string>();
        do { options.Add(ReadAtom(depth)); } while (Take("|"));
        return string.Join(" | ", options);
    }

    private string ReadAtom(int depth)
    {
        string name = ReadName();
        if (Scalars.Contains(name) || classes.Contains(name)) return name;
        if (!Containers.Contains(name)) throw new FormatException();
        Expect("[");
        var arguments = new List<string> { ReadType(depth + 1) };
        while (Take(","))
        {
            if (name == "tuple" && arguments.Count == 1 && Take("..."))
            {
                arguments.Add("...");
                break;
            }
            arguments.Add(ReadType(depth + 1));
        }
        Expect("]");
        if ((name is "list" or "set") && arguments.Count != 1 || name == "dict" && arguments.Count != 2)
            throw new FormatException();
        return $"{name}[{string.Join(", ", arguments)}]";
    }

    private string ReadName()
    {
        SkipSpace();
        int start = _position;
        if (start == text.Length || !StartsName(text[start])) throw new FormatException();
        _position++;
        while (_position < text.Length && ContinuesName(text[_position])) _position++;
        return text[start.._position];
    }

    private static bool StartsName(char c) => char.IsAsciiLetter(c) || c == '_';
    private static bool ContinuesName(char c) => StartsName(c) || char.IsAsciiDigit(c);

    private void SkipSpace()
    {
        while (_position < text.Length && char.IsWhiteSpace(text[_position])) _position++;
    }

    private bool Take(string token)
    {
        SkipSpace();
        if (!text.AsSpan(_position).StartsWith(token, StringComparison.Ordinal)) return false;
        _position += token.Length;
        return true;
    }

    private void Expect(string token)
    {
        if (!Take(token)) throw new FormatException();
    }

    private void RequireEnd()
    {
        SkipSpace();
        if (_position != text.Length) throw new FormatException();
    }
}
