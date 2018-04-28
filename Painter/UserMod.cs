using Harmony;
using ICities;
using Painter.TranslationFramework;
using System.Reflection;

namespace Painter
{
    public class UserMod : IUserMod
    {
        public string Name => "Painter";
        public string Description => Translation.GetTranslation("PAINTER-DESCRIPTION");
        public static Translation Translation = new Translation();
        public void OnEnabled()
        {
            var harmony = HarmonyInstance.Create("com.tpb.painter");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
