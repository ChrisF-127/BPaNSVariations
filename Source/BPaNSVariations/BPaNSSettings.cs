using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BPaNSVariations
{
	public enum BiosculpterPodEffecterState
	{
		Default,
		AlwaysOn,
		AlwaysOff,
	}

	internal class BPaNSSettings : ModSettings
	{
		#region PROPERTIES
		public BiosculpterPodEffecterState DefaultBPReadyEffecterState { get; private set; }
		private BiosculpterPodEffecterState _bpReadyEffecterState;
		public BiosculpterPodEffecterState BPReadyEffecterState 
		{
			get => _bpReadyEffecterState;
			set => SetBPReadyEffecterState(value);
		}

		public Color DefaultBPReadyEffecterColor { get; private set; }
		private Color _bpReadyEffecterColor;
		public Color BPReadyEffecterColor
		{
			get => _bpReadyEffecterColor;
			set => SetBPReadyEffecterColor(value);
		}
		#endregion

		#region CONSTRUCTORS
		public BPaNSSettings()
		{
			_bpReadyEffecterState = DefaultBPReadyEffecterState = BiosculpterPodEffecterState.Default;
			_bpReadyEffecterColor = DefaultBPReadyEffecterColor = ThingDefOf.BiosculpterPod.GetCompProperties<CompProperties_BiosculpterPod>().selectCycleColor;
		}
		#endregion

		#region PRIVATE METHODS
		private void SetBPReadyEffecterState(BiosculpterPodEffecterState state, bool force = false)
		{
			if (!force && _bpReadyEffecterState == state)
				return;
			_bpReadyEffecterState = state;

			if (state is BiosculpterPodEffecterState.AlwaysOn)
			{
				BPaNSUtility.BiosculpterScanner_Ready.fadeInTime = 0f;
				BPaNSUtility.BiosculpterScanner_Ready.fadeOutTime = 0f;
				// new motes are generated AT and not AFTER "ticksBetweenMotes" (e.g. a value of 1 generates a mote on every tick)
				// 	this causes the time to be 1 tick shorter than expected (179 instead of 180 ticks)!
				BPaNSUtility.BiosculpterScanner_Ready.solidTime = (BPaNSUtility.BiosculpterPod_Ready.children[0].ticksBetweenMotes - 1) / 60f;
			}
			else
			{
				BPaNSUtility.BiosculpterScanner_Ready.fadeInTime = BPaNSUtility.OriginalBiosculpterScanner_ReadyValues.Item1;
				BPaNSUtility.BiosculpterScanner_Ready.fadeOutTime = BPaNSUtility.OriginalBiosculpterScanner_ReadyValues.Item2;
				BPaNSUtility.BiosculpterScanner_Ready.solidTime = BPaNSUtility.OriginalBiosculpterScanner_ReadyValues.Item3;
			}

			if (state is BiosculpterPodEffecterState.AlwaysOff)
			{
				foreach (var prop in BPaNSUtility.GetAll_CompProperties_BiosculpterPod())
					prop.readyEffecter = null;
			}
			else
			{
				foreach (var prop in BPaNSUtility.GetAll_CompProperties_BiosculpterPod())
					prop.readyEffecter = BPaNSUtility.BiosculpterPod_Ready;
			}
		}

		private void SetBPReadyEffecterColor(Color color, bool force = false)
		{
			if (!force && _bpReadyEffecterColor == color)
				return;
			_bpReadyEffecterColor = color;

			foreach (var prop in BPaNSUtility.GetAll_CompProperties_BiosculpterPod())
				prop.selectCycleColor = color;
		}
		#endregion

		#region OVERRIDES
		public override void ExposeData()
		{
			base.ExposeData();

			var v0 = BPReadyEffecterState;
			Scribe_Values.Look(ref v0, nameof(BPReadyEffecterState), DefaultBPReadyEffecterState);
			BPReadyEffecterState = v0;

			var v1 = BPReadyEffecterColor;
			Scribe_Values.Look(ref v1, nameof(BPReadyEffecterColor), DefaultBPReadyEffecterColor);
			BPReadyEffecterColor = v1;
		}
		#endregion
	}
}
