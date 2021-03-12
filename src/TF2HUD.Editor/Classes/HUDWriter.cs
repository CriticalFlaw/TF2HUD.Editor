using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TF2HUD.Editor.Classes
{
    /// <summary>
    ///     Writes a HUD folders/files object properties to a HUD
    /// </summary>
    public static class HUDWriter
    {
        // Writes a Dictionary of HUD folders, files, elements, values into a HUD folder, using a provided set of values
        // Example:
        //
        // compiler.json:
        // {
        //     "resource": {
        //         "ui": {
        //             "hudplayerhealth.res": {
        //                 "PlayerStatusHealthValue": {
        //                     "xpos": "$test_hud_health_xpos",
        //                     "ypos": "$test_hud_bold_font ? HudFontBold : HudFontRegular"
        //                 }
        //             }
        //         }
        //     }
        // }
        //
        // Usage:
        //
        // string location = "C:\Program Files (x86)\Steam\steamapps\common\Team Fortress 2\tf\custom\test_hud"
        // Dictionary<string, dynamic> Options = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(File.ReadAllText("compiler.json"));
        // Dictionary<string, string> InputIDs = new()
        // {
        //     test_hud_health_xpos = "c-50",
        //     test_hud_bold_font   = "true",
        // }
        // HUDWriter.Write(location, Options, InputIDs)'
        //
        public static void Write(string HUDFolderPath, Dictionary<string, dynamic> Options,
            Dictionary<string, string> InputValues)
        {
            // Iterate Dictionary and invoke relevant method for each item
            void IterateFolder(Dictionary<string, dynamic> Folder, string FolderPath)
            {
                foreach (var Item in Folder.Keys)
                    if (Folder[Item].GetType() == typeof(Dictionary<string, dynamic>))
                    {
                        if (!Item.Contains("."))
                            IterateFolder(Folder[Item], FolderPath + "\\" + Item);
                        else
                            WriteOptionsToFile(FolderPath + "\\" + Item, Folder[Item]);
                    }
            }

            // Read HUD file, find main panel element (Resource/UI/File.res), then recurse both
            // the HUD files elements AND the option elements at the same time
            void WriteOptionsToFile(string FilePath, Dictionary<string, dynamic> FileElements)
            {
                if (!File.Exists(FilePath))
                    throw new Exception($"Cannot write options to file {FilePath} because it doesn't exist!");

                var obj = VDF.Parse(File.ReadAllText(FilePath));
                Dictionary<string, dynamic> elements = null;
                var index = 0;
                var len = obj.Keys.Count;

                // Obtain first complex value that is not #base
                while (elements == null && index < len)
                {
                    if (obj.Values.ElementAt(index).GetType() == typeof(Dictionary<string, dynamic>) &&
                        obj.Keys.ElementAt(index) != "#base")
                        elements = obj.Values.ElementAt(index);
                    index++;
                }

                // Ensure HUD Elements object found
                if (elements == null)
                    throw new Exception($"Could not find HUD Elements in {FilePath}!");

                void IterateHUDElement(Dictionary<string, dynamic> elementOptions, Dictionary<string, dynamic> element)
                {
                    foreach (var elementProperty in elementOptions.Keys)
                        if (elementOptions[elementProperty].GetType() == typeof(Dictionary<string, dynamic>))
                        {
                            if (!element.ContainsKey(elementProperty))
                                throw new Exception($"Cannot find element {elementProperty} in {FilePath}!");

                            if (element[elementProperty].GetType() != typeof(Dictionary<string, dynamic>))
                                throw new Exception(
                                    $"Cannot iterate element {elementProperty} in {FilePath} because it is not a Dictionary<string, dynamic>!");

                            IterateHUDElement(elementOptions[elementProperty], element[elementProperty]);
                        }
                        else
                        {
                            if (!elementOptions.ContainsKey(elementProperty))
                                throw new Exception($"Cannot find {elementProperty} in {FilePath}");

                            string value = elementOptions[elementProperty];

                            // Test if value matches a ternary statement
                            // (only compares items with dictionary boolean, does not evaluate expressions)
                            if (Regex.IsMatch(value, @"\$.* ? .* : .*"))
                            {
                                var matches = Regex.Matches(value, @"[^\$?: ]+");
                                var inputID = matches[0].ToString();
                                var trueValue = matches[1].ToString();
                                var falseValue = matches[2].ToString();

                                if (InputValues[inputID] == "true")
                                    element[elementProperty] = trueValue;
                                else
                                    element[elementProperty] = falseValue;
                            }
                            // Use input value directly
                            else if (value.StartsWith("$"))
                            {
                                var inputID = value.Substring(1, value.Length - 1);
                                element[elementProperty] = InputValues[inputID];
                            }
                            else
                            {
                                element[elementProperty] = value;
                            }
                        }
                }

                IterateHUDElement(FileElements, elements);

                File.WriteAllText(FilePath, VDF.Stringify(obj));
            }

            IterateFolder(Options, HUDFolderPath);
        }
    }
}