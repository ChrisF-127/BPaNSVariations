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
using static UnityEngine.UI.CanvasScaler;

namespace BPaNSVariations
{
	internal delegate string AdditionalTextDelegate<T>(T value);

	internal class BPaNSControls
	{
		#region FIELDS
		public const float SettingsRowHeight = 32f;
		public const float ThinSettingsRowHeight = 26f;
		public static readonly Color ModifiedColor = Color.cyan;
		public static readonly Color SelectionColor = Color.green;

		public static GameFont OriTextFont;
		public static TextAnchor OriTextAnchor;
		public static Color OriColor;

		public float _settingsViewHeight = 0;
		private Vector2 _settingsScrollPosition;

		#region SETTINGS
		private ThingDefCountClass _bpBuildCostNewDef = null;
		private TargetWrapper<BiosculpterPodEffectAnimation> _bpReadyEffectStateTargetWrapper = null;
		private readonly BiosculpterPodEffectAnimation[] _bpReadyEffectStates = (BiosculpterPodEffectAnimation[])Enum.GetValues(typeof(BiosculpterPodEffectAnimation));
		private ThingDefCountClass _bpRegenerationCycleIngredientsNewDef = null;

		private string _bpNutritionRequiredBuffer;
		private string _bpBiotunedDurationBuffer;
		private string _bpBiotunedCycleSpeedFactorBuffer;
		private List<string> _bpBuildCostBuffers;
		private string _bpWorkToBuildBuffer;
		private string _bpActivePowerConsumptionBuffer;
		private string _bpStandbyPowerConsumptionBuffer;
		private string[] _bpReadyEffectColorBuffers;
		private string _bpMedicCycleDurationBuffer;
		private string _bpRegenerationCycleDurationBuffer;
		private List<string> _bpRegenerationCycleIngredientsBuffers;
		private string _bpAgeReversalCycleDurationBuffer;
		private string _bpAgeReversalCycleAgeReversedBuffer;
		private string _bpPleasureCycleDurationBuffer;
		private string _bpPleasureCycleMoodEffectBuffer;
		private string _bpPleasureCycleMoodDurationBuffer;
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
					new Rect(0, 0, viewWidth, _settingsViewHeight));

				// Biosculpter Pod Settings
				CreateBiosculpterPodSettings(
					settings, 
					ref totalHeight, 
					viewWidth);

				// Neural Supercharger
				CreateNeuralSuperchargerSettings(
					settings, 
					ref totalHeight, 
					viewWidth);

				// Sleep Accelerator
				CreateSleepAcceleratorSettings(
					settings, 
					ref totalHeight, 
					viewWidth);
			}
			finally
			{
				// Remember settings view height for potential scrolling
				_settingsViewHeight = totalHeight;

				// End ScrollView and Group
				Widgets.EndScrollView();
				GUI.EndGroup();

				// Reset text settings
				Text.Font = OriTextFont;
				Text.Anchor = OriTextAnchor;
				GUI.color = OriColor;
			}
		}
		#endregion

		#region PRIVATE METHODS
		private void CreateBiosculpterPodSettings(BPaNSSettings settings, ref float offsetY, float viewWidth)
		{
			// Biosculpter Pod
			CreateTitle(
				ref offsetY, 
				viewWidth, 
				"SY_BNV.TitleBiosculpterPod".Translate());

			// Biosculpter Pod - General Settings
			CreateSeparator(
				ref offsetY, 
				viewWidth,
				"SY_BNV.SeparatorBPGeneralSettings".Translate());
			// Biosculpter Pod - General Settings - Nutrition Required
			settings.BPNutritionRequired = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPNutritionRequired".Translate(),
				"SY_BNV.TooltipBPNutritionRequired".Translate(),
				settings.BPNutritionRequired,
				settings.DefaultBPNutritionRequired,
				ref _bpNutritionRequiredBuffer,
				additionalText: v => "SY_BNV.Nutrition".Translate());
			// Biosculpter Pod - General Settings - Biotuned Duration
			settings.BPBiotunedDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPBiotunedDuration".Translate(),
				"SY_BNV.TooltipBPBiotunedDuration".Translate(),
				settings.BPBiotunedDuration,
				settings.DefaultBPBiotunedDuration,
				ref _bpBiotunedDurationBuffer,
				additionalText: TicksToYearText,
				unit: "Ticks");
			// Biosculpter Pod - General Settings - Biotuned Cycle Speed Factor
			settings.BPBiotunedCycleSpeedFactor = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPBiotunedCycleSpeedFactor".Translate(),
				"SY_BNV.TooltipBPBiotunedCycleSpeedFactor".Translate(),
				settings.BPBiotunedCycleSpeedFactor,
				settings.DefaultBPBiotunedCycleSpeedFactor,
				ref _bpBiotunedCycleSpeedFactorBuffer);
			// Biosculpter Pod - General Settings - Build Cost
			if (_bpBuildCostNewDef == null)
				_bpBuildCostNewDef = new ThingDefCountClass();
			CreateThingDefListControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPBuildCost".Translate(),
				ref _bpBuildCostNewDef,
				settings.BPBuildCost,
				settings.DefaultBPBuildCost,
				settings.BuildCostThingDefs,
				ref _bpBuildCostBuffers);
			settings.ApplyBPBuildCost();
			// Biosculpter Pod - General Settings - Work to Build
			settings.BPWorkToBuild = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPWorkToBuild".Translate(),
				"SY_BNV.TooltipBPWorkToBuild".Translate(),
				settings.BPWorkToBuild,
				settings.DefaultBPWorkToBuild,
				ref _bpWorkToBuildBuffer,
				additionalText: WorkToBuildToWorkLeft);
			// Biosculpter Pod - General Settings - Active Power Consumption
			settings.BPActivePowerConsumption = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPActivePowerConsumption".Translate(),
				"SY_BNV.TooltipBPActivePowerConsumption".Translate(),
				settings.BPActivePowerConsumption,
				settings.DefaultBPActivePowerConsumption,
				ref _bpActivePowerConsumptionBuffer,
				unit: "W");
			// Biosculpter Pod - General Settings - Standby Power Consumption
			settings.BPStandbyPowerConsumption = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPStandbyPowerConsumption".Translate(),
				"SY_BNV.TooltipBPStandbyPowerConsumption".Translate(),
				settings.BPStandbyPowerConsumption,
				settings.DefaultBPStandbyPowerConsumption,
				ref _bpStandbyPowerConsumptionBuffer,
				unit: "W");

			// Biosculpter Pod - Ready Effect
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorBPReadyEffect".Translate());
			// Biosculpter Pod - Ready Effect - Ready Effect Animation
			if (_bpReadyEffectStateTargetWrapper == null)
				_bpReadyEffectStateTargetWrapper = new TargetWrapper<BiosculpterPodEffectAnimation>(settings.BPReadyEffectState);
			settings.BPReadyEffectState = CreateDropdownSelectorControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPReadyEffectState".Translate(),
				"SY_BNV.TooltipBPReadyEffectState".Translate(),
				settings.BPReadyEffectState != settings.DefaultBPReadyEffectState,
				_bpReadyEffectStateTargetWrapper,
				settings.DefaultBPReadyEffectState,
				_bpReadyEffectStates,
				state => BPReadyEffectStateToString(state).Translate());
			// Biosculpter Pod - Ready Effect - Ready Effect Color
			settings.BPReadyEffectColor = CreateColorControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPReadyEffectColor".Translate(),
				"SY_BNV.TooltipBPReadyEffectColor".Translate(),
				settings.BPReadyEffectColor,
				settings.DefaultBPReadyEffectColor,
				ref _bpReadyEffectColorBuffers);

			// Biosculpter Pod - Medic Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorBPMedicCycle".Translate());
			// Biosculpter Pod - Medic Cycle - Duration
			settings.BPMedicCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPMedicCycleDuration".Translate(),
				"SY_BNV.TooltipBPMedicCycleDuration".Translate(),
				settings.BPMedicCycleDuration,
				settings.DefaultBPMedicCycleDuration,
				ref _bpMedicCycleDurationBuffer,
				additionalText: DaysToText,
				unit: "d");

			// Biosculpter Pod - Regeneration Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorBPRegenerationCycle".Translate());
			// Biosculpter Pod - Regeneration Cycle - Duration
			settings.BPRegenerationCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPRegenerationCycleDuration".Translate(),
				"SY_BNV.TooltipBPRegenerationCycleDuration".Translate(),
				settings.BPRegenerationCycleDuration,
				settings.DefaultBPRegenerationCycleDuration,
				ref _bpRegenerationCycleDurationBuffer,
				additionalText: DaysToText,
				unit: "d");
			// Biosculpter Pod - General Settings - Build Cost
			if (_bpRegenerationCycleIngredientsNewDef == null)
				_bpRegenerationCycleIngredientsNewDef = new ThingDefCountClass();
			CreateThingDefListControl(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPRegenerationCycleIngredients".Translate(),
				ref _bpRegenerationCycleIngredientsNewDef,
				settings.BPRegenerationCycleIngredients,
				settings.DefaultBPRegenerationCycleIngredients,
				settings.MedicineThingDefs,
				ref _bpRegenerationCycleIngredientsBuffers);
			settings.ApplyBPRegenerationCycleIngredients();

			// Biosculpter Pod - Age Reversal Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorBPAgeReversalCycle".Translate());
			// Biosculpter Pod - Age Reversal Cycle - Duration
			settings.BPAgeReversalCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPAgeReversalCycleDuration".Translate(),
				"SY_BNV.TooltipBPAgeReversalCycleDuration".Translate(),
				settings.BPAgeReversalCycleDuration,
				settings.DefaultBPAgeReversalCycleDuration,
				ref _bpAgeReversalCycleDurationBuffer,
				additionalText: DaysToText,
				unit: "d");
			// Biosculpter Pod - Age Reversal Cycle - Age reversed
			settings.BPAgeReversalCycleAgeReversed = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPAgeReversalCycleAgeReversed".Translate(),
				"SY_BNV.TooltipBPAgeReversalCycleAgeReversed".Translate(),
				settings.BPAgeReversalCycleAgeReversed,
				settings.DefaultBPAgeReversalCycleAgeReversed,
				ref _bpAgeReversalCycleAgeReversedBuffer,
				additionalText: YearsToText,
				unit: "y");

			// Biosculpter Pod - Pleasure Cycle
			CreateSeparator(
				ref offsetY,
				viewWidth,
				"SY_BNV.SeparatorBPPleasureCycle".Translate());
			// Biosculpter Pod - Pleasure Cycle - Duration
			settings.BPPleasureCycleDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPPleasureCycleDuration".Translate(),
				"SY_BNV.TooltipBPPleasureCycleDuration".Translate(),
				settings.BPPleasureCycleDuration,
				settings.DefaultBPPleasureCycleDuration,
				ref _bpPleasureCycleDurationBuffer,
				additionalText: DaysToText,
				unit: "d");
			// Biosculpter Pod - Pleasure Cycle - Mood Effect
			settings.BPPleasureCycleMoodEffect = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPPleasureCycleMoodEffect".Translate(),
				"SY_BNV.TooltipBPPleasureCycleMoodEffect".Translate(),
				settings.BPPleasureCycleMoodEffect,
				settings.DefaultBPPleasureCycleMoodEffect,
				ref _bpPleasureCycleMoodEffectBuffer);
			// Biosculpter Pod - Pleasure Cycle - Mood Duration
			settings.BPPleasureCycleMoodDuration = CreateNumeric(
				ref offsetY,
				viewWidth,
				"SY_BNV.BPPleasureCycleMoodDuration".Translate(),
				"SY_BNV.TooltipBPPleasureCycleMoodDuration".Translate(),
				settings.BPPleasureCycleMoodDuration,
				settings.DefaultBPPleasureCycleMoodDuration,
				ref _bpPleasureCycleMoodDurationBuffer,
				additionalText: DaysToText,
				unit: "d");

			// Margin
			offsetY += SettingsRowHeight / 2;
		}

		private void CreateNeuralSuperchargerSettings(BPaNSSettings settings, ref float offsetY, float viewWidth)
		{
			// Neural Supercharger
			CreateTitle(
				ref offsetY,
				viewWidth,
				"SY_BNV.TitleNeuralSupercharger".Translate());


			// Margin
			offsetY += SettingsRowHeight / 2;
		}

		private void CreateSleepAcceleratorSettings(BPaNSSettings settings, ref float offsetY, float viewWidth)
		{
			// Sleep Accelerator
			CreateTitle(
				ref offsetY,
				viewWidth,
				"SY_BNV.TitleSleepAccelerator".Translate());


			// Margin
			offsetY += SettingsRowHeight / 2;
		}


		private string BPReadyEffectStateToString(BiosculpterPodEffectAnimation state)
		{
			switch (state)
			{
				case BiosculpterPodEffectAnimation.Default:
					return "SY_BNV.BPReadyEffectState_Default";
				case BiosculpterPodEffectAnimation.AlwaysOn:
					return "SY_BNV.BPReadyEffectState_AlwaysOn";
				case BiosculpterPodEffectAnimation.AlwaysOff:
					return "SY_BNV.BPReadyEffectState_AlwaysOff";
			}
			throw new Exception($"{nameof(BPaNSVariations)}.{nameof(BPaNSControls)}.{nameof(BPReadyEffectStateToString)}: unknown state encountered: {state}");
		}

		private string WorkToBuildToWorkLeft(float work) =>
			$"{work / 60f:0}\t{"SY_BNV.Work".Translate()}";

		private string YearsToText(float years) =>
			DaysToText(years * 60f);
		private string DaysToText(float days) =>
			HoursToText(days * 24f);
		private string HoursToText(float hours)
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

		private string TicksToYearText(int ticks) =>
			YearsToText(ticks / 3600000f);
		#endregion

		#region UI METHODS
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

		public static void CreateTitle(
			ref float offsetY,
			float viewWidth,
			string text)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0, offsetY, viewWidth, SettingsRowHeight), text);
			Text.Font = GameFont.Small;
			offsetY += SettingsRowHeight + 2;
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

		public static T CreateNumeric<T>(
			ref float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			T value,
			T defaultValue,
			ref string valueBuffer,
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
				ref valueBuffer,
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
			ref string valueBuffer,
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
			Widgets.TextFieldNumeric(textFieldRect, ref value, ref valueBuffer, min, max);
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
				valueBuffer = null;
			}

			offsetY += SettingsRowHeight;
			return value;
		}

		public static Color CreateColorControl(
			ref float offsetY,
			float viewWidth,
			string label,
			string tooltip,
			Color value,
			Color defaultValue,
			ref string[] valueBuffers)
		{
			var controlWidth = GetControlWidth(viewWidth);
			var isModified = value != defaultValue;

			if (valueBuffers?.Length != 3)
				valueBuffers = new string[3];

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
			DrawTooltip(textFieldRect, $"{"SY_BNV.Red".Translate()}:\n{tooltip}");
			GUI.color = Color.red;
			DrawTextFieldUnit<float?>(textFieldRect, null, "R");
			GUI.color = OriColor;

			// Green
			x += w + 4;
			textFieldRect = new Rect(x, offsetY + 6, w, SettingsRowHeight - 12);
			Widgets.TextFieldNumeric(textFieldRect, ref value.g, ref valueBuffers[1], 0f, 1f);
			DrawTooltip(textFieldRect, $"{"SY_BNV.Green".Translate()}:\n{tooltip}");
			GUI.color = Color.green;
			DrawTextFieldUnit<float?>(textFieldRect, null, "G");
			GUI.color = OriColor;

			// Blue
			x += w + 4;
			textFieldRect = new Rect(x, offsetY + 6, w, SettingsRowHeight - 12);
			Widgets.TextFieldNumeric(textFieldRect, ref value.b, ref valueBuffers[2], 0f, 1f);
			DrawTooltip(textFieldRect, $"{"SY_BNV.Blue".Translate()}:\n{tooltip}");
			GUI.color = Color.blue;
			DrawTextFieldUnit<float?>(textFieldRect, null, "B");
			GUI.color = OriColor;

			// Reset button
			if (isModified && DrawResetButton(offsetY, viewWidth, defaultValue.ToString()))
			{
				value = defaultValue;

				valueBuffers[0] = null;
				valueBuffers[1] = null;
				valueBuffers[2] = null;
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

			offsetY += SettingsRowHeight;
			return valueWrapper.Item;
		}

		public static void CreateThingDefListControl(
			ref float offsetY,
			float viewWidth,
			string label,
			ref ThingDefCountClass newThing,
			List<ThingDefCountClass> values,
			List<ThingDefCountClass> defaultValues,
			IEnumerable<ThingDef> availableThingDefs,
			ref List<string> valueBuffers)
		{
			var oriOffsetY = offsetY;
			var controlWidth = GetControlWidth(viewWidth);
			var isModified = values.IsModified(defaultValues);

			if (!(valueBuffers?.Count >= 1))
				valueBuffers = new List<string> { null };

			// Label
			if (isModified)
				GUI.color = ModifiedColor;
			Widgets.Label(new Rect(0, offsetY, controlWidth, ThinSettingsRowHeight), label);
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
				newThing.thingDef?.LabelCap);
			DrawTooltip(thingDefRect, "SY_BNV.SelectNewThingDef".Translate());

			// New ThingDef count
			var countRect = new Rect(thingDefRect.x + thingDefRect.width + 4, offsetY + 5, controlWidth / 2 - ThinSettingsRowHeight, ThinSettingsRowHeight - 6);
			var valueBuffer = valueBuffers[0];
			Widgets.TextFieldNumeric(countRect, ref newThing.count, ref valueBuffer, 1, int.MaxValue);
			valueBuffers[0] = valueBuffer;
			DrawTooltip(countRect, "SY_BNV.Count".Translate());

			// Add button
			var smallButtonRect = new Rect(countRect.x + countRect.width + 4, offsetY + 3, ThinSettingsRowHeight - 2, ThinSettingsRowHeight - 2);
			var newThingThingDef = newThing.thingDef;
			if (newThing.thingDef != null && !values.Any(v => v.thingDef == newThingThingDef) && Widgets.ButtonText(smallButtonRect, "+"))
			{
				values.Add(newThing);
				newThing = new ThingDefCountClass();
				valueBuffers[0] = null;
			}
			DrawTooltip(smallButtonRect, "SY_BNV.Add".Translate());

			// Row offset
			offsetY += ThinSettingsRowHeight + 2;


			// Create list
			for (int i = 0; i < values.Count; i++)
			{
				var value = values[i];
				var bufferPos = i + 1;

				// Set new offsetY
				thingDefRect.y = offsetY + 4;
				countRect.y = offsetY + 5;
				smallButtonRect.y = offsetY + 3;

				// ThingDef
				if (!defaultValues.Any(v => v.thingDef == value.thingDef && v.count == value.count))
					GUI.color = ModifiedColor;
				Widgets.Label(thingDefRect, value.thingDef.LabelCap);
				GUI.color = OriColor;

				// Count
				if (valueBuffers.Count == bufferPos) // should never be less than "i + 1"!
					valueBuffers.Add(null);
				valueBuffer = valueBuffers[bufferPos];
				var intValue = value.count;
				Widgets.TextFieldNumeric(countRect, ref intValue, ref valueBuffer, 1, int.MaxValue);
				value.count = intValue;
				valueBuffers[bufferPos] = valueBuffer;
				DrawTooltip(countRect, "SY_BNV.Count".Translate());

				// Remove button
				if (Widgets.ButtonText(smallButtonRect, "-") && values.Any(v => v.thingDef == value.thingDef))
				{
					values.Remove(value);
					valueBuffers.RemoveAt(bufferPos);
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
					sb.Append($"{item.thingDef?.LabelCap} {item.count}\n");
				return sb.ToString();
			}
			if (isModified && DrawResetButton(oriOffsetY, viewWidth, itemToString(defaultValues)))
			{
				values.Overwrite(defaultValues);
				valueBuffers = new List<string> { null };
			}

			offsetY += 4;
		}
		#endregion
	}

	internal class TargetWrapper<T>
	{
		public T Item { get; set; }

		public TargetWrapper(T item)
		{
			Item = item;
		}
	}
}
