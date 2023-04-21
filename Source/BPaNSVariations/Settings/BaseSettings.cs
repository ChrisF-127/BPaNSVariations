using BPaNSVariations.Utility;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public virtual float DefaultStandbyPowerConsumption { get; }
		public virtual float StandbyPowerConsumption
		{
			get => Def.GetSingleCompPropertiesOfType<CompProperties_Power>().idlePowerDraw;
			set => Def.GetSingleCompPropertiesOfType<CompProperties_Power>().idlePowerDraw = value;
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
			DefaultActivePowerConsumption = ActivePowerConsumption;
			DefaultStandbyPowerConsumption = StandbyPowerConsumption;
			DefaultBuildCost.SetFrom(BuildCost);
			DefaultWorkToBuild = WorkToBuild;
			#endregion
		}
		#endregion

		#region METHODS
		public virtual string GetName() =>
			Def.LabelCap;

		public virtual bool IsModified() =>
			DefaultActivePowerConsumption != ActivePowerConsumption
			|| DefaultStandbyPowerConsumption != StandbyPowerConsumption
			|| DefaultBuildCost.AnyDifference(BuildCost)
			|| DefaultWorkToBuild != WorkToBuild;

		public virtual void ExposeData()
		{
			#region GENERAL
			float floatValue = ActivePowerConsumption;
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
		}

		public virtual void CopyTo(BaseSettings to)
		{
			to.ActivePowerConsumption = ActivePowerConsumption;
			to.StandbyPowerConsumption = StandbyPowerConsumption;
			to.BuildCost.SetFrom(BuildCost);
			to.WorkToBuild = WorkToBuild;
		}
		#endregion
	}
}
