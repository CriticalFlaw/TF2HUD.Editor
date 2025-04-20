using System;
using System.Collections.Generic;
using HUDEdit.Models;

namespace HUDEdit.Classes;

/// <summary>
/// Sample Animation Script
/// Commands:
///     Animate (panel name) (variable) (target value) (interpolator) (start time) (duration)
///     Variables:
///         FgColor
///         BgColor
///         Position
///         Size
///         Blur - HUD panels only
///         TextColor - HUD panels only
///         Ammo2Color - HUD panels only
///         Alpha - HUD weapon selection only
///         SelectionAlpha - HUD weapon selection only
///         TextScan - HUD weapon selection only
///     Interpolator:
///         Linear
///         Accel - starts moving slow, ends fast
///         Deaccel - starts moving fast, ends slow
///         Spline - simple ease in/out curve
///         Pulse - freq over the duration, the value is pulsed (cosine) freq times ending at the dest value (assuming freq is integral)
///         Flicker - randomness factor 0.0 to 1.0 over duration, each frame if random # is less than factor, use end value, otherwise use prev value
///         Gain - (bias) Lower bias values bias towards 0.5 and higher bias values bias away from it
///         Bias - (bias) Lower values bias the curve towards 0 and higher values bias it towards 1
///         RunEvent (event name) (start time) - starts another even running at the specified time
///         StopEvent (event name) (start time) - stops another event that is current running at the specified time
///         StopAnimation (panel name) (variable) (start time) - stops all animations referring to the specified variable in the specified panel
///         StopPanelAnimations (panel name) (start time) - stops all active animations operating on the specified panel
///         SetFont (panel name) (fontparameter) (fontname from scheme) (set time)
///         SetTexture (panel name) (textureidname) (texturefilename) (set time)
///         SetString (panel name) (string varname) (stringvalue) (set time)
/// </summary>
internal static class HUDAnimations
{
    public static Dictionary<string, List<HUDAnimation>> Parse(string text)
    {
        VDFTokenizer tokenizer = new VDFTokenizer(text);

        Dictionary<string, List<HUDAnimation>> ParseFile()
        {
            Dictionary<string, List<HUDAnimation>> animations = new();

            while (true)
            {
                var token = tokenizer.Next();
                if (token == null) break;
                switch (token.Value.Type)
                {
                    case VDFTokenType.String:
                        {
                            if (string.Equals(token.Value.Value, "event", StringComparison.CurrentCultureIgnoreCase))
                            {
                                var eventName = tokenizer.Next();
                                if (eventName == null) throw new VDFSyntaxException(VDFTokenType.String, "EOF", ["event name"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
                                if (eventName.Value.Type == VDFTokenType.String) animations[eventName.Value.Value] = ParseEvent();
                                else throw new VDFSyntaxException(eventName.Value.Type, eventName.Value.Value, ["event name"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
                            }
                            else throw new VDFSyntaxException(token.Value.Type, token.Value.Value, ["\"event\"", "EOF"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
                            break;
                        }
                    default:
                        throw new VDFSyntaxException(token.Value.Type, token.Value.Value, ["\"event\"", "EOF"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
                }
            }

            return animations;
        }

        List<HUDAnimation> ParseEvent()
        {
            List<HUDAnimation> events = new();
            var token = tokenizer.Next();
            if (token == null) throw new VDFSyntaxException(VDFTokenType.String, "EOF", ["{"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
            if (token.Value.Type != VDFTokenType.ControlCharacter || token.Value.Value != "{") throw new VDFSyntaxException(token.Value.Type, token.Value.Value, ["{"], tokenizer.Index, tokenizer.Line, tokenizer.Character);

            while (true)
            {
                var animationCommandToken = tokenizer.Next();
                if (animationCommandToken == null) throw new VDFSyntaxException(VDFTokenType.String, "EOF", ["animation command", "}"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
                if (animationCommandToken.Value.Type != VDFTokenType.String) throw new VDFSyntaxException(animationCommandToken.Value.Type, animationCommandToken.Value.Value, ["animation command", "}"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
                if (animationCommandToken.Value.Value == "}") break;

                events.Add(ParseAnimation(animationCommandToken.Value.Value));
            }

            return events;
        }

        string ReadString()
        {
            var token = tokenizer.Next();
            if (token == null) throw new VDFSyntaxException(VDFTokenType.String, "EOF", ["string"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
            if (token.Value.Type != VDFTokenType.String) throw new VDFSyntaxException(token.Value.Type, token.Value.Value, ["string"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
            return token.Value.Value;
        }

        string ReadNumber()
        {
            return float.Parse(ReadString()).ToString();
        }

        string ReadBool()
        {
            var token = ReadString();
            if (token != "0" || token != "1") throw new VDFSyntaxException(VDFTokenType.String, token, ["0", "1"], tokenizer.Index, tokenizer.Line, tokenizer.Character);
            return token;
        }

        HUDAnimation ParseAnimation(string type)
        {
            HUDAnimation animation = type.ToLower() switch
            {
                "animate" => new Animate
                {
                    Element = ReadString(),
                    Property = ReadString(),
                    Value = ReadString(),
                    Interpolator = ReadString().ToLower() switch
                    {
                        "linear" => new LinearInterpolator(),
                        "accel" => new AccelInterpolator(),
                        "deaccel" => new DeAccelInterpolator(),
                        "spline" => new SplineInterpolator(),
                        "pulse" => new PulseInterpolator { Frequency = ReadString() },
                        "flicker" => new FlickerInterpolator { Randomness = ReadString() },
                        "bias" => new BiasInterpolator { Bias = ReadString() },
                        "gain" => new GainInterpolator { Bias = ReadString() },
                        var interpolator => throw new VDFSyntaxException(VDFTokenType.String, interpolator, ["interpolator"], tokenizer.Index, tokenizer.Line, tokenizer.Character),
                    },
                    Delay = ReadNumber(),
                    Duration = ReadNumber(),
                },
                "runevent" => new RunEvent
                {
                    Event = ReadString(),
                    Delay = ReadNumber(),
                },
                "stopevent" => new StopEvent
                {
                    Event = ReadString(),
                    Delay = ReadNumber(),
                },
                "setvisible" => new SetVisible
                {
                    Element = ReadString(),
                    Visible = ReadString(),
                    Delay = ReadNumber(),
                },
                "firecommand" => new FireCommand
                {
                    Delay = ReadNumber(),
                    Command = ReadString(),
                },
                "runeventchild" => new RunEventChild
                {
                    Element = ReadString(),
                    Event = ReadString(),
                    Delay = ReadNumber(),
                },
                "setinputenabled" => new SetInputEnabled
                {
                    Element = ReadString(),
                    Enabled = ReadBool(),
                    Delay = ReadNumber(),
                },
                "playsound" => new PlaySound
                {
                    Delay = ReadNumber(),
                    Sound = ReadString(),
                },
                "stoppanelanimations" => new StopPanelAnimations
                {
                    Element = ReadString(),
                    Delay = ReadNumber(),
                },
                _ => throw new VDFSyntaxException(VDFTokenType.String, type, ["animation command"], tokenizer.Index, tokenizer.Line, tokenizer.Character),
            };
            var conditionalToken = tokenizer.Next(true);
            if (conditionalToken != null && conditionalToken.Value.Type == VDFTokenType.Conditional)
            {
                animation.Conditional = conditionalToken.Value.Value;
                tokenizer.Next();
            }

            return animation;
        }

        return ParseFile();
    }

    /// <summary>
    /// Converts animation events into a string value to be written into HUD animations.
    /// </summary>
    /// <param name="animations"></param>
    /// <returns></returns>
    public static string Stringify(Dictionary<string, List<HUDAnimation>> animations)
    {
        var stringValue = "";
        const string newLine = "\r\n";

        foreach (var key in animations.Keys)
        {
            stringValue += $"event {key}{newLine}{{{newLine}";
            foreach (var animation in animations[key])
            {
                stringValue += $"\t{animation}{newLine}";
            }
            stringValue += $"}}{newLine}";
        }
        return stringValue;
    }
}