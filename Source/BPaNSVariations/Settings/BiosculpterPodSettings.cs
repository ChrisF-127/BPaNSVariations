using BPaNSVariations.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
		//General
		//x Build cost [requires reload]
		//x Work to build
		//x Active power consumption
		//x Standby power consumption

		//Ready Effect
		//x State [global]
		//x Color

		//Specific
		//x Nutrition required [global]
		//x Biotuned duration [global]
		//x Biotuned cycle speed factor
		//x Speed Factor
		//x Cleanliness Factor [global]

		//Medical
		//x Duration

		//Bioregen
		//x Duration
		//x Medicine required

		//Age Reversal
		//x Duration
		//x Age reversed [global]

		//Pleasure
		//x Duration
		//x Mood effect [global]
		//x Mood duration [global]

		//Research
		//- Biosculpting
		//- Bioregeneration

		#region PROPERTIES
		public static EffecterDef ReadyEffecterDef { get; private set; }
		public static FleckDef ReadyEffecterFleckDef { get; private set; }
		public static (float FadeIn, float FadeOut, float Solid) OriginalReadyEffecterValues { get; private set; } // FadeIn, FadeOut, Solid


		#region GENERAL
		public float DefaultStandbyPowerConsumption { get; }
		public float StandbyPowerConsumption
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_Power>().idlePowerDraw;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_Power>().idlePowerDraw = value;
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
		public Color DefaultReadyEffectColor { get; }
		public Color ReadyEffectColor
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_BiosculpterPod>().selectCycleColor;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_BiosculpterPod>().selectCycleColor = value;
		}
		#endregion

		#region SPECIFIC
		public static float DefaultNutritionRequired { get; private set; }
		public static float NutritionRequired { get; set; } // applied via several CompBiosculpterPod-Transpilers
		public static int DefaultBiotunedDuration { get; private set; }
		public static int BiotunedDuration { get; set; } // applied via CompBiosculpterPod.SetBiotuned-Transpiler
		public float DefaultBiotunedCycleSpeedFactor { get; }
		public float BiotunedCycleSpeedFactor
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_BiosculpterPod>().biotunedCycleSpeedFactor;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_BiosculpterPod>().biotunedCycleSpeedFactor = value;
		}
		public float DefaultSpeedFactor { get; }
		public float SpeedFactor
		{
			get => Def.GetStatValueAbstract(StatDefOf.BiosculpterPodSpeedFactor);
			set => Def.SetStatBaseValue(StatDefOf.BiosculpterPodSpeedFactor, value);
		}
		public static SimpleCurve DefaultCleanlinessEffectCurve { get; private set; }
		public static RoomStatDef BiosculpterPodSpeedFactorStatDef { get; } = DefDatabase<RoomStatDef>.GetNamed("BiosculpterPodSpeedFactor");
		public static SimpleCurve CleanlinessEffectCurve => BiosculpterPodSpeedFactorStatDef.curve;
		#endregion

		#region MEDIC CYCLE
		public float DefaultMedicCycleDuration { get; }
		public float MedicCycleDuration
		{
			get => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_MedicCycle>().durationDays;
			set => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_MedicCycle>().durationDays = value;
		}
		#endregion

		#region REGENERATION CYCLE
		public float DefaultRegenerationCycleDuration { get; }
		public float RegenerationCycleDuration
		{
			get => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>().durationDays;
			set => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>().durationDays = value;
		}
		public List<ThingDefCountClass> DefaultRegenerationCycleIngredients { get; } = new List<ThingDefCountClass>();
		public List<ThingDefCountClass> RegenerationCycleIngredients => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>().extraRequiredIngredients;
		#endregion

		#region AGE REVERSAL CYCLE
		public float DefaultAgeReversalCycleDuration { get; }
		public float AgeReversalCycleDuration
		{
			get => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_AgeReversalCycle, CompBiosculpterPod_AgeReversalCycle>().durationDays;
			set => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_AgeReversalCycle, CompBiosculpterPod_AgeReversalCycle>().durationDays = value;
		}
		public static float DefaultAgeReversalCycleAgeReversed { get; private set; }
		public static float AgeReversalCycleAgeReversed { get; set; } // applied via CompBiosculpterPod_AgeReversalCycle.CycleCompleted-Transpiler
		#endregion

		#region PLEASURE CYCLE
		public float DefaultPleasureCycleDuration { get; }
		public float PleasureCycleDuration
		{
			get => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_PleasureCycle, CompBiosculpterPod_PleasureCycle>().durationDays;
			set => Def.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_PleasureCycle, CompBiosculpterPod_PleasureCycle>().durationDays = value;
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
		public BiosculpterPodSettings(ThingDef biosculpterPod) : base(biosculpterPod)
		{
			#region GENERAL
			DefaultStandbyPowerConsumption = StandbyPowerConsumption;
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
			if (Scribe.EnterNode(nameof(BiosculpterPodSettings)))
			{
				try
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

		public static bool IsGlobalModified() =>
			DefaultReadyEffectState != ReadyEffectState
			|| DefaultNutritionRequired != NutritionRequired
			|| DefaultBiotunedDuration != BiotunedDuration
			|| !DefaultCleanlinessEffectCurve.SequenceEqual(CleanlinessEffectCurve)
			|| DefaultAgeReversalCycleAgeReversed != AgeReversalCycleAgeReversed
			|| DefaultPleasureCycleMoodEffect != PleasureCycleMoodEffect
			|| DefaultPleasureCycleMoodDuration != PleasureCycleMoodDuration;


		public static void ApplyCleanlinessCurve()
		{
			BiosculpterPodSpeedFactorStatDef.roomlessScore = CleanlinessEffectCurve.MinBy(v => v.x).y;
		}
		#endregion

		#region OVERRIDES
		public override bool IsModified() =>
			base.IsModified()
			|| DefaultStandbyPowerConsumption != StandbyPowerConsumption
			|| DefaultReadyEffectColor != ReadyEffectColor
			|| DefaultBiotunedCycleSpeedFactor != BiotunedCycleSpeedFactor
			|| DefaultSpeedFactor != SpeedFactor
			|| DefaultMedicCycleDuration != MedicCycleDuration
			|| DefaultRegenerationCycleDuration != RegenerationCycleDuration
			|| DefaultRegenerationCycleIngredients.IsModified(RegenerationCycleIngredients)
			|| DefaultAgeReversalCycleDuration != AgeReversalCycleDuration
			|| DefaultPleasureCycleDuration != PleasureCycleDuration;

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && !IsModified())
				return;

			if (Scribe.EnterNode(Def.defName))
			{
				try
				{
					base.ExposeData();

					#region GENERAL
					float floatValue = StandbyPowerConsumption;
					Scribe_Values.Look(ref floatValue, nameof(StandbyPowerConsumption), DefaultStandbyPowerConsumption);
					StandbyPowerConsumption = floatValue;
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

					BPaNSUtility.ExposeList(RegenerationCycleIngredients, nameof(RegenerationCycleIngredients), () => RegenerationCycleIngredients.IsModified(DefaultRegenerationCycleIngredients));
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
				base.CopyTo(to);

				copy.StandbyPowerConsumption = StandbyPowerConsumption;

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
