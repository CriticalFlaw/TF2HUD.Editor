using System.Text.RegularExpressions;

namespace HUDEditor.Models
{
    internal abstract class HUDAnimation
    {
        public abstract string Type { get; }
        public string? Conditional { get; set; }
        public abstract override string ToString();

        protected string Print(string str)
        {
            return Regex.IsMatch(str, "\\s") ? $"\"{str}\"" : str;
        }

        protected string PrintConditional()
        {
            return Conditional != null ? $" {Conditional}" : "";
        }
    }

    internal class Animate : HUDAnimation
    {
        public override string Type => nameof(Animate);
        public required string Element { get; init; }
        public required string Property { get; init; }
        public required string Value { get; init; }
        public required Interpolator Interpolator { get; init; }
        public required string Delay { get; init; }
        public required string Duration { get; init; }

        public override string ToString()
        {
            return $"{Type} {Print(Element)} {Print(Property)} {Print(Property)} {Print(Value)} {Interpolator} {Delay} {Duration}" + PrintConditional();
        }
    }

    internal abstract class Interpolator
    {
        public abstract override string ToString();
    }

    internal class LinearInterpolator : Interpolator
    {
        public override string ToString() => "Linear";
    }

    internal class AccelInterpolator : Interpolator
    {
        public override string ToString() => "Accel";
    }

    internal class DeAccelInterpolator : Interpolator
    {
        public override string ToString() => "DeAccel";
    }

    internal class SplineInterpolator : Interpolator
    {
        public override string ToString() => "Spline";
    }

    internal class PulseInterpolator : Interpolator
    {
        public required string Frequency { get; init; }
        public override string ToString() => $"Pulse {Frequency}";
    }

    internal class FlickerInterpolator : Interpolator
    {
        public required string Randomness { get; init; }
        public override string ToString() => $"Flicker {Randomness}";
    }

    internal class GainInterpolator : Interpolator
    {
        public required string Bias { get; init; }
        public override string ToString() => $"Gain {Bias}";
    }

    internal class BiasInterpolator : Interpolator
    {
        public required string Bias { get; init; }
        public override string ToString() => $"Bias {Bias}";
    }

    internal class RunEvent : HUDAnimation
    {
        public override string Type => nameof(RunEvent);
        public required string Event { get; set; }
        public required string Delay { get; set; }

        public override string ToString()
        {
            return $"{Type} {Print(Event)} {Delay}" + PrintConditional();
        }
    }

    internal class StopEvent : HUDAnimation
    {
        public override string Type => nameof(StopEvent);
        public required string Event { get; set; }
        public required string Delay { get; set; }

        public override string ToString()
        {
            return $"{Type} {Print(Event)} {Delay}" + PrintConditional();
        }
    }

    internal class SetVisible : HUDAnimation
    {
        public override string Type => nameof(SetVisible);
        public required string Element { get; set; }
        public required string Visible { get; set; }
        public required string Delay { get; set; }

        public override string ToString()
        {
            return $"{Type} {Print(Element)} {Visible} {Delay}" + PrintConditional();
        }
    }

    internal class FireCommand : HUDAnimation
    {
        public override string Type => nameof(FireCommand);
        public required string Delay { get; set; }
        public required string Command { get; set; }

        public override string ToString()
        {
            return $"{Type} {Delay} {Print(Command)}" + PrintConditional();
        }
    }

    internal class RunEventChild : HUDAnimation
    {
        public override string Type => nameof(RunEventChild);
        public required string Element { get; set; }
        public required string Event { get; set; }
        public required string Delay { get; set; }

        public override string ToString()
        {
            return $"{Type} {Print(Element)} {Print(Event)} {Delay}" + PrintConditional();
        }
    }

    internal class SetInputEnabled : HUDAnimation
    {
        public override string Type => nameof(SetInputEnabled);
        public required string Element { get; set; }
        public required string Enabled { get; set; }
        public required string Delay { get; set; }

        public override string ToString()
        {
            return $"{Type} {Print(Element)} {Enabled} {Delay}" + PrintConditional();
        }
    }

    internal class PlaySound : HUDAnimation
    {
        public override string Type => nameof(PlaySound);
        public required string Delay { get; set; }
        public required string Sound { get; set; }

        public override string ToString()
        {
            return $"{Type} {Delay} {Print(Sound)}" + PrintConditional();
        }
    }

    internal class StopPanelAnimations : HUDAnimation
    {
        public override string Type => nameof(StopPanelAnimations);
        public required string Element { get; init; }
        public required string Delay { get; init; }

        public override string ToString()
        {
            return $"{Type} {Print(Element)} {Delay}" + PrintConditional();
        }
    }
}