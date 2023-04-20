using BPaNSVariations.Settings;
using BPaNSVariations.Utility;
using MonoMod.Utils;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BPaNSVariations.Controls
{
	internal class BPaNSControls
	{
		#region PROPERTIES
		public BPaNSSettings Settings { get; }

		public List<SelectableType> SelectableTypes { get; }

		public List<BaseControls> AllControls { get; }
		public List<BiosculpterPodControls> BiosculpterPodControls { get; }
		public List<NeuralSuperchargerControls> NeuralSuperchargerControls { get; }
		public List<SleepAcceleratorControls> SleepAcceleratorControls { get; }
		#endregion

		#region FIELDS
		private float _settingsViewHeight = 0;
		private Vector2 _settingsScrollPosition;

		public SelectableType _selectedType;
		public BaseControls _selectedControls;
		#endregion

		#region CONSTRUCTORS
		public BPaNSControls(BPaNSSettings settings)
		{
			Settings = settings ?? throw new ArgumentNullException(nameof(settings));

			BiosculpterPodControls = Settings.BiosculpterPodSettings.Select(s => new BiosculpterPodControls(s)).ToList();
			NeuralSuperchargerControls = Settings.NeuralSuperchargerSettings.Select(s => new NeuralSuperchargerControls(s)).ToList();
			SleepAcceleratorControls = Settings.SleepAcceleratorSettings.Select(s => new SleepAcceleratorControls(s)).ToList();

			SelectableTypes = new List<SelectableType>
			{
				new SelectableType("SY_BNV.BiosculpterPod".Translate(), Utility.SelectableTypes.BiosculpterPod, () => BiosculpterPodControls.Any(c => c.IsModified)),
				new SelectableType("SY_BNV.NeuralSupercharger".Translate(), Utility.SelectableTypes.NeuralSupercharger, () => NeuralSuperchargerControls.Any(c => c.IsModified)),
				new SelectableType("SY_BNV.SleepAccelerator".Translate(), Utility.SelectableTypes.SleepAccelerator, () => SleepAcceleratorControls.Any(c => c.IsModified))
			};

			AllControls = new List<BaseControls>();
			AllControls.AddRange(BiosculpterPodControls);
			AllControls.AddRange(NeuralSuperchargerControls);
			AllControls.AddRange(SleepAcceleratorControls);
		}
		#endregion

		#region PUBLIC METHODS
		public void DoSettingsWindowContents(Rect inRect)
		{
			// Save original settings
			BaseControls.OriTextFont = Text.Font;
			BaseControls.OriTextAnchor = Text.Anchor;
			BaseControls.OriColor = GUI.color;

			var width = inRect.width;
			var height = inRect.height;
			var viewWidth = width - 16;
			var offsetY = 0f;
			var selectorRowHeight = 0f;

			try
			{
				// Begin Group
				GUI.BeginGroup(inRect);
				Text.Anchor = TextAnchor.MiddleLeft;

				// Type Selection
				CreateSelector(ref offsetY, viewWidth, SelectableTypes, ref _selectedType);

				// Building Selection
				var prevSelected = _selectedControls;
				switch (_selectedType.Type)
				{
					case Utility.SelectableTypes.BiosculpterPod:
						CreateSelector(ref offsetY, viewWidth, BiosculpterPodControls, ref _selectedControls);
						break;
					case Utility.SelectableTypes.NeuralSupercharger:
						CreateSelector(ref offsetY, viewWidth, NeuralSuperchargerControls, ref _selectedControls);
						break;
					case Utility.SelectableTypes.SleepAccelerator:
						CreateSelector(ref offsetY, viewWidth, SleepAcceleratorControls, ref _selectedControls);
						break;
				}
				if (_selectedControls != prevSelected)
					_selectedControls.ResetBuffers();

				// Divider
				offsetY += 4;
				GUI.color = Widgets.SeparatorLineColor;
				Widgets.DrawLineHorizontal(0, offsetY, width);
				GUI.color = BaseControls.OriColor;
				offsetY += 8;
				selectorRowHeight = offsetY;

				// Begin ScrollView
				Widgets.BeginScrollView(
					new Rect(0, offsetY, width, height - selectorRowHeight),
					ref _settingsScrollPosition,
					new Rect(0, offsetY, viewWidth, _settingsViewHeight));

				// Settings
				_selectedControls.CreateSettings(
					ref offsetY, 
					viewWidth,
					out bool copy);

				// Copy settings
				if (copy)
				{
					void copyFunc(BaseControls controls)
					{
						if (_selectedControls != controls)
							_selectedControls.Settings.CopyTo(controls.Settings);
						controls.ResetBuffers();
					}
					switch (_selectedType.Type)
					{
						case Utility.SelectableTypes.BiosculpterPod:
							BiosculpterPodControls.ForEach(copyFunc);
							break;
						case Utility.SelectableTypes.NeuralSupercharger:
							NeuralSuperchargerControls.ForEach(copyFunc);
							break;
						case Utility.SelectableTypes.SleepAccelerator:
							SleepAcceleratorControls.ForEach(copyFunc);
							break;
					}
				}
			}
			finally
			{
				// Remember settings view height for potential scrolling
				_settingsViewHeight = offsetY - selectorRowHeight;

				// End ScrollView and Group
				Widgets.EndScrollView();
				GUI.EndGroup();

				// Reset text settings
				Text.Font = BaseControls.OriTextFont;
				Text.Anchor = BaseControls.OriTextAnchor;
				GUI.color = BaseControls.OriColor;
			}
		}
		#endregion

		#region PRIVATE METHODS
		private void CreateSelector<T>(ref float offsetY, float viewWidth, IEnumerable<T> values, ref T selected)
			where T : ISelectableItem
		{
			var count = values.Count();

			const float margin = 2f;
			var width = viewWidth / count;
			var height = BaseControls.ThinSettingsRowHeight;

			var i = 0;
			foreach (var value in values)
			{
				var rect = new Rect(margin + width * i, offsetY, width - margin * 2f, height);

				// Colorize if selected
				if (value.Equals(selected))
					GUI.color = BaseControls.SelectionColor;
				// Colorize if modified
				else if (value.IsModified)
					GUI.color = BaseControls.ModifiedColor;
				// Draw button
				if (Widgets.ButtonText(rect, value.Label))
					selected = value;
				// Reset color
				GUI.color = Color.white;

				i++;
			}

			if (!values.Contains(selected))
				selected = values.First();

			offsetY += height + 2;
		}
		#endregion
	}
}
