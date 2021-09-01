using HUDEditor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HUDEditor.Classes
{
    public interface IUtilities
    {
        List<Tuple<string, string, string>> ItemRarities();
        List<string> CrosshairStyles();
        bool CheckUserPath(string hudPath);
        string CommentTextLine(string[] lines, int index);
        string ConvertToRgba(string hex);
        Dictionary<string, dynamic> CreateNestedObject(Dictionary<string, dynamic> obj, IEnumerable<string> keys);
        void DownloadHud(string url);
        string EncodeID(string id);
        Task<string> Fetch(string url);
        string GetDimmedColor(string rgba);
        string GetGrayedColor(string rgba);
        List<int> GetLineNumbersContainingString(string[] lines, string value);
        string GetLocalizedString(string key);
        string GetPulsedColor(string rgba);
        string GetShadowColor(string rgba);
        bool IsGameRunning();
        void Merge(Dictionary<string, dynamic> object1, Dictionary<string, dynamic> object2);
        void OpenWebpage(string url);
        string GetFileName(Controls control);
        IEnumerable<string> GetFileNames(Controls control);
        bool SearchRegistry();
        string UncommentTextLine(string[] lines, int index);
    }
}