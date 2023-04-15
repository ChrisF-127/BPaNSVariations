using System;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

namespace BPaNSVariations
{
	internal class BPaNSVariations : Mod
	{
		#region PROPERTIES
		public static BPaNSVariations Instance { get; private set; } = null;
		public static BPaNSSettings Settings { get; private set; } = null;
		public static BPaNSControls Controls { get; } = new BPaNSControls();
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
			Controls.CreateSettingsUI(inRect, Settings);

			base.DoSettingsWindowContents(inRect);
		}
		#endregion

		#region PRIVATE METHODS
		private void Initialize()
		{
			Settings = GetSettings<BPaNSSettings>();
		}
		#endregion
	}
}
