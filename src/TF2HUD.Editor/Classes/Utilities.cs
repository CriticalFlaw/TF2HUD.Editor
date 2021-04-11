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
        public static string GetPulsedColor(string rgba)
        {
            var colors = Array.ConvertAll(rgba.Split(' '), x => int.Parse(x));

            // Apply the pulse change and return the color.
            colors[^1] = colors[^1] >= 50 ? colors[^1] - 50 : colors[^1];
            return $"{colors[0]} {colors[1]} {colors[2]} {colors[^1]}";
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

        public static void OpenWebpage(string url)
        {
            Process.Start("explorer", url);
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
}