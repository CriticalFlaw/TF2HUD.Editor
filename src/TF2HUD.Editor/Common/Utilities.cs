using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Common
{
    public static class Utilities
    {
        /// <summary>
        ///     Get the line number of a given text value found in a string array.
        /// </summary>
        public static int FindIndex(string[] array, string value, int skip = 0)
        {
            var list = array.Skip(skip);
            var index = list.Select((v, i) => new {Index = i, Value = v}) // Pair up values and indexes
                .Where(p => p.Value.Contains(value)) // Do the filtering
                .Select(p => p.Index); // Keep the index and drop the value
            return index.First() + skip;
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
        public static string[] CommentOutTextLineSuper(string[] lines, string start, string query, bool commentOut)
        {
            var index1 = FindIndex(lines, query, FindIndex(lines, start));
            var index2 = FindIndex(lines, query, index1++);
            lines[index1] = commentOut ? lines[index1].Replace("//", string.Empty) : CommentOutTextLine(lines[index1]);
            lines[index2] = commentOut ? lines[index2].Replace("//", string.Empty) : CommentOutTextLine(lines[index2]);
            return lines;
        }

        /// <summary>
        ///     Convert  HEX code to an RGB value.
        /// </summary>
        /// <param name="hex">HEX code of the color to be converted to RGB.</param>
        /// <param name="alpha">Flag the color as having a lower alpha value than normal.</param>
        /// <param name="pulse">Flag the color as a pulse, slightly changing green channel.</param>
        public static string RgbConverter(string hex, int alpha = 255, bool pulse = false)
        {
            var color = ColorTranslator.FromHtml(hex);
            var pulseNew = pulse && color.G >= 50 ? color.G - 50 : color.G;
            return $"{color.R} {pulseNew} {color.B} {alpha}";
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

        public static void OpenLinkButton(HUDS hud, Links link)
        {
            var url = hud switch
            {
                HUDS.flawhud => link switch
                {
                    Links.Steam => Resources.url_flawhud_steam,
                    Links.GitHub => Resources.url_flawhud_github,
                    Links.hudsTF => Resources.url_flawhud_hudstf,
                    _ => throw new NotImplementedException()
                },
                HUDS.rayshud => link switch
                {
                    Links.Steam => Resources.url_rayshud_steam,
                    Links.GitHub => Resources.url_rayshud_github,
                    Links.hudsTF => Resources.url_rayshud_hudstf,
                    _ => throw new NotImplementedException()
                },
                _ => string.Empty
            };
            OpenWebpage(url);
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
}