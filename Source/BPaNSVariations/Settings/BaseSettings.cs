using BPaNSVariations.Utility;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace BPaNSVariations.Settings
{
	internal abstract class BaseSettings : IExposable
	{
		#region PROPERTIES
		public virtual ThingDef Def { get; }

		public virtual float DefaultActivePowerConsumption { get; }
		public virtual float ActivePowerConsumption
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_Power>().basePowerConsumption;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_Power>().basePowerConsumption = value;
		}
		public virtual List<ThingDefCountClass> DefaultBuildCost { get; } = new List<ThingDefCountClass>();
		public virtual List<ThingDefCountClass> BuildCost => Def.costList;
		public virtual float DefaultWorkToBuild { get; }
		public virtual float WorkToBuild
		{
			get => Def.GetStatValueAbstract(StatDefOf.WorkToBuild);
			set => Def.SetStatBaseValue(StatDefOf.WorkToBuild, value);
		}
		#endregion

		#region CONSTRUCTORS
		public BaseSettings(ThingDef def)
		{
			Def = def;

			#region GENERAL
			DefaultWorkToBuild = WorkToBuild;
			DefaultBuildCost.SetFrom(BuildCost);
			DefaultActivePowerConsumption = ActivePowerConsumption;
			#endregion
		}
		#endregion

		#region METHODS
		public virtual string GetName() =>
			Def.LabelCap;

		public virtual bool IsModified() =>
			DefaultWorkToBuild != WorkToBuild
			|| DefaultBuildCost.AnyDifference(BuildCost)
			|| DefaultActivePowerConsumption != ActivePowerConsumption;

		public virtual void ExposeData()
		{
			#region GENERAL
			float floatValue = WorkToBuild;
			Scribe_Values.Look(ref floatValue, nameof(WorkToBuild), DefaultWorkToBuild);
			WorkToBuild = floatValue;

			BPaNSUtility.ExposeList(BuildCost, nameof(BuildCost), () => BuildCost.AnyDifference(DefaultBuildCost));

			floatValue = ActivePowerConsumption;
			Scribe_Values.Look(ref floatValue, nameof(ActivePowerConsumption), DefaultActivePowerConsumption);
			ActivePowerConsumption = floatValue;
			#endregion
		}

		public virtual void CopyTo(BaseSettings to)
		{
			to.ActivePowerConsumption = ActivePowerConsumption;
			to.BuildCost.SetFrom(BuildCost);
			to.WorkToBuild = WorkToBuild;
		}
		#endregion
	}
}
