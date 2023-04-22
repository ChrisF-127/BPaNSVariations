using BPaNSVariations.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BPaNSVariations.Settings
{
	internal class NeuralSuperchargerSettings : BaseSettings
	{
		//General
		//x Build cost [requires reload]
		//x Work to build
		//x Power consumption

		//Specific
		//x Charge time [new apply only]
		//x Anyone can build toggle [global, requires restart]

		//Hediff
		//x Duration [global, new apply only]
		//x Consciousness [global]
		//x Global Learning Factor [global]
		//x Hunger Rate Factor [global]

		//Research
		//- Neural Supercharger

		#region PROPERTIES
		public static bool DefaultAnyoneCanBuild { get; private set; }
		public static bool _anyoneCanBuild = false;
		public static bool AnyoneCanBuild
		{
			get => _anyoneCanBuild;
			set => SetAnyoneCanBuild(value);
		}
		public int DefaultTicksToRecharge { get; }
		public int TicksToRecharge
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_NeuralSupercharger>().ticksToRecharge;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_NeuralSupercharger>().ticksToRecharge = value;
		}

		public static int DefaultHediffDisappearsAfterTicks { get; private set; }
		public static int HediffDisappearsAfterTicks
		{
			get => HediffDefOf.NeuralSupercharge.GetSingleCompPropertiesOfType<HediffCompProperties_Disappears>().disappearsAfterTicks.max;
			set => HediffDefOf.NeuralSupercharge.GetSingleCompPropertiesOfType<HediffCompProperties_Disappears>().disappearsAfterTicks.max = value;
		}
		public static List<PawnCapacityModifier> DefaultHediffPawnCapacityModifiers { get; private set; }
		public static List<PawnCapacityModifier> HediffPawnCapacityModifiers
		{
			get => HediffDefOf.NeuralSupercharge.stages.First().capMods;
			set => HediffDefOf.NeuralSupercharge.stages.First().capMods = value;
		}
		public static List<StatModifier> DefaultHediffStatFactors { get; private set; }
		public static List<StatModifier> HediffStatFactors
		{
			get => HediffDefOf.NeuralSupercharge.stages.First().statFactors;
			set => HediffDefOf.NeuralSupercharge.stages.First().statFactors = value;
		}
		public static List<StatModifier> DefaultHediffStatOffset { get; private set; }
		public static List<StatModifier> HediffStatOffset
		{
			get => HediffDefOf.NeuralSupercharge.stages.First().statOffsets;
			set => HediffDefOf.NeuralSupercharge.stages.First().statOffsets = value;
		}
		public static float DefaultHediffHungerRateFactor { get; private set; }
		public static float HediffHungerRateFactor
		{
			get => HediffDefOf.NeuralSupercharge.stages.First().hungerRateFactorOffset;
			set => HediffDefOf.NeuralSupercharge.stages.First().hungerRateFactorOffset = value;
		}
		#endregion

		#region FIELDS
		private static readonly List<MemeDef> _memeDefsWithDesignator = new List<MemeDef>();
		#endregion

		#region CONSTRUCTORS
		public NeuralSuperchargerSettings(ThingDef neuralSupercharger) : base(neuralSupercharger)
		{
			DefaultTicksToRecharge = TicksToRecharge;
		}
		#endregion

		#region PUBLIC METHODS
		public static void InitializeStatics()
		{
			DefaultAnyoneCanBuild = AnyoneCanBuild;

			DefaultHediffDisappearsAfterTicks = HediffDisappearsAfterTicks;

			if (HediffPawnCapacityModifiers == null)
				HediffPawnCapacityModifiers = new List<PawnCapacityModifier>();
			DefaultHediffPawnCapacityModifiers = HediffPawnCapacityModifiers.Select(x => x.Clone()).ToList();

			if (HediffStatFactors == null)
				HediffStatFactors = new List<StatModifier>();
			DefaultHediffStatFactors = HediffStatFactors.Select(x => x.Clone()).ToList();

			if (HediffStatOffset == null)
				HediffStatOffset = new List<StatModifier>();
			DefaultHediffStatOffset = HediffStatOffset.Select(x => x.Clone()).ToList();

			DefaultHediffHungerRateFactor = HediffHungerRateFactor;
		}

		public static void ExposeStatics()
		{
			if (Scribe.EnterNode(nameof(NeuralSuperchargerSettings)))
			{
				try
				{
					bool boolValue = AnyoneCanBuild;
					Scribe_Values.Look(ref boolValue, nameof(AnyoneCanBuild), DefaultAnyoneCanBuild);
					AnyoneCanBuild = boolValue;

					int intValue = HediffDisappearsAfterTicks;
					Scribe_Values.Look(ref intValue, nameof(HediffDisappearsAfterTicks), DefaultHediffDisappearsAfterTicks);
					HediffDisappearsAfterTicks = intValue;

					float floatValue = HediffHungerRateFactor;
					Scribe_Values.Look(ref floatValue, nameof(HediffHungerRateFactor), DefaultHediffHungerRateFactor);
					HediffHungerRateFactor = floatValue;

					BPaNSUtility.ExposeListLook(HediffPawnCapacityModifiers, nameof(HediffPawnCapacityModifiers), DefaultHediffPawnCapacityModifiers, BPaNSUtility.LookPawnCapacityModifier, BPaNSUtility.IsModified);
					BPaNSUtility.ExposeListLook(HediffStatFactors, nameof(HediffStatFactors), DefaultHediffStatFactors, BPaNSUtility.LookStatModifier, BPaNSUtility.IsModified);
					BPaNSUtility.ExposeListLook(HediffStatOffset, nameof(HediffStatOffset), DefaultHediffStatOffset, BPaNSUtility.LookStatModifier, BPaNSUtility.IsModified);
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
			DefaultAnyoneCanBuild != AnyoneCanBuild
			|| DefaultHediffDisappearsAfterTicks != HediffDisappearsAfterTicks
			|| DefaultHediffPawnCapacityModifiers.IsModified(HediffPawnCapacityModifiers)
			|| DefaultHediffStatFactors.IsModified(HediffStatFactors)
			|| DefaultHediffStatOffset.IsModified(HediffStatOffset)
			|| DefaultHediffHungerRateFactor != HediffHungerRateFactor;
		#endregion

		#region OVERRIDES
		public override bool IsModified() =>
			base.IsModified()
			|| DefaultTicksToRecharge != TicksToRecharge;

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && !IsModified())
				return;

			if (Scribe.EnterNode(Def.defName))
			{
				try
				{
					base.ExposeData();

					int intValue = TicksToRecharge;
					Scribe_Values.Look(ref intValue, nameof(TicksToRecharge), DefaultTicksToRecharge);
					TicksToRecharge = intValue;
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
			if (to != this && to is NeuralSuperchargerSettings copy)
			{
				base.CopyTo(to);

				copy.TicksToRecharge = TicksToRecharge;
			}
		}
		#endregion

		#region PRIVATE METHODS
		private static void SetAnyoneCanBuild(bool value)
		{
			if (_anyoneCanBuild != value)
			{
				_anyoneCanBuild = value;

				if (value)
				{
					foreach (var memeDef in DefDatabase<MemeDef>.AllDefs)
					{
						if (memeDef.addDesignatorGroups?.Contains(BPaNSDefOf.SY_BNV_NeuralSuperchargers) == true)
						{
							if (!_memeDefsWithDesignator.Contains(memeDef))
								_memeDefsWithDesignator.AddDistinct(memeDef);
							memeDef.addDesignatorGroups.Remove(BPaNSDefOf.SY_BNV_NeuralSuperchargers);
						}
					}
				}
				else
				{
					foreach (var memeDef in _memeDefsWithDesignator)
						if (!memeDef.addDesignatorGroups.Contains(BPaNSDefOf.SY_BNV_NeuralSuperchargers))
							memeDef.addDesignatorGroups.Add(BPaNSDefOf.SY_BNV_NeuralSuperchargers);
				}

				foreach (var def in BPaNSUtility.GetNeuralSuperchargerDefs())
					def.canGenerateDefaultDesignator = value;
			}
		}
		#endregion
	}
}
