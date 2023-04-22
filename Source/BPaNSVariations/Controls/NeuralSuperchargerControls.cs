using BPaNSVariations.Settings;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using BPaNSVariations.Utility;

namespace BPaNSVariations.Controls
{
	internal class NeuralSuperchargerControls : BaseControls
	{
		#region PROPERTIES
		public NeuralSuperchargerSettings NeuralSuperchargerSettings => 
			(NeuralSuperchargerSettings)Settings;

		public override bool CanBeCopied =>
			true;
		#endregion

		#region CONSTRUCTORS
		public NeuralSuperchargerControls(NeuralSuperchargerSettings settings) : base(settings)
		{
		}
		#endregion

		#region FIELDS
		private PawnCapacityModifier _newPawnCapacityModifier;
		private StatModifier _newStatModifierFactor;
		private StatModifier _newStatModifierOffset;
		#endregion

		#region OVERRIDES
		public override void CreateSettings(ref float offsetY, float viewWidth)
		{
			// General
			base.CreateSettings(ref offsetY, viewWidth);

			#region SPECIFIC
			// Specific
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorSpecific".Translate());
			// Specific - [GLOBAL] Anyone Can Build
			NeuralSuperchargerSettings.AnyoneCanBuild = CreateCheckbox(
				ref offsetY,
				viewWidth,
				"SY_BNV.AnyoneCanBuild".Translate(),
				"SY_BNV.TooltipAnyoneCanBuild".Translate(),
				NeuralSuperchargerSettings.AnyoneCanBuild,
				NeuralSuperchargerSettings.DefaultAnyoneCanBuild,
				"SY_BNV.DescAnyoneCanBuild".Translate());
			// Specific - Ticks To Recharge
			NeuralSuperchargerSettings.TicksToRecharge = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.TicksToRecharge".Translate(),
				"SY_BNV.TooltipTicksToRecharge".Translate(),
				NeuralSuperchargerSettings.TicksToRecharge,
				NeuralSuperchargerSettings.DefaultTicksToRecharge,
				"TicksToRecharge",
				additionalText: TicksToYearText);
			#endregion

			#region HEDIFF
			// Hediff
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorHediffNeuralSupercharge".Translate());
			// Hediff - [GLOBAL] Hediff Disappears After Ticks
			NeuralSuperchargerSettings.HediffDisappearsAfterTicks = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.HediffDisappearsAfterTicks".Translate(),
				"SY_BNV.TooltipHediffDisappearsAfterTicks".Translate(),
				NeuralSuperchargerSettings.HediffDisappearsAfterTicks,
				NeuralSuperchargerSettings.DefaultHediffDisappearsAfterTicks,
				"HediffDisappearsAfterTicks",
				additionalText: TicksToYearText);
			// Hediff - [GLOBAL] Hediff Pawn Capacity Modifiers
			if (_newPawnCapacityModifier == null)
				_newPawnCapacityModifier = new PawnCapacityModifier();
			CreateCountListControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.HediffPawnCapacityModifiers".Translate(),
				ref _newPawnCapacityModifier,
				NeuralSuperchargerSettings.HediffPawnCapacityModifiers,
				NeuralSuperchargerSettings.DefaultHediffPawnCapacityModifiers,
				BPaNSVariations.Settings.PawnCapacityDefs,
				"HediffPawnCapacityModifiers",
				(listA, listB) =>
				{
					if (listA.Count != listB.Count)
						return true;
					foreach (var a in listA)
						if (!listB.Any(b => a.capacity == b.capacity && a.offset == b.offset))
							return true;
					foreach (var b in listB)
						if (!listA.Any(a => a.capacity == b.capacity && a.offset == b.offset))
							return true;
					return false;
				},
				(to, from) =>
				{
					to.Clear();
					to.AddRange(from.Select(x => x.Clone()));
				},
				obj => obj.capacity,
				(obj, t) => obj.capacity = t,
				obj => obj.offset,
				(obj, v) => obj.offset = v,
				t => t?.LabelCap,
				v => $"{(v > 0f ? '+' : ' ')}{v:P0}");
			// Hediff - [GLOBAL] Hediff Stat Factors
			if (_newStatModifierFactor == null)
				_newStatModifierFactor = new StatModifier();
			CreateCountListControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.HediffStatFactors".Translate(),
				ref _newStatModifierFactor,
				NeuralSuperchargerSettings.HediffStatFactors,
				NeuralSuperchargerSettings.DefaultHediffStatFactors,
				BPaNSVariations.Settings.StatDefs,
				"HediffStatFactors",
				(listA, listB) =>
				{
					if (listA.Count != listB.Count)
						return true;
					foreach (var a in listA)
						if (!listB.Any(b => a.stat == b.stat && a.value == b.value))
							return true;
					foreach (var b in listB)
						if (!listA.Any(a => a.stat == b.stat && a.value == b.value))
							return true;
					return false;
				},
				(to, from) =>
				{
					to.Clear();
					to.AddRange(from.Select(x => new StatModifier
					{
						stat = x.stat,
						value = x.value,
					}));
				},
				obj => obj.stat,
				(obj, t) => obj.stat = t,
				obj => obj.value,
				(obj, v) => obj.value = v,
				t => t?.LabelCap,
				v => $"{(v != 0f ? 'x' : ' ')}{v:P0}");
			// Hediff - [GLOBAL] Hediff Stat Offset
			if (_newStatModifierOffset == null)
				_newStatModifierOffset = new StatModifier();
			CreateCountListControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.HediffStatOffset".Translate(),
				ref _newStatModifierOffset,
				NeuralSuperchargerSettings.HediffStatOffset,
				NeuralSuperchargerSettings.DefaultHediffStatOffset,
				BPaNSVariations.Settings.StatDefs,
				"HediffStatOffset",
				(listA, listB) =>
				{
					if (listA.Count != listB.Count)
						return true;
					foreach (var a in listA)
						if (!listB.Any(b => a.stat == b.stat && a.value == b.value))
							return true;
					foreach (var b in listB)
						if (!listA.Any(a => a.stat == b.stat && a.value == b.value))
							return true;
					return false;
				},
				(to, from) =>
				{
					to.Clear();
					to.AddRange(from.Select(x => new StatModifier
					{
						stat = x.stat,
						value = x.value,
					}));
				},
				obj => obj.stat,
				(obj, t) => obj.stat = t,
				obj => obj.value,
				(obj, v) => obj.value = v,
				t => t?.LabelCap,
				v => $"{(v > 0f ? '+' : ' ')}{v:P0}");
			// Hediff - [GLOBAL] Hediff Hunger Rate Factor
			NeuralSuperchargerSettings.HediffHungerRateFactor = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.HediffHungerRateFactor".Translate(),
				"SY_BNV.TooltipHediffHungerRateFactor".Translate(),
				NeuralSuperchargerSettings.HediffHungerRateFactor,
				NeuralSuperchargerSettings.DefaultHediffHungerRateFactor,
				"HediffHungerRateFactor",
				min: -10f,
				max: 10f,
				additionalText: ValueToPercent);
			#endregion

			// Margin
			offsetY += SettingsRowHeight / 2;
		}
		#endregion
	}
}
