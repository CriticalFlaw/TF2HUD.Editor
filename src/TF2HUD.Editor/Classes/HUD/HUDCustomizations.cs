using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using HUDEditor.Models;
using HUDEditor.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HUDEditor.Classes
{
    public partial class HUD
    {
        /// <summary>
        ///     Apply user selected customizations to the HUD files.
        /// </summary>
        public bool ApplyCustomizations()
        {
            try
            {
                // HUD Background Image.
                // Set the HUD Background image path when applying, because it's possible
                // the user did not have their tf/custom folder set up when this HUD constructor was called.
                HudBackground = new HUDBackground($"{MainWindow.HudPath}\\{Name}\\");

                var hudSettings = ControlOptions.Values;
                // var hudSettings = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText($"JSON//{Name}.json")).Controls.Values;

                // If the developer defined customization folders for their HUD, then copy those files.
                if (!string.IsNullOrWhiteSpace(CustomizationsFolder))
                    MoveCustomizationFiles(hudSettings);

                // This Dictionary contains folders/files/properties as they should be written to the hud.
                // 'IterateFolder' and 'IterateHUDFileProperties' will write the properties to this.
                var hudFolders = new Dictionary<string, dynamic>();

                foreach (var group in hudSettings)
                foreach (var control in group)
                {
                    var setting = Settings.GetSetting(control.Name);
                    if (setting is null) continue;
                    WriteToFile(control, setting, hudFolders);
                }

                static void IterateProperties(Dictionary<string, dynamic> folder, string folderPath)
                {
                    foreach (var property in folder.Keys)
                        if (folder[property].GetType() == typeof(Dictionary<string, dynamic>))
                        {
                            if (property.Contains("."))
                            {
                                var filePath = folderPath + "\\" + property;
                                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                                // Read file, check each topmost element until we come to an element that matches
                                // the pattern (Resource/UI/HudFile.res) which indicates it's a HUD ui file
                                // if it IS a ui file, create a Dictionary to contain the elements specified in
                                // 'folder[property]', then merge the 2 Dictionaries.
                                // if the file has no matching top level elements, VDF.Stringify directly
                                // -Revan

                                if (File.Exists(filePath))
                                {
                                    var obj = VDF.Parse(File.ReadAllText(filePath));

                                    // Initialize to null to check whether matching element has been found
                                    Dictionary<string, dynamic> hudContainer = null;
                                    const string pattern = "(Resource/UI/)*.res";

                                    int preventInfinite = 0, len = obj.Keys.Count;
                                    while (hudContainer is null && preventInfinite < len)
                                    {
                                        var key = obj.Keys.ElementAt(preventInfinite);

                                        // Match pattern here, also ensure item is a HUD element
                                        if (Regex.IsMatch(key, pattern) &&
                                            obj[key].GetType() == typeof(Dictionary<string, dynamic>))
                                            // Initialise hudContainer and create inner Dictionary
                                            //  to contain elements specified
                                            hudContainer = new Dictionary<string, dynamic> { [key] = folder[property] };

                                        preventInfinite++;
                                    }

                                    if (hudContainer is not null)
                                    {
                                        Utilities.Merge(obj, hudContainer);
                                        File.WriteAllText(filePath, VDF.Stringify(obj));
                                    }
                                    else
                                    {
                                        // Write folder[property] to hud file
                                        Utilities.Merge(obj, folder[property]);
                                        File.WriteAllText(filePath, VDF.Stringify(obj));
                                    }
                                }
                                else
                                {
                                    File.WriteAllText(filePath, VDF.Stringify(folder[property]));
                                }
                            }
                            else
                            {
                                IterateProperties(folder[property], folderPath + "\\" + property);
                            }
                        }
                }

                // write hudFolders to the HUD once instead of each WriteToFile call reading and writing
                var hudPath = MainWindow.HudPath + "\\" + Name;
                IterateProperties(hudFolders, hudPath);
                HudBackground.Apply();
                return true;
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        ///     Copy files used for folder-based customizations.
        /// </summary>
        public bool MoveCustomizationFiles(Dictionary<string, Controls[]>.ValueCollection hudSettings)
        {
            try
            {
                // Check if the customization folder exist.
                var path = $"{MainWindow.HudPath}\\{Name}\\";
                if (!Directory.Exists($"{path}\\{CustomizationsFolder}")) return true;

                // Check if the "enabled" folder exists. If not, create it.
                if (!Directory.Exists($"{path}\\{EnabledFolder}"))
                    Directory.CreateDirectory($"{path}\\{EnabledFolder}");

                // Get user's settings for the selected HUD.
                var userSettings = JsonConvert.DeserializeObject<UserJson>(File.ReadAllText(HUDSettings.UserFile))
                    ?.Settings.Where(x => x.HUD == Name);

                foreach (var group in hudSettings)
                foreach (var control in group)
                {
                    // Loop through every control on the page, find the matching user setting.
                    var setting = userSettings.FirstOrDefault(x => x.Name == control.Name);
                    if (setting is null) continue; // User setting not found, skipping.

                    var custom = path + CustomizationsFolder;
                    var enabled = path + EnabledFolder;

                    switch (control.Type.ToLowerInvariant())
                    {
                        case "checkbox":
                            if (control.RenameFile is not null)
                            {
                                if (control.RenameFile.OldName.EndsWith('/'))
                                {
                                    if (Directory.Exists(path + control.RenameFile.NewName))
                                        Directory.Move(path + control.RenameFile.NewName,
                                            path + control.RenameFile.OldName);

                                    if (string.Equals(setting.Value, "true", StringComparison.CurrentCultureIgnoreCase))
                                        Directory.Move(path + control.RenameFile.OldName,
                                            path + control.RenameFile.NewName);
                                }
                                else
                                {
                                    if (File.Exists(path + control.RenameFile.NewName))
                                        File.Move(path + control.RenameFile.NewName, path + control.RenameFile.OldName);

                                    if (string.Equals(setting.Value, "true", StringComparison.CurrentCultureIgnoreCase))
                                        File.Move(path + control.RenameFile.OldName, path + control.RenameFile.NewName);
                                }
                            }

                            var fileName = Utilities.GetFileNames(control);
                            if (fileName is null or not string) continue; // File name not found, skipping.

                            custom += $"\\{fileName}";
                            enabled += $"\\{fileName}";

                            // If true, move the customization file into the enabled folder, otherwise move it back.
                            if (string.Equals(setting.Value, "true", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (Directory.Exists(custom))
                                    Directory.Move(custom, enabled);
                                else if (File.Exists(custom + ".res"))
                                    File.Move(custom + ".res", enabled + ".res", true);
                            }
                            else
                            {
                                if (Directory.Exists(enabled))
                                    Directory.Move(enabled, custom);
                                else if (File.Exists(enabled + ".res"))
                                    File.Move(enabled + ".res", custom + ".res", true);
                            }

                            break;

                        case "dropdown":
                        case "dropdownmenu":
                        case "select":
                        case "combobox":
                            foreach (var option in control.Options.Where(x => x.RenameFile is not null))
                                if (option.RenameFile.OldName.EndsWith('/'))
                                {
                                    if (Directory.Exists(path + option.RenameFile.NewName))
                                        Directory.Move(path + option.RenameFile.NewName,
                                            path + option.RenameFile.OldName);

                                    if (string.Equals(option.Value, setting.Value))
                                        Directory.Move(path + option.RenameFile.OldName,
                                            path + option.RenameFile.NewName);
                                }
                                else
                                {
                                    if (File.Exists(path + option.RenameFile.NewName))
                                        File.Move(path + option.RenameFile.NewName, path + option.RenameFile.OldName);

                                    if (string.Equals(option.Value, setting.Value))
                                        File.Move(path + option.RenameFile.OldName, path + option.RenameFile.NewName);
                                }

                            var fileNames = Utilities.GetFileNames(control);
                            if (fileNames is null or not string[]) continue; // File names not found, skipping.

                            // Move every file assigned to this control back to the customization folder first.
                            foreach (string file in fileNames)
                            {
                                var name = file.Replace(".res", string.Empty);
                                if (Directory.Exists(enabled + $"\\{name}"))
                                    Directory.Move(enabled + $"\\{name}", custom + $"\\{name}");
                                else if (File.Exists(enabled + $"\\{name}.res"))
                                    File.Move(enabled + $"\\{name}.res", custom + $"\\{name}.res", true);
                            }

                            // Only move the files for the control option selected by the user.
                            if (!string.Equals(setting.Value, "0"))
                            {
                                var name = control.Options[int.Parse(setting.Value)].FileName;
                                if (string.IsNullOrWhiteSpace(name)) break;

                                name = name.Replace(".res", string.Empty);
                                if (Directory.Exists(custom + $"\\{name}"))
                                    Directory.Move(custom + $"\\{name}", enabled + $"\\{name}");
                                else if (File.Exists(custom + $"\\{name}.res"))
                                    File.Move(custom + $"\\{name}.res", enabled + $"\\{name}.res", true);
                            }

                            break;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        ///     Write user selected options to HUD files.
        /// </summary>
        /// <param name="hudSetting">Settings as defined for the HUD</param>
        /// <param name="userSetting">Settings as selected by the user</param>
        /// <param name="hudFolders">folders/files/properties Dictionary to write HUD properties to</param>
        private void WriteToFile(Controls hudSetting, Setting userSetting, Dictionary<string, dynamic> hudFolders)
        {
            try
            {
                MainWindow.Logger.Info($"Applying: {userSetting.Name}");
                var (files, special, specialParameters) = GetControlInfo(hudSetting, userSetting);

                string EvaluateValue(string input)
                {
                    var result = "";
                    // Match ternary statement ($word, question mark, word, colon, then word)
                    // all with 0 or more whitespace in-between.
                    // example: "$value ? ON : OFF"
                    if (Regex.IsMatch(input, "^\\$\\w+\\s*\\?\\s*\\w+\\s*:\\s*\\w+$"))
                    {
                        var tokens = Regex.Matches(input, "\\w+");
                        var checkBoxChecked = tokens[0].Value == "value"
                            ? string.Equals(userSetting.Value, "True", StringComparison.CurrentCultureIgnoreCase)
                            : Settings.GetSetting<bool>(tokens[0].Value);
                        return checkBoxChecked ? tokens[1].Value : tokens[2].Value;
                    }

                    // Iterate through matches and evaluate expressions wrapped in curly braces
                    // example: Size: $value | Outline:{$fh_val_xhair_outline ? ON : OFF}
                    var index = 0;
                    foreach (Match match in Regex.Matches(input, "{.+?}"))
                    {
                        result += input[index..match.Index];
                        result += EvaluateValue(match.Value[1..^1]);
                        index = match.Index + match.Value.Length;
                    }

                    result += input[index..];
                    return result.Replace("$value", userSetting.Value);
                }

                // Check for special cases like stock or custom backgrounds.
                if (special is not null)
                {
                    // Assume the value of any customization that references 'special' is a bool
                    var enable = string.Equals(userSetting.Value, "True", StringComparison.CurrentCultureIgnoreCase);

                    // If the control is a ComboBox, compare the user value against the default item index.
                    if (string.Equals(userSetting.Type, "ComboBox", StringComparison.CurrentCultureIgnoreCase))
                        enable = !string.Equals(userSetting.Value, "0");

                    EvaluateSpecial(special, userSetting, enable, specialParameters);
                }

                if (files is null) return;

                // Applies $value (and handles keywords where applicable) to provided HUD element
                // JObject and returns a HUD element Dictionary, recursively
                Dictionary<string, dynamic> CompileHudElement(JObject element,
                    string absolutePath, string relativePath, Dictionary<string, dynamic> hudElementRef,
                    string objectPath)
                {
                    var hudElement = new Dictionary<string, dynamic>();
                    foreach (var property in element)
                        if (string.Equals(property.Key, "replace", StringComparison.CurrentCultureIgnoreCase))
                        {
                            var values = property.Value.ToArray();

                            string find, replace;
                            if (string.Equals(userSetting.Value, "true", StringComparison.CurrentCultureIgnoreCase))
                            {
                                find = values[0].ToString();
                                replace = values[1].ToString();
                            }
                            else
                            {
                                find = values[1].ToString();
                                replace = values[0].ToString();
                            }

                            MainWindow.Logger.Info($"Replace value \"{find}\" with \"{replace}\".");
                            File.WriteAllText(absolutePath, File.ReadAllText(absolutePath).Replace(find, replace));
                        }
                        else if (string.Equals(property.Key, "#base", StringComparison.OrdinalIgnoreCase))
                        {
                            var output = "";
                            if (property.Value.ToString().Contains("true"))
                                output = property.Value.ToArray().Aggregate(output, (current, value) => current +
                                    (string.Equals(userSetting.Value, "true", StringComparison.CurrentCultureIgnoreCase)
                                        ? "#base " + value.First.First() + "\r\n"
                                        : "#base " + value.Last.First() + "\r\n"));
                            else
                                output += "#base " + property.Value + "\n";

                            File.WriteAllText(absolutePath, output);
                        }
                        else if (property.Value?.GetType() == typeof(JObject))
                        {
                            var currentObj = property.Value.ToObject<JObject>();

                            if (currentObj.ContainsKey("true") && currentObj.ContainsKey("false"))
                            {
                                hudElement[property.Key] = currentObj[userSetting.Value.ToLowerInvariant()];
                                MainWindow.Logger.Info($"Set \"{property.Key}\" to \"{userSetting.Value}\".");
                            }
                            else
                            {
                                MainWindow.Logger.Info(property.Key);
                                var newhudElementRef = hudElementRef.ContainsKey(property.Key)
                                    ? (Dictionary<string, dynamic>)hudElementRef[property.Key]
                                    : new Dictionary<string, dynamic>();
                                hudElement[property.Key] = CompileHudElement(currentObj,
                                    absolutePath, relativePath, newhudElementRef,
                                    $"{objectPath}{property.Key}/");
                            }
                        }
                        else
                        {
                            if (string.Equals(userSetting.Type, "ColorPicker",
                                StringComparison.CurrentCultureIgnoreCase))
                            {
                                // If the color is supposed to have a pulse, set the pulse value in the schema.
                                if (hudSetting.Pulse)
                                {
                                    var pulseKey = property.Key + "Pulse";
                                    hudElement[pulseKey] = Utilities.GetPulsedColor(userSetting.Value);
                                }
                                // If the color is supposed to have a pulse, set the pulse value in the schema.
                                else if (hudSetting.Shadow)
                                {
                                    var pulseKey = property.Key + "Shadow";
                                    hudElement[pulseKey] = Utilities.GetShadowColor(userSetting.Value);
                                }

                                // If the color value is for an item rarity, update the dimmed and grayed values.
                                foreach (var (item1, item2, item3) in Utilities.ItemRarities)
                                {
                                    if (!string.Equals(property.Key, item1)) continue;
                                    hudElement[item2] = Utilities.GetDimmedColor(userSetting.Value);
                                    hudElement[item3] = Utilities.GetGrayedColor(userSetting.Value);
                                }
                            }

                            // Check for already existing keys and warn user
                            if (hudElementRef.ContainsKey(property.Key))
                                MainWindow.Logger.Warn(
                                    $"{relativePath} => {objectPath} already contains key {property.Key}!");

                            hudElement[property.Key] = EvaluateValue(property.Value.ToString());
                            MainWindow.Logger.Info($"Set \"{property.Key}\" to \"{userSetting.Value}\".");
                        }

                    return hudElement;
                }

                // # Applies animation options to .txt file and handles keywords where applicable.
                //
                // This method takes a JObject of type <string, tuple | Animation> and applies
                // each keyword to the provided .txt file
                //
                // keywords:
                //     replace    takes a tuple of [true, false] values, evaluates $value and
                //                replaces text in the file
                //
                //     comment    takes an array of strings, and adds two forward slashes before
                //                each line that contains the any of the strings
                //
                //     uncomment  takes an array of strings, and removes te two forward slashes
                //                before each line that contains the any of the strings
                //
                // If the JObject property does not match a keyword, it is assumed to be an event
                // name and List of HUD Animations, in which case the method will parse the animation
                // file and overwrite the provided event animations with the JObject property's event
                // animations
                //
                void WriteAnimationCustomizations(string filePath, JObject animationOptions)
                {
                    HUDAnimation CreateAnimation(Dictionary<string, dynamic> animation,
                        KeyValuePair<string, JToken> animationOption)
                    {
                        return animation?["Type"].ToString().ToLower() switch
                        {
                            "animate" => new Animate
                            {
                                Type = "Animate",
                                Element = EvaluateValue(animation["Element"]),
                                Property = EvaluateValue(animation["Property"]),
                                Value = EvaluateValue(animation["Value"]),
                                Interpolator = EvaluateValue(animation["Interpolator"]),
                                Delay = EvaluateValue(animation["Delay"]),
                                Duration = EvaluateValue(animation["Duration"])
                            },
                            "runevent" => new RunEvent
                            {
                                Type = "RunEvent",
                                Event = EvaluateValue(animation["Event"]),
                                Delay = EvaluateValue(animation["Delay"])
                            },
                            "stopevent" => new StopEvent
                            {
                                Type = "StopEvent",
                                Event = EvaluateValue(animation["Event"]),
                                Delay = EvaluateValue(animation["Delay"])
                            },
                            "setvisible" => new SetVisible
                            {
                                Type = "StopEvent",
                                Element = EvaluateValue(animation["Element"]),
                                Delay = EvaluateValue(animation["Delay"]),
                                Duration = EvaluateValue(animation["Duration"])
                            },
                            "firecommand" => new FireCommand
                            {
                                Type = "FireCommand",
                                Delay = EvaluateValue(animation["Delay"]),
                                Command = EvaluateValue(animation["Command"])
                            },
                            "runeventchild" => new RunEventChild
                            {
                                Type = "RunEventChild",
                                Element = EvaluateValue(animation["Element"]),
                                Event = EvaluateValue(animation["Event"]),
                                Delay = EvaluateValue(animation["Delay"])
                            },
                            "setinputenabled" => new SetInputEnabled
                            {
                                Type = "SetInputEnabled",
                                Element = EvaluateValue(animation["Element"]),
                                Visible = EvaluateValue(animation["Visible"]),
                                Delay = EvaluateValue(animation["Delay"])
                            },
                            "playsound" => new PlaySound
                            {
                                Type = "PlaySound",
                                Delay = EvaluateValue(animation["Delay"]),
                                Sound = EvaluateValue(animation["Sound"])
                            },
                            "stoppanelanimations" => new StopPanelAnimations
                            {
                                Type = "StopPanelAnimations",
                                Element = EvaluateValue(animation["Element"]),
                                Delay = EvaluateValue(animation["Delay"])
                            },
                            _ => throw new Exception(
                                $"Unexpected animation type '{animation?["Type"]}' in {animationOption.Key}!")
                        };
                    }


                    // Don't read animations file unless the user requests a new event
                    // the majority of the animation customisations are for enabling/disabling
                    // events, which use the 'replace' keyword
                    Dictionary<string, List<HUDAnimation>> animations = null;
                    MainWindow.Logger.Info($"Processing: {filePath}");

                    foreach (var animationOption in animationOptions)
                        switch (animationOption.Key.ToLowerInvariant())
                        {
                            case "replace":
                            {
                                // Example:
                                // "replace": [
                                //   "HudSpyDisguiseFadeIn_disabled",
                                //   "HudSpyDisguiseFadeIn"
                                // ]

                                var values = animationOption.Value.ToArray();

                                string find, replace;
                                if (string.Equals(userSetting.Value, "true", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    find = values[0].ToString();
                                    replace = values[1].ToString();
                                }
                                else
                                {
                                    find = values[1].ToString();
                                    replace = values[0].ToString();
                                }

                                File.WriteAllText(filePath, File.ReadAllText(filePath).Replace(find, replace));
                                break;
                            }
                            case "comment":
                            {
                                // Example:
                                // "comment": [
                                //   "StopEvent",
                                //   "StopEvent"
                                // ]

                                var values = animationOption.Value.ToArray();

                                if (bool.TryParse(userSetting.Value, out var valid))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetLineNumbersContainingString(lines, value))
                                        lines[index] = valid
                                            ? Utilities.CommentTextLine(lines, index)
                                            : Utilities.UncommentTextLine(lines, index);
                                    File.WriteAllLines(filePath, lines);
                                }
                                else if (int.TryParse(userSetting.Value, out _))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetLineNumbersContainingString(lines, value))
                                        lines[index] = Utilities.CommentTextLine(lines, index);
                                    File.WriteAllLines(filePath, lines);
                                }

                                break;
                            }
                            case "uncomment":
                            {
                                // Example:
                                // "uncomment": [
                                //   "StopEvent",
                                //   "StopEvent"
                                // ]

                                var values = animationOption.Value.ToArray();

                                if (bool.TryParse(userSetting.Value, out var valid))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetLineNumbersContainingString(lines, value))
                                        lines[index] = valid
                                            ? Utilities.UncommentTextLine(lines, index)
                                            : Utilities.CommentTextLine(lines, index);
                                    File.WriteAllLines(filePath, lines);
                                }
                                else if (int.TryParse(userSetting.Value, out _))
                                {
                                    var lines = File.ReadAllLines(filePath);
                                    foreach (string value in values)
                                    foreach (var index in Utilities.GetLineNumbersContainingString(lines, value))
                                        lines[index] = Utilities.UncommentTextLine(lines, index);
                                    File.WriteAllLines(filePath, lines);
                                }

                                break;
                            }
                            default:
                            {
                                // animation
                                // example:
                                // "HudHealthBonusPulse": [
                                //   {
                                //     "Type": "Animate",
                                //     "Element": "PlayerStatusHealthValue",
                                //     "Property": "Fgcolor",
                                //     "Value": "0 170 255 255",
                                //     "Interpolator": "Linear",
                                //     "Delay": "0",
                                //     "Duration": "0"
                                //   }
                                // ]

                                animations ??= HUDAnimations.Parse(File.ReadAllText(filePath));

                                // Create new event or animation statements could stack
                                // over multiple 'apply customisations'
                                animations[animationOption.Key] = new List<HUDAnimation>();

                                JToken[] animationevents;

                                if (animationOption.Value.Type == JTokenType.Object)
                                {
                                    var animationsContainer =
                                        animationOption.Value.ToObject<Dictionary<string, JToken>>();
                                    if (animationsContainer.ContainsKey("true") &&
                                        animationsContainer.ContainsKey("false"))
                                    {
                                        var selection = animationsContainer[userSetting.Value.ToLower()];
                                        animationevents = selection.ToArray();
                                    }
                                    else
                                    {
                                        throw new Exception($"Unexpected object at {animationOption.Key}!");
                                    }
                                }
                                else
                                {
                                    animationevents = animationOption.Value.ToArray();
                                }


                                foreach (var option in animationevents)
                                {
                                    var animation = option.ToObject<Dictionary<string, dynamic>>();

                                    dynamic current = CreateAnimation(animation, animationOption);

                                    // Animate statements can have an extra argument make sure to account for them
                                    if (current.GetType() == typeof(Animate))
                                    {
                                        if (string.Equals(current.Interpolator, "pulse",
                                            StringComparison.CurrentCultureIgnoreCase))
                                            current.Frequency = EvaluateValue(animation["Frequency"]);

                                        if (string.Equals(current.Interpolator, "gain",
                                                StringComparison.CurrentCultureIgnoreCase) ||
                                            string.Equals(current.Interpolator, "bias",
                                                StringComparison.CurrentCultureIgnoreCase))
                                            current.Bias = EvaluateValue(animation["Bias"]);
                                    }

                                    animations[animationOption.Key].Add(current);
                                }

                                break;
                            }
                        }

                    if (animations is not null) File.WriteAllText(filePath, HUDAnimations.Stringify(animations));
                }

                string[] resFileExtensions = { "res", "vmt", "vdf" };

                foreach (var filePath in files)
                {
                    var relativePath = string.Join('/', Regex.Split(filePath.Key, @"[\/]+"));
                    var absolutePath = MainWindow.HudPath + "\\" + Name + "\\" +
                                       string.Join('\\', relativePath.Split('/'));
                    var extension = filePath.Key.Split(".")[^1];

                    if (resFileExtensions.Contains(extension))
                    {
                        var hudFile = Utilities.CreateNestedObject(hudFolders, relativePath.Split('/'));
                        MainWindow.Logger.Info($"Go to => {relativePath}");

                        Utilities.Merge(hudFile, CompileHudElement(filePath.Value?.ToObject<JObject>(),
                            absolutePath, relativePath, hudFile, ""));
                    }
                    else if (string.Equals(extension, "txt"))
                    {
                        // Assume .txt is always an animation file (may cause issues with mod_textures.txt but assume we are only editing hud files)
                        WriteAnimationCustomizations(absolutePath, filePath.Value?.ToObject<JObject>());
                    }
                    else
                    {
                        MainWindow.ShowMessageBox(MessageBoxImage.Error, string.Format(Utilities.GetLocalizedString(Resources.error_unknown_extension), extension));
                    }
                }
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
            }
        }

        private static (JObject, string, string[]) GetControlInfo(Controls hudSetting, Setting userSetting)
        {
            if (!string.Equals(hudSetting.Type, "ComboBox", StringComparison.CurrentCultureIgnoreCase))
                return (hudSetting.Files, hudSetting.Special, hudSetting.SpecialParameters);
            // Determine files using the files of the selected item's label or value
            // Could cause issues if label and value are both numbers but numbered differently
            var selected = hudSetting.Options.First(x => x.Label == userSetting.Value || x.Value == userSetting.Value);
            return (selected.Files, selected.Special, selected.SpecialParameters);
        }

        private void EvaluateSpecial(string special, Setting userSetting, bool enable, string[] parameters)
        {
            // Check for special conditions, namely if we should enable stock backgrounds.
            if (string.Equals(special, "StockBackgrounds", StringComparison.CurrentCultureIgnoreCase))
                HudBackground.SetStockBackgrounds(enable);

            if (string.Equals(special, "HUDBackground", StringComparison.CurrentCultureIgnoreCase))
                HudBackground.SetHUDBackground(parameters[0]);

            if (string.Equals(special, "CustomBackground", StringComparison.CurrentCultureIgnoreCase) &&
                Uri.TryCreate(userSetting.Value, UriKind.Absolute, out _))
                HudBackground.SetCustomBackground(userSetting.Value);

            if (string.Equals(special, "TransparentViewmodels", StringComparison.CurrentCultureIgnoreCase))
                CopyTransparentViewmodelAddon(enable);
        }

        /// <summary>
        ///     Copy configuration file for transparent viewmodels into the HUD's cfg folder.
        /// </summary>
        public static bool CopyTransparentViewmodelAddon(bool enable = false)
        {
            try
            {
                // Copy the config file required for this feature
                if (!enable || Process.GetProcessesByName("hl2").Any()) return true;
                MainWindow.Logger.Info(
                    $"Copying mastercomfig-transparent-viewmodels-addon.vpk to {MainWindow.HudPath}");
                File.Copy(
                    Directory.GetCurrentDirectory() + "\\Resources\\mastercomfig-transparent-viewmodels-addon.vpk",
                    MainWindow.HudPath + "\\mastercomfig-transparent-viewmodels-addon.vpk", true);
                return true;
            }
            catch (Exception e)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, string.Format(Utilities.GetLocalizedString(Resources.error_transparent_vm), e.Message));
                return false;
            }
        }
    }
}