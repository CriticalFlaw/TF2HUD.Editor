using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using HUDEditor.Models;

namespace HUDEditor.Classes
{
    /// <summary>
    ///     Sample Animation Script
    ///     Commands:
    ///     Animate (panel name) (variable) (target value) (interpolator) (start time) (duration)
    ///     Variables:
    ///     FgColor
    ///     BgColor
    ///     Position
    ///     Size
    ///     Blur        (HUD panels only)
    ///     TextColor    (HUD panels only)
    ///     Ammo2Color    (HUD panels only)
    ///     Alpha        (HUD weapon selection only)
    ///     SelectionAlpha  (HUD weapon selection only)
    ///     TextScan    (HUD weapon selection only)
    ///     Interpolator:
    ///     Linear
    ///     Accel - starts moving slow, ends fast
    ///     Deaccel - starts moving fast, ends slow
    ///     Spline - simple ease in/out curve
    ///     Pulse - ( freq ) over the duration, the value is pulsed (cosine) freq times ending at the dest value (assuming freq
    ///     is integral)
    ///     Flicker - ( randomness factor 0.0 to 1.0 ) over duration, each frame if random # is less than factor, use end
    ///     value, otherwise use prev value
    ///     Gain - ( bias ) Lower bias values bias towards 0.5 and higher bias values bias away from it.
    ///     Bias - ( bias ) Lower values bias the curve towards 0 and higher values bias it towards 1.
    ///     RunEvent (event name) (start time) - starts another even running at the specified time
    ///     StopEvent (event name) (start time) - stops another event that is current running at the specified time
    ///     StopAnimation (panel name) (variable) (start time) - stops all animations referring to the specified variable in
    ///     the specified panel
    ///     StopPanelAnimations (panel name) (start time) - stops all active animations operating on the specified panel
    ///     SetFont (panel name) (fontparameter) (fontname from scheme) (set time)
    ///     SetTexture (panel name) (textureidname) (texturefilename) (set time)
    ///     SetString (panel name) (string varname) (stringvalue) (set time)
    /// </summary>
    internal static class HUDAnimations
    {
        public static Dictionary<string, List<HUDAnimation>> Parse(string text)
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
                while ((ignoredCharacters.Contains(text[x]) || text[x] == '/') && x < text.Length - 1)
                {
                    if (text[x] == '/')
                    {
                        if (text[x + 1] == '/')
                            while (text[x] != '\n' && x < text.Length - 1)
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

                    while (text[x] != '"' && x < text.Length - 1)
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
                    while (x < text.Length && !ignoredCharacters.Contains(text[x]))
                    {
                        if (text[x] == '"') throw new Exception($"Unexpected double quote at position {x}");
                        currentToken += text[x];
                        x++;
                    }
                }

                if (!lookAhead) index = x;

                return currentToken;
            }

            Dictionary<string, List<HUDAnimation>> ParseFile()
            {
                Dictionary<string, List<HUDAnimation>> animations = new();
                var currentToken = Next();

                while (string.Equals(currentToken, "event", StringComparison.CurrentCultureIgnoreCase))
                {
                    var eventName = Next();
                    animations[eventName] = ParseEvent();
                    currentToken = Next();
                }

                return animations;
            }

            List<HUDAnimation> ParseEvent()
            {
                List<HUDAnimation> events = new();
                var nextToken = Next();

                if (string.Equals(nextToken, "{"))
                    while (nextToken != "}" && nextToken != "EOF")
                    {
                        // NextToken is not a closing brace therefore it is the animation type.
                        // Pass the animation type to the animation.
                        nextToken = Next();
                        if (nextToken != "}") events.Add(ParseAnimation(nextToken));
                    }
                else
                    throw new Exception($"Unexpected ${nextToken} at position {index}! Are you missing an opening brace?");
				
                return events;
            }

            void SetInterpolator(Animate animation)
            {
                var interpolator = Next().ToLower();
                if (string.Equals(interpolator, "pulse", StringComparison.CurrentCultureIgnoreCase))
                {
                    animation.Interpolator = interpolator;
                    animation.Frequency = Next();
                }
                else if (new[] { "gain", "bias" }.Contains(interpolator))
                {
                    animation.Interpolator = interpolator[0].ToString().ToUpper() + interpolator[1..];
                    animation.Bias = Next();
                }
                else
                {
                    animation.Interpolator = interpolator;
                }
            }

            HUDAnimation ParseAnimation(string type)
            {
                dynamic animation;
                type = type.ToLower();

                switch (type)
                {
                    case "animate":
                        animation = new Animate();
                        animation.Type = type;
                        animation.Element = Next();
                        animation.Property = Next();
                        animation.Value = Next();
                        SetInterpolator(animation);
                        animation.Delay = Next();
                        animation.Duration = Next();
                        break;
                    case "runevent":
                        animation = new RunEvent();
                        animation.Type = type;
                        animation.Event = Next();
                        animation.Delay = Next();
                        break;
                    case "stopevent":
                        animation = new StopEvent();
                        animation.Type = type;
                        animation.Event = Next();
                        animation.Delay = Next();
                        break;
                    case "setvisible":
                        animation = new SetVisible();
                        animation.Type = type;
                        animation.Element = Next();
                        animation.Delay = Next();
                        animation.Duration = Next();
                        break;
                    case "firecommand":
                        animation = new FireCommand();
                        animation.Type = type;
                        animation.Delay = Next();
                        animation.Command = Next();
                        break;
                    case "runeventchild":
                        animation = new RunEventChild();
                        animation.Type = type;
                        animation.Element = Next();
                        animation.Event = Next();
                        animation.Delay = Next();
                        break;
                    case "setinputenabled":
                        animation = new SetInputEnabled();
                        animation.Element = Next();
                        animation.Visible = int.Parse(Next());
                        animation.Delay = Next();
                        break;
                    case "playsound":
                        animation = new PlaySound();
                        animation.Delay = Next();
                        animation.Sound = Next();
                        break;
                    case "stoppanelanimations":
                        animation = new StopPanelAnimations();
                        animation.Element = Next();
                        animation.Delay = Next();
                        break;
                    default:
                        Debug.WriteLine(text.Substring(index - 25, 25));
                        throw new Exception($"Unexpected {type} at position {index}");
                }

                if (Next(true).StartsWith('[')) animation.OSTag = Next();
                return animation;
            }

            return ParseFile();
        }

        public static string Stringify(Dictionary<string, List<HUDAnimation>> animations)
        {
            var stringValue = "";
            const char tab = '\t';
            const string newLine = "\r\n";

            static string FormatWhiteSpace(string text)
            {
                return Regex.IsMatch(text, "\\s") ? $"\"{text}\"" : text;
            }

            static string GetInterpolator(Animate animation)
            {
                return animation.Interpolator.ToLower() switch
                {
                    "Pulse" => $"Pulse {animation.Frequency}",
                    "Gain" or "Bias" => $"Gain {animation.Bias}",
                    _ => $"{animation.Interpolator}"
                };
            }

            foreach (var key in animations.Keys)
            {
                stringValue += $"event {key}{newLine}{{{newLine}";
                foreach (dynamic animation in animations[key])
                {
                    stringValue += tab;
                    Type T = animation.GetType();

                    if (T == typeof(Animate))
                        stringValue +=
                            $"Animate {FormatWhiteSpace(animation.Element)} {FormatWhiteSpace(animation.Property)} {FormatWhiteSpace(animation.Value)} {GetInterpolator(animation)} {animation.Delay} {animation.Duration}";
                    else if (T == typeof(RunEvent) || T == typeof(StopEvent))
                        stringValue += $"RunEvent {FormatWhiteSpace(animation.Event)} {animation.Delay}";
                    else if (T == typeof(StopEvent))
                        stringValue += $"StopEvent {FormatWhiteSpace(animation.Event)} {animation.Delay}";
                    else if (T == typeof(SetVisible))
                        stringValue +=
                            $"SetVisible {FormatWhiteSpace(animation.Element)} {animation.Delay} {animation.Duration}";
                    else if (T == typeof(FireCommand))
                        stringValue += $"FireCommand {animation.Delay} {FormatWhiteSpace(animation.Command)}";
                    else if (T == typeof(RunEventChild))
                        stringValue +=
                            $"RunEventChild {FormatWhiteSpace(animation.Element)} {FormatWhiteSpace(animation.Event)} {animation.Delay}";
                    else if (T == typeof(SetVisible))
                        stringValue +=
                            $"SetVisible {FormatWhiteSpace(animation.Element)} {animation.Visible} {animation.Delay}";
                    else if (T == typeof(PlaySound))
                        stringValue += $"PlaySound {animation.Delay} {FormatWhiteSpace(animation.Sound)}";

                    if (animation.OSTag != null) stringValue += " " + animation.OSTag;

                    stringValue += newLine;
                }

                stringValue += $"}}{newLine}";
            }

            return stringValue;
        }
    }
}