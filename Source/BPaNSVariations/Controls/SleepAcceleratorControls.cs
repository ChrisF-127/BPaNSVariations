using BPaNSVariations.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BPaNSVariations.Controls
{
	internal class SleepAcceleratorControls : BaseControls
	{
		#region PROPERTIES
		public SleepAcceleratorSettings SleepAcceleratorSettings => (SleepAcceleratorSettings)Settings;
		#endregion

		#region CONSTRUCTORS
		public SleepAcceleratorControls(SleepAcceleratorSettings settings) : base(settings)
		{
		}
		#endregion

		#region OVERRIDES
		public override void CreateSettings(ref float offsetY, float viewWidth)
		{
			// Sleep Accelerator
			CreateTitle(
				ref offsetY,
				viewWidth,
				Label);


			// Margin
			offsetY += SettingsRowHeight / 2;
		}
		#endregion
	}
}
