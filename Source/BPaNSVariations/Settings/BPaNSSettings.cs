using BPaNSVariations.Utility;
using RimWorld;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Verse;

namespace BPaNSVariations.Settings
{
	internal class BPaNSSettings : ModSettings
	{
		#region PROPERTIES
		public List<BaseSettings> AllSettings { get; }
		public List<BiosculpterPodSettings> BiosculpterPodSettings { get; }
		public List<NeuralSuperchargerSettings> NeuralSuperchargerSettings { get; }
		public List<SleepAcceleratorSettings> SleepAcceleratorSettings { get; }

		public ReadOnlyCollection<ThingDef> BuildCostThingDefs { get; }
		public ReadOnlyCollection<ThingDef> MedicineThingDefs { get; }

		public ReadOnlyCollection<PawnCapacityDef> PawnCapacityDefs { get; }
		public ReadOnlyCollection<StatDef> StatDefs { get; }
		#endregion

		#region FIELDS
		#endregion

		#region CONSTRUCTORS
		public BPaNSSettings()
		{
			// Get Def lists
			var thingDefs = DefDatabase<ThingDef>.AllDefs.Where(
				def =>
				def.category == ThingCategory.Item
				&&  def.CountAsResource
				&& !def.MadeFromStuff
				&& (def.thingCategories?.Contains(ThingCategoryDefOf.ResourcesRaw) == true
				||  def.thingCategories?.Contains(ThingCategoryDefOf.Manufactured) == true)
				).ToList();
			thingDefs.SortBy(def => def.label ?? "");
			BuildCostThingDefs = thingDefs.AsReadOnly();

			thingDefs = DefDatabase<ThingDef>.AllDefs.Where(def => def.IsMedicine).ToList();
			thingDefs.SortBy(def => def.label ?? "");
			MedicineThingDefs = thingDefs.AsReadOnly();

			var pawnCapacityDefs = DefDatabase<PawnCapacityDef>.AllDefs.ToList();
			pawnCapacityDefs.SortBy(def => def.label ?? "");
			PawnCapacityDefs = pawnCapacityDefs.AsReadOnly();

			var statDefs = DefDatabase<StatDef>.AllDefs.Where(
				def => 
				def.category == StatCategoryDefOf.BasicsPawn
				|| def.category == StatCategoryDefOf.BasicsPawnImportant
				|| def.category == StatCategoryDefOf.PawnCombat
				|| def.category == StatCategoryDefOf.PawnMisc
				|| def.category == StatCategoryDefOf.PawnSocial
				|| def.category == StatCategoryDefOf.PawnWork
				).ToList();
			statDefs.SortBy(def => def.label ?? "");
			StatDefs = statDefs.AsReadOnly();

			// Create Settings
			BiosculpterPodSettings = BPaNSUtility.GetBiosculpterPodDefs().Select(def => new BiosculpterPodSettings(def)).ToList();
			NeuralSuperchargerSettings = BPaNSUtility.GetNeuralSuperchargerDefs().Select(def => new NeuralSuperchargerSettings(def)).ToList();
			SleepAcceleratorSettings = BPaNSUtility.GetSleepAcceleratorDefs().Select(def => new SleepAcceleratorSettings(def)).ToList();

			AllSettings = new List<BaseSettings>();
			AllSettings.AddRange(BiosculpterPodSettings);
			AllSettings.AddRange(NeuralSuperchargerSettings);
			AllSettings.AddRange(SleepAcceleratorSettings);
		}
		#endregion

		#region OVERRIDES
		public override void ExposeData()
		{
			base.ExposeData();

			Settings.BiosculpterPodSettings.ExposeStatics();
			Settings.NeuralSuperchargerSettings.ExposeStatics();
			Settings.SleepAcceleratorSettings.ExposeStatics();

			foreach (var settings in AllSettings)
				settings.ExposeData();
		}
		#endregion
	}
}
