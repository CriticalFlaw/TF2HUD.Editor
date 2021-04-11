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
        public static List<Tuple<string, string, string>> itemRarities = new()
        {
            new Tuple<string, string, string>("QualityColorNormal", "DimmQualityColorNormal",
                "QualityColorNormal_GreyedOut"),
            new Tuple<string, string, string>("QualityColorUnique", "DimmQualityColorUnique",
                "QualityColorUnique_GreyedOut"),
            new Tuple<string, string, string>("QualityColorStrange", "DimmQualityColorStrange",
                "QualityColorStrange_GreyedOut"),
            new Tuple<string, string, string>("QualityColorVintage", "DimmQualityColorVintage",
                "QualityColorVintage_GreyedOut"),
            new Tuple<string, string, string>("QualityColorHaunted", "DimmQualityColorHaunted",
                "QualityColorHaunted_GreyedOut"),
            new Tuple<string, string, string>("QualityColorrarity1", "DimmQualityColorrarity1",
                "QualityColorrarity1_GreyedOut"),
            new Tuple<string, string, string>("QualityColorCollectors", "DimmQualityColorCollectors",
                "QualityColorCollectors_GreyedOut"),
            new Tuple<string, string, string>("QualityColorrarity4", "DimmQualityColorrarity4",
                "QualityColorrarity4_GreyedOut"),
            new Tuple<string, string, string>("QualityColorCommunity", "DimmQualityColorCommunity",
                "QualityColorCommunity_GreyedOut"),
            new Tuple<string, string, string>("QualityColorDeveloper", "DimmQualityColorDeveloper",
                "QualityColorDeveloper_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityCommon", "DimmItemRarityCommon", "ItemRarityCommon_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityUncommon", "DimmItemRarityUncommon",
                "ItemRarityUncommon_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityRare", "DimmItemRarityRare", "ItemRarityRare_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityMythical", "DimmItemRarityMythical",
                "ItemRarityMythical_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityLegendary", "DimmItemRarityLegendary",
                "ItemRarityLegendary_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityAncient", "DimmItemRarityAncient",
                "ItemRarityAncient_GreyedOut")
        };

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
        ///     Convert HEX code to an RGBA value.
        /// </summary>
        /// <param name="hex">HEX code of the color to be converted to RGBA.</param>
        public static string RgbaConverter(string hex)
        {
            var color = ColorTranslator.FromHtml(hex);
            return $"{color.R} {color.G} {color.B} {color.A}";
        }

        /// <summary>
        ///     Get a pulsed color by reducing a given color channel by 50.
        /// </summary>
        /// <param name="rgba">RGBA color that will have its alpha value reduced.</param>
        /// <param name="index">Index value for a color in the RGBA to "pulse".</param>
        public static string GetPulsedColor(string rgba, int index = 1)
        {
            var colors = Array.ConvertAll(rgba.Split(' '), x => int.Parse(x));

            // If the green channel cannot be reduced, try blue then red. If there's no candidate, return the original color.
            if (colors[index] < 50)
            {
                if (colors[index + 1] > 50)
                    index++;
                else if (colors[index - 1] > 50)
                    index--;
            }

            // Apply the pulse change and return the color.
            colors[index] = colors[index] >= 50 ? colors[index] - 50 : colors[index];
            return $"{colors[0]} {colors[1]} {colors[2]} {colors[3]}";
        }

        /// <summary>
        ///     Get a dimmed color by reducing the alpha channel to 100.
        /// </summary>
        /// <param name="rgba">RGBA color that will have its alpha value reduced.</param>
        public static string GetDimmedColor(string rgba)
        {
            var colors = Array.ConvertAll(rgba.Split(' '), x => int.Parse(x));

            // Return the color with a reduced alpha channel.
            return $"{colors[0]} {colors[1]} {colors[2]} 100";
        }

        /// <summary>
        ///     Get a grayed color by reducing each color channel by 75%.
        /// </summary>
        /// <param name="rgba">RGBA color that will have its alpha value reduced.</param>
        public static string GetGrayedColor(string rgba)
        {
            var colors = Array.ConvertAll(rgba.Split(' '), x => int.Parse(x));

            // Reduce each color channel (except alpha) by 75%, then return the color.
            for (var x = 0; x < colors.Length; x++)
                colors[x] = Convert.ToInt32(colors[x] * 0.25);
            return $"{colors[0]} {colors[1]} {colors[2]} 255";
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
            Process.Start("explorer", url);
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
                if (Obj1[i].GetType() == typeof(Dictionary<string, dynamic>))
                {
                    if (Obj2.ContainsKey(i) && Obj2[i].GetType() == typeof(Dictionary<string, dynamic>)) Merge(Obj1[i], Obj2[i]);
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
        internal static dynamic GetFileNames(Controls control)
        {
            if (!string.IsNullOrWhiteSpace(control.FileName)) return control.FileName.Replace(".res", string.Empty);

            if (control.ComboFiles is not null) return control.ComboFiles;

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