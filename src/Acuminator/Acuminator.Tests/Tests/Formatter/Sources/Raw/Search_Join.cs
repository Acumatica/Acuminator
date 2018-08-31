using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.IN
{
	public class InventoryItem : IBqlTable
	{
		#region ItemClassID
		public abstract class itemClassID : PX.Data.IBqlField
		{
		}
		protected int? _ItemClassID;

		/// <summary>
		/// The identifier of the <see cref="INItemClass">Item Class</see>, to which the Inventory Item belongs.
		/// Item Classes provide default settings for items, which belong to them, and are used to group items.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INItemClass.ItemClassID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
		[PXDefault(typeof(Search2<INItemClass.itemClassID, InnerJoin<INSetup, On<Current<InventoryItem.stkItem>, Equal<boolFalse>, And<INSetup.dfltNonStkItemClassID, Equal<INItemClass.itemClassID>, Or<Current<InventoryItem.stkItem>, Equal<boolTrue>, And<INSetup.dfltStkItemClassID, Equal<INItemClass.itemClassID>>>>>>>))]
		[PXUIRequired(typeof(INItemClass.stkItem))]
		public virtual int? ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion
	}
}
