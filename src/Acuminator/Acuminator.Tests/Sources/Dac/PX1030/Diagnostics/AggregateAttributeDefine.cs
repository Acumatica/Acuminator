using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace Acuminator.Tests.Sources.Dac.PX1030.Diagnostics
{
	public class AggregateAttributeDefine : PXGraph<AggregateAttributeDefine>
	{
	}
}



/// <summary>
/// This is a Generic Attribute that Aggregates other attributes and exposes there public properties.
/// The Attributes aggregated can be of the following types:
/// - DBFieldAttribute such as PXBDInt, PXDBString, etc.
/// - PXUIFieldAttribute
/// - PXSelectorAttribute
/// - PXDefaultAttribute
/// </summary>
public class AcctSubAttribute : PXAggregateAttribute, IPXInterfaceField, IPXCommandPreparingSubscriber, IPXRowSelectingSubscriber
{
	protected int _DBAttrIndex = -1;
	protected int _NonDBAttrIndex = -1;
	protected int _UIAttrIndex = -1;
	protected int _SelAttrIndex = -1;
	protected int _DefAttrIndex = -1;

	protected PXDBFieldAttribute DBAttribute => _DBAttrIndex == -1 ? null : (PXDBFieldAttribute)_Attributes[_DBAttrIndex];
	protected PXEventSubscriberAttribute NonDBAttribute => _NonDBAttrIndex == -1 ? null : _Attributes[_NonDBAttrIndex];
	protected PXUIFieldAttribute UIAttribute => _UIAttrIndex == -1 ? null : (PXUIFieldAttribute)_Attributes[_UIAttrIndex];
	protected PXDimensionSelectorAttribute SelectorAttribute => _SelAttrIndex == -1 ? null : (PXDimensionSelectorAttribute)_Attributes[_SelAttrIndex];
	protected PXDefaultAttribute DefaultAttribute => _DefAttrIndex == -1 ? null : (PXDefaultAttribute)_Attributes[_DefAttrIndex];

	protected virtual void Initialize()
	{
		_DBAttrIndex = -1;
		_NonDBAttrIndex = -1;
		_UIAttrIndex = -1;
		_SelAttrIndex = -1;
		_DefAttrIndex = -1;

		foreach (PXEventSubscriberAttribute attr in _Attributes)
		{
			if (attr is PXDBFieldAttribute)
			{
				_DBAttrIndex = _Attributes.IndexOf(attr);
				foreach (PXEventSubscriberAttribute sibling in _Attributes)
				{
					if (!object.ReferenceEquals(attr, sibling) && PXAttributeFamilyAttribute.IsSameFamily(attr.GetType(), sibling.GetType()))
					{
						_NonDBAttrIndex = _Attributes.IndexOf(sibling);
						break;
					}
				}
			}
			if (attr is PXUIFieldAttribute)
			{
				_UIAttrIndex = _Attributes.IndexOf(attr);
			}
			if (attr is PXDimensionSelectorAttribute)
			{
				_SelAttrIndex = _Attributes.IndexOf(attr);
			}
			if (attr is PXSelectorAttribute && _SelAttrIndex < 0)
			{
				_SelAttrIndex = _Attributes.IndexOf(attr);
			}
			if (attr is PXDefaultAttribute)
			{
				_DefAttrIndex = _Attributes.IndexOf(attr);
			}
		}
	}

	public AcctSubAttribute()
	{
		Initialize();
		this.Filterable = true;
	}

	public bool IsDBField { get; set; } = true;

	#region DBAttribute delagation
	public new string FieldName
	{
		get { return DBAttribute?.FieldName; }
		set { DBAttribute.FieldName = value; }
	}

	public bool IsKey
	{
		get { return DBAttribute?.IsKey ?? false; }
		set { DBAttribute.IsKey = value; }
	}

	public bool IsFixed
	{
		get { return ((PXDBStringAttribute)DBAttribute)?.IsFixed ?? false; }
		set
		{
			((PXDBStringAttribute)DBAttribute).IsFixed = value;
			if (NonDBAttribute != null)
				((PXStringAttribute)NonDBAttribute).IsFixed = value;
		}
	}

	public Type BqlField
	{
		get { return DBAttribute?.BqlField; }
		set
		{
			DBAttribute.BqlField = value;
			BqlTable = DBAttribute.BqlTable;
		}
	}
	#endregion

	#region UIAttribute delagation
	public PXUIVisibility Visibility
	{
		get { return UIAttribute?.Visibility ?? PXUIVisibility.Undefined; }
		set
		{
			if (UIAttribute != null)
				UIAttribute.Visibility = value;
		}
	}

	public bool Visible
	{
		get { return UIAttribute?.Visible ?? true; }
		set
		{
			if (UIAttribute != null)
				UIAttribute.Visible = value;
		}
	}

	public bool Enabled
	{
		get { return UIAttribute?.Enabled ?? true; }
		set
		{
			if (UIAttribute != null)
				UIAttribute.Enabled = value;
		}
	}

	public string DisplayName
	{
		get { return UIAttribute?.DisplayName; }
		set
		{
			if (UIAttribute != null)
				UIAttribute.DisplayName = value;
		}
	}

	public string FieldClass
	{
		get { return UIAttribute?.FieldClass; }
		set
		{
			if (UIAttribute != null)
				UIAttribute.FieldClass = value;
		}
	}

	public bool Required
	{
		get { return UIAttribute?.Required ?? false; }
		set
		{
			if (UIAttribute != null)
				UIAttribute.Required = value;
		}
	}

	public virtual int TabOrder
	{
		get { return UIAttribute?.TabOrder ?? _FieldOrdinal; }
		set
		{
			if (UIAttribute != null)
				UIAttribute.TabOrder = value;
		}
	}

	public virtual PXErrorHandling ErrorHandling
	{
		get { return UIAttribute?.ErrorHandling ?? PXErrorHandling.WhenVisible; }
		set
		{
			if (UIAttribute != null)
				UIAttribute.ErrorHandling = value;
		}
	}
	#endregion

	#region SelectorAttribute delagation
	public virtual Type DescriptionField
	{
		get { return SelectorAttribute?.DescriptionField; }
		set
		{
			if (SelectorAttribute != null)
				SelectorAttribute.DescriptionField = value;
		}
	}

	public virtual bool DirtyRead
	{
		get { return SelectorAttribute?.DirtyRead ?? false; }
		set
		{
			if (SelectorAttribute != null)
				SelectorAttribute.DirtyRead = value;
		}
	}

	public virtual bool CacheGlobal
	{
		get { return SelectorAttribute?.CacheGlobal ?? false; }
		set
		{
			if (SelectorAttribute != null)
				SelectorAttribute.CacheGlobal = value;
		}
	}

	public virtual bool ValidComboRequired
	{
		get { return SelectorAttribute?.ValidComboRequired ?? false; }
		set
		{
			if (SelectorAttribute != null)
				SelectorAttribute.ValidComboRequired = value;
		}
	}

	public virtual bool Filterable
	{
		get { return SelectorAttribute?.Filterable ?? false; }
		set
		{
			if (SelectorAttribute != null)
				SelectorAttribute.Filterable = value;
		}
	}
	#endregion

	#region DefaultAttribute delagation
	public virtual PXPersistingCheck PersistingCheck
	{
		get { return DefaultAttribute?.PersistingCheck ?? PXPersistingCheck.Nothing; }
		set
		{
			if (DefaultAttribute != null)
				DefaultAttribute.PersistingCheck = value;
		}
	}
	#endregion

	#region Implementation

	public virtual void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
	{
		e.Expr = new Data.SQLTree.Constant(string.Empty);
		e.Cancel = true;
	}



	public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
	{
		sender.SetValue(e.Row, _FieldOrdinal, null);
	}


	#endregion

	#region Initialization

	public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
	{
		if (typeof(ISubscriber) == typeof(IPXCommandPreparingSubscriber)
			|| typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber))
		{
			if (IsDBField == false)
			{
				if (NonDBAttribute == null)
				{
					subscribers.Add(this as ISubscriber);
				}
				else
				{
					if (typeof(ISubscriber) == typeof(IPXRowSelectingSubscriber))
					{
						subscribers.Add(this as ISubscriber);
					}
					else
					{
						NonDBAttribute.GetSubscriber(subscribers);
					}
				}

				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (i != _DBAttrIndex && i != _NonDBAttrIndex)
					{
						_Attributes[i].GetSubscriber(subscribers);
					}
				}
			}
			else
			{
				base.GetSubscriber(subscribers);

				if (NonDBAttribute != null)
				{
					subscribers.Remove(NonDBAttribute as ISubscriber);
				}

				subscribers.Remove(this as ISubscriber);
			}
		}
		else
		{
			base.GetSubscriber(subscribers);

			if (NonDBAttribute != null)
			{
				subscribers.Remove(NonDBAttribute as ISubscriber);
			}
		}
	}
	#endregion

	#region IPXInterfaceField Members
	private IPXInterfaceField PXInterfaceField => UIAttribute;

	public string ErrorText
	{
		get { return PXInterfaceField?.ErrorText; }
		set
		{
			if (PXInterfaceField != null)
				PXInterfaceField.ErrorText = value;
		}
	}

	public object ErrorValue
	{
		get { return PXInterfaceField?.ErrorValue; }
		set
		{
			if (PXInterfaceField != null)
				PXInterfaceField.ErrorValue = value;
		}
	}

	public PXErrorLevel ErrorLevel
	{
		get { return PXInterfaceField?.ErrorLevel ?? PXErrorLevel.Undefined; }
		set
		{
			if (PXInterfaceField != null)
				PXInterfaceField.ErrorLevel = value;
		}
	}

	public PXCacheRights MapEnableRights
	{
		get { return PXInterfaceField?.MapEnableRights ?? PXCacheRights.Select; }
		set
		{
			if (PXInterfaceField != null)
				PXInterfaceField.MapEnableRights = value;
		}
	}

	public PXCacheRights MapViewRights
	{
		get { return PXInterfaceField?.MapViewRights ?? PXCacheRights.Select; }
		set
		{
			if (PXInterfaceField != null)
				PXInterfaceField.MapViewRights = value;
		}
	}

	public bool ViewRights => PXInterfaceField?.ViewRights ?? true;

	public void ForceEnabled() => PXInterfaceField?.ForceEnabled();

	#endregion

}

/// <summary>
/// Branch Field.
/// </summary>
/// <remarks>In case your DAC  supports multiple branches add this attribute to the Branch field of your DAC.</remarks>
[PXRestrictor(typeof(Where<Branch.active, Equal<True>>), Messages.BranchInactive)]
public class BranchAttribute : BranchBaseAttribute
{
	public BranchAttribute(Type sourceType)
		: base(sourceType, addDefaultAttribute: true)
	{
	}

	protected BranchAttribute(Type sourceType, Type searchType)
		: base(sourceType, searchType)
	{
	}

	public BranchAttribute()
		: base()
	{
	}
}

[PXDBInt()]
[PXInt]
[PXUIField(DisplayName = "Branch", FieldClass = _FieldClass)]
public abstract class BranchBaseAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber
{
	public const string _FieldClass = "BRANCH";
	public const string _DimensionName = "BIZACCT";
	private bool _IsDetail = true;
	private bool _Suppress = false;

	public bool IsDetail
	{
		get
		{
			return this._IsDetail;
		}
		set
		{
			this._IsDetail = value;
		}
	}

	public bool IsEnabledWhenOneBranchIsAccessible { get; set; }

	public BranchBaseAttribute(Type sourceType, bool addDefaultAttribute = true)
		: this(sourceType,
				typeof(Search2<Branch.branchID,
							InnerJoin<Organization,
								On<Branch.organizationID, Equal<Organization.organizationID>>>,
							Where<MatchWithBranch<Branch.branchID>>>))
	{
		if (addDefaultAttribute)
		{
			_Attributes.Add(sourceType != null ? new PXDefaultAttribute(sourceType) : new PXDefaultAttribute());
		}

		Initialize();
	}

	protected BranchBaseAttribute(Type sourceType, Type searchType)
		: base()
	{
		if (sourceType == null || !typeof(IBqlField).IsAssignableFrom(sourceType) || sourceType == typeof(AccessInfo.branchID))
		{
			IsDetail = false;
		}

		if (IsDetail)
		{
			_Attributes.Add(new PXRestrictorAttribute(BqlCommand.Compose(
					typeof(Where2<,>),
					typeof(SameOrganizationBranch<,>),
					typeof(Branch.branchID),
					typeof(Current<>), sourceType,
					typeof(Or<>), typeof(FeatureInstalled<FeaturesSet.interBranch>)),
				Messages.InterBranchFeatureIsDisabled));
		}

		PXDimensionSelectorAttribute attr =
			new PXDimensionSelectorAttribute(_DimensionName,
												searchType,
												typeof(Branch.branchCD),
												typeof(Branch.branchCD), typeof(Branch.acctName), typeof(Branch.ledgerID), typeof(Organization.organizationName), typeof(Branch.defaultPrinter));
		attr.ValidComboRequired = true;
		attr.DescriptionField = typeof(Branch.acctName);
		_Attributes.Add(attr);
		_SelAttrIndex = _Attributes.Count - 1;

		Initialize();
	}

	public BranchBaseAttribute()
		: this(typeof(AccessInfo.branchID), addDefaultAttribute: true)
	{
	}

	public bool Suppress()
	{
		object[] ids = PXAccess.GetBranches();

		return (ids == null || ids.Length <= 1) && !IsEnabledWhenOneBranchIsAccessible;
	}

	public override void CacheAttached(PXCache sender)
	{
		base.CacheAttached(sender);

		_Suppress = Suppress();
	}

	public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
	{
		base.GetSubscriber<ISubscriber>(subscribers);
		if (typeof(ISubscriber) == typeof(IPXFieldSelectingSubscriber))
		{
			subscribers.Remove(this as ISubscriber);
			subscribers.Add(this as ISubscriber);
		}
	}

	public virtual void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
	{
		if (_Suppress && e.ReturnState is PXFieldState)
		{
			PXFieldState state = (PXFieldState)e.ReturnState;

			state.Enabled = false;
			if (_IsDetail)
			{
				state.Visible = false;
				state.Visibility = PXUIVisibility.Invisible;
			}
		}
	}
}

/// <summary>
/// Base Attribute for AccountCD field. Aggregates PXFieldAttribute, PXUIFieldAttribute and PXDimensionAttribute.
/// PXDimensionAttribute selector has no restrictions and returns all records.
/// </summary>
[PXDBString(10, IsUnicode = true, InputMask = "")]
[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
public sealed class AccountRawAttribute : AcctSubAttribute
{
	private string _DimensionName = "ACCOUNT";

	public AccountRawAttribute()
		: base()
	{
		PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
		attr.ValidComboRequired = false;
		_Attributes.Add(attr);
		_SelAttrIndex = _Attributes.Count - 1;
	}
}

/// <summary>
/// Base Attribute for SubCD field. Aggregates PXFieldAttribute, PXUIFieldAttribute and PXDimensionAttribute.
/// PXDimensionAttribute selector has no restrictions and returns all records.
/// </summary>
[PXDBString(30, IsUnicode = true, InputMask = "")]
[PXUIField(DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible, FieldClass = _DimensionName)]
public class SubAccountRawAttribute : AcctSubAttribute
{
	protected const string _DimensionName = "SUBACCOUNT";

	public SubAccountRawAttribute()
		: base()
	{
		PXDimensionAttribute attr = new PXDimensionAttribute(_DimensionName);
		attr.ValidComboRequired = false;
		_Attributes.Add(attr);
		_SelAttrIndex = _Attributes.Count - 1;
	}

	public SubAccountRawAttribute(PX.Data.DimensionLookupMode lookupMode)
		: base()
	{
		PXDimensionValueLookupModeAttribute attr = new PXDimensionValueLookupModeAttribute(_DimensionName, lookupMode);
		attr.ValidComboRequired = false;
		_Attributes.Add(attr);
		_SelAttrIndex = _Attributes.Count - 1;
	}

	protected bool _SuppressValidation = false;
	public bool SuppressValidation
	{
		get
		{
			return this._SuppressValidation;
		}
		set
		{
			this._SuppressValidation = value;
		}
	}
	public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
	{
		base.GetSubscriber<ISubscriber>(subscribers);
		if (this._SuppressValidation)
		{
			if (typeof(ISubscriber) == typeof(IPXFieldVerifyingSubscriber) && (_SelAttrIndex >= 0))
			{
				subscribers.Remove(_Attributes[_SelAttrIndex] as ISubscriber);
			}
		}
	}
}

/// <summary>
/// Attribute suppress the dimension's lookup mode
/// </summary>
public sealed class PXDimensionValueLookupModeAttribute : PXDimensionAttribute
{

	public PX.Data.DimensionLookupMode LookupMode
	{
		get;
		set;
	}
	public PXDimensionValueLookupModeAttribute(string dimension, PX.Data.DimensionLookupMode lookupMode) : base(dimension)
	{
		LookupMode = lookupMode;
	}
	/// <exclude/>
	public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
	{
		if (_AttributeLevel == PXAttributeLevel.Item || e.IsAltered)
		{
			e.ReturnState = PXSegmentedState.CreateInstance(e.ReturnState, _FieldName, _Definition != null && _Definition.Dimensions.ContainsKey(_Dimension) ? _Definition.Dimensions[_Dimension] : new PXSegment[0],
				!(e.ReturnState is PXFieldState) || String.IsNullOrEmpty(((PXFieldState)e.ReturnState).ViewName) ? "_" + _Dimension + "_Segments_" : null,
				LookupMode,
				ValidComboRequired, _Wildcard);
			((PXSegmentedState)e.ReturnState).IsUnicode = true;
			((PXSegmentedState)e.ReturnState).DescriptionName = typeof(SegmentValue.descr).Name;
		}
	}
}
