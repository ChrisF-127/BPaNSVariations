using BPaNSVariations.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BPaNSVariations.Settings
{
	internal class SleepAcceleratorSettings : BaseSettings
	{
		//General
		//x Build cost [requires reload]
		//x Work to build
		//x Active power consumption
		//x In Use power consumption [requires reload if toggled]

		//Misc
		//x Anyone can build toggle [global, requires restart]

		//Hediff
		//x Bed Rest Effectiveness
		//x Bed Hunger Rate Multiplier

		#region PROPERTIES
		public float? DefaultInUsePowerConsumption { get; }
		public float? InUsePowerConsumption
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_FacilityInUse>().inUsePowerConsumption;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_FacilityInUse>().inUsePowerConsumption = value;
		}

		public static bool DefaultAnyoneCanBuild { get; private set; }
		public static bool _anyoneCanBuild = false;
		public static bool AnyoneCanBuild
		{
			get => _anyoneCanBuild;
			set => SetAnyoneCanBuild(value);
		}

		public float DefaultBedRestEffectiveness { get; }
		public float BedRestEffectiveness
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_Facility>().statOffsets.First(v => v.stat == StatDefOf.BedRestEffectiveness).value;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_Facility>().statOffsets.First(v => v.stat == StatDefOf.BedRestEffectiveness).value = value;
		}
		public float DefaultBedHungerRateFactor { get; }
		public float BedHungerRateFactor
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_Facility>().statOffsets.First(v => v.stat == StatDefOf.BedHungerRateFactor).value;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_Facility>().statOffsets.First(v => v.stat == StatDefOf.BedHungerRateFactor).value = value;
		}
		#endregion

		#region FIELDS
		private static readonly List<MemeDef> _memeDefsWithDesignator = new List<MemeDef>();
		#endregion

		#region CONSTRUCTORS
		public SleepAcceleratorSettings(ThingDef sleepAccelerator) : base(sleepAccelerator)
		{
			DefaultInUsePowerConsumption = InUsePowerConsumption;

			DefaultBedRestEffectiveness = BedRestEffectiveness;
			DefaultBedHungerRateFactor = BedHungerRateFactor;
		}
		#endregion

		#region PUBLIC METHODS
		public static void InitializeStatics()
		{
			DefaultAnyoneCanBuild = AnyoneCanBuild;
		}

		public static void ExposeStatics()
		{
			if (Scribe.EnterNode(nameof(SleepAcceleratorSettings)))
			{
				try
				{
					bool boolValue = AnyoneCanBuild;
					Scribe_Values.Look(ref boolValue, nameof(AnyoneCanBuild), DefaultAnyoneCanBuild);
					AnyoneCanBuild = boolValue;
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
			DefaultAnyoneCanBuild != AnyoneCanBuild;
		#endregion

		#region OVERRIDES
		public override bool IsModified() =>
			base.IsModified()
			|| InUsePowerConsumption != DefaultInUsePowerConsumption
			|| BedRestEffectiveness != DefaultBedRestEffectiveness
			|| BedHungerRateFactor != DefaultBedHungerRateFactor;

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && !IsModified())
				return;

			if (Scribe.EnterNode(Def.defName))
			{
				try
				{
					base.ExposeData();

					float? nullableFloatValue = InUsePowerConsumption;
					Scribe_Values.Look(ref nullableFloatValue, nameof(InUsePowerConsumption), DefaultInUsePowerConsumption);
					InUsePowerConsumption = nullableFloatValue;

					float floatValue = BedRestEffectiveness;
					Scribe_Values.Look(ref floatValue, nameof(BedRestEffectiveness), DefaultBedRestEffectiveness);
					BedRestEffectiveness = floatValue;
					floatValue = BedHungerRateFactor;
					Scribe_Values.Look(ref floatValue, nameof(BedHungerRateFactor), DefaultBedHungerRateFactor);
					BedHungerRateFactor = floatValue;
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
			if (to != this && to is SleepAcceleratorSettings copy)
			{
				base.CopyTo(to);

				copy.InUsePowerConsumption = InUsePowerConsumption;

				copy.BedRestEffectiveness = BedRestEffectiveness;
				copy.BedHungerRateFactor = BedHungerRateFactor;
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
						if (memeDef.addDesignators?.Contains(BPaNSDefOf.SleepAccelerator) == true)
						{
							if (!_memeDefsWithDesignator.Contains(memeDef))
								_memeDefsWithDesignator.AddDistinct(memeDef);
							memeDef.addDesignators.Remove(BPaNSDefOf.SleepAccelerator);
						}
					}
				}
				else
				{
					foreach (var memeDef in _memeDefsWithDesignator)
						if (!memeDef.addDesignators.Contains(BPaNSDefOf.SleepAccelerator))
							memeDef.addDesignators.Add(BPaNSDefOf.SleepAccelerator);
				}

				foreach (var def in BPaNSUtility.GetSleepAcceleratorDefs())
					def.canGenerateDefaultDesignator = value;
			}
		}
		#endregion
	}
}
