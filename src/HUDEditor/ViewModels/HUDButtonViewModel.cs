﻿using Avalonia.Media.Imaging;
using HUDEditor.Classes;

namespace HUDEditor.ViewModels;

internal class HUDButtonViewModel : ViewModelBase
{
    public HUD Hud { get; }
    public string Name => Hud.Name;
    public string Author => Hud.Author;
    public Bitmap Thumbnail => Hud.ThumbnailImage;
    public bool Unique => Hud.Unique;
    public string Icon => Hud.Unique ? "\u05AE" : "";
    public int Column { get; }
    public int Row { get; }

    public HUDButtonViewModel(HUD hud, int column, int row)
    {
        Hud = hud;
        Column = column;
        Row = row;
    }
}
