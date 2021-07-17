using Newtonsoft.Json;
using HarmonyLib;
using PeterHan.PLib.Options;
using PeterHan.PLib.Core;
using System.Reflection;

namespace SystemClockRemake
{
    // Load harmony and PLib
    public class SystemClock : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            new POptions().RegisterOptions(this, typeof(SystemClockOptions));
        }
    }
    // Config format
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("https://github.com/TheLurkingCat/ONI-SystemClock")]
    public class SystemClockOptions
    {
        [Option("Time Format", "Format string of time, which will be passed to System.DateTime.ToString")]
        [JsonProperty]
        public string TimeStringFormat { get; set; }
        public SystemClockOptions()
        {
            TimeStringFormat = "HH:mm:ss";
        }
    }
    // Use PLib.Options to load config.
    [HarmonyPatch(typeof(Game), "Load")]
    public static class PatchGameLoad
    {
        public static SystemClockOptions Option { get; private set; }

        public static void Prefix()
        {
            ReadSettings();
        }

        public static void ReadSettings()
        {
            Option = POptions.ReadSettings<SystemClockOptions>() ?? new SystemClockOptions();
        }
    }
    // Refresh clock per second
    [HarmonyPatch(typeof(MeterScreen), "Refresh")]
    public static class PatchMeterScreenRender
    {
        public static void Postfix()
        {
            // New Game doesn't call load to read settings
            if (PatchGameLoad.Option is null)
            {
                PatchGameLoad.ReadSettings();
            }
            // Check screen spawned
            var ColonyUI = TopLeftControlScreen.Instance?.GetLocText();
            if ((ColonyUI?.text) is null)
                return;

            ColonyUI.text = SaveGame.Instance.BaseName + "\n";
            try
            {
                ColonyUI.text += System.DateTime.Now.ToString(PatchGameLoad.Option.TimeStringFormat);
            }
            catch (System.FormatException)
            {
                ColonyUI.text += System.DateTime.Now.ToString("HH:mm:ss");
            }
        }
    }
    // Reflection
    public static class TopLeftControlScreenExtension
    {
        public static LocText GetLocText(this TopLeftControlScreen obj)
        {
            var field = obj.GetType().GetField("locText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return (LocText)field?.GetValue(obj);
            
        }
    }
}
