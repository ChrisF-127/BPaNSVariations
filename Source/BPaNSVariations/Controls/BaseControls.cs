using BPaNSVariations.Settings;
using BPaNSVariations.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace BPaNSVariations.Controls
{
	internal abstract class BaseControls : ISelectableItem
	{
		#region PROPERTIES
		public BaseSettings Settings { get; }

		public virtual string Label =>
			Settings.GetName();
		public virtual bool IsModified =>
			Settings.IsModified();

		protected virtual bool CanBeCopied =>
			false;

		protected virtual Dictionary<string, string> ValueBuffers { get; } = new Dictionary<string, string>();
		#endregion

		#region FIELDS
		public const float SettingsRowHeight = 32f;
		public const float ThinSettingsRowHeight = 26f;
		public static readonly Color ModifiedColor = Color.cyan;
		public static readonly Color SelectionColor = Color.green;

		public static GameFont OriTextFont;
		public static TextAnchor OriTextAnchor;
		public static Color OriColor;

		private ThingDefCountClass _buildCostNewDef = null;
		#endregion

		#region CONSTRUCTORS
		public BaseControls(BaseSettings settings)
		{
			Settings = settings;
		}
		#endregion

		#region METHODS
		public virtual void CreateSettings(ref float offsetY, float viewWidth, out bool copy)
		{
			// Label
			copy = CreateTitle(
				ref offsetY,
				viewWidth,
				Label,
				CanBeCopied);

			#region GENERAL
			// General
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorGeneral".Translate());
			// General - Build Cost
			if (_buildCostNewDef == null)
				_buildCostNewDef = new ThingDefCountClass();
			CreateThingDefListControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.BuildCost".Translate(),
				ref _buildCostNewDef,
				Settings.BuildCost,
				Settings.DefaultBuildCost,
				BPaNSVariations.Settings.BuildCostThingDefs,
				"BuildCost");
			// General - Work to Build
			Settings.WorkToBuild = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.WorkToBuild".Translate(),
				"SY_BNV.TooltipWorkToBuild".Translate(),
				Settings.WorkToBuild,
				Settings.DefaultWorkToBuild,
				"WorkToBuild",
				additionalText: WorkToBuildToWorkLeft);
			// General - Active Power Consumption
			Settings.ActivePowerConsumption = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.ActivePowerConsumption".Translate(),
				"SY_BNV.TooltipActivePowerConsumption".Translate(),
				Settings.ActivePowerConsumption,
				Settings.DefaultActivePowerConsumption,
				"ActivePowerConsumption",
				unit: "W");
			#endregion
		}

		public virtual void ResetBuffers() =>
			ValueBuffers.Clear();
		#endregion

		#region PUBLIC METHODS
		public static float GetControlWidth(float viewWidth) =>
			viewWidth / 2 - SettingsRowHeight - 4;

		public static bool DrawResetButton(float offsetY, float viewWidth, string tooltip)
		{
			var buttonRect = new Rect(viewWidth + 2 - (SettingsRowHeight * 2), offsetY + 2, SettingsRowHeight * 2 - 4, SettingsRowHeight - 4);
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

		public static bool CreateTitle(
			ref float offsetY,
			float viewWidth,
			string text,
			bool showCopyTo)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0, offsetY, viewWidth, SettingsRowHeight), text);
			Text.Font = GameFont.Small;

			var copyTo = false;
			if (showCopyTo)
			{
				var buttonRect = new Rect(viewWidth + 2 - (SettingsRowHeight * 4), offsetY + 2, SettingsRowHeight * 4 - 4, SettingsRowHeight - 4);
				DrawTooltip(buttonRect, "SY_BNV.TooltipCopyToAll".Translate());
				copyTo = Widgets.ButtonText(buttonRect, "SY_BNV.CopyToAll".Translate());
			}
			offsetY += SettingsRowHeight + 2;
			return copyTo;
		}

		public static void CreateSeparator(
			ref float offsetY,
			float viewWidth,
			string text)
		{
			offsetY += 5;
			Widgets.ListSeparator(ref offsetY, viewWidth, text);
			offsetY += 5;
			Text.Anchor = TextAnchor.MiddleLeft;
		}

		public static bool CreateCheckbox(
			ref float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			bool value,
			bool defaultValue,
			string text = null)
		{
			var controlWidth = GetControlWidth(viewWidth);
			var isModified = value != defaultValue;

			// Label
			if (isModified)
				GUI.color = ModifiedColor;
			Widgets.Label(new Rect(0, offsetY, controlWidth, SettingsRowHeight), label);
			GUI.color = OriColor;

			// Setting
			var checkboxSize = SettingsRowHeight - 8;
			Widgets.Checkbox(controlWidth, offsetY + (SettingsRowHeight - checkboxSize) / 2, ref value, checkboxSize);
			DrawTooltip(new Rect(controlWidth, offsetY, checkboxSize, checkboxSize), tooltip);

			// Text
			if (text != null)
				Widgets.Label(new Rect(controlWidth + checkboxSize + 4, offsetY + 4, controlWidth - checkboxSize - 6, SettingsRowHeight - 8), $"({text})");

			// Reset button
			if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue.ToString()))
				value = defaultValue;

			offsetY += SettingsRowHeight;
			return value;
		}

		public T CreateNumeric<T>(
			ref float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			T value,
			T defaultValue,
			string valueBufferKey,
			float min = 0f,
			float max = 1e+9f,
			AdditionalTextDelegate<T> additionalText = null,
			string unit = null)
			where T : struct, IComparable =>
			CreateNumeric(
				ref offsetY,
				viewWidth,
				label,
				tooltip,
				!value.Equals(defaultValue),
				value,
				defaultValue,
				valueBufferKey,
				ValueBuffers,
				min,
				max,
				additionalText,
				unit);
		public static T CreateNumeric<T>(
			ref float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			bool isModified,
			T value,
			T defaultValue,
			string valueBufferKey,
			Dictionary<string, string> valueBuffers,
			float min = 0f,
			float max = 1e+9f,
			AdditionalTextDelegate<T> additionalText = null,
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
			var textFieldRect = new Rect(controlWidth + 2, offsetY + 6, controlWidth / 2 - 4, SettingsRowHeight - 12);
			var valueBuffer = valueBuffers.GetOrAddDefault(valueBufferKey);
			Widgets.TextFieldNumeric(textFieldRect, ref value, ref valueBuffer, min, max);
			valueBuffers[valueBufferKey] = valueBuffer;
			if (!string.IsNullOrWhiteSpace(tooltip))
				DrawTooltip(textFieldRect, tooltip);

			// Unit
			DrawTextFieldUnit<T?>(textFieldRect, null, unit);

			// Additional Text
			if (additionalText != null)
			{
				var additionalTextRect = textFieldRect;
				additionalTextRect.x += textFieldRect.width + 8;
				additionalTextRect.width -= 8;
				Widgets.Label(additionalTextRect, additionalText(value));
			}

			// Reset button
			if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue.ToString()))
			{
				value = defaultValue;
				valueBuffers.Remove(valueBufferKey);
			}

			offsetY += SettingsRowHeight;
			return value;
		}

		public T? CreateNullableNumeric<T>(
			ref float offsetY,
			float viewWidth,
			string label,
			string labelDisabled,
			string tooltip,
			string tooltipCheckbox,
			T? value,
			T? defaultValue,
			string valueBufferKey,
			float min = 0f,
			float max = 1e+9f,
			AdditionalTextDelegate<T?> additionalText = null,
			string unit = null)
			where T : struct =>
			CreateNullableNumeric(
				ref offsetY,
				viewWidth,
				label,
				labelDisabled,
				tooltip,
				tooltipCheckbox,
				!(value == null && defaultValue == null || value != null && defaultValue != null && value.Equals(defaultValue)),
				value,
				defaultValue,
				valueBufferKey,
				ValueBuffers,
				min,
				max,
				additionalText,
				unit);
		public static T? CreateNullableNumeric<T>(
			ref float offsetY,
			float viewWidth,
			string label,
			string labelDisabled,
			string tooltip,
			string tooltipCheckbox,
			bool isModified,
			T? value,
			T? defaultValue,
			string valueBufferKey,
			Dictionary<string, string> valueBuffers,
			float min = 0f,
			float max = 1e+9f,
			AdditionalTextDelegate<T?> additionalText = null,
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
			var selected = value != null;
			var checkboxSize = SettingsRowHeight - 8;
			Widgets.Checkbox(controlWidth, offsetY + (SettingsRowHeight - checkboxSize) / 2, ref selected, checkboxSize);
			DrawTooltip(new Rect(controlWidth, offsetY, checkboxSize, checkboxSize), tooltipCheckbox);

			// Value
			float offsetX = controlWidth + checkboxSize + 4;
			float width = controlWidth / 2 - checkboxSize - 6;
			if (selected)
			{
				var textFieldRect = new Rect(offsetX, offsetY + 6, width, SettingsRowHeight - 12);
				var val = value ?? defaultValue ?? default;
				var valueBuffer = valueBuffers.GetOrAddDefault(valueBufferKey);
				Widgets.TextFieldNumeric(textFieldRect, ref val, ref valueBuffer, min, max);
				valueBuffers[valueBufferKey] = valueBuffer;
				DrawTooltip(textFieldRect, tooltip);
				value = val;

				// Unit
				DrawTextFieldUnit<T?>(textFieldRect, null, unit);

				// Additional Text
				if (additionalText != null)
				{
					var additionalTextRect = textFieldRect;
					additionalTextRect.x += textFieldRect.width + 8;
					additionalTextRect.width = controlWidth / 2 - 12;
					Widgets.Label(additionalTextRect, additionalText(value));
				}
			}
			else
			{
				// Label when disabled
				if (labelDisabled != null)
					Widgets.Label(new Rect(offsetX, offsetY + 4, controlWidth - checkboxSize - 12, SettingsRowHeight - 8), $"({labelDisabled})");

				value = null;
			}

			// Reset button
			if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue?.ToString() ?? "null"))
			{
				value = defaultValue;
				valueBuffers.Remove(valueBufferKey);
			}

			// Output
			offsetY += SettingsRowHeight;
			return value;
		}

		public Color CreateColorControl(
			ref float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			Color value,
			Color defaultValue,
			string valueBufferKey) =>
			CreateColorControl(
				ref offsetY,
				viewWidth,
				label,
				tooltip,
				value,
				defaultValue,
				valueBufferKey,
				ValueBuffers);
		public static Color CreateColorControl(
			ref float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			Color value,
			Color defaultValue,
			string valueBufferKey,
			Dictionary<string, string> valueBuffers)
		{
			var controlWidth = GetControlWidth(viewWidth);
			var isModified = value != defaultValue;

			var bufferR = valueBufferKey + "_R";
			var bufferG = valueBufferKey + "_G";
			var bufferB = valueBufferKey + "_B";

			// Label
			if (isModified)
				GUI.color = ModifiedColor;
			Widgets.Label(new Rect(0, offsetY, controlWidth - 8, SettingsRowHeight), label);
			GUI.color = OriColor;

			// Red
			var x = controlWidth + 2;
			var w = (controlWidth / 3) - 4;
			var textFieldRect = new Rect(x, offsetY + 6, w, SettingsRowHeight - 12);
			var valueBuffer = valueBuffers.GetOrAddDefault(bufferR);
			Widgets.TextFieldNumeric(textFieldRect, ref value.r, ref valueBuffer, 0f, 1f);
			valueBuffers[bufferR] = valueBuffer;
			DrawTooltip(textFieldRect, $"{"SY_BNV.Red".Translate()}:\n{tooltip}");
			GUI.color = Color.red;
			DrawTextFieldUnit<float?>(textFieldRect, null, "R");
			GUI.color = OriColor;

			// Green
			x += w + 4;
			textFieldRect = new Rect(x, offsetY + 6, w, SettingsRowHeight - 12);
			valueBuffer = valueBuffers.GetOrAddDefault(bufferG);
			Widgets.TextFieldNumeric(textFieldRect, ref value.g, ref valueBuffer, 0f, 1f);
			valueBuffers[bufferG] = valueBuffer;
			DrawTooltip(textFieldRect, $"{"SY_BNV.Green".Translate()}:\n{tooltip}");
			GUI.color = Color.green;
			DrawTextFieldUnit<float?>(textFieldRect, null, "G");
			GUI.color = OriColor;

			// Blue
			x += w + 4;
			textFieldRect = new Rect(x, offsetY + 6, w, SettingsRowHeight - 12);
			valueBuffer = valueBuffers.GetOrAddDefault(bufferB);
			Widgets.TextFieldNumeric(textFieldRect, ref value.b, ref valueBuffer, 0f, 1f);
			valueBuffers[bufferB] = valueBuffer;
			DrawTooltip(textFieldRect, $"{"SY_BNV.Blue".Translate()}:\n{tooltip}");
			GUI.color = Color.blue;
			DrawTextFieldUnit<float?>(textFieldRect, null, "B");
			GUI.color = OriColor;

			// Reset button
			if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue.ToString()))
			{
				value = defaultValue;

				valueBuffers.Remove(bufferR);
				valueBuffers.Remove(bufferG);
				valueBuffers.Remove(bufferB);
			}

			offsetY += SettingsRowHeight;
			return value;
		}

		public static T CreateDropdownSelectorControl<T>(
			ref float offsetY,
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
			Widgets.Label(new Rect(0, offsetY, controlWidth - 8, SettingsRowHeight), label);
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

			offsetY += SettingsRowHeight;
			return valueWrapper.Item;
		}

		public void CreateThingDefListControl(
			ref float offsetY,
			float viewWidth,
			string label,
			ref ThingDefCountClass newThing,
			List<ThingDefCountClass> values,
			List<ThingDefCountClass> defaultValues,
			IEnumerable<ThingDef> availableThingDefs,
			string valueBufferKey) =>
			CreateThingDefListControl(
				ref offsetY,
				viewWidth,
				label,
				ref newThing,
				values,
				defaultValues,
				availableThingDefs,
				valueBufferKey,
				ValueBuffers);
		public static void CreateThingDefListControl(
			ref float offsetY,
			float viewWidth,
			string label,
			ref ThingDefCountClass newThing,
			List<ThingDefCountClass> values,
			List<ThingDefCountClass> defaultValues,
			IEnumerable<ThingDef> availableThingDefs,
			string valueBufferKey,
			Dictionary<string, string> valueBuffers)
		{
			var oriOffsetY = offsetY;
			var controlWidth = GetControlWidth(viewWidth);
			var isModified = values.AnyDifference(defaultValues);

			// Label
			if (isModified)
				GUI.color = ModifiedColor;
			Widgets.Label(new Rect(0, offsetY, controlWidth, SettingsRowHeight), label);
			GUI.color = OriColor;

			// Menu Generator
			IEnumerable<Widgets.DropdownMenuElement<ThingDef>> menuGenerator(ThingDefCountClass target)
			{
				foreach (var def in availableThingDefs)
				{
					yield return new Widgets.DropdownMenuElement<ThingDef>
					{
						option = new FloatMenuOption(def.LabelCap, () => target.thingDef = def),
						payload = def,
					};
				}
			}

			offsetY += 1;

			// New ThingDef selector
			var thingDefRect = new Rect(controlWidth + 2, offsetY + 4, controlWidth / 2 - 4, ThinSettingsRowHeight - 4);
			Widgets.Dropdown(
				thingDefRect,
				newThing,
				null,
				menuGenerator,
				newThing.thingDef?.LabelCap ?? $"({"SY_BNV.Select".Translate()})");
			DrawTooltip(thingDefRect, "SY_BNV.SelectNewThingDef".Translate());


			// New ThingDef count
			var countRect = new Rect(thingDefRect.x + thingDefRect.width + 4, offsetY + 5, controlWidth / 2 - ThinSettingsRowHeight, ThinSettingsRowHeight - 6);
			var bufferKey = valueBufferKey + "_0";
			var valueBuffer = valueBuffers.GetOrAddDefault(bufferKey);
			Widgets.TextFieldNumeric(countRect, ref newThing.count, ref valueBuffer, 1, int.MaxValue);
			valueBuffers[bufferKey] = valueBuffer;
			DrawTooltip(countRect, "SY_BNV.Count".Translate());

			// Add button
			var smallButtonRect = new Rect(countRect.x + countRect.width + 4, offsetY + 3, ThinSettingsRowHeight - 2, ThinSettingsRowHeight - 2);
			var newThingThingDef = newThing.thingDef;
			if (newThing.thingDef != null && !values.Any(v => v.thingDef == newThingThingDef) && Widgets.ButtonText(smallButtonRect, "+"))
			{
				values.Add(newThing);
				newThing = new ThingDefCountClass();
				valueBuffers.Remove(bufferKey);
			}
			DrawTooltip(smallButtonRect, "SY_BNV.Add".Translate());

			// Row offset
			offsetY += ThinSettingsRowHeight + 2;


			// Create list
			var defIconRect = thingDefRect;
			defIconRect.width = ThinSettingsRowHeight - 4;
			thingDefRect.x += defIconRect.width + 4;
			thingDefRect.width -= defIconRect.width + 4;
			for (int i = 0; i < values.Count; i++)
			{
				var value = values[i];
				var bufferPos = i + 1;

				// Set new offsetY
				defIconRect.y = offsetY + 4;
				thingDefRect.y = offsetY + 4;
				countRect.y = offsetY + 5;
				smallButtonRect.y = offsetY + 3;

				// ThingDef Icon
				Widgets.DefIcon(defIconRect, value.thingDef);

				// ThingDef Label
				if (!defaultValues.Any(v => v.thingDef == value.thingDef && v.count == value.count))
					GUI.color = ModifiedColor;
				Widgets.Label(thingDefRect, value.thingDef.LabelCap);
				GUI.color = OriColor;

				// Count
				bufferKey = valueBufferKey + $"_{bufferPos}";
				valueBuffer = valueBuffers.GetOrAddDefault(bufferKey);
				var intValue = value.count;
				Widgets.TextFieldNumeric(countRect, ref intValue, ref valueBuffer, 1, int.MaxValue);
				value.count = intValue;
				valueBuffers[bufferKey] = valueBuffer;
				DrawTooltip(countRect, "SY_BNV.Count".Translate());

				// Remove button
				if (Widgets.ButtonText(smallButtonRect, "-") && values.Any(v => v.thingDef == value.thingDef))
				{
					values.Remove(value);
					valueBuffers.Remove(bufferKey);
				}
				DrawTooltip(smallButtonRect, "SY_BNV.Remove".Translate());

				// Row offset
				offsetY += ThinSettingsRowHeight;
			}


			// Reset (must be added last to not cause focus to jump when it appears)
			string itemToString(List<ThingDefCountClass> thingDefCountList)
			{
				var sb = new StringBuilder();
				foreach (var item in thingDefCountList)
					sb.Append($"\n{item.count}x {item.thingDef?.LabelCap}");
				return sb.ToString();
			}
			if (isModified && DrawResetButton(oriOffsetY, viewWidth, itemToString(defaultValues)))
			{
				values.SetFrom(defaultValues);
				valueBuffers.RemoveAll(vb => vb.Key.StartsWith(valueBufferKey));
			}

			offsetY += 4;
		}

		public void CreateSimpleCurveControl(
			ref float offsetY,
			float viewWidth,
			string label,
			SimpleCurve curve,
			SimpleCurve defaultCurve,
			string valueBufferKey) =>
			CreateSimpleCurveControl(
				ref offsetY,
				viewWidth,
				label,
				curve,
				defaultCurve,
				valueBufferKey,
				ValueBuffers);
		public static void CreateSimpleCurveControl(
			ref float offsetY,
			float viewWidth,
			string label,
			SimpleCurve curve,
			SimpleCurve defaultCurve,
			string valueBufferKey,
			Dictionary<string, string> valueBuffers)
		{
			var oriOffsetY = offsetY;
			var controlWidth = GetControlWidth(viewWidth);
			var isModified = !curve.SequenceEqual(defaultCurve);

			// Label
			if (isModified)
				GUI.color = ModifiedColor;
			Widgets.Label(new Rect(0, offsetY, controlWidth, SettingsRowHeight), label);
			GUI.color = OriColor;

			offsetY += 1;

			// Create list
			var xRect = new Rect(controlWidth + 2, 0, controlWidth / 2 - 4, ThinSettingsRowHeight - 6);
			var yRect = new Rect(xRect.x + xRect.width + 4, 0, controlWidth / 2 - ThinSettingsRowHeight, ThinSettingsRowHeight - 6);
			for (int i = 0; i < curve.Count(); i++)
			{
				var value = curve[i];
				var bufferPos = i + 1;

				// Set new offset
				xRect.y = offsetY + 5;
				yRect.y = offsetY + 5;

				var x = value.x;
				var y = value.y;

				// X
				var bufferKey = valueBufferKey + $"_{bufferPos}_x";
				var valueBuffer = valueBuffers.GetOrAddDefault(bufferKey);
				Widgets.TextFieldNumeric(xRect, ref x, ref valueBuffer, -1E+09f);
				valueBuffers[bufferKey] = valueBuffer;
				DrawTooltip(xRect, "SY_BNV.TooltipCurveX".Translate());

				// Y
				bufferKey = valueBufferKey + $"_{bufferPos}_y";
				valueBuffer = valueBuffers.GetOrAddDefault(bufferKey);
				Widgets.TextFieldNumeric(yRect, ref y, ref valueBuffer, -1E+09f);
				valueBuffers[bufferKey] = valueBuffer;
				DrawTooltip(yRect, "SY_BNV.TooltipCurveY".Translate());

				// Set if changed
				if (value.x != x || value.y != y) 
					curve[i] = new CurvePoint(x, y);

				// Row offset
				offsetY += ThinSettingsRowHeight;
			}


			// Reset (must be added last to not cause focus to jump when it appears)
			string itemToString(SimpleCurve c)
			{
				var sb = new StringBuilder();
				foreach (var p in c)
					sb.Append($"\n{p.x:0.000}\t{p.y:0.000}");
				return sb.ToString();
			}
			if (isModified && DrawResetButton(oriOffsetY, viewWidth, itemToString(defaultCurve)))
			{
				curve.SetPoints(defaultCurve);
				valueBuffers.RemoveAll(vb => vb.Key.StartsWith(valueBufferKey));
			}

			offsetY += 4;
		}
		#endregion

		#region PRIVATE METHODS
		protected string WorkToBuildToWorkLeft(float work) =>
			$"{work / 60f:0} {"SY_BNV.Work".Translate()}";

		protected string YearsToText(float years) =>
			DaysToText(years * 60f);
		protected string DaysToText(float days) =>
			HoursToText(days * 24f);
		protected string HoursToText(float hours)
		{
			var output = new StringBuilder();
			var y = Mathf.Floor(hours / 24f / 60f);
			if (y > 0) output.Append($"{y:0} y");
			output.Append("\t");
			var d = Mathf.Floor(hours / 24f % 60f);
			if (y > 0 || d > 0) output.Append($"{d:0} d");
			output.Append("\t");
			var h = hours % 24f;
			output.Append($"{h:0.00} h");
			return output.ToString();
		}

		protected string TicksToYearText(int ticks) =>
			YearsToText(ticks / 3600000f);

		protected string ValueToPercent(float f) =>
			$"{f:P0}";
		#endregion

		#region CLASSES
		internal delegate string AdditionalTextDelegate<T>(T value);

		internal class TargetWrapper<T>
		{
			public T Item { get; set; }

			public TargetWrapper(T item)
			{
				Item = item;
			}
		}
		#endregion
	}
}
