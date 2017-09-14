#define USE_CACHE_EXTENSION

using System;
using System.Collections.Generic;
using System.Linq;

using PX.Common;

using PX.Data;
using PX.Data.EP;

using PX.TM;

using PX.Objects.AP.BQL;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;

using APQuickCheck = PX.Objects.AP.Standalone.APQuickCheck;
using CRLocation = PX.Objects.CR.Standalone.Location;
using IRegister = PX.Objects.CM.IRegister;

namespace PX.Objects.AP
{
    public class APDocType : ILabelProvider
    {
        public const string Invoice = "INV";
        public const string CreditAdj = "ACR";
        public const string DebitAdj = "ADR";
        public const string Check = "CHK";
        public const string VoidCheck = "VCK";
        public const string Prepayment = "PPM";
        public const string Refund = "REF";
        public const string QuickCheck = "QCK";
        public const string VoidQuickCheck = "VQC";

        protected static readonly IEnumerable<ValueLabelPair> _valueLabelPairs = new ValueLabelList
        {
            { Invoice, Messages.Invoice },
            { CreditAdj, Messages.CreditAdj },
            { DebitAdj, Messages.DebitAdj },
            { Check, Messages.Check },
            { VoidCheck, Messages.VoidCheck },
            { Prepayment, Messages.Prepayment },
            { Refund, Messages.Refund },
            { QuickCheck, Messages.QuickCheck },
            { VoidQuickCheck, Messages.VoidQuickCheck },
        };

        protected static readonly IEnumerable<ValueLabelPair> _valuePrintableLabelPairs = new ValueLabelList
        {
            { Invoice, Messages.PrintInvoice },
            { CreditAdj, Messages.PrintCreditAdj },
            { DebitAdj, Messages.PrintDebitAdj },
            { Check, Messages.PrintCheck },
            { VoidCheck, Messages.PrintVoidCheck },
            { Prepayment, Messages.PrintPrepayment },
            { Refund,   Messages.PrintRefund },
            { QuickCheck,   Messages.PrintQuickCheck },
            { VoidQuickCheck, Messages.PrintVoidQuickCheck },
        };

        public static readonly string[] Values = { Invoice, CreditAdj, DebitAdj, Check, VoidCheck, Prepayment, Refund, QuickCheck, VoidQuickCheck };
        public static readonly string[] Labels = { Messages.Invoice, Messages.CreditAdj, Messages.DebitAdj, Messages.Check, Messages.VoidCheck, Messages.Prepayment, Messages.Refund, Messages.QuickCheck, Messages.VoidQuickCheck };

        public IEnumerable<ValueLabelPair> ValueLabelPairs => _valueLabelPairs;

        public class ListAttribute : LabelListAttribute
        {
            public ListAttribute() : base(_valueLabelPairs)
            { }
        }

        /// <summary>
        /// Defines a Selector of the AP Document types with shorter description.<br/>
        /// In the screens displayed as combo-box.<br/>
        /// Mostly used in the reports.<br/>
        /// </summary>
        public class PrintListAttribute : LabelListAttribute
        {
            public PrintListAttribute() : base(_valuePrintableLabelPairs)
            { }
        }

        public class invoice : Constant<string>
        {
            public invoice() : base(Invoice) { }
        }

        public class creditAdj : Constant<string>
        {
            public creditAdj() : base(CreditAdj) { }
        }

        public class debitAdj : Constant<string>
        {
            public debitAdj() : base(DebitAdj) { }
        }

        public class check : Constant<string>
        {
            public check() : base(Check) { }
        }

        public class voidCheck : Constant<string>
        {
            public voidCheck() : base(VoidCheck) { }
        }

        public class prepayment : Constant<string>
        {
            public prepayment() : base(Prepayment) { }
        }

        public class refund : Constant<string>
        {
            public refund() : base(Refund) { }
        }

        public class quickCheck : Constant<string>
        {
            public quickCheck() : base(QuickCheck) { }
        }

        public class voidQuickCheck : Constant<string>
        {
            public voidQuickCheck() : base(VoidQuickCheck) { }
        }

        public static string DocClass(string DocType)
        {
            switch (DocType)
            {
                case Invoice:
                case CreditAdj:
                case DebitAdj:
                case QuickCheck:
                case VoidQuickCheck:
                    return GLTran.tranClass.Normal;
                case Check:
                case VoidCheck:
                case Refund:
                    return GLTran.tranClass.Payment;
                case Prepayment:
                    return GLTran.tranClass.Charge;
                default:
                    return null;
            }
        }

        public static bool? Payable(string DocType)
        {
            switch (DocType)
            {
                case Invoice:
                case CreditAdj:
                    return true;
                case Check:
                case DebitAdj:
                case VoidCheck:
                case Prepayment:
                case Refund:
                case QuickCheck:
                case VoidQuickCheck:
                    return false;
                default:
                    return null;
            }
        }

        public static Int16? SortOrder(string DocType)
        {
            switch (DocType)
            {
                case Invoice:
                case CreditAdj:
                case QuickCheck:
                    return 0;
                case Prepayment:
                    return 1;
                case DebitAdj:
                    return 2;
                case Check:
                    return 3;
                case VoidCheck:
                case VoidQuickCheck:
                    return 4;
                case Refund:
                    return 5;
                default:
                    return null;
            }
        }

        public static Decimal? SignBalance(string DocType)
        {
            switch (DocType)
            {
                case Refund:
                case Invoice:
                case CreditAdj:
                    return 1m;
                case DebitAdj:
                case Check:
                case VoidCheck:
                    return -1m;
                case Prepayment:
                    return -1m;
                case QuickCheck:
                case VoidQuickCheck:
                    return 0m;
                default:
                    return null;
            }
        }

        public static Decimal? SignAmount(string DocType)
        {
            switch (DocType)
            {
                case Refund:
                case Invoice:
                case CreditAdj:
                case QuickCheck:
                    return 1m;
                case DebitAdj:
                case Check:
                case VoidCheck:
                case VoidQuickCheck:
                    return -1m;
                case Prepayment:
                    return -1m;
                default:
                    return null;
            }
        }

        public static string TaxDrCr(string DocType)
        {
            switch (DocType)
            {
                //Invoice Types
                case Invoice:
                case CreditAdj:
                case QuickCheck:
                    return DrCr.Debit;
                case DebitAdj:
                case VoidQuickCheck:
                    return DrCr.Credit;
                //Payment Types
                case Check:
                case Prepayment:
                case VoidCheck:
                    return DrCr.Debit;
                case Refund:
                    return DrCr.Credit;
                default:
                    return DrCr.Debit;
            }
        }

        public static bool? HasNegativeAmount(string docType)
        {
            switch (docType)
            {
                case Refund:
                case Invoice:
                case CreditAdj:
                case QuickCheck:
                case DebitAdj:
                case Check:
                case Prepayment:
                case VoidQuickCheck:
                    return false;
                case VoidCheck:
                    return true;
                default:
                    return null;
            }
        }
    }

    public class APDocStatus
    {
        public static readonly string[] Values = { Hold, Balanced, Voided, Scheduled, Open, Closed, Printed, Prebooked, PendingApproval, Rejected, Reserved };
        public static readonly string[] Labels = { Messages.Hold, Messages.Balanced, Messages.Voided, Messages.Scheduled, Messages.Open, Messages.Closed, Messages.Printed, Messages.Prebooked, Messages.PendingApproval, Messages.Rejected, Messages.Reserved };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(Values, Labels) { }
        }

        public const string Hold = "H";
        public const string Balanced = "B";
        public const string Voided = "V";
        public const string Scheduled = "S";
        public const string Open = "N";
        public const string Closed = "C";
        public const string Printed = "P";
        public const string Prebooked = "K";
        public const string PendingApproval = "E";
        public const string Rejected = "R";
        public const string Reserved = "Z";

        public class hold : Constant<string>
        {
            public hold() : base(Hold) { }
        }

        public class balanced : Constant<string>
        {
            public balanced() : base(Balanced) { }
        }

        public class voided : Constant<string>
        {
            public voided() : base(Voided) { }
        }

        public class scheduled : Constant<string>
        {
            public scheduled() : base(Scheduled) { }
        }

        public class open : Constant<string>
        {
            public open() : base(Open) { }
        }

        public class closed : Constant<string>
        {
            public closed() : base(Closed) { }
        }

        public class printed : Constant<string>
        {
            public printed() : base(Printed) { }
        }

        public class prebooked : Constant<string>
        {
            public prebooked() : base(Prebooked) { }
        }

        public class pendingApproval : Constant<string>
        {
            public pendingApproval() : base(PendingApproval) { }
        }

        public class rejected : Constant<string>
        {
            public rejected() : base(Rejected) { }
        }

        public class reserved : Constant<string>
        {
            public reserved() : base(Reserved) { }
        }
    }

    /// <summary>
    /// An auxiliary DAC that is used in Accounts Payable and Accounts Receivable balance reports
    /// to properly join documents with their adjustments.
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.APAROrd)]
    public partial class APAROrd : PX.Data.IBqlTable
    {
        #region Ord
        public abstract class ord : PX.Data.IBqlField
        {
        }
        protected Int16? _Ord;

        /// <summary>
        /// [key] The field is used in reports for joining and filtering purposes.
        /// </summary>
        [PXDBShort(IsKey = true)]
        public virtual Int16? Ord
        {
            get
            {
                return this._Ord;
            }
            set
            {
                this._Ord = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// Primary DAC for the Accounts Payable documents.
    /// Includes fields common to all types of AP documents.
    /// </summary>
    [PXCacheName(Messages.Document)]
    [System.SerializableAttribute()]
    [PXPrimaryGraph(new Type[] {
        typeof(APQuickCheckEntry),
        typeof(TX.TXInvoiceEntry),
        typeof(APInvoiceEntry),
        typeof(APPaymentEntry)
    },
        new Type[] {
        typeof(Select<APQuickCheck,
            Where<APQuickCheck.docType, Equal<Current<APRegister.docType>>,
            And<APQuickCheck.refNbr, Equal<Current<APRegister.refNbr>>>>>),
        typeof(Select<APInvoice,
            Where<APInvoice.docType, Equal<Current<APRegister.docType>>,
            And<APInvoice.refNbr, Equal<Current<APRegister.refNbr>>,
            And<Where<APInvoice.released, Equal<False>, And<APInvoice.origModule, Equal<GL.BatchModule.moduleTX>>>>>>>),
        typeof(Select<APInvoice,
            Where<APInvoice.docType, Equal<Current<APRegister.docType>>,
            And<APInvoice.refNbr, Equal<Current<APRegister.refNbr>>>>>),
        typeof(Select<APPayment,
            Where<APPayment.docType, Equal<Current<APRegister.docType>>,
            And<APPayment.refNbr, Equal<Current<APRegister.refNbr>>>>>)
        })]
    public partial class APRegister : IBqlTable, IRegister, IBalance
    {
        #region Selected
        public abstract class selected : PX.Data.IBqlField
        {
        }
        protected bool? _Selected = false;

        /// <summary>
        /// Indicates whether the record is selected for mass processing or not.
        /// </summary>
        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
        #region Hidden
        public abstract class hidden : PX.Data.IBqlField
        {
        }
        protected bool? _Hidden = false;

        /// <summary>
        /// If set to <c>true</c>, this field indicates that the document represents a payment by a separate check.
        /// In this case the payment cannot be combined with other payments to the vendor.
        /// Applicable only in case <see cref="PX.Objects.CR.CRLocation.VSeparateCheck">VSeparateCheck</see> option
        /// is turned on for the <see cref="Vendor">Vendor</see>.
        /// </summary>
        [PXBool]
        [PXDefault(false)]
        public virtual bool? Hidden
        {
            get
            {
                return _Hidden;
            }
            set
            {
                _Hidden = value;
            }
        }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.IBqlField
        {
        }
        protected Int32? _BranchID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.GL.Branch">Branch</see>, to which the document belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.GL.Branch.BranchID">Branch.BranchID</see> field.
        /// </value>
        [GL.Branch()]
        public virtual Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion
        #region Passed
        public virtual bool? Passed
        {
            get;
            set;
        }
        #endregion
        #region DocType
        public abstract class docType : PX.Data.IBqlField
        {
        }
        protected String _DocType;

        /// <summary>
        /// [key] Type of the document.
        /// </summary>
        /// <value>
        /// Possible values are: "INV" - Invoice, "ACR" - Credit Adjustment, "ADR" - Debit Adjustment,
        /// "CHK" - Check, "VCK" - Void Check, "PPM" - Prepayment, "REF" - Vendor Refund,
        /// "QCK" - Quick Check, "VQC" - Void Quick Check.
        /// </value>
        [PXDBString(3, IsKey = true, IsFixed = true)]
        [PXDefault()]
        [APDocType.List()]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
        [PXFieldDescription]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }

        [PXString]
        [PXUIFieldAttribute(DisplayName = "Document Type (Internal)")]
        public string InternalDocType
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return DocType;
            }
        }
        #endregion
        #region PrintDocType
        public abstract class printDocType : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// Type of the document for displaying in reports.
        /// This field has the same set of possible internal values as the <see cref="DocType"/> field,
        /// but exposes different user-friendly values.
        /// </summary>
        /// <value>
        ///
        /// </value>
        [PXString(3, IsFixed = true)]
        [APDocType.PrintList()]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Enabled = true)]
        public virtual String PrintDocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
            }
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.IBqlField
        {
        }
        protected String _RefNbr;

        /// <summary>
        /// [key] Reference number of the document.
        /// </summary>
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDefault()]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        [PXSelector(typeof(Search<APRegister.refNbr, Where<APRegister.docType, Equal<Optional<APRegister.docType>>>>), Filterable = true)]
        [PXFieldDescription]
        public virtual String RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
        #region OrigModule
        public abstract class origModule : PX.Data.IBqlField
        {
        }
        protected String _OrigModule;

        /// <summary>
        /// Module, from which the document originates.
        /// </summary>
        /// <value>
        /// Code of the module of the system. Defaults to "AP".
        /// Possible values are: "GL", "AP", "AR", "CM", "CA", "IN", "DR", "FA", "PM", "TX", "SO", "PO".
        /// </value>
        [PXDBString(2, IsFixed = true)]
        [PXDefault(GL.BatchModule.AP)]
        [PXUIField(DisplayName = "Source", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [GL.BatchModule.FullList()]
        public virtual String OrigModule
        {
            get
            {
                return this._OrigModule;
            }
            set
            {
                this._OrigModule = value;
            }
        }
        #endregion
        #region DocDate
        public abstract class docDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _DocDate;

        /// <summary>
        /// Date of the document.
        /// </summary>
        [PXDBDate()]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? DocDate
        {
            get
            {
                return this._DocDate;
            }
            set
            {
                this._DocDate = value;
            }
        }
        #endregion
        #region OrigDocDate
        public abstract class origDocDate : PX.Data.IBqlField
        {
        }
        protected DateTime? _OrigDocDate;

        /// <summary>
        /// Date of the original (source) document.
        /// </summary>
        [PXDBDate()]
        public virtual DateTime? OrigDocDate
        {
            get
            {
                return this._OrigDocDate;
            }
            set
            {
                this._OrigDocDate = value;
            }
        }
        #endregion
        #region TranPeriodID
        public abstract class tranPeriodID : PX.Data.IBqlField
        {
        }
        protected String _TranPeriodID;

        /// <summary>
        /// <see cref="FinPeriod">Financial Period</see> of the document.
        /// </summary>
        /// <value>
        /// Determined by the <see cref="APRegister.DocDate">date of the document</see>. Unlike <see cref="APRegister.FinPeriodID"/>
        /// the value of this field can't be overriden by user.
        /// </value>
        [TranPeriodID(typeof(APRegister.docDate))]
        [PXUIField(DisplayName = "Transaction Period")]
        public virtual String TranPeriodID
        {
            get
            {
                return this._TranPeriodID;
            }
            set
            {
                this._TranPeriodID = value;
            }
        }
        #endregion
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.IBqlField
        {
        }
        protected String _FinPeriodID;

        /// <summary>
        /// <see cref="FinPeriod">Financial Period</see> of the document.
        /// </summary>
        /// <value>
        /// Defaults to the period, to which the <see cref="APRegister.DocDate"/> belongs, but can be overriden by user.
        /// </value>
        [APOpenPeriod(typeof(APRegister.docDate))]
        [PXDefault()]
        [PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.IBqlField
        {
        }
        /// <summary>
        /// Identifier of the <see cref="Vendor"/>, whom the document belongs to.
        /// </summary>
        [VendorActive(
            Visibility = PXUIVisibility.SelectorVisible,
            DescriptionField = typeof(Vendor.acctName),
            CacheGlobal = true,
            Filterable = true)]
        [PXDefault]
        public virtual int? VendorID
        {
            get;
            set;
        }
        #endregion
        #region VendorID_Vendor_acctName
        public abstract class vendorID_Vendor_acctName : PX.Data.IBqlField
        {
        }
        #endregion
        #region VendorLocationID
        public abstract class vendorLocationID : PX.Data.IBqlField
        {
        }
        protected Int32? _VendorLocationID;

        /// <summary>
        /// Identifier of the <see cref="Location">Location</see> of the <see cref="Vendor">Vendor</see>, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Location.LocationID"/> field. Defaults to vendor's <see cref="Vendor.DefLocationID">default location</see>.
        /// </value>
        [LocationID(
            typeof(Where<Location.bAccountID, Equal<Optional<APRegister.vendorID>>,
                And<Location.isActive, Equal<True>,
                And<MatchWithBranch<Location.vBranchID>>>>),
            DescriptionField = typeof(Location.descr),
            Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Coalesce<
            Search2<Vendor.defLocationID,
            InnerJoin<CRLocation,
                On<CRLocation.locationID, Equal<Vendor.defLocationID>,
                And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
            Where<Vendor.bAccountID, Equal<Current<APRegister.vendorID>>,
                And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>,
            Search<CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<APRegister.vendorID>>,
            And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>))]
        public virtual Int32? VendorLocationID
        {
            get
            {
                return this._VendorLocationID;
            }
            set
            {
                this._VendorLocationID = value;
            }
        }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.IBqlField
        {
        }
        protected String _CuryID;

        /// <summary>
        /// Code of the <see cref="PX.Objects.CM.Currency">Currency</see> of the document.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Company.BaseCuryID">company's base currency</see>.
        /// </value>
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
        [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<Company.baseCuryID>))]
        [PXSelector(typeof(Currency.curyID))]
        public virtual String CuryID
        {
            get
            {
                return this._CuryID;
            }
            set
            {
                this._CuryID = value;
            }
        }
        #endregion
        #region APAccountID
        public abstract class aPAccountID : PX.Data.IBqlField
        {
        }
        protected Int32? _APAccountID;

        /// <summary>
        /// Identifier of the AP account, to which the document belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
        [PXDefault]
        [Account(typeof(APRegister.branchID), typeof(Search<Account.accountID,
                    Where2<Match<Current<AccessInfo.userName>>,
                         And<Account.active, Equal<True>,
                         And<Account.isCashAccount, Equal<False>,
                         And<Where<Current<GLSetup.ytdNetIncAccountID>, IsNull,
                          Or<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>), DisplayName = "AP Account")]
        public virtual Int32? APAccountID
        {
            get
            {
                return this._APAccountID;
            }
            set
            {
                this._APAccountID = value;
            }
        }
        #endregion
        #region APSubID
        public abstract class aPSubID : PX.Data.IBqlField
        {
        }
        protected Int32? _APSubID;

        /// <summary>
        /// Identifier of the AP subaccount, to which the document belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
        [PXDefault]
        [SubAccount(typeof(APRegister.aPAccountID), typeof(APRegister.branchID), true, DescriptionField = typeof(Sub.description), DisplayName = "AP Subaccount", Visibility = PXUIVisibility.Visible)]
        public virtual Int32? APSubID
        {
            get
            {
                return this._APSubID;
            }
            set
            {
                this._APSubID = value;
            }
        }
        #endregion
        #region LineCntr
        public abstract class lineCntr : PX.Data.IBqlField
        {
        }
        protected Int32? _LineCntr;

        /// <summary>
        /// Counter of the document lines, used <i>internally</i> to assign numbers to newly created lines.
        /// It is not recommended to rely on this fields to determine the exact count of lines, because it might not reflect the latter under various conditions.
        /// </summary>
        [PXDBInt()]
        [PXDefault(0)]
        public virtual Int32? LineCntr
        {
            get
            {
                return this._LineCntr;
            }
            set
            {
                this._LineCntr = value;
            }
        }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.IBqlField
        {
        }
        protected Int64? _CuryInfoID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> object associated with the document.
        /// </summary>
        /// <value>
        /// Generated automatically. Corresponds to the <see cref="PX.Objects.CM.CurrencyInfo.CurrencyInfoID"/> field.
        /// </value>
        [PXDBLong()]
        [CurrencyInfo(ModuleCode = "AP")]
        public virtual Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
        #region CuryOrigDocAmt
        public abstract class curyOrigDocAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryOrigDocAmt;

        /// <summary>
        /// The amount to be paid for the document in the currency of the document. (See <see cref="CuryID"/>)
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.origDocAmt))]
        [PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? CuryOrigDocAmt
        {
            get
            {
                return this._CuryOrigDocAmt;
            }
            set
            {
                this._CuryOrigDocAmt = value;
            }
        }
        #endregion
        #region OrigDocAmt
        public abstract class origDocAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _OrigDocAmt;

        /// <summary>
        /// The amount to be paid for the document in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Amount")]
        public virtual Decimal? OrigDocAmt
        {
            get
            {
                return this._OrigDocAmt;
            }
            set
            {
                this._OrigDocAmt = value;
            }
        }
        #endregion
        #region CuryDocBal
        public abstract class curyDocBal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDocBal;

        /// <summary>
        /// The balance of the Accounts Payable document after tax (if inclusive) and the discount in the currency of the document. (See <see cref="CuryID"/>)
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.docBal), BaseCalc = false)]
        [PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Decimal? CuryDocBal
        {
            get
            {
                return this._CuryDocBal;
            }
            set
            {
                this._CuryDocBal = value;
            }
        }
        #endregion
        #region DocBal
        public abstract class docBal : PX.Data.IBqlField
        {
        }
        protected Decimal? _DocBal;

        /// <summary>
        /// The balance of the Accounts Payable document after tax (if inclusive) and the discount in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DocBal
        {
            get
            {
                return this._DocBal;
            }
            set
            {
                this._DocBal = value;
            }
        }
        #endregion
        #region DiscTot
        public abstract class discTot : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscTot;

        /// <summary>
        /// Total discount associated with the document in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscTot
        {
            get
            {
                return this._DiscTot;
            }
            set
            {
                this._DiscTot = value;
            }
        }
        #endregion
        #region CuryDiscTot
        public abstract class curyDiscTot : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDiscTot;

        /// <summary>
        /// Total discount associated with the document in the currency of the document. (See <see cref="CuryID"/>)
        /// </summary>
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.discTot))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Discount Total", Enabled = true)]
        public virtual Decimal? CuryDiscTot
        {
            get
            {
                return this._CuryDiscTot;
            }
            set
            {
                this._CuryDiscTot = value;
            }
        }
        #endregion
        #region DocDisc
        public abstract class docDisc : PX.Data.IBqlField
        {
        }
        protected Decimal? _DocDisc;
        [PXBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? DocDisc
        {
            get
            {
                return this._DocDisc;
            }
            set
            {
                this._DocDisc = value;
            }
        }
        #endregion
        #region CuryDocDisc
        public abstract class curyDocDisc : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDocDisc;
        [PXCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.docDisc))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Document Discount", Enabled = true)]
        public virtual Decimal? CuryDocDisc
        {
            get
            {
                return this._CuryDocDisc;
            }
            set
            {
                this._CuryDocDisc = value;
            }
        }
        #endregion
        #region CuryOrigDiscAmt
        public abstract class curyOrigDiscAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryOrigDiscAmt;

        /// <summary>
        /// !REV! The amount of the cash discount taken for the original document.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.origDiscAmt))]
        [PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? CuryOrigDiscAmt
        {
            get
            {
                return this._CuryOrigDiscAmt;
            }
            set
            {
                this._CuryOrigDiscAmt = value;
            }
        }
        #endregion
        #region OrigDiscAmt
        public abstract class origDiscAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _OrigDiscAmt;

        /// <summary>
        /// The amount of the cash discount taken for the original document.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? OrigDiscAmt
        {
            get
            {
                return this._OrigDiscAmt;
            }
            set
            {
                this._OrigDiscAmt = value;
            }
        }
        #endregion
        #region CuryDiscTaken
        public abstract class curyDiscTaken : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDiscTaken;

        /// <summary>
        /// !REV! The amount of the cash discount taken.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.discTaken))]
        public virtual Decimal? CuryDiscTaken
        {
            get
            {
                return this._CuryDiscTaken;
            }
            set
            {
                this._CuryDiscTaken = value;
            }
        }
        #endregion
        #region DiscTaken
        public abstract class discTaken : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscTaken;

        /// <summary>
        /// The amount of the cash discount taken.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscTaken
        {
            get
            {
                return this._DiscTaken;
            }
            set
            {
                this._DiscTaken = value;
            }
        }
        #endregion
        #region CuryDiscBal
        public abstract class curyDiscBal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryDiscBal;

        /// <summary>
        /// The difference between the cash discount that was available and the actual amount of cash discount taken.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.discBal), BaseCalc = false)]
        [PXUIField(DisplayName = "Cash Discount Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Decimal? CuryDiscBal
        {
            get
            {
                return this._CuryDiscBal;
            }
            set
            {
                this._CuryDiscBal = value;
            }
        }
        #endregion
        #region DiscBal
        public abstract class discBal : PX.Data.IBqlField
        {
        }
        protected Decimal? _DiscBal;

        /// <summary>
        /// The difference between the cash discount that was available and the actual amount of cash discount taken.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscBal
        {
            get
            {
                return this._DiscBal;
            }
            set
            {
                this._DiscBal = value;
            }
        }
        #endregion
        #region CuryOrigWhTaxAmt
        public abstract class curyOrigWhTaxAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryOrigWhTaxAmt;

        /// <summary>
        /// The amount of withholding tax calculated for the document, if applicable, in the currency of the document. (See <see cref="CuryID"/>)
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.origWhTaxAmt))]
        [PXUIField(DisplayName = "With. Tax", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Decimal? CuryOrigWhTaxAmt
        {
            get
            {
                return this._CuryOrigWhTaxAmt;
            }
            set
            {
                this._CuryOrigWhTaxAmt = value;
            }
        }
        #endregion
        #region OrigWhTaxAmt
        public abstract class origWhTaxAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _OrigWhTaxAmt;

        /// <summary>
        /// The amount of withholding tax calculated for the document, if applicable, in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? OrigWhTaxAmt
        {
            get
            {
                return this._OrigWhTaxAmt;
            }
            set
            {
                this._OrigWhTaxAmt = value;
            }
        }
        #endregion
        #region CuryWhTaxBal
        public abstract class curyWhTaxBal : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryWhTaxBal;

        /// <summary>
        /// !REV! The difference between the original amount of withholding tax to be payed and the amount that was actually paid.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.whTaxBal), BaseCalc = false)]
        public virtual Decimal? CuryWhTaxBal
        {
            get
            {
                return this._CuryWhTaxBal;
            }
            set
            {
                this._CuryWhTaxBal = value;
            }
        }
        #endregion
        #region WhTaxBal
        public abstract class whTaxBal : PX.Data.IBqlField
        {
        }
        protected Decimal? _WhTaxBal;

        /// <summary>
        /// The difference between the original amount of withholding tax to be payed and the amount that was actually paid.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? WhTaxBal
        {
            get
            {
                return this._WhTaxBal;
            }
            set
            {
                this._WhTaxBal = value;
            }
        }
        #endregion
        #region CuryTaxWheld
        public abstract class curyTaxWheld : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryTaxWheld;

        /// <summary>
        /// !REV! The amount of tax withheld from the payments to the document.
        /// (Presented in the currency of the document, see <see cref="CuryID"/>)
        /// </summary>
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.taxWheld))]
        public virtual Decimal? CuryTaxWheld
        {
            get
            {
                return this._CuryTaxWheld;
            }
            set
            {
                this._CuryTaxWheld = value;
            }
        }
        #endregion
        #region TaxWheld
        public abstract class taxWheld : PX.Data.IBqlField
        {
        }
        protected Decimal? _TaxWheld;

        /// <summary>
        /// The amount of tax withheld from the payments to the document.
        /// (Presented in the base currency of the company, see <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? TaxWheld
        {
            get
            {
                return this._TaxWheld;
            }
            set
            {
                this._TaxWheld = value;
            }
        }
        #endregion
        #region CuryChargeAmt
        public abstract class curyChargeAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _CuryChargeAmt;

        /// <summary>
        /// The amount of charges associated with the document in the currency of the document. (See <see cref="CuryID"/>)
        /// </summary>
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.chargeAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Finance Charges", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Decimal? CuryChargeAmt
        {
            get
            {
                return this._CuryChargeAmt;
            }
            set
            {
                this._CuryChargeAmt = value;
            }
        }
        #endregion
        #region ChargeAmt
        public abstract class chargeAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _ChargeAmt;

        /// <summary>
        /// The amount of charges associated with the document in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
        /// </summary>
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? ChargeAmt
        {
            get
            {
                return this._ChargeAmt;
            }
            set
            {
                this._ChargeAmt = value;
            }
        }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.IBqlField
        {
        }
        protected String _DocDesc;

        /// <summary>
        /// Description of the document.
        /// </summary>
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String DocDesc
        {
            get
            {
                return this._DocDesc;
            }
            set
            {
                this._DocDesc = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.IBqlField
        {
        }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.IBqlField
        {
        }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.IBqlField
        {
        }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.IBqlField
        {
        }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.IBqlField
        {
        }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        #region DocClass
        public abstract class docClass : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// Class of the document. This field is calculated based on the <see cref="DocType"/>.
        /// </summary>
        /// <value>
        /// Possible values are: "N" - for Invoice, Credit Adjustment, Debit Adjustment, Quick Check and Void Quick Check; "P" - for Check, Void Check and Refund; "U" - for Prepayment.
        /// </value>
        [PXString(1, IsFixed = true)]
        public virtual string DocClass
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return APDocType.DocClass(this._DocType);
            }
            set
            {
            }
        }
        #endregion
        #region BatchNbr
        public abstract class batchNbr : PX.Data.IBqlField
        {
        }
        protected String _BatchNbr;

        /// <summary>
        /// Number of the <see cref="Batch"/>, generated for the document on release.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Batch.BatchNbr">Batch.BatchNbr</see> field.
        /// </value>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleAP>>>))]
        public virtual String BatchNbr
        {
            get
            {
                return this._BatchNbr;
            }
            set
            {
                this._BatchNbr = value;
            }
        }
        #endregion
        #region PrebookBatchNbr
        public abstract class prebookBatchNbr : PX.Data.IBqlField
        {
        }
        protected String _PrebookBatchNbr;

        /// <summary>
        /// Stores the number of the <see cref="Batch"/> generated during prebooking.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Batch.BatchNbr">Batch.BatchNbr</see> field.
        /// </value>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Pre-Releasing Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXSelector(typeof(Batch.batchNbr))]
        public virtual String PrebookBatchNbr
        {
            get
            {
                return this._PrebookBatchNbr;
            }
            set
            {
                this._PrebookBatchNbr = value;
            }
        }
        #endregion
        #region VoidBatchNbr
        public abstract class voidBatchNbr : PX.Data.IBqlField
        {
        }
        protected String _VoidBatchNbr;

        /// <summary>
        /// Stores the number of the <see cref="Batch"/> generated when the document was voided.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Batch.BatchNbr">Batch.BatchNbr</see> field.
        /// </value>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Void Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXSelector(typeof(Batch.batchNbr))]
        public virtual String VoidBatchNbr
        {
            get
            {
                return this._VoidBatchNbr;
            }
            set
            {
                this._VoidBatchNbr = value;
            }
        }
        #endregion
        #region Released
        public abstract class released : PX.Data.IBqlField
        {
        }
        protected Boolean? _Released;

        /// <summary>
        /// When set to <c>true</c> indicates that the document was released.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Released", Visible = false)]
        public virtual Boolean? Released
        {
            get
            {
                return this._Released;
            }
            set
            {
                this._Released = value;
            }
        }
        #endregion
        #region OpenDoc
        public abstract class openDoc : PX.Data.IBqlField
        {
        }
        protected Boolean? _OpenDoc;

        /// <summary>
        /// When set to <c>true</c> indicates that the document is open.
        /// </summary>
        [PXDBBool()]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Open", Visible = false)]
        public virtual Boolean? OpenDoc
        {
            get
            {
                return this._OpenDoc;
            }
            set
            {
                this._OpenDoc = value;
            }
        }
        #endregion
        #region Hold
        public abstract class hold : PX.Data.IBqlField
        {
        }
        protected Boolean? _Hold;

        /// <summary>
        /// When set to <c>true</c> indicates that the document is on hold and thus cannot be released.
        /// </summary>
        [PXDBBool()]
        [PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true, typeof(APSetup.holdEntry))]
        public virtual Boolean? Hold
        {
            get
            {
                return this._Hold;
            }
            set
            {
                this._Hold = value;
            }
        }
        #endregion
        #region Scheduled
        public abstract class scheduled : PX.Data.IBqlField
        {
        }
        protected Boolean? _Scheduled;

        /// <summary>
        /// When set to <c>true</c> indicates that the document is part of a <c>Schedule</c> and serves as a template for generating other documents according to it.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Scheduled
        {
            get
            {
                return this._Scheduled;
            }
            set
            {
                this._Scheduled = value;
            }
        }
        #endregion
        #region Voided
        public abstract class voided : PX.Data.IBqlField
        {
        }
        protected Boolean? _Voided;

        /// <summary>
        /// When set to <c>true</c> indicates that the document was voided. In this case <see cref="VoidBatchNbr"/> field will hold the number of the voiding <see cref="Batch"/>.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Void", Visible = false)]
        public virtual Boolean? Voided
        {
            get
            {
                return this._Voided;
            }
            set
            {
                this._Voided = value;
            }
        }
        #endregion
        #region Printed
        public abstract class printed : PX.Data.IBqlField
        {
        }
        protected Boolean? _Printed;

        /// <summary>
        /// When set to <c>true</c> indicates that the document was printed.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? Printed
        {
            get
            {
                return this._Printed;
            }
            set
            {
                this._Printed = value;
            }
        }
        #endregion
        #region Prebooked
        public abstract class prebooked : PX.Data.IBqlField
        {
        }
        protected Boolean? _Prebooked;

        /// <summary>
        /// When set to <c>true</c> indicates that the document was prebooked.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prebooked")]
        public virtual Boolean? Prebooked
        {
            get
            {
                return this._Prebooked;
            }
            set
            {
                this._Prebooked = value;
            }
        }
        #endregion
        #region Approved
        public abstract class approved : PX.Data.IBqlField
        {
        }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? Approved
        {
            get;
            set;
        }
        #endregion
        #region Rejected
        public abstract class rejected : IBqlField
        {
        }
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? Rejected
        {
            get;
            set;
        }
        #endregion
        #region RequestApproval
        public abstract class requestApproval : PX.Data.IBqlField
        {
        }
        /// <summary>
        /// Indicates that <see cref="APSetup"/> requests approval
        /// for AP documents.
        /// </summary>
        [PXBool]
        [PXDefault(typeof(Search<APSetup.invoiceRequestApproval>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBScalar(typeof(Search<APSetup.invoiceRequestApproval>))]
        [PXUIField(DisplayName = "Request Approval", Visible = false)]
        public virtual bool? RequestApproval
        {
            get;
            set;
        }
        #endregion
        #region DontApprove
        public abstract class dontApprove : PX.Data.IBqlField
        {
        }
        /// <summary>
        /// Indicates that the current document should be excluded from the
        /// approval process.
        /// </summary>
        [PXBool]
        [PXFormula(typeof(Switch<
            Case<Where<
                APRegister.origModule, Equal<BatchModule.moduleEP>,
                Or<IsTableEmpty<APSetupApproval>, Equal<True>>>,
                True,
            Case<Where<
                APRegister.requestApproval, Equal<True>>,
                False>>,
            True>))]
        [PXUIField(DisplayName = "Don't Approve", Visible = false, Enabled = false)]
        public virtual bool? DontApprove
        {
            get;
            set;
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.IBqlField
        {
        }
        protected Guid? _NoteID;

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field.
        /// </value>
        [PXNote(DescriptionField = typeof(APRegister.refNbr))]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
            }
        }
        #endregion
        #region RefNoteID
        public abstract class refNoteID : PX.Data.IBqlField
        {
        }
        protected Guid? _RefNoteID;

        /// <summary>
        /// !REV!
        /// </summary>
        [PXDBGuid()]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        #endregion
        #region ClosedFinPeriodID
        public abstract class closedFinPeriodID : PX.Data.IBqlField
        {
        }
        protected String _ClosedFinPeriodID;

        /// <summary>
        /// The <see cref="PX.Objects.GL.FinancialPeriod">Financial Period</see>, in which the document was closed.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="FinPeriodID"/> field.
        /// </value>
        [FinPeriodID()]
        [PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
        public virtual String ClosedFinPeriodID
        {
            get
            {
                return this._ClosedFinPeriodID;
            }
            set
            {
                this._ClosedFinPeriodID = value;
            }
        }
        #endregion
        #region ClosedTranPeriodID
        public abstract class closedTranPeriodID : PX.Data.IBqlField
        {
        }
        protected String _ClosedTranPeriodID;

        /// <summary>
        /// The <see cref="PX.Objects.GL.FinancialPeriod">Financial Period</see>, in which the document was closed.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="TranPeriodID"/> field.
        /// </value>
        [FinPeriodID()]
        [PXUIField(DisplayName = "Closed Period", Visibility = PXUIVisibility.Invisible)]
        public virtual String ClosedTranPeriodID
        {
            get
            {
                return this._ClosedTranPeriodID;
            }
            set
            {
                this._ClosedTranPeriodID = value;
            }
        }
        #endregion
        #region RGOLAmt
        public abstract class rGOLAmt : PX.Data.IBqlField
        {
        }
        protected Decimal? _RGOLAmt;

        /// <summary>
        /// Realized Gain and Loss amount associated with the document.
        /// </summary>
        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? RGOLAmt
        {
            get
            {
                return this._RGOLAmt;
            }
            set
            {
                this._RGOLAmt = value;
            }
        }
        #endregion
        #region CuryRoundDiff
        public abstract class curyRoundDiff : IBqlField { }

        /// <summary>
        /// The difference between the original amount and the rounded amount in the currency of the document. (See <see cref="CuryID"/>)
        /// (Applicable only in case <see cref="PX.Objects.CS.FeaturesSet.InvoiceRounding">Invoice Rounding</see> feature is on.)
        /// </summary>
        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.roundDiff), BaseCalc = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Rounding Diff.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public decimal? CuryRoundDiff
        {
            get;
            set;
        }
        #endregion
        #region RoundDiff
        public abstract class roundDiff : IBqlField { }

        /// <summary>
        /// The difference between the original amount and the rounded amount in the base currency of the company. (See <see cref="Company.BaseCuryID"/>)
        /// (Applicable only in case <see cref="PX.Objects.CS.FeaturesSet.InvoiceRounding">Invoice Rounding</see> feature is on.)
        /// </summary>
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public decimal? RoundDiff
        {
            get;
            set;
        }
        #endregion
        #region CuryTaxRoundDiff
        public abstract class curyTaxRoundDiff : IBqlField { }

        [PXDBCurrency(typeof(APRegister.curyInfoID), typeof(APRegister.taxRoundDiff), BaseCalc = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Rounding Diff.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public decimal? CuryTaxRoundDiff
        {
            get;
            set;
        }
        #endregion
        #region TaxRoundDiff
        public abstract class taxRoundDiff : IBqlField { }

        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public decimal? TaxRoundDiff
        {
            get;
            set;
        }
        #endregion
        #region Payable

        /// <summary>
        /// Read-only field indicating whether the document is payable. Depends solely on the <see cref="DocType">APRegister.DocType</see> field.
        /// Opposite to <see cref="Paying"/> field.
        /// </summary>
        /// <value>
        /// <c>true</c> - for payable documents, e.g. bills; <c>false</c> - for paying, e.g. checks.
        /// </value>
        public virtual Boolean? Payable
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return APDocType.Payable(this._DocType);
            }
            set
            {
            }
        }
        #endregion
        #region Paying

        /// <summary>
        /// Read-only field indicating whether the document is paying. Depends solely on the <see cref="DocType">APRegister.DocType</see> field.
        /// Opposite to <see cref="Payable"/> field.
        /// </summary>
        /// <value>
        /// <c>true</c> - for paying documents, e.g. checks; <c>false</c> - for payable ones, e.g. bills.
        /// </value>
        public virtual Boolean? Paying
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return (APDocType.Payable(this._DocType) == false);
            }
            set
            {
            }
        }
        #endregion
        #region SortOrder

        /// <summary>
        /// Read-only field determining the sort order for AP documents based on the <see cref="DocType"/> field.
        /// </summary>
        public virtual Int16? SortOrder
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return APDocType.SortOrder(this._DocType);
            }
            set
            {
            }
        }
        #endregion
        #region SignBalance

        /// <summary>
        /// Read-only field indicating the sign of the document's impact on AP balance .
        /// Depends solely on the <see cref="DocType"/>
        /// </summary>
        /// <value>
        /// Can be <c>1</c>, <c>-1</c> or <c>0</c>.
        /// </value>
        public virtual Decimal? SignBalance
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return APDocType.SignBalance(this._DocType);
            }
            set
            {
            }
        }
        #endregion
        #region SignAmount

        /// <summary>
        /// Read-only field indicating the sign of the document amount.
        /// Depends solely on the <see cref="DocType"/>
        /// </summary>
        /// <value>
        /// Can be <c>1</c>, <c>-1</c> or <c>0</c>.
        /// </value>
        public virtual Decimal? SignAmount
        {
            [PXDependsOnFields(typeof(docType))]
            get
            {
                return APDocType.SignAmount(this._DocType);
            }
            set
            {
            }
        }
        #endregion
        #region Status
        public abstract class status : IBqlField { }
        protected string _Status;

        /// <summary>
        /// Status of the document. The field is calculated based on the values of status flag. It can't be changed directly.
        /// The fields tht determine status of a document are: <see cref="Hold"/>, <see cref="Released"/>, <see cref="Voided"/>,
        /// <see cref="Scheduled"/>, <see cref="Prebooked"/>, <see cref="Printed"/>, <see cref="Approved"/>, <see cref="Rejected"/>.
        /// </summary>
        /// <value>
        /// Possible values are:
        /// <c>"H"</c> - Hold, <c>"B"</c> - Balanced, <c>"V"</c> - Voided, <c>"S"</c> - Scheduled,
        /// <c>"N"</c> - Open, <c>"C"</c> - Closed, <c>"P"</c> - Printed, <c>"K"</c> - Prebooked,
        /// <c>"E"</c> - Pending Approval, <c>"R"</c> - Rejected, <c>"Z"</c> - Reserved.
        /// Defaults to Hold.
        /// </value>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(APDocStatus.Hold)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [APDocStatus.List]
        [SetStatus]
        [PXDependsOnFields(
            typeof(APRegister.voided),
            typeof(APRegister.hold),
            typeof(APRegister.scheduled),
            typeof(APRegister.released),
            typeof(APRegister.printed),
            typeof(APRegister.prebooked),
            typeof(APRegister.openDoc),
            typeof(APRegister.approved),
            typeof(APRegister.rejected))]
        public virtual string Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region Methods
        public class SetStatusAttribute : PXEventSubscriberAttribute, IPXRowUpdatingSubscriber, IPXRowInsertingSubscriber
        {
            public override void CacheAttached(PXCache sender)
            {
                base.CacheAttached(sender);

                sender.Graph.FieldUpdating.AddHandler(
                    sender.GetItemType(),
                    nameof(APRegister.hold),
                    (cache, e) =>
                    {
                        PXBoolAttribute.ConvertValue(e);

                        APRegister item = e.Row as APRegister;
                        if (item != null)
                        {
                            StatusSet(cache, item, (bool?)e.NewValue);
                        }
                    });

                sender.Graph.FieldVerifying.AddHandler(
                    sender.GetItemType(),
                    nameof(APRegister.status),
                    (cache, e) => { e.NewValue = cache.GetValue<APRegister.status>(e.Row); });

                sender.Graph.RowSelecting.AddHandler(sender.GetItemType(), RowSelecting);

                sender.Graph.RowSelected.AddHandler(
                    sender.GetItemType(),
                    (cache, e) =>
                    {
                        APRegister document = e.Row as APRegister;

                        if (document != null)
                        {
                            StatusSet(cache, document, document.Hold);
                        }
                    });
            }

            protected virtual void StatusSet(PXCache cache, APRegister item, bool? HoldVal)
            {
                if (item.Voided == true)
                {
                    item.Status = APDocStatus.Voided;
                }
                else if (item.Hold == true)
                {
                    if (item.Released == true)
                    {
                        item.Status = APDocStatus.Reserved;
                    }
                    else
                    {
                        item.Status = APDocStatus.Hold;
                    }
                }
                else if (item.Scheduled == true)
                {
                    item.Status = APDocStatus.Scheduled;
                }
                else if (item.Rejected == true)
                {
                    item.Status = APDocStatus.Rejected;
                }
                else if (item.Released != true)
                {
                    if (item.Printed == true && item.DocType == APDocType.Check)
                    {
                        item.Status = APDocStatus.Printed;
                    }
                    else if (item.Prebooked == true)
                    {
                        item.Status = APDocStatus.Prebooked;
                    }
                    else if (
                        item.Approved != true &&
                        item.DontApprove != true)
                    {
                        item.Status = APDocStatus.PendingApproval;
                    }
                    else
                    {
                        item.Status = APDocStatus.Balanced;
                    }
                }
                else if (item.OpenDoc == true)
                {
                    item.Status = APDocStatus.Open;
                }
                else if (item.OpenDoc == false)
                {
                    item.Status = APDocStatus.Closed;
                }
            }

            public virtual void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
            {
                APRegister item = (APRegister)e.Row;
                if (item != null)
                    StatusSet(sender, item, item.Hold);
            }

            public virtual void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
            {
                APRegister item = (APRegister)e.Row;
                StatusSet(sender, item, item.Hold);
            }

            public virtual void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
            {
                APRegister item = (APRegister)e.NewRow;
                StatusSet(sender, item, item.Hold);
            }
        }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : IBqlField
        {
        }
        protected string _ScheduleID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.GL.Schedule">Schedule</see> object, associated with the document.
        /// In case <see cref="Scheduled"/> is <c>true</c>, ScheduleID points to the Schedule, to which the document belongs as a template.
        /// Otherwise, ScheduleID points to the Schedule, from which this document was generated, if any.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Objects.GL.Schedule.ScheduleID"/> field.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
        public virtual string ScheduleID
        {
            get
            {
                return this._ScheduleID;
            }
            set
            {
                this._ScheduleID = value;
            }
        }
        #endregion
        #region ImpRefNbr
        public abstract class impRefNbr : PX.Data.IBqlField
        {
        }
        protected String _ImpRefNbr;

        /// <summary>
        /// Implementation specific reference number of the document.
        /// This field is neither filled nor used by the core Acumatica itself, but may be utilized by customizations or extensions.
        /// </summary>
        [PXDBString(15, IsUnicode = true)]
        public virtual String ImpRefNbr
        {
            get
            {
                return this._ImpRefNbr;
            }
            set
            {
                this._ImpRefNbr = value;
            }
        }
        #endregion

        #region IsTaxValid
        public abstract class isTaxValid : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// When <c>true</c>, indicates that the amount of tax calculated with the external Tax Engine(Avalara) is up to date.
        /// If this field equals <c>false</c>, the document was updated since last synchronization with the Tax Engine
        /// and taxes might need recalculation.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tax is up to date", Enabled = false)]
        public virtual Boolean? IsTaxValid
        {
            get;
            set;
        }
        #endregion
        #region IsTaxPosted
        public abstract class isTaxPosted : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// When <c>true</c>, indicates that the tax information was successfully commited to the external Tax Engine(Avalara).
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tax is posted/commited to the external Tax Engine(Avalara)", Enabled = false)]
        public virtual Boolean? IsTaxPosted
        {
            get;
            set;
        }
        #endregion
        #region IsTaxSaved
        public abstract class isTaxSaved : PX.Data.IBqlField
        {
        }

        /// <summary>
        /// Indicates whether the tax information related to the document was saved to the external Tax Engine (Avalara).
        /// </summary>
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Tax is saved in external Tax Engine(Avalara)", Enabled = false)]
        public virtual Boolean? IsTaxSaved
        {
            get;
            set;
        }
        #endregion
        #region OrigDocType
        public abstract class origDocType : PX.Data.IBqlField
        {
        }
        protected String _OrigDocType;

        /// <summary>
        /// Type of the original (source) document.
        /// </summary>
        [PXDBString(3, IsFixed = true)]
        [APDocType.List()]
        [PXUIField(DisplayName = "Orig. Doc. Type")]
        public virtual String OrigDocType
        {
            get
            {
                return this._OrigDocType;
            }
            set
            {
                this._OrigDocType = value;
            }
        }
        #endregion
        #region OrigRefNbr
        public abstract class origRefNbr : PX.Data.IBqlField
        {
        }
        protected String _OrigRefNbr;

        /// <summary>
        /// Reference number of the original (source) document.
        /// </summary>
        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Orig. Ref. Nbr.")]
        public virtual String OrigRefNbr
        {
            get
            {
                return this._OrigRefNbr;
            }
            set
            {
                this._OrigRefNbr = value;
            }
        }
        #endregion
        #region Released
        public abstract class releasedOrPrebooked : IBqlField
        {
        }
        /// <summary>
        /// Read-only field that is equal to <c>true</c> in case the document
        /// was either <see cref="Prebooked">prebooked</see> or
        /// <see cref="Released">released</see>.
        /// </summary>
        [PXBool]
        public virtual bool? ReleasedOrPrebooked
        {
            [PXDependsOnFields(typeof(released), typeof(prebooked))]
            get
            {
                return
                    this.Released == true ||
                    this.Prebooked == true;
            }
            set
            {
            }
        }
        #endregion
        #region TaxCalcMode
        public abstract class taxCalcMode : IBqlField { }
        protected string _TaxCalcMode;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(VendorClass.taxCalcMode.TaxSetting, typeof(Search<Location.vTaxCalcMode, Where<Location.bAccountID, Equal<Current<APRegister.vendorID>>,
            And<Location.locationID, Equal<Current<APRegister.vendorLocationID>>>>>))]
        [VendorClass.taxCalcMode.List]
        [PXUIField(DisplayName = "Tax Calculation Mode")]
        public virtual string TaxCalcMode
        {
            get { return this._TaxCalcMode; }
            set { this._TaxCalcMode = value; }
        }
        #endregion
        internal string WarningMessage { get; set; }
        #region EmployeeWorkgroupID
        public abstract class employeeWorkgroupID : PX.Data.IBqlField
        {
        }
        /// <summary>
        /// The workgroup that is responsible for the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.TM.EPCompanyTree.WorkGroupID">EPCompanyTree.WorkGroupID</see> field.
        /// </value>
        [PXDBInt]
        [PXDefault(typeof(Vendor.workgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXCompanyTreeSelector]
        [PXUIField(DisplayName = Messages.WorkgroupID, Enabled = false)]
        public virtual int? EmployeeWorkgroupID
        {
            get;
            set;
        }
        #endregion
        #region EmployeeID
        public abstract class employeeID : IBqlField
        {
        }
        /// <summary>
        /// The <see cref="EPEmployee">employee</see> responsible
        /// for the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="EPEmployee.PKID"/> field.
        /// </value>
        [PXDBGuid]
        [PXDefault(typeof(Coalesce<
            Search<
                CREmployee.userID,
                Where<
                    CREmployee.userID, Equal<Current<AccessInfo.userID>>,
                    And<CREmployee.status, NotEqual<BAccount.status.inactive>>>>,
            Search<
                BAccount.ownerID,
                Where<
                    BAccount.bAccountID, Equal<Current<APRegister.vendorID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXOwnerSelector(typeof(APRegister.employeeWorkgroupID))]
        [PXUIField(DisplayName = Messages.Owner)]
        public virtual Guid? EmployeeID
        {
            get;
            set;
        }
        #endregion
        #region WorkgroupID
        public abstract class workgroupID : PX.Data.IBqlField
        {
        }
        /// <summary>
        /// The workgroup that is responsible for document
        /// approval process.
        /// </summary>
        [PXInt]
        [PXSelector(
            typeof(Search<EPCompanyTree.workGroupID>),
            SubstituteKey = typeof(EPCompanyTree.description))]
        [PXUIField(DisplayName = Messages.ApprovalWorkGroupID, Enabled = false)]
        public virtual int? WorkgroupID
        {
            get;
            set;
        }
        #endregion
        #region OwnerID
        public abstract class ownerID : IBqlField
        {
        }
        /// <summary>
        /// The <see cref="EPEmployee">employee</see> responsible
        /// for document approval process.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="EPEmployee.PKID"/> field.
        /// </value>
        [PXGuid]
        [PXOwnerSelector]
        [PXUIField(DisplayName = Messages.Approver, Enabled = false)]
        public virtual Guid? OwnerID
        {
            get;
            set;
        }
        #endregion
    }

    [PXProjection(typeof(Select2<APRegister,
        InnerJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<APRegister.vendorID>>>>))]
    [PXBreakInheritance]
    [Serializable]
    public partial class APRegisterAccess : Vendor
    {
        #region DocType
        public abstract class docType : PX.Data.IBqlField
        {
        }
        protected String _DocType;
        [PXDBString(3, IsKey = true, IsFixed = true, BqlField = typeof(APRegister.docType))]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.IBqlField
        {
        }
        protected String _RefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(APRegister.refNbr))]
        public virtual String RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
        #region Scheduled
        public abstract class scheduled : PX.Data.IBqlField
        {
        }
        protected Boolean? _Scheduled;
        [PXDBBool(BqlField = typeof(APRegister.scheduled))]
        public virtual Boolean? Scheduled
        {
            get
            {
                return this._Scheduled;
            }
            set
            {
                this._Scheduled = value;
            }
        }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : IBqlField
        {
        }
        protected string _ScheduleID;
        [PXDBString(10, IsUnicode = true, BqlField = typeof(APRegister.scheduleID))]
        public virtual string ScheduleID
        {
            get
            {
                return this._ScheduleID;
            }
            set
            {
                this._ScheduleID = value;
            }
        }
        #endregion
    }

    [PXProjection(typeof(Select<APRegister>))]
    public class APRegisterP : IBqlTable
    {
        #region OrigModule
        public abstract class origModule : PX.Data.IBqlField
        {
        }
        protected String _OrigModule;

        [PXDBString(2, IsFixed = true, BqlField = typeof(APRegister.origModule))]
        public virtual String OrigModule
        {
            get
            {
                return this._OrigModule;
            }
            set
            {
                this._OrigModule = value;
            }
        }
        #endregion
        #region DocDesc
        public abstract class docDesc : PX.Data.IBqlField
        {
        }
        protected String _DocDesc;

        [PXDBString(60, IsUnicode = true, BqlField = typeof(APRegister.docDesc))]
        public virtual String DocDesc
        {
            get
            {
                return this._DocDesc;
            }
            set
            {
                this._DocDesc = value;
            }
        }
        #endregion
        #region DocType
        public abstract class docType : PX.Data.IBqlField
        {
        }
        protected String _DocType;

        [PXDBString(3, IsFixed = true, BqlField = typeof(APRegister.docType))]
        public virtual String OrigDocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.IBqlField
        {
        }
        protected String _RefNbr;

        [PXDBString(15, IsUnicode = true, BqlField = typeof(APRegister.refNbr))]
        public virtual String RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
    }
}