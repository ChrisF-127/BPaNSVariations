using BPaNSVariations.Settings;
using BPaNSVariations.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BPaNSVariations.Controls
{
	internal class BiosculpterPodControls : BaseControls
	{
		#region PROPERTIES
		public BiosculpterPodSettings BiosculpterPodSettings => 
			(BiosculpterPodSettings)Settings;
		#endregion

		#region FIELDS
		private ThingDefCountClass _buildCostNewDef = null;
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
			// Biosculpter Pod
			CreateTitle(
				ref offsetY,
				viewWidth,
				Label);

			#region GENERAL
			// Biosculpter Pod - General
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorGeneral".Translate());
			// Biosculpter Pod - General - Active Power Consumption
			BiosculpterPodSettings.ActivePowerConsumption = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.ActivePowerConsumption".Translate(),
				"SY_BNV.TooltipActivePowerConsumption".Translate(),
				BiosculpterPodSettings.ActivePowerConsumption,
				BiosculpterPodSettings.DefaultActivePowerConsumption,
				"ActivePowerConsumption",
				unit: "W");
			// Biosculpter Pod - General - Standby Power Consumption
			BiosculpterPodSettings.StandbyPowerConsumption = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.StandbyPowerConsumption".Translate(),
				"SY_BNV.TooltipStandbyPowerConsumption".Translate(),
				BiosculpterPodSettings.StandbyPowerConsumption,
				BiosculpterPodSettings.DefaultStandbyPowerConsumption,
				"StandbyPowerConsumption",
				unit: "W");
			// Biosculpter Pod - General - Build Cost
			if (_buildCostNewDef == null)
				_buildCostNewDef = new ThingDefCountClass();
			CreateThingDefListControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.BuildCost".Translate(),
				ref _buildCostNewDef,
				BiosculpterPodSettings.BuildCost,
				BiosculpterPodSettings.DefaultBuildCost,
				BPaNSVariations.Settings.BuildCostThingDefs,
				"BuildCost");
			// Biosculpter Pod - General - Work to Build
			BiosculpterPodSettings.WorkToBuild = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.WorkToBuild".Translate(),
				"SY_BNV.TooltipWorkToBuild".Translate(),
				BiosculpterPodSettings.WorkToBuild,
				BiosculpterPodSettings.DefaultWorkToBuild,
				"WorkToBuild",
				additionalText: WorkToBuildToWorkLeft);
			#endregion

			#region READY EFFECT
			// Biosculpter Pod - Ready Effect
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorReadyEffect".Translate());
			// Biosculpter Pod - Ready Effect - [GLOBAL] Ready Effect Animation
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
			// Biosculpter Pod - Ready Effect - Ready Effect Color
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
			// Biosculpter Pod - Specific
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorSpecific".Translate());
			// Biosculpter Pod - Specific - [GLOBAL] Nutrition Required
			BiosculpterPodSettings.NutritionRequired = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.NutritionRequired".Translate(),
				"SY_BNV.TooltipNutritionRequired".Translate(),
				BiosculpterPodSettings.NutritionRequired,
				BiosculpterPodSettings.DefaultNutritionRequired,
				"NutritionRequired",
				additionalText: v => "SY_BNV.Nutrition".Translate());
			// Biosculpter Pod - Specifics - [GLOBAL] Biotuned Duration
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
			// Biosculpter Pod - Specific - Biotuned Cycle Speed Factor
			BiosculpterPodSettings.BiotunedCycleSpeedFactor = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BiotunedCycleSpeedFactor".Translate(),
				"SY_BNV.TooltipBiotunedCycleSpeedFactor".Translate(),
				BiosculpterPodSettings.BiotunedCycleSpeedFactor,
				BiosculpterPodSettings.DefaultBiotunedCycleSpeedFactor,
				"BiotunedCycleSpeedFactor",
				additionalText: ValueToPercent);
			// Biosculpter Pod - Specific - Overall Speed Factor
			BiosculpterPodSettings.SpeedFactor = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.SpeedFactor".Translate(),
				"SY_BNV.TooltipSpeedFactor".Translate(),
				BiosculpterPodSettings.SpeedFactor,
				BiosculpterPodSettings.DefaultSpeedFactor,
				"SpeedFactor",
				additionalText: ValueToPercent);
			// Biosculpter Pod - Specific - [GLOBAL] Cleanliness Effect Curve
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
			// Biosculpter Pod - Medic Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorMedicCycle".Translate());
			// Biosculpter Pod - Medic Cycle - Duration
			BiosculpterPodSettings.MedicCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.MedicCycleDuration".Translate(),
				"SY_BNV.TooltipMedicCycleDuration".Translate(),
				BiosculpterPodSettings.MedicCycleDuration,
				BiosculpterPodSettings.DefaultMedicCycleDuration,
				"MedicCycleDuration",
				additionalText: DaysToText,
				unit: "d");
			#endregion

			#region REGENERATION CYCLE
			// Biosculpter Pod - Regeneration Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorRegenerationCycle".Translate());
			// Biosculpter Pod - Regeneration Cycle - Duration
			BiosculpterPodSettings.RegenerationCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.RegenerationCycleDuration".Translate(),
				"SY_BNV.TooltipRegenerationCycleDuration".Translate(),
				BiosculpterPodSettings.RegenerationCycleDuration,
				BiosculpterPodSettings.DefaultRegenerationCycleDuration,
				"RegenerationCycleDuration",
				additionalText: DaysToText,
				unit: "d");
			// Biosculpter Pod - General Settings - Build Cost
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
			// Biosculpter Pod - Age Reversal Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorAgeReversalCycle".Translate());
			// Biosculpter Pod - Age Reversal Cycle - Duration
			BiosculpterPodSettings.AgeReversalCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.AgeReversalCycleDuration".Translate(),
				"SY_BNV.TooltipAgeReversalCycleDuration".Translate(),
				BiosculpterPodSettings.AgeReversalCycleDuration,
				BiosculpterPodSettings.DefaultAgeReversalCycleDuration,
				"AgeReversalCycleDuration",
				additionalText: DaysToText,
				unit: "d");
			// Biosculpter Pod - Age Reversal Cycle - [GLOBAL] Age reversed
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
			// Biosculpter Pod - Pleasure Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorPleasureCycle".Translate());
			// Biosculpter Pod - Pleasure Cycle - Duration
			BiosculpterPodSettings.PleasureCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.PleasureCycleDuration".Translate(),
				"SY_BNV.TooltipPleasureCycleDuration".Translate(),
				BiosculpterPodSettings.PleasureCycleDuration,
				BiosculpterPodSettings.DefaultPleasureCycleDuration,
				"PleasureCycleDuration",
				additionalText: DaysToText,
				unit: "d");
			// Biosculpter Pod - Pleasure Cycle - [GLOBAL] Mood Effect
			BiosculpterPodSettings.PleasureCycleMoodEffect = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.PleasureCycleMoodEffect".Translate(),
				"SY_BNV.TooltipPleasureCycleMoodEffect".Translate(),
				BiosculpterPodSettings.PleasureCycleMoodEffect,
				BiosculpterPodSettings.DefaultPleasureCycleMoodEffect,
				"PleasureCycleMoodEffect");
			// Biosculpter Pod - Pleasure Cycle - [GLOBAL] Mood Duration
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

			// Margin
			offsetY += SettingsRowHeight / 2;
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
