using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BPaNSVariations.Settings
{
	internal abstract class BaseSettings : IExposable
	{
		public abstract string GetName();
		public abstract bool IsModified();

		public abstract void ExposeData();

		protected abstract void Initialize();

#warning TODO Copy to other
	}
}
