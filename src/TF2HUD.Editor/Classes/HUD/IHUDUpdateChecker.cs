namespace HUDEditor.Classes
{
    public interface IHUDUpdateChecker
    {
        bool AreEqual(string hud1Name, object hud1, object hud2, string[] ignoreList);
    }
}