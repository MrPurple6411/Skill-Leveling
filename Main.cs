namespace Skill_Leveling
{
    using HarmonyLib;
    using Skill_Leveling.Settings;
    using System.Reflection;
    using UnityEngine;

    public class Main : BaseManager
    {
        internal static readonly SettingsCategory SettingsCategory;
        
        static Main()
        {
            var ModName = Assembly.GetExecutingAssembly().GetName().Name;

            Debug.Log($"[{ModName}] Loading");
            new Harmony($"MrPurple6411.{ModName}").PatchAll();
            Debug.Log($"[{ModName}] Patched Successfully");

            SettingsCategory = Manager<SettingsManager>.Instance.globalSettingsCategories.Find(x => x.id == "MrPurple6411_Settings");
            if (SettingsCategory == null)
            {
                SettingsCategory = new SettingsCategory(new OctDatGlobalInitializer())
                {
                    id = "MrPurple6411_Settings",
                    name = "MrPurple's Mod Settings",
                    canReset = true,
                    global = true,
                    order = -1000
                };
                SettingsCategory.PostInit();
            }
            SettingsCategory.AddSetting(new ExperienceMultiplierSetting(new OctDatGlobalInitializer()));
            SettingsCategory.AddSetting(new SkillExperienceMultiplierSetting(new OctDatGlobalInitializer()));
        }
    }
}
