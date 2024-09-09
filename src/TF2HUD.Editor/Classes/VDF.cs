using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HUDEditor.Classes
{
    internal class VDFTokenizer
    {
        public static readonly char[] IgnoredChars = { ' ', '\t', '\r', '\n' };
        public static readonly char[] TokenTerminate = { '"', '{', '}' };

        private readonly string Text;
        public int Index { get; private set; } = 0;
        public int Line { get; private set; } = 1;
        public int Character { get; private set; } = 1;
        private bool EOFRead = false;

        public VDFTokenizer(string text)
        {
            Text = text;
        }

        public VDFToken? Next(bool peek = false)
        {
            int index = Index;
            int line = Line;
            int character = Character;

            while (index < Text.Length && (IgnoredChars.Contains(Text[index]) || Text[index] == '/'))
            {
                if (Text[index] == '\n')
                {
                    line++;
                    character = 1;
                }
                else if (Text[index] == '/')
                {
                    index++;
                    if (index < Text.Length && Text[index] == '/')
                    {
                        while (index < Text.Length && Text[index] != '\n')
                        {
                            index++;
                        }
                        line++;
                        character = 1;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    character++;
                }
                index++;
            }

            // Return EOF if we've reached the end of the text file.
            if (index >= Text.Length)
            {
                if (EOFRead) throw new EndOfStreamException();
                if (!peek) EOFRead = true;
                return null;
            }

            VDFToken token;

            switch (Text[index])
            {
                case '{':
                case '}':
                    {
                        token = new VDFToken
                        {
                            Type = VDFTokenType.ControlCharacter,
                            Value = Text[index].ToString(),
                        };

                        index++;
                        character++;
                        break;
                    }
                case '"':
                    {
                        // If we encounter a quote, read the enclosed text until the next quotation mark.

                        // Skip the opening quotation mark.
                        index++;
                        character++;

                        var start = index;

                        while (Text[index] != '"')
                        {
                            if (index >= Text.Length)
                            {
                                throw new VDFSyntaxException(VDFTokenType.String, "EOF", new[] { "closing double quote" }, index, line, character);
                            }
                            index++;
                            character++;
                        }

                        var end = index;

                        // Skip the closing quotation mark.
                        index++;
                        character++;

                        token = new VDFToken
                        {
                            Type = VDFTokenType.String,
                            Value = Text[start..end],
                        };

                        break;
                    }
                default:
                    {
                        var start = index;

                        while (index < Text.Length && !IgnoredChars.Contains(Text[index]))
                        {
                            if (TokenTerminate.Contains(Text[index]))
                            {
                                break;
                            }
                            index++;
                            character++;
                        }

                        var end = index;

                        var str = Text[start..end];

                        token = new VDFToken
                        {
                            Type = str.StartsWith('[') && str.EndsWith(']') ? VDFTokenType.Conditional : VDFTokenType.String,
                            Value = str,
                        };

                        break;
                    }
            }

            if (!peek)
            {
                Index = index;
                Line = line;
                Character = character;
            }

            return token;
        }
    }

    internal record struct VDFToken
    {
        public required VDFTokenType Type { get; init; }
        public required string Value { get; init; }
    }

    internal enum VDFTokenType
    {
        String,
        Conditional,
        ControlCharacter,
    }

    internal class VDFSyntaxException : Exception
    {
        public VDFSyntaxException(VDFTokenType type, string unexpected, string[] expected, int index, int line, int character) : base($"Unexpected {type} '{unexpected}' at position {index} (line {line}, character {character}). Expected {string.Join(" | ", expected)}")
        {
        }
    }

    internal static class VDF
    {
        public static string ConditionalDelimiter = "^";

        public static Dictionary<string, dynamic> Parse(string text)
        {
            var tokeniser = new VDFTokenizer(text);

            Dictionary<string, dynamic> ParseObject(bool isObject)
            {
                Dictionary<string, dynamic> objectRef = new();

                VDFToken? objectTerminator = isObject ? new VDFToken { Type = VDFTokenType.ControlCharacter, Value = "}" } : null;

                while (true)
                {
                    string key;
                    dynamic value;
                    string conditional;

                    var keyToken = tokeniser.Next();

                    if (keyToken == objectTerminator) break;
                    if (keyToken == null) throw new VDFSyntaxException(VDFTokenType.String, "EOF", new[] { "key" }, tokeniser.Index, tokeniser.Line, tokeniser.Character);

                    switch (keyToken.Value.Type)
                    {
                        case VDFTokenType.String:
                            {
                                key = keyToken.Value.Value;

                                var valueToken = tokeniser.Next();
                                if (valueToken == null) throw new VDFSyntaxException(VDFTokenType.String, "EOF", new[] { "value", "{", "conditional" }, tokeniser.Index, tokeniser.Line, tokeniser.Character);

                                if (valueToken.Value.Type == VDFTokenType.Conditional)
                                {
                                    conditional = valueToken.Value.Value;
                                    valueToken = tokeniser.Next();
                                    if (valueToken == null) throw new VDFSyntaxException(VDFTokenType.String, "EOF", new[] { "value", "{" }, tokeniser.Index, tokeniser.Line, tokeniser.Character);
                                }

                                switch (valueToken.Value.Type)
                                {
                                    case VDFTokenType.ControlCharacter:
                                        {

                                            if (valueToken.Value.Value == "{")
                                            {
                                                // Object
                                                value = ParseObject(true);
                                                conditional = null;
                                                break;
                                            }
                                            else
                                                throw new VDFSyntaxException(VDFTokenType.ControlCharacter, valueToken.Value.Value, new[] { "{" }, tokeniser.Index, tokeniser.Line, tokeniser.Character);
                                        }
                                    case VDFTokenType.String:
                                        {
                                            value = valueToken.Value.Value;

                                            var conditionalToken = tokeniser.Next(true);
                                            if (conditionalToken?.Type == VDFTokenType.Conditional)
                                            {
                                                conditional = conditionalToken.Value.Value;
                                                tokeniser.Next();
                                            }
                                            else
                                            {
                                                conditional = null;
                                            }
                                            break;
                                        }
                                    case VDFTokenType.Conditional:
                                        throw new VDFSyntaxException(VDFTokenType.Conditional, valueToken.Value.Value, new[] { "value", "{" }, tokeniser.Index, tokeniser.Line, tokeniser.Character);
                                    default:
                                        throw new Exception();
                                }
                                break;
                            }
                        case VDFTokenType.ControlCharacter:
                            throw new VDFSyntaxException(VDFTokenType.ControlCharacter, keyToken.Value.Value, new[] { "key" }, tokeniser.Index, tokeniser.Line, tokeniser.Character);
                        case VDFTokenType.Conditional:
                            throw new VDFSyntaxException(VDFTokenType.Conditional, keyToken.Value.Value, new[] { "key" }, tokeniser.Index, tokeniser.Line, tokeniser.Character);
                        default:
                            throw new Exception();
                    }

                    if (conditional is not null)
                    {
                        key += $"{VDF.ConditionalDelimiter}{conditional}";
                    }

                    if (objectRef.TryGetValue(key, out var existing))
                    {
                        if (existing is List<dynamic> existingList)
                        {
                            // Object list exists
                            existingList.Add(value);
                        }
                        else
                        {
                            // Object already exists
                            objectRef[key] = new List<dynamic> { existing, value };
                        }
                    }
                    else
                    {
                        // Object does not exist
                        objectRef[key] = value;
                    }
                }

                return objectRef;
            }

            return ParseObject(false);
        }

        public static string Stringify(Dictionary<string, dynamic> obj, int tabs = 0)
        {
            var stringValue = "";
            const char tab = '\t';
            const string newLine = "\r\n";

            foreach (var key in obj.Keys)
                if (obj[key].GetType() == typeof(List<dynamic>))
                {
                    // Item has multiple instances
                    foreach (var item in obj[key])
                        if (item.GetType() == typeof(Dictionary<string, dynamic>))
                        {
                            // Check for conditional.
                            var keyTokens = key.Split('^');
                            if (keyTokens.Length > 1)
                                stringValue += $"{new string(tab, tabs)}\"{key}\" {keyTokens[1]}{newLine}";
                            else
                                stringValue += $"{new string(tab, tabs)}{key}{newLine}";

                            stringValue += $"{new string(tab, tabs)}{{{newLine}";
                            stringValue += $"{Stringify(item, tabs + 1)}{new string(tab, tabs)}}}{newLine}";
                        }
                        else
                        {
                            // Check for conditional.
                            var keyTokens = key.Split('^');
                            if (keyTokens.Length > 1)
                                stringValue += $"{new string(tab, tabs)}\"{key}\"\t\"{item}\" {keyTokens[1]}{newLine}";
                            else
                                stringValue += $"{new string(tab, tabs)}\"{key}\"\t\"{item}\"{newLine}";
                        }
                }
                else
                {
                    // There is only one object object/value
                    if (obj[key] is IDictionary<string, dynamic>)
                    {
                        // Check for conditional.
                        var keyTokens = key.Split('^');
                        if (keyTokens.Length > 1)
                        {
                            stringValue += $"{new string(tab, tabs)}\"{keyTokens[0]}\" {keyTokens[1]}{newLine}";
                            stringValue += $"{new string(tab, tabs)}{{{newLine}";
                            stringValue += $"{Stringify(obj[key], tabs + 1)}{new string(tab, tabs)}}}{newLine}";
                        }
                        else
                        {
                            stringValue += $"{new string(tab, tabs)}\"{key}\"{newLine}";
                            stringValue += $"{new string(tab, tabs)}{{{newLine}";
                            stringValue += $"{Stringify(obj[key], tabs + 1)}{new string(tab, tabs)}}}{newLine}";
                        }
                    }
                    else
                    {
                        // Check for an conditional.
                        var keyTokens = key.Split('^');
                        if (keyTokens.Length > 1)
                            stringValue +=
                                $"{new string(tab, tabs)}\"{keyTokens[0]}\"\t\"{obj[key]}\" {keyTokens[1]}{newLine}";
                        else
                            stringValue += $"{new string(tab, tabs)}\"{key}\"\t\"{obj[key]}\"{newLine}";
                    }
                }

            return stringValue;
        }
    }
}