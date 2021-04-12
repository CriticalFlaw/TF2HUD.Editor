using System;
using System.Collections.Generic;
using System.Linq;

namespace TF2HUD.Editor.Classes
{
    internal static class VDF
    {
        public static Dictionary<string, dynamic> Parse(string text, string osTagDelimiter = "^")
        {
            var index = 0;
            char[] ignoredCharacters = {' ', '\t', '\r', '\n'};

            string Next(bool LookAhead = false)
            {
                var CurrentToken = "";
                var x = index;

                // Return EOF if we've reached the end of the text file.
                if (x >= text.Length - 1) return "EOF";

                // Discard any text that is preempted by a comment tag (//) until the next line.
                while ((ignoredCharacters.Contains(text[x]) || text[x] == '/') && x <= text.Length - 1)
                {
                    if (text[x] == '/')
                    {
                        if (text[x + 1] == '/')
                            while (text[x] != '\n')
                                x++;
                    }
                    else
                    {
                        x++;
                    }

                    if (x >= text.Length) return "EOF";
                }

                // If we encounter a quote, read the enclosed text until the next quotation mark.
                if (text[x] == '"')
                {
                    // Skip the opening quotation mark.
                    x++;

                    while (text[x] != '"' && x < text.Length)
                    {
                        if (text[x] == '\n') throw new Exception($"Unexpected end of line at position {x}");
                        CurrentToken += text[x];
                        x++;
                    }

                    // Skip the closing quotation mark.
                    x++;
                }
                else
                {
                    // Read the text until reaching whitespace or an end of the file.
                    while (!ignoredCharacters.Contains(text[x]) && x < text.Length - 1)
                    {
                        if (text[x] == '"') throw new Exception($"Unexpected double quote at position {x}");
                        CurrentToken += text[x];
                        x++;
                    }
                }

                if (!LookAhead) index = x;

                return CurrentToken;
            }

            Dictionary<string, dynamic> ParseObject()
            {
                Dictionary<string, dynamic> objectRef = new();

                var currentToken = Next();
                var nextToken = Next(true);

                while (currentToken != "}" && nextToken != "EOF")
                {
                    if (Next(true).StartsWith('['))
                    {
                        // Object with an OS tag
                        currentToken += $"{osTagDelimiter}{Next()}";
                        Next(); // Skip over opening brace
                        objectRef[currentToken] = ParseObject();
                    }
                    else if (nextToken == "{")
                    {
                        // Object
                        Next(); // Skip over opening brace

                        if (objectRef.TryGetValue(currentToken, out var Value))
                        {
                            if (objectRef[currentToken].GetType() == typeof(List<dynamic>))
                            {
                                // Object list exists
                                objectRef[currentToken].Add(ParseObject());
                            }
                            else
                            {
                                // Object already exists
                                objectRef[currentToken] = new List<dynamic>();
                                objectRef[currentToken].Add(Value);
                                objectRef[currentToken].Add(ParseObject());
                            }
                        }
                        else
                        {
                            // Object does not exist
                            objectRef[currentToken] = ParseObject();
                        }
                    }
                    else
                    {
                        // Primitive
                        Next(); // Skip over value

                        // Check primitive OS tag
                        if (Next(true).StartsWith('[')) currentToken += $"{osTagDelimiter}{Next()}";

                        if (objectRef.TryGetValue(currentToken, out var Value))
                        {
                            // dynamic property exists
                            if (objectRef[currentToken].GetType() == typeof(List<dynamic>))
                            {
                                // Array already exists
                                objectRef[currentToken].Add(nextToken);
                            }
                            else
                            {
                                // Primitive type already exists
                                objectRef[currentToken] = new List<dynamic>();
                                objectRef[currentToken].Add(Value);
                                objectRef[currentToken].Add(nextToken);
                            }
                        }
                        else
                        {
                            // Property doesn't exist
                            objectRef[currentToken] = nextToken;
                        }
                    }

                    currentToken = Next();
                    nextToken = Next(true);
                }

                return objectRef;
            }

            return ParseObject();
        }

        public static string Stringify(Dictionary<string, dynamic> Obj, int Tabs = 0)
        {
            var stringValue = "";
            const char tab = '\t';
            const string newLine = "\r\n";

            foreach (var Key in Obj.Keys)
                if (Obj[Key].GetType() == typeof(List<dynamic>))
                {
                    // Item has multiple instances
                    foreach (var item in Obj[Key])
                        if (item.GetType() == typeof(Dictionary<string, dynamic>))
                        {
                            // Check for an OS tag.
                            var KeyTokens = Key.Split('^');
                            if (KeyTokens.Length > 1)
                                stringValue += $"{new string(tab, Tabs)}\"{Key}\" {KeyTokens[1]}{newLine}";
                            else
                                stringValue += $"{new string(tab, Tabs)}{Key}{newLine}";

                            stringValue += $"{new string(tab, Tabs)}{{{newLine}";
                            stringValue += $"{Stringify(item, Tabs + 1)}{new string(tab, Tabs)}}}{newLine}";
                        }
                        else
                        {
                            // Check for an OS tag.
                            var KeyTokens = Key.Split('^');
                            if (KeyTokens.Length > 1)
                                stringValue += $"{new string(tab, Tabs)}\"{Key}\"\t\"{item}\" {KeyTokens[1]}{newLine}";
                            else
                                stringValue += $"{new string(tab, Tabs)}\"{Key}\"\t\"{item}\"{newLine}";
                        }
                }
                else
                {
                    // There is only one object object/value
                    if (Obj[Key] is IDictionary<string, dynamic>)
                    {
                        // Check for an OS tag.
                        var KeyTokens = Key.Split('^');
                        if (KeyTokens.Length > 1)
                        {
                            stringValue += $"{new string(tab, Tabs)}\"{KeyTokens[0]}\" {KeyTokens[1]}{newLine}";
                            stringValue += $"{new string(tab, Tabs)}{{{newLine}";
                            stringValue += $"{Stringify(Obj[Key], Tabs + 1)}{new string(tab, Tabs)}}}{newLine}";
                        }
                        else
                        {
                            stringValue += $"{new string(tab, Tabs)}\"{Key}\"{newLine}";
                            stringValue += $"{new string(tab, Tabs)}{{{newLine}";
                            stringValue += $"{Stringify(Obj[Key], Tabs + 1)}{new string(tab, Tabs)}}}{newLine}";
                        }
                    }
                    else
                    {
                        // Check for an OS tag.
                        var KeyTokens = Key.Split('^');
                        if (KeyTokens.Length > 1)
                            stringValue +=
                                $"{new string(tab, Tabs)}\"{KeyTokens[0]}\"\t\"{Obj[Key]}\" {KeyTokens[1]}{newLine}";
                        else
                            stringValue += $"{new string(tab, Tabs)}\"{Key}\"\t\"{Obj[Key]}\"{newLine}";
                    }
                }

            return stringValue;
        }
    }
}