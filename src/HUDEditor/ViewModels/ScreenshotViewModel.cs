﻿using Avalonia.Media.Imaging;

namespace HUDEditor.ViewModels;

public class ScreenshotViewModel
{
    public Bitmap? ImageSource { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
}