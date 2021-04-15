using System.IO;

namespace TF2HUD.Editor.Classes
{
    /// <summary>
    ///     <para>Handles the options and priority of a HUD's custom background</para>
    ///     <code>this.BackgroundManager = new BackgroundManager();</code>
    ///     <code>this.BackgroundManager.SetStockBackgrounds();</code>
    ///     <code>this.BackgroundManager.SetCustomBackground("background.png");</code>
    ///     <code>this.BackgroundManager.Apply();</code>
    ///     <para>
    ///         In this example, we set stock backgrounds AND a custom background.
    ///         When calling the apply method, the BackgroundManager will override
    ///         the stock backgrounds with the custom background and apply changes
    ///         to the HUD
    ///     </para>
    /// </summary>
    internal class BackgroundManager
    {
        private string customImagePath;
        private readonly string HUDFolderPath;
        private bool useCustomBackground;
        private bool useStockBackgrounds;

        public BackgroundManager(string hudPath)
        {
            HUDFolderPath = hudPath;
        }

        public void SetStockBackgrounds(bool enable)
        {
            useStockBackgrounds = enable;
        }

        public void SetCustomBackground(string imagePath)
        {
            useCustomBackground = true;
            customImagePath = imagePath;
        }

        public void Apply()
        {
            var path = HUDFolderPath;

            // Revert everything back to normal before changing the name extension.
            var directoryPath = new DirectoryInfo(path + "\\materials\\console");
            foreach (var file in directoryPath.GetFiles())
            {
                var target = file.FullName;
                if (file.Name.EndsWith("bak"))
                {
                    target = target.Replace("bak", "vtf");
                    if (File.Exists(target)) File.Delete(target);
                    File.Move(file.FullName, target);
                }

                if (file.Name.EndsWith("temp"))
                {
                    target = target.Replace("temp", "vmt");
                    if (File.Exists(target)) File.Delete(target);
                    File.Move(file.FullName, target);
                }
            }

            // Do the same for the chapter backgrounds file as well.
            var chapterBackgrounds = path + "\\scripts\\chapterbackgrounds.bak";
            if (File.Exists(chapterBackgrounds))
                File.Move(chapterBackgrounds, chapterBackgrounds.Replace(".bak", ".txt"));

            // If we're not enabling stock background, then skip this process.
            if (!useStockBackgrounds) return;

            // Rename the file extensions so that the game does not use them.
            foreach (var file in directoryPath.GetFiles())
            {
                var target = file.FullName;
                if (file.Name.EndsWith("vtf"))
                {
                    target = target.Replace("vtf", "bak");
                    if (File.Exists(target)) File.Delete(target);
                    File.Move(file.FullName, target);
                }

                if (file.Name.EndsWith("vmt"))
                {
                    target = target.Replace("vmt", "temp");
                    if (File.Exists(target)) File.Delete(target);
                    File.Move(file.FullName, target);
                }
            }

            if (File.Exists(chapterBackgrounds.Replace(".bak", ".txt")))
                File.Move(chapterBackgrounds.Replace(".bak", ".txt"), chapterBackgrounds);
        }
    }
}