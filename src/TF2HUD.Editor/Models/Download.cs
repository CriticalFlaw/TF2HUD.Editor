namespace HUDEditor.Models;

public class Download
{
    public string Content => Source;
    public string Source { get; set; }
    public string Link { get; set; }
}