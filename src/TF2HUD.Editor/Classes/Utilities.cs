using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using TF2HUD.Editor.JSON;

namespace TF2HUD.Editor.Classes
{
    public static class Utilities
    {
        /// <summary>
        ///     Get the line number of a given text value found in a string array.
        /// </summary>
        public static int FindIndex(string[] array, string value, int skip = 0)
        {
            try
            {
                var list = array.Skip(skip);
                var index = list.Select((v, i) => new {Index = i, Value = v}) // Pair up values and indexes
                    .Where(p => p.Value.Contains(value)) // Do the filtering
                    .Select(p => p.Index); // Keep the index and drop the value
                return index.First() + skip;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        ///     Clear all existing comment identifiers, then apply a fresh one.
        /// </summary>
        public static string CommentOutTextLine(string value)
        {
            return string.Concat("//", value.Replace("//", string.Empty));
        }

        /// <summary>
        ///     Clear all existing comment identifiers, then apply a fresh one.
        /// </summary>
        public static string UncommentOutTextLine(string value)
        {
            return value.Replace("//", string.Empty);
        }

        /// <summary>
        ///     Clear all existing comment identifiers, then apply a fresh one.
        /// </summary>
        public static List<int> GetStringIndexes(string[] lines, string text)
        {
            var indexList = new List<int>();
            for (var x = 0; x < lines.Length; x++)
                if (lines[x].Contains(text) || lines[x].Contains(text.Replace(" ", "\t")))
                    indexList.Add(x);
            return indexList;
        }

        /// <summary>
        ///     Convert  HEX code to an RGB value.
        /// </summary>
        /// <param name="hex">HEX code of the color to be converted to RGB.</param>
        /// <param name="alpha">Flag the color as having a lower alpha value than normal.</param>
        /// <param name="pulse">Flag the color as a pulse, slightly changing green channel.</param>
        public static string RgbConverter(string hex, bool pulse = false)
        {
            var color = ColorTranslator.FromHtml(hex);
            var pulseNew = pulse && color.G >= 50 ? color.G - 50 : color.G;
            return $"{color.R} {pulseNew} {color.B} {color.A}";
        }

        /// <summary>
        ///     Convert an Enum name to a user-friendly string value.
        /// </summary>
        public static string GetStringValue(Enum value)
        {
            // Get the type, FieldInfo for this type and StringValue attributes
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attributes =
                fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match, or enum value if no match
            return attributes.Length > 0 ? attributes[0].StringValue : value.ToString();
        }

        public static void OpenWebpage(string url)
        {
            try
            {
                // Attempt to open the issue tracker, if that fails, try other methods.
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        ///     Convert string value to a boolean
        /// </summary>
        public static bool ParseBool(string input)
        {
            return input.ToLower() switch
            {
                "yes" or "1" or "true" => true,
                _ => false
            };
        }

        public static void Merge(Dictionary<string, dynamic> Obj1, Dictionary<string, dynamic> Obj2)
        {
            foreach (var i in Obj1.Keys)
                if (Obj1[i].GetType().Name.Contains("Dictionary"))
                {
                    if (Obj2.ContainsKey(i) && Obj2[i].GetType().Name.Contains("Dictionary")) Merge(Obj1[i], Obj2[i]);
                }
                else
                {
                    if (Obj2.ContainsKey(i)) Obj1[i] = Obj2[i];
                }

            foreach (var j in Obj2.Keys)
                if (!Obj1.ContainsKey(j))
                    Obj1[j] = Obj2[j];
        }

        public static Dictionary<string, dynamic> CreateNestedObject(Dictionary<string, dynamic> Obj,
            IEnumerable<string> Keys)
        {
            var ObjectReference = Obj;
            foreach (var Key in Keys)
            {
                if (!ObjectReference.ContainsKey(Key))
                    ObjectReference[Key] = new Dictionary<string, dynamic>();
                ObjectReference = ObjectReference[Key];
            }

            return ObjectReference;
        }

        /// <summary>
        ///     Retrieve the filename from the HUD schema control using a string value.
        /// </summary>
        internal static string GetFileName(Dictionary<string, Controls[]>.ValueCollection controlGroups, string name)
        {
            foreach (var group in controlGroups)
            foreach (var control in @group.Where(x => x.FileName is not null))
                if (string.Equals(control.Name, name))
                    return control.FileName.Replace(".res", string.Empty);

            return null;
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class StringValueAttribute : Attribute
    {
        public StringValueAttribute(string value)
        {
            StringValue = value;
        }

        public string StringValue { get; protected set; }
    }

    public class ItemColorList
    {
        public string Assassin = "#D32CE6";
        public string Civilian = "#B0C3D9";
        public string Collectors = "#AA0000";
        public string Commando = "#8847FF";
        public string Community = "#70B04A";
        public string Elite = "#EB4B4B";
        public string Freelance = "#5E98D9";
        public string Genuine = "#4D7455";
        public string Haunted = "#38F3AB";
        public string Mercenary = "#4B69FF";
        public string Normal = "#B2B2B2";
        public string Strange = "#CF6A32";
        public string Unique = "#FFD700";
        public string Unusual = "#8650AC";
        public string Valve = "#A50F79";
        public string Vintage = "#476291";
    }

    /// <summary>
    ///     List of possible positions for item effect meters.
    /// </summary>
    public enum Positions
    {
        Top,
        Middle,
        Bottom,
        Default
    }
}