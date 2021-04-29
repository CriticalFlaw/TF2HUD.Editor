namespace HUDEditor.Models
{
    internal class HUDAnimation
    {
        public string Type { get; set; }
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
}
