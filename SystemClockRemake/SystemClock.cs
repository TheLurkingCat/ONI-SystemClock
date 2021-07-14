using Newtonsoft.Json;
using HarmonyLib;
using PeterHan.PLib.Options;
using PeterHan.PLib.Core;

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
            Option = POptions.ReadSettings<SystemClockOptions>() ?? new SystemClockOptions();
        }
    }
    // Refresh clock per second
    [HarmonyPatch(typeof(MeterScreen), "Render1000ms")]
    public static class PatchMeterScreenRender
    {
        public static void Postfix()
        {
            TopLeftControlScreen.Instance.RefreshName();
        }
    }
    // Append text to colony name
    [HarmonyPatch(typeof(TopLeftControlScreen), "RefreshName")]
    public static class PatchColonyRefreshName
    {
        public static void Postfix(ref LocText ___locText)
        {
            if (SaveGame.Instance == null)
                return;
            ___locText.text += string.Format("\n{0}", System.DateTime.Now.ToString(PatchGameLoad.Option.TimeStringFormat));
        }
    }
}