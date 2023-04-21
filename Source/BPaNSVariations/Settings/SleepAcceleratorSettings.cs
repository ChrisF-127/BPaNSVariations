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
		#endregion

		#region CONSTRUCTORS
		public SleepAcceleratorSettings(ThingDef sleepAccelerator) : base(sleepAccelerator)
		{
		}
		#endregion

		#region OVERRIDES
		public override bool IsModified() =>
			base.IsModified();

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving && !IsModified())
				return;

			if (Scribe.EnterNode(Def.defName))
			{
				try
				{
					base.ExposeData();

					// TODO
				}
				catch (Exception exc)
				{
					Log.Error(exc.ToString());
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
		}

		public override void CopyTo(BaseSettings to)
		{
			if (to != this && to is SleepAcceleratorSettings copy)
			{
				base.CopyTo(to);

				// TODO
			}
		}
		#endregion
	}
}
