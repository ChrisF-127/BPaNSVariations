using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

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
		public float DefaultBPNutritionRequired { get; private set; }
		public float BPNutritionRequired { get; set; } // applied via several CompBiosculpterPod-Transpilers

		public float DefaultBPMedicCycleDuration { get; private set; }
		private float _bpMedicCycleDuration;
		public float BPMedicCycleDuration
		{
			get => _bpMedicCycleDuration;
			set => _bpMedicCycleDuration = SetBPCycleDuration<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_MedicCycle>(value);
		}

		public float DefaultBPRegenerationCycleDuration { get; private set; }
		private float _bpRegenerationCycleDuration;
		public float BPRegenerationCycleDuration
		{
			get => _bpRegenerationCycleDuration;
			set => _bpRegenerationCycleDuration = SetBPCycleDuration<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>(value);
		}
#warning TODO medicine required, choose type & amount

		public float DefaultBPAgeReversalCycleDuration { get; private set; }
		private float _bpAgeReversalCycleDuration;
		public float BPAgeReversalCycleDuration
		{
			get => _bpAgeReversalCycleDuration;
			set => _bpAgeReversalCycleDuration = SetBPCycleDuration<CompProperties_BiosculpterPod_AgeReversalCycle, CompBiosculpterPod_AgeReversalCycle>(value);
		}
		public float DefaultBPAgeReversalCycleAgeReversed { get; private set; }
		public float BPAgeReversalCycleAgeReversed { get; set; } // applied via CompBiosculpterPod_AgeReversalCycle.CycleCompleted-Transpiler

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

		#region CONSTRUCTORS
		public BPaNSSettings()
		{
			_bpReadyEffectState = DefaultBPReadyEffectState = BiosculpterPodEffectAnimation.Default;
			_bpReadyEffectColor = DefaultBPReadyEffectColor = ThingDefOf.BiosculpterPod.GetCompProperties<CompProperties_BiosculpterPod>().selectCycleColor;
			BPNutritionRequired = DefaultBPNutritionRequired = CompBiosculpterPod.NutritionRequired;

			var medicCycle = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_MedicCycle>();
			_bpMedicCycleDuration = DefaultBPMedicCycleDuration = medicCycle.durationDays;

			var regenerationCycle = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_HealingCycle, CompBiosculpterPod_RegenerationCycle>();
			_bpRegenerationCycleDuration = DefaultBPRegenerationCycleDuration = regenerationCycle.durationDays;

			var ageReversedCycle = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_AgeReversalCycle, CompBiosculpterPod_AgeReversalCycle>();
			_bpAgeReversalCycleDuration = DefaultBPAgeReversalCycleDuration = ageReversedCycle.durationDays;
			BPAgeReversalCycleAgeReversed = DefaultBPAgeReversalCycleAgeReversed = 1f; // 1 year = 3'600'000 ticks

			var pleasureCycle = ThingDefOf.BiosculpterPod.GetSingleCompPropertiesOfTypeWithCompClass<CompProperties_BiosculpterPod_PleasureCycle, CompBiosculpterPod_PleasureCycle>();
			_bpPleasureCycleDuration = DefaultBPPleasureCycleDuration = pleasureCycle.durationDays;
			_bpPleasureCycleMoodEffect = DefaultBPPleasureCycleMoodEffect = ThoughtDefOf.BiosculpterPleasure.stages.First().baseMoodEffect;
			_bpPleasureCycleMoodDuration = DefaultBPPleasureCycleMoodDuration = ThoughtDefOf.BiosculpterPleasure.durationDays;
		}
		#endregion

		#region OVERRIDES
		public override void ExposeData()
		{
			base.ExposeData();

			Color colorValue;
			float floatValue;

			var bpreState = BPReadyEffectState;
			Scribe_Values.Look(ref bpreState, nameof(BPReadyEffectState), DefaultBPReadyEffectState);
			BPReadyEffectState = bpreState;
			colorValue = BPReadyEffectColor;
			Scribe_Values.Look(ref colorValue, nameof(BPReadyEffectColor), DefaultBPReadyEffectColor);
			BPReadyEffectColor = colorValue;
			floatValue = BPNutritionRequired;
			Scribe_Values.Look(ref floatValue, nameof(BPNutritionRequired), DefaultBPNutritionRequired);
			BPNutritionRequired = floatValue;

			floatValue = BPMedicCycleDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPMedicCycleDuration), DefaultBPMedicCycleDuration);
			BPMedicCycleDuration = floatValue;

			floatValue = BPRegenerationCycleDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPRegenerationCycleDuration), DefaultBPRegenerationCycleDuration);
			BPRegenerationCycleDuration = floatValue;

			floatValue = BPAgeReversalCycleDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPAgeReversalCycleDuration), DefaultBPAgeReversalCycleDuration);
			BPAgeReversalCycleDuration = floatValue;
			floatValue = BPAgeReversalCycleAgeReversed;
			Scribe_Values.Look(ref floatValue, nameof(BPAgeReversalCycleAgeReversed), DefaultBPAgeReversalCycleAgeReversed);
			BPAgeReversalCycleAgeReversed = floatValue;

			floatValue = BPPleasureCycleDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPPleasureCycleDuration), DefaultBPPleasureCycleDuration);
			BPPleasureCycleDuration = floatValue;
			floatValue = BPPleasureCycleMoodEffect;
			Scribe_Values.Look(ref floatValue, nameof(BPPleasureCycleMoodEffect), DefaultBPPleasureCycleMoodEffect);
			BPPleasureCycleMoodEffect = floatValue;
			floatValue = BPPleasureCycleMoodDuration;
			Scribe_Values.Look(ref floatValue, nameof(BPPleasureCycleMoodDuration), DefaultBPPleasureCycleMoodDuration);
			BPPleasureCycleMoodDuration = floatValue;
		}
		#endregion

		#region PRIVATE METHODS
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
