using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace BPaNSVariations
{
	public enum BiosculpterPodEffectAnimation
	{
		Default,
		AlwaysOn,
		AlwaysOff,
	}

	internal class BPaNSSettings : ModSettings
	{
		#region PROPERTIES
		#region BIOSCULPTER PODS
		#region GENERAL
		public float DefaultBPActivePowerConsumption { get; private set; }
		private float _bpActivePowerConsumption;
		public float BPActivePowerConsumption
		{
			get => _bpActivePowerConsumption;
			set => _bpActivePowerConsumption = SetBPActivePowerConsumption(value);
		}
		public float DefaultBPStandbyPowerConsumption { get; private set; }
		private float _bpStandbyPowerConsumption;
		public float BPStandbyPowerConsumption
		{
			get => _bpStandbyPowerConsumption;
			set => _bpStandbyPowerConsumption = SetBPStandbyPowerConsumption(value);
		}
		public List<ThingDefCountClass> DefaultBPBuildCost { get; } = new List<ThingDefCountClass>();
		public List<ThingDefCountClass> BPBuildCost { get; } = new List<ThingDefCountClass>(); // applied via ApplyBPBuildCost
		public float DefaultBPWorkToBuild { get; private set; }
		private float _bpWorkToBuild;
		public float BPWorkToBuild
		{
			get => _bpWorkToBuild;
			set => _bpWorkToBuild = SetBPWorkToBuild(value);
		}
		#endregion

		#region READY EFFECT
		public BiosculpterPodEffectAnimation DefaultBPReadyEffectState { get; private set; }
		private BiosculpterPodEffectAnimation _bpReadyEffectState;
		public BiosculpterPodEffectAnimation BPReadyEffectState
		{
			get => _bpReadyEffectState;
			set => _bpReadyEffectState = SetBPReadyEffectState(value);
		}
		public Color DefaultBPReadyEffectColor { get; private set; }
		private Color _bpReadyEffectColor;
		public Color BPReadyEffectColor
		{
			get => _bpReadyEffectColor;
			set => _bpReadyEffectColor = SetBPReadyEffectColor(value);
		}
		#endregion

		#region SPECIFIC
		public float DefaultBPNutritionRequired { get; private set; }
		public float BPNutritionRequired { get; set; } // applied via several CompBiosculpterPod-Transpilers
		public int DefaultBPBiotunedDuration { get; private set; }
		public int BPBiotunedDuration { get; set; } // applied via CompBiosculpterPod.SetBiotuned-Transpiler
		public float DefaultBPBiotunedCycleSpeedFactor { get; private set; }
		private float _bpBiotunedCycleSpeedFactor;
		public float BPBiotunedCycleSpeedFactor
		{
			get => _bpBiotunedCycleSpeedFactor;
			set => _bpBiotunedCycleSpeedFactor = SetBPBiotunedCycleSpeedFactor(value);
		}
		public float DefaultBPSpeedFactor { get; private set; }
		private float _bpSpeedFactor;
		public float BPSpeedFactor
		{
			get => _bpSpeedFactor;
			set => _bpSpeedFactor = SetBPSpeedFactor(value);
		}
		#endregion

		#region MEDIC CYCLE
		public float DefaultBPMedicCycleDuration { get; private set; }
		private float _bpMedicCycleDuration;
		public float BPMedicCycleDuration
		{
			get => _bpMedicCycleDuration;
			set => _bpMedicCycleDuration = SetBPCycleDuration<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_MedicCycle>(value);
		}
		#endregion

		#region REGENERATION CYCLE
		public float DefaultBPRegenerationCycleDuration { get; private set; }
		private float _bpRegenerationCycleDuration;
		public float BPRegenerationCycleDuration
		{
			get => _bpRegenerationCycleDuration;
			set => _bpRegenerationCycleDuration = SetBPCycleDuration<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>(value);
		}
		public List<ThingDefCountClass> DefaultBPRegenerationCycleIngredients { get; } = new List<ThingDefCountClass>();
		public List<ThingDefCountClass> BPRegenerationCycleIngredients { get; } = new List<ThingDefCountClass>(); // applied via ApplyBPRegenerationCycleExtraRequiredIngredients
		#endregion

		#region AGE REVERSAL CYCLE
		public float DefaultBPAgeReversalCycleDuration { get; private set; }
		private float _bpAgeReversalCycleDuration;
		public float BPAgeReversalCycleDuration
		{
			get => _bpAgeReversalCycleDuration;
			set => _bpAgeReversalCycleDuration = SetBPCycleDuration<CompProperties_BiosculpterPod_AgeReversalCycle, CompBiosculpterPod_AgeReversalCycle>(value);
		}
		public float DefaultBPAgeReversalCycleAgeReversed { get; private set; }
		public float BPAgeReversalCycleAgeReversed { get; set; } // applied via CompBiosculpterPod_AgeReversalCycle.CycleCompleted-Transpiler
		#endregion

		#region PLEASURE CYCLE
		public float DefaultBPPleasureCycleDuration { get; private set; }
		private float _bpPleasureCycleDuration;
		public float BPPleasureCycleDuration
		{
			get => _bpPleasureCycleDuration;
			set => _bpPleasureCycleDuration = SetBPCycleDuration<CompProperties_BiosculpterPod_PleasureCycle, CompBiosculpterPod_PleasureCycle>(value);
		}
		public float DefaultBPPleasureCycleMoodEffect { get; private set; }
		private float _bpPleasureCycleMoodEffect;
		public float BPPleasureCycleMoodEffect
		{
			get => _bpPleasureCycleMoodEffect;
			set => _bpPleasureCycleMoodEffect = SetPBPleasureCycleMoodEffect(value);
		}
		public float DefaultBPPleasureCycleMoodDuration { get; private set; }
		private float _bpPleasureCycleMoodDuration;
		public float BPPleasureCycleMoodDuration
		{
			get => _bpPleasureCycleMoodDuration;
			set => _bpPleasureCycleMoodDuration = SetPBPleasureCycleMoodDuration(value);
		}
		#endregion
		#endregion
		#endregion

		#region FIELDS
		public readonly ReadOnlyCollection<ThingDef> BuildCostThingDefs;
		public readonly ReadOnlyCollection<ThingDef> MedicineThingDefs;
		#endregion

		#region CONSTRUCTORS
		public BPaNSSettings()
		{
			var thingDefs = DefDatabase<ThingDef>.AllDefs.Where(
				(def) =>
				def.category == ThingCategory.Item
				&&  def.CountAsResource
				&& !def.MadeFromStuff
				&& (def.thingCategories?.Contains(ThingCategoryDefOf.ResourcesRaw) == true
				||  def.thingCategories?.Contains(ThingCategoryDefOf.Manufactured) == true)
				).ToList();
			thingDefs.SortBy((def) => def.label ?? "");
			BuildCostThingDefs = thingDefs.AsReadOnly();

			thingDefs = DefDatabase<ThingDef>.AllDefs.Where(
				(def) => def.IsMedicine).ToList();
			thingDefs.SortBy((def) => def.label ?? "");
			MedicineThingDefs = thingDefs.AsReadOnly();

			// Biosculpter Pods
			InitializeBiosculpterPodSettings();
		}
		#endregion

		#region PUBLIC METHODS
		public void ApplyBPBuildCost()
		{
			var defs = BPaNSUtility.GetBiosculpterPodDefs();
			foreach (var def in defs)
				if (def.costList.IsModified(BPBuildCost))
					def.costList.Overwrite(BPBuildCost);
		}
		public void ApplyBPRegenerationCycleIngredients()
		{
			var props = BPaNSUtility.GetBiosculpterPodDefs().GetCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>();
			foreach (var prop in props)
				if (prop.extraRequiredIngredients.IsModified(BPRegenerationCycleIngredients))
					prop.extraRequiredIngredients.Overwrite(BPRegenerationCycleIngredients);
		}
		#endregion

		#region OVERRIDES
		public override void ExposeData()
		{
			base.ExposeData();

			// Biosculpter Pods
			ExposeDataBiosculpterPods();
		}
		#endregion

		#region PRIVATE METHODS
		private void InitializeBiosculpterPodSettings()
		{
			var biosculpterPod = ThingDefOf.BiosculpterPod.GetCompProperties<CompProperties_BiosculpterPod>();
			var bpPower = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfType<CompProperties_Power>();
			var bpMedicCycle = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_MedicCycle>();
			var bpRegenerationCycle = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>();
			var bpAgeReversedCycle = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_AgeReversalCycle, CompBiosculpterPod_AgeReversalCycle>();
			var bpPleasureCycle = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_PleasureCycle, CompBiosculpterPod_PleasureCycle>();
			var bpBiosculpterPleasure = ThoughtDefOf.BiosculpterPleasure;

			#region GENERAL
			_bpActivePowerConsumption = DefaultBPActivePowerConsumption = bpPower.basePowerConsumption;
			_bpStandbyPowerConsumption = DefaultBPStandbyPowerConsumption = bpPower.idlePowerDraw;
			BPBuildCost.Overwrite(ThingDefOf.BiosculpterPod.costList);
			DefaultBPBuildCost.Overwrite(BPBuildCost);
			ApplyBPBuildCost();
			_bpWorkToBuild = DefaultBPWorkToBuild = ThingDefOf.BiosculpterPod.GetStatValueAbstract(StatDefOf.WorkToBuild);
			#endregion

			#region READY EFFECT
			_bpReadyEffectState = DefaultBPReadyEffectState = BiosculpterPodEffectAnimation.Default;
			_bpReadyEffectColor = DefaultBPReadyEffectColor = biosculpterPod.selectCycleColor;
			#endregion

			#region SPECIFIC
			BPNutritionRequired = DefaultBPNutritionRequired = CompBiosculpterPod.NutritionRequired;
			BPBiotunedDuration = DefaultBPBiotunedDuration = 4800000; // hardcoded
			_bpBiotunedCycleSpeedFactor = DefaultBPBiotunedCycleSpeedFactor = biosculpterPod.biotunedCycleSpeedFactor;
			_bpSpeedFactor = DefaultBPSpeedFactor = ThingDefOf.BiosculpterPod.GetStatValueAbstract(StatDefOf.BiosculpterPodSpeedFactor);
			#endregion

			#region MEDIC CYCLE
			_bpMedicCycleDuration = DefaultBPMedicCycleDuration = bpMedicCycle.durationDays;
			#endregion

			#region REGENERATION CYCLE
			_bpRegenerationCycleDuration = DefaultBPRegenerationCycleDuration = bpRegenerationCycle.durationDays;
			BPRegenerationCycleIngredients.Overwrite(bpRegenerationCycle.extraRequiredIngredients);
			DefaultBPRegenerationCycleIngredients.Overwrite(BPRegenerationCycleIngredients);
			ApplyBPRegenerationCycleIngredients();
			#endregion

			#region AGE REVERSAL CYCLE
			_bpAgeReversalCycleDuration = DefaultBPAgeReversalCycleDuration = bpAgeReversedCycle.durationDays;
			BPAgeReversalCycleAgeReversed = DefaultBPAgeReversalCycleAgeReversed = 1f; // hardcoded, 1 year = 3'600'000 ticks
			#endregion

			#region PLEASURE CYCLE
			_bpPleasureCycleDuration = DefaultBPPleasureCycleDuration = bpPleasureCycle.durationDays;
			_bpPleasureCycleMoodEffect = DefaultBPPleasureCycleMoodEffect = bpBiosculpterPleasure.stages.First().baseMoodEffect;
			_bpPleasureCycleMoodDuration = DefaultBPPleasureCycleMoodDuration = bpBiosculpterPleasure.durationDays;
			#endregion
		}

		private void ExposeDataBiosculpterPods()
		{
			Color colorValue;
			float floatValue;
			int intValue;

			#region GENERAL
			floatValue = BPActivePowerConsumption;
			Scribe_Values.Look(ref floatValue, nameof(BPActivePowerConsumption), DefaultBPActivePowerConsumption);
			BPActivePowerConsumption = floatValue;

			floatValue = BPStandbyPowerConsumption;
			Scribe_Values.Look(ref floatValue, nameof(BPStandbyPowerConsumption), DefaultBPStandbyPowerConsumption);
			BPStandbyPowerConsumption = floatValue;

			BPaNSUtility.ExposeList(BPBuildCost, nameof(BPBuildCost), () => BPBuildCost.IsModified(DefaultBPBuildCost));
			ApplyBPBuildCost();
			floatValue = BPWorkToBuild;

			Scribe_Values.Look(ref floatValue, nameof(BPWorkToBuild), DefaultBPWorkToBuild);
			BPWorkToBuild = floatValue;
			#endregion

			#region READY EFFECT
			var bpreState = BPReadyEffectState;
			Scribe_Values.Look(ref bpreState, nameof(BPReadyEffectState), DefaultBPReadyEffectState);
			BPReadyEffectState = bpreState;

			colorValue = BPReadyEffectColor;
			Scribe_Values.Look(ref colorValue, nameof(BPReadyEffectColor), DefaultBPReadyEffectColor);
			BPReadyEffectColor = colorValue;
			#endregion

			#region SPECIFIC
			floatValue = BPNutritionRequired;
			Scribe_Values.Look(ref floatValue, nameof(BPNutritionRequired), DefaultBPNutritionRequired);
			BPNutritionRequired = floatValue;

			intValue = BPBiotunedDuration;
			Scribe_Values.Look(ref intValue, nameof(BPBiotunedDuration), DefaultBPBiotunedDuration);
			BPBiotunedDuration = intValue;

			floatValue = BPBiotunedCycleSpeedFactor;
			Scribe_Values.Look(ref floatValue, nameof(BPBiotunedCycleSpeedFactor), DefaultBPBiotunedCycleSpeedFactor);
			BPBiotunedCycleSpeedFactor = floatValue;

			floatValue = BPSpeedFactor;
			Scribe_Values.Look(ref floatValue, nameof(BPSpeedFactor), DefaultBPSpeedFactor);
			BPSpeedFactor = floatValue;
			#endregion

			#region MEDIC CYCLE
			floatValue = BPMedicCycleDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPMedicCycleDuration), DefaultBPMedicCycleDuration);
			BPMedicCycleDuration = floatValue;
			#endregion

			#region REGENERATION CYCLE
			floatValue = BPRegenerationCycleDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPRegenerationCycleDuration), DefaultBPRegenerationCycleDuration);
			BPRegenerationCycleDuration = floatValue;

			BPaNSUtility.ExposeList(BPRegenerationCycleIngredients, nameof(BPRegenerationCycleIngredients), () => BPRegenerationCycleIngredients.IsModified(DefaultBPRegenerationCycleIngredients));
			ApplyBPRegenerationCycleIngredients();
			#endregion

			#region AGE REVERSAL CYCLE
			floatValue = BPAgeReversalCycleDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPAgeReversalCycleDuration), DefaultBPAgeReversalCycleDuration);
			BPAgeReversalCycleDuration = floatValue;

			floatValue = BPAgeReversalCycleAgeReversed;
			Scribe_Values.Look(ref floatValue, nameof(BPAgeReversalCycleAgeReversed), DefaultBPAgeReversalCycleAgeReversed);
			BPAgeReversalCycleAgeReversed = floatValue;
			#endregion

			#region PLEASURE CYCLE
			floatValue = BPPleasureCycleDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPPleasureCycleDuration), DefaultBPPleasureCycleDuration);
			BPPleasureCycleDuration = floatValue;

			floatValue = BPPleasureCycleMoodEffect;
			Scribe_Values.Look(ref floatValue, nameof(BPPleasureCycleMoodEffect), DefaultBPPleasureCycleMoodEffect);
			BPPleasureCycleMoodEffect = floatValue;

			floatValue = BPPleasureCycleMoodDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPPleasureCycleMoodDuration), DefaultBPPleasureCycleMoodDuration);
			BPPleasureCycleMoodDuration = floatValue;
			#endregion
		}

		private float SetBPBiotunedCycleSpeedFactor(float value)
		{
			var props = BPaNSUtility.GetBiosculpterPodDefs().GetCompPropertiesOfType<CompProperties_BiosculpterPod>();
			foreach (var prop in props)
				prop.biotunedCycleSpeedFactor = value;
			return value;
		}
		private float SetBPSpeedFactor(float value)
		{
			var defs = BPaNSUtility.GetBiosculpterPodDefs();
			foreach (var def in defs)
				def.SetStatBaseValue(StatDefOf.BiosculpterPodSpeedFactor, value);
			return value;
		}
		private float SetBPActivePowerConsumption(float value)
		{
			var props = BPaNSUtility.GetBiosculpterPodDefs().GetCompPropertiesOfType<CompProperties_Power>();
			foreach (var prop in props)
				prop.basePowerConsumption = value;
			return value;
		}
		private float SetBPStandbyPowerConsumption(float value)
		{
			var props = BPaNSUtility.GetBiosculpterPodDefs().GetCompPropertiesOfType<CompProperties_Power>();
			foreach (var prop in props)
				prop.idlePowerDraw = value;
			return value;
		}

		private float SetBPWorkToBuild(float value)
		{
			var defs = BPaNSUtility.GetBiosculpterPodDefs();
			foreach (var def in defs)
				def.SetStatBaseValue(StatDefOf.WorkToBuild, value);
			return value;
		}

		private BiosculpterPodEffectAnimation SetBPReadyEffectState(BiosculpterPodEffectAnimation state)
		{
			if (state is BiosculpterPodEffectAnimation.AlwaysOn)
			{
				BPaNSUtility.BiosculpterScanner_Ready.fadeInTime = 0f;
				BPaNSUtility.BiosculpterScanner_Ready.fadeOutTime = 0f;
				// new motes are generated AT and not AFTER "ticksBetweenMotes" (e.g. a value of 1 generates a mote on every tick)
				// 	this causes the time to be 1 tick shorter than expected (179 instead of 180 ticks)!
				BPaNSUtility.BiosculpterScanner_Ready.solidTime = (BPaNSUtility.BiosculpterPod_Ready.children[0].ticksBetweenMotes - 1) / 60f;
			}
			else
			{
				BPaNSUtility.BiosculpterScanner_Ready.fadeInTime = BPaNSUtility.OriginalBiosculpterScanner_ReadyValues.FadeIn;
				BPaNSUtility.BiosculpterScanner_Ready.fadeOutTime = BPaNSUtility.OriginalBiosculpterScanner_ReadyValues.FadeOut;
				BPaNSUtility.BiosculpterScanner_Ready.solidTime = BPaNSUtility.OriginalBiosculpterScanner_ReadyValues.Solid;
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
					prop.readyEffecter = BPaNSUtility.BiosculpterPod_Ready;
			}
			return state;
		}
		private Color SetBPReadyEffectColor(Color color)
		{
			foreach (var prop in BPaNSUtility.GetBiosculpterPodDefs().GetCompPropertiesOfType<CompProperties_BiosculpterPod>())
				prop.selectCycleColor = color;
			return color;
		}

		private float SetPBPleasureCycleMoodEffect(float effect) => 
			ThoughtDefOf.BiosculpterPleasure.stages.First().baseMoodEffect = effect;
		private float SetPBPleasureCycleMoodDuration(float duration) =>
			ThoughtDefOf.BiosculpterPleasure.durationDays = duration;

		private float SetBPCycleDuration<Prop, Class>(float duration)
			where Prop : CompProperties_BiosculpterPod_BaseCycle
			where Class : CompBiosculpterPod_Cycle
		{
			var props = BPaNSUtility.GetBiosculpterPodDefs().GetCompPropertiesOfTypeWithCompClass<Prop, Class>();
			if (props.Count() == 0)
				throw new Exception($"{nameof(SetBPCycleDuration)} found no CompProperties for:\n{typeof(Prop)}\n{typeof(Class)}");
			foreach (var prop in props)
				prop.durationDays = duration;
			return duration;
		}
		#endregion
	}
}
