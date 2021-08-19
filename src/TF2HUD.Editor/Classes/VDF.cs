using System;
using System.Collections.Generic;
using System.Linq;

namespace HUDEditor.Classes
{
    internal static class VDF
    {
        public static Dictionary<string, dynamic> Parse(string text, string osTagDelimiter = "^")
        {
            var index = 0;
            char[] ignoredCharacters = { ' ', '\t', '\r', '\n' };

            string Next(bool lookAhead = false)
            {
                var currentToken = "";
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
                        currentToken += text[x];
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
                        currentToken += text[x];
                        x++;
                    }
                }

                if (!lookAhead) index = x;

                return currentToken;
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

                        if (objectRef.TryGetValue(currentToken, out var value))
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
                                objectRef[currentToken].Add(value);
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

                        if (objectRef.TryGetValue(currentToken, out var value))
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
                                objectRef[currentToken].Add(value);
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
                            // Check for an OS tag.
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
                            // Check for an OS tag.
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
                        // Check for an OS tag.
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
                        // Check for an OS tag.
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