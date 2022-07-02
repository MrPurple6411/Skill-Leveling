namespace Skill_Leveling.Patches
{

    using HarmonyLib;
    using System;
    using UnityEngine;
    using System.Diagnostics;

    [HarmonyPatch]
    public static class Patches
    {
        internal static int SkillXPMultiplier = 1;
        internal static float XPMultiplier = 1;

        /// <summary>
        /// This patch takes the xp gained and multiplies it by the XpMultiplier before applying it to the specified Attribute.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(Character), nameof(Character.AwardExperience), new Type[] { typeof(string), typeof(float), typeof(bool) })]
        [HarmonyPrefix]
        public static void Prefix(Character __instance, ref string attribute, ref float delta, bool modifyByAgeAndPotential)
        {
            SettingsManager settingsManager = Manager<SettingsManager>.Instance;
            CommandManager commandManager = Manager<CommandManager>.Instance;
            if (__instance is null || __instance.stats is null || __instance.attributes is null || 
                settingsManager is null || commandManager is null || 
                (!Manager<FactionManager>.Instance?.IsPlayerFaction(__instance.faction) ?? true)) return;


            try
            {
                // This section of code is here because if we modify the delta when
                // this is being called by the GenerateXP Method when
                // creating a character then the game gets stuck in a infinite while loop.
                StackTrace stackTrace = new StackTrace();
                StackFrame stackFrame = stackTrace.GetFrame(2);
                string methodName = stackFrame.GetMethod().Name;
                if (methodName == "GenerateXP")
                {
                    return;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: \n{ex}");
                return;
            }

            var multipliedSkillXP = delta * SkillXPMultiplier;
            delta *= XPMultiplier;
            if (attribute != null)
            {
                int index = -1;
                for (int i = 0; i < __instance.attributes.Length; i++)
                {
                    if (__instance.attributes[i].name == attribute)
                    {
                        index = i;
                        break;
                    }
                }
                if(index == -1)
                    return;


                if (modifyByAgeAndPotential)
                {
                    multipliedSkillXP *= 1f + __instance.GetAttributeRawPotential(index) / 5f;
                    multipliedSkillXP *= 4f / settingsManager.GetGameFloatValue("Oct.Settings.Game.DaysPerYear");
                    multipliedSkillXP *= Character.AgeXPRate(__instance.age, __instance.type.lifespan);
                }

                CharacterAttribute[] attributes = __instance.attributes;
                attributes[index].xp += multipliedSkillXP;

                int attributeLevel = OctoberMath.FloorToInt(__instance.stats.GetStatValue(attribute));
                float experienceNeeded = __instance.ExperienceNeeded(attributeLevel) / 2f;
                
                if (attributes[index].xp >= experienceNeeded && attributeLevel < __instance.level + 1)
                {
                    __instance.stats.SetStat(attribute, attributeLevel + 1);
                    attributes[index].xp -= experienceNeeded;
                    commandManager.AutoAssignJobsDirty();
                }
                //set attribute to null so that the original method doesnt add xp to the attribute as well.
                attribute = null;
                // modify the xp gained by the attribute potential just like the original would have if we didn't nullify the attribute parameter.
                delta *= 1f + __instance.GetAttributeRawPotential(index) / 5f;
            }
        }

        /// <summary>
        /// Makes it so Player Nobles will never hit the level cap of 20 and so will be able to keep gaining skill xp forever.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(AbilityCharacterFacet), nameof(AbilityCharacterFacet.AddAbilityOfType))]
        [HarmonyPostfix]
        public static void Postfix(AbilityCharacterFacet __instance)
        {
            if (Manager<FactionManager>.Instance.IsPlayerFaction(__instance.character.faction) && __instance.character.level >= 20)
            {
                __instance.character.level = 19;
            }
        }

        /// <summary>
        /// Makes it so Player Nobles auto level up to the next level for levels that will not give a skill choice.
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(Character), nameof(Character.NotifyOrAddLevel))]
        [HarmonyPrefix]
        public static bool Prefix(Character __instance)
        {
            bool addedLevel = false;
            if (Manager<FactionManager>.Instance.IsPlayerFaction(__instance.faction))
            {
                if(__instance.level+1 < (__instance.abilities.AcquiredAbilityCount() + 1) * 5)
                {
                    __instance.AddLevel();
                    addedLevel= true;
                }
                if (addedLevel && __instance.level == 20)
                {
                    __instance.level = 19;
                }
            }
            return !addedLevel;
        }
    }
}
