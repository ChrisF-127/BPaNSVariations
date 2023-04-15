using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.UI.CanvasScaler;
using Verse.Noise;

namespace BPaNSVariations
{
	internal delegate T ConvertDelegate<T>(T value);

	internal class BPaNSControls
	{
		#region FIELDS
		public static float SettingsViewHeight = 0;
		public const float SettingsRowHeight = 32;

		public static GameFont OriTextFont;
		public static TextAnchor OriTextAnchor;
		public static Color OriColor;
		public static Color ModifiedColor = Color.cyan;
		public static Color SelectionColor = Color.green;

		private Vector2 _settingsScrollPosition;

		#region SETTINGS
		private TargetWrapper<BiosculpterPodEffecterState> _bpEffecterStateTargetWrapper = null;
		private readonly BiosculpterPodEffecterState[] _bpEffecterStates = (BiosculpterPodEffecterState[])Enum.GetValues(typeof(BiosculpterPodEffecterState));
		private readonly string[] _bpReadyEffecterColorBuffers = new string[3];
		#endregion
		#endregion

		#region PUBLIC METHODS
		public void CreateSettingsUI(Rect inRect, BPaNSSettings settings)
		{
			// Save original settings
			OriTextFont = Text.Font;
			OriTextAnchor = Text.Anchor;
			OriColor = GUI.color;

			var width = inRect.width;
			var height = inRect.height;
			var viewWidth = width - 16;
			var totalHeight = 0f;

			try
			{
				// Begin Group
				GUI.BeginGroup(inRect);
				Text.Anchor = TextAnchor.MiddleLeft;

				// Begin ScrollView
				Widgets.BeginScrollView(
					new Rect(0, 0, width, height),
					ref _settingsScrollPosition,
					new Rect(0, 0, viewWidth, SettingsViewHeight));

				// Biosculpter Pod - Ready Effecter State
				if (_bpEffecterStateTargetWrapper == null)
					_bpEffecterStateTargetWrapper = new TargetWrapper<BiosculpterPodEffecterState>(settings.BPReadyEffecterState);
				CreateDropdownSelectorControl(
					totalHeight,
					viewWidth,
					"SY_BNV.BPReadyEffecterState".Translate(),
					"SY_BNV.TooltipBPReadyEffecterState".Translate(),
					settings.BPReadyEffecterState != settings.DefaultBPReadyEffecterState,
					_bpEffecterStateTargetWrapper,
					settings.DefaultBPReadyEffecterState,
					_bpEffecterStates,
					state => BPReadyEffecterStateToString(state).Translate());
				totalHeight += SettingsRowHeight;
				settings.BPReadyEffecterState = _bpEffecterStateTargetWrapper.Item;

				// Biosculpter Pod - Ready Effecter Color
				var color = CreateColorControl(
					totalHeight,
					viewWidth,
					"SY_BNV.BPReadyEffecterColor".Translate(),
					"SY_BNV.TooltipBPReadyEffecterColor".Translate(),
					settings.BPReadyEffecterColor,
					settings.DefaultBPReadyEffecterColor,
					_bpReadyEffecterColorBuffers);
				totalHeight += SettingsRowHeight;
				settings.BPReadyEffecterColor = color;
			}
			finally
			{
				// Remember settings view height for potential scrolling
				SettingsViewHeight = totalHeight;

				// End ScrollView and Group
				Widgets.EndScrollView();
				GUI.EndGroup();

				// Reset text settings
				Text.Font = OriTextFont;
				Text.Anchor = OriTextAnchor;
				GUI.color = OriColor;
			}
		}


		#region CONTROLS
		public static float GetControlWidth(float viewWidth) =>
			viewWidth / 2 - SettingsRowHeight - 4;

		public static bool DrawResetButton(float offsetY, float viewWidth, string tooltip)
		{
			var buttonRect = new Rect(viewWidth + 2 - (SettingsRowHeight * 2), offsetY + 2, SettingsRowHeight * 2 - 2, SettingsRowHeight - 4);
			DrawTooltip(buttonRect, "SY_BNV.TooltipDefaultValue".Translate() + " " + tooltip);
			return Widgets.ButtonText(buttonRect, "SY_BNV.Reset".Translate());
		}
		public static void DrawTooltip(Rect rect, string tooltip)
		{
			if (Mouse.IsOver(rect))
			{
				ActiveTip activeTip = new ActiveTip(tooltip);
				activeTip.DrawTooltip(GenUI.GetMouseAttachedWindowPos(activeTip.TipRect.width, activeTip.TipRect.height) + (UI.MousePositionOnUIInverted - Event.current.mousePosition));
			}
		}
		public static void DrawTextFieldUnit<T>(Rect rect, T value, string unit)
		{
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(new Rect(rect.x + 4, rect.y + 1, rect.width - 8, rect.height), $"{value?.ToString() ?? ""} {unit ?? ""}");
			Text.Anchor = TextAnchor.MiddleLeft;
		}

		public static T CreateNumeric<T>(
			float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			bool isModified,
			T value,
			T defaultValue,
			ref string valueBuffer,
			float min = 0f,
			float max = 1e+9f,
			ConvertDelegate<T> convert = null,
			string unit = null)
			where T : struct
		{
			var controlWidth = GetControlWidth(viewWidth);

			// Label
			if (isModified)
				GUI.color = ModifiedColor;
			Widgets.Label(new Rect(0, offsetY, controlWidth - 8, SettingsRowHeight), label);
			GUI.color = OriColor;

			// Setting
			var textFieldRect = new Rect(controlWidth + 2, offsetY + 6, controlWidth - 4, SettingsRowHeight - 12);
			Widgets.TextFieldNumeric(textFieldRect, ref value, ref valueBuffer, min, max);
			if (!string.IsNullOrWhiteSpace(tooltip))
				DrawTooltip(textFieldRect, tooltip);

			// Unit
			DrawTextFieldUnit(textFieldRect, convert != null ? (T?)convert(value) : null, unit);

			// Reset button
			if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue.ToString()))
			{
				value = defaultValue;
				valueBuffer = null;
			}

			return value;
		}

		public static Color CreateColorControl(
			float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			Color value,
			Color defaultValue,
			string[] valueBuffers)
		{
			var controlWidth = GetControlWidth(viewWidth);
			var isModified = value != defaultValue;

			// Label
			if (isModified)
				GUI.color = ModifiedColor;
			Widgets.Label(new Rect(0, offsetY, controlWidth - 8, SettingsRowHeight), label);
			GUI.color = OriColor;

			// Red
			var x = controlWidth + 2;
			var w = (controlWidth / 3) - 4;
			var textFieldRect = new Rect(x, offsetY + 6, w, SettingsRowHeight - 12);
			Widgets.TextFieldNumeric(textFieldRect, ref value.r, ref valueBuffers[0], 0f, 1f);
			DrawTooltip(textFieldRect, $"{tooltip} {"SY_BNV.Red".Translate()}");
			DrawTextFieldUnit<float?>(textFieldRect, null, "R");

			// Green
			x += w + 4;
			textFieldRect = new Rect(x, offsetY + 6, w, SettingsRowHeight - 12);
			Widgets.TextFieldNumeric(textFieldRect, ref value.g, ref valueBuffers[1], 0f, 1f);
			DrawTooltip(textFieldRect, $"{tooltip} {"SY_BNV.Green".Translate()}");
			DrawTextFieldUnit<float?>(textFieldRect, null, "G");

			// Blue
			x += w + 4;
			textFieldRect = new Rect(x, offsetY + 6, w, SettingsRowHeight - 12);
			Widgets.TextFieldNumeric(textFieldRect, ref value.b, ref valueBuffers[2], 0f, 1f);
			DrawTooltip(textFieldRect, $"{tooltip} {"SY_BNV.Blue".Translate()}");
			DrawTextFieldUnit<float?>(textFieldRect, null, "B");

			// Reset button
			if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue.ToString()))
			{
				value = defaultValue;

				valueBuffers[0] = null;
				valueBuffers[1] = null;
				valueBuffers[2] = null;
			}

			return value;
		}

		public static void CreateDropdownSelectorControl<T>(
			float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			bool isModified,
			TargetWrapper<T> valueWrapper,
			T DefaultValue,
			IEnumerable<T> list,
			Func<T, string> itemToString)
		{
			var controlWidth = GetControlWidth(viewWidth);

			// Label
			if (isModified)
				GUI.color = ModifiedColor;
			Widgets.Label(new Rect(0, offsetY, controlWidth, SettingsRowHeight), label);
			GUI.color = OriColor;

			// Menu Generator
			IEnumerable<Widgets.DropdownMenuElement<T>> menuGenerator(TargetWrapper<T> target)
			{
				foreach (var item in list)
				{
					yield return new Widgets.DropdownMenuElement<T>
					{
						option = new FloatMenuOption(itemToString(item), () => target.Item = item),
						payload = item,
					};
				}
			}

			// Dropdown
			var rect = new Rect(controlWidth + 2, offsetY + 2, controlWidth - 4, SettingsRowHeight - 4);
			Widgets.Dropdown(
				rect,
				valueWrapper,
				null,
				menuGenerator,
				itemToString(valueWrapper.Item));
			DrawTooltip(rect, tooltip);

			// Reset
			if (isModified && DrawResetButton(offsetY, viewWidth, itemToString(DefaultValue)))
				valueWrapper.Item = DefaultValue;
		}
		#endregion
		#endregion

		#region PRIVATE METHODS
		private string BPReadyEffecterStateToString(BiosculpterPodEffecterState state)
		{
			switch (state)
			{
				case BiosculpterPodEffecterState.Default:
					return "SY_BNV.BPReadyEffecterState_Default";
				case BiosculpterPodEffecterState.AlwaysOn:
					return "SY_BNV.BPReadyEffecterState_AlwaysOn";
				case BiosculpterPodEffecterState.AlwaysOff:
					return "SY_BNV.BPReadyEffecterState_AlwaysOff";
			}
			throw new Exception($"{nameof(BPaNSVariations)}.{nameof(BPaNSControls)}.{nameof(BPReadyEffecterStateToString)}: unknown state encountered: {state}");
		}
		#endregion
	}
}
