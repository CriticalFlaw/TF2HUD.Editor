using HUDEditor.Models;

namespace HUDEditor.Classes
{
    public interface IHUDFactory
    {
        HUD CreateHUD(string name, HudJson schema, bool uniq);
    }
}