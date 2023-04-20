using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BPaNSVariations.Settings
{
	internal class SleepAcceleratorSettings : BaseSettings
	{
		#region PROPERTIES
		public ThingDef SleepAccelerator { get; }
		#endregion

		#region CONSTRUCTORS
		public SleepAcceleratorSettings(ThingDef sleepAccelerator)
		{
			SleepAccelerator = sleepAccelerator;
		}
		#endregion

		#region OVERRIDES
		public override string GetName() =>
			SleepAccelerator.LabelCap;
		public override bool IsModified() =>
			false;

		protected override void Initialize()
		{
		}

		public override void ExposeData()
		{
		}

		public override void CopyTo(BaseSettings to)
		{
			if (to != this && to is SleepAcceleratorSettings copy)
			{

			}
		}
		#endregion
	}
}
