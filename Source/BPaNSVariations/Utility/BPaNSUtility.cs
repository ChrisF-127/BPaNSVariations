using System.Linq;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using static Mono.Security.X509.X520;
using System.Xml;
using UnityEngine;

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

		public static bool Compare(this PawnCapacityModifier a, PawnCapacityModifier b) =>
			a.capacity == b.capacity
			|| a.offset == b.offset
			|| a.postFactor == b.postFactor
			|| a.setMax == b.setMax
			|| a.setMaxCurveEvaluateStat == b.setMaxCurveEvaluateStat
			|| a.setMaxCurveOverride == b.setMaxCurveOverride;
		public static PawnCapacityModifier Clone(this PawnCapacityModifier modifier) =>
			new PawnCapacityModifier
			{
				capacity = modifier.capacity,
				offset = modifier.offset,
				postFactor = modifier.postFactor,
				setMax = modifier.setMax,
				setMaxCurveEvaluateStat = modifier.setMaxCurveEvaluateStat,
				setMaxCurveOverride = modifier.setMaxCurveOverride
			};
		public static void LookPawnCapacityModifier(PawnCapacityModifier value)
		{
			Scribe_Defs.Look(ref value.capacity, nameof(value.capacity));
			Scribe_Values.Look(ref value.offset, nameof(value.offset));
			Scribe_Values.Look(ref value.postFactor, nameof(value.postFactor));
			Scribe_Values.Look(ref value.setMax, nameof(value.setMax));
			Scribe_Defs.Look(ref value.setMaxCurveEvaluateStat, nameof(value.setMaxCurveEvaluateStat));
			Scribe_Values.Look(ref value.setMaxCurveOverride, nameof(value.setMaxCurveOverride));
		}

		public static bool Compare(this StatModifier a, StatModifier b) =>
			a.stat == b.stat
			|| a.value == b.value;
		public static StatModifier Clone(this StatModifier modifier) =>
			new StatModifier
			{
				stat = modifier.stat,
				value = modifier.value,
			};
		public static void LookStatModifier(StatModifier value)
		{
			Scribe_Defs.Look(ref value.stat, nameof(value.stat));
			Scribe_Values.Look(ref value.value, nameof(value.value));
		}

		public static void ExposeListLook<T>(IList<T> values, string nodeName, IList<T> defaultValues, Action<T> look, Func<T, T, bool> compare)
			where T : new()
		{
			Log.Message($"{Scribe.mode}");

			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (values.Count != defaultValues.Count || !values.Any(v => defaultValues.Any(d => compare(v, d))))
				{
					if (Scribe.EnterNode(nodeName))
					{
						foreach (var value in values)
						{
							if (Scribe.EnterNode("li"))
							{
								look(value);
								Scribe.ExitNode();
							}
						}
						Scribe.ExitNode();
					}
				}
			}
			else
			{
				if (Scribe.EnterNode(nodeName))
				{
					if (Scribe.loader.curXmlParent != null)
					{
						int i = 0;
						var curXmlParent = Scribe.loader.curXmlParent;
						var curPathRelToParent = Scribe.loader.curPathRelToParent;
						foreach (XmlNode childNode in curXmlParent.ChildNodes)
						{
							// Enter
							Scribe.loader.curXmlParent = childNode;
							Scribe.loader.curPathRelToParent = curPathRelToParent + "/li";

							if (i >= values.Count)
								values.Add(new T());
							look(values[i]);

							// Exit
							Scribe.loader.curXmlParent = curXmlParent;
							Scribe.loader.curPathRelToParent = curPathRelToParent;
							i++;
						}
					}
					Scribe.ExitNode();
				}
			}
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
