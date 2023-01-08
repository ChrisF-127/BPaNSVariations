using HugsLib.Settings;
using System;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace BPaNSVariations
{
	public class BPaNSVariations : HugsLib.ModBase
	{
		private enum ColorSelector
		{
			R, G, B
		}

		#region FIELDS
		private SettingHandle<bool> _biosculpterPodReadyEffecterAlwaysOn;
		private SettingHandle<bool> _biosculpterPodReadyEffecterAlwaysOff;
		private SettingHandle<float> _biosculpterPodReadyEffecterColorR;
		private SettingHandle<float> _biosculpterPodReadyEffecterColorG;
		private SettingHandle<float> _biosculpterPodReadyEffecterColorB;
		#endregion

		#region OVERRIDES
		public override string ModIdentifier => 
			"BiosculpterPodAndNeuralSuperchargerVariations";

		public override void DefsLoaded()
		{
			// Create settings
			_biosculpterPodReadyEffecterAlwaysOn = Settings.GetHandle(
				"biosculpterPodReadyEffecterAlwaysOn",
				"SY_BNV.BiosculpterPodReadyEffecterAlwaysOnTitle".Translate(),
				"SY_BNV.BiosculpterPodReadyEffecterAlwaysOnDesc".Translate(),
				true);
			_biosculpterPodReadyEffecterAlwaysOn.ValueChanged += value => ChangeBiosculpterPodReadyEffecterAlwaysOn((SettingHandle<bool>)value);
			_biosculpterPodReadyEffecterAlwaysOff = Settings.GetHandle(
				"biosculpterPodReadyEffecterAlwaysOff",
				"SY_BNV.BiosculpterPodReadyEffecterAlwaysOffTitle".Translate(),
				"SY_BNV.BiosculpterPodReadyEffecterAlwaysOffDesc".Translate(),
				true);
			_biosculpterPodReadyEffecterAlwaysOff.ValueChanged += value => ChangeBiosculpterPodReadyEffecterAlwaysOff((SettingHandle<bool>)value);

			var biosculpterPodComp = ThingDefOf.BiosculpterPod.GetCompProperties<CompProperties_BiosculpterPod>();
			_biosculpterPodReadyEffecterColorR = Settings.GetHandle(
				"biosculpterPodReadyEffecterColorR",
				"SY_BNV.BiosculpterPodReadyEffecterColorRTitle".Translate(),
				"SY_BNV.BiosculpterPodReadyEffecterColorRDesc".Translate(),
				biosculpterPodComp.selectCycleColor.r);
			_biosculpterPodReadyEffecterColorR.ValueChanged += value => ChangeBiosculpterPodReadyEffecterColor(ColorSelector.R, (SettingHandle<float>)value);
			_biosculpterPodReadyEffecterColorG = Settings.GetHandle(
				"biosculpterPodReadyEffecterColorG",
				"SY_BNV.BiosculpterPodReadyEffecterColorGTitle".Translate(),
				"SY_BNV.BiosculpterPodReadyEffecterColorGDesc".Translate(),
				biosculpterPodComp.selectCycleColor.g);
			_biosculpterPodReadyEffecterColorG.ValueChanged += value => ChangeBiosculpterPodReadyEffecterColor(ColorSelector.G, (SettingHandle<float>)value);
			_biosculpterPodReadyEffecterColorB = Settings.GetHandle(
				"biosculpterPodReadyEffecterColorB",
				"SY_BNV.BiosculpterPodReadyEffecterColorBTitle".Translate(),
				"SY_BNV.BiosculpterPodReadyEffecterColorBDesc".Translate(),
				biosculpterPodComp.selectCycleColor.b);
			_biosculpterPodReadyEffecterColorB.ValueChanged += value => ChangeBiosculpterPodReadyEffecterColor(ColorSelector.B, (SettingHandle<float>)value);


			// Apply settings
			ChangeBiosculpterPodReadyEffecterAlwaysOn(_biosculpterPodReadyEffecterAlwaysOn);
			ChangeBiosculpterPodReadyEffecterAlwaysOff(_biosculpterPodReadyEffecterAlwaysOff);

			ChangeBiosculpterPodReadyEffecterColor(ColorSelector.R, _biosculpterPodReadyEffecterColorR);
			ChangeBiosculpterPodReadyEffecterColor(ColorSelector.G, _biosculpterPodReadyEffecterColorG);
			ChangeBiosculpterPodReadyEffecterColor(ColorSelector.B, _biosculpterPodReadyEffecterColorB);
		}
		#endregion

		#region PRIVATE METHODS
		private void ChangeBiosculpterPodReadyEffecterAlwaysOn(bool on)
		{
			if (on && _biosculpterPodReadyEffecterAlwaysOff.Value)
				_biosculpterPodReadyEffecterAlwaysOff.Value = false;

			if (on)
			{
				BPaNSStatics.BiosculpterScanner_Ready.fadeInTime = 0f;
				BPaNSStatics.BiosculpterScanner_Ready.fadeOutTime = 0f;
				// new motes are generated AT and not AFTER "ticksBetweenMotes" (e.g. a value of 1 generates a mote on every tick)
				// 	this causes the time to be 1 tick shorter than expected (179 instead of 180 ticks)!
				BPaNSStatics.BiosculpterScanner_Ready.solidTime = (BPaNSStatics.BiosculpterPod_Ready.children[0].ticksBetweenMotes - 1) / 60f;
			}
			else
			{
				BPaNSStatics.BiosculpterScanner_Ready.fadeInTime = BPaNSStatics.OriginalBiosculpterScanner_ReadyValues.Item1;
				BPaNSStatics.BiosculpterScanner_Ready.fadeOutTime = BPaNSStatics.OriginalBiosculpterScanner_ReadyValues.Item2;
				BPaNSStatics.BiosculpterScanner_Ready.solidTime = BPaNSStatics.OriginalBiosculpterScanner_ReadyValues.Item3;
			}
		}

		private void ChangeBiosculpterPodReadyEffecterAlwaysOff(bool off)
		{
			if (off && _biosculpterPodReadyEffecterAlwaysOn.Value)
				_biosculpterPodReadyEffecterAlwaysOn.Value = false;

			foreach (var prop in BPaNSStatics.GetAll_CompProperties_BiosculpterPod())
				prop.readyEffecter = !off ? BPaNSStatics.BiosculpterPod_Ready : null;
		}

		private void ChangeBiosculpterPodReadyEffecterColor(ColorSelector rgb, float value)
		{
			switch (rgb)
			{
				case ColorSelector.R:
					foreach (var prop in BPaNSStatics.GetAll_CompProperties_BiosculpterPod())
						prop.selectCycleColor.r = value;
					break;
				case ColorSelector.G:
					foreach (var prop in BPaNSStatics.GetAll_CompProperties_BiosculpterPod())
						prop.selectCycleColor.g = value;
					break;
				case ColorSelector.B:
					foreach (var prop in BPaNSStatics.GetAll_CompProperties_BiosculpterPod())
						prop.selectCycleColor.b = value;
					break;
			}
		}
		#endregion
	}
}
