using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using Verse.AI;
using System;
using System.Linq;
using HugsLib.Settings;
using System.Collections;

namespace BPaNSVariations
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		#region CONSTRUCTORS
		static HarmonyPatches()
		{
			var harmony = new Harmony("syrus.bpansvariations");

			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), "PostDraw"),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.CompBiosculpterPod_PostDraw_Transpiler)));
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), "CompTick"),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.CompBiosculpterPod_CompTick_Transpiler)));
			harmony.Patch(
				AccessTools.Method(typeof(JobGiver_GetNeuralSupercharge), "ClosestSupercharger"),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.JobGiver_GetNeuralSupercharge_ClosestSupercharger_Transpiler)));
		}
		#endregion

		#region HARMONY PATCHES
		static IEnumerable<CodeInstruction> CompBiosculpterPod_PostDraw_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count - 2; i++)
			{
				if (list[i].opcode == OpCodes.Callvirt && list[i].operand is MethodBase mi && mi.Name == "get_DrawPos"
					&& list[i + 1].opcode == OpCodes.Ldarg_0
					&& list[i + 2].opcode == OpCodes.Ldfld)
				{
					list[i].opcode = OpCodes.Call;
					list[i].operand = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Modify_BiosculpterPod_PawnDrawOffset));
					break;
				}
			}
			return list;
		}

		static IEnumerable<CodeInstruction> CompBiosculpterPod_CompTick_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count - 2; i++)
			{
				if (list[i].opcode == OpCodes.Call && list[i].operand is MethodBase mi && mi.DeclaringType == typeof(TargetInfo) && mi.Name == "op_Implicit"
					&& list[i - 3].opcode == OpCodes.Ldfld
					&& list[i - 3].operand is FieldInfo fieldInfo && (fieldInfo.Name == "readyEffecter" || fieldInfo.Name == "operatingEffecter"))
				{
					list[i].opcode = OpCodes.Call;
					list[i].operand = AccessTools.Method(typeof(HarmonyPatches), nameof(HarmonyPatches.Modify_BiosculpterPod_TargetInfo));

					// changes multiple, so don't break!
				}
			}
			return list;
		}

		static IEnumerable<CodeInstruction> JobGiver_GetNeuralSupercharge_ClosestSupercharger_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator generator)
		{
			var pawnFieldInfo = default(FieldInfo);
			var validatorMethodInfo = default(MethodInfo);
			var applied = false;

			foreach (var instruction in codeInstructions)
			{
				// Find Pawn-FieldInfo (easiest way to find this!)
				if (instruction.opcode == OpCodes.Ldfld
					&& instruction.operand is FieldInfo fi
					&& fi.Name == "pawn")
					pawnFieldInfo = fi;
				// Find Validator-MethodInfo (very much the same here!)
				else if (instruction.opcode == OpCodes.Ldftn
					&& instruction.operand is MethodInfo mi
					&& mi.Name == "<ClosestSupercharger>g__Validator|0")
					validatorMethodInfo = mi;
				// Add new functionality at the end
				else if (instruction.opcode == OpCodes.Ret
					&& pawnFieldInfo != null
					&& validatorMethodInfo != null)
				{
					// create new label
					var label0 = generator.DefineLabel();

					//dup NULL
					yield return new CodeInstruction(OpCodes.Dup);
					//brtrue.s Label0
					yield return new CodeInstruction(OpCodes.Brtrue_S, label0);
					//pop NULL
					yield return new CodeInstruction(OpCodes.Pop);
					//ldloc.0 NULL
					yield return new CodeInstruction(OpCodes.Ldloc_0);
					//ldfld Verse.Pawn RimWorld.<>c__DisplayClass2_0::pawn
					yield return new CodeInstruction(OpCodes.Ldfld, pawnFieldInfo);
					//ldloc.0 NULL
					yield return new CodeInstruction(OpCodes.Ldloc_0);
					//ldftn System.Boolean RimWorld.<>c__DisplayClass2_0::<ClosestSupercharger2>g__Validator|0(Verse.Thing x)
					yield return new CodeInstruction(OpCodes.Ldftn, validatorMethodInfo);
					//newobj System.Void System.Predicate`1<Verse.Thing>::.ctor(System.Object object, System.IntPtr method)
					yield return new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(Predicate<Thing>), new Type[] { typeof(object), typeof(IntPtr) }));
					//call static Verse.Thing BPaNSVariations.HarmonyPatches::ClosestSuperChargerInternal(Verse.Pawn pawn, System.Predicate`1<Verse.Thing> validator)
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(ClosestSuperChargerInternal)));
					//ret NULL [Label0]
					instruction.labels.Add(label0);

					// notify that it's been applied
					applied = true;
				}
				yield return instruction;
			}

			// check if the patch has been applied
			if (!applied)
				Log.Error($"{nameof(BPaNSVariations)}: {nameof(JobGiver_GetNeuralSupercharge_ClosestSupercharger_Transpiler)} failed to apply patch (pawn: '{pawnFieldInfo}' validator: '{validatorMethodInfo}')");
		}
		#endregion

		#region PRIVATE METHODS
		private static Vector3 Modify_BiosculpterPod_PawnDrawOffset(ThingWithComps parent)
		{
			var rotation = parent.Rotation;
			var interactionCell = parent.InteractionCell.ToVector3();
			if (rotation == Rot4.South)
				return interactionCell + new Vector3(0.5f, 0, 2.0f);
			if (rotation == Rot4.West)
				return interactionCell + new Vector3(2.0f, 0, 0.35f);
			if (rotation == Rot4.North)
				return interactionCell + new Vector3(0.5f, 0, -0.9f);
			if (rotation == Rot4.East)
				return interactionCell + new Vector3(-1.0f, 0, 0.35f);
			return parent.DrawPos;
		}

		private static TargetInfo Modify_BiosculpterPod_TargetInfo(ThingWithComps parent)
		{
			var rot = parent.Rotation;
			if (rot == Rot4.South)
				return new TargetInfo(parent.InteractionCell + new IntVec3(0, 0, 2), parent.Map);
			if (rot == Rot4.West)
				return new TargetInfo(parent.InteractionCell + new IntVec3(2, 0, 0), parent.Map);
			if (rot == Rot4.North)
				return new TargetInfo(parent.InteractionCell + new IntVec3(0, 0, -2), parent.Map);
			if (rot == Rot4.East)
				return new TargetInfo(parent.InteractionCell + new IntVec3(-2, 0, 0), parent.Map);
			return parent;
		}

		private static Thing ClosestSuperChargerInternal(Pawn pawn, Predicate<Thing> validator) => 
			GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(BPaNSStatics.NeuralSupercharger_1x2_Center), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, validator);
		#endregion
	}
}
