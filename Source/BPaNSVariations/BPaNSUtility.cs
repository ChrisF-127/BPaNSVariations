using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;
using Verse.AI;
using System.Reflection;
using System.Windows.Markup;

namespace BPaNSVariations
{
	[StaticConstructorOnStartup]
	internal static class BPaNSUtility
	{
		#region FIELDS
		public readonly static ThingDef BiosculpterPod_2x2_Left;
		public readonly static ThingDef BiosculpterPod_2x2_Right;
		public readonly static ThingDef BiosculpterPod_1x2_Center;
		public readonly static ThingDef BiosculpterPod_1x3_Center;
		public readonly static EffecterDef BiosculpterPod_Ready;
		public readonly static FleckDef BiosculpterScanner_Ready;
		public readonly static (float FadeIn, float FadeOut, float Solid) OriginalBiosculpterScanner_ReadyValues; // FadeIn, FadeOut, Solid

		public readonly static ThingDef NeuralSupercharger_1x2_Center;
		#endregion

		#region CONSTRUCTORS
		static BPaNSUtility()
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
			OriginalBiosculpterScanner_ReadyValues = (BiosculpterScanner_Ready.fadeInTime, BiosculpterScanner_Ready.fadeOutTime, BiosculpterScanner_Ready.solidTime);


			// --- NEURAL SUPERCHARGER
			NeuralSupercharger_1x2_Center = DefDatabase<ThingDef>.GetNamed("NeuralSupercharger_1x2_Center");

			// floor effect for 1x2 neural supercharger
			var neuralSuperchargerChargedFloor_1x2_Center = DefDatabase<FleckDef>.GetNamed("NeuralSuperchargerChargedFloor_1x2_Center");
			neuralSuperchargerChargedFloor_1x2_Center.graphicData.drawSize.y = 2f;
			var neuralSuperchargerCharged_1x2_Center = DefDatabase<EffecterDef>.GetNamed("NeuralSuperchargerCharged_1x2_Center");
			neuralSuperchargerCharged_1x2_Center.children.First(e => e.fleckDef.defName == "NeuralSuperchargerChargedFloor").fleckDef = neuralSuperchargerChargedFloor_1x2_Center;
			NeuralSupercharger_1x2_Center.GetCompProperties<CompProperties_NeuralSupercharger>().effectCharged = neuralSuperchargerCharged_1x2_Center;
		}
		#endregion

		#region PUBLIC METHODS
		public static T GetSingleCompPropertiesOfType<T>(this ThingDef def)
			where T : CompProperties
		{
			var props = GetCompPropertiesOfType<T>(def);
			if (props.Count() != 1)
				throw new Exception($"{nameof(GetSingleCompPropertiesOfType)} encountered invalid number of CompProperties:\n{typeof(T)}\n{props.Count()}");
			return props.First();
		}
		public static IEnumerable<T> GetCompPropertiesOfType<T>(params ThingDef[] defs)
			where T : CompProperties => 
			defs.GetCompPropertiesOfType<T>();
		public static IEnumerable<T> GetCompPropertiesOfType<T>(this IEnumerable<ThingDef> defs)
			where T : CompProperties
		{
			foreach (var def in defs)
				foreach (var comp in def.comps)
					if (comp is T t)
						yield return t;
		}
		public static Prop GetSingleCompPropertiesOfTypeWithCompClass<Prop, Class>(this ThingDef def)
			where Prop : CompProperties
			where Class : ThingComp
		{
			var props = GetCompPropertiesOfTypeWithCompClass<Prop, Class>(def);
			if (props.Count() != 1)
				throw new Exception($"{nameof(GetSingleCompPropertiesOfTypeWithCompClass)} encountered invalid number of CompProperties:\n{typeof(Prop)}\n{typeof(Class)}\n{props.Count()}");
			return props.First();
		}
		public static IEnumerable<Prop> GetCompPropertiesOfTypeWithCompClass<Prop, Class>(params ThingDef[] defs)
			where Prop : CompProperties
			where Class : ThingComp =>
			defs.GetCompPropertiesOfTypeWithCompClass<Prop, Class>();
		public static IEnumerable<Prop> GetCompPropertiesOfTypeWithCompClass<Prop, Class>(this IEnumerable<ThingDef> defs)
			where Prop : CompProperties
			where Class : ThingComp
		{
			foreach (var comp in GetCompPropertiesOfType<Prop>(defs))
				if (comp.compClass == typeof(Class))
					yield return comp;
		}

		public static IEnumerable<ThingDef> GetBiosculpterPodDefs()
		{
			yield return ThingDefOf.BiosculpterPod;
			yield return BiosculpterPod_2x2_Left;
			yield return BiosculpterPod_2x2_Right;
			yield return BiosculpterPod_1x2_Center;
			yield return BiosculpterPod_1x3_Center;
		}
		public static bool IsDefBiosculpterPod(ThingDef def) =>
			GetBiosculpterPodDefs().Contains(def);

		public static IEnumerable<ThingDef> GetNeuralSuperchargerDefs()
		{
			yield return ThingDefOf.NeuralSupercharger;
			yield return NeuralSupercharger_1x2_Center;
		}
		public static bool IsDefNeuralSupercharger(ThingDef def) =>
			GetNeuralSuperchargerDefs().Contains(def);


		public static void Overwrite(this List<ThingDefCountClass> to, IEnumerable<ThingDefCountClass> from)
		{
			to.Clear();
			to.AddRange(from.Select(v => new ThingDefCountClass(v.thingDef, v.count)));
		}


		public static bool IsModified(this List<ThingDefCountClass> listA, List<ThingDefCountClass> listB)
		{
			if (listA.Count != listB.Count)
				return true;
			foreach (var a in listA)
				if (!listB.Any(b => a.thingDef == b.thingDef && a.count == b.count))
					return true;
			foreach (var b in listB)
				if (!listA.Any(a => a.thingDef == b.thingDef && a.count == b.count))
					return true;
			return false;
		}


		public static void ExposeList<T>(List<T> values, string name, Func<bool> isModified)
		{
			if (Scribe.mode != LoadSaveMode.Saving || isModified())
			{
				var temp = values;
				Scribe_Collections.Look(ref temp, name);
				if (temp != null && values != temp)
				{
					values.Clear();
					values.AddRange(temp);
				}
			}
		}
		#endregion
	}
}
