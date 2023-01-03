using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace BPaNSVariations
{
	[StaticConstructorOnStartup]
	internal static class BPaNSStatics
	{
		#region FIELDS
		public readonly static ThingDef BiosculpterPod_2x2_Left;
		public readonly static ThingDef BiosculpterPod_2x2_Right;
		public readonly static ThingDef BiosculpterPod_1x2_Center;
		public readonly static ThingDef BiosculpterPod_1x3_Center;

		public readonly static ThingDef NeuralSupercharger_1x2_Center;

		public readonly static EffecterDef BiosculpterPod_Ready;
		public readonly static FleckDef BiosculpterScanner_Ready;
		public readonly static Tuple<float, float, float> OriginalBiosculpterScanner_ReadyValues; // FadeIn, FadeOut, Solid
		#endregion

		#region CONSTRUCTORS
		static BPaNSStatics()
		{
			// --- BIOSCULPTER POD
			BiosculpterPod_2x2_Left = DefDatabase<ThingDef>.GetNamed("BiosculpterPod_2x2_Left");
			BiosculpterPod_2x2_Right = DefDatabase<ThingDef>.GetNamed("BiosculpterPod_2x2_Right");
			BiosculpterPod_1x2_Center = DefDatabase<ThingDef>.GetNamed("BiosculpterPod_1x2_Center");
			BiosculpterPod_1x3_Center = DefDatabase<ThingDef>.GetNamed("BiosculpterPod_1x3_Center");

			var biosculpterPod_Operating = DefDatabase<EffecterDef>.GetNamed("BiosculpterPod_Operating");
			BiosculpterPod_Ready = DefDatabase<EffecterDef>.GetNamed("BiosculpterPod_Ready");

			var biosculpterScanner_Forward = DefDatabase<FleckDef>.GetNamed("BiosculpterScanner_Forward");
			var biosculpterScanner_Backward = DefDatabase<FleckDef>.GetNamed("BiosculpterScanner_Backward");
			BiosculpterScanner_Ready = DefDatabase<FleckDef>.GetNamed("BiosculpterScanner_Ready");

			// Fix effecter position; necessary since we make the effect appear between the interaction spot and 1.5 cells away from it depending on rotation, 
			//	but it needs to be 1 cells away, which TargetInfo does not allow for without giving it a Thing with a fitting center which we do not have on 2x2
			biosculpterPod_Operating.offsetTowardsTarget = new FloatRange(0.5f, 0.5f);
			BiosculpterPod_Ready.offsetTowardsTarget = new FloatRange(0.5f, 0.5f);

			// Resize FleckDefs for the Effecters to look more fitting for the smaller buildings
			biosculpterScanner_Forward.graphicData.drawSize = new Vector2(1.5f, 0.5f); // standard is 3x1
			biosculpterScanner_Backward.graphicData.drawSize = new Vector2(1f, 0.5f); // standard is 2x1
			BiosculpterScanner_Ready.graphicData.drawSize = new Vector2(1f, 2f); // standard is 2x2
			OriginalBiosculpterScanner_ReadyValues = new Tuple<float, float, float>(BiosculpterScanner_Ready.fadeInTime, BiosculpterScanner_Ready.fadeOutTime, BiosculpterScanner_Ready.solidTime);


			// --- NEURAL SUPERCHARGER
			NeuralSupercharger_1x2_Center = DefDatabase<ThingDef>.GetNamed("NeuralSupercharger_1x2_Center");

			// replace floor effect for 1x2 neural supercharger
			var neuralSuperchargerChargedFloor_1x2_Center = DefDatabase<FleckDef>.GetNamed("NeuralSuperchargerChargedFloor_1x2_Center");
			neuralSuperchargerChargedFloor_1x2_Center.graphicData.drawSize.y = 2f;
			var neuralSuperchargerCharged_1x2_Center = DefDatabase<EffecterDef>.GetNamed("NeuralSuperchargerCharged_1x2_Center");
			neuralSuperchargerCharged_1x2_Center.children.First(e => e.fleckDef.defName == "NeuralSuperchargerChargedFloor").fleckDef = neuralSuperchargerChargedFloor_1x2_Center;
			NeuralSupercharger_1x2_Center.GetCompProperties<CompProperties_NeuralSupercharger>().effectCharged = neuralSuperchargerCharged_1x2_Center;
		}
		#endregion

		#region PUBLIC METHODS
		public static IEnumerable<CompProperties_BiosculpterPod> GetAll_CompProperties_BiosculpterPod()
		{
			yield return ThingDefOf.BiosculpterPod.GetCompProperties<CompProperties_BiosculpterPod>();
			yield return BiosculpterPod_2x2_Left.GetCompProperties<CompProperties_BiosculpterPod>();
			yield return BiosculpterPod_2x2_Right.GetCompProperties<CompProperties_BiosculpterPod>();
			yield return BiosculpterPod_1x2_Center.GetCompProperties<CompProperties_BiosculpterPod>();
			yield return BiosculpterPod_1x3_Center.GetCompProperties<CompProperties_BiosculpterPod>();
		}

		public static bool IsDefBiosculpterPod(ThingDef def) =>
			def == ThingDefOf.BiosculpterPod 
			|| def == BiosculpterPod_2x2_Left 
			|| def == BiosculpterPod_2x2_Right 
			|| def == BiosculpterPod_1x2_Center 
			|| def == BiosculpterPod_1x3_Center;

		public static bool IsDefNeuralSupercharger(ThingDef def) =>
			def == ThingDefOf.NeuralSupercharger 
			|| def == NeuralSupercharger_1x2_Center;
		#endregion
	}
}
