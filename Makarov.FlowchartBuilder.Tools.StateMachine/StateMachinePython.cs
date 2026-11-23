using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Makarov.FlowchartBuilder.Tools.StateMachine;

internal static class StateMachinePython
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "False", "None", "True", "and", "as", "assert", "async", "await", "break", "class", "continue",
        "def", "del", "elif", "else", "except", "finally", "for", "from", "global", "if", "import",
        "in", "is", "lambda", "nonlocal", "not", "or", "pass", "raise", "return", "try", "while",
        "with", "yield"
    };

    public static string Generate(MachineDefinition machine, IEnumerable<MachineState> states)
    {
        var ordered = states.OrderBy(s => s.Name, StringComparer.Ordinal).ToArray();
        var names = EnumNames(ordered);
        var code = new StringBuilder();
        code.AppendLine("from __future__ import annotations").AppendLine().AppendLine("from enum import Enum")
            .AppendLine().AppendLine().AppendLine("class State(Enum):");
        foreach (var state in ordered)
            code.AppendLine($"    {names[state.Id]} = {Literal(state.Name)}");

        code.AppendLine().AppendLine().AppendLine("_TRANSITIONS = {");
        foreach (var state in ordered)
        {
            var available = machine.Available(state.Id);
            string key = $"    State.{names[state.Id]}: ";
            if (available.Count == 0)
                code.AppendLine(key + "{},");
            else
            {
                code.AppendLine(key + "{");
                foreach (var edge in available)
                    code.AppendLine($"        {Literal(edge.Event)}: State.{names[edge.TargetId]},");
                code.AppendLine("    },");
            }
        }
        code.AppendLine("}").AppendLine();
        var finalStates = ordered.Where(state => state.IsFinal).Select(state => $"State.{names[state.Id]}").ToArray();
        string finals = finalStates.Length == 0 ? "set()" : "{" + string.Join(", ", finalStates) + "}";
        code.AppendLine($"_FINAL_STATES = {finals}").AppendLine().AppendLine();
        code.AppendLine($$"""
            class StateMachine:
                def __init__(self) -> None:
                    self._state = State.{{names[machine.Initial.Id]}}

                @property
                def state(self) -> State:
                    return self._state

                @property
                def is_final(self) -> bool:
                    return self._state in _FINAL_STATES

                def reset(self) -> State:
                    self._state = State.{{names[machine.Initial.Id]}}
                    return self._state

                def available_events(self) -> tuple[str, ...]:
                    return tuple(_TRANSITIONS[self._state])

                def handle(self, event: str) -> State:
                    transitions = _TRANSITIONS[self._state]
                    if not isinstance(event, str) or event not in transitions:
                        raise ValueError(f"Unknown event {event!r} in state {self._state.value!r}")
                    self._state = transitions[event]
                    return self._state
            """.ReplaceLineEndings());
        return code.ToString();
    }

    private static Dictionary<string, string> EnumNames(MachineState[] states)
    {
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in states.GroupBy(s => Identifier(s.Name), StringComparer.Ordinal))
        {
            if (group.Key is not { } name || group.Count() != 1) continue;
            names.Add(group.First().Id, name);
            used.Add(name);
        }
        foreach (var state in states.Where(s => !names.ContainsKey(s.Id)).OrderBy(s => s.Id, StringComparer.Ordinal))
        {
            // The persistent document ID keeps a generated member stable when another state moves or is added.
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(state.Id)))[..10];
            string prefix = $"STATE_{hash}";
            string candidate = prefix;
            for (int i = 2; !used.Add(candidate); i++) candidate = $"{prefix}_{i}";
            names.Add(state.Id, candidate);
        }
        return names;
    }

    private static string? Identifier(string name)
    {
        if (Keywords.Contains(name) || !IsMemberLetter(name[0])) return null;
        var result = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (c is ' ' or '-' or '_')
            {
                if (result[^1] != '_') result.Append('_');
            }
            else if (IsMemberLetter(c) || char.IsAsciiDigit(c))
            {
                if (i > 0 && char.IsUpper(c) && char.IsLower(name[i - 1])) result.Append('_');
                result.Append(char.ToUpperInvariant(c));
            }
            else return null;
        }
        return result.ToString();
    }

    private static bool IsMemberLetter(char c) => char.IsAsciiLetter(c)
        || c is >= 'А' and <= 'я' or 'Ё' or 'ё' or 'Є' or 'є' or 'І' or 'і' or 'Ї' or 'ї' or 'Ґ' or 'ґ';

    private static string Literal(string value)
    {
        var result = new StringBuilder("\"");
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            switch (c)
            {
                case '"': result.Append("\\\""); break;
                case '\\': result.Append("\\\\"); break;
                case '\n': result.Append("\\n"); break;
                case '\r': result.Append("\\r"); break;
                case '\t': result.Append("\\t"); break;
                default:
                    if (char.IsHighSurrogate(c) && i + 1 < value.Length && char.IsLowSurrogate(value[i + 1]))
                        result.Append(c).Append(value[++i]);
                    else if (char.IsSurrogate(c) || char.IsControl(c) || c is '\u2028' or '\u2029'
                        || char.GetUnicodeCategory(c) == UnicodeCategory.Format)
                        result.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    else
                        result.Append(c);
                    break;
            }
        }
        return result.Append('"').ToString();
    }
}
