using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPaNSVariations.Utility
{
	internal enum SelectableTypes
	{
		BiosculpterPod,
		NeuralSupercharger,
		SleepAccelerator,
	}

	internal interface ISelectableItem
	{
		string Label { get; }
		bool IsModified { get; }
	}

	internal class SelectableType : ISelectableItem
	{
		#region PROPERTIES
		public static readonly SelectableTypes[] SelectableTypes = (SelectableTypes[])Enum.GetValues(typeof(SelectableTypes));

		public string Label { get; }
		public bool IsModified =>
			_isModified();

		public SelectableTypes Type { get; }
		#endregion

		#region FIELDS
		private readonly Func<bool> _isModified;
		#endregion

		#region CONSTRUCTORS
		public SelectableType(string label, SelectableTypes type, Func<bool> isModified)
		{
			Label = label;
			Type = type;

			_isModified = isModified;
		}
		#endregion
	}
}
