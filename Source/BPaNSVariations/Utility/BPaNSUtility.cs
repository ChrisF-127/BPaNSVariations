using System.Linq;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;

namespace BPaNSVariations.Utility
{
	[StaticConstructorOnStartup]
	internal static class BPaNSUtility
	{
		#region CONSTRUCTORS
		static BPaNSUtility()
		{
			// floor effect for 1x2 neural supercharger
			var neuralSuperchargerChargedFloor_1x2_Center = DefDatabase<FleckDef>.GetNamed("NeuralSuperchargerChargedFloor_1x2_Center");
			neuralSuperchargerChargedFloor_1x2_Center.graphicData.drawSize.y = 2f;
			var neuralSuperchargerCharged_1x2_Center = DefDatabase<EffecterDef>.GetNamed("NeuralSuperchargerCharged_1x2_Center");
			neuralSuperchargerCharged_1x2_Center.children.First(e => e.fleckDef.defName == "NeuralSuperchargerChargedFloor").fleckDef = neuralSuperchargerChargedFloor_1x2_Center;
			BPaNSDefOf.NeuralSupercharger_1x2_Center.GetCompProperties<CompProperties_NeuralSupercharger>().effectCharged = neuralSuperchargerCharged_1x2_Center;
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

		public static T GetSingleCompPropertiesOfType<T>(this HediffDef def)
			where T : HediffCompProperties
		{
			var props = GetCompPropertiesOfType<T>(def);
			if (props.Count() != 1)
				throw new Exception($"{nameof(GetSingleCompPropertiesOfType)} encountered invalid number of CompProperties:\n{typeof(T)}\n{props.Count()}");
			return props.First();
		}
		public static IEnumerable<T> GetCompPropertiesOfType<T>(params HediffDef[] defs)
			where T : HediffCompProperties =>
			defs.GetCompPropertiesOfType<T>();
		public static IEnumerable<T> GetCompPropertiesOfType<T>(this IEnumerable<HediffDef> defs)
			where T : HediffCompProperties
		{
			foreach (var def in defs)
				foreach (var comp in def.comps)
					if (comp is T t)
						yield return t;
		}

		public static IEnumerable<ThingDef> GetBiosculpterPodDefs()
		{
			yield return ThingDefOf.BiosculpterPod;
			yield return BPaNSDefOf.BiosculpterPod_2x2_Left;
			yield return BPaNSDefOf.BiosculpterPod_2x2_Right;
			yield return BPaNSDefOf.BiosculpterPod_1x2_Center;
			yield return BPaNSDefOf.BiosculpterPod_1x3_Center;
		}
		public static bool IsDefBiosculpterPod(ThingDef def) =>
			GetBiosculpterPodDefs().Contains(def);

		public static IEnumerable<ThingDef> GetNeuralSuperchargerDefs()
		{
			yield return ThingDefOf.NeuralSupercharger;
			yield return BPaNSDefOf.NeuralSupercharger_1x2_Center;
		}
		public static bool IsDefNeuralSupercharger(ThingDef def) =>
			GetNeuralSuperchargerDefs().Contains(def);

		public static IEnumerable<ThingDef> GetSleepAcceleratorDefs()
		{
			yield return BPaNSDefOf.SleepAccelerator;
		}
		public static bool IsDefSleepAccelerator(ThingDef def) =>
			GetSleepAcceleratorDefs().Contains(def);


		public static void SetFrom(this List<ThingDefCountClass> to, IEnumerable<ThingDefCountClass> from)
		{
			to.Clear();
			to.AddRange(from.Select(v => new ThingDefCountClass(v.thingDef, v.count)));
		}
		public static bool AnyDifference(this List<ThingDefCountClass> listA, List<ThingDefCountClass> listB)
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

		public static TValue GetOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
		{
			if (dictionary.TryGetValue(key, out TValue value))
				return value;
			dictionary.Add(key, defaultValue);
			return defaultValue;
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
		public static void ExposeSimpleCurve(SimpleCurve curve, string name, Func<bool> isModified)
		{
			if (Scribe.mode != LoadSaveMode.Saving || isModified())
			{
				var temp = curve.ToList();
				Scribe_Collections.Look(ref temp, name);
				if (temp != null)
					curve.SetPoints(temp);
			}
		}
		#endregion
	}
}
