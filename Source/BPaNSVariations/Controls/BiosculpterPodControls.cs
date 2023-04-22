using BPaNSVariations.Settings;
using System;
using Verse;

namespace BPaNSVariations.Controls
{
	internal class BiosculpterPodControls : BaseControls
	{
		#region PROPERTIES
		public BiosculpterPodSettings BiosculpterPodSettings => 
			(BiosculpterPodSettings)Settings;

		public override bool CanBeCopied =>
			true;
		#endregion

		#region FIELDS
		private TargetWrapper<BiosculpterPodEffectAnimation> _readyEffectStateTargetWrapper = null;
		private readonly BiosculpterPodEffectAnimation[] _readyEffectStates = (BiosculpterPodEffectAnimation[])Enum.GetValues(typeof(BiosculpterPodEffectAnimation));
		private ThingDefCountClass _regenerationCycleIngredientsNewDef = null;
		#endregion

		#region CONSTRUCTORS
		public BiosculpterPodControls(BiosculpterPodSettings settings) : base(settings)
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
			BiosculpterPodSettings.StandbyPowerConsumption = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.StandbyPowerConsumption".Translate(),
				"SY_BNV.TooltipStandbyPowerConsumption".Translate(),
				BiosculpterPodSettings.StandbyPowerConsumption,
				BiosculpterPodSettings.DefaultStandbyPowerConsumption,
				"StandbyPowerConsumption",
				unit: "W");
			#endregion

			#region READY EFFECT
			// Ready Effect
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorReadyEffect".Translate());
			// Ready Effect - [GLOBAL] Ready Effect State
			if (_readyEffectStateTargetWrapper == null)
				_readyEffectStateTargetWrapper = new TargetWrapper<BiosculpterPodEffectAnimation>(BiosculpterPodSettings.ReadyEffectState);
			BiosculpterPodSettings.ReadyEffectState = CreateDropdownSelectorControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.ReadyEffectState".Translate(),
				"SY_BNV.TooltipReadyEffectState".Translate(),
				BiosculpterPodSettings.ReadyEffectState != BiosculpterPodSettings.DefaultReadyEffectState,
				_readyEffectStateTargetWrapper,
				BiosculpterPodSettings.DefaultReadyEffectState,
				_readyEffectStates,
				state => ReadyEffectStateToString(state).Translate());
			// Ready Effect - Ready Effect Color
			BiosculpterPodSettings.ReadyEffectColor = CreateColorControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.ReadyEffectColor".Translate(),
				"SY_BNV.TooltipReadyEffectColor".Translate(),
				BiosculpterPodSettings.ReadyEffectColor,
				BiosculpterPodSettings.DefaultReadyEffectColor,
				"ReadyEffectColor");
			#endregion

			#region SPECIFIC
			// Specific
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorSpecific".Translate());
			// Specific - [GLOBAL] Nutrition Required
			BiosculpterPodSettings.NutritionRequired = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.NutritionRequired".Translate(),
				"SY_BNV.TooltipNutritionRequired".Translate(),
				BiosculpterPodSettings.NutritionRequired,
				BiosculpterPodSettings.DefaultNutritionRequired,
				"NutritionRequired",
				additionalText: v => "SY_BNV.Nutrition".Translate());
			// Specifics - [GLOBAL] Biotuned Duration
			BiosculpterPodSettings.BiotunedDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BiotunedDuration".Translate(),
				"SY_BNV.TooltipBiotunedDuration".Translate(),
				BiosculpterPodSettings.BiotunedDuration,
				BiosculpterPodSettings.DefaultBiotunedDuration,
				"BiotunedDuration",
				additionalText: TicksToYearText,
				unit: "Ticks");
			// Specific - Biotuned Cycle Speed Factor
			BiosculpterPodSettings.BiotunedCycleSpeedFactor = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BiotunedCycleSpeedFactor".Translate(),
				"SY_BNV.TooltipBiotunedCycleSpeedFactor".Translate(),
				BiosculpterPodSettings.BiotunedCycleSpeedFactor,
				BiosculpterPodSettings.DefaultBiotunedCycleSpeedFactor,
				"BiotunedCycleSpeedFactor",
				additionalText: ValueToPercent);
			// Specific - Overall Speed Factor
			BiosculpterPodSettings.SpeedFactor = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.SpeedFactor".Translate(),
				"SY_BNV.TooltipSpeedFactor".Translate(),
				BiosculpterPodSettings.SpeedFactor,
				BiosculpterPodSettings.DefaultSpeedFactor,
				"SpeedFactor",
				additionalText: ValueToPercent);
			// Specific - [GLOBAL] Cleanliness Effect Curve
			CreateSimpleCurveControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.CleanlinessEffectCurve".Translate(),
				BiosculpterPodSettings.CleanlinessEffectCurve,
				BiosculpterPodSettings.DefaultCleanlinessEffectCurve,
				"CleanlinessEffectCurve");
			BiosculpterPodSettings.ApplyCleanlinessCurve();
			#endregion

			#region MEDIC CYCLE
			// Medic Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorMedicCycle".Translate());
			// Medic Cycle - Duration
			BiosculpterPodSettings.MedicCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.CycleDuration".Translate(),
				"SY_BNV.TooltipCycleDuration".Translate(),
				BiosculpterPodSettings.MedicCycleDuration,
				BiosculpterPodSettings.DefaultMedicCycleDuration,
				"MedicCycleDuration",
				additionalText: DaysToText,
				unit: "d");
			#endregion

			#region REGENERATION CYCLE
			// Regeneration Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorRegenerationCycle".Translate());
			// Regeneration Cycle - Duration
			BiosculpterPodSettings.RegenerationCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.CycleDuration".Translate(),
				"SY_BNV.TooltipCycleDuration".Translate(),
				BiosculpterPodSettings.RegenerationCycleDuration,
				BiosculpterPodSettings.DefaultRegenerationCycleDuration,
				"RegenerationCycleDuration",
				additionalText: DaysToText,
				unit: "d");
			// Regeneration Cycle - Ingredients
			if (_regenerationCycleIngredientsNewDef == null)
				_regenerationCycleIngredientsNewDef = new ThingDefCountClass();
			CreateThingDefListControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.RegenerationCycleIngredients".Translate(),
				ref _regenerationCycleIngredientsNewDef,
				BiosculpterPodSettings.RegenerationCycleIngredients,
				BiosculpterPodSettings.DefaultRegenerationCycleIngredients,
				BPaNSVariations.Settings.MedicineThingDefs,
				"RegenerationCycleIngredients");
			#endregion

			#region AGE REVERSAL CYCLE
			// Age Reversal Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorAgeReversalCycle".Translate());
			// Age Reversal Cycle - Duration
			BiosculpterPodSettings.AgeReversalCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.CycleDuration".Translate(), 
				"SY_BNV.TooltipCycleDuration".Translate(),
				BiosculpterPodSettings.AgeReversalCycleDuration,
				BiosculpterPodSettings.DefaultAgeReversalCycleDuration,
				"AgeReversalCycleDuration",
				additionalText: DaysToText,
				unit: "d");
			// Age Reversal Cycle - [GLOBAL] Age reversed
			BiosculpterPodSettings.AgeReversalCycleAgeReversed = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.AgeReversalCycleAgeReversed".Translate(),
				"SY_BNV.TooltipAgeReversalCycleAgeReversed".Translate(),
				BiosculpterPodSettings.AgeReversalCycleAgeReversed,
				BiosculpterPodSettings.DefaultAgeReversalCycleAgeReversed,
				"AgeReversalCycleAgeReversed",
				additionalText: YearsToText,
				unit: "y");
			#endregion

			#region PLEASURE CYCLE
			// Pleasure Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorPleasureCycle".Translate());
			// Pleasure Cycle - Duration
			BiosculpterPodSettings.PleasureCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.CycleDuration".Translate(), 
				"SY_BNV.TooltipCycleDuration".Translate(),
				BiosculpterPodSettings.PleasureCycleDuration,
				BiosculpterPodSettings.DefaultPleasureCycleDuration,
				"PleasureCycleDuration",
				additionalText: DaysToText,
				unit: "d");
			// Pleasure Cycle - [GLOBAL] Mood Effect
			BiosculpterPodSettings.PleasureCycleMoodEffect = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.PleasureCycleMoodEffect".Translate(),
				"SY_BNV.TooltipPleasureCycleMoodEffect".Translate(),
				BiosculpterPodSettings.PleasureCycleMoodEffect,
				BiosculpterPodSettings.DefaultPleasureCycleMoodEffect,
				"PleasureCycleMoodEffect");
			// Pleasure Cycle - [GLOBAL] Mood Duration
			BiosculpterPodSettings.PleasureCycleMoodDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.PleasureCycleMoodDuration".Translate(),
				"SY_BNV.TooltipPleasureCycleMoodDuration".Translate(),
				BiosculpterPodSettings.PleasureCycleMoodDuration,
				BiosculpterPodSettings.DefaultPleasureCycleMoodDuration,
				"PleasureCycleMoodDuration",
				additionalText: DaysToText,
				unit: "d");
			#endregion
		}
		#endregion

		#region PRIVATE METHODS
		private string ReadyEffectStateToString(BiosculpterPodEffectAnimation state)
		{
			switch (state)
			{
				case BiosculpterPodEffectAnimation.Default:
					return "SY_BNV.BPReadyEffectState_Default";
				case BiosculpterPodEffectAnimation.AlwaysOn:
					return "SY_BNV.BPReadyEffectState_AlwaysOn";
				case BiosculpterPodEffectAnimation.AlwaysOff:
					return "SY_BNV.BPReadyEffectState_AlwaysOff";
			}
			throw new Exception($"{nameof(BPaNSVariations)}.{nameof(BPaNSControls)}.{nameof(ReadyEffectStateToString)}: unknown state encountered: {state}");
		}
		#endregion
	}
}
