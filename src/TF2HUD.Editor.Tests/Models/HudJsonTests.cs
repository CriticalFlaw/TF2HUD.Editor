using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using HUDEditor.Models;

namespace HUDEditorTests.Models.Tests;

[TestClass()]
public class HudJsonTests
{
    [TestMethod()]
    public async Task HudJsonDiscordTest()
    {
        var jsonFiles = Directory.EnumerateFiles("..\\..\\..\\..\\TF2HUD.Editor\\JSON")
            .Select((jsonFile) => JsonConvert.DeserializeObject<HudJson>(File.ReadAllText(jsonFile)))
            .OfType<HudJson>()
            .Where((hudJson) => hudJson.Links.Discord != null)
            .ToArray();

        var statusCodes = await Task.WhenAll(
            jsonFiles.Select(async (hudJson) =>
            {
                var invite = new Uri(hudJson.Links.Discord).LocalPath.Split("/").Last();
                var response = await new HttpClient().GetAsync($"https://discord.com/api/v10/invites/{invite}");
                return response.StatusCode;
            })
        );

        CollectionAssert.AreEqual(
            Enumerable.Repeat(HttpStatusCode.OK, jsonFiles.Length).ToArray(),
            statusCodes
        );
    }
}
