namespace Skill_Leveling.Settings
{
    using System;
    using Patches;
    using UnityEngine;

    internal class SkillExperienceMultiplierSetting : SliderSettingDefinition
    {
        public SkillExperienceMultiplierSetting(OctDatGlobalInitializer initializer) : base(initializer)
        {
            this.id = "Oct.Settings.MySettings.SkillXpMultiplier";
            this.name = "Skill Experience Multiplier";
            this.order = Main.SettingsCategory.settings.Count;
            this.category = Main.SettingsCategory;
            this.min = 0f;
            this.max = 50f;
            this.step = 1;
            this.unit = "X";
            this.defaultValue = 1f;
        }

        public override void Apply()
        {
            Patches.SkillXPMultiplier = (int)Math.Round((float)this.GetValue());

            base.Apply();
        }

        public override Color LabelColor()
        {
            return Color.magenta;
        }
    }
    internal class ExperienceMultiplierSetting : SliderSettingDefinition
    {
        public ExperienceMultiplierSetting(OctDatGlobalInitializer initializer) : base(initializer)
        {
            this.id = "Oct.Settings.MySettings.XpMultiplier";
            this.name = "Experience Multiplier";
            this.order = Main.SettingsCategory.settings.Count;
            this.category = Main.SettingsCategory;
            this.min = 0f;
            this.max = 5f;
            this.step = 0.1f;
            this.unit = "X";
            this.defaultValue = 1f;
        }

        public override void Apply()
        {
            Patches.XPMultiplier = (float)this.GetValue();
            base.Apply();
        }

        public override Color LabelColor()
        {
            return Color.magenta;
        }
    }

}
