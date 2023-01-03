using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using Verse.AI;
using System;

namespace BPaNSVariations
{
	[StaticConstructorOnStartup]
	public static class HarmonyPatches
	{
		#region PRIVATE FIELDS
		// ThingRequestGroup id for Biosculpter Pods
		private static readonly int ThingRequestGroup_BiosculpterPod;
		// ThingRequestGroup id for Neural Superchargers
		private static readonly int ThingRequestGroup_NeuralSupercharger;
		#endregion

		#region CONSTRUCTORS
		static HarmonyPatches()
		{
			// Reinitialize ThingListGroupHelper.AllGroupsroups -- must be executed before adding ThingRequestGroups!
			{
				// create new array for groups with increased size
				var count = ThingListGroupHelper.AllGroups.Length;
				var array = new ThingRequestGroup[count + 2];

				// copy existing groups
				ThingListGroupHelper.AllGroups.CopyTo(array, 0);

				// add group for Biosculpter Pod
				ThingRequestGroup_BiosculpterPod = count;
				array[count] = (ThingRequestGroup)count;

				// add group for Neural Supercharger
				count++;
				ThingRequestGroup_NeuralSupercharger = count;
				array[count] = (ThingRequestGroup)count;

				// replace AllGroups with new array
				AccessTools.Field(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.AllGroups)).SetValue(null, array);
			}


			// Create Harmony
			var harmony = new Harmony("syrus.bpansvariations");

			// Adjust drawing position for pawn in Biosculpter Pod for new dimensions
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.PostDraw)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.CompBiosculpterPod_PostDraw_Transpiler)));
			// Adjust Biosculpter Pod effects to fit new dimensions
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.CompTick)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.CompBiosculpterPod_CompTick_Transpiler)));

			// Patch in new group for Biosculpter Pod into ThingListGroupHelper.Includes method and prevent it from throwing exception for unused groups
			harmony.Patch(
				AccessTools.Method(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.ThingListGroupHelper_Includes_Transpiler)));
			// Patch in new group for Biosculpter Pod into ThingListGroupHelper.StoreInRegion method and prevent it from throwing exception for unused groups
			harmony.Patch(
				AccessTools.Method(typeof(ThingRequestGroupUtility), nameof(ThingRequestGroupUtility.StoreInRegion)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.ThingRequestGroupUtility_StoreInRegion_Transpiler)));

			// Replace WorkGiver_HaulToBiosculpterPod.PotentialWorkThingRequest's singleDef with a group instead
			harmony.Patch(
				AccessTools.PropertyGetter(typeof(WorkGiver_HaulToBiosculpterPod), nameof(WorkGiver_HaulToBiosculpterPod.PotentialWorkThingRequest)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.WorkGiver_HaulToBiosculpterPod_PotentialWorkThingRequest_Transpiler)));

			// Patch JobGiver_GetNeuralSupercharge.ClosestSupercharger to search for a group for neural superchargers instead of a singleDef
			harmony.Patch(
				AccessTools.Method(typeof(JobGiver_GetNeuralSupercharge), nameof(JobGiver_GetNeuralSupercharge.ClosestSupercharger)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.JobGiver_GetNeuralSupercharge_ClosestSupercharger_Transpiler)));
		}
		#endregion

		#region HARMONY PATCHES
		static IEnumerable<CodeInstruction> CompBiosculpterPod_PostDraw_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count - 2; i++)
			{
				// replace Pawn.DrawPos with modifying method
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
				// replace implicit TargetInfo creation for ThingWithComps for readyEffecter & operatingEffecter with modifying method
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


		static IEnumerable<CodeInstruction> ThingListGroupHelper_Includes_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var label68 = generator.DefineLabel();
			var label69 = generator.DefineLabel();

			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count; i++)
			{
				// AFTER switch Labels...
				if (list[i].opcode == OpCodes.Switch)
				{
					//ldarg.0 NULL
					list.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0));
					//ldc.i4 >>ThingRequestGroup_BiosculpterPod<<
					list.Insert(++i, new CodeInstruction(OpCodes.Ldc_I4, ThingRequestGroup_BiosculpterPod));
					//beq Label68
					list.Insert(++i, new CodeInstruction(OpCodes.Beq, label68));

					//ldarg.0 NULL
					list.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0));
					//ldc.i4 >>ThingRequestGroup_NeuralSupercharger<<
					list.Insert(++i, new CodeInstruction(OpCodes.Ldc_I4, ThingRequestGroup_NeuralSupercharger));
					//beq Label69
					list.Insert(++i, new CodeInstruction(OpCodes.Beq, label69));
				}
				// BEFORE ldstr "group" [Label70]
				else if (list[i].opcode == OpCodes.Ldstr && list[i].operand is "group")
				{
					//ldarg.1 NULL [Label68]
					list.Insert(i++, new CodeInstruction(OpCodes.Ldarg_1) { labels = new List<Label> { label68 } });
					//call static System.Boolean BPaNSVariations.BPaNSStatics::IsDefBiosculpterPod(Verse.ThingDef def)
					list.Insert(i++, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPaNSStatics), nameof(BPaNSStatics.IsDefBiosculpterPod))));
					//ret NULL
					list.Insert(i++, new CodeInstruction(OpCodes.Ret));

					//ldarg.1 NULL [Label69]
					list.Insert(i++, new CodeInstruction(OpCodes.Ldarg_1) { labels = new List<Label> { label69 } });
					//call static System.Boolean BPaNSVariations.BPaNSStatics::IsDefNeuralSupercharger(Verse.ThingDef def)
					list.Insert(i++, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPaNSStatics), nameof(BPaNSStatics.IsDefNeuralSupercharger))));
					//ret NULL
					list.Insert(i++, new CodeInstruction(OpCodes.Ret));
				}
			}
			return list;
		}

		static IEnumerable<CodeInstruction> ThingRequestGroupUtility_StoreInRegion_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var label68 = generator.DefineLabel();
			var label69 = generator.DefineLabel();

			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count; i++)
			{
				// AFTER switch Labels...
				if (list[i].opcode == OpCodes.Switch)
				{
					//ldarg.0 NULL
					list.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0));
					//ldc.i4 >>ThingRequestGroup_BiosculpterPod<<
					list.Insert(++i, new CodeInstruction(OpCodes.Ldc_I4, ThingRequestGroup_BiosculpterPod));
					//beq Label68
					list.Insert(++i, new CodeInstruction(OpCodes.Beq, label68));

					//ldarg.0 NULL
					list.Insert(++i, new CodeInstruction(OpCodes.Ldarg_0));
					//ldc.i4 >>ThingRequestGroup_NeuralSupercharger<<
					list.Insert(++i, new CodeInstruction(OpCodes.Ldc_I4, ThingRequestGroup_NeuralSupercharger));
					//beq Label69
					list.Insert(++i, new CodeInstruction(OpCodes.Beq, label69));
				}
				// BEFORE ldstr "group" [Label70]
				else if (list[i].opcode == OpCodes.Ldstr && list[i].operand is "group")
				{
					//ldc.i4.1 NULL [Label68]
					list.Insert(i++, new CodeInstruction(OpCodes.Ldc_I4_1) { labels = new List<Label> { label68 } });
					//ret NULL
					list.Insert(i++, new CodeInstruction(OpCodes.Ret));

					//ldc.i4.1 NULL [Label69]
					list.Insert(i++, new CodeInstruction(OpCodes.Ldc_I4_1) { labels = new List<Label> { label69 } });
					//ret NULL
					list.Insert(i++, new CodeInstruction(OpCodes.Ret));
				}
			}
			return list;
		}


		static IEnumerable<CodeInstruction> WorkGiver_HaulToBiosculpterPod_PotentialWorkThingRequest_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//ldc.i4 >>ThingRequestGroup_BiosculpterPod<<
			yield return new CodeInstruction(OpCodes.Ldc_I4, ThingRequestGroup_BiosculpterPod);
			//call static Verse.ThingRequest Verse.ThingRequest::ForGroup(Verse.ThingRequestGroup group)
			yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ThingRequest), nameof(ThingRequest.ForGroup)));
			//ret NULL
			yield return new CodeInstruction(OpCodes.Ret);
		}

		static IEnumerable<CodeInstruction> JobGiver_GetNeuralSupercharge_ClosestSupercharger_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			// Original replacement using a field:
			// ++ ldsfld System.Int32 BPaNSVariations.HarmonyPatches::ThingRequestGroup_NeuralSupercharger
			// ++ conv.u1 NULL
			// ++ call static Verse.ThingRequest Verse.ThingRequest::ForGroup(Verse.ThingRequestGroup group)
			// But this should work as well as ThingRequestGroup_NeuralSupercharger can be treated as a constant value:
			// ++ ldc.i4 >>ThingRequestGroup_NeuralSupercharger<<
			// ++ call static Verse.ThingRequest Verse.ThingRequest::ForGroup(Verse.ThingRequestGroup group)

			foreach (var instruction in instructions)
			{
				// -- ldsfld Verse.ThingDef RimWorld.ThingDefOf::NeuralSupercharger
				if (instruction.opcode == OpCodes.Ldsfld
					&& instruction.operand is FieldInfo fi
					&& fi.DeclaringType == typeof(ThingDefOf)
					&& fi.Name == "NeuralSupercharger")
				{
					// ++ ldc.i4 >>ThingRequestGroup_NeuralSupercharger<<
					instruction.opcode = OpCodes.Ldc_I4;
					instruction.operand = ThingRequestGroup_NeuralSupercharger;
				}
				// -- call static Verse.ThingRequest Verse.ThingRequest::ForDef(Verse.ThingDef singleDef)
				else if (instruction.opcode == OpCodes.Call
					&& instruction.operand is MethodInfo mi
					&& mi.DeclaringType == typeof(ThingRequest)
					&& mi.Name == "ForDef")
				{
					// ++ call static Verse.ThingRequest Verse.ThingRequest::ForGroup(Verse.ThingRequestGroup group)
					instruction.operand = AccessTools.Method(typeof(ThingRequest), nameof(ThingRequest.ForGroup));
				}
				yield return instruction;
			}
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
		#endregion
	}
}
