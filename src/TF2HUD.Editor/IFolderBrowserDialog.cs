using System;
using System.Windows.Forms;

namespace HUDEditor
{
    public interface IFolderBrowserDialog : IDisposable
    {
        string Description { get; set; }
        string SelectedPath { get; set; }
        bool ShowNewFolderButton { get; set; }
        bool UseDescriptionForTitle { get; set; }

        DialogResult ShowDialog();
    }
}