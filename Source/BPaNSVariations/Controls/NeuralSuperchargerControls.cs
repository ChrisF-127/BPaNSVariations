using BPaNSVariations.Settings;
using Verse;

namespace BPaNSVariations.Controls
{
	internal class NeuralSuperchargerControls : BaseControls
	{
		#region PROPERTIES
		public NeuralSuperchargerSettings NeuralSuperchargerSettings => 
			(NeuralSuperchargerSettings)Settings;

		protected override bool CanBeCopied =>
			true;
		#endregion

		#region CONSTRUCTORS
		public NeuralSuperchargerControls(NeuralSuperchargerSettings settings) : base(settings)
		{
		}
		#endregion

		#region OVERRIDES
		public override void CreateSettings(ref float offsetY, float viewWidth, out bool copy)
		{
			// General
			base.CreateSettings(ref offsetY, viewWidth, out copy);

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
			// Hediff - [GLOBAL] Hediff Consciousness
			NeuralSuperchargerSettings.HediffConsciousness = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.HediffConsciousness".Translate(),
				"SY_BNV.TooltipHediffConsciousness".Translate(),
				NeuralSuperchargerSettings.HediffConsciousness,
				NeuralSuperchargerSettings.DefaultHediffConsciousness,
				"HediffConsciousness",
				min: -10f,
				max: 10f,
				additionalText: ValueToPercent);
			// Hediff - [GLOBAL] Hediff Global Learning Factor
			NeuralSuperchargerSettings.HediffLearningFactor = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.HediffLearningFactor".Translate(),
				"SY_BNV.TooltipHediffLearningFactor".Translate(),
				NeuralSuperchargerSettings.HediffLearningFactor,
				NeuralSuperchargerSettings.DefaultHediffLearningFactor,
				"HediffLearningFactor",
				min: -10f,
				max: 10f,
				additionalText: ValueToPercent);
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
