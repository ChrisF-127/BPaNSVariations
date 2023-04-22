using BPaNSVariations.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

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
	internal interface ISelectableItemWithType : ISelectableItem
	{
		SelectableTypes Type { get; }
		ISelectableItem SelectedItem { get; set; }
	}

	internal class SelectableType<T> : ISelectableItemWithType
		where T : class, ISelectableItem
	{
		#region PROPERTIES
		public static readonly SelectableTypes[] SelectableTypes = (SelectableTypes[])Enum.GetValues(typeof(SelectableTypes));

		public string Label { get; }
		public bool IsModified =>
			SelectableItems.Any(c => c.IsModified)
			|| _isGlobalModified?.Invoke() == true;

		public SelectableTypes Type { get; }

		public List<T> SelectableItems { get; }
		public ISelectableItem SelectedItem { get; set; }
		#endregion

		#region FIELDS
		private readonly Func<bool> _isGlobalModified;
		#endregion

		#region CONSTRUCTORS
		public SelectableType(string label, SelectableTypes type, List<T> selectableItems, Func<bool> isGlobalModified)
		{
			Label = label;
			Type = type;
			SelectableItems = selectableItems;
			_isGlobalModified = isGlobalModified;
		}
		#endregion
	}
}
