using RimWorld;
using Verse;

namespace BPaNSVariations.Utility
{
	[DefOf]
	static class BPaNSDefOf
	{
#pragma warning disable 0649 // disable "never assigned to" warning
		public static ThingDef BiosculpterPod_2x2_Left;
		public static ThingDef BiosculpterPod_2x2_Right;
		public static ThingDef BiosculpterPod_1x2_Center;
		public static ThingDef BiosculpterPod_1x3_Center;

		public static ThingDef NeuralSupercharger_1x2_Center;

		public static ThingDef SleepAccelerator;

		public static DesignatorDropdownGroupDef SY_BNV_BiosculpterPods;
		public static DesignatorDropdownGroupDef SY_BNV_NeuralSuperchargers;
#pragma warning restore 0649
	}
}
