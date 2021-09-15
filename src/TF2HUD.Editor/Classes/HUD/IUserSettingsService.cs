using HUDEditor.Models;

namespace HUDEditor.Classes
{
    public interface IUserSettingsService
    {
        UserJson Read();
        void Save(UserJson settings);
    }
}