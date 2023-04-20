using BPaNSVariations.Utility;
using RimWorld;
using System;
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
		#endregion

		#region FIELDS
		#endregion

		#region CONSTRUCTORS
		public BPaNSSettings()
		{
			// Get ThingDef lists
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

			foreach (var settings in AllSettings)
				settings.ExposeData();
		}
		#endregion
	}
}
