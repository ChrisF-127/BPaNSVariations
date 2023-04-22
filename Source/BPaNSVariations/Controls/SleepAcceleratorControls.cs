using BPaNSVariations.Settings;
using Verse;

namespace BPaNSVariations.Controls
{
	internal class SleepAcceleratorControls : BaseControls
	{
		#region PROPERTIES
		public SleepAcceleratorSettings SleepAcceleratorSettings => 
			(SleepAcceleratorSettings)Settings;
		#endregion

		#region CONSTRUCTORS
		public SleepAcceleratorControls(SleepAcceleratorSettings settings) : base(settings)
		{
		}
		#endregion

		#region OVERRIDES
		public override void CreateSettings(ref float offsetY, float viewWidth)
		{
			// General
			base.CreateSettings(ref offsetY, viewWidth);

			#region GENERAL
			// General - Standby Power Consumption
			SleepAcceleratorSettings.InUsePowerConsumption = CreateNullableNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.InUsePowerConsumption".Translate(),
				"SY_BNV.InUsePowerConsumptionDisabled".Translate(),
				"SY_BNV.TooltipInUsePowerConsumption".Translate(),
				"SY_BNV.TooltipInUsePowerConsumptionCheckbox".Translate(),
				SleepAcceleratorSettings.InUsePowerConsumption,
				SleepAcceleratorSettings.DefaultInUsePowerConsumption,
				"InUsePowerConsumption",
				unit: "W");
			#endregion

			#region SPECIFIC
			// Specific
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorSpecific".Translate());
			// Specific - [GLOBAL] Anyone Can Build
			SleepAcceleratorSettings.AnyoneCanBuild = CreateCheckbox(
				ref offsetY,
				viewWidth,
				"SY_BNV.AnyoneCanBuild".Translate(),
				"SY_BNV.TooltipAnyoneCanBuild".Translate(),
				SleepAcceleratorSettings.AnyoneCanBuild,
				SleepAcceleratorSettings.DefaultAnyoneCanBuild,
				"SY_BNV.DescAnyoneCanBuild".Translate());
			// Specific - Bed Rest Effectiveness
			SleepAcceleratorSettings.BedRestEffectiveness = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BedRestEffectiveness".Translate(),
				"SY_BNV.TooltipBedRestEffectiveness".Translate(),
				SleepAcceleratorSettings.BedRestEffectiveness,
				SleepAcceleratorSettings.DefaultBedRestEffectiveness,
				"BedRestEffectiveness",
				additionalText: ValueToPercent);
			// Specific - Bed Hunger Rate Factor
			SleepAcceleratorSettings.BedHungerRateFactor = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BedHungerRateFactor".Translate(),
				"SY_BNV.TooltipBedHungerRateFactor".Translate(),
				SleepAcceleratorSettings.BedHungerRateFactor,
				SleepAcceleratorSettings.DefaultBedHungerRateFactor,
				"BedHungerRateFactor",
				additionalText: ValueToPercent);
			#endregion


			// Margin
			offsetY += SettingsRowHeight / 2;
		}
		#endregion
	}
}
