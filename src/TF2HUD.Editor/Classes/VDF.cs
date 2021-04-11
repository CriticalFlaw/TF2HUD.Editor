using System;
using System.Collections.Generic;
using System.Linq;

namespace TF2HUD.Editor.Classes
{
    internal static class VDF
    {
        public static Dictionary<string, dynamic> Parse(string Str, string OSTagDelimeter = "^")
        {
            var i = 0;
            char[] WhiteSpaceIgnore = {' ', '\t', '\r', '\n'};

            string Next(bool LookAhead = false)
            {
                var CurrentToken = "";
                var j = i;

                if (j >= Str.Length - 1) return "EOF";

                while ((WhiteSpaceIgnore.Contains(Str[j]) || Str[j] == '/') && j <= Str.Length - 1)
                {
                    if (Str[j] == '/')
                    {
                        if (Str[j + 1] == '/')
                            while (Str[j] != '\n')
                                j++;
                    }
                    else
                    {
                        j++;
                    }

                    if (j >= Str.Length) return "EOF";
                }

                if (Str[j] == '"')
                {
                    // Read until next quote (ignore opening quote)
                    j++;
                    while (Str[j] != '"' && j < Str.Length)
                    {
                        if (Str[j] == '\n') throw new Exception($"Unexpected end of line at position {j}");
                        CurrentToken += Str[j];
                        j++;
                    }

                    j++; // Skip over closing quote
                }
                else
                {
                    // Read until whitespace (or end of file)
                    while (!WhiteSpaceIgnore.Contains(Str[j]) && j < Str.Length - 1)
                    {
                        if (Str[j] == '"') throw new Exception($"Unexpected double quote at position {j}");
                        CurrentToken += Str[j];
                        j++;
                    }
                }

                if (!LookAhead) i = j;

                //if (j > Str.Length)
                //{
                //	return "EOF";
                //}

                return CurrentToken;
            }

            Dictionary<string, dynamic> ParseObject()
            {
                Dictionary<string, dynamic> Obj = new();

                var CurrentToken = Next();
                var NextToken = Next(true);

                while (CurrentToken != "}" && NextToken != "EOF")
                {
                    if (Next(true).StartsWith('['))
                    {
                        // Object with OS Tag
                        CurrentToken += $"{OSTagDelimeter}{Next()}";
                        Next(); // Skip over opening brace
                        Obj[CurrentToken] = ParseObject();
                    }
                    else if (string.Equals(NextToken, "{"))
                    {
                        // Object
                        Next(); // Skip over opening brace

                        if (Obj.TryGetValue(CurrentToken, out var Value))
                        {
                            if (Obj[CurrentToken].GetType() == typeof(List<dynamic>))
                            {
                                // Object list exists
                                Obj[CurrentToken].Add(ParseObject());
                            }
                            else
                            {
                                // Object already exists
                                Obj[CurrentToken] = new List<dynamic>();
                                Obj[CurrentToken].Add(Value);
                                Obj[CurrentToken].Add(ParseObject());
                            }
                        }
                        else
                        {
                            // Object doesnt exist
                            Obj[CurrentToken] = ParseObject();
                        }
                    }
                    else
                    {
                        // Primitive

                        Next(); // Skip over value

                        // Check primitive os tag
                        if (Next(true).StartsWith('[')) CurrentToken += $"{OSTagDelimeter}{Next()}";

                        if (Obj.TryGetValue(CurrentToken, out var Value))
                        {
                            // dynamic property exists
                            if (Obj[CurrentToken].GetType() == typeof(List<dynamic>))
                            {
                                // Array already exists
                                Obj[CurrentToken].Add(NextToken);
                            }
                            else
                            {
                                // Primitive type already exists
                                Obj[CurrentToken] = new List<dynamic>();
                                Obj[CurrentToken].Add(Value);
                                Obj[CurrentToken].Add(NextToken);
                            }
                        }
                        else
                        {
                            // Property doesn't exist
                            Obj[CurrentToken] = NextToken;
                        }
                    }

                    CurrentToken = Next();
                    NextToken = Next(true);
                }

                return Obj;
            }

            return ParseObject();
        }

        public static string Stringify(Dictionary<string, dynamic> Obj, int Tabs = 0)
        {
            var Str = "";
            var Tab = '\t';
            var NewLine = "\r\n";
            foreach (var Key in Obj.Keys)
                if (Obj[Key].GetType() == typeof(List<dynamic>))
                {
                    // Item has multiple instances
                    foreach (var Item in Obj[Key])
                        if (Item.GetType() == typeof(Dictionary<string, dynamic>))
                        {
                            var KeyTokens = Key.Split('^');
                            if (KeyTokens.Length > 1)
                                // OS Tag
                                Str += $"{new string(Tab, Tabs)}\"{Key}\" {KeyTokens[1]}{NewLine}";
                            else
                                // No OS Tag
                                Str += $"{new string(Tab, Tabs)}{Key}{NewLine}";
                            Str += $"{new string(Tab, Tabs)}{{{NewLine}";
                            Str += $"{Stringify(Item, Tabs + 1)}{new string(Tab, Tabs)}}}{NewLine}";
                        }
                        else
                        {
                            var KeyTokens = Key.Split('^');
                            if (KeyTokens.Length > 1)
                                // OS Tag
                                Str += $"{new string(Tab, Tabs)}\"{Key}\"\t\"{Item}\" {KeyTokens[1]}{NewLine}";
                            else
                                // No OS Tag
                                Str += $"{new string(Tab, Tabs)}\"{Key}\"\t\"{Item}\"{NewLine}";
                        }
                }
                else
                {
                    // There is only one object object/value
                    if (Obj[Key] is IDictionary<string, dynamic>)
                    {
                        var KeyTokens = Key.Split('^');
                        if (KeyTokens.Length > 1)
                        {
                            Str += $"{new string(Tab, Tabs)}\"{KeyTokens[0]}\" {KeyTokens[1]}{NewLine}";
                            Str += $"{new string(Tab, Tabs)}{{{NewLine}";
                            Str += $"{Stringify(Obj[Key], Tabs + 1)}{new string(Tab, Tabs)}}}{NewLine}";
                        }
                        else
                        {
                            // No OS Tag
                            Str += $"{new string(Tab, Tabs)}\"{Key}\"{NewLine}";
                            Str += $"{new string(Tab, Tabs)}{{{NewLine}";
                            Str += $"{Stringify(Obj[Key], Tabs + 1)}{new string(Tab, Tabs)}}}{NewLine}";
                        }
                    }
                    else
                    {
                        var KeyTokens = Key.Split('^');
                        if (KeyTokens.Length > 1)
                            // OS Tag
                            Str += $"{new string(Tab, Tabs)}\"{KeyTokens[0]}\"\t\"{Obj[Key]}\" {KeyTokens[1]}{NewLine}";
                        else
                            // No OS Tag
                            Str += $"{new string(Tab, Tabs)}\"{Key}\"\t\"{Obj[Key]}\"{NewLine}";
                    }
                }

            return Str;
        }
    }
}