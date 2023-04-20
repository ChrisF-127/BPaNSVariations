using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BPaNSVariations.Settings
{
	internal class NeuralSuperchargerSettings : BaseSettings
	{
		#region PROPERTIES
		public ThingDef NeuralSupercharger { get; }
		#endregion

		#region CONSTRUCTORS
		public NeuralSuperchargerSettings(ThingDef neuralSupercharger)
		{
			NeuralSupercharger = neuralSupercharger;
		}
		#endregion

		#region OVERRIDES
		public override string GetName() =>
			NeuralSupercharger.LabelCap;
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
			if (to != this && to is NeuralSuperchargerSettings copy)
			{

			}
		}
		#endregion
	}
}
