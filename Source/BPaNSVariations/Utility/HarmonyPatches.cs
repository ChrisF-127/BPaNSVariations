using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using BPaNSVariations.Settings;
using static Verse.ThreadLocalDeepProfiler;

namespace BPaNSVariations.Utility
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
			// Reinitialize ThingListGroupHelper.AllGroups with addition groups for Biosculpter Pod and Neural Supercharger
			// -- Must be executed before adding ThingRequestGroups!
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


			// -- BIOSCULPTER POD PATCHES

			// Adjust drawing position for pawn in Biosculpter Pod for new dimensions
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.PostDraw)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompBiosculpterPod_PostDraw_Transpiler)));
			// Adjust Biosculpter Pod effects to fit new dimensions
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.CompTick)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompBiosculpterPod_CompTick_Transpiler)));
			// Following patches are required to patch "Required Nutrition" for Biosculpter Pods
			harmony.Patch(
				AccessTools.PropertyGetter(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.RequiredNutritionRemaining)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompBiosculpterPod_RequiredNutrition_Transpiler)));
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.CompInspectStringExtra)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompBiosculpterPod_RequiredNutrition_Transpiler)));
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.LiquifyNutrition)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompBiosculpterPod_RequiredNutrition_Transpiler)));
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.CompGetGizmosExtra)),
				postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompBiosculpterPod_CompGetGizmosExtra_Postfix)));
			// Apply Biotuned Cycle Speed Factor patch
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.SetBiotuned)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompBiosculpterPod_SetBiotuned_Transpiler)));

			// Apply Age Reversal Cycle patch
			harmony.Patch(
				AccessTools.Method(typeof(CompBiosculpterPod_AgeReversalCycle), nameof(CompBiosculpterPod_AgeReversalCycle.CycleCompleted)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CompBiosculpterPod_AgeReversalCycle_CycleCompleted_Transpiler)));

			// Patch in new group for Biosculpter Pod into ThingListGroupHelper.Includes method and prevent it from throwing exception for unused groups
			harmony.Patch(
				AccessTools.Method(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(ThingListGroupHelper_Includes_Transpiler)));
			// Patch in new group for Biosculpter Pod into ThingListGroupHelper.StoreInRegion method and prevent it from throwing exception for unused groups
			harmony.Patch(
				AccessTools.Method(typeof(ThingRequestGroupUtility), nameof(ThingRequestGroupUtility.StoreInRegion)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(ThingRequestGroupUtility_StoreInRegion_Transpiler)));

			// Replace WorkGiver_HaulToBiosculpterPod.PotentialWorkThingRequest's singleDef with a group instead
			harmony.Patch(
				AccessTools.PropertyGetter(typeof(WorkGiver_HaulToBiosculpterPod), nameof(WorkGiver_HaulToBiosculpterPod.PotentialWorkThingRequest)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(WorkGiver_HaulToBiosculpterPod_PotentialWorkThingRequest_Transpiler)));


			// -- NEURAL SUPERCHARGER PATCHES

			// Patch JobGiver_GetNeuralSupercharge.ClosestSupercharger to search for a group for neural superchargers instead of a singleDef
			harmony.Patch(
				AccessTools.Method(typeof(JobGiver_GetNeuralSupercharge), nameof(JobGiver_GetNeuralSupercharge.ClosestSupercharger)),
				transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobGiver_GetNeuralSupercharge_ClosestSupercharger_Transpiler)));
		}
		#endregion

		#region HARMONY PATCHES
		static IEnumerable<CodeInstruction> CompBiosculpterPod_PostDraw_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patched = false;
			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count - 2; i++)
			{
				// replace Pawn.DrawPos with modifying method
				if (list[i].opcode == OpCodes.Callvirt && list[i].operand is MethodBase mi && mi.Name == "get_DrawPos"
					&& list[i + 1].opcode == OpCodes.Ldarg_0
					&& list[i + 2].opcode == OpCodes.Ldfld)
				{
					list[i].opcode = OpCodes.Call;
					list[i].operand = AccessTools.Method(typeof(HarmonyPatches), nameof(Modify_BiosculpterPod_PawnDrawOffset));

					patched = true;
					break;
				}
			}
			if (!patched)
				Log.Error($"{nameof(BPaNSVariations)} failed to apply {nameof(CompBiosculpterPod_PostDraw_Transpiler)}");
			return list;
		}
		static IEnumerable<CodeInstruction> CompBiosculpterPod_CompTick_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patchedTargetInfo = 0;
			var patchedBiotuning = false;
			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count - 2; i++)
			{
				// replace implicit TargetInfo creation for ThingWithComps for readyEffect & operatingEffect with modifying method
				if (list[i].opcode == OpCodes.Call && list[i].operand is MethodBase mb && mb.DeclaringType == typeof(TargetInfo) && mb.Name == "op_Implicit"
					&& list[i - 3].opcode == OpCodes.Ldfld
					&& list[i - 3].operand is FieldInfo fieldInfo && (fieldInfo.Name == "readyEffecter" || fieldInfo.Name == "operatingEffecter"))
				{
					list[i].opcode = OpCodes.Call;
					list[i].operand = AccessTools.Method(typeof(HarmonyPatches), nameof(Modify_BiosculpterPod_TargetInfo));

					// patches multiple, so no break
					patchedTargetInfo++;
				}

				// -- ldc.i4 4800000
				// ++ call static System.Single BPaNSVariations.BiosculpterPodSettings::get_BPBiotunedDuration()
				//  1 stfld System.Int32 RimWorld.CompBiosculpterPod::biotunedCountdownTicks
				if (!patchedBiotuning
					&& list[i + 0].opcode == OpCodes.Ldc_I4
					&& list[i + 1].opcode == OpCodes.Stfld
					&& list[i + 1].operand is FieldInfo fi
					&& fi.DeclaringType == typeof(CompBiosculpterPod)
					&& fi.Name == nameof(CompBiosculpterPod.biotunedCountdownTicks))
				{
					list[i] = new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(BiosculpterPodSettings), nameof(BiosculpterPodSettings.BiotunedDuration)));

					patchedBiotuning = true;
				}
			}
			if (patchedTargetInfo != 4)
				Log.Error($"{nameof(BPaNSVariations)} failed to apply {nameof(CompBiosculpterPod_CompTick_Transpiler)}");
			return list;
		}
		static IEnumerable<CodeInstruction> CompBiosculpterPod_RequiredNutrition_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// Hopefully the value of "CompBiosculpterPod.NutritionRequired" is never used anywhere else in the patched methods, otherwise it'll cause collateral!
			var patched = false;
			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count; i++)
			{
				// -- ldc.r4 5
				// ++ call static System.Single BPaNSVariations.BiosculpterPodSettings::get_BPNutritionRequired()
				if (list[i].opcode == OpCodes.Ldc_R4
					&& list[i].operand is float value 
					&& value == CompBiosculpterPod.NutritionRequired)
				{
					list[i] = new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(BiosculpterPodSettings), nameof(BiosculpterPodSettings.NutritionRequired)));

					// potentially patches multiple, so no break
					patched = true;
				}
			}
			if (!patched)
				Log.Error($"{nameof(BPaNSVariations)} failed to apply {nameof(CompBiosculpterPod_RequiredNutrition_Transpiler)}");
			return list;
		}
		static IEnumerable<Gizmo> CompBiosculpterPod_CompGetGizmosExtra_Postfix(IEnumerable<Gizmo> __result, CompBiosculpterPod __instance)
		{
			const string label = "DEV: fill nutrition and cycle ingredients";
			foreach (var item in __result)
			{
				if (item is Command_Action commandAction
					&& commandAction.defaultLabel == label)
				{
					yield return new Command_Action
					{
						defaultLabel = label,
						action = delegate
						{
							__instance.liquifiedNutrition = BiosculpterPodSettings.NutritionRequired;
							__instance.devFillPodLatch = true;
						},
						disabled = __instance.State == BiosculpterPodState.Occupied || (__instance.devFillPodLatch && __instance.liquifiedNutrition == BiosculpterPodSettings.NutritionRequired)
					};
				}
				else
					yield return item;
			}	
		}
		static IEnumerable<CodeInstruction> CompBiosculpterPod_SetBiotuned_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patched = false;
			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count; i++)
			{
				// -- ldc.i4 4800000
				// ++ call static System.Single BPaNSVariations.BiosculpterPodSettings::get_BPBiotunedDuration()
				//  1 stfld System.Int32 RimWorld.CompBiosculpterPod::biotunedCountdownTicks
				if (list[i + 0].opcode == OpCodes.Ldc_I4
					&& list[i + 1].opcode == OpCodes.Stfld
					&& list[i + 1].operand is FieldInfo fi
					&& fi.DeclaringType == typeof(CompBiosculpterPod)
					&& fi.Name == nameof(CompBiosculpterPod.biotunedCountdownTicks))
				{
					list[i] = new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(BiosculpterPodSettings), nameof(BiosculpterPodSettings.BiotunedDuration)));

					patched = true;
					break;
				}
			}
			if (!patched)
				Log.Error($"{nameof(BPaNSVariations)} failed to apply {nameof(CompBiosculpterPod_SetBiotuned_Transpiler)}");
			return list;
		}

		static IEnumerable<CodeInstruction> CompBiosculpterPod_AgeReversalCycle_CycleCompleted_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patched = false;
			var list = new List<CodeInstruction>(instructions);
			for (int i = 0; i < list.Count - 3; i++)
			{
				// ++ call static System.Single BPaNSVariations.BPaNSSettings::get_BPAgeReversalAgeReversed()
				// ++ mul NULL
				//  0 conv.i4 NULL
				//  1 stloc.0 NULL
				//  2 ldc.r4 3600000
				//  3 ldarg.1 NULL
				if (list[i + 0].opcode == OpCodes.Conv_I4
					&& list[i + 1].opcode == OpCodes.Stloc_0
					&& list[i + 2].opcode == OpCodes.Ldc_R4
					&& list[i + 3].opcode == OpCodes.Ldarg_1)
				{
					list.Insert(i++, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(BiosculpterPodSettings), nameof(BiosculpterPodSettings.AgeReversalCycleAgeReversed))));
					list.Insert(i++, new CodeInstruction(OpCodes.Mul));

					patched = true;
					break;
				}
			}
			if (!patched)
				Log.Error($"{nameof(BPaNSVariations)} failed to apply {nameof(CompBiosculpterPod_AgeReversalCycle_CycleCompleted_Transpiler)}");
			return list;
		}


		static IEnumerable<CodeInstruction> ThingListGroupHelper_Includes_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var patched0 = false;
			var patched1 = false;
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

					patched0 = true;
				}
				// BEFORE ldstr "group" [Label70]
				else if (list[i].opcode == OpCodes.Ldstr && list[i].operand is "group")
				{
					//ldarg.1 NULL [Label68]
					list.Insert(i++, new CodeInstruction(OpCodes.Ldarg_1) { labels = new List<Label> { label68 } });
					//call static System.Boolean BPaNSVariations.BPaNSStatics::IsDefBiosculpterPod(Verse.ThingDef def)
					list.Insert(i++, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPaNSUtility), nameof(BPaNSUtility.IsDefBiosculpterPod))));
					//ret NULL
					list.Insert(i++, new CodeInstruction(OpCodes.Ret));

					//ldarg.1 NULL [Label69]
					list.Insert(i++, new CodeInstruction(OpCodes.Ldarg_1) { labels = new List<Label> { label69 } });
					//call static System.Boolean BPaNSVariations.BPaNSStatics::IsDefNeuralSupercharger(Verse.ThingDef def)
					list.Insert(i++, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPaNSUtility), nameof(BPaNSUtility.IsDefNeuralSupercharger))));
					//ret NULL
					list.Insert(i++, new CodeInstruction(OpCodes.Ret));

					patched1 = true;
				}
			}
			if (!patched0 || !patched1)
				Log.Error($"{nameof(BPaNSVariations)} failed to apply {nameof(ThingListGroupHelper_Includes_Transpiler)}");
			return list;
		}
		static IEnumerable<CodeInstruction> ThingRequestGroupUtility_StoreInRegion_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var patched0 = false;
			var patched1 = false;
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

					patched0 = true;
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

					patched1 = true;
				}
			}
			if (!patched0 || !patched1)
				Log.Error($"{nameof(BPaNSVariations)} failed to apply {nameof(ThingListGroupHelper_Includes_Transpiler)}");
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
