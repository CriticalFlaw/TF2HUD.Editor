using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

// sample animation script
//
//
// commands:
//    Animate <panel name> <variable> <target value> <interpolator> <start time> <duration>
//        variables:
//            FgColor
//            BgColor
//            Position
//            Size
//            Blur        (hud panels only)
//            TextColor    (hud panels only)
//            Ammo2Color    (hud panels only)
//            Alpha        (hud weapon selection only)
//            SelectionAlpha  (hud weapon selection only)
//            TextScan    (hud weapon selection only)
//
//        interpolator:
//            Linear
//            Accel - starts moving slow, ends fast
//            Deaccel - starts moving fast, ends slow
//            Spline - simple ease in/out curve
//            Pulse - < freq > over the duration, the value is pulsed (cosine) freq times ending at the dest value (assuming freq is integral)
//            Flicker - < randomness factor 0.0 to 1.0 > over duration, each frame if random # is less than factor, use end value, otherwise use prev value
//            Gain - < bias > Lower bias values bias towards 0.5 and higher bias values bias away from it.
//            Bias - < bias > Lower values bias the curve towards 0 and higher values bias it towards 1.
//
//    RunEvent <event name> <start time>
//        starts another even running at the specified time
//
//    StopEvent <event name> <start time>
//        stops another event that is current running at the specified time
//
//    StopAnimation <panel name> <variable> <start time>
//        stops all animations refering to the specified variable in the specified panel
//
//    StopPanelAnimations <panel name> <start time>
//        stops all active animations operating on the specified panel
//
//  SetFont <panel name> <fontparameter> <fontname from scheme> <set time>
//
//    SetTexture <panel name> <textureidname> <texturefilename> <set time>
//
//  SetString <panel name> <string varname> <stringvalue> <set time>

namespace TF2HUD.Editor.Classes
{
    internal class HUDAnimation
    {
        public string Type { get; set; }
        public string OSTag { get; set; }
    }

    internal class Animate : HUDAnimation
    {
        public string Element { get; set; }
        public string Property { get; set; }
        public string Value { get; set; }
        public string Interpolator { get; set; }
        public string Frequency { get; set; }
        public string Bias { get; set; }
        public string Delay { get; set; }
        public string Duration { get; set; }
    }

    internal class RunEvent : HUDAnimation
    {
        public string Event { get; set; }
        public string Delay { get; set; }
    }

    internal class StopEvent : HUDAnimation
    {
        public string Event { get; set; }
        public string Delay { get; set; }
    }

    internal class SetVisible : HUDAnimation
    {
        public string Element { get; set; }
        public string Delay { get; set; }
        public string Duration { get; set; }
    }

    internal class FireCommand : HUDAnimation
    {
        public string Delay { get; set; }
        public string Command { get; set; }
    }

    internal class RunEventChild : HUDAnimation
    {
        public string Element { get; set; }
        public string Event { get; set; }
        public string Delay { get; set; }
    }

    internal class SetInputEnabled : HUDAnimation
    {
        public string Element { get; set; }
        public int Visible { get; set; }
        public string Delay { get; set; }
    }

    internal class PlaySound : HUDAnimation
    {
        public string Delay { get; set; }
        public string Sound { get; set; }
    }

    internal class StopPanelAnimations : HUDAnimation
    {
        public string Element { get; set; }
        public string Delay { get; set; }
    }

    internal static class HUDAnimations
    {
        public static Dictionary<string, List<HUDAnimation>> Parse(string Str)
        {
            var i = 0;
            char[] WhiteSpaceIgnore = {' ', '\t', '\r', '\n'};

            string Next(bool LookAhead = false)
            {
                var CurrentToken = "";
                var j = i;

                if (j >= Str.Length - 1) return "EOF";

                while ((WhiteSpaceIgnore.Contains(Str[j]) || Str[j] == '/') && j < Str.Length - 1)
                {
                    if (Str[j] == '/')
                    {
                        if (Str[j + 1] == '/')
                            while (Str[j] != '\n' && j < Str.Length - 1)
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
                    while (Str[j] != '"' && j < Str.Length - 1)
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
                    while (j < Str.Length && !WhiteSpaceIgnore.Contains(Str[j]))
                    {
                        if (Str[j] == '"') throw new Exception($"Unexpected double quote at position {j}");
                        CurrentToken += Str[j];
                        j++;
                    }
                }

                if (!LookAhead) i = j;

                //if (j > Str.Length)
                //{
                //    return "EOF";
                //}

                return CurrentToken;
            }

            Dictionary<string, List<HUDAnimation>> ParseFile()
            {
                Dictionary<string, List<HUDAnimation>> Animations = new();

                var CurrentToken = Next();

                // System.Diagnostics.Debugger.Break();

                while (CurrentToken == "event")
                {
                    var EventName = Next();
                    Animations[EventName] = ParseEvent();
                    CurrentToken = Next();
                }

                return Animations;
            }

            List<HUDAnimation> ParseEvent()
            {
                List<HUDAnimation> Event = new();
                var NextToken = Next();
                if (NextToken == "{")
                    // string NextToken = Next();
                    while (NextToken != "}" && NextToken != "EOF")
                    {
                        // NextToken is not a closing brace therefore it is the animation type
                        // Pass the animation type to the animation
                        NextToken = Next();
                        if (NextToken != "}") Event.Add(ParseAnimation(NextToken));
                    }
                else
                    throw new Exception($"Unexpected ${NextToken} at position {i}! Are you missing an opening brace?");

                return Event;
            }

            void SetInterpolator(Animate Animation)
            {
                var Interpolator = Next().ToLower();
                if (Interpolator == "pulse")
                {
                    Animation.Interpolator = Interpolator;
                    Animation.Frequency = Next();
                }
                else if (new[] {"gain", "bias"}.Contains(Interpolator))
                {
                    Animation.Interpolator = Interpolator[0].ToString().ToUpper() +
                                             Interpolator.Substring(1, Interpolator.Length - 1);
                    Animation.Bias = Next();
                }
                else
                {
                    Animation.Interpolator = Interpolator;
                }
            }


            HUDAnimation ParseAnimation(string AnimationType)
            {
                dynamic Animation;
                AnimationType = AnimationType.ToLower();

                if (AnimationType == "animate")
                {
                    Animation = new Animate();
                    Animation.Type = AnimationType;
                    Animation.Element = Next();
                    Animation.Property = Next();
                    Animation.Value = Next();
                    SetInterpolator(Animation);
                    Animation.Delay = Next();
                    Animation.Duration = Next();
                }
                else if (AnimationType == "runevent")
                {
                    Animation = new RunEvent();
                    Animation.Type = AnimationType;
                    Animation.Event = Next();
                    Animation.Delay = Next();
                }
                else if (AnimationType == "stopevent")
                {
                    Animation = new StopEvent();
                    Animation.Type = AnimationType;
                    Animation.Event = Next();
                    Animation.Delay = Next();
                }
                else if (AnimationType == "setvisible")
                {
                    Animation = new SetVisible();
                    Animation.Type = AnimationType;
                    Animation.Element = Next();
                    Animation.Delay = Next();
                    Animation.Duration = Next();
                }
                else if (AnimationType == "firecommand")
                {
                    Animation = new FireCommand();
                    Animation.Type = AnimationType;
                    Animation.Delay = Next();
                    Animation.Command = Next();
                }
                else if (AnimationType == "runeventchild")
                {
                    Animation = new RunEventChild();
                    Animation.Type = AnimationType;
                    Animation.Element = Next();
                    Animation.Event = Next();
                    Animation.Delay = Next();
                }
                else if (AnimationType == "setinputenabled")
                {
                    Animation = new SetInputEnabled();
                    Animation.Element = Next();
                    Animation.Visible = int.Parse(Next());
                    Animation.Delay = Next();
                }
                else if (AnimationType == "playsound")
                {
                    Animation = new PlaySound();
                    Animation.Delay = Next();
                    Animation.Sound = Next();
                }
                else if (AnimationType == "stoppanelanimations")
                {
                    Animation = new StopPanelAnimations();
                    Animation.Element = Next();
                    Animation.Delay = Next();
                }
                else
                {
                    Debug.WriteLine(Str.Substring(i - 25, 25));
                    throw new Exception($"Unexpected {AnimationType} at position {i}");
                }

                if (Next(true).StartsWith('[')) Animation.OSTag = Next();

                return Animation;
            }

            return ParseFile();
        }

        public static string Stringify(Dictionary<string, List<HUDAnimation>> Animations)
        {
            var Str = "";
            var Tab = '\t';
            var NewLine = "\r\n";

            string FormatWhiteSpace(string Str)
            {
                return Regex.IsMatch(Str, "\\s") ? $"\"{Str}\"" : Str;
            }

            string GetInterpolator(Animate Animation)
            {
                var Interpolator = Animation.Interpolator.ToLower();
                switch (Interpolator)
                {
                    case "Pulse":
                        return $"Pulse {Animation.Frequency}";
                    case "Gain":
                    case "Bias":
                        return $"Gain {Animation.Bias}";
                    default:
                        return $"{Animation.Interpolator}";
                }
            }

            foreach (var Event in Animations.Keys)
            {
                Str += $"event {Event}{NewLine}{{{NewLine}";
                foreach (dynamic Execution in Animations[Event])
                {
                    Str += Tab;
                    Type T = Execution.GetType();

                    if (T == typeof(Animate))
                        Str +=
                            $"Animate {FormatWhiteSpace(Execution.Element)} {FormatWhiteSpace(Execution.Property)} {FormatWhiteSpace(Execution.Value)} {GetInterpolator(Execution)} {Execution.Delay} {Execution.Duration}";
                    else if (T == typeof(RunEvent) || T == typeof(StopEvent))
                        Str += $"RunEvent {FormatWhiteSpace(Execution.Event)} {Execution.Delay}";
                    else if (T == typeof(SetVisible))
                        Str +=
                            $"SetVisible {FormatWhiteSpace(Execution.Element)} {Execution.Delay} {Execution.Duration}";
                    else if (T == typeof(FireCommand))
                        Str += $"FireCommand {Execution.Delay} {FormatWhiteSpace(Execution.Command)}";
                    else if (T == typeof(RunEventChild))
                        Str +=
                            $"RunEventChild {FormatWhiteSpace(Execution.Element)} {FormatWhiteSpace(Execution.Event)} {Execution.Delay}";
                    else if (T == typeof(SetVisible))
                        Str +=
                            $"SetVisible {FormatWhiteSpace(Execution.Element)} {Execution.Visible} {Execution.Delay}";
                    else if (T == typeof(PlaySound))
                        Str += $"PlaySound {Execution.Delay} {FormatWhiteSpace(Execution.Sound)}";

                    if (Execution.OSTag != null) Str += " " + Execution.OSTag;

                    Str += NewLine;
                }

                Str += $"}}{NewLine}";
            }

            return Str;
        }
    }
}