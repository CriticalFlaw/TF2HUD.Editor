using HUDEditor.Models;
using log4net;
using log4net.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor.Classes
{
    public class HUDUpdateChecker : IHUDUpdateChecker
    {
        private readonly ILog _logger;

        public HUDUpdateChecker(ILog logger)
        {
            _logger = logger;
        }

        public bool AreEqual(string hud1Name, object hud1, object hud2, string[] ignoreList)
        {
            foreach (var field in hud1.GetType().GetFields())
            {
                if (ignoreList.Contains(field.Name)) continue;
                if (field.GetValue(hud1) is null && field.GetValue(hud2) is null) continue;

                if (field.FieldType == typeof(string[]))
                {
                    var arr1 = (string[])field.GetValue(hud1);
                    var arr2 = (string[])field.GetValue(hud2);

                    if (arr1 is null && arr2 is not null)
                    {
                        LogChange($"{field.Name}", "*not present*", $"{arr2}[{arr2.Length}]");
                        return false;
                    }

                    if (arr1 is not null && arr2 is null)
                    {
                        LogChange($"{field.Name}", "Argument 2 [0]", "*not present*");
                        return false;
                    }

                    if (!arr1.Length.Equals(arr2.Length))
                    {
                        LogChange($"{field.Name}.Length", arr1.Length.ToString(), arr2.Length.ToString());
                        return false;
                    }

                    for (var i = 0; i < arr1.Length; i++)
                    {
                        if (arr1[i] == arr2[i]) continue;
                        LogChange($"{field.Name}[{i}]", arr1[i], arr2[i]);
                        return false;
                    }
                }
                else if (field.FieldType == typeof(Dictionary<string, Controls[]>))
                {
                    var value1 = (Dictionary<string, Controls[]>)field.GetValue(hud1);
                    var value2 = (Dictionary<string, Controls[]>)field.GetValue(hud2);

                    if (!value1.Keys.Count.Equals(value2.Keys.Count))
                    {
                        LogChange($"{field.Name}.Keys.Count", value1.Keys.Count.ToString(),
                            value2.Keys.Count.ToString());
                        return false;
                    }

                    for (var i = 0; i < value1.Keys.Count; i++)
                    {
                        if (value1.Keys.ElementAt(i) == value2.Keys.ElementAt(i)) continue;
                        LogChange($"Controls[{i}].Header", value1.Keys.ElementAt(i), value2.Keys.ElementAt(i));
                        return false;
                    }

                    foreach (var key in value1.Keys)
                    {
                        if (!value1[key].Length.Equals(value2[key].Length))
                        {
                            LogChange($"{field.Name}[\"{key}\"].Length", value1[key].Length.ToString(),
                                value2[key].Length.ToString());
                            return false;
                        }

                        for (var i = 0; i < value1[key].Length; i++)
                        {
                            var comparison = AreEqual(hud1Name, value1[key][i], value2[key][i], new[]
                            {
                                    "Control",
                                    "Value"
                                });
                            if (!comparison) return false;
                        }
                    }
                }
                else if (field.FieldType == typeof(JObject))
                {
                    if (!CompareFiles((JObject)field.GetValue(hud1), (JObject)field.GetValue(hud2),
                        $"{field.Name}.Files => ")) return false;
                }
                else if (field.FieldType == typeof(Option[]))
                {
                    var arr1 = (Option[])field.GetValue(hud1);
                    var arr2 = (Option[])field.GetValue(hud2);

                    if (arr1 is null && arr2 is not null)
                    {
                        LogChange($"{field.Name}", "*not present*", $"{arr2}[{arr2.Length}]");
                        return false;
                    }

                    if (arr1 is not null && arr2 is null)
                    {
                        LogChange($"{field.Name}", $"{arr1}[{arr1.Length}]", "*not present*");
                        return false;
                    }

                    if (!arr1.Length.Equals(arr2.Length))
                    {
                        LogChange($"{field.Name}.Length", arr1.Length.ToString(), arr2.Length.ToString());
                        return false;
                    }

                    if (arr1.Select((t, i) => AreEqual(hud1Name, t, arr2[i], new string[] { })).Any(comparison => !comparison))
                        return false;
                }
                else if (!string.Equals(field.GetValue(hud1)?.ToString(), field.GetValue(hud2)?.ToString()))
                {
                    LogChange(field.Name, field.GetValue(hud1)?.ToString(), field.GetValue(hud2)?.ToString());
                    return false;
                }
            }

            _logger.Info($"{hud1Name}: no fields changed, HUD has not been updated.");
            return true;
        }

        private bool CompareFiles(JObject obj1, JObject obj2, string path = "")
        {
            if (obj1 is null && obj2 is null) return true;

            foreach (var x in obj1)
            {
                if (obj2.ContainsKey(x.Key)) continue;
                LogChange($"{path}{x.Key}", x.Key, "*not present*");
                return false;
            }

            foreach (var x in obj2)
            {
                if (obj1.ContainsKey(x.Key)) continue;
                LogChange($"{path}{x.Key}", "*not present*", x.Key);
                return false;
            }

            foreach (var x in obj1)
                if (obj1[x.Key].Type == JTokenType.Object && obj2[x.Key].Type == JTokenType.Object)
                {
                    if (!CompareFiles(obj1[x.Key].ToObject<JObject>(), obj2[x.Key].ToObject<JObject>(),
                        $"{path}/{x.Key}")) return false;
                }
                else if (x.Value.Type == JTokenType.Array && obj2[x.Key].Type == JTokenType.Array)
                {
                    var arr1 = obj1[x.Key].ToArray();
                    var arr2 = obj2[x.Key].ToArray();

                    if (!arr1.Length.Equals(arr2.Length))
                    {
                        LogChange($"{path}{x.Key}.Length", arr1.Length.ToString(), arr2.Length.ToString());
                        return false;
                    }

                    for (var i = 0; i < arr1.Length; i++)
                    {
                        if (arr1[i].ToString() == arr2[i].ToString()) continue;
                        LogChange($"{path}{x.Key}/[{i}]", arr1[i].ToString(), arr2[i].ToString());
                        return false;
                    }
                }
                else if (!string.Equals(x.Value.ToString(), obj2[x.Key].ToString()))
                {
                    LogChange(x.Key, x.Value.ToString(), obj2[x.Key].ToString());
                    return false;
                }

            return true;
        }

        private void LogChange(string hud1Name, string prop, string before = "", string after = "")
        {
            var message = !string.IsNullOrWhiteSpace(before) ? $" (\"{before}\" => \"{after}\")" : string.Empty;
            _logger.Info($"{hud1Name}: {prop} has changed{message}, HUD has been updated.");
        }
    }
}
