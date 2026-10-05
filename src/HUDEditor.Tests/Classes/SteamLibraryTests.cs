using System;
using System.Collections.Generic;
using System.IO;
using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests.Classes;

/// <summary>
/// Tests for reading Steam library locations from libraryfolders.vdf.
/// </summary>
public class SteamLibraryTests
{
    [Fact]
    public void ParseLibraryFolders_ExtractsPathsFromValidVdfContent()
    {
        var paths = Parse("""
            "path" "C:\Program Files (x86)\Steam"
            "path" "D:\Games\Steam"
            "path" "E:\SteamLibrary"
            """);

        Assert.Contains(@"C:\Program Files (x86)\Steam", paths);
        Assert.Contains(@"D:\Games\Steam", paths);
        Assert.Contains(@"E:\SteamLibrary", paths);
    }

    [Fact]
    public void ParseLibraryFolders_HandlesBackslashEscapes()
    {
        var paths = Parse("""
            "path" "C:\\Program Files\\Steam"
            """);

        Assert.Equal(@"C:\Program Files\Steam", Assert.Single(paths));
    }

    [Fact]
    public void ParseLibraryFolders_IgnoresInvalidLines()
    {
        var paths = Parse("""
            "path" "C:\Steam"
            random garbage line
                invalid line
            "path" "D:\Games"
            #comment
            other text
            """);

        Assert.Equal(new[] { @"C:\Steam", @"D:\Games" }, paths);
    }

    [Fact]
    public void ParseLibraryFolders_ReturnsEmptyListForEmptyFile()
    {
        Assert.Empty(Parse(string.Empty));
    }

    private static List<string> Parse(string vdfContent)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"libraryfolders_{Guid.NewGuid():N}.vdf");
        File.WriteAllText(tempFile, vdfContent);
        try
        {
            return [.. Utilities.ParseLibraryFolders(tempFile)];
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
