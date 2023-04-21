using System;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using BPaNSVariations.Controls;
using BPaNSVariations.Settings;

namespace BPaNSVariations
{
	internal class BPaNSVariations : Mod
	{
		#region PROPERTIES
		public static BPaNSVariations Instance { get; private set; }
		public static BPaNSSettings Settings { get; private set; }
		public static BPaNSControls Controls { get; private set; }
		#endregion

		#region CONSTRUCTORS
		public BPaNSVariations(ModContentPack content) : base(content)
		{
			Instance = this;

			LongEventHandler.ExecuteWhenFinished(Initialize);
		}
		#endregion

		#region OVERRIDES
		public override string SettingsCategory() =>
			"Biosculpter Pod and Neural Supercharger Variations";

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Controls.DoSettingsWindowContents(inRect);

			base.DoSettingsWindowContents(inRect);
		}
		#endregion

		#region PRIVATE METHODS
		private void Initialize()
		{
			BiosculpterPodSettings.InitializeStatics();
			NeuralSuperchargerSettings.InitializeStatics();

			Settings = GetSettings<BPaNSSettings>();
			Controls = new BPaNSControls(Settings);
		}
		#endregion
	}
}
