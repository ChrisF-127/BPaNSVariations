using BPaNSVariations.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BPaNSVariations.Settings
{
	internal enum BiosculpterPodEffectAnimation
	{
		Default,
		AlwaysOn,
		AlwaysOff,
	}

	internal class BiosculpterPodSettings : BaseSettings
	{
		#region PROPERTIES
		public ThingDef BiosculpterPod { get; }

		public static EffecterDef ReadyEffecterDef { get; private set; }
		public static FleckDef ReadyEffecterFleckDef { get; private set; }
		public static (float FadeIn, float FadeOut, float Solid) OriginalReadyEffecterValues { get; private set; } // FadeIn, FadeOut, Solid

		#region GENERAL
		public float DefaultActivePowerConsumption { get; private set; }
		public float ActivePowerConsumption
		{
			get => BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_Power>().basePowerConsumption;
			set => BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_Power>().basePowerConsumption = value;
		}
		public float DefaultStandbyPowerConsumption { get; private set; }
		public float StandbyPowerConsumption
		{
			get => BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_Power>().idlePowerDraw;
			set => BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_Power>().idlePowerDraw = value;
		}
		public List<ThingDefCountClass> DefaultBuildCost { get; } = new List<ThingDefCountClass>();
		public List<ThingDefCountClass> BuildCost => BiosculpterPod.costList;
		public float DefaultWorkToBuild { get; private set; }
		public float WorkToBuild
		{
			get => BiosculpterPod.GetStatValueAbstract(StatDefOf.WorkToBuild);
			set => BiosculpterPod.SetStatBaseValue(StatDefOf.WorkToBuild, value);
		}
		#endregion

		#region READY EFFECT
		public static BiosculpterPodEffectAnimation DefaultReadyEffectState { get; private set; }
		private static BiosculpterPodEffectAnimation _readyEffectState;
		public static BiosculpterPodEffectAnimation ReadyEffectState
		{
			get => _readyEffectState;
			set => _readyEffectState = SetReadyEffectState(value);
		}
		public Color DefaultReadyEffectColor { get; private set; }
		public Color ReadyEffectColor
		{
			get => BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_BiosculpterPod>().selectCycleColor;
			set => BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_BiosculpterPod>().selectCycleColor = value;
		}
		#endregion

		#region SPECIFIC
		public static float DefaultNutritionRequired { get; private set; }
		public static float NutritionRequired { get; set; } // applied via several CompBiosculpterPod-Transpilers
		public static int DefaultBiotunedDuration { get; private set; }
		public static int BiotunedDuration { get; set; } // applied via CompBiosculpterPod.SetBiotuned-Transpiler
		public float DefaultBiotunedCycleSpeedFactor { get; private set; }
		public float BiotunedCycleSpeedFactor
		{
			get => BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_BiosculpterPod>().biotunedCycleSpeedFactor;
			set => BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_BiosculpterPod>().biotunedCycleSpeedFactor = value;
		}
		public float DefaultSpeedFactor { get; private set; }
		public float SpeedFactor
		{
			get => BiosculpterPod.GetStatValueAbstract(StatDefOf.BiosculpterPodSpeedFactor);
			set => BiosculpterPod.SetStatBaseValue(StatDefOf.BiosculpterPodSpeedFactor, value);
		}
		public static SimpleCurve DefaultCleanlinessEffectCurve { get; private set; }
		public static SimpleCurve CleanlinessEffectCurve => RoomStatDefOf.BiosculpterPodSpeedFactor.curve;
		#endregion

		#region MEDIC CYCLE
		public float DefaultMedicCycleDuration { get; private set; }
		public float MedicCycleDuration
		{
			get => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_MedicCycle>().durationDays;
			set => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_MedicCycle>().durationDays = value;
		}
		#endregion

		#region REGENERATION CYCLE
		public float DefaultRegenerationCycleDuration { get; private set; }
		public float RegenerationCycleDuration
		{
			get => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>().durationDays;
			set => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>().durationDays = value;
		}
		public List<ThingDefCountClass> DefaultRegenerationCycleIngredients { get; } = new List<ThingDefCountClass>();
		public List<ThingDefCountClass> RegenerationCycleIngredients => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>().extraRequiredIngredients;
		#endregion

		#region AGE REVERSAL CYCLE
		public float DefaultAgeReversalCycleDuration { get; private set; }
		public float AgeReversalCycleDuration
		{
			get => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_AgeReversalCycle, CompBiosculpterPod_AgeReversalCycle>().durationDays;
			set => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_AgeReversalCycle, CompBiosculpterPod_AgeReversalCycle>().durationDays = value;
		}
		public static float DefaultAgeReversalCycleAgeReversed { get; private set; }
		public static float AgeReversalCycleAgeReversed { get; set; } // applied via CompBiosculpterPod_AgeReversalCycle.CycleCompleted-Transpiler
		#endregion

		#region PLEASURE CYCLE
		public float DefaultPleasureCycleDuration { get; private set; }
		public float PleasureCycleDuration
		{
			get => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_PleasureCycle, CompBiosculpterPod_PleasureCycle>().durationDays;
			set => BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_PleasureCycle, CompBiosculpterPod_PleasureCycle>().durationDays = value;
		}
		public static float DefaultPleasureCycleMoodEffect { get; private set; }
		public static float PleasureCycleMoodEffect
		{
			get => ThoughtDefOf.BiosculpterPleasure.stages.First().baseMoodEffect;
			set => ThoughtDefOf.BiosculpterPleasure.stages.First().baseMoodEffect = value;
		}
		public static float DefaultPleasureCycleMoodDuration { get; private set; }
		public static float PleasureCycleMoodDuration
		{
			get => ThoughtDefOf.BiosculpterPleasure.durationDays;
			set => ThoughtDefOf.BiosculpterPleasure.durationDays = value;
		}
		#endregion
		#endregion

		#region CONSTRUCTORS
		public BiosculpterPodSettings(ThingDef biosculpterPod)
		{
			BiosculpterPod = biosculpterPod;

			Initialize();
		}
		#endregion

		#region PUBLIC METHODS
		public static void InitializeStatics()
		{
			var operatingEffecterDef = DefDatabase<EffecterDef>.GetNamed("BiosculpterPod_Operating");
			ReadyEffecterDef = DefDatabase<EffecterDef>.GetNamed("BiosculpterPod_Ready");

			var forwardFleckDef = DefDatabase<FleckDef>.GetNamed("BiosculpterScanner_Forward");
			var backwardFleckDef = DefDatabase<FleckDef>.GetNamed("BiosculpterScanner_Backward");
			ReadyEffecterFleckDef = DefDatabase<FleckDef>.GetNamed("BiosculpterScanner_Ready");

			// Fix effecter position; necessary since we make the effect appear between the interaction spot and 1.5 cells away from it depending on rotation, 
			//	but it needs to be 1 cells away, which TargetInfo does not allow for without giving it a Thing with a fitting center which we do not have on 2x2
			operatingEffecterDef.offsetTowardsTarget = new FloatRange(0.5f, 0.5f);
			ReadyEffecterDef.offsetTowardsTarget = new FloatRange(0.5f, 0.5f);

			// Resize FleckDefs for the Effecters to look more fitting for the smaller buildings
			forwardFleckDef.graphicData.drawSize = new Vector2(1.5f, 0.5f); // standard is 3x1
			backwardFleckDef.graphicData.drawSize = new Vector2(1f, 0.5f); // standard is 2x1
			ReadyEffecterFleckDef.graphicData.drawSize = new Vector2(1f, 2f); // standard is 2x2
			OriginalReadyEffecterValues = (ReadyEffecterFleckDef.fadeInTime, ReadyEffecterFleckDef.fadeOutTime, ReadyEffecterFleckDef.solidTime);


			#region SPECIFIC
			NutritionRequired = DefaultNutritionRequired = CompBiosculpterPod.NutritionRequired;
			BiotunedDuration = DefaultBiotunedDuration = 4800000; // hardcoded
			DefaultCleanlinessEffectCurve = new SimpleCurve(CleanlinessEffectCurve);
			ApplyCleanlinessCurve();
			#endregion

			#region READY EFFECT
			_readyEffectState = DefaultReadyEffectState = BiosculpterPodEffectAnimation.Default;
			#endregion

			#region AGE REVERSAL CYCLE
			AgeReversalCycleAgeReversed = DefaultAgeReversalCycleAgeReversed = 1f; // hardcoded, 1 year = 3'600'000 ticks
			#endregion

			#region PLEASURE CYCLE
			DefaultPleasureCycleMoodEffect = PleasureCycleMoodEffect;
			DefaultPleasureCycleMoodDuration = PleasureCycleMoodDuration;
			#endregion
		}

		public static void ExposeStatics()
		{
			#region SPECIFIC
			float floatValue = NutritionRequired;
			Scribe_Values.Look(ref floatValue, nameof(NutritionRequired), DefaultNutritionRequired);
			NutritionRequired = floatValue;

			int intValue = BiotunedDuration;
			Scribe_Values.Look(ref intValue, nameof(BiotunedDuration), DefaultBiotunedDuration);
			BiotunedDuration = intValue;

			BPaNSUtility.ExposeSimpleCurve(CleanlinessEffectCurve, nameof(CleanlinessEffectCurve), () => !CleanlinessEffectCurve.SequenceEqual(DefaultCleanlinessEffectCurve));
			ApplyCleanlinessCurve();
			#endregion

			#region READY EFFECT
			var bpreState = ReadyEffectState;
			Scribe_Values.Look(ref bpreState, nameof(ReadyEffectState), DefaultReadyEffectState);
			ReadyEffectState = bpreState;
			#endregion

			#region AGE REVERSAL CYCLE
			floatValue = AgeReversalCycleAgeReversed;
			Scribe_Values.Look(ref floatValue, nameof(AgeReversalCycleAgeReversed), DefaultAgeReversalCycleAgeReversed);
			AgeReversalCycleAgeReversed = floatValue;
			#endregion

			#region PLEASURE CYCLE
			floatValue = PleasureCycleMoodEffect;
			Scribe_Values.Look(ref floatValue, nameof(PleasureCycleMoodEffect), DefaultPleasureCycleMoodEffect);
			PleasureCycleMoodEffect = floatValue;

			floatValue = PleasureCycleMoodDuration;
			Scribe_Values.Look(ref floatValue, nameof(PleasureCycleMoodDuration), DefaultPleasureCycleMoodDuration);
			PleasureCycleMoodDuration = floatValue;
			#endregion
		}

		public static void ApplyCleanlinessCurve()
		{
			RoomStatDefOf.BiosculpterPodSpeedFactor.roomlessScore = CleanlinessEffectCurve.MinBy(v => v.x).y;
		}
		#endregion

		#region OVERRIDES
		public override string GetName() =>
			BiosculpterPod.LabelCap;
		public override bool IsModified() =>
			DefaultActivePowerConsumption != ActivePowerConsumption
			|| DefaultStandbyPowerConsumption != StandbyPowerConsumption
			|| DefaultBuildCost.AnyDifference(BuildCost)
			|| DefaultWorkToBuild != WorkToBuild
			//|| DefaultReadyEffectState != ReadyEffectState
			|| DefaultReadyEffectColor != ReadyEffectColor
			//|| DefaultNutritionRequired != NutritionRequired
			//|| DefaultBiotunedDuration != BiotunedDuration
			|| DefaultBiotunedCycleSpeedFactor != BiotunedCycleSpeedFactor
			|| DefaultSpeedFactor != SpeedFactor
			//|| DefaultCleanlinessEffectCurve.SequenceEqual(CleanlinessEffectCurve)
			|| DefaultMedicCycleDuration != MedicCycleDuration
			|| DefaultRegenerationCycleDuration != RegenerationCycleDuration
			|| DefaultRegenerationCycleIngredients.AnyDifference(RegenerationCycleIngredients)
			|| DefaultAgeReversalCycleDuration != AgeReversalCycleDuration
			//|| DefaultAgeReversalCycleAgeReversed != AgeReversalCycleAgeReversed
			|| DefaultPleasureCycleDuration != PleasureCycleDuration
			//|| DefaultPleasureCycleMoodEffect != PleasureCycleMoodEffect
			//|| DefaultPleasureCycleMoodDuration != PleasureCycleMoodDuration
			;

		protected override void Initialize()
		{
			#region GENERAL
			DefaultActivePowerConsumption = ActivePowerConsumption;
			DefaultStandbyPowerConsumption = StandbyPowerConsumption;
			DefaultBuildCost.SetFrom(BuildCost);
			DefaultWorkToBuild = WorkToBuild;
			#endregion

			#region SPECIFIC
			DefaultBiotunedCycleSpeedFactor = BiotunedCycleSpeedFactor;
			DefaultSpeedFactor = SpeedFactor;
			#endregion

			#region READY EFFECT
			DefaultReadyEffectColor = ReadyEffectColor;
			#endregion

			#region MEDIC CYCLE
			DefaultMedicCycleDuration = MedicCycleDuration;
			#endregion

			#region REGENERATION CYCLE
			DefaultRegenerationCycleDuration = RegenerationCycleDuration;
			DefaultRegenerationCycleIngredients.SetFrom(RegenerationCycleIngredients);
			#endregion

			#region AGE REVERSAL CYCLE
			DefaultAgeReversalCycleDuration = AgeReversalCycleDuration;
			#endregion

			#region PLEASURE CYCLE
			DefaultPleasureCycleDuration = PleasureCycleDuration;
			#endregion
		}

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && !IsModified())
				return;

			if (Scribe.EnterNode(BiosculpterPod.defName))
			{
				try
				{
					float floatValue;

					#region GENERAL
					floatValue = ActivePowerConsumption;
					Scribe_Values.Look(ref floatValue, nameof(ActivePowerConsumption), DefaultActivePowerConsumption);
					ActivePowerConsumption = floatValue;

					floatValue = StandbyPowerConsumption;
					Scribe_Values.Look(ref floatValue, nameof(StandbyPowerConsumption), DefaultStandbyPowerConsumption);
					StandbyPowerConsumption = floatValue;

					BPaNSUtility.ExposeList(BuildCost, nameof(BuildCost), () => BuildCost.AnyDifference(DefaultBuildCost));

					floatValue = WorkToBuild;
					Scribe_Values.Look(ref floatValue, nameof(WorkToBuild), DefaultWorkToBuild);
					WorkToBuild = floatValue;
					#endregion

					#region SPECIFIC
					floatValue = BiotunedCycleSpeedFactor;
					Scribe_Values.Look(ref floatValue, nameof(BiotunedCycleSpeedFactor), DefaultBiotunedCycleSpeedFactor);
					BiotunedCycleSpeedFactor = floatValue;

					floatValue = SpeedFactor;
					Scribe_Values.Look(ref floatValue, nameof(SpeedFactor), DefaultSpeedFactor);
					SpeedFactor = floatValue;
					#endregion

					#region READY EFFECT
					Color colorValue = ReadyEffectColor;
					Scribe_Values.Look(ref colorValue, nameof(ReadyEffectColor), DefaultReadyEffectColor);
					ReadyEffectColor = colorValue;
					#endregion

					#region MEDIC CYCLE
					floatValue = MedicCycleDuration;
					Scribe_Values.Look(ref floatValue, nameof(MedicCycleDuration), DefaultMedicCycleDuration);
					MedicCycleDuration = floatValue;
					#endregion

					#region REGENERATION CYCLE
					floatValue = RegenerationCycleDuration;
					Scribe_Values.Look(ref floatValue, nameof(RegenerationCycleDuration), DefaultRegenerationCycleDuration);
					RegenerationCycleDuration = floatValue;

					BPaNSUtility.ExposeList(RegenerationCycleIngredients, nameof(RegenerationCycleIngredients), () => RegenerationCycleIngredients.AnyDifference(DefaultRegenerationCycleIngredients));
					#endregion

					#region AGE REVERSAL CYCLE
					floatValue = AgeReversalCycleDuration;
					Scribe_Values.Look(ref floatValue, nameof(AgeReversalCycleDuration), DefaultAgeReversalCycleDuration);
					AgeReversalCycleDuration = floatValue;
					#endregion

					#region PLEASURE CYCLE
					floatValue = PleasureCycleDuration;
					Scribe_Values.Look(ref floatValue, nameof(PleasureCycleDuration), DefaultPleasureCycleDuration);
					PleasureCycleDuration = floatValue;
					#endregion
				}
				catch (Exception exc)
				{
					Log.Error(exc.ToString());
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
		}

		public override void CopyTo(BaseSettings to)
		{
			if (to != this && to is BiosculpterPodSettings copy)
			{
				copy.ActivePowerConsumption = ActivePowerConsumption;
				copy.StandbyPowerConsumption = StandbyPowerConsumption;
				copy.BuildCost.SetFrom(BuildCost);
				copy.WorkToBuild = WorkToBuild;

				copy.ReadyEffectColor = ReadyEffectColor;

				copy.BiotunedCycleSpeedFactor = BiotunedCycleSpeedFactor;
				copy.SpeedFactor = SpeedFactor;

				copy.MedicCycleDuration = MedicCycleDuration;

				copy.RegenerationCycleDuration = RegenerationCycleDuration;
				copy.RegenerationCycleIngredients.SetFrom(RegenerationCycleIngredients);

				copy.AgeReversalCycleDuration = AgeReversalCycleDuration;

				copy.PleasureCycleDuration = PleasureCycleDuration;
			}
		}
		#endregion

		#region PRIVATE METHODS
		private static BiosculpterPodEffectAnimation SetReadyEffectState(BiosculpterPodEffectAnimation state)
		{
			if (state is BiosculpterPodEffectAnimation.AlwaysOn)
			{
				ReadyEffecterFleckDef.fadeInTime = 0f;
				ReadyEffecterFleckDef.fadeOutTime = 0f;
				// new motes are generated AT and not AFTER "ticksBetweenMotes" (e.g. a value of 1 generates a mote on every tick)
				// 	this causes the time to be 1 tick shorter than expected (179 instead of 180 ticks)!
				ReadyEffecterFleckDef.solidTime = (ReadyEffecterDef.children[0].ticksBetweenMotes - 1) / 60f;
			}
			else
			{
				ReadyEffecterFleckDef.fadeInTime = OriginalReadyEffecterValues.FadeIn;
				ReadyEffecterFleckDef.fadeOutTime = OriginalReadyEffecterValues.FadeOut;
				ReadyEffecterFleckDef.solidTime = OriginalReadyEffecterValues.Solid;
			}

			var props = BPaNSUtility.GetBiosculpterPodDefs().GetCompPropertiesOfType<CompProperties_BiosculpterPod>();
			if (state is BiosculpterPodEffectAnimation.AlwaysOff)
			{
				foreach (var prop in props)
					prop.readyEffecter = null;
			}
			else
			{
				foreach (var prop in props)
					prop.readyEffecter = ReadyEffecterDef;
			}
			return state;
		}
		#endregion
	}
}
