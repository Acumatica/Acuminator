using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using PX.Data;

using PX.Common;

using PX.Objects.AR.BQL;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.TX;
using PX.Objects.CA;
using PX.Objects.DR;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.Objects.AR.CCPaymentProcessing;
using PX.Objects.AR.Overrides.ARDocumentRelease;
using PX.Objects.Common;
using PX.Objects.Common.DataIntegrity;

using Avalara.AvaTax.Adapter;
using Avalara.AvaTax.Adapter.TaxService;

using SOOrder = PX.Objects.SO.SOOrder;
using SOInvoice = PX.Objects.SO.SOInvoice;
using SOOrderShipment = PX.Objects.SO.SOOrderShipment;
using INTran = PX.Objects.IN.INTran;
using PMTran = PX.Objects.PM.PMTran;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.AR
{
    [System.SerializableAttribute()]
    public partial class BalancedARDocument : ARRegister
    {
        #region Selected
        public new abstract class selected : IBqlField
        {
        }
        #endregion
        #region DocType
        public new abstract class docType : PX.Data.IBqlField
        {
        }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.IBqlField
        {
        }
        #endregion
        #region OrigModule
        public new abstract class origModule : PX.Data.IBqlField
        {
        }
        #endregion
        #region OpenDoc
        public new abstract class openDoc : PX.Data.IBqlField
        {
        }
        #endregion
        #region Released
        public new abstract class released : PX.Data.IBqlField
        {
        }
        #endregion
        #region Hold
        public new abstract class hold : PX.Data.IBqlField
        {
        }
        #endregion
        #region Scheduled
        public new abstract class scheduled : PX.Data.IBqlField
        {
        }
        #endregion
        #region Voided
        public new abstract class voided : PX.Data.IBqlField
        {
        }
        #endregion
        #region Status
        public new abstract class status : PX.Data.IBqlField
        {
        }
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [ARDocStatus.List()]
        public override String Status
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
        #region CreatedByID
        public new abstract class createdByID : PX.Data.IBqlField
        {
        }
        #endregion
        #region LastModifiedByID
        public new abstract class lastModifiedByID : PX.Data.IBqlField
        {
        }
        #endregion
        #region CustomerRefNbr
        public abstract class customerRefNbr : IBqlField
        {
        }
        protected String _CustomerRefNbr;
        [PXString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer Order")]
        public String CustomerRefNbr
        {
            get
            {
                return _CustomerRefNbr;
            }
            set
            {
                _CustomerRefNbr = value;
            }
        }
        #endregion
        #region IsTaxValid
        public new abstract class isTaxValid : PX.Data.IBqlField
        {
        }
        #endregion
        #region IsTaxPosted
        public new abstract class isTaxPosted : PX.Data.IBqlField
        {
        }
        #endregion
        #region IsTaxSaved
        public new abstract class isTaxSaved : PX.Data.IBqlField
        {
        }
        #endregion
        #region PaymentMethodID
        public abstract class paymentMethodID : IBqlField { }
        [PXString(10, IsUnicode = true)]
        [PXUIField(DisplayName = CA.Messages.PaymentMethod, Visible = false)]
        public virtual string PaymentMethodID { get; set; }
        #endregion
    }


    public class PXMassProcessException : PXException
    {
        protected Exception _InnerException;
        protected int _ListIndex;

        public int ListIndex
        {
            get
            {
                return this._ListIndex;
            }
        }

        public PXMassProcessException(int ListIndex, Exception InnerException)
            : base(InnerException is PXOuterException ? InnerException.Message + "\r\n" + String.Join("\r\n", ((PXOuterException)InnerException).InnerMessages) : InnerException.Message, InnerException)
        {
            this._ListIndex = ListIndex;
        }

        public PXMassProcessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PXReflectionSerializer.RestoreObjectProps(this, info);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            PXReflectionSerializer.GetObjectData(this, info);
            base.GetObjectData(info, context);
        }
    }

    [PX.Objects.GL.TableAndChartDashboardType]
    public class ARDocumentRelease : PXGraph<ARDocumentRelease>
    {
        public PXCancel<BalancedARDocument> Cancel;
        [PXFilterable]
        [PX.SM.PXViewDetailsButton(typeof(BalancedARDocument.refNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
        public PXProcessingJoin<BalancedARDocument,
            LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<BalancedARDocument.docType>,
                    And<ARInvoice.refNbr, Equal<BalancedARDocument.refNbr>>>,
                LeftJoin<ARPayment, On<ARPayment.docType, Equal<BalancedARDocument.docType>,
                    And<ARPayment.refNbr, Equal<BalancedARDocument.refNbr>>>,
                InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<BalancedARDocument.customerID>>,
                LeftJoin<ARAdjust, On<ARAdjust.adjgDocType, Equal<BalancedARDocument.docType>,
                And<ARAdjust.adjgRefNbr, Equal<BalancedARDocument.refNbr>,
                And<ARAdjust.adjNbr, Equal<BalancedARDocument.adjCntr>,
                And<ARAdjust.hold, Equal<boolFalse>>>>>>>>>,
                Where2<Match<Customer, Current<AccessInfo.userName>>, And<ARRegister.hold, Equal<boolFalse>, And<ARRegister.voided, Equal<boolFalse>, And<ARRegister.scheduled, Equal<boolFalse>, And<Where<BalancedARDocument.released, Equal<boolFalse>, And<BalancedARDocument.origModule, Equal<GL.BatchModule.moduleAR>, Or<BalancedARDocument.openDoc, Equal<boolTrue>, And<ARAdjust.adjdRefNbr, IsNotNull>>>>>>>>>> ARDocumentList;

        public PXSetup<ARSetup> arsetup;

        public static string[] TransClassesWithoutZeroPost = { GLTran.tranClass.Discount, GLTran.tranClass.RealizedAndRoundingGOL, GLTran.tranClass.WriteOff };

        public ARDocumentRelease()
        {
            ARSetup setup = arsetup.Current;
            ARDocumentList.SetProcessDelegate(
                delegate (List<BalancedARDocument> list)
                {
                    List<ARRegister> newlist = new List<ARRegister>(list.Count);
                    foreach (BalancedARDocument doc in list)
                    {
                        newlist.Add(doc);
                    }
                    ReleaseDoc(newlist, true);
                }
            );
            ARDocumentList.SetProcessCaption(Messages.Release);
            ARDocumentList.SetProcessAllCaption(Messages.ReleaseAll);
        }

        public delegate void ARMassProcessDelegate(ARRegister ardoc, bool isAborted);

        public delegate void ARMassProcessReleaseTransactionScopeDelegate(ARRegister ardoc);

        public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess)
        {
            ReleaseDoc(list, isMassProcess, null, null);
        }

        public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess, List<Batch> externalPostList)
        {
            ReleaseDoc(list, isMassProcess, externalPostList, null);
        }

        public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess, List<Batch> externalPostList, ARMassProcessDelegate onsuccess)
        {
            ReleaseDoc(list, isMassProcess, externalPostList, onsuccess, null);
        }

        /// <summary>
        /// Static function for release of AR documents and posting of the released batch.
        /// Released batches will be posted if the corresponded flag in ARSetup is set to true.
        /// SkipPost parameter is used to override this flag. 
        /// This function can not be called from inside of the covering DB transaction scope, unless skipPost is set to true.     
        /// </summary>
        /// <param name="list">List of the documents to be released</param>
        /// <param name="isMassProcess">Flag specifing if the function is called from mass process - affects error handling</param>
        /// <param name="externalPostList"> List of batches that should not be posted inside the release procedure</param>
        /// <param name="onsuccess"> Delegate to be called if release process completed successfully</param>
        /// <param name="onreleasecomplete"> Delegate to be called inside the transaction scope of AR document release process</param>
        public static void ReleaseDoc(List<ARRegister> list, bool isMassProcess, List<Batch> externalPostList, ARMassProcessDelegate onsuccess, ARMassProcessReleaseTransactionScopeDelegate onreleasecomplete)
        {
            bool failed = false;
            ARReleaseProcess rg = PXGraph.CreateInstance<ARReleaseProcess>();
            JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
            je.FieldVerifying.AddHandler<GLTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            je.FieldVerifying.AddHandler<GLTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
            je.RowInserting.AddHandler<GLTran>((sender, e) => { je.SetZeroPostIfUndefined((GLTran)e.Row, TransClassesWithoutZeroPost); });

            PostGraph pg = PXGraph.CreateInstance<PostGraph>();
            Dictionary<int, int> batchbind = new Dictionary<int, int>();
            List<Batch> pmBatchList = new List<Batch>();
            bool isSkipPost = externalPostList != null;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                    continue;

                ARRegister doc = list[i];
                try
                {
                    bool onefailed = false;
                    rg.Clear();

                    if (onsuccess != null || onreleasecomplete != null)
                    {
                        PXTimeStampScope.SetRecordComesFirst(typeof(ARRegister), true);
                        PXTimeStampScope.DuplicatePersisted(rg.ARDocument.Cache, doc, typeof(ARInvoice));
                    }

                    try
                    {
                        List<ARRegister> childs = rg.ReleaseDocProc(je, doc, pmBatchList, onreleasecomplete);

                        object cached;
                        if ((cached = rg.ARDocument.Cache.Locate(doc)) != null)
                        {
                            PXCache<ARRegister>.RestoreCopy(doc, (ARRegister)cached);
                            doc.Selected = true;
                        }

                        int k;
                        if ((k = je.created.IndexOf(je.BatchModule.Current)) >= 0 && batchbind.ContainsKey(k) == false)
                        {
                            batchbind.Add(k, i);
                        }


                        if (childs != null)
                        {
                            foreach (ARRegister child in childs)
                            {
                                rg.Clear();
                                rg.ReleaseDocProc(je, child, pmBatchList, null);

                                if ((cached = rg.ARDocument.Cache.Locate(doc)) != null)
                                {
                                    PXCache<ARRegister>.RestoreCopy(doc, (ARRegister)cached);
                                    doc.Selected = true;
                                }
                            }
                        }
                    }
                    catch
                    {
                        je.CleanupCreated(batchbind.Keys);
                        je.Clear();

                        onefailed = true;
                        throw;
                    }
                    finally
                    {
                        if (onsuccess != null)
                        {
                            onsuccess(doc, onefailed);
                        }
                    }

                    if (isMassProcess)
                    {
                        if (string.IsNullOrEmpty(doc.WarningMessage))
                            PXProcessing<ARRegister>.SetInfo(i, ActionsMessages.RecordProcessed);
                        else
                        {
                            PXProcessing<ARRegister>.SetWarning(i, doc.WarningMessage);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (isMassProcess)
                    {
                        PXProcessing<ARRegister>.SetError(i, e);
                        failed = true;
                    }
                    else
                    {
                        throw new PXMassProcessException(i, e);
                    }
                }
            }

            if (isSkipPost)
            {
                if (rg.AutoPost)
                    externalPostList.AddRange(je.created);
            }
            else
            {
                for (int i = 0; i < je.created.Count; i++)
                {
                    Batch batch = je.created[i];
                    try
                    {
                        if (rg.AutoPost)
                        {
                            pg.Clear();
                            pg.PostBatchProc(batch);
                        }
                    }
                    catch (Exception e)
                    {
                        if (isMassProcess)
                        {
                            failed = true;
                            PXProcessing<ARRegister>.SetError(batchbind[i], e);
                        }
                        else
                        {
                            throw new PXMassProcessException(batchbind[i], e);
                        }
                    }
                }
            }
            if (failed)
            {
                throw new PXOperationCompletedWithErrorException(GL.Messages.DocumentsNotReleased);
            }

            List<PM.ProcessInfo<Batch>> infoList = new List<ProcessInfo<Batch>>();
            ProcessInfo<Batch> processInfo = new ProcessInfo<Batch>(0);
            processInfo.Batches.AddRange(pmBatchList);
            infoList.Add(processInfo);
            PM.RegisterRelease.Post(infoList, isMassProcess);
        }

        protected virtual IEnumerable ardocumentlist()
        {
            PXSelectBase<BalancedARDocument> cmd = new PXSelectJoinGroupBy<BalancedARDocument,
                InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<BalancedARDocument.customerID>>,
                LeftJoin<ARAdjust, On<ARAdjust.adjgDocType, Equal<BalancedARDocument.docType>,
                    And<ARAdjust.adjgRefNbr, Equal<BalancedARDocument.refNbr>,
                    And<ARAdjust.adjNbr, Equal<BalancedARDocument.adjCntr>,
                    And<ARAdjust.hold, Equal<False>>>>>,
                LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<BalancedARDocument.docType>,
                    And<ARInvoice.refNbr, Equal<BalancedARDocument.refNbr>>>,
                LeftJoin<ARPayment, On<ARPayment.docType, Equal<BalancedARDocument.docType>,
                    And<ARPayment.refNbr, Equal<BalancedARDocument.refNbr>>>>>>>,
                Where2<Match<Customer, Current<AccessInfo.userName>>,
                    And<ARRegister.hold, Equal<False>,
                    And<ARRegister.voided, Equal<False>,
                    And<ARRegister.scheduled, Equal<False>,
                    And2<Where<ARInvoice.refNbr, IsNotNull, Or<ARPayment.refNbr, IsNotNull>>,
                    And<Where<BalancedARDocument.released, Equal<False>,
                        And<BalancedARDocument.origModule, Equal<GL.BatchModule.moduleAR>,
                        Or<BalancedARDocument.openDoc, Equal<True>, And<ARAdjust.adjdRefNbr, IsNotNull>>>>>>>>>>,
                Aggregate<GroupBy<BalancedARDocument.docType,
                GroupBy<BalancedARDocument.refNbr,
                GroupBy<BalancedARDocument.released,
                GroupBy<BalancedARDocument.openDoc,
                GroupBy<BalancedARDocument.hold,
                GroupBy<BalancedARDocument.scheduled,
                GroupBy<BalancedARDocument.voided,
                GroupBy<BalancedARDocument.createdByID,
                GroupBy<BalancedARDocument.lastModifiedByID,
                GroupBy<BalancedARDocument.isTaxValid,
                GroupBy<BalancedARDocument.isTaxSaved,
                GroupBy<BalancedARDocument.isTaxPosted,
                GroupBy<ARInvoice.dontPrint,
                GroupBy<ARInvoice.printed,
                GroupBy<ARInvoice.dontEmail,
                GroupBy<ARInvoice.emailed>>>>>>>>>>>>>>>>>,
                OrderBy<Asc<BalancedARDocument.docType,
                        Asc<BalancedARDocument.refNbr>>>>(this);
                        
            int startRow = PXView.StartRow;
            int totalRows = 0;

            bool isSyncPosition = PXView.Searches.Any(search => search != null);

            foreach (PXResult<BalancedARDocument, Customer, ARAdjust, ARInvoice, ARPayment> res in
                cmd.View.Select(
                    null,
                    null,
                    PXView.Searches,
                    ARDocumentList.View.GetExternalSorts(),
                    ARDocumentList.View.GetExternalDescendings(),
                    ARDocumentList.View.GetExternalFilters(),
                    ref startRow,
                    PXView.MaximumRows,
                    ref totalRows))
            {
                BalancedARDocument ardoc = (BalancedARDocument)res;
                ardoc = ARDocumentList.Locate(ardoc) ?? ardoc;
                ARInvoice invoice = (ARInvoice)res;
                ARPayment payment = (ARPayment)res;

                ardoc.PaymentMethodID = payment?.PaymentMethodID;

                if (invoice != null && string.IsNullOrEmpty(invoice.InvoiceNbr) == false)
                {
                    ardoc.CustomerRefNbr = invoice.InvoiceNbr;
                }
                else if (payment != null && string.IsNullOrEmpty(payment.ExtRefNbr) == false)
                {
                    ardoc.CustomerRefNbr = payment.ExtRefNbr;
                }

                if (invoice != null && string.IsNullOrEmpty(invoice.RefNbr) == false)
                {
                    //if PrintBeforeRelease is off and EmailBeforeRelease is on document will be simply skipped
                    if (ardoc.Released == false && ardoc.Status == ARDocStatus.PendingPrint && arsetup.Current.PrintBeforeRelease != true)
                    {
                        ardoc.Status = ARDocStatus.Balanced;
                    }
                    if (ardoc.Released == false && ardoc.Status == ARDocStatus.PendingEmail && arsetup.Current.EmailBeforeRelease != true)
                    {
                        ardoc.Status = ARDocStatus.Balanced;
                    }
                }

                if (payment != null && payment.PMInstanceID != null
                        && this.arsetup.Current.IntegratedCCProcessing == true)
                {
                    // Filter out payments by credit cards - they are processed separately
                    PXResult<CustomerPaymentMethod, PaymentMethod> paymentMethodResult = (PXResult<CustomerPaymentMethod, PaymentMethod>)
                        PXSelectJoin<
                        CustomerPaymentMethod,
                        InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>>,
                            Where<CustomerPaymentMethod.pMInstanceID, Equal<Required<CustomerPaymentMethod.pMInstanceID>>>>
                            .Select(this, payment.PMInstanceID);

                    PaymentMethod paymentMethod = paymentMethodResult;

                    if (paymentMethod != null &&
                        paymentMethod.PaymentType == PaymentMethodType.CreditCard &&
                        paymentMethod.ARIsProcessingRequired == true)
                    {
                        continue;
                    }
                }

                ARAdjust adj = res;
                if (ardoc.Released == true ||
                       invoice.RefNbr == null ||
                         (
                             (arsetup.Current.PrintBeforeRelease == false ||
                                invoice.Printed == true || invoice.DontPrint == true)
                             &&
                             (arsetup.Current.EmailBeforeRelease == false ||
                                invoice.Emailed == true || invoice.DontEmail == true)
                         ))
                {
                    if (adj.AdjdRefNbr != null)
                    {
                        ardoc.DocDate = adj.AdjgDocDate;
                        ardoc.TranPeriodID = adj.AdjgTranPeriodID;
                        ardoc.FinPeriodID = adj.AdjgFinPeriodID;
                    }
                    yield return new PXResult<BalancedARDocument, ARInvoice, ARPayment, Customer, ARAdjust>(ardoc, res, res, res, res);
                }
            }

            PXView.StartRow = 0;
        }

        [PXHidden()]
        [Serializable()]
        public partial class ARInvoice : IBqlTable
        {
            #region DocType
            public abstract class docType : PX.Data.IBqlField
            {
            }
            [PXDBString(3, IsKey = true, IsFixed = true)]
            public virtual String DocType
            {
                get;
                set;
            }
            #endregion
            #region RefNbr
            public abstract class refNbr : PX.Data.IBqlField
            {
            }
            [PXDBString(15, IsKey = true, IsUnicode = true)]
            public virtual String RefNbr
            {
                get;
                set;
            }
            #endregion
            #region InvoiceNbr
            public abstract class invoiceNbr : PX.Data.IBqlField
            {
            }
            [PXDBString(15, IsKey = true, IsUnicode = true)]
            public virtual String InvoiceNbr
            {
                get;
                set;
            }
            #endregion
            #region DontPrint
            public abstract class dontPrint : PX.Data.IBqlField
            {
            }
            protected Boolean? _DontPrint;
            [PXDBBool()]
            public virtual Boolean? DontPrint
            {
                get;
                set;
            }
            #endregion
            #region Printed
            public abstract class printed : PX.Data.IBqlField
            {
            }
            protected Boolean? _Printed;
            [PXDBBool()]
            public virtual Boolean? Printed
            {
                get;
                set;
            }
            #endregion
            #region DontEmail
            public abstract class dontEmail : PX.Data.IBqlField
            {
            }
            protected Boolean? _DontEmail;
            [PXDBBool()]
            public virtual Boolean? DontEmail
            {
                get;
                set;
            }
            #endregion
            #region Emailed
            public abstract class emailed : PX.Data.IBqlField
            {
            }
            protected Boolean? _Emailed;
            [PXDBBool()]
            public virtual Boolean? Emailed
            {
                get;
                set;
            }
            #endregion
        }

        [PXHidden()]
        [Serializable()]
        public partial class ARPayment : IBqlTable
        {
            #region DocType
            public abstract class docType : PX.Data.IBqlField
            {
            }
            [PXDBString(3, IsKey = true, IsFixed = true)]
            public virtual String DocType
            {
                get;
                set;
            }
            #endregion
            #region RefNbr
            public abstract class refNbr : PX.Data.IBqlField
            {
            }
            [PXDBString(15, IsKey = true, IsUnicode = true)]
            public virtual String RefNbr
            {
                get;
                set;
            }
            #endregion
            #region ExtRefNbr
            public abstract class extRefNbr : PX.Data.IBqlField
            {
            }
            [PXDBString(15, IsUnicode = true)]
            public virtual String ExtRefNbr
            {
                get;
                set;
            }
            #endregion
            #region PMInstanceID
            public abstract class pMInstanceID : PX.Data.IBqlField
            {
            }
            [PXDBInt()]
            public virtual int? PMInstanceID
            {
                get;
                set;
            }
            #endregion
            #region PaymentMethodID
            public abstract class paymentMethodID : IBqlField { }
            [PXDBString(10, IsUnicode = true)]
            public virtual string PaymentMethodID { get; set; }
            #endregion
        }
    }

    public class ARPayment_CurrencyInfo_Currency_Customer : PXSelectJoin<ARPayment, InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARPayment.curyInfoID>>, InnerJoin<Currency, On<Currency.curyID, Equal<CurrencyInfo.curyID>>, LeftJoin<Customer, On<Customer.bAccountID, Equal<ARPayment.customerID>>, LeftJoin<CashAccount, On<CashAccount.cashAccountID, Equal<ARPayment.cashAccountID>>>>>>, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>, And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>
    {
        public ARPayment_CurrencyInfo_Currency_Customer(PXGraph graph)
            : base(graph)
        {
        }
    }

    public class ARInvoice_CurrencyInfo_Terms_Customer : PXSelectJoin<ARInvoice,
        InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARInvoice.curyInfoID>>,
        LeftJoin<Terms, On<Terms.termsID, Equal<ARInvoice.termsID>>,
        LeftJoin<Customer, On<Customer.bAccountID, Equal<ARInvoice.customerID>>,
        LeftJoin<Account, On<ARInvoice.aRAccountID, Equal<Account.accountID>>>>>>,
        Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>
    {
        public ARInvoice_CurrencyInfo_Terms_Customer(PXGraph graph)
            : base(graph)
        {
        }
    }

    public class ARReleaseProcess : PXGraph<ARReleaseProcess>
    {
        public PXSelect<ARRegister> ARDocument;

        public PXSelectJoin<
            ARTran,
                LeftJoin<ARTax,
                    On<ARTax.tranType, Equal<ARTran.tranType>,
                    And<ARTax.refNbr, Equal<ARTran.refNbr>,
                    And<ARTax.lineNbr, Equal<ARTran.lineNbr>>>>,
                LeftJoin<Tax,
                    On<Tax.taxID, Equal<ARTax.taxID>>,
                LeftJoin<DRDeferredCode,
                    On<DRDeferredCode.deferredCodeID, Equal<ARTran.deferredCode>>,
                LeftJoin<PMTran,
                    On<PMTran.tranID, Equal<ARTran.pMTranID>>,
                LeftJoin<SO.SOOrderType,
                    On<SO.SOOrderType.orderType, Equal<ARTran.sOOrderType>>>>>>>,
            Where<
                ARTran.tranType, Equal<Required<ARTran.tranType>>,
                And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>,
            OrderBy<
                Asc<ARTran.lineNbr,
                Asc<Tax.taxCalcLevel>>>>
            ARTran_TranType_RefNbr;

        public PXSelectJoin<
            ARTaxTran,
                LeftJoin<Account,
                    On<Account.accountID, Equal<ARTaxTran.accountID>>>,
            Where<
                ARTaxTran.module, Equal<BatchModule.moduleAR>,
                And<ARTaxTran.tranType, Equal<Required<ARTaxTran.tranType>>,
                And<ARTaxTran.refNbr, Equal<Required<ARTaxTran.refNbr>>>>>,
            OrderBy<
                Asc<Tax.taxCalcLevel>>>
            ARTaxTran_TranType_RefNbr;
        public PXSelect<SVATConversionHist> SVATConversionHistory;

        public PXSelect<Batch> Batch;

        public ARInvoice_CurrencyInfo_Terms_Customer ARInvoice_DocType_RefNbr;
        public ARPayment_CurrencyInfo_Currency_Customer ARPayment_DocType_RefNbr;

        public PXSelectJoin<
            ARAdjust,
                InnerJoin<CurrencyInfo,
                    On<CurrencyInfo.curyInfoID, Equal<ARAdjust.adjdCuryInfoID>>,
                InnerJoin<Currency,
                    On<Currency.curyID, Equal<CurrencyInfo.curyID>>,
                // Adjusted ARInvoice records. Replaced with ARRegister 
                // for performance reasons (to avoid projection subselect).
                // -
                LeftJoin<ARRegister,
                    On<ARRegister.docType, Equal<ARAdjust.adjdDocType>,
                    And<ARRegister.refNbr, Equal<ARAdjust.adjdRefNbr>,
                    And<Where<
                        ARRegister.docType, Equal<ARInvoiceType.invoice>,
                        Or<ARRegister.docType, Equal<ARInvoiceType.debitMemo>,
                        Or<ARRegister.docType, Equal<ARInvoiceType.creditMemo>,
                        Or<ARRegister.docType, Equal<ARInvoiceType.finCharge>,
                        Or<ARRegister.docType, Equal<ARInvoiceType.smallCreditWO>,
                        Or<ARRegister.docType, Equal<ARInvoiceType.cashSale>,
                        Or<ARRegister.docType, Equal<ARInvoiceType.cashReturn>>>>>>>>>>>,
                LeftJoin<ARPayment,
                    On<ARPayment.docType, Equal<ARAdjust.adjdDocType>,
                    And<ARPayment.refNbr, Equal<ARAdjust.adjdRefNbr>>>>>>>,
            Where<
                ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
                And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
                And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>
            ARAdjust_AdjgDocType_RefNbr_CustomerID;

        public PXSelectJoin<
            ARAdjust,
                InnerJoin<CurrencyInfo,
                    On<CurrencyInfo.curyInfoID, Equal<ARAdjust.adjdCuryInfoID>>,
                InnerJoin<Currency,
                    On<Currency.curyID, Equal<CurrencyInfo.curyID>>,
                LeftJoin<ARPayment,
                    On<ARPayment.docType, Equal<ARAdjust.adjgDocType>,
                    And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>>>,
            Where<
                ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
                And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
                And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>
            ARAdjust_AdjdDocType_RefNbr_CustomerID;

        public PXSelect<ARPaymentChargeTran, Where<ARPaymentChargeTran.docType, Equal<Required<ARPaymentChargeTran.docType>>, And<ARPaymentChargeTran.refNbr, Equal<Required<ARPaymentChargeTran.refNbr>>>>> ARPaymentChargeTran_DocType_RefNbr;

        public PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>,
                                            And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>> ARDoc_SalesPerTrans;

        public PXSelect<CATran> CashTran;
        public PXSelect<INTran> intranselect;
        public PXSetup<GLSetup> glsetup;
        public PXSetup<SOSetup> SOSetup;

        public PXSelect<Tax> taxes;
        public PM.PMCommitmentSelect Commitments;
        protected PXResultset<ARAdjust> ARAdjustsToRelease;

        private ARSetup _arsetup;
        public ARSetup arsetup
        {
            get
            {
                _arsetup = (_arsetup ?? PXSelect<ARSetup>.Select(this));
                return _arsetup;
            }
        }

        public bool AutoPost
        {
            get
            {
                return (bool)arsetup.AutoPost;
            }
        }

        public bool SummPost
        {
            get
            {
                return (arsetup.TransactionPosting == "S");
            }
        }

        public string InvoiceRounding
        {
            get
            {
                return arsetup.InvoiceRounding;
            }
        }

        public decimal? InvoicePrecision
        {
            get
            {
                return arsetup.InvoicePrecision;
            }
        }

        public decimal? RoundingLimit
        {
            get
            {
                return glsetup.Current.RoundingLimit;
            }
        }

        protected ARInvoiceEntry _ie = null;
        public ARInvoiceEntry ie
        {
            get
            {
                _ie = (_ie ?? PXGraph.CreateInstance<ARInvoiceEntry>());
                return _ie;
            }
        }

        protected ARPaymentEntry _pe = null;
        public ARPaymentEntry pe
        {
            get
            {
                _pe = (_pe ?? PXGraph.CreateInstance<ARPaymentEntry>());
                return _pe;
            }
        }

        [PXDBString(6, IsFixed = true)]
        [PXDefault()]
        protected virtual void ARPayment_AdjFinPeriodID_CacheAttached(PXCache sender)
        { }

        [PXDBString(6, IsFixed = true)]
        [PXDefault()]
        protected virtual void ARPayment_AdjTranPeriodID_CacheAttached(PXCache sender)
        { }


        [PXDBString(1, IsFixed = true)]
        public virtual void Tax_TaxType_CacheAttached(PXCache sender) { }

        [PXDBString(1, IsFixed = true)]
        public virtual void Tax_TaxCalcLevel_CacheAttached(PXCache sender) { }

        public ARReleaseProcess()
        {
            //Caches[typeof(ARRegister)] = new PXCache<ARRegister>(this);
            OpenPeriodAttribute.SetValidatePeriod<ARRegister.finPeriodID>(ARDocument.Cache, null, PeriodValidation.Nothing);
            OpenPeriodAttribute.SetValidatePeriod<ARPayment.adjFinPeriodID>(ARPayment_DocType_RefNbr.Cache, null, PeriodValidation.Nothing);

            PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.customerID>(Caches[typeof(ARAdjust)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgDocType>(Caches[typeof(ARAdjust)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgRefNbr>(Caches[typeof(ARAdjust)], null, false);
            PXDBLiteDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgCuryInfoID>(Caches[typeof(ARAdjust)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgDocDate>(Caches[typeof(ARAdjust)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgFinPeriodID>(Caches[typeof(ARAdjust)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARAdjust.adjgTranPeriodID>(Caches[typeof(ARAdjust)], null, false);

            PXDBDefaultAttribute.SetDefaultForInsert<ARTran.tranType>(Caches[typeof(ARTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForInsert<ARTran.refNbr>(Caches[typeof(ARTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.tranType>(Caches[typeof(ARTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.refNbr>(Caches[typeof(ARTran)], null, false);
            PXDBLiteDefaultAttribute.SetDefaultForUpdate<ARTran.curyInfoID>(Caches[typeof(ARTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.tranDate>(Caches[typeof(ARTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.finPeriodID>(Caches[typeof(ARTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARTran.customerID>(Caches[typeof(ARTran)], null, false);

            PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.tranType>(Caches[typeof(ARTaxTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.refNbr>(Caches[typeof(ARTaxTran)], null, false);
            PXDBLiteDefaultAttribute.SetDefaultForUpdate<ARTaxTran.curyInfoID>(Caches[typeof(ARTaxTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.tranDate>(Caches[typeof(ARTaxTran)], null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<ARTaxTran.taxZoneID>(Caches[typeof(ARTaxTran)], null, false);

            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.refNbr>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranDate>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.finPeriodID>(intranselect.Cache, null, false);
            PXDBDefaultAttribute.SetDefaultForUpdate<INTran.tranPeriodID>(intranselect.Cache, null, false);

            PXFormulaAttribute.SetAggregate<ARAdjust.curyAdjgAmt>(Caches[typeof(ARAdjust)], null);
            PXFormulaAttribute.SetAggregate<ARAdjust.curyAdjdAmt>(Caches[typeof(ARAdjust)], null);
            PXFormulaAttribute.SetAggregate<ARAdjust.adjAmt>(Caches[typeof(ARAdjust)], null);
        }

        protected virtual void ARPayment_CashAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void ARPayment_PMInstanceID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void ARPayment_PaymentMethodID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void ARPayment_ExtRefNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void ARRegister_FinPeriodID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.FieldName = string.Empty;
                e.Cancel = true;
            }
        }

        protected virtual void ARRegister_TranPeriodID_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.FieldName = string.Empty;
                e.Cancel = true;
            }
        }

        protected virtual void ARRegister_DocDate_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                e.FieldName = string.Empty;
                e.Cancel = true;
            }
        }

        protected virtual void ARAdjust_AdjdRefNbr_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            e.Cancel = true;
        }

        protected virtual void ARTran_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            e.Cancel = _IsIntegrityCheck;
        }

        protected virtual void ARTran_TaxCategoryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = null;
            e.Cancel = true;
        }

        protected virtual void ARTran_SalesPersonID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = null;
            e.Cancel = true;
        }

        private ARHist CreateHistory(int? BranchID, int? AccountID, int? SubID, int? CustomerID, string PeriodID)
        {
            ARHist accthist = new ARHist();
            accthist.BranchID = BranchID;
            accthist.AccountID = AccountID;
            accthist.SubID = SubID;
            accthist.CustomerID = CustomerID;
            accthist.FinPeriodID = PeriodID;
            return (ARHist)Caches[typeof(ARHist)].Insert(accthist);
        }

        private CuryARHist CreateHistory(int? BranchID, int? AccountID, int? SubID, int? CustomerID, string CuryID, string PeriodID)
        {
            CuryARHist accthist = new CuryARHist();
            accthist.BranchID = BranchID;
            accthist.AccountID = AccountID;
            accthist.SubID = SubID;
            accthist.CustomerID = CustomerID;
            accthist.CuryID = CuryID;
            accthist.FinPeriodID = PeriodID;
            return (CuryARHist)Caches[typeof(CuryARHist)].Insert(accthist);
        }

        private class ARHistItemDiscountsBucket : ARHistBucket
        {
            public ARHistItemDiscountsBucket(ARTran tran)
                : base()
            {
                switch (tran.TranType)
                {
                    case ARDocType.Invoice:
                    case ARDocType.DebitMemo:
                    case ARDocType.CashSale:
                        SignPtdItemDiscounts = 1m;
                        break;
                    case ARDocType.CreditMemo:
                    case ARDocType.CashReturn:
                        SignPtdItemDiscounts = -1m;
                        break;
                }
            }
        }

        private class ARHistBucket
        {
            public int? arAccountID = null;
            public int? arSubID = null;
            public decimal SignPayments = 0m;
            public decimal SignDeposits = 0m;
            public decimal SignSales = 0m;
            public decimal SignFinCharges = 0m;
            public decimal SignCrMemos = 0m;
            public decimal SignDrMemos = 0m;
            public decimal SignDiscTaken = 0m;
            public decimal SignRGOL = 0m;
            public decimal SignPtd = 0m;
            public decimal SignPtdItemDiscounts = 0m;

            public ARHistBucket(GLTran tran, string TranType)
            {
                arAccountID = tran.AccountID;
                arSubID = tran.SubID;

                switch (TranType + tran.TranClass)
                {
                    case "CSLN":
                        SignSales = -1m;
                        SignPayments = -1m;
                        SignPtd = 0m;
                        break;
                    case "RCSN":
                        SignSales = -1m;
                        SignPayments = -1m;
                        SignPtd = 0m;
                        break;
                    case "INVN":
                        SignSales = 1m;
                        SignPtd = 1m;
                        break;
                    case "DRMN":
                        SignDrMemos = 1m;
                        SignPtd = 1m;
                        break;
                    case "FCHN":
                        SignFinCharges = 1m;
                        SignPtd = 1m;
                        break;
                    case "CRMP":
                    case "CRMN":
                        SignCrMemos = -1m;
                        SignPtd = 1m;
                        break;
                    case "CRMR":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignCrMemos = -1m;
                        SignRGOL = 1m;
                        break;
                    case "CRMD":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignCrMemos = -1m;
                        SignDiscTaken = 1m;
                        break;
                    case "CRMB":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignCrMemos = 0m;
                        break;
                    case "PPMP":
                        SignDeposits = -1m;
                        break;
                    case "PPMU":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignDeposits = -1m;
                        SignDrMemos = -1m;
                        SignPtd = -1m;
                        break;
                    case "PMTU":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignPayments = -1m;
                        SignDrMemos = -1m;
                        break;
                    case "RPMP":
                    case "RPMN":
                    case "PMTP":
                    case "PMTN":
                    case "PPMN":
                    case "REFP":
                    case "REFN":
                        SignPayments = -1m;
                        SignPtd = 1m;
                        break;
                    case "REFU":
                        SignDeposits = -1m;
                        break;
                    case "RPMR":
                    case "PPMR":
                    case "PMTR":
                    case "REFR":
                    case "CSLR":
                    case "RCSR":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignPayments = -1m;
                        SignRGOL = 1m;
                        break;
                    case "RPMD":
                    case "PPMD":
                    case "PMTD":
                    case "REFD": //not really happens
                    case "CSLD":
                    case "RCSD":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignPayments = -1m;
                        SignDiscTaken = 1m;
                        break;
                    case "SMCP":
                        //Zero Update
                        //will insert SCWO Account in ARHistory for trial balance report
                        //arAccountID = tran.OrigAccountID;
                        //arSubID = tran.OrigSubID;
                        break;
                    case "SMCN":
                        SignDrMemos = 1m;
                        SignPtd = 1m;
                        break;
                    case "SMBP":
                        //Zero Update
                        //will insert SBWO Account in ARHistory for trial balance report
                        break;
                    case "SMBD": //not really happens
                                 //Zero Update
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        break;
                    case "SMBN":
                        SignCrMemos = -1m;
                        SignPtd = 1m;
                        break;
                    case "SMBR":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignCrMemos = -1m;
                        SignRGOL = 1m;
                        break;
                    case "RPMB":
                    case "PPMB":
                    case "REFB": //not really happens
                    case "CSLB":
                    case "RCSB":
                    case "PMTB":
                    case "SMBB":
                        arAccountID = tran.OrigAccountID;
                        arSubID = tran.OrigSubID;
                        SignPayments = -1m;
                        SignCrMemos = 1m;
                        break;
                }
            }

            public ARHistBucket()
            {
            }
        }

        private void UpdateHist<History>(History accthist, ARHistBucket bucket, bool FinFlag, GLTran tran)
            where History : class, IBaseARHist
        {
            if (_IsIntegrityCheck == false || accthist.DetDeleted == false)
            {
                accthist.FinFlag = FinFlag;
                accthist.PtdPayments += bucket.SignPayments * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdSales += bucket.SignSales * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdDrAdjustments += bucket.SignDrMemos * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdCrAdjustments += bucket.SignCrMemos * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdFinCharges += bucket.SignFinCharges * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdDiscounts += bucket.SignDiscTaken * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdRGOL += bucket.SignRGOL * (tran.DebitAmt - tran.CreditAmt);
                accthist.YtdBalance += bucket.SignPtd * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdDeposits += bucket.SignDeposits * (tran.DebitAmt - tran.CreditAmt);
                accthist.YtdDeposits += bucket.SignDeposits * (tran.DebitAmt - tran.CreditAmt);
                accthist.PtdItemDiscounts += bucket.SignPtdItemDiscounts * (tran.DebitAmt - tran.CreditAmt);
            }
        }

        private void UpdateFinHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
            where History : class, IBaseARHist
        {
            UpdateHist<History>(accthist, bucket, true, tran);
        }

        private void UpdateTranHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
            where History : class, IBaseARHist
        {
            UpdateHist<History>(accthist, bucket, false, tran);
        }

        private void CuryUpdateHist<History>(History accthist, ARHistBucket bucket, bool FinFlag, GLTran tran)
            where History : class, ICuryARHist, IBaseARHist
        {
            if (_IsIntegrityCheck == false || accthist.DetDeleted == false)
            {
                UpdateHist<History>(accthist, bucket, FinFlag, tran);

                accthist.FinFlag = FinFlag;
                accthist.CuryPtdPayments += bucket.SignPayments * (tran.CuryDebitAmt - tran.CuryCreditAmt);
                accthist.CuryPtdSales += bucket.SignSales * (tran.CuryDebitAmt - tran.CuryCreditAmt);
                accthist.CuryPtdDrAdjustments += bucket.SignDrMemos * (tran.CuryDebitAmt - tran.CuryCreditAmt);
                accthist.CuryPtdCrAdjustments += bucket.SignCrMemos * (tran.CuryDebitAmt - tran.CuryCreditAmt);
                accthist.CuryPtdFinCharges += bucket.SignFinCharges * (tran.CuryDebitAmt - tran.CuryCreditAmt);
                accthist.CuryPtdDiscounts += bucket.SignDiscTaken * (tran.CuryDebitAmt - tran.CuryCreditAmt);
                accthist.CuryYtdBalance += bucket.SignPtd * (tran.CuryDebitAmt - tran.CuryCreditAmt);
                accthist.CuryPtdDeposits += bucket.SignDeposits * (tran.CuryDebitAmt - tran.CuryCreditAmt);
                accthist.CuryYtdDeposits += bucket.SignDeposits * (tran.CuryDebitAmt - tran.CuryCreditAmt);
            }
        }

        private void CuryUpdateFinHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
            where History : class, ICuryARHist, IBaseARHist
        {
            CuryUpdateHist<History>(accthist, bucket, true, tran);
        }

        private void CuryUpdateTranHist<History>(History accthist, ARHistBucket bucket, GLTran tran)
            where History : class, ICuryARHist, IBaseARHist
        {
            CuryUpdateHist<History>(accthist, bucket, false, tran);
        }


        protected void UpdateItemDiscountsHistory(ARTran tran, ARRegister ardoc)
        {
            ARHistBucket bucket = new ARHistItemDiscountsBucket(tran);
            {
                ARHist accthist = CreateHistory(tran.BranchID, ardoc.ARAccountID, ardoc.ARSubID, ardoc.CustomerID, ardoc.FinPeriodID);
                if (accthist != null)
                {
                    UpdateFinHist<ARHist>(accthist, bucket, new GLTran { DebitAmt = tran.DiscAmt, CreditAmt = 0m });
                }
            }

            {
                ARHist accthist = CreateHistory(tran.BranchID, ardoc.ARAccountID, ardoc.ARSubID, ardoc.CustomerID, ardoc.TranPeriodID);
                if (accthist != null)
                {
                    UpdateTranHist<ARHist>(accthist, bucket, new GLTran { DebitAmt = tran.DiscAmt, CreditAmt = 0m });
                }
            }
        }

        private void UpdateHistory(GLTran tran, Customer cust)
        {
            string HistTranType = tran.TranType;
            if (tran.TranType == ARDocType.VoidPayment)
            {
                ARRegister doc = PXSelect<ARRegister, Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>, And<Where<ARRegister.docType, Equal<ARDocType.payment>, Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>, OrderBy<Asc<Switch<Case<Where<ARRegister.docType, Equal<ARDocType.payment>>, int0>, int1>, Asc<ARRegister.docType, Asc<ARRegister.refNbr>>>>>.Select(this, tran.RefNbr);
                if (doc != null)
                {
                    HistTranType = doc.DocType;
                }
            }

            ARHistBucket bucket = new ARHistBucket(tran, HistTranType);
            UpdateHistory(tran, cust, bucket);
        }

        private void UpdateHistory(GLTran tran, Customer cust, ARHistBucket bucket)
        {
            {
                ARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, tran.FinPeriodID);
                if (accthist != null)
                {
                    UpdateFinHist<ARHist>(accthist, bucket, tran);
                }
            }

            {
                ARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, tran.TranPeriodID);
                if (accthist != null)
                {
                    UpdateTranHist<ARHist>(accthist, bucket, tran);
                }
            }
        }

        private void UpdateHistory(GLTran tran, Customer cust, CurrencyInfo info)
        {
            string HistTranType = tran.TranType;
            if (tran.TranType == ARDocType.VoidPayment)
            {
                ARRegister doc = PXSelect<ARRegister, Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>, And<Where<ARRegister.docType, Equal<ARDocType.payment>, Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>, OrderBy<Asc<Switch<Case<Where<ARRegister.docType, Equal<ARDocType.payment>>, int0>, int1>, Asc<ARRegister.docType, Asc<ARRegister.refNbr>>>>>.Select(this, tran.RefNbr);
                if (doc != null)
                {
                    HistTranType = doc.DocType;
                }
            }

            ARHistBucket bucket = new ARHistBucket(tran, HistTranType);
            UpdateHistory(tran, cust, info, bucket);
        }

        private void UpdateHistory(GLTran tran, Customer cust, CurrencyInfo info, ARHistBucket bucket)
        {
            {
                CuryARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, info.CuryID, tran.FinPeriodID);
                if (accthist != null)
                {
                    CuryUpdateFinHist<CuryARHist>(accthist, bucket, tran);
                }
            }

            {
                CuryARHist accthist = CreateHistory(tran.BranchID, bucket.arAccountID, bucket.arSubID, cust.BAccountID, info.CuryID, tran.TranPeriodID);
                if (accthist != null)
                {
                    CuryUpdateTranHist<CuryARHist>(accthist, bucket, tran);
                }
            }
        }

        private List<ARRegister> CreateInstallments(PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res)
        {
            ARInvoice ardoc = (ARInvoice)res;
            CurrencyInfo info = (CurrencyInfo)res;
            Terms terms = (Terms)res;
            Customer customer = (Customer)res;
            List<ARRegister> ret = new List<ARRegister>();

            decimal CuryTotalInstallments = 0m;

            ARInvoiceEntry docgraph = PXGraph.CreateInstance<ARInvoiceEntry>();

            PXResultset<TermsInstallments> installments = TermsAttribute.SelectInstallments(this, terms, (DateTime)ardoc.DueDate);
            foreach (TermsInstallments inst in installments)
            {
                docgraph.customer.Current = customer;
                PXCache sender = ARInvoice_DocType_RefNbr.Cache;
                //force precision population
                object CuryOrigDocAmt = sender.GetValueExt(ardoc, "CuryOrigDocAmt");

                CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
                new_info.CuryInfoID = null;
                new_info = docgraph.currencyinfo.Insert(new_info);

                ARInvoice new_ardoc = PXCache<ARInvoice>.CreateCopy(ardoc);
                new_ardoc.CuryInfoID = new_info.CuryInfoID;
                new_ardoc.DueDate = ((DateTime)new_ardoc.DueDate).AddDays((double)inst.InstDays);
                new_ardoc.DiscDate = new_ardoc.DueDate;
                new_ardoc.InstallmentNbr = inst.InstallmentNbr;
                new_ardoc.MasterRefNbr = new_ardoc.RefNbr;
                new_ardoc.RefNbr = null;
                new_ardoc.ProjectID = ProjectDefaultAttribute.NonProject(docgraph);
                TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.NoCalc);

                if (inst.InstallmentNbr == installments.Count)
                {
                    new_ardoc.CuryOrigDocAmt = new_ardoc.CuryOrigDocAmt - CuryTotalInstallments;
                }
                else
                {
                    if (terms.InstallmentMthd == TermsInstallmentMethod.AllTaxInFirst)
                    {
                        new_ardoc.CuryOrigDocAmt = PXDBCurrencyAttribute.Round(sender, ardoc, (decimal)((ardoc.CuryOrigDocAmt - ardoc.CuryTaxTotal) * inst.InstPercent / 100m), CMPrecision.TRANCURY);
                        if (inst.InstallmentNbr == 1)
                        {
                            new_ardoc.CuryOrigDocAmt += (decimal)ardoc.CuryTaxTotal;
                        }
                    }
                    else
                    {
                        new_ardoc.CuryOrigDocAmt = PXDBCurrencyAttribute.Round(sender, ardoc, (decimal)(ardoc.CuryOrigDocAmt * inst.InstPercent / 100m), CMPrecision.TRANCURY);
                    }
                }
                new_ardoc.CuryDocBal = new_ardoc.CuryOrigDocAmt;
                new_ardoc.CuryLineTotal = new_ardoc.CuryOrigDocAmt;
                new_ardoc.CuryTaxTotal = 0m;
                new_ardoc.CuryOrigDiscAmt = 0m;
                new_ardoc.CuryVatTaxableTotal = 0m;
                new_ardoc.CuryDiscTot = 0m;
                new_ardoc.OrigModule = BatchModule.AR;
                new_ardoc = docgraph.Document.Insert(new_ardoc);
                CuryTotalInstallments += (decimal)new_ardoc.CuryOrigDocAmt;

                //Insert of ARInvoice causes the TaxZone to change thus setting the TaxCalc back to TaxCalc.Calc for the External (Avalara)
                //Set it back to NoCalc to avoid Document Totals recalculation on adding new transactions: 
                TaxAttribute.SetTaxCalc<ARTran.taxCategoryID>(docgraph.Transactions.Cache, null, TaxCalc.NoCalc);

                ARTran new_artran = new ARTran();
                new_artran.AccountID = new_ardoc.ARAccountID;
                new_artran.SubID = new_ardoc.ARSubID;
                new_artran.CuryTranAmt = new_ardoc.CuryOrigDocAmt;
                using (new PXLocaleScope(customer.LocaleName))
                {
                    new_artran.TranDesc = PXMessages.LocalizeNoPrefix(Messages.MultiplyInstallmentsTranDesc);
                }

                docgraph.Transactions.Insert(new_artran);

                foreach (ARSalesPerTran sptran in docgraph.salesPerTrans.Select())
                {
                    docgraph.salesPerTrans.Delete(sptran);
                }

                foreach (ARSalesPerTran sptran in PXSelect<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Required<ARSalesPerTran.docType>>, And<ARSalesPerTran.refNbr, Equal<Required<ARSalesPerTran.refNbr>>>>>.Select(this, ardoc.DocType, ardoc.RefNbr))
                {
                    ARSalesPerTran new_sptran = PXCache<ARSalesPerTran>.CreateCopy(sptran);
                    new_sptran.RefNbr = null;
                    new_sptran.CuryInfoID = new_info.CuryInfoID;

                    new_sptran.RefCntr = 999;
                    new_sptran.CuryCommnblAmt = PXDBCurrencyAttribute.Round(sender, ardoc, (decimal)(sptran.CuryCommnblAmt * inst.InstPercent / 100m), CMPrecision.TRANCURY);
                    new_sptran.CuryCommnAmt = PXDBCurrencyAttribute.Round(sender, ardoc, (decimal)(sptran.CuryCommnAmt * inst.InstPercent / 100m), CMPrecision.TRANCURY);
                    new_sptran = docgraph.salesPerTrans.Insert(new_sptran);
                }

                docgraph.Save.Press();

                ret.Add((ARRegister)docgraph.Document.Current);

                docgraph.Clear();
            }

            if (installments.Count > 0)
            {
                docgraph.Document.WhereNew<Where<ARInvoice.docType, Equal<Optional<ARInvoice.docType>>>>();
                docgraph.Document.Search<ARInvoice.refNbr>(ardoc.RefNbr, ardoc.DocType);
                docgraph.Document.Current.InstallmentCntr = Convert.ToInt16(installments.Count);
                docgraph.Document.Cache.SetStatus(docgraph.Document.Current, PXEntryStatus.Updated);

                docgraph.Save.Press();
                docgraph.Clear();
            }

            return ret;
        }

        private void SetClosedPeriodsFromLatestApplication(ARRegister doc, int? adjNbr)
        {
            // We should collect applications both from original and voiding documents
            // because in some cases their applications may have different periods
            //
            IEnumerable<string> docTypes = doc.PossibleOriginalDocumentTypes().Append(doc.DocType).Distinct();

            foreach (string docType in docTypes)
            {
                foreach (ARAdjust adj in PXSelect<ARAdjust,
                    Where2<Where<
                        ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
                            And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
                        Or<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
                            And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>>>>>,
                        And<Where<ARAdjust.released, Equal<True>,
                            Or<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>>
                    .Select(this, docType, doc.RefNbr, docType, doc.RefNbr, adjNbr))
                {
                    doc.ClosedFinPeriodID = PeriodIDAttribute.Max(doc.ClosedFinPeriodID, adj.AdjgFinPeriodID);
                    doc.ClosedTranPeriodID = PeriodIDAttribute.Max(doc.ClosedTranPeriodID, adj.AdjgTranPeriodID);
                }
            }

            doc.ClosedFinPeriodID = PeriodIDAttribute.Max(doc.ClosedFinPeriodID, doc.FinPeriodID);
            doc.ClosedTranPeriodID = PeriodIDAttribute.Max(doc.ClosedTranPeriodID, doc.TranPeriodID);
        }

        private void SetAdjgPeriodsFromLatestApplication(ARRegister doc, ARAdjust adj)
        {
            if (adj.VoidAppl == true)
            {
                // We should collect original applications to find max periods and dates,
                // because in some cases their values can be greater than values from voiding application
                //
                foreach (string adjgDocType in doc.PossibleOriginalDocumentTypes())
                {
                    ARAdjust orig = PXSelect<ARAdjust,
                        Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
                            And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
                            And<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
                            And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
                            And<ARAdjust.adjNbr, Equal<Required<ARAdjust.voidAdjNbr>>,
                            And<ARAdjust.released, Equal<True>>>>>>>>
                        .SelectSingleBound(this, null, adj.AdjdDocType, adj.AdjdRefNbr, adjgDocType, adj.AdjgRefNbr, adj.VoidAdjNbr);
                    if (orig != null)
                    {
                        adj.AdjgFinPeriodID = PeriodIDAttribute.Max(orig.AdjgFinPeriodID, adj.AdjgFinPeriodID);
                        adj.AdjgTranPeriodID = PeriodIDAttribute.Max(orig.AdjgTranPeriodID, adj.AdjgTranPeriodID);
                        adj.AdjgDocDate = PeriodIDAttribute.Max((DateTime)orig.AdjgDocDate, (DateTime)adj.AdjgDocDate);

                        break;
                    }
                }
            }

            adj.AdjgFinPeriodID = PeriodIDAttribute.Max(adj.AdjdFinPeriodID, adj.AdjgFinPeriodID);
            adj.AdjgTranPeriodID = PeriodIDAttribute.Max(adj.AdjdTranPeriodID, adj.AdjgTranPeriodID);
            adj.AdjgDocDate = PeriodIDAttribute.Max((DateTime)adj.AdjdDocDate, (DateTime)adj.AdjgDocDate);
        }

        private void CreatePayment(PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res, ref List<ARRegister> ret)
        {
            ret = ret ?? new List<ARRegister>();

            Lazy<ARPaymentEntry> lazyPaymentEntry = new Lazy<ARPaymentEntry>(() =>
            {
                ARPaymentEntry result = CreateInstance<ARPaymentEntry>();

                result.AutoPaymentApp = true;
                result.arsetup.Current.HoldEntry = false;
                result.arsetup.Current.RequireControlTotal = false;

                return result;
            });

            ARInvoice invoice = PXCache<ARInvoice>.CreateCopy(res);

            PXResultset<SOInvoice> invoicesNotCSL = PXSelectJoin<SOInvoice,
                InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
                    InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>,
                        LeftJoin<ARPayment, On<ARPayment.docType, Equal<SOInvoice.docType>,
                            And<ARPayment.refNbr, Equal<SOInvoice.refNbr>>>>>>,
                Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>,
                    And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>,
                        And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
                            And<ARPayment.refNbr, IsNull>>>>>.Select(this, invoice.DocType, invoice.RefNbr);

            if (invoicesNotCSL.Count() > 0)
            {
                foreach (SOInvoice soinvoice in invoicesNotCSL)
                {
                    PXSelectBase<CCProcTran> cmd = new PXSelectJoin<CCProcTran,
                        LeftJoin<SOOrderShipment, On<CCProcTran.origDocType, Equal<SOOrderShipment.orderType>,
                            And<CCProcTran.origRefNbr, Equal<SOOrderShipment.orderNbr>>>>,
                        Where<CCProcTran.docType, Equal<Required<CCProcTran.docType>>,
                            And<CCProcTran.refNbr, Equal<Required<CCProcTran.refNbr>>,
                                Or<Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
                                    And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>,
                                        And<CCProcTran.refNbr, IsNull,
                                            And<CCProcTran.docType, IsNull>>>>>>>,
                        OrderBy<Desc<CCProcTran.tranNbr>>>(this);

                    CCPaymentEntry.UpdateCapturedState<SOInvoice>(soinvoice, cmd.Select(soinvoice.DocType, soinvoice.RefNbr, soinvoice.DocType, soinvoice.RefNbr));

                    if (soinvoice.IsCCCaptured == true)
                    {
                        ARPaymentEntry docgraph = lazyPaymentEntry.Value;

                        if (((Terms)res).InstallmentType != TermsInstallmentType.Single)
                        {
                            throw new PXException(Messages.PrepaymentAppliedToMultiplyInstallments);
                        }

                        ARPayment payment = new ARPayment()
                        {
                            DocType = ARDocType.Payment,
                            AdjDate = soinvoice.AdjDate,
                            AdjFinPeriodID = soinvoice.AdjFinPeriodID
                        };

                        payment = PXCache<ARPayment>.CreateCopy(docgraph.Document.Insert(payment));
                        payment.CustomerID = invoice.CustomerID;
                        payment.CustomerLocationID = invoice.CustomerLocationID;
                        payment.ARAccountID = invoice.ARAccountID;
                        payment.ARSubID = invoice.ARSubID;

                        payment.PaymentMethodID = soinvoice.PaymentMethodID;
                        payment.PMInstanceID = soinvoice.PMInstanceID;
                        payment.CashAccountID = soinvoice.CashAccountID;
                        payment.ExtRefNbr = soinvoice.ExtRefNbr;
                        payment.CuryOrigDocAmt = soinvoice.CuryCCCapturedAmt;

                        docgraph.Document.Update(payment);

                        invoice.Released = true;
                        invoice.OpenDoc = true;
                        invoice.CuryDocBal = invoice.CuryOrigDocAmt;
                        invoice.DocBal = invoice.OrigDocAmt;
                        invoice.CuryDiscBal = invoice.CuryOrigDiscAmt;
                        invoice.DiscBal = invoice.OrigDiscAmt;

                        docgraph.Caches[typeof(ARInvoice)].SetStatus(invoice, PXEntryStatus.Held);


                        decimal? _CuryAdjgAmt = payment.CuryOrigDocAmt > invoice.CuryDocBal ? invoice.CuryDocBal : payment.CuryOrigDocAmt;
                        decimal? _CuryAdjgDiscAmt = payment.CuryOrigDocAmt > invoice.CuryDocBal ? 0m : invoice.CuryDiscBal;

                        if (_CuryAdjgDiscAmt + _CuryAdjgAmt > invoice.CuryDocBal)
                        {
                            _CuryAdjgDiscAmt = invoice.CuryDocBal - _CuryAdjgAmt;
                        }

                        ARAdjust adj = new ARAdjust()
                        {
                            AdjdDocType = soinvoice.DocType,
                            AdjdRefNbr = soinvoice.RefNbr,
                            CuryAdjgAmt = _CuryAdjgAmt,
                            CuryAdjgDiscAmt = _CuryAdjgDiscAmt
                        };

                        docgraph.Adjustments.Insert(adj);

                        using (PXTransactionScope ts = new PXTransactionScope())
                        {
                            docgraph.Save.Press();

                            PXDatabase.Update<CCProcTran>(
                                new PXDataFieldAssign("DocType", docgraph.Document.Current.DocType),
                                new PXDataFieldAssign("RefNbr", docgraph.Document.Current.RefNbr),
                                new PXDataFieldRestrict("DocType", PXDbType.Char, 3, soinvoice.DocType, PXComp.EQ),
                                new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, soinvoice.RefNbr, PXComp.EQ)
                                );

                            int i = 0;
                            bool ccproctranupdated = false;

                            foreach (SOOrderShipment order in PXSelect<SOOrderShipment,
                                Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
                                    And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>.Select(docgraph, soinvoice.DocType, soinvoice.RefNbr))
                            {
                                ccproctranupdated |= PXDatabase.Update<CCProcTran>(
                                    new PXDataFieldAssign("DocType", docgraph.Document.Current.DocType),
                                    new PXDataFieldAssign("RefNbr", docgraph.Document.Current.RefNbr),
                                    new PXDataFieldRestrict("OrigDocType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
                                    new PXDataFieldRestrict("OrigRefNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ),
                                    new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, null, PXComp.ISNULL)
                                    );

                                if (ccproctranupdated && i > 0)
                                {
                                    throw new PXException(Messages.ERR_CCMultiplyPreauthCombined);
                                }

                                i++;
                            }

                            ts.Complete();
                        }

                        ret.Add(docgraph.Document.Current);

                        docgraph.Clear();
                    }
                }
            }
            else
            {
                PXResultset<SOInvoice> invoicesCSL = PXSelectJoin<SOInvoice,
                InnerJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.pMInstanceID, Equal<SOInvoice.pMInstanceID>>,
                    InnerJoin<PaymentMethod, On<PaymentMethod.paymentMethodID, Equal<CustomerPaymentMethod.paymentMethodID>>,
                        LeftJoin<ARPayment, On<ARPayment.docType, Equal<SOInvoice.docType>,
                            And<ARPayment.refNbr, Equal<SOInvoice.refNbr>>>>>>,
                Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>,
                    And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>,
                        And<PaymentMethod.paymentType, Equal<PaymentMethodType.creditCard>,
                            And<ARPayment.refNbr, IsNotNull>>>>>.Select(this, invoice.DocType, invoice.RefNbr);

                foreach (PXResult<SOInvoice, CustomerPaymentMethod, PaymentMethod, ARPayment> csls in invoicesCSL)
                {
                    SOInvoice currInvoice = csls;
                    ARPayment currPayment = csls;

                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        PXDatabase.Update<CCProcTran>(
                            new PXDataFieldAssign("DocType", currPayment.DocType),
                            new PXDataFieldAssign("RefNbr", currPayment.RefNbr),
                            new PXDataFieldRestrict("DocType", PXDbType.Char, 3, currInvoice.DocType, PXComp.EQ),
                            new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, currInvoice.RefNbr, PXComp.EQ)
                            );

                        int i = 0;
                        bool ccproctranupdated = false;

                        foreach (SOOrderShipment order in PXSelect<SOOrderShipment,
                            Where<SOOrderShipment.invoiceType, Equal<Required<SOOrderShipment.invoiceType>>,
                                And<SOOrderShipment.invoiceNbr, Equal<Required<SOOrderShipment.invoiceNbr>>>>>.Select(this, currInvoice.DocType, currInvoice.RefNbr))
                        {
                            ccproctranupdated |= PXDatabase.Update<CCProcTran>(
                                new PXDataFieldAssign("DocType", currPayment.DocType),
                                new PXDataFieldAssign("RefNbr", currPayment.RefNbr),
                                new PXDataFieldRestrict("OrigDocType", PXDbType.Char, 3, order.OrderType, PXComp.EQ),
                                new PXDataFieldRestrict("OrigRefNbr", PXDbType.NVarChar, 15, order.OrderNbr, PXComp.EQ),
                                new PXDataFieldRestrict("RefNbr", PXDbType.NVarChar, 15, null, PXComp.ISNULL)
                                );

                            if (ccproctranupdated && i > 0)
                            {
                                throw new PXException(Messages.ERR_CCMultiplyPreauthCombined);
                            }

                            i++;
                        }

                        ts.Complete();
                    }
                }
            }
        }

        private void UpdateARBalancesDates(ARRegister ardoc)
        {
            ARBalances arbal = (ARBalances)Caches[typeof(ARBalances)].Insert(new ARBalances
            {
                BranchID = ardoc.BranchID,
                CustomerID = ardoc.CustomerID,
                CustomerLocationID = ardoc.CustomerLocationID
            });

            if (ardoc.DueDate != null && (arbal.OldInvoiceDate == null || ardoc.DueDate <= arbal.OldInvoiceDate))
            {
                if (ardoc.OpenDoc == true && ardoc.OrigDocAmt > 0)
                {
                    arbal.OldInvoiceDate = ardoc.DueDate;
                }
            }
        }

        private void UpdateARBalancesDates(ARRegister invoice, int rowCount)
        {
            ARBalances arbal = (ARBalances)Caches[typeof(ARBalances)].Insert(new ARBalances
            {
                BranchID = invoice.BranchID,
                CustomerID = invoice.CustomerID,
                CustomerLocationID = invoice.CustomerLocationID
            });

            if (invoice.DueDate != null && (arbal.OldInvoiceDate == null || invoice.DueDate <= arbal.OldInvoiceDate))
            {
                if (invoice.OpenDoc == true)
                {
                    if (invoice.OrigDocAmt > 0)
                    {
                        arbal.OldInvoiceDate = invoice.DueDate;
                    }
                }
                else
                {
                    if (arbal.DatesUpdated != true)
                    {
                        arbal.tstamp = PXDatabase.SelectTimeStamp();
                        arbal.DatesUpdated = true;
                    }
                    arbal.OldInvoiceDate = null;

                    foreach (ARInvoice olddoc in PXSelectReadonly<ARInvoice,
                        Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>,
                        And<ARInvoice.customerLocationID, Equal<Required<ARInvoice.customerLocationID>>,
                        And<ARInvoice.branchID, Equal<Required<ARInvoice.branchID>>,
                        And<ARInvoice.released, Equal<True>,
                        And<ARInvoice.openDoc, Equal<True>,
                        And<ARInvoice.dueDate, IsNotNull,
                        And<ARInvoice.origDocAmt, Greater<decimal0>>>>>>>>,
                        OrderBy<Asc<ARInvoice.dueDate>>>
                        .SelectWindowed(this, 0, rowCount + 1, invoice.CustomerID, invoice.CustomerLocationID, invoice.BranchID))
                    {
                        ARRegister cached = (ARRegister)ARDocument.Cache.Locate(olddoc);
                        if (cached == null || (bool)cached.OpenDoc)
                        {
                            arbal.OldInvoiceDate = olddoc.DueDate;
                            break;
                        }
                    }
                }
            }
        }

        public static decimal? RoundAmount(decimal? amount, string RoundType, decimal? precision)
        {
            decimal? toround = amount / precision;

            switch (RoundType)
            {
                case RoundingType.Floor:
                    return Math.Floor((decimal)toround) * precision;
                case RoundingType.Ceil:
                    return Math.Ceiling((decimal)toround) * precision;
                case RoundingType.Mathematical:
                    return Math.Round((decimal)toround, 0, MidpointRounding.AwayFromZero) * precision;
                default:
                    return amount;
            }
        }

        protected virtual decimal? RoundAmount(decimal? amount)
        {
            return RoundAmount(amount, this.InvoiceRounding, this.InvoicePrecision);
        }

        public virtual List<ARRegister> ReleaseDocProc(JournalEntry je, ref ARRegister doc, PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res, out PM.PMRegister pmDoc)
        {
            pmDoc = null;
            ARInvoice ardoc = (ARInvoice)res;
            CurrencyInfo info = (CurrencyInfo)res;
            Terms terms = (Terms)res;
            Customer vend = (Customer)res;
            Account arAccount = (Account)res;

            List<ARRegister> ret = null;

            if ((bool)doc.Released == false)
            {
                if (_IsIntegrityCheck == false)
                {
                    if ((bool)arsetup.PrintBeforeRelease && ardoc.Printed != true && ardoc.DontPrint != true)
                        throw new PXException(Messages.Invoice_NotPrinted_CannotRelease);
                    if ((bool)arsetup.EmailBeforeRelease && ardoc.Emailed != true && ardoc.DontEmail != true)
                        throw new PXException(Messages.Invoice_NotEmailed_CannotRelease);
                }

                if (ardoc.CreditHold == true)
                {
                    throw new PXException(
                        Messages.InvoiceCreditHoldCannotRelease,
                        GetLabel.For<ARDocType>(ardoc.DocType),
                        ardoc.RefNbr);
                }

                string _InstallmentType = terms.InstallmentType;

                if (_IsIntegrityCheck && ardoc.InstallmentNbr == null)
                {
                    _InstallmentType = ardoc.InstallmentCntr != null
                        ? _InstallmentType = TermsInstallmentType.Multiple
                        : _InstallmentType = TermsInstallmentType.Single;
                }

                if (_InstallmentType == TermsInstallmentType.Multiple && (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn))
                {
                    throw new PXException(Messages.Cash_Sale_Cannot_Have_Multiply_Installments);
                }

                if (_InstallmentType == TermsInstallmentType.Multiple && ardoc.InstallmentNbr == null)
                {
                    if (_IsIntegrityCheck == false)
                    {
                        ret = CreateInstallments(res);
                    }
                    doc.CuryDocBal = 0m;
                    doc.DocBal = 0m;
                    doc.CuryDiscBal = 0m;
                    doc.DiscBal = 0m;
                    doc.CuryDiscTaken = 0m;
                    doc.DiscTaken = 0m;

                    doc.OpenDoc = false;
                    doc.ClosedFinPeriodID = doc.FinPeriodID;
                    doc.ClosedTranPeriodID = doc.TranPeriodID;

                    UpdateARBalances(this, doc, -1m * doc.OrigDocAmt, 0m);
                }
                else
                {
                    doc.CuryDocBal = doc.CuryOrigDocAmt;
                    doc.DocBal = doc.OrigDocAmt;
                    doc.CuryDiscBal = doc.CuryOrigDiscAmt;
                    doc.DiscBal = doc.OrigDiscAmt;
                    doc.CuryDiscTaken = 0m;
                    doc.DiscTaken = 0m;
                    doc.RGOLAmt = 0m;

                    doc.OpenDoc = true;
                    doc.ClosedFinPeriodID = null;
                    doc.ClosedTranPeriodID = null;

                    UpdateARBalancesDates(ardoc);
                }

                //should always restore ARRegister to ARInvoice after above assignments
                PXCache<ARRegister>.RestoreCopy(ardoc, doc);

                CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
                new_info.CuryInfoID = null;
                new_info.ModuleCode = "GL";
                new_info.BaseCalc = false;
                new_info = je.currencyinfo.Insert(new_info) ?? new_info;

                if (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn)
                {
                    GLTran tran = new GLTran();
                    tran.SummPost = true;
                    tran.ZeroPost = false;
                    tran.BranchID = ardoc.BranchID;
                    tran.AccountID = ardoc.ARAccountID;
                    tran.ReclassificationProhibited = true;
                    tran.SubID = ardoc.ARSubID;
                    tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.CuryOrigDocAmt + ardoc.CuryOrigDiscAmt;
                    tran.DebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.OrigDocAmt + ardoc.OrigDiscAmt;
                    tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.CuryOrigDocAmt + ardoc.CuryOrigDiscAmt : 0m;
                    tran.CreditAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.OrigDocAmt + ardoc.OrigDiscAmt : 0m;
                    tran.TranType = ardoc.DocType;
                    tran.TranClass = GLTran.tranClass.Normal;
                    tran.RefNbr = ardoc.RefNbr;
                    tran.TranDesc = ardoc.DocDesc;
                    tran.TranPeriodID = ardoc.TranPeriodID;
                    tran.FinPeriodID = ardoc.FinPeriodID;
                    tran.TranDate = ardoc.DocDate;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;

                    if (arAccount != null && arAccount.AccountGroupID != null && ardoc.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(this, ardoc.ProjectID))
                    {
                        PMAccountTask mapping = PXSelect<PMAccountTask,
                            Where<PMAccountTask.projectID, Equal<Required<PMAccountTask.projectID>>,
                                And<PMAccountTask.accountID, Equal<Required<PMAccountTask.accountID>>>>>.Select(this, ardoc.ProjectID, ardoc.ARAccountID);

                        if (mapping == null)
                        {
                            throw new PXException(Messages.AccounTaskMappingNotFound);
                        }

                        tran.ProjectID = ardoc.ProjectID;
                        tran.TaskID = mapping.TaskID;
                    }
                    else
                    {
                        tran.ProjectID = ProjectDefaultAttribute.NonProject(this);
                        tran.TaskID = null;
                    }

                    //no history update should take place
                    je.GLTranModuleBatNbr.Insert(tran);
                }
                else
                {
                    GLTran tran = new GLTran();
                    tran.SummPost = true;
                    tran.BranchID = ardoc.BranchID;
                    tran.AccountID = ardoc.ARAccountID;
                    tran.ReclassificationProhibited = true;
                    tran.SubID = ardoc.ARSubID;
                    tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.CuryOrigDocAmt;
                    tran.DebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.OrigDocAmt - ardoc.RGOLAmt;
                    tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.CuryOrigDocAmt : 0m;
                    tran.CreditAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.OrigDocAmt - ardoc.RGOLAmt : 0m;
                    tran.TranType = ardoc.DocType;
                    tran.TranClass = GLTran.tranClass.Normal;
                    tran.RefNbr = ardoc.RefNbr;
                    tran.TranDesc = ardoc.DocDesc;
                    tran.TranPeriodID = ardoc.TranPeriodID;
                    tran.FinPeriodID = ardoc.FinPeriodID;
                    tran.TranDate = ardoc.DocDate;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;

                    if (arAccount != null && arAccount.AccountGroupID != null && ardoc.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(this, ardoc.ProjectID))
                    {
                        PMAccountTask mapping = PXSelect<PMAccountTask,
                            Where<PMAccountTask.projectID, Equal<Required<PMAccountTask.projectID>>,
                                And<PMAccountTask.accountID, Equal<Required<PMAccountTask.accountID>>>>>.Select(this, ardoc.ProjectID, ardoc.ARAccountID);

                        if (mapping == null)
                        {
                            throw new PXException(Messages.ARAccountTaskMappingNotFound, arAccount.AccountCD);
                        }

                        tran.ProjectID = ardoc.ProjectID;
                        tran.TaskID = mapping.TaskID;
                    }
                    else
                    {
                        tran.ProjectID = ProjectDefaultAttribute.NonProject(this);
                        tran.TaskID = null;
                    }

                    //if (ardoc.InstallmentNbr == null || ardoc.InstallmentNbr == 0)
                    if ((bool)doc.OpenDoc)
                    {
                        UpdateHistory(tran, vend);
                        UpdateHistory(tran, vend, info);
                    }

                    je.GLTranModuleBatNbr.Insert(tran);
                }
                ARTran prev_n = new ARTran();

                foreach (PXResult<ARTran, ARTax, Tax, DRDeferredCode, PMTran, SO.SOOrderType> r in ARTran_TranType_RefNbr.Select((object)ardoc.DocType, ardoc.RefNbr))
                {
                    ARTran n = r;
                    SO.SOOrderType sotype = r;

                    n.TranDate = ardoc.DocDate;
                    n.FinPeriodID = ardoc.FinPeriodID;

                    if (_IsIntegrityCheck == false && sotype.PostLineDiscSeparately == true && sotype.DiscountAcctID != null && n.CuryDiscAmt > 0.00005m && !object.Equals(prev_n, n))
                    {
                        ARTran new_tran = PXCache<ARTran>.CreateCopy(n);
                        new_tran.InventoryID = null;
                        new_tran.TaxCategoryID = null;
                        new_tran.SalesPersonID = null;
                        new_tran.UOM = null;
                        new_tran.LineType = SO.SOLineType.Discount;
                        using (new PXLocaleScope(vend.LocaleName))
                            new_tran.TranDesc = PXMessages.LocalizeNoPrefix(SO.Messages.LineDiscDescr);
                        new_tran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<ARTran.lineNbr>(ARTran_TranType_RefNbr.Cache, doc);
                        new_tran.CuryTranAmt = PXDBCurrencyAttribute.RoundCury<ARTran.curyInfoID>(ARTran_TranType_RefNbr.Cache, new_tran, (decimal)new_tran.CuryDiscAmt * (decimal)(n.CuryTranAmt != 0m && n.CuryTaxableAmt.GetValueOrDefault() != 0 ? n.CuryTaxableAmt / n.CuryTranAmt : 1m));
                        new_tran.CuryExtPrice = PXDBCurrencyAttribute.RoundCury<ARTran.curyInfoID>(ARTran_TranType_RefNbr.Cache, new_tran, (decimal)new_tran.CuryDiscAmt * (decimal)(n.CuryTranAmt != 0m && n.CuryTaxableAmt.GetValueOrDefault() != 0 ? n.CuryTaxableAmt / n.CuryTranAmt : 1m));
                        PXDBCurrencyAttribute.CalcBaseValues<ARTran.curyTranAmt>(ARTran_TranType_RefNbr.Cache, new_tran);
                        PXDBCurrencyAttribute.CalcBaseValues<ARTran.curyExtPrice>(ARTran_TranType_RefNbr.Cache, new_tran);
                        new_tran.CuryDiscAmt = 0m;
                        new_tran.Qty = 0m;
                        new_tran.DiscPct = 0m;
                        new_tran.CuryUnitPrice = 0m;
                        new_tran.DiscountID = null;
                        new_tran.DiscountSequenceID = null;
                        new_tran.DetDiscIDC1 = null;
                        new_tran.DetDiscIDC2 = null;
                        new_tran.DetDiscSeqIDC1 = null;
                        new_tran.DetDiscSeqIDC2 = null;
                        new_tran.DocDiscIDC1 = null;
                        new_tran.DocDiscIDC2 = null;
                        new_tran.DocDiscSeqIDC1 = null;
                        new_tran.DocDiscSeqIDC2 = null;

                        ARTran_TranType_RefNbr.Cache.Insert(new_tran);

                        new_tran = PXCache<ARTran>.CreateCopy(n);
                        new_tran.InventoryID = null;
                        new_tran.TaxCategoryID = null;
                        new_tran.SalesPersonID = null;
                        new_tran.UOM = null;
                        new_tran.LineType = SO.SOLineType.Discount;
                        using (new PXLocaleScope(vend.LocaleName))
                            new_tran.TranDesc = PXMessages.LocalizeNoPrefix(SO.Messages.LineDiscDescr);

                        switch (sotype.DiscAcctDefault)
                        {
                            case SO.SODiscAcctSubDefault.OrderType:
                                new_tran.AccountID = sotype.DiscountAcctID;
                                break;
                            case SO.SODiscAcctSubDefault.MaskLocation:
                                {
                                    Location customerloc = PXSelect<Location,
                                                                    Where<Location.bAccountID, Equal<Required<ARInvoice.customerID>>,
                                                                        And<Location.locationID, Equal<Required<ARInvoice.customerLocationID>>>>>
                                                               .Select(this, ardoc.CustomerID, ardoc.CustomerLocationID);
                                    if (customerloc != null)
                                    {
                                        if (customerloc.CDiscountAcctID != null)
                                            new_tran.AccountID = customerloc.CDiscountAcctID;
                                        else
                                        {
                                            if (PXAccess.FeatureInstalled<FeaturesSet.accountLocations>())
                                                throw new PXException(IN.Messages.DiscountAccountIsNotSetupLocation, customerloc.LocationCD, Caches[typeof(ARInvoice)].GetValueExt<ARInvoice.customerID>(ardoc).ToString());
                                            else
                                                throw new PXException(IN.Messages.DiscountAccountIsNotSetupCustomer, Caches[typeof(ARInvoice)].GetValueExt<ARInvoice.customerID>(ardoc).ToString());
                                        }
                                    }
                                }
                                break;
                        }

                        new_tran.LineNbr = (int?)PXLineNbrAttribute.NewLineNbr<ARTran.lineNbr>(ARTran_TranType_RefNbr.Cache, doc);
                        new_tran.CuryTranAmt = -PXDBCurrencyAttribute.RoundCury<ARTran.curyInfoID>(ARTran_TranType_RefNbr.Cache, new_tran, (decimal)new_tran.CuryDiscAmt * (decimal)(n.CuryTranAmt != 0m && n.CuryTaxableAmt.GetValueOrDefault() != 0 ? n.CuryTaxableAmt / n.CuryTranAmt : 1m));
                        new_tran.CuryExtPrice = -PXDBCurrencyAttribute.RoundCury<ARTran.curyInfoID>(ARTran_TranType_RefNbr.Cache, new_tran, (decimal)new_tran.CuryDiscAmt * (decimal)(n.CuryTranAmt != 0m && n.CuryTaxableAmt.GetValueOrDefault() != 0 ? n.CuryTaxableAmt / n.CuryTranAmt : 1m));
                        PXDBCurrencyAttribute.CalcBaseValues<ARTran.curyTranAmt>(ARTran_TranType_RefNbr.Cache, new_tran);
                        PXDBCurrencyAttribute.CalcBaseValues<ARTran.curyExtPrice>(ARTran_TranType_RefNbr.Cache, new_tran);
                        new_tran.CuryDiscAmt = 0m;
                        new_tran.DiscPct = 0m;
                        new_tran.Qty = 0m;
                        new_tran.CuryUnitPrice = 0m;
                        new_tran.DiscountID = null;
                        new_tran.DiscountSequenceID = null;
                        new_tran.DetDiscIDC1 = null;
                        new_tran.DetDiscIDC2 = null;
                        new_tran.DetDiscSeqIDC1 = null;
                        new_tran.DetDiscSeqIDC2 = null;
                        new_tran.DocDiscIDC1 = null;
                        new_tran.DocDiscIDC2 = null;
                        new_tran.DocDiscSeqIDC1 = null;
                        new_tran.DocDiscSeqIDC2 = null;

                        if (sotype.UseDiscountSubFromSalesSub == false)
                        {
                            Location customerloc = PXSelect<Location,
                                                            Where<Location.bAccountID, Equal<Required<ARInvoice.customerID>>,
                                                                And<Location.locationID, Equal<Required<ARInvoice.customerLocationID>>>>>
                                                       .Select(this, ardoc.CustomerID, ardoc.CustomerLocationID);
                            CRLocation companyloc = PXSelectJoin<CRLocation,
                                                                         InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>,
                                                                             And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
                                                                         InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>>,
                                                                         Where<Branch.branchID, Equal<Required<ARRegister.branchID>>>>
                                                      .Select(this, ardoc.BranchID);
                            object ordertype_SubID = GetValue<SO.SOOrderType.discountSubID>(sotype);
                            object customer_Location = GetValue<Location.cDiscountSubID>(customerloc);
                            object company_Location = GetValue<CRLocation.cMPDiscountSubID>(companyloc);

                            //if (customer_Location != null && company_Location != null)
                            {
                                object value = SO.SODiscSubAccountMaskAttribute.MakeSub<SO.SOOrderType.discSubMask>(this,
                                                                                                                    sotype.DiscSubMask,
                                                                                                                    new object[]
                                                                                                                    {
                                                                                                                        ordertype_SubID,
                                                                                                                        customer_Location,
                                                                                                                        company_Location
                                                                                                                    },
                                                                                                                    new Type[]
                                                                                                                    {
                                                                                                                        typeof(SO.SOOrderType.discountSubID),
                                                                                                                        typeof(Location.cDiscountSubID),
                                                                                                                        typeof(Location.cMPDiscountSubID)
                                                                                                                    });
                                ARTran_TranType_RefNbr.Cache.RaiseFieldUpdating<ARTran.subID>(new_tran, ref value);
                                new_tran.SubID = (int?)value;
                            }
                        }

                        ARTran_TranType_RefNbr.Cache.Insert(new_tran);
                    }
                    prev_n = n;
                }

                List<PXResult<ARTran, PMTran>> creditMemoPMReversal = new List<PXResult<ARTran, PMTran>>();
                List<PXResult<ARTran, PMTran>> nonglBillLater = new List<PXResult<ARTran, PMTran>>();
                List<PXResult<ARTran, PMTran>> nonglReverse = new List<PXResult<ARTran, PMTran>>();
                prev_n = new ARTran();

                foreach (PXResult<ARTran, ARTax, Tax, DRDeferredCode, PMTran, SO.SOOrderType> r in ARTran_TranType_RefNbr.Select((object)ardoc.DocType, ardoc.RefNbr))
                {
                    ARTran n = r;
                    ARTax x = r;
                    Tax salestax = r;
                    DRDeferredCode defcode = r;
                    PMTran pmtran = r;

                    if (!object.Equals(prev_n, n) && _IsIntegrityCheck == false && n.Released == true)
                    {
                        throw new PXException(Messages.Document_Status_Invalid);
                    }

                    if (!object.Equals(prev_n, n))
                    {
                        GLTran tran = new GLTran();
                        tran.SummPost = this.SummPost && string.IsNullOrEmpty(n.PMDeltaOption);
                        tran.BranchID = n.BranchID;
                        tran.CuryInfoID = new_info.CuryInfoID;
                        tran.TranType = n.TranType;
                        tran.TranClass = ardoc.DocClass;
                        tran.RefNbr = n.RefNbr;
                        tran.InventoryID = n.InventoryID;
                        tran.UOM = n.UOM;
                        tran.Qty = (n.DrCr == DrCr.Credit) ? n.Qty : -1 * n.Qty;
                        tran.TranDate = n.TranDate;
                        tran.ProjectID = n.ProjectID;
                        tran.TaskID = n.TaskID;
                        tran.AccountID = n.AccountID;
                        tran.SubID = GetValueInt<ARTran.subID>(je, n);
                        tran.TranDesc = n.TranDesc;
                        tran.Released = true;
                        tran.ReferenceID = ardoc.CustomerID;
                        tran.TranLineNbr = (tran.SummPost == true) ? null : n.LineNbr;

                        Amount postedAmount = GetSalesPostingAmount(this, doc, n, x, salestax, amount => PXDBCurrencyAttribute.Round(je.GLTranModuleBatNbr.Cache, tran, amount, CMPrecision.TRANCURY));

                        tran.CuryDebitAmt = (n.DrCr == DrCr.Debit) ? postedAmount.Cury : 0m;
                        tran.DebitAmt = (n.DrCr == DrCr.Debit) ? postedAmount.Base : 0m;
                        tran.CuryCreditAmt = (n.DrCr == DrCr.Debit) ? 0m : postedAmount.Cury;
                        tran.CreditAmt = (n.DrCr == DrCr.Debit) ? 0m : postedAmount.Base;

                        if (_IsIntegrityCheck == false)
                        {
                            bool transactionsAdded = false;

                            if (defcode != null && defcode.DeferredCodeID != null)
                            {
                                DRProcess dr = PXGraph.CreateInstance<DRProcess>();

                                dr.CreateSchedule(n, defcode, ardoc, postedAmount.Base.Value, isDraft: false);
                                dr.Actions.PressSave();

                                var transactions = je.CreateTransBySchedule(dr, tran);
                                je.CorrectCuryAmountsDueToRounding(transactions, tran, postedAmount.Cury.Value);

                                foreach (var generated in transactions)
                                {
                                    je.GLTranModuleBatNbr.Insert(generated);
                                }

                                transactionsAdded = transactions.Any();
                            }

                            if (transactionsAdded == false)
                            {
                                je.GLTranModuleBatNbr.Insert(tran);
                            }
                        }

                        UpdateItemDiscountsHistory(n, ardoc);

                        if (!_IsIntegrityCheck)
                        {
                            bool tranCostSet = false;
                            if (n.LineType == SO.SOLineType.Inventory || n.LineType == SO.SOLineType.NonInventory)
                            {
                                n.TranCost = 0m;

                                foreach (INTran intran in PXSelect<INTran,
                                    Where<INTran.sOShipmentType, Equal<Current<ARTran.sOShipmentType>>, And<INTran.sOShipmentNbr, Equal<Current<ARTran.sOShipmentNbr>>,
                                    And<INTran.sOOrderType, Equal<Current<ARTran.sOOrderType>>, And<INTran.sOOrderNbr, Equal<Current<ARTran.sOOrderNbr>>, And<INTran.sOOrderLineNbr, Equal<Current<ARTran.sOOrderLineNbr>>>>>>>>.SelectMultiBound(this, new object[] { n }))
                                {
                                    intran.ARDocType = n.TranType;
                                    intran.ARRefNbr = n.RefNbr;
                                    intran.ARLineNbr = n.LineNbr;
                                    intran.UnitPrice = n.UnitPrice;
                                    intran.TranAmt = Math.Sign((decimal)n.Qty) != Math.Sign((decimal)n.BaseQty) ? -n.TranAmt : n.TranAmt;

                                    if (n.Qty != 0m && n.SOShipmentLineNbr == null)
                                    {
                                        object TranAmt = Math.Sign((decimal)n.Qty) != Math.Sign((decimal)n.BaseQty) ? -n.TranAmt : n.TranAmt / n.Qty * intran.Qty;
                                        this.intranselect.Cache.RaiseFieldUpdating<INTran.tranAmt>(intran, ref TranAmt);
                                        intran.TranAmt = (decimal?)TranAmt;
                                    }

                                    this.intranselect.Cache.SetStatus(intran, PXEntryStatus.Updated);

                                    if (intran.Released == true)
                                    {
                                        IN.InventoryItem initem = PXSelect<IN.InventoryItem,
                                            Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.SelectSingleBound(this, null, n.InventoryID);

                                        if (initem != null && initem.StkItem != true && initem.KitItem == true && PXAccess.FeatureInstalled<FeaturesSet.kitAssemblies>() && SOSetup != null && SOSetup.Current != null)
                                        {
                                            switch (SOSetup.Current.SalesProfitabilityForNSKits)
                                            {
                                                case SalesProfitabilityNSKitMethod.NSKitStandardCostOnly: //do nothing 
                                                    break;
                                                case SalesProfitabilityNSKitMethod.NSKitStandardAndStockComponentsCost: //kit standard cost will be added later
                                                    n.TranCost += intran.TranCost;
                                                    n.TranCostOrig = n.TranCost;
                                                    n.IsTranCostFinal = true;
                                                    break;
                                                case SalesProfitabilityNSKitMethod.StockComponentsCostOnly:
                                                    n.TranCost += intran.TranCost;
                                                    n.TranCostOrig = n.TranCost;
                                                    n.IsTranCostFinal = true;
                                                    tranCostSet = true;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            n.TranCost += intran.TranCost;
                                            n.TranCostOrig = n.TranCost;
                                            n.IsTranCostFinal = true;
                                            tranCostSet = true;
                                        }
                                    }

                                    if (intran.UpdateShippedNotInvoiced == true)
                                    {
                                        PXResultset<IN.INTranCost> trancosts = PXSelect<IN.INTranCost, Where<IN.INTranCost.costDocType, Equal<Required<IN.INTranCost.costDocType>>, And<IN.INTranCost.costRefNbr, Equal<Required<IN.INTranCost.costRefNbr>>, And<IN.INTranCost.lineNbr, Equal<Required<IN.INTranCost.lineNbr>>>>>>.Select(this, intran.DocType, intran.RefNbr, intran.LineNbr);
                                        if (trancosts.Count < 1) throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.ShippedNotInvoicedINtranNotReleased, intran.RefNbr));
                                        foreach (IN.INTranCost trancost in trancosts)
                                        {
                                            PXResult<IN.InventoryItem, IN.INPostClass> itemPostClassRes = (PXResult<IN.InventoryItem, IN.INPostClass>)PXSelectJoin<IN.InventoryItem, LeftJoin<IN.INPostClass, On<IN.INPostClass.postClassID, Equal<IN.InventoryItem.postClassID>>>, Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.Select(this, intran.InventoryID);
                                            IN.INSite site = PXSelect<IN.INSite, Where<IN.INSite.siteID, Equal<Required<IN.INSite.siteID>>>>.Select(this, intran.SiteID);

                                            if (trancost != null && trancost.COGSAcctID != null && intran != null)
                                            {
                                                //Credit shipped-not-invoiced account
                                                bool isControlAccount = false;
                                                GLTran tranFromIN = new GLTran();
                                                tranFromIN.SummPost = this.SummPost;
                                                tranFromIN.BranchID = intran.BranchID;
                                                tran.CuryInfoID = new_info.CuryInfoID;
                                                tranFromIN.TranType = trancost.TranType;
                                                tranFromIN.TranClass = GLTran.tranClass.Normal;

                                                tranFromIN.AccountID = intran.COGSAcctID ?? IN.INReleaseProcess.GetAccountDefaults<IN.INPostClass.cOGSAcctID>(this, (IN.InventoryItem)itemPostClassRes, site, (IN.INPostClass)itemPostClassRes, intran, out isControlAccount);
                                                tranFromIN.SubID = intran.COGSSubID ?? IN.INReleaseProcess.GetAccountDefaults<IN.INPostClass.cOGSSubID>(this, (IN.InventoryItem)itemPostClassRes, site, (IN.INPostClass)itemPostClassRes, intran, out isControlAccount);

                                                tranFromIN.CuryDebitAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost : 0m;
                                                tranFromIN.DebitAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost : 0m;
                                                tranFromIN.CuryCreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost;
                                                tranFromIN.CreditAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost;

                                                tranFromIN.RefNbr = trancost.RefNbr;
                                                tranFromIN.InventoryID = trancost.InventoryID;
                                                tranFromIN.Qty = (trancost.InvtMult == (short)1) ? trancost.Qty : -trancost.Qty;
                                                tranFromIN.UOM = intran.UOM;
                                                tranFromIN.TranDesc = intran.TranDesc;
                                                tranFromIN.TranDate = n.TranDate;
                                                bool isNonProject = trancost.InvtMult == (short)0;
                                                tranFromIN.ProjectID = isNonProject ? PM.ProjectDefaultAttribute.NonProject(this) : intran.ProjectID;
                                                tranFromIN.TaskID = isNonProject ? null : intran.TaskID;
                                                tranFromIN.Released = true;

                                                je.GLTranModuleBatNbr.Insert(tranFromIN);

                                                //Debit COGS account
                                                tranFromIN = new GLTran();
                                                tranFromIN.SummPost = this.SummPost;
                                                tranFromIN.BranchID = intran.BranchID;
                                                tranFromIN.AccountID = IN.INReleaseProcess.GetAccountDefaults<IN.INPostClass.cOGSAcctID>(this, (IN.InventoryItem)itemPostClassRes, site, (IN.INPostClass)itemPostClassRes);
                                                if (((IN.INPostClass)itemPostClassRes) != null && ((IN.INPostClass)itemPostClassRes).COGSSubFromSales != true) //we cannot use intran here to retrive cogs/sales sub 
                                                    tranFromIN.SubID = IN.INReleaseProcess.GetAccountDefaults<IN.INPostClass.cOGSSubID>(this, (IN.InventoryItem)itemPostClassRes, site, (IN.INPostClass)itemPostClassRes);
                                                else
                                                    tranFromIN.SubID = n.SubID;

                                                tranFromIN.CuryDebitAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost;
                                                tranFromIN.DebitAmt = (trancost.InvtMult == (short)1) ? 0m : trancost.TranCost;
                                                tranFromIN.CuryCreditAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost : 0m;
                                                tranFromIN.CreditAmt = (trancost.InvtMult == (short)1) ? trancost.TranCost : 0m;

                                                tranFromIN.TranType = trancost.TranType;
                                                tranFromIN.TranClass = GLTran.tranClass.Normal;
                                                tranFromIN.RefNbr = trancost.RefNbr;
                                                tranFromIN.InventoryID = trancost.InventoryID;
                                                tranFromIN.Qty = (trancost.InvtMult == (short)1) ? -trancost.Qty : trancost.Qty;
                                                tranFromIN.UOM = intran.UOM;
                                                tranFromIN.TranDesc = intran.TranDesc;
                                                tranFromIN.TranDate = n.TranDate;
                                                tranFromIN.ProjectID = (trancost.InvtMult == (short)1) ? PM.ProjectDefaultAttribute.NonProject(this) : intran.ProjectID;
                                                tranFromIN.TaskID = (trancost.InvtMult == (short)1) ? null : intran.TaskID;
                                                tranFromIN.Released = true;
                                                tranFromIN.TranLineNbr = (tranFromIN.SummPost == true) ? null : intran.LineNbr;

                                                je.GLTranModuleBatNbr.Insert(tranFromIN);
                                            }
                                        }
                                    }
                                }
                                if (n.SOShipmentType == SO.SOShipmentType.DropShip)
                                {
                                    foreach (INTran intran in PXSelect<INTran,
                                        Where<INTran.pOReceiptNbr, Equal<Current<ARTran.sOShipmentNbr>>, And<INTran.pOReceiptLineNbr, Equal<Current<ARTran.sOShipmentLineNbr>>,
                                        And<INTran.docType, Equal<IN.INDocType.adjustment>, And<INTran.released, Equal<True>, And<INTran.aRRefNbr, IsNull, And<INTran.sOShipmentNbr, IsNull>>>>>>>.SelectMultiBound(this, new object[] { n }))
                                    {
                                        n.TranCost += intran.TranCost;
                                    }
                                }
                            }
                            if (n.InventoryID != null && (n.LineType == null || ((n.LineType == SO.SOLineType.Inventory || n.LineType == SO.SOLineType.NonInventory) && !tranCostSet) || n.LineType == SO.SOLineType.MiscCharge))
                            {
                                //TO DO: review this part and add more accurate cost selection conditions (INItemSite?)
                                PXResult<IN.InventoryItem, IN.INItemSite> result = (PXResult<IN.InventoryItem, IN.INItemSite>)PXSelectJoin<IN.InventoryItem,
                                    LeftJoin<IN.INItemSite, On<IN.INItemSite.inventoryID, Equal<IN.InventoryItem.inventoryID>,
                                    And<IN.INItemSite.siteID, Equal<Required<IN.INItemSite.siteID>>>>>,
                                    Where<IN.InventoryItem.inventoryID, Equal<Required<IN.InventoryItem.inventoryID>>>>.SelectSingleBound(this, null, n.SiteID, n.InventoryID);
                                if (result != null)
                                {
                                    IN.InventoryItem item = (IN.InventoryItem)result;
                                    IN.INItemSite itemSite = (IN.INItemSite)result;
                                    if (item.StkItem == true)
                                    {
                                        if (itemSite.ValMethod != null)
                                        {
                                            if (item.ValMethod == IN.INValMethod.Standard)
                                            {
                                                n.TranCostOrig = n.Qty * itemSite.StdCost;
                                            }
                                            else
                                            {
                                                n.TranCostOrig = n.Qty * itemSite.AvgCost;
                                            }
                                        }
                                    }
                                    else if (n.SOShipmentType != SOShipmentType.DropShip)
                                    {
                                        if (item.KitItem == true && PXAccess.FeatureInstalled<FeaturesSet.kitAssemblies>() && SOSetup != null && SOSetup.Current != null)
                                        {
                                            switch (SOSetup.Current.SalesProfitabilityForNSKits)
                                            {
                                                case SalesProfitabilityNSKitMethod.NSKitStandardCostOnly:
                                                    n.TranCost += n.Qty * item.StdCost;
                                                    n.TranCostOrig = n.TranCost;
                                                    n.IsTranCostFinal = true;
                                                    break;
                                                case SalesProfitabilityNSKitMethod.NSKitStandardAndStockComponentsCost:
                                                    n.TranCost += n.Qty * item.StdCost;
                                                    n.TranCostOrig = n.TranCost;
                                                    n.IsTranCostFinal = HasStockComponents(n);
                                                    break;
                                                case SalesProfitabilityNSKitMethod.StockComponentsCostOnly:
                                                    n.IsTranCostFinal = HasStockComponents(n);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            n.TranCost += n.Qty * item.StdCost;
                                            n.TranCostOrig = n.TranCost;
                                            n.IsTranCostFinal = true;
                                        }
                                    }
                                }
                            }

                            if (n.CommitmentID != null)
                            {
                                decimal sign = (n.DrCr == DrCr.Credit) ? Decimal.One : Decimal.MinusOne;

                                PMCommitment container = new PMCommitment();
                                container.CommitmentID = n.CommitmentID;
                                container.UOM = n.UOM;
                                container.InventoryID = n.InventoryID;
                                container.InvoicedAmount = sign * n.TranAmt;
                                container.InvoicedQty = sign * n.Qty;

                                PMCommitmentAttribute.AddToInvoiced(Caches[typeof(SO.SOLine)], container);
                            }
                        }

                        if (pmtran.TranID != null)
                        {
                            if (pmtran.IsNonGL != true && pmtran.Reverse == PM.PMReverse.OnInvoice && pmtran.OffsetAccountID != null)
                            {
                                if (doc.DocType == ARDocType.CreditMemo && doc.OrigDocType == ARDocType.Invoice)//credit memo reversal only for Invoice created by PM billing. CreditMemo created by PM Billing should be handled same as INV.
                                {
                                    creditMemoPMReversal.Add(new PXResult<ARTran, PMTran>(n, pmtran));
                                }
                                else if (pmtran.AccountID != pmtran.OffsetAccountID)
                                {
                                    tran.PMTranID = null;
                                    tran.OrigPMTranID = pmtran.TranID;
                                    tran.TranLineNbr = null;

                                    decimal amount = pmtran.Amount.Value;
                                    if (n.TranAmt < pmtran.Amount && n.PMDeltaOption != ARTran.pMDeltaOption.CompleteLine)
                                    {
                                        amount = n.TranAmt.Value;
                                    }

                                    decimal curyval;
                                    PXDBCurrencyAttribute.CuryConvCury(je.GLTranModuleBatNbr.Cache, new_info, amount, out curyval);

                                    tran.AccountID = pmtran.OffsetAccountID;
                                    tran.SubID = pmtran.OffsetSubID;
                                    tran.CuryDebitAmt = curyval;
                                    tran.DebitAmt = amount;
                                    tran.CuryCreditAmt = 0m;
                                    tran.CreditAmt = 0m;

                                    je.GLTranModuleBatNbr.Insert(tran);

                                    tran.AccountID = pmtran.AccountID;
                                    tran.SubID = pmtran.SubID;
                                    tran.CuryDebitAmt = 0m;
                                    tran.DebitAmt = 0m;
                                    tran.CuryCreditAmt = curyval;
                                    tran.CreditAmt = amount;

                                    je.GLTranModuleBatNbr.Insert(tran);
                                }
                            }
                            else if (pmtran.IsNonGL == true)
                            {
                                //NON-GL 

                                if (doc.DocType == ARDocType.CreditMemo && doc.OrigDocType == ARDocType.Invoice)
                                //credit memo reversal only for Invoice created by PM billing. CreditMemo created by PM Billing should be handled same as INV.
                                {
                                    creditMemoPMReversal.Add(new PXResult<ARTran, PMTran>(n, pmtran));
                                }
                                else
                                {
                                    if (n.TranAmt < pmtran.Amount && n.PMDeltaOption != ARTran.pMDeltaOption.CompleteLine)
                                    {
                                        nonglBillLater.Add(new PXResult<ARTran, PMTran>(n, pmtran));
                                    }
                                    if (pmtran.Reverse == PMReverse.OnInvoice)
                                    {
                                        nonglReverse.Add(new PXResult<ARTran, PMTran>(n, pmtran));
                                    }
                                }
                            }
                        }

                        n.Released = true;
                        ARTran_TranType_RefNbr.Cache.SmartSetStatus(n, PXEntryStatus.Updated);
                    }
                    prev_n = n;
                }

                if (_IsIntegrityCheck == false)
                {
                    if (creditMemoPMReversal.Count > 0)
                    {
                        PM.PMAllocator allocator = PXGraph.CreateInstance<PM.PMAllocator>();
                        allocator.ReverseCreditMemo(doc.RefNbr, creditMemoPMReversal);
                        allocator.Actions.PressSave();
                        pmDoc = allocator.Document.Current;
                    }
                    else if (nonglBillLater.Count > 0 || nonglReverse.Count > 0)
                    {
                        PM.PMAllocator allocator = PXGraph.CreateInstance<PM.PMAllocator>();
                        allocator.NonGlBillLaterAndReverse(doc.RefNbr, nonglBillLater, nonglReverse);
                        allocator.Actions.PressSave();
                        pmDoc = allocator.Document.Current;
                    }
                }
                foreach (PXResult<ARTaxTran, Account> rs in ARTaxTran_TranType_RefNbr.Select(ardoc.DocType, ardoc.RefNbr))
                {
                    ARTaxTran x = (ARTaxTran)rs;
                    Account taxAccount = (Account)rs;
                    GLTran tran = new GLTran();
                    tran.SummPost = this.SummPost;
                    tran.BranchID = x.BranchID;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.TranType = x.TranType;
                    tran.TranClass = GLTran.tranClass.Tax;
                    tran.RefNbr = x.RefNbr;
                    tran.TranDate = x.TranDate;
                    tran.AccountID = x.AccountID;
                    tran.SubID = x.SubID;
                    tran.TranDesc = x.TaxID;
                    tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? x.CuryTaxAmt : 0m;
                    tran.DebitAmt = (ardoc.DrCr == DrCr.Debit) ? x.TaxAmt : 0m;
                    tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : x.CuryTaxAmt;
                    tran.CreditAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : x.TaxAmt;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;

                    if (taxAccount != null && taxAccount.AccountGroupID != null && ardoc.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(this, ardoc.ProjectID))
                    {
                        PMAccountTask mapping = PXSelect<PMAccountTask,
                            Where<PMAccountTask.projectID, Equal<Required<PMAccountTask.projectID>>,
                                And<PMAccountTask.accountID, Equal<Required<PMAccountTask.accountID>>>>>.Select(this, ardoc.ProjectID, taxAccount.AccountID);

                        if (mapping == null)
                        {
                            throw new PXException(Messages.TaxAccountTaskMappingNotFound, taxAccount.AccountCD);
                        }

                        tran.ProjectID = ardoc.ProjectID;
                        tran.TaskID = mapping.TaskID;
                    }

                    je.GLTranModuleBatNbr.Insert(tran);

                    x.Released = true;
                    ARTaxTran_TranType_RefNbr.Cache.SetStatus(x, PXEntryStatus.Updated);

                    if (PXAccess.FeatureInstalled<FeaturesSet.vATReporting>() && _IsIntegrityCheck == false &&
                        (x.TaxType == TX.TaxType.PendingPurchase || x.TaxType == TX.TaxType.PendingSales))
                    {
                        AP.Vendor vendor = PXSelect<AP.Vendor, Where<AP.Vendor.bAccountID,
                            Equal<Required<AP.Vendor.bAccountID>>>>.SelectSingleBound(this, null, x.VendorID);

                        decimal mult = ReportTaxProcess.GetMultByTranType(BatchModule.AR, x.TranType);
                        string reversalMethod = x.TranType == ARDocType.CashSale || x.TranType == ARDocType.CashReturn
                            ? SVATTaxReversalMethods.OnDocuments
                            : vendor?.SVATReversalMethod;

                        SVATConversionHist histSVAT = new SVATConversionHist
                        {
                            Module = BatchModule.AR,
                            AdjdBranchID = x.BranchID,
                            AdjdDocType = x.TranType,
                            AdjdRefNbr = x.RefNbr,
                            AdjgDocType = x.TranType,
                            AdjgRefNbr = x.RefNbr,
                            AdjdDocDate = doc.DocDate,
                            AdjdFinPeriodID = doc.FinPeriodID,

                            TaxID = x.TaxID,
                            TaxType = x.TaxType,
                            TaxRate = x.TaxRate,
                            VendorID = x.VendorID,
                            ReversalMethod = reversalMethod,

                            CuryInfoID = x.CuryInfoID,
                            CuryTaxableAmt = x.CuryTaxableAmt * mult,
                            CuryTaxAmt = x.CuryTaxAmt * mult,
                            CuryUnrecognizedTaxAmt = x.CuryTaxAmt * mult
                        };

                        histSVAT.FillBaseAmounts(SVATConversionHistory.Cache);
                        SVATConversionHistory.Cache.Insert(histSVAT);
                    }
                }

                foreach (ARSalesPerTran n in ARDoc_SalesPerTrans.Select(doc.DocType, doc.RefNbr))
                {
                    //multiply installments master and deferred revenue should not have commission
                    n.Released = doc.OpenDoc;
                    ARDoc_SalesPerTrans.Cache.Update(n);

                    PXTimeStampScope.DuplicatePersisted(ARDoc_SalesPerTrans.Cache, n, typeof(ARSalesPerTran));
                }

                if (_IsIntegrityCheck == false)
                {
                    foreach (PXResult<ARAdjust, ARPayment> appres in PXSelectJoin<ARAdjust, InnerJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>, Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>, And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>, And<ARAdjust.released, Equal<False>, And<ARAdjust.voided, Equal<False>>>>>>.Select(this, doc.DocType, doc.RefNbr))
                    {
                        ARAdjust adj = (ARAdjust)appres;
                        ARPayment payment = (ARPayment)appres;

                        if (((ARAdjust)appres).CuryAdjdAmt > 0m)
                        {
                            if (_InstallmentType != TermsInstallmentType.Single)
                            {
                                throw new PXException(Messages.PrepaymentAppliedToMultiplyInstallments);
                            }

                            if (ret == null)
                            {
                                ret = new List<ARRegister>();
                            }

                            //sync fields with the max value:
                            if (DateTime.Compare((DateTime)payment.AdjDate, (DateTime)adj.AdjdDocDate) < 0)
                            {
                                payment.AdjDate = adj.AdjdDocDate;
                                payment.AdjFinPeriodID = adj.AdjdFinPeriodID;
                                payment.AdjTranPeriodID = adj.AdjdTranPeriodID;
                            }
                            else if (string.Compare(payment.AdjFinPeriodID, adj.AdjgFinPeriodID) > 0)
                            {
                                adj.AdjgDocDate = payment.AdjDate;
                                adj.AdjgFinPeriodID = payment.AdjFinPeriodID;
                                adj.AdjgTranPeriodID = payment.AdjTranPeriodID;
                            }

                            if (payment.Released == true)
                            {
                                ret.Add(payment);
                            }

                            ARPayment_DocType_RefNbr.Cache.Update(payment);

                            adj.AdjAmt += adj.RGOLAmt;
                            adj.RGOLAmt = -adj.RGOLAmt;
                            adj.Hold = false;

                            ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.SetStatus(adj, PXEntryStatus.Updated);
                        }
                    }

                    if (doc.DocType == ARDocType.CreditMemo)
                    {
                        if (ret == null)
                        {
                            ret = new List<ARRegister>();
                        }

                        doc.AdjCntr = 0;

                        ret.Add(doc);
                    }

                    CreatePayment(res, ref ret);
                }

                Batch arbatch = je.BatchModule.Current;

                if (Math.Abs(Math.Round((decimal)(arbatch.CuryDebitTotal - arbatch.CuryCreditTotal), 4)) >= 0.00005m)
                {
                    VerifyRoundingAllowed(ardoc, arbatch, je.currencyinfo.Current.BaseCuryID);

                    GLTran tran = new GLTran();
                    tran.SummPost = true;
                    tran.BranchID = ardoc.BranchID;
                    Currency c = PXSelect<Currency, Where<Currency.curyID, Equal<Required<CurrencyInfo.curyID>>>>.Select(this, doc.CuryID);

                    if (Math.Sign((decimal)(arbatch.CuryDebitTotal - arbatch.CuryCreditTotal)) == 1)
                    {
                        tran.AccountID = c.RoundingGainAcctID;
                        tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, c);
                        tran.CuryCreditAmt = Math.Round((decimal)(arbatch.CuryDebitTotal - arbatch.CuryCreditTotal), 4);
                        tran.CuryDebitAmt = 0m;
                    }
                    else
                    {
                        tran.AccountID = c.RoundingLossAcctID;
                        tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, c);
                        tran.CuryCreditAmt = 0m;
                        tran.CuryDebitAmt = Math.Round((decimal)(arbatch.CuryCreditTotal - arbatch.CuryDebitTotal), 4);
                    }
                    tran.CreditAmt = 0m;
                    tran.DebitAmt = 0m;
                    tran.TranType = doc.DocType;
                    tran.RefNbr = doc.RefNbr;
                    tran.TranClass = GLTran.tranClass.Normal;
                    tran.TranDesc = GL.Messages.RoundingDiff;
                    tran.LedgerID = arbatch.LedgerID;
                    tran.FinPeriodID = arbatch.FinPeriodID;
                    tran.TranDate = arbatch.DateEntered;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;

                    CurrencyInfo infocopy = new CurrencyInfo();
                    infocopy = je.currencyinfo.Insert(infocopy) ?? infocopy;

                    tran.CuryInfoID = infocopy.CuryInfoID;
                    je.GLTranModuleBatNbr.Insert(tran);
                }

                if (Math.Abs(Math.Round((decimal)(arbatch.DebitTotal - arbatch.CreditTotal), 4)) >= 0.00005m)
                {
                    GLTran tran = new GLTran();
                    tran.SummPost = true;
                    tran.BranchID = ardoc.BranchID;
                    Currency c = PXSelect<Currency, Where<Currency.curyID, Equal<Required<CurrencyInfo.curyID>>>>.Select(this, doc.CuryID);

                    if (Math.Sign((decimal)(arbatch.DebitTotal - arbatch.CreditTotal)) == 1)
                    {
                        tran.AccountID = c.RoundingGainAcctID;
                        tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, c);
                        tran.CreditAmt = Math.Round((decimal)(arbatch.DebitTotal - arbatch.CreditTotal), 4);
                        tran.DebitAmt = 0m;
                    }
                    else
                    {
                        tran.AccountID = c.RoundingLossAcctID;
                        tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, c);
                        tran.CreditAmt = 0m;
                        tran.DebitAmt = Math.Round((decimal)(arbatch.CreditTotal - arbatch.DebitTotal), 4);
                    }
                    tran.CuryCreditAmt = 0m;
                    tran.CuryDebitAmt = 0m;
                    tran.TranType = doc.DocType;
                    tran.RefNbr = doc.RefNbr;
                    tran.TranClass = GLTran.tranClass.Normal;
                    tran.TranDesc = GL.Messages.RoundingDiff;
                    tran.LedgerID = arbatch.LedgerID;
                    tran.FinPeriodID = arbatch.FinPeriodID;
                    tran.TranDate = arbatch.DateEntered;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;

                    CurrencyInfo infocopy = new CurrencyInfo();
                    infocopy = je.currencyinfo.Insert(infocopy) ?? infocopy;

                    tran.CuryInfoID = infocopy.CuryInfoID;
                    je.GLTranModuleBatNbr.Insert(tran);
                }

                if (doc.CuryDocBal == 0m)
                {
                    doc.DocBal = 0m;
                    doc.CuryDiscBal = 0m;
                    doc.DiscBal = 0m;

                    doc.OpenDoc = false;
                    doc.ClosedFinPeriodID = doc.FinPeriodID;
                    doc.ClosedTranPeriodID = doc.TranPeriodID;
                }
            }

            return ret;
        }

        public virtual bool HasStockComponents(ARTran n)
        {
            return (n.IsTranCostFinal == true
                    || (PXSelect<SOShipLineSplit, Where<SOShipLineSplit.shipmentNbr, Equal<Current<ARTran.sOShipmentNbr>>,
                    And<SOShipLineSplit.lineNbr, Equal<Current<ARTran.sOShipmentLineNbr>>,
                    And<SOShipLineSplit.isStockItem, Equal<True>>>>>.SelectSingleBound(this, new object[] { n }).Count() == 0
                    && PXSelect<SOLineSplit, Where<SOLineSplit.orderType, Equal<Current<ARTran.sOOrderType>>,
                    And<SOLineSplit.orderNbr, Equal<Current<ARTran.sOOrderNbr>>,
                    And<SOLineSplit.lineNbr, Equal<Current<ARTran.sOOrderLineNbr>>,
                    And<SOLineSplit.isStockItem, Equal<True>>>>>>.SelectSingleBound(this, new object[] { n }).Count() == 0));

        }

        public class Amount : Tuple<decimal?, decimal?>
        {
            public decimal? Cury { get { return Item1; } }
            public decimal? Base { get { return Item2; } }

            public Amount(decimal? cury, decimal? baaase)
                : base(cury, baaase)
            {
            }
        }

        public static Amount GetSalesPostingAmount(PXGraph graph, ARTran documentLine)
        {
            var documentLineWithTaxes = new PXSelectJoin<
                ARTran,
                    LeftJoin<ARTax,
                        On<ARTax.tranType, Equal<ARTran.tranType>,
                        And<ARTax.refNbr, Equal<ARTran.refNbr>,
                        And<ARTax.lineNbr, Equal<ARTran.lineNbr>>>>,
                    LeftJoin<Tax,
                        On<Tax.taxID, Equal<ARTax.taxID>>,
                    LeftJoin<ARRegister,
                        On<ARRegister.docType, Equal<ARTran.tranType>,
                        And<ARRegister.refNbr, Equal<ARTran.refNbr>>>>>>,
                Where<
                    ARTran.tranType, Equal<Required<ARTran.tranType>>,
                    And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>,
                    And<ARTran.lineNbr, Equal<Required<ARTran.lineNbr>>>>>>
                (graph);

            PXResult<ARTran, ARTax, Tax, ARRegister> queryResult =
                documentLineWithTaxes
                    .Select(documentLine.TranType, documentLine.RefNbr, documentLine.LineNbr)
                    .Cast<PXResult<ARTran, ARTax, Tax, ARRegister>>()
                    .First();

            Func<decimal, decimal> roundingFunction = amount =>
                PXDBCurrencyAttribute.Round(
                    graph.Caches[typeof(ARTran)],
                    documentLine,
                    amount,
                    CMPrecision.TRANCURY);

            return GetSalesPostingAmount(graph, queryResult, documentLine, queryResult, queryResult, roundingFunction);
        }

        [Obsolete("The method has been renamed to " + nameof(GetSalesPostingAmount) + ".", false)]
        public static Amount GetPostedTranAmount(PXGraph graph, ARRegister doc, ARTran tran, ARTax tranTax, Tax salestax, Func<decimal, decimal> round)
            => GetSalesPostingAmount(graph, doc, tran, tranTax, salestax, round);

        /// <summary>
        /// Gets the amount to be posted to the sales acount 
        /// for the given document line.
        /// </summary>
        public static Amount GetSalesPostingAmount(
            PXGraph graph,
            ARRegister document,
            ARTran documentLine,
            ARTax lineTax,
            Tax salesTax,
            Func<decimal, decimal> round)
        {
            if (lineTax?.TaxID != null &&
                salesTax != null &&
                salesTax.TaxCalcLevel == "0")
            {
                decimal? curyAddUp = documentLine.CuryTranAmt
                    - round((decimal)(documentLine.CuryTranAmt * documentLine.GroupDiscountRate * documentLine.DocumentDiscountRate));

                decimal? addUp = documentLine.TranAmt
                    - round((decimal)(documentLine.TranAmt * documentLine.GroupDiscountRate * documentLine.DocumentDiscountRate));

                return new Amount(
                    lineTax.CuryTaxableAmt + curyAddUp,
                    lineTax.TaxableAmt + addUp);
            }

            bool postedPPD = document.PendingPPD == true && document.DocType == ARDocType.CreditMemo;

            if (!postedPPD &&
                lineTax != null &&
                lineTax.TaxID == null &&
                document.DocType == ARDocType.DebitMemo &&
                document.OrigDocType == ARDocType.CreditMemo &&
                document.OrigRefNbr != null)
            {
                // Same logic for the DebitMemo, reversed from the PPD CreditMemo
                // -
                postedPPD = PXSelect<
                    ARRegister,
                    Where<
                        ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>,
                        And<ARRegister.docType, Equal<Required<ARRegister.docType>>,
                        And<ARRegister.pendingPPD, Equal<True>>>>>
                    .SelectSingleBound(graph, null, document.OrigRefNbr, document.OrigDocType).Count > 0;
            }

            return postedPPD ?
                new Amount(documentLine.CuryTaxableAmt, documentLine.TaxableAmt) :
                new Amount(documentLine.CuryTranAmt, documentLine.TranAmt);
        }

        protected virtual void VerifyRoundingAllowed(ARInvoice document, Batch batch, string baseCuryID)
        {
            //p. "document" may be used in customization (see AC-59109)

            if (this.InvoiceRounding == RoundingType.Currency)
            {
                throw new PXException(Messages.DocumentOutOfBalance);
            }

            decimal roundDiff = Math.Abs(Math.Round((decimal)(batch.DebitTotal - batch.CreditTotal), 4));
            if (roundDiff > this.RoundingLimit)
            {
                throw new PXException(AP.Messages.RoundingAmountTooBig, baseCuryID, roundDiff,
                    IN.PXDBQuantityAttribute.Round(this.RoundingLimit));
            }
        }

        protected object GetValue<Field>(object data)
            where Field : IBqlField
        {
            return this.Caches[typeof(Field).DeclaringType].GetValue(data, typeof(Field).Name);
        }

        public int? GetValueInt<SourceField>(PXGraph target, object item)
            where SourceField : IBqlField
        {
            PXCache source = this.Caches[typeof(SourceField).DeclaringType];
            PXCache dest = target.Caches[typeof(SourceField).DeclaringType];

            object value = source.GetValueExt<SourceField>(item);
            if (value is PXFieldState)
            {
                value = ((PXFieldState)value).Value;
            }

            if (value != null)
            {
                dest.RaiseFieldUpdating<SourceField>(item, ref value);
            }

            return (int?)value;
        }

        public static void UpdateARBalances(PXGraph graph, ARRegister ardoc, decimal? BalanceAmt)
        {
            //voided payment is both released and voided
            //voided invoice(previously scheduled) is not released and voided
            if ((bool)ardoc.Released && ardoc.Voided == false && ardoc.SignBalance != 0m)
            {
                UpdateARBalances(graph, ardoc, ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt, 0m);
            }
            else if (ardoc.Hold == false && ardoc.Scheduled == false && ardoc.Voided == false && ardoc.SignBalance != 0m)
            {
                UpdateARBalances(graph, ardoc, 0m, ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt);
            }
        }

        public static void UpdateARBalances(PXGraph graph, ARRegister ardoc, decimal? CurrentBal, decimal? UnreleasedBal)
        {
            if (ardoc.CustomerID != null && ardoc.CustomerLocationID != null)
            {
                ARBalances arbal = new ARBalances();
                arbal.BranchID = ardoc.BranchID;
                arbal.CustomerID = ardoc.CustomerID;
                arbal.CustomerLocationID = ardoc.CustomerLocationID;
                arbal = (ARBalances)graph.Caches[typeof(ARBalances)].Insert(arbal);

                arbal.CurrentBal += CurrentBal;
                arbal.UnreleasedBal += UnreleasedBal;
            }
        }

        public static void UpdateARBalances(PXGraph graph, ARInvoice ardoc, decimal? BalanceAmt)
        {
            //voided payment is both released and voided
            //voided invoice(previously scheduled) is not released and voided
            if ((bool)ardoc.Released && ardoc.Voided == false && ardoc.SignBalance != 0m)
            {
                UpdateARBalances(graph, ardoc, ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt, 0m);
            }
            else if (ardoc.Hold == false && ardoc.CreditHold == false && ardoc.Scheduled == false && ardoc.Voided == false && ardoc.SignBalance != 0m)
            {
                UpdateARBalances(graph, ardoc, 0m, ardoc.SignBalance == 1m ? BalanceAmt : -BalanceAmt);
            }
        }

        public static void UpdateARBalances(PXGraph graph, SOOrder order, decimal? UnbilledAmount, decimal? UnshippedAmount)
        {
            if (order.CustomerID != null && order.CustomerLocationID != null)
            {
                ARBalances arbal = new ARBalances();
                arbal.BranchID = order.BranchID;
                arbal.CustomerID = order.CustomerID;
                arbal.CustomerLocationID = order.CustomerLocationID;
                arbal = (ARBalances)graph.Caches[typeof(ARBalances)].Insert(arbal);

                if (ARDocType.SignBalance(order.ARDocType) != 0m && ARDocType.SignBalance(order.ARDocType) != null)
                {
                    decimal? BalanceAmt;
                    if (order.ShipmentCntr == 0)
                    {
                        BalanceAmt = UnbilledAmount;
                    }
                    else
                    {
                        BalanceAmt = UnshippedAmount;
                        arbal.TotalOpenOrders += ARDocType.SignBalance(order.ARDocType) == 1m ? (UnbilledAmount - UnshippedAmount) : -(UnbilledAmount - UnshippedAmount);
                    }

                    //don't check field 'Completed' here it will cause incorrect calculation of 'Open Order Balance'
                    if (order.Cancelled == false && order.Hold == false && order.CreditHold == false && order.InclCustOpenOrders == true)
                    {
                        arbal.TotalOpenOrders += ARDocType.SignBalance(order.ARDocType) == 1m ? BalanceAmt : -BalanceAmt;
                    }
                }
            }
        }

        private void UpdateARBalances(ARAdjust adj, ARRegister ardoc)
        {
            if (object.Equals(ardoc.DocType, adj.AdjdDocType) && string.Equals(ardoc.RefNbr, adj.AdjdRefNbr, StringComparison.OrdinalIgnoreCase))
            {
                if (ardoc.CustomerID != null && ardoc.CustomerLocationID != null)
                {
                    ARBalances arbal = new ARBalances();
                    arbal.BranchID = ardoc.BranchID;
                    arbal.CustomerID = ardoc.CustomerID;
                    arbal.CustomerLocationID = ardoc.CustomerLocationID;
                    arbal = (ARBalances)this.Caches[typeof(ARBalances)].Insert(arbal);

                    arbal.CurrentBal += adj.AdjdTBSign * (adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt) - adj.RGOLAmt;
                }
            }
            else if (object.Equals(ardoc.DocType, adj.AdjgDocType) && string.Equals(ardoc.RefNbr, adj.AdjgRefNbr, StringComparison.OrdinalIgnoreCase))
            {
                if (ardoc.CustomerID != null && ardoc.CustomerLocationID != null)
                {
                    ARBalances arbal = new ARBalances();
                    arbal.BranchID = ardoc.BranchID;
                    arbal.CustomerID = ardoc.CustomerID;
                    arbal.CustomerLocationID = ardoc.CustomerLocationID;
                    arbal = (ARBalances)this.Caches[typeof(ARBalances)].Insert(arbal);

                    arbal.CurrentBal += adj.AdjgTBSign * (adj.AdjAmt);
                }
            }
            else
            {
                throw new PXException(Messages.AdjustRefersNonExistentDocument, adj.AdjgDocType, adj.AdjgRefNbr, adj.AdjdDocType, adj.AdjdRefNbr);
            }
        }

        private void UpdateARBalances(ARRegister ardoc)
        {
            UpdateARBalances(this, ardoc, ardoc.OrigDocAmt);
        }

        private void UpdateARBalances(ARRegister ardoc, decimal? BalanceAmt)
        {
            UpdateARBalances(this, ardoc, BalanceAmt);
        }

        public virtual void VoidOrigAdjustment(ARAdjust adj)
        {
            bool isVoidPayment = (adj.AdjgDocType == ARDocType.VoidPayment);
            ARAdjust voidadj = PXSelect<ARAdjust, Where2<Where<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
                Or<ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>>>,
                And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
                And<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
                And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
                And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>>>>>>>.Select
            (
                this,
                isVoidPayment ? ARDocType.Prepayment : adj.AdjgDocType,
                isVoidPayment ? ARDocType.Payment : adj.AdjgDocType,
                adj.AdjgRefNbr,
                adj.AdjdDocType,
                adj.AdjdRefNbr,
                adj.VoidAdjNbr
            );

            if (voidadj != null)
            {
                if ((bool)voidadj.Voided)
                {
                    throw new PXException(Messages.DocumentApplicationAlreadyVoided);
                }

                voidadj.Voided = true;
                Caches[typeof(ARAdjust)].Update(voidadj);

                adj.AdjAmt = -voidadj.AdjAmt;
                adj.AdjDiscAmt = -voidadj.AdjDiscAmt;
                adj.AdjWOAmt = -voidadj.AdjWOAmt;
                adj.RGOLAmt = -voidadj.RGOLAmt;
                adj.CuryAdjdAmt = -voidadj.CuryAdjdAmt;
                adj.CuryAdjdDiscAmt = -voidadj.CuryAdjdDiscAmt;
                adj.CuryAdjdWOAmt = -voidadj.CuryAdjdWOAmt;
                adj.CuryAdjgAmt = -voidadj.CuryAdjgAmt;
                adj.CuryAdjgDiscAmt = -voidadj.CuryAdjgDiscAmt;
                adj.CuryAdjgWOAmt = -voidadj.CuryAdjgWOAmt;

                Caches[typeof(ARAdjust)].Update(adj);

                if (voidadj.AdjgDocType == ARDocType.CreditMemo && voidadj.AdjdHasPPDTaxes == true)
                {
                    ARRegister crmemo = PXSelect<ARRegister, Where<ARRegister.docType, Equal<ARDocType.creditMemo>,
                        And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>>>>.SelectSingleBound(this, null, voidadj.AdjgRefNbr);
                    if (crmemo != null && crmemo.PendingPPD == true)
                    {
                        PXUpdate<Set<ARAdjust.pPDCrMemoRefNbr, Null>, ARAdjust,
                        Where<ARAdjust.pendingPPD, Equal<True>,
                            And<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
                             And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
                            And<ARAdjust.pPDCrMemoRefNbr, Equal<Required<ARAdjust.pPDCrMemoRefNbr>>>>>>>
                        .Update(this, voidadj.AdjdDocType, voidadj.AdjdRefNbr, voidadj.AdjgRefNbr);
                    }
                }
            }
        }

        public virtual void UpdateBalances(ARAdjust adj, ARRegister adjddoc)
        {
            ARRegister ardoc = (ARRegister)adjddoc;
            ARRegister cached = (ARRegister)ARDocument.Cache.Locate(ardoc);
            if (cached != null)
            {
                ardoc = cached;
            }
            else if (_IsIntegrityCheck == true)
            {
                return;
            }

            if (_IsIntegrityCheck == false && adj.VoidAdjNbr != null)
            {
                VoidOrigAdjustment(adj);
            }

            if (string.Equals(adj.AdjdDocType, ardoc.DocType) && string.Equals(adj.AdjdRefNbr, ardoc.RefNbr, StringComparison.OrdinalIgnoreCase))
            {
                ardoc.CuryDocBal -= (adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt + adj.CuryAdjdWOAmt);
                ardoc.DocBal -= (adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt + (adj.ReverseGainLoss == false ? adj.RGOLAmt : -adj.RGOLAmt));
                ardoc.CuryDiscBal -= adj.CuryAdjdDiscAmt;
                ardoc.DiscBal -= adj.AdjDiscAmt;
                ardoc.CuryDiscTaken += adj.CuryAdjdDiscAmt;
                ardoc.DiscTaken += adj.AdjDiscAmt;
                ardoc.RGOLAmt += adj.RGOLAmt;
            }
            else if (string.Equals(adj.AdjgDocType, ardoc.DocType) && string.Equals(adj.AdjgRefNbr, ardoc.RefNbr, StringComparison.OrdinalIgnoreCase))
            {
                ardoc.CuryDocBal -= adj.CuryAdjgAmt;
                ardoc.DocBal -= adj.AdjAmt;
            }

            if (ardoc.CuryDiscBal == 0m)
            {
                ardoc.DiscBal = 0m;
            }

            if (_IsIntegrityCheck == false && ardoc.CuryDocBal < 0m)
            {
                throw new PXException(Messages.DocumentBalanceNegative);
            }

            if (_IsIntegrityCheck == false && adj.AdjgDocDate < adjddoc.DocDate)
            {
                throw new PXException(Messages.ApplDate_Less_DocDate, PXUIFieldAttribute.GetDisplayName<ARPayment.adjDate>(ARPayment_DocType_RefNbr.Cache));
            }

            if (_IsIntegrityCheck == false && string.Compare(adj.AdjgFinPeriodID, adjddoc.FinPeriodID) < 0)
            {
                throw new PXException(Messages.ApplPeriod_Less_DocPeriod, PXUIFieldAttribute.GetDisplayName<ARPayment.adjFinPeriodID>(ARPayment_DocType_RefNbr.Cache));
            }

            if (ardoc.CuryDocBal == 0m)
            {
                ardoc.CuryDiscBal = 0m;
                ardoc.DiscBal = 0m;
                ardoc.DocBal = 0m;
                ardoc.OpenDoc = false;

                SetClosedPeriodsFromLatestApplication(ardoc, adj.AdjNbr);
            }
            else
            {
                if (ardoc.CuryDocBal == ardoc.CuryOrigDocAmt)
                {
                    ardoc.CuryDiscBal = ardoc.CuryOrigDiscAmt;
                    ardoc.DiscBal = ardoc.OrigDiscAmt;
                    ardoc.CuryDiscTaken = 0m;
                    ardoc.DiscTaken = 0m;
                }

                ardoc.OpenDoc = true;
                ardoc.ClosedTranPeriodID = null;
                ardoc.ClosedFinPeriodID = null;
            }

            ARDocument.Cache.Update(ardoc);
        }

        private void UpdateVoidedCheck(ARRegister voidcheck)
        {
            foreach (string origDocType in voidcheck.PossibleOriginalDocumentTypes())
            {
                foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer> res in ARPayment_DocType_RefNbr
                    .Select(origDocType, voidcheck.RefNbr, voidcheck.CustomerID))
                {
                    ARRegister ardoc = res;
                    ARRegister cached = (ARRegister)ARDocument.Cache.Locate(ardoc);

                    if (cached != null)
                    {
                        ardoc = cached;
                    }

                    ardoc.Voided = true;
                    ardoc.OpenDoc = false;
                    ardoc.Hold = false;
                    ardoc.CuryDocBal = 0m;
                    ardoc.DocBal = 0m;
                    ardoc.ClosedFinPeriodID = voidcheck.ClosedFinPeriodID;
                    ardoc.ClosedTranPeriodID = voidcheck.ClosedTranPeriodID;

                    ARDocument.Cache.Update(ardoc);

                    if (!_IsIntegrityCheck)
                    {
                        // For the voided document, we must remove all unreleased applications.
                        // -
                        foreach (ARAdjust application in PXSelect<ARAdjust,
                            Where<
                                ARAdjust.adjgDocType, Equal<Required<ARAdjust.adjgDocType>>,
                                And<ARAdjust.adjgRefNbr, Equal<Required<ARAdjust.adjgRefNbr>>,
                                And<ARAdjust.adjNbr, Equal<Required<ARAdjust.adjNbr>>,
                                And<ARAdjust.released, NotEqual<True>>>>>>
                            .Select(this, ardoc.DocType, ardoc.RefNbr, ardoc.AdjCntr))
                        {
                            Caches[typeof(ARAdjust)].Delete(application);
                        }
                    }

                    PXDatabase.Update<SO.SOAdjust>(
                        new PXDataFieldRestrict(typeof(SO.SOAdjust.adjgDocType).Name, PXDbType.VarChar, SO.SOAdjust.AdjgDocTypeLength, ardoc.DocType, PXComp.EQ),
                        new PXDataFieldRestrict(typeof(SO.SOAdjust.adjgRefNbr).Name, PXDbType.VarChar, SO.SOAdjust.AdjgRefNbrLength, ardoc.RefNbr, PXComp.EQ),
                        new PXDataFieldAssign(typeof(SO.SOAdjust.voided).Name, PXDbType.Bit, true));
                }
            }
        }

        private void VerifyVoidCheckNumberMatchesOriginalPayment(ARPayment voidcheck)
        {
            foreach (string origDocType in voidcheck.PossibleOriginalDocumentTypes())
            {
                foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer> res in ARPayment_DocType_RefNbr
                    .Select(origDocType, voidcheck.RefNbr, voidcheck.CustomerID))
                {
                    ARPayment payment = res;
                    if (_IsIntegrityCheck == false &&
                        !string.Equals(voidcheck.ExtRefNbr, payment.ExtRefNbr, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new PXException(Messages.VoidAppl_CheckNbr_NotMatchOrigPayment);
                    }
                }
            }
        }

        public virtual void ReleaseDocProc(JournalEntry je, ref ARRegister doc, PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res)
        {
            ReleaseDocProc(je, ref doc, res, doc.AdjCntr);

            //increment default for AdjNbr
            doc.AdjCntr++;
        }

        /// <summary>
        /// Ensures that no unreleased voiding document exists for the specified payment.
        /// (If the applications of the voided and the voiding document are not
        /// synchronized, it can lead to a balance discrepancy, see AC-78131).
        /// </summary>
        public static void EnsureNoUnreleasedVoidPaymentExists(PXGraph selectGraph, ARRegister payment, string actionDescription)
        {
            ARRegister unreleasedVoidPayment =
                HasUnreleasedVoidPayment<ARRegister.docType, ARRegister.refNbr>.Select(selectGraph, payment);

            if (unreleasedVoidPayment != null)
            {
                throw new PXException(
                    Common.Messages.CannotPerformActionOnDocumentUnreleasedVoidPaymentExists,
                    PXLocalizer.Localize(GetLabel.For<ARDocType>(payment.DocType)),
                    payment.RefNbr,
                    PXLocalizer.Localize(actionDescription),
                    PXLocalizer.Localize(GetLabel.For<ARDocType>(unreleasedVoidPayment.DocType)),
                    PXLocalizer.Localize(GetLabel.For<ARDocType>(payment.DocType)),
                    PXLocalizer.Localize(GetLabel.For<ARDocType>(unreleasedVoidPayment.DocType)),
                    unreleasedVoidPayment.RefNbr);
            }
        }

        public virtual void ReleaseDocProc(JournalEntry je, ref ARRegister doc, PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res, int? AdjNbr)
        {
            ARPayment ardoc = res;
            CurrencyInfo info = res;
            Customer vend = res;
            Currency paycury = res;
            CashAccount cashacct = res;

            EnsureNoUnreleasedVoidPaymentExists(this, ardoc, Common.Messages.ActionReleased);

            CustomerClass custclass = PXSelectJoin<CustomerClass, InnerJoin<ARSetup, On<ARSetup.dfltCustomerClassID, Equal<CustomerClass.customerClassID>>>>.Select(this, null);

            CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
            new_info.CuryInfoID = null;
            new_info.ModuleCode = "GL";
            new_info = je.currencyinfo.Insert(new_info) ?? new_info;

            if (doc.Released == false)
            {
                //should always restore ARRegister to ARPayment after invoice part release of cash sale
                PXCache<ARRegister>.RestoreCopy(ardoc, doc);

                if (doc.DocType != ARDocType.SmallBalanceWO)
                {
                    GLTran tran = new GLTran();
                    tran.SummPost = true;
                    tran.BranchID = cashacct.BranchID;
                    tran.AccountID = cashacct.AccountID;
                    tran.SubID = cashacct.SubID;
                    tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.CuryOrigDocAmt : 0m;
                    tran.DebitAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.OrigDocAmt : 0m;
                    tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.CuryOrigDocAmt;
                    tran.CreditAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.OrigDocAmt;
                    tran.TranType = ardoc.DocType;
                    tran.TranClass = ardoc.DocClass;
                    tran.RefNbr = ardoc.RefNbr;
                    tran.TranDesc = ardoc.DocDesc;
                    tran.TranDate = ardoc.DocDate;
                    tran.TranPeriodID = ardoc.TranPeriodID;
                    tran.FinPeriodID = ardoc.FinPeriodID;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.Released = true;
                    tran.CATranID = ardoc.CATranID;
                    tran.ReferenceID = ardoc.CustomerID;
                    tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(this);

                    je.GLTranModuleBatNbr.Insert(tran);

                    //Debit Payment AR Account
                    tran = new GLTran();
                    tran.SummPost = true;
                    //if (ardoc.DocType == ARDocType.CashSale || ardoc.DocType == ARDocType.CashReturn)
                    if (!ARPaymentType.CanHaveBalance(ardoc.DocType))
                    {
                        tran.ZeroPost = false;
                    }
                    tran.BranchID = ardoc.BranchID;
                    tran.AccountID = ardoc.ARAccountID;
                    tran.ReclassificationProhibited = true;
                    tran.SubID = ardoc.ARSubID;
                    tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.CuryOrigDocAmt;
                    tran.DebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : ardoc.OrigDocAmt;
                    tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.CuryOrigDocAmt : 0m;
                    tran.CreditAmt = (ardoc.DrCr == DrCr.Debit) ? ardoc.OrigDocAmt : 0m;
                    tran.TranType = ardoc.DocType;
                    tran.TranClass = GLTran.tranClass.Payment;
                    tran.RefNbr = ardoc.RefNbr;
                    tran.TranDesc = ardoc.DocDesc;
                    tran.TranDate = ardoc.DocDate;
                    tran.TranPeriodID = ardoc.TranPeriodID;
                    tran.FinPeriodID = ardoc.FinPeriodID;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;
                    tran.ProjectID = ardoc.ProjectID;
                    tran.TaskID = ardoc.TaskID;

                    UpdateHistory(tran, vend);
                    UpdateHistory(tran, vend, new_info);

                    je.GLTranModuleBatNbr.Insert(tran);
                }

                foreach (ARPaymentChargeTran charge in ARPaymentChargeTran_DocType_RefNbr.Select(doc.DocType, doc.RefNbr))
                {
                    bool isCADebit = charge.GetCASign() == 1;

                    GLTran tran = new GLTran();
                    tran.SummPost = true;
                    tran.BranchID = cashacct.BranchID;
                    tran.AccountID = cashacct.AccountID;
                    tran.SubID = cashacct.SubID;
                    tran.CuryDebitAmt = isCADebit ? charge.CuryTranAmt : 0m;
                    tran.DebitAmt = isCADebit ? charge.TranAmt : 0m;
                    tran.CuryCreditAmt = isCADebit ? 0m : charge.CuryTranAmt;
                    tran.CreditAmt = isCADebit ? 0m : charge.TranAmt;
                    tran.TranType = charge.DocType;
                    tran.TranClass = ardoc.DocClass;
                    tran.RefNbr = charge.RefNbr;
                    tran.TranDesc = charge.TranDesc;
                    tran.TranDate = charge.TranDate;
                    tran.TranPeriodID = charge.TranPeriodID;
                    tran.FinPeriodID = charge.FinPeriodID;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.Released = true;
                    tran.CATranID = charge.CashTranID ?? ardoc.CATranID;
                    tran.ReferenceID = ardoc.CustomerID;

                    je.GLTranModuleBatNbr.Insert(tran);

                    tran = new GLTran();
                    tran.SummPost = true;
                    tran.ZeroPost = false;
                    tran.BranchID = ardoc.BranchID;
                    tran.AccountID = charge.AccountID;
                    tran.SubID = charge.SubID;
                    tran.CuryDebitAmt = isCADebit ? 0m : charge.CuryTranAmt;
                    tran.DebitAmt = isCADebit ? 0m : charge.TranAmt;
                    tran.CuryCreditAmt = isCADebit ? charge.CuryTranAmt : 0m;
                    tran.CreditAmt = isCADebit ? charge.TranAmt : 0m;
                    tran.TranType = charge.DocType;
                    tran.TranClass = GLTran.tranClass.Charge;
                    tran.RefNbr = charge.RefNbr;
                    tran.TranDesc = charge.TranDesc;
                    tran.TranDate = charge.TranDate;
                    tran.TranPeriodID = charge.TranPeriodID;
                    tran.FinPeriodID = charge.FinPeriodID;
                    tran.CuryInfoID = new_info.CuryInfoID;
                    tran.Released = true;
                    tran.ReferenceID = ardoc.CustomerID;

                    je.GLTranModuleBatNbr.Insert(tran);

                    charge.Released = true;
                    ARPaymentChargeTran_DocType_RefNbr.Update(charge);
                }

                doc.CuryDocBal = doc.CuryOrigDocAmt;
                doc.DocBal = doc.OrigDocAmt;

                doc.Voided = false;
                doc.OpenDoc = true;
                doc.ClosedFinPeriodID = null;
                doc.ClosedTranPeriodID = null;

                if (ardoc.VoidAppl == true)
                {
                    doc.OpenDoc = false;
                    doc.ClosedFinPeriodID = doc.FinPeriodID;
                    doc.ClosedTranPeriodID = doc.TranPeriodID;

                    VerifyVoidCheckNumberMatchesOriginalPayment(ardoc);
                }
            }

            if (doc.DocType == ARDocType.CashSale || doc.DocType == ARDocType.CashReturn)
            {
                if (_IsIntegrityCheck == false)
                {
                    ARAdjust adj = new ARAdjust();
                    adj.AdjdDocType = doc.DocType;
                    adj.AdjdRefNbr = doc.RefNbr;
                    adj.AdjdBranchID = doc.BranchID;
                    adj.AdjgDocType = doc.DocType;
                    adj.AdjgRefNbr = doc.RefNbr;
                    adj.AdjgBranchID = doc.BranchID;
                    adj.AdjdCustomerID = doc.CustomerID;
                    adj.AdjdARAcct = doc.ARAccountID;
                    adj.AdjdARSub = doc.ARSubID;
                    adj.AdjdCuryInfoID = doc.CuryInfoID;
                    adj.AdjdDocDate = doc.DocDate;
                    adj.AdjdFinPeriodID = doc.FinPeriodID;
                    adj.AdjdTranPeriodID = doc.TranPeriodID;
                    adj.AdjdOrigCuryInfoID = doc.CuryInfoID;
                    adj.AdjgCuryInfoID = doc.CuryInfoID;
                    adj.AdjgDocDate = doc.DocDate;
                    adj.AdjgFinPeriodID = doc.FinPeriodID;
                    adj.AdjgTranPeriodID = doc.TranPeriodID;
                    adj.AdjNbr = doc.AdjCntr;
                    adj.AdjAmt = doc.OrigDocAmt;
                    adj.AdjDiscAmt = doc.OrigDiscAmt;
                    adj.RGOLAmt = 0m;
                    adj.CuryAdjdAmt = doc.CuryOrigDocAmt;
                    adj.CuryAdjdDiscAmt = doc.CuryOrigDiscAmt;
                    adj.CuryAdjgAmt = doc.CuryOrigDocAmt;
                    adj.CuryAdjgDiscAmt = doc.CuryOrigDiscAmt;
                    adj.Released = false;
                    adj.CustomerID = doc.CustomerID;
                    ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Insert(adj);
                }

                doc.CuryDocBal += doc.CuryOrigDiscAmt;
                doc.DocBal += doc.OrigDiscAmt;
                doc.ClosedFinPeriodID = doc.FinPeriodID;
                doc.ClosedTranPeriodID = doc.TranPeriodID;
            }

            doc.Released = true;

            PXResultset<ARAdjust> adjustments = ARAdjustsToRelease
                                                ?? ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, AdjNbr);

            var lastAdjustment = ProcessAdjustments(je, adjustments, doc, ardoc, vend, new_info, paycury);

            ARAdjust prev_adj = lastAdjustment.Item1;
            CurrencyInfo prev_info = lastAdjustment.Item2;

            if (_IsIntegrityCheck == false && (bool)ardoc.VoidAppl == false && doc.CuryDocBal < 0m)
            {
                throw new PXException(Messages.DocumentBalanceNegative);
            }

            /// The case, when payment is open and sum of base amounts for applications 
            /// exceeds payment base amount, due to small rounding for each application.
            /// 
            bool isOpenPaymentWithNegativeBalance = doc.CuryDocBal > 0m && doc.DocBal < 0;

            if (!_IsIntegrityCheck &&
                prev_adj.AdjdRefNbr != null &&
                (doc.CuryDocBal == 0m && doc.DocBal != 0m || isOpenPaymentWithNegativeBalance))
            {
                ProcessRounding(je, doc, prev_adj, ardoc, vend, paycury, prev_info, new_info, doc.DocBal, prev_adj.ReverseGainLoss);
            }

            bool isVoidingDoc = ardoc.VoidAppl == true ||
                (ardoc.SelfVoidingDoc == true && prev_adj.Voided == true);

            if (doc.CuryDocBal == 0m || isVoidingDoc)
            {
                doc.CuryDocBal = 0m;
                doc.DocBal = 0m;
                doc.OpenDoc = false;

                SetClosedPeriodsFromLatestApplication(doc, doc.AdjCntr);
                if (isVoidingDoc && doc.DocType != ARDocType.CashReturn)
                {
                    UpdateVoidedCheck(doc);
                }

                if (!(bool)ardoc.VoidAppl)
                {
                    DeactivateOneTimeCustomerIfAllDocsIsClosed(vend);
                }
            }
            else
            {
                if (isOpenPaymentWithNegativeBalance)
                {
                    doc.DocBal = 0m;
                }

                doc.OpenDoc = true;
                doc.ClosedTranPeriodID = null;
                doc.ClosedFinPeriodID = null;
            }
        }

        protected void DeactivateOneTimeCustomerIfAllDocsIsClosed(Customer customer)
        {
            if (customer.Status != BAccount.status.OneTime)
                return;

            ARRegister apRegister = PXSelect<ARRegister,
                                                Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>,
                                                        And<ARRegister.released, Equal<boolTrue>,
                                                        And<ARRegister.openDoc, Equal<boolTrue>>>>>
                                                .SelectWindowed(this, 0, 1, customer.BAccountID);

            if (apRegister != null)
                return;

            customer.Status = BAccount.status.Inactive;
            Caches[typeof(Customer)].Update(customer);
            Caches[typeof(Customer)].Persist(PXDBOperation.Update);
            Caches[typeof(Customer)].Persisted(false);
        }

        protected virtual Tuple<ARAdjust, CurrencyInfo> ProcessAdjustments(JournalEntry je, PXResultset<ARAdjust> adjustments, ARRegister paymentRegister, ARPayment payment, Customer paymentCustomer, CurrencyInfo new_info, Currency paycury)
        {
            ARAdjust prev_adj = new ARAdjust();
            CurrencyInfo prev_info = new CurrencyInfo();

            foreach (PXResult<ARAdjust, CurrencyInfo, Currency, ARRegister, ARPayment> adjres in adjustments)
            {
                ARAdjust adj = adjres;
                CurrencyInfo vouch_info = adjres;
                Currency cury = adjres;
                ARRegister adjustedInvoice = adjres;
                ARPayment adjgdoc = adjres;

                EnsureNoUnreleasedVoidPaymentExists(
                    this,
                    adjgdoc,
                    payment.DocType == ARDocType.Refund
                        ? Common.Messages.ActionRefunded
                        : Common.Messages.ActionAdjusted);

                if (adj.CuryAdjgAmt == 0m && adj.CuryAdjgDiscAmt == 0m)
                {
                    ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Delete(adj);
                    continue;
                }

                if (adj.Hold == true)
                {
                    throw new PXException(Messages.Document_OnHold_CannotRelease);
                }

                if (_IsIntegrityCheck == false && adj.PendingPPD == true)
                {
                    adjustedInvoice.PendingPPD = !adj.Voided;
                    ARDocument.Cache.Update(adjustedInvoice);
                }

                bool notValidatingCrossCustomerFromAdjusting = (_IsIntegrityCheck && adj.CustomerID != adj.AdjdCustomerID) == false;

                ProcessSalesPersonCommission(adj, payment, adjustedInvoice, cury);

                if (adjustedInvoice.RefNbr != null)
                {
                    if (notValidatingCrossCustomerFromAdjusting)
                    {
                        //Void Payment is processed after SC, avoid balance update since SC does not hold any balance even after application reversal and has WO Account in AR Account
                        if (adjustedInvoice.DocType != ARDocType.SmallCreditWO)
                        {
                            UpdateBalances(adj, adjustedInvoice);
                            UpdateARBalances(adj, adjustedInvoice);
                        }
                        else if (adj.Voided == true && adjustedInvoice.Voided != true)
                        {
                            adjustedInvoice.Voided = true;
                            ARDocument.Cache.Update(adjustedInvoice);

                            if (_IsIntegrityCheck == false && adj.VoidAdjNbr != null)
                            {
                                VoidOrigAdjustment(adj);
                            }
                        }
                    }

                    UpdateARBalances(adj, paymentRegister);

                    if (notValidatingCrossCustomerFromAdjusting)
                    {
                        UpdateARBalancesDates(adjustedInvoice, adjustments.Count);
                    }
                }
                else
                {
                    UpdateBalances(adj, adjgdoc);
                    UpdateARBalances(adj, paymentRegister);
                    UpdateARBalances(adj, adjgdoc);
                }

                ProcessAdjusting(je, adj, payment, paymentCustomer, new_info);

                if (notValidatingCrossCustomerFromAdjusting)
                {
                    ProcessAdjusted(je, adj, adjustedInvoice, payment, vouch_info, new_info);
                }

                ProcessCashDiscount(je, adj, payment, paymentCustomer, new_info, vouch_info);
                ProcessWriteOff(je, adj, payment, paymentCustomer, new_info, vouch_info);
                ProcessGOL(je, adj, payment, paymentCustomer, adjustedInvoice, paycury, cury, new_info, vouch_info);

                //true for Quick Check and Void Quick Check
                if (adj.AdjgDocType != adj.AdjdDocType || adj.AdjgRefNbr != adj.AdjdRefNbr)
                {
                    paymentRegister.CuryDocBal -= adj.AdjgBalSign * adj.CuryAdjgAmt;
                    paymentRegister.DocBal -= adj.AdjgBalSign * adj.AdjAmt;
                }

                if (_IsIntegrityCheck == false)
                {
                    if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
                    {
                        je.Save.Press();
                    }

                    if (!je.BatchModule.Cache.IsDirty)
                    {
                        adj.AdjBatchNbr = ((Batch)je.BatchModule.Current).BatchNbr;
                    }
                    adj.Released = true;
                    adj = (ARAdjust)Caches[typeof(ARAdjust)].Update(adj);
                }

                prev_adj = adj;
                prev_info = adjres;

                ProcessSVATAdjustments(prev_adj, adjustedInvoice, paymentRegister);
            }

            return new Tuple<ARAdjust, CurrencyInfo>(prev_adj, prev_info);
        }

        protected virtual void ProcessSVATAdjustments(ARAdjust adj, ARRegister adjddoc, ARRegister adjgdoc)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.vATReporting>() && _IsIntegrityCheck == false)
            {
                foreach (SVATConversionHist docSVAT in PXSelect<SVATConversionHist, Where<
                    SVATConversionHist.module, Equal<BatchModule.moduleAR>,
                    And2<Where<SVATConversionHist.adjdDocType, Equal<Current<ARAdjust.adjdDocType>>,
                        And<SVATConversionHist.adjdRefNbr, Equal<Current<ARAdjust.adjdRefNbr>>,
                        Or<SVATConversionHist.adjdDocType, Equal<Current<ARAdjust.adjgDocType>>,
                        And<SVATConversionHist.adjdRefNbr, Equal<Current<ARAdjust.adjgRefNbr>>>>>>,
                    And<SVATConversionHist.reversalMethod, Equal<SVATTaxReversalMethods.onPayments>,
                    And<Where<SVATConversionHist.adjdDocType, Equal<SVATConversionHist.adjgDocType>,
                        And<SVATConversionHist.adjdRefNbr, Equal<SVATConversionHist.adjgRefNbr>>>>>>>>
                    .SelectMultiBound(this, new object[] { adj }))
                {
                    bool isPayment = adj.AdjgDocType == docSVAT.AdjdDocType && adj.AdjgRefNbr == docSVAT.AdjdRefNbr;
                    decimal percent = isPayment
                        ? ((adj.CuryAdjgAmt ?? 0m) + (adj.CuryAdjgDiscAmt ?? 0m) + (adj.CuryAdjgWOAmt ?? 0m)) / (adjgdoc.CuryOrigDocAmt ?? 0m)
                        : ((adj.CuryAdjdAmt ?? 0m) + (adj.CuryAdjdDiscAmt ?? 0m) + (adj.CuryAdjdWOAmt ?? 0m)) / (adjddoc.CuryOrigDocAmt ?? 0m);
                    decimal curyTaxableAmt = PXDBCurrencyAttribute.RoundCury(SVATConversionHistory.Cache, docSVAT, (docSVAT.CuryTaxableAmt ?? 0m) * percent);
                    decimal curyTaxAmt = PXDBCurrencyAttribute.RoundCury(SVATConversionHistory.Cache, docSVAT, (docSVAT.CuryTaxAmt ?? 0m) * percent);

                    SVATConversionHist adjSVAT = new SVATConversionHist
                    {
                        Module = BatchModule.AR,
                        AdjdBranchID = adj.AdjdBranchID,
                        AdjdDocType = isPayment ? adj.AdjgDocType : adj.AdjdDocType,
                        AdjdRefNbr = isPayment ? adj.AdjgRefNbr : adj.AdjdRefNbr,
                        AdjgDocType = isPayment ? adj.AdjdDocType : adj.AdjgDocType,
                        AdjgRefNbr = isPayment ? adj.AdjdRefNbr : adj.AdjgRefNbr,
                        AdjNbr = adj.AdjNbr,
                        AdjdDocDate = adj.AdjgDocDate,
                        AdjdFinPeriodID = adj.AdjdFinPeriodID,

                        TaxID = docSVAT.TaxID,
                        TaxType = docSVAT.TaxType,
                        TaxRate = docSVAT.TaxRate,
                        VendorID = docSVAT.VendorID,
                        ReversalMethod = SVATTaxReversalMethods.OnPayments,

                        CuryInfoID = docSVAT.CuryInfoID,
                        CuryTaxableAmt = curyTaxableAmt,
                        CuryTaxAmt = curyTaxAmt,
                        CuryUnrecognizedTaxAmt = curyTaxAmt
                    };

                    adjSVAT.FillBaseAmounts(SVATConversionHistory.Cache);

                    if (adjSVAT.CuryTaxAmt != docSVAT.CuryTaxAmt &&
                        (isPayment ? adjgdoc.CuryDocBal : adjddoc.CuryDocBal) == 0m)
                    {
                        PXResultset<SVATConversionHist> rows = PXSelect<SVATConversionHist, Where<
                            SVATConversionHist.module, Equal<BatchModule.moduleAR>,
                            And<SVATConversionHist.adjdDocType, Equal<Current<SVATConversionHist.adjdDocType>>,
                            And<SVATConversionHist.adjdRefNbr, Equal<Current<SVATConversionHist.adjdRefNbr>>,
                            And<SVATConversionHist.taxID, Equal<Current<SVATConversionHist.taxID>>,
                            And<Where<SVATConversionHist.adjdDocType, NotEqual<SVATConversionHist.adjgDocType>,
                                Or<SVATConversionHist.adjdRefNbr, NotEqual<SVATConversionHist.adjgRefNbr>>>>>>>>>
                            .SelectMultiBound(this, new object[] { docSVAT });
                        if (rows.Any())
                        {
                            adjSVAT.CuryTaxableAmt = docSVAT.CuryTaxableAmt;
                            adjSVAT.TaxableAmt = docSVAT.TaxableAmt;
                            adjSVAT.CuryTaxAmt = docSVAT.CuryTaxAmt;
                            adjSVAT.TaxAmt = docSVAT.TaxAmt;

                            foreach (SVATConversionHist row in rows)
                            {
                                adjSVAT.CuryTaxableAmt -= (row.CuryTaxableAmt ?? 0m);
                                adjSVAT.TaxableAmt -= (row.TaxableAmt ?? 0m);
                                adjSVAT.CuryTaxAmt -= (row.CuryTaxAmt ?? 0m);
                                adjSVAT.TaxAmt -= (row.TaxAmt ?? 0m);
                            }

                            adjSVAT.CuryUnrecognizedTaxAmt = adjSVAT.CuryTaxAmt;
                            adjSVAT.UnrecognizedTaxAmt = adjSVAT.TaxAmt;
                        }
                    }

                    adjSVAT = (SVATConversionHist)SVATConversionHistory.Cache.Insert(adjSVAT);

                    docSVAT.Processed = false;
                    docSVAT.AdjgFinPeriodID = null;

                    PXTimeStampScope.PutPersisted(SVATConversionHistory.Cache, docSVAT, PXDatabase.SelectTimeStamp());
                    SVATConversionHistory.Cache.Update(docSVAT);
                }
            }
        }

        protected virtual void ProcessAdjustmentsOnlyAdjusted(JournalEntry je, PXResultset<ARAdjust> adjustments)
        {
            foreach (PXResult<ARAdjust, CurrencyInfo, Currency, ARInvoice, ARPayment> adjres in adjustments)
            {
                ARAdjust adj = adjres;
                CurrencyInfo vouch_info = adjres;
                Currency cury = adjres;
                ARInvoice adjddoc = adjres;
                ARPayment adjgdoc = adjres;

                if (adj.CuryAdjgAmt == 0m && adj.CuryAdjgDiscAmt == 0m)
                {
                    ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Delete(adj);
                    continue;
                }

                if (adj.Hold == true)
                {
                    throw new PXException(Messages.Document_OnHold_CannotRelease);
                }

                if (adjddoc.RefNbr != null)
                {
                    if (adjddoc.DocType != ARDocType.SmallCreditWO)
                    {
                        UpdateBalances(adj, adjddoc);
                        UpdateARBalances(adj, adjddoc);
                    }
                    else if (adj.Voided == true && adjddoc.Voided != true)
                    {
                        adjddoc.Voided = true;
                        Caches[typeof(ARInvoice)].Update(adjddoc);

                        if (_IsIntegrityCheck == false && adj.VoidAdjNbr != null)
                        {
                            VoidOrigAdjustment(adj);
                        }
                    }

                    UpdateARBalancesDates(adjddoc, adjustments.Count);
                }

                CurrencyInfo payment_info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(this, adjgdoc.CuryInfoID);

                ProcessAdjusted(je, adj, adjddoc, adjgdoc, vouch_info, payment_info);
            }
        }

        private void ProcessSalesPersonCommission(ARAdjust adj, ARPayment payment, ARRegister adjustedInvoice, Currency cury)
        {
            /// dont process if validating child
            List<ARSalesPerTran> spTrans = new List<ARSalesPerTran>();
            if (payment.DocType != ARDocType.CreditMemo) //Credit memos are treates as negative invoice
            {
                foreach (ARSalesPerTran iSPT in this.ARDoc_SalesPerTrans.Select(adjustedInvoice.DocType, adjustedInvoice.RefNbr))
                {
                    ARSalesPerTran paySPT = new ARSalesPerTran();
                    Copy(paySPT, payment);
                    Copy(paySPT, adj);
                    decimal applRatio = ((adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt) / adjustedInvoice.CuryOrigDocAmt).Value;
                    if (payment.DocType == ARDocType.CashSale || payment.DocType == ARDocType.CashReturn)
                    {
                        applRatio = 1m;
                    }
                    else if (payment.DocType == ARDocType.CreditMemo)
                    {
                        applRatio = -applRatio;
                    }
                    CopyShare(paySPT, iSPT, applRatio, (cury.DecimalPlaces ?? 2));
                    paySPT = this.ARDoc_SalesPerTrans.Insert(paySPT);
                }
            }
        }

        private void ProcessAdjusting(JournalEntry je, ARAdjust adj, ARPayment payment, Customer paymentCustomer, CurrencyInfo new_info)
        {
            GLTran tran = new GLTran();
            tran.SummPost = true;
            tran.ZeroPost = false;
            tran.BranchID = adj.AdjgBranchID;
            tran.AccountID = payment.ARAccountID;
            tran.ReclassificationProhibited = true;
            tran.SubID = payment.ARSubID;
            tran.DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjAmt : 0m;
            tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgAmt : 0m;
            tran.CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjAmt;
            tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgAmt;
            tran.TranType = adj.AdjgDocType;
            tran.TranClass = GLTran.tranClass.Payment;
            tran.RefNbr = adj.AdjgRefNbr;
            tran.TranDesc = payment.DocDesc;
            tran.TranDate = adj.AdjgDocDate;
            tran.TranPeriodID = adj.AdjgTranPeriodID;
            tran.FinPeriodID = adj.AdjgFinPeriodID;
            tran.CuryInfoID = new_info.CuryInfoID;
            tran.Released = true;
            tran.ReferenceID = payment.CustomerID;
            tran.OrigAccountID = adj.AdjdARAcct;
            tran.OrigSubID = adj.AdjdARSub;
            tran.ProjectID = payment.ProjectID;
            tran.TaskID = payment.TaskID;

            UpdateHistory(tran, paymentCustomer);
            UpdateHistory(tran, paymentCustomer, new_info);

            if (adj.AdjdDocType == ARDocType.SmallCreditWO)
            {
                bool isPrepayment = (adj.AdjgDocType == ARDocType.Prepayment);
                if (adj.AdjgDocType == ARDocType.VoidPayment)
                {
                    ARRegister orig = PXSelect<ARRegister, Where<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>,
                        And<Where<ARRegister.docType, Equal<ARDocType.payment>,
                        Or<ARRegister.docType, Equal<ARDocType.prepayment>>>>>,
                        OrderBy<Asc<Switch<Case<Where<ARRegister.docType, Equal<ARDocType.payment>>, int0>, int1>, Asc<ARRegister.docType, Asc<ARRegister.refNbr>>>>>.Select(this, tran.RefNbr);
                    isPrepayment = (orig != null && orig.DocType == ARDocType.Prepayment);
                }

                if (isPrepayment)
                {
                    ARHistBucket bucket = new ARHistBucket();
                    bucket.arAccountID = tran.AccountID;
                    bucket.arSubID = tran.SubID;
                    bucket.SignPayments = 1m;
                    bucket.SignDeposits = -1m;
                    bucket.SignPtd = -1m;

                    UpdateHistory(tran, paymentCustomer, bucket);
                    UpdateHistory(tran, paymentCustomer, new_info, bucket);
                }
            }

            je.GLTranModuleBatNbr.Insert(tran);
        }

        private void ProcessAdjusted(JournalEntry je, ARAdjust adj, ARRegister adjustedInvoice, ARPayment payment, CurrencyInfo vouch_info, CurrencyInfo new_info)
        {
            Customer voucherCustomer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(this, adj.AdjdCustomerID);

            //Credit Voucher AR Account/minus RGOL for refund
            var tran = new GLTran();
            tran.SummPost = true;
            tran.ZeroPost = false;
            tran.BranchID = adj.AdjdBranchID;
            //Small-Credit has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
            tran.AccountID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARAccountID : adj.AdjdARAcct;
            tran.ReclassificationProhibited = true;
            tran.SubID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARSubID : adj.AdjdARSub;
            //Small-Credit reversal should update history for payment AR Account(AdjdARAcct)
            tran.OrigAccountID = adj.AdjdARAcct;
            tran.OrigSubID = adj.AdjdARSub;
            tran.CreditAmt = (adj.AdjgGLSign == 1m) ? adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt + adj.RGOLAmt : 0m;
            tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? tran.CreditAmt : adj.CuryAdjgAmt + adj.CuryAdjgDiscAmt + adj.CuryAdjgWOAmt) : 0m;
            tran.DebitAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjAmt + adj.AdjDiscAmt + adj.AdjWOAmt - adj.RGOLAmt;
            tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? 0m : (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? tran.DebitAmt : adj.CuryAdjgAmt + adj.CuryAdjgDiscAmt + adj.CuryAdjgWOAmt);
            tran.TranType = adj.AdjgDocType;
            //always N for AdjdDocs except Prepayment
            tran.TranClass = ARDocType.DocClass(adj.AdjdDocType);
            tran.RefNbr = adj.AdjgRefNbr;
            tran.TranDesc = payment.DocDesc;
            tran.TranDate = adj.AdjgDocDate;
            tran.TranPeriodID = adj.AdjgTranPeriodID;
            tran.FinPeriodID = adj.AdjgFinPeriodID;
            tran.CuryInfoID = new_info.CuryInfoID;
            tran.Released = true;
            tran.ReferenceID = payment.CustomerID;
            tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

            UpdateHistory(tran, voucherCustomer);

            je.GLTranModuleBatNbr.Insert(tran);

            //Update CuryHistory in Voucher currency
            tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt + adj.CuryAdjdWOAmt) : 0m;
            tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? 0m : (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : adj.CuryAdjdAmt + adj.CuryAdjdDiscAmt + adj.CuryAdjdWOAmt);
            UpdateHistory(tran, voucherCustomer, vouch_info);
        }

        private void ProcessCashDiscount(JournalEntry je, ARAdjust adj, ARPayment payment, Customer paymentCustomer, CurrencyInfo new_info, CurrencyInfo vouch_info)
        {
            //Credit Discount Taken/does not apply to refund, since no disc in AD
            var tran = new GLTran();
            tran.SummPost = this.SummPost;
            tran.BranchID = adj.AdjdBranchID;
            tran.AccountID = paymentCustomer.DiscTakenAcctID;
            tran.SubID = paymentCustomer.DiscTakenSubID;
            tran.OrigAccountID = adj.AdjdARAcct;
            tran.OrigSubID = adj.AdjdARSub;
            tran.DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjDiscAmt : 0m;
            tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgDiscAmt : 0m;
            tran.CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjDiscAmt;
            tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgDiscAmt;
            tran.TranType = adj.AdjgDocType;
            tran.TranClass = GLTran.tranClass.Discount;
            tran.RefNbr = adj.AdjgRefNbr;
            tran.TranDesc = payment.DocDesc;
            tran.TranDate = adj.AdjgDocDate;
            tran.TranPeriodID = adj.AdjgTranPeriodID;
            tran.FinPeriodID = adj.AdjgFinPeriodID;
            tran.CuryInfoID = new_info.CuryInfoID;
            tran.Released = true;
            tran.ReferenceID = payment.CustomerID;
            tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

            UpdateHistory(tran, paymentCustomer);

            je.GLTranModuleBatNbr.Insert(tran);

            //Update CuryHistory in Voucher currency
            tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjdDiscAmt : 0m;
            tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjdDiscAmt;
            UpdateHistory(tran, paymentCustomer, vouch_info);
        }

        private void ProcessWriteOff(JournalEntry je, ARAdjust adj, ARPayment payment, Customer paymentCustomer, CurrencyInfo new_info, CurrencyInfo vouch_info)
        {
            //Credit WO Account
            if (adj.AdjWOAmt != 0)
            {
                ARInvoice adjusted = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(pe, adj.AdjdDocType, adj.AdjdRefNbr);

                ReasonCode reasonCode = PXSelect<ReasonCode, Where<ReasonCode.reasonCodeID, Equal<Required<ReasonCode.reasonCodeID>>>>.Select(this, adj.WriteOffReasonCode);

                if (reasonCode == null)
                    throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.ReasonCodeNotFound, adj.WriteOffReasonCode));

                Location customerLocation = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                    And<Location.locationID, Equal<Required<Location.locationID>>>>>.Select((PXGraph)pe, adjusted.CustomerID, adjusted.CustomerLocationID);

                CRLocation companyLocation = PXSelectJoin<CRLocation,
                    InnerJoin<BAccountR, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>,
                InnerJoin<GL.Branch, On<BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>, Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select((PXGraph)pe, adjusted.BranchID);

                object value = null;
                if (reasonCode.Usage == ReasonCodeUsages.BalanceWriteOff || reasonCode.Usage == ReasonCodeUsages.CreditWriteOff)
                {
                    value = ReasonCodeSubAccountMaskAttribute.MakeSub<ReasonCode.subMask>((PXGraph)pe, reasonCode.SubMask,
                        new object[] { reasonCode.SubID, customerLocation.CSalesSubID, companyLocation.CMPSalesSubID },
                        new Type[] { typeof(ReasonCode.subID), typeof(Location.cSalesSubID), typeof(Location.cMPSalesSubID) });
                }
                else
                {
                    throw new PXException(Messages.InvalidReasonCode);
                }

                var tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = adj.AdjdBranchID;
                tran.AccountID = reasonCode.AccountID;
                tran.SubID = reasonCode.SubID;
                tran.OrigAccountID = adj.AdjdARAcct;
                tran.OrigSubID = adj.AdjdARSub;
                tran.DebitAmt = (adj.AdjgGLSign == 1m) ? adj.AdjWOAmt : 0m;
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjgWOAmt : 0m;
                tran.CreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.AdjWOAmt;
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjgWOAmt;
                tran.TranType = adj.AdjgDocType;
                tran.TranClass = GLTran.tranClass.WriteOff;
                tran.RefNbr = adj.AdjgRefNbr;
                tran.TranDesc = payment.DocDesc;
                tran.TranDate = adj.AdjgDocDate;
                tran.TranPeriodID = adj.AdjgTranPeriodID;
                tran.FinPeriodID = adj.AdjgFinPeriodID;
                tran.CuryInfoID = new_info.CuryInfoID;
                tran.Released = true;
                tran.ReferenceID = payment.CustomerID;
                tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

                UpdateHistory(tran, paymentCustomer);

                GLTran tranNew = je.GLTranModuleBatNbr.Insert(tran);
                je.GLTranModuleBatNbr.SetValueExt<GLTran.subID>(tranNew, value);

                //Update CuryHistory in Voucher currency
                tran.CuryDebitAmt = (adj.AdjgGLSign == 1m) ? adj.CuryAdjdWOAmt : 0m;
                tran.CuryCreditAmt = (adj.AdjgGLSign == 1m) ? 0m : adj.CuryAdjdWOAmt;
                UpdateHistory(tran, paymentCustomer, vouch_info);
            }
        }

        private void ProcessGOL(JournalEntry je, ARAdjust adj, ARPayment payment, Customer paymentCustomer, ARRegister adjustedInvoice, Currency paycury, Currency cury, CurrencyInfo new_info, CurrencyInfo vouch_info)
        {
            //Debit/Credit RGOL Account
            if (cury.RealGainAcctID != null && cury.RealLossAcctID != null)
            {
                var tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.BranchID : adj.AdjdBranchID;
                tran.AccountID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl)
                    ? cury.RealLossAcctID
                    : cury.RealGainAcctID;
                tran.SubID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl)
                    ? GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, tran.BranchID, cury)
                    : GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, tran.BranchID, cury);
                //SC has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
                tran.OrigAccountID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARAccountID : adj.AdjdARAcct;
                tran.OrigSubID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARSubID : adj.AdjdARSub;
                tran.CreditAmt = (adj.RGOLAmt < 0m) ? -1m * adj.RGOLAmt : 0m;
                //!object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) === precision alteration before payment application
                tran.CuryCreditAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : 0m;
                tran.DebitAmt = (adj.RGOLAmt > 0m) ? adj.RGOLAmt : 0m;
                tran.CuryDebitAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : 0m;
                tran.TranType = adj.AdjgDocType;
                tran.TranClass = GLTran.tranClass.RealizedAndRoundingGOL;
                tran.RefNbr = adj.AdjgRefNbr;
                tran.TranDesc = payment.DocDesc;
                tran.TranDate = adj.AdjgDocDate;
                tran.TranPeriodID = adj.AdjgTranPeriodID;
                tran.FinPeriodID = adj.AdjgFinPeriodID;
                tran.CuryInfoID = new_info.CuryInfoID;
                tran.Released = true;
                tran.ReferenceID = payment.CustomerID;
                tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

                UpdateHistory(tran, paymentCustomer);

                je.GLTranModuleBatNbr.Insert(tran);

                //Update CuryHistory in Voucher currency
                tran.CuryDebitAmt = 0m;
                tran.CuryCreditAmt = 0m;
                UpdateHistory(tran, paymentCustomer, vouch_info);
            }
            //Debit/Credit Rounding Gain-Loss Account
            else if (paycury.RoundingGainAcctID != null && paycury.RoundingLossAcctID != null)
            {
                var tran = new GLTran();
                tran.SummPost = this.SummPost;
                tran.BranchID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.BranchID : adj.AdjdBranchID;
                tran.AccountID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl)
                    ? paycury.RoundingLossAcctID
                    : paycury.RoundingGainAcctID;
                tran.SubID = (adj.RGOLAmt > 0m && !(bool)adj.VoidAppl || adj.RGOLAmt < 0m && (bool)adj.VoidAppl)
                    ? GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, paycury)
                    : GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, paycury);

                //SC has Payment AR Account in AdjdARAcct  and WO Account in ARAccountID
                tran.OrigAccountID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARAccountID : adj.AdjdARAcct;
                tran.OrigSubID = (adj.AdjdDocType == ARDocType.SmallCreditWO) ? adjustedInvoice.ARSubID : adj.AdjdARSub;
                tran.CreditAmt = (adj.RGOLAmt < 0m) ? -1m * adj.RGOLAmt : 0m;
                //!object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) === precision alteration before payment application
                tran.CuryCreditAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.CreditAmt : 0m;
                tran.DebitAmt = (adj.RGOLAmt > 0m) ? adj.RGOLAmt : 0m;
                tran.CuryDebitAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) && !object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? tran.DebitAmt : 0m;
                tran.TranType = adj.AdjgDocType;
                tran.TranClass = GLTran.tranClass.RealizedAndRoundingGOL;
                tran.RefNbr = adj.AdjgRefNbr;
                tran.TranDesc = payment.DocDesc;
                tran.TranDate = adj.AdjgDocDate;
                tran.TranPeriodID = adj.AdjgTranPeriodID;
                tran.FinPeriodID = adj.AdjgFinPeriodID;
                tran.CuryInfoID = new_info.CuryInfoID;
                tran.Released = true;
                tran.ReferenceID = payment.CustomerID;
                tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

                UpdateHistory(tran, paymentCustomer);

                je.GLTranModuleBatNbr.Insert(tran);

                //Update CuryHistory in Voucher currency
                tran.CuryDebitAmt = 0m;
                tran.CuryCreditAmt = 0m;
                UpdateHistory(tran, paymentCustomer, vouch_info);
            }
        }

        private void ProcessRounding(
            JournalEntry je,
            ARRegister doc,
            ARAdjust prev_adj,
            ARPayment ardoc,
            Customer vend,
            Currency paycury,
            CurrencyInfo prev_info,
            CurrencyInfo new_info,
            decimal? amount,
            bool? isReversed = false)
        {
            if (prev_adj.VoidAppl == true || Equals(new_info.CuryID, new_info.BaseCuryID))
            {
                throw new PXException(Messages.UnexpectedRoundingForApplication);
            }

            UpdateARBalances(this, doc, amount, 0m);

            //BaseCalc should be false
            prev_adj.AdjAmt += amount;
            decimal? roundingLoss = isReversed != true ? amount : -amount;
            prev_adj.RGOLAmt -= roundingLoss;
            prev_adj = (ARAdjust)Caches[typeof(ARAdjust)].Update(prev_adj);

            ARRegister adjdDoc = (ARRegister)ARDocument.Cache.Locate(
                new ARRegister { DocType = prev_adj.AdjdDocType, RefNbr = prev_adj.AdjdRefNbr });
            if (adjdDoc != null)
            {
                adjdDoc.RGOLAmt -= roundingLoss;
                ARDocument.Cache.Update(adjdDoc);
            }

            //signs are reversed to RGOL
            GLTran tran = new GLTran();
            tran.SummPost = SummPost;
            tran.BranchID = ardoc.BranchID;
            tran.AccountID = (roundingLoss < 0m)
                ? paycury.RoundingLossAcctID
                : paycury.RoundingGainAcctID;
            tran.SubID = (roundingLoss < 0m)
                ? GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, paycury)
                : GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, paycury);
            tran.OrigAccountID = prev_adj.AdjdARAcct;
            tran.OrigSubID = prev_adj.AdjdARSub;
            tran.CreditAmt = (roundingLoss > 0m) ? roundingLoss : 0m;
            tran.CuryCreditAmt = 0m;
            tran.DebitAmt = (roundingLoss < 0m) ? -roundingLoss : 0m;
            tran.CuryDebitAmt = 0m;
            tran.TranType = prev_adj.AdjgDocType;
            tran.TranClass = "R";
            tran.RefNbr = prev_adj.AdjgRefNbr;
            tran.TranDesc = ardoc.DocDesc;
            tran.TranDate = prev_adj.AdjgDocDate;
            tran.TranPeriodID = prev_adj.AdjgTranPeriodID;
            tran.FinPeriodID = prev_adj.AdjgFinPeriodID;
            tran.CuryInfoID = new_info.CuryInfoID;
            tran.Released = true;
            tran.ReferenceID = ardoc.CustomerID;
            tran.ProjectID = ProjectDefaultAttribute.NonProject(je);

            UpdateHistory(tran, vend);
            //Update CuryHistory in Voucher currency
            UpdateHistory(tran, vend, prev_info);

            je.GLTranModuleBatNbr.Insert(tran);

            //Credit Payment AR Account
            tran = new GLTran();
            tran.SummPost = true;
            tran.ZeroPost = false;
            tran.BranchID = ardoc.BranchID;
            tran.AccountID = ardoc.ARAccountID;
            tran.SubID = ardoc.ARSubID;
            tran.ReclassificationProhibited = true;
            tran.DebitAmt = (roundingLoss > 0m) ? roundingLoss : 0m;
            tran.CuryDebitAmt = 0m;
            tran.CreditAmt = (roundingLoss < 0m) ? -roundingLoss : 0m;
            tran.CuryCreditAmt = 0m;
            tran.TranType = prev_adj.AdjgDocType;
            tran.TranClass = "P";
            tran.RefNbr = prev_adj.AdjgRefNbr;
            tran.TranDesc = ardoc.DocDesc;
            tran.TranDate = prev_adj.AdjgDocDate;
            tran.TranPeriodID = prev_adj.AdjgTranPeriodID;
            tran.FinPeriodID = prev_adj.AdjgFinPeriodID;
            tran.CuryInfoID = new_info.CuryInfoID;
            tran.Released = true;
            tran.ReferenceID = ardoc.CustomerID;
            tran.OrigAccountID = prev_adj.AdjdARAcct;
            tran.OrigSubID = prev_adj.AdjdARSub;
            tran.ProjectID = ProjectDefaultAttribute.NonProject(je);

            UpdateHistory(tran, vend);
            //Update CuryHistory in Payment currency
            UpdateHistory(tran, vend, new_info);

            je.GLTranModuleBatNbr.Insert(tran);
        }

        public virtual void ReleaseSmallCreditProc(JournalEntry je, ref ARRegister doc, PXResult<ARInvoice, CurrencyInfo, Terms, Customer> res)
        {
            ARInvoice ardoc = res;
            CurrencyInfo info = res;
            Customer vend = res;
            CustomerClass custclass = PXSelectJoin<CustomerClass, InnerJoin<ARSetup, On<ARSetup.dfltCustomerClassID, Equal<CustomerClass.customerClassID>>>>.Select(this, null);

            CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(info);
            new_info.CuryInfoID = null;
            new_info.ModuleCode = "GL";
            new_info = je.currencyinfo.Insert(new_info) ?? new_info;

            if (doc.Released == false)
            {
                doc.CuryDocBal = doc.CuryOrigDocAmt;
                doc.DocBal = doc.OrigDocAmt;

                doc.OpenDoc = true;
                doc.ClosedFinPeriodID = null;
                doc.ClosedTranPeriodID = null;
            }

            doc.Released = true;
            decimal? docRGOLAmt = doc.RGOLAmt;
            ARAdjust prev_adj = new ARAdjust();

            foreach (PXResult<ARAdjust, CurrencyInfo, Currency, ARPayment> adjres in ARAdjust_AdjdDocType_RefNbr_CustomerID
                .Select(doc.DocType, doc.RefNbr, doc.AdjCntr))
            {
                ARAdjust adj = adjres;
                CurrencyInfo vouch_info = adjres;
                Currency cury = adjres;
                ARPayment adjddoc = adjres;

                EnsureNoUnreleasedVoidPaymentExists(this, adjddoc, Common.Messages.ActionWrittenOff);

                /// For correct balance calculation, 
                /// application RGOL amount should be equal to zero. 
                /// 
                decimal? adjRGOLAmt = adj.RGOLAmt;
                adj.RGOLAmt = 0m;

                UpdateBalances(adj, adjddoc);
                UpdateARBalances(adj, doc);
                UpdateARBalances(adj, adjddoc);

                /// Credit WO Account
                /// 
                GLTran tran = new GLTran();
                tran.SummPost = true;
                tran.BranchID = ardoc.BranchID;
                tran.AccountID = ardoc.ARAccountID;
                tran.ReclassificationProhibited = true;
                tran.SubID = ardoc.ARSubID;
                tran.DebitAmt = (ardoc.DrCr == DrCr.Debit) ? adj.AdjAmt : 0m;
                tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? adj.CuryAdjgAmt : 0m;
                tran.CreditAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : adj.AdjAmt;
                tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : adj.CuryAdjgAmt;
                tran.TranType = adj.AdjdDocType;
                tran.TranClass = GLTran.tranClass.Payment;
                tran.RefNbr = adj.AdjdRefNbr;
                tran.TranDesc = ardoc.DocDesc;
                tran.TranDate = adj.AdjdDocDate;
                tran.TranPeriodID = ardoc.TranPeriodID;
                tran.FinPeriodID = ardoc.FinPeriodID;
                tran.CuryInfoID = new_info.CuryInfoID;
                tran.Released = true;
                tran.ReferenceID = ardoc.CustomerID;
                tran.OrigAccountID = adjddoc.ARAccountID;
                tran.OrigSubID = adjddoc.ARSubID;
                tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

                /// Create history for SCWO Account.
                /// 
                UpdateHistory(tran, vend);

                je.GLTranModuleBatNbr.Insert(tran);

                tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjdAmt) : 0m;
                tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjdAmt);

                UpdateHistory(tran, vend, vouch_info);

                /// Debit Payment AR Account.
                /// 
                tran = new GLTran();
                tran.SummPost = true;
                tran.BranchID = adjddoc.BranchID;
                tran.AccountID = adjddoc.ARAccountID;
                tran.ReclassificationProhibited = true;
                tran.SubID = adjddoc.ARSubID;
                tran.CreditAmt = (ardoc.DrCr == DrCr.Debit) ? adj.AdjAmt : 0m;
                tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjgAmt) : 0m;
                tran.DebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : adj.AdjAmt;
                tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : (object.Equals(new_info.CuryID, new_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjgAmt);
                tran.TranType = adj.AdjdDocType;
                tran.TranClass = GLTran.tranClass.Normal;
                tran.RefNbr = adj.AdjdRefNbr;
                tran.TranDesc = ardoc.DocDesc;
                tran.TranDate = adj.AdjdDocDate;
                tran.TranPeriodID = ardoc.TranPeriodID;
                tran.FinPeriodID = ardoc.FinPeriodID;
                tran.CuryInfoID = new_info.CuryInfoID;
                tran.Released = true;
                tran.ReferenceID = ardoc.CustomerID;
                tran.ProjectID = PM.ProjectDefaultAttribute.NonProject(je);

                UpdateHistory(tran, vend);
                ARHistBucket bucket = null;
                bool isPrepayment = adj.AdjgDocType == ARDocType.Prepayment && adj.AdjdDocType == ARDocType.SmallCreditWO;
                if (isPrepayment)
                {
                    bucket = new ARHistBucket();
                    bucket.arAccountID = tran.AccountID;
                    bucket.arSubID = tran.SubID;
                    bucket.SignPayments = 1m;
                    bucket.SignDeposits = -1m;
                    bucket.SignPtd = -1m;

                    UpdateHistory(tran, vend, bucket);
                }

                je.GLTranModuleBatNbr.Insert(tran);

                /// Update CuryHistory in Voucher currency.
                /// 
                tran.CuryCreditAmt = (ardoc.DrCr == DrCr.Debit) ? (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjdAmt) : 0m;
                tran.CuryDebitAmt = (ardoc.DrCr == DrCr.Debit) ? 0m : (object.Equals(vouch_info.CuryID, vouch_info.BaseCuryID) ? adj.AdjAmt : adj.CuryAdjdAmt);
                UpdateHistory(tran, vend, vouch_info);
                if (isPrepayment)
                {
                    UpdateHistory(tran, vend, vouch_info, bucket);
                }

                /// No Discount should take place.
                /// No RGOL should take place.
                /// 
                doc.CuryDocBal -= adj.CuryAdjgAmt;
                doc.DocBal -= adj.AdjAmt;

                if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
                {
                    je.Save.Press();
                }

                if (!je.BatchModule.Cache.IsDirty)
                {
                    adj.AdjBatchNbr = ((Batch)je.BatchModule.Current).BatchNbr;
                }

                adj.Released = true;
                adj.RGOLAmt = adjRGOLAmt;
                prev_adj = (ARAdjust)Caches[typeof(ARAdjust)].Update(adj);

                if (!_IsIntegrityCheck && adjRGOLAmt != 0m)
                {
                    prev_adj.RGOLAmt = 0m;
                    ProcessRounding(je, doc, prev_adj, adjddoc, vend, cury, vouch_info, new_info, adjRGOLAmt);
                }
            }

            /// In cases when the SmallCreditWO document has RGOL amount, we should decrease it DocBal 
            /// to this value (which is sum of RGOL amounts for applications),
            /// because RGOL amount for it applications is excluded from DocBal calculation.
            /// 
            if (doc.CuryDocBal == 0m && doc.DocBal - docRGOLAmt != 0m)
            {
                throw new PXException(Messages.DocumentBalanceNegative);
            }

            if ((bool)doc.OpenDoc == false || doc.CuryDocBal == 0m)
            {
                doc.CuryDocBal = 0m;
                doc.DocBal = 0m;
                doc.OpenDoc = false;

                doc.ClosedFinPeriodID = doc.FinPeriodID;
                doc.ClosedTranPeriodID = doc.TranPeriodID;
            }

            /// Increment default for AdjNbr.
            /// 
            doc.AdjCntr++;
        }

        private void SegregateBatch(JournalEntry je, int? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo)
        {
            JournalEntry.SegregateBatch(je, BatchModule.AR, branchID, curyID, docDate, finPeriodID, description, curyInfo, null);
        }

        public virtual List<ARRegister> ReleaseDocProc(JournalEntry je, ARRegister ardoc, List<Batch> pmBatchList)
        {
            return ReleaseDocProc(je, ardoc, pmBatchList, null);
        }

        public virtual List<ARRegister> ReleaseDocProc(JournalEntry je, ARRegister ardoc, List<Batch> pmBatchList, ARDocumentRelease.ARMassProcessReleaseTransactionScopeDelegate onreleasecomplete)
        {
            List<ARRegister> ret = null;

            if ((bool)ardoc.Hold)
            {
                throw new PXException(Messages.Document_OnHold_CannotRelease);
            }

            // Finding some known data inconsistency problems,
            // if any, the process will be stopped.
            // 
            if (_IsIntegrityCheck != true)
            {
                new DataIntegrityValidator<ARRegister>(
                    je, ARDocument.Cache, ardoc, BatchModule.AR, ardoc.CustomerID, ardoc.Released, arsetup.DataInconsistencyHandlingMode)
                    .CheckTransactionsExistenceForUnreleasedDocument()
                    .Commit();
            }

            ARPayment pmt = PXSelect<ARPayment, Where<ARPayment.docType, Equal<Required<ARPayment.docType>>,
                And<ARPayment.refNbr, Equal<Required<ARPayment.refNbr>>>>>.Select(je, ardoc.DocType, ardoc.RefNbr);
            if (pmt != null && pmt.DocType != ARDocType.CreditMemo && pmt.DocType != ARDocType.SmallBalanceWO && arsetup.RequireExtRef == true && string.IsNullOrEmpty(pmt.ExtRefNbr))
            {
                throw new PXException(ErrorMessages.FieldIsEmpty, PXUIFieldAttribute.GetDisplayName<ARPayment.extRefNbr>(je.Caches[typeof(ARPayment)]));
            }

            ARRegister doc = PXCache<ARRegister>.CreateCopy(ardoc);

            //using (new PXConnectionScope())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    //mark as updated so that doc will not expire from cache and update with Released = 1 will not override balances/amount in document
                    ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

                    UpdateARBalances(doc, -doc.OrigDocAmt);

                    bool Released = (bool)doc.Released;
                    List<PM.PMRegister> pmDocList = new List<PM.PMRegister>();

                    foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res in ARInvoice_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr))
                    {
                        Customer c = res;
                        switch (c.Status)
                        {
                            case Customer.status.Inactive:
                            case Customer.status.Hold:
                            case Customer.status.CreditHold:
                                throw new PXSetPropertyException(Messages.CustomerIsInStatus, new Customer.status.ListAttribute().ValueLabelDic[c.Status]);
                        }

                        PM.PMRegister pmDoc = null;
                        ARInvoice invoice = res;

                        //must check for CM application in different period
                        if (doc.Released == false)
                        {
                            SegregateBatch(je, doc.BranchID, doc.CuryID, doc.DocDate, doc.FinPeriodID, doc.DocDesc, (CurrencyInfo)res);
                        }
                        if (doc.DocType == ARDocType.SmallCreditWO)
                        {
                            ReleaseSmallCreditProc(je, ref doc, res);
                        }
                        else if (_IsIntegrityCheck == false && invoice.DocType != ARDocType.CashSale && invoice.DocType != ARDocType.CashReturn && ((Customer)res).AutoApplyPayments == true && invoice.Released == false)
                        {
                            bool isCCCaptured = false;
                            if (invoice.OrigModule == BatchModule.SO)
                            {
                                SOInvoice soinvoice = PXSelect<SOInvoice, Where<SOInvoice.docType, Equal<Required<SOInvoice.docType>>, And<SOInvoice.refNbr, Equal<Required<SOInvoice.refNbr>>>>>.Select(this, invoice.DocType, invoice.RefNbr);
                                isCCCaptured = soinvoice != null && soinvoice.IsCCCaptured == true;
                            }

                            if (!isCCCaptured)
                            {
                                ie.Clear();
                                ie.Document.Current = invoice;

                                if (ie.Adjustments_Inv.View.SelectSingle() == null)
                                {
                                    ie.LoadInvoicesProc();
                                }
                                ie.Save.Press();
                                doc = (ARRegister)ie.Document.Current;
                                ret = ReleaseDocProc(je, ref doc, new PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account>(ie.Document.Current, (CurrencyInfo)res, (Terms)res, (Customer)res, (Account)res), out pmDoc);
                            }
                        }
                        else
                        {
                            ret = ReleaseDocProc(je, ref doc, res, out pmDoc);
                        }
                        //ensure correct PXDBDefault behaviour on ARTran persisting
                        ARInvoice_DocType_RefNbr.Current = (ARInvoice)res;
                        if (pmDoc != null)
                            pmDocList.Add(pmDoc);
                    }

                    foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res in ARPayment_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr, doc.CustomerID))
                    {
                        Customer c = res;
                        if (c.Status == Customer.status.Inactive)
                        {
                            throw new PXSetPropertyException(Messages.CustomerIsInStatus,
                                new Customer.status.ListAttribute().ValueLabelDic[c.Status]);
                        }

                        ARPayment payment = res;
                        if ((doc.DocType == ARDocType.Payment || doc.DocType == ARDocType.VoidPayment || doc.DocType == ARDocType.Refund) && doc.Released == false)
                        {
                            SegregateBatch(je, doc.BranchID, doc.CuryID, ((ARPayment)res).DocDate, ((ARPayment)res).FinPeriodID, doc.DocDesc, (CurrencyInfo)res);

                            if (_IsIntegrityCheck == false && ((Customer)res).AutoApplyPayments == true && payment.DocType == ARDocType.Payment && payment.Released == false)
                            {
                                pe.Clear();
                                bool anyAdj = false;
                                if (PXTransactionScope.IsScoped)
                                {
                                    //It's required to select curyInfo as it may be not committed to the database yet.
                                    //So it cannot be selected through RowSelecting for the balance calculation if it is not in the database.
                                    //(as RowSelecting have it's own connection scope)
                                    pe.CurrencyInfo_CuryInfoID.Select(payment.CuryInfoID);
                                    foreach (ARAdjust adj in pe.Adjustments_Raw.Select(payment.DocType, payment.RefNbr, payment.AdjCntr))
                                    {
                                        if (!anyAdj)
                                        {
                                            anyAdj = true;
                                            pe.CurrencyInfo_CuryInfoID.View.Clear();
                                        }
                                        pe.CurrencyInfo_CuryInfoID.Select(adj.AdjdCuryInfoID);
                                    }
                                    pe.CurrencyInfo_CuryInfoID.Select(payment.CuryInfoID);
                                }
                                else
                                {
                                    anyAdj = (pe.Adjustments_Raw.View.SelectSingle() != null);
                                }
                                pe.Document.Current = payment;
                                if (!anyAdj)
                                {
                                    pe.LoadInvoicesProc(false);
                                }
                                pe.Save.Press();
                                doc = pe.Document.Current;
                                ReleaseDocProc(je, ref doc, new PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount>(pe.Document.Current, (CurrencyInfo)res, (Currency)res, (Customer)res, (CashAccount)res), -1);
                            }
                            else
                            {
                                ReleaseDocProc(je, ref doc, res, -1);
                            }

                            if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
                            {
                                je.Save.Press();
                            }

                            if (!je.BatchModule.Cache.IsDirty && string.IsNullOrEmpty(doc.BatchNbr))
                            {
                                doc.BatchNbr = je.BatchModule.Current.BatchNbr;
                            }

                            Dictionary<string, List<PXResult<ARAdjust>>> appsbyperiod = new Dictionary<string, List<PXResult<ARAdjust>>>();
                            Dictionary<string, DateTime?> datesbyperiod = new Dictionary<string, DateTime?>();

                            foreach (PXResult<ARAdjust> adjres in ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, doc.AdjCntr))
                            {
                                ARAdjust adj = (ARAdjust)adjres;
                                SetAdjgPeriodsFromLatestApplication(doc, adj);

                                List<PXResult<ARAdjust>> apps;
                                if (!appsbyperiod.TryGetValue(adj.AdjgFinPeriodID, out apps))
                                {
                                    appsbyperiod[adj.AdjgFinPeriodID] = apps = new List<PXResult<ARAdjust>>();
                                }
                                apps.Add(adjres);

                                DateTime? maxdate;
                                if (!datesbyperiod.TryGetValue(adj.AdjgFinPeriodID, out maxdate))
                                {
                                    datesbyperiod[adj.AdjgFinPeriodID] = maxdate = adj.AdjgDocDate;
                                }

                                if (DateTime.Compare((DateTime)adj.AdjgDocDate, (DateTime)maxdate) > 0)
                                {
                                    datesbyperiod[adj.AdjgFinPeriodID] = adj.AdjgDocDate;
                                }

                                if (doc.OpenDoc == false)
                                {
                                    //this is true for VoidCheck
                                    doc.OpenDoc = true;
                                    doc.CuryDocBal = doc.CuryOrigDocAmt;
                                    doc.DocBal = doc.OrigDocAmt;
                                }
                            }

                            Batch paymentBatch = je.BatchModule.Current;

                            int i = -2;
                            try
                            {
                                foreach (KeyValuePair<string, List<PXResult<ARAdjust>>> pair in appsbyperiod)
                                {
                                    JournalEntry.SegregateBatch(je, BatchModule.AR, doc.BranchID, doc.CuryID, datesbyperiod[pair.Key], pair.Key,
                                        doc.DocDesc, (CurrencyInfo)res, paymentBatch);

                                    ARAdjustsToRelease = new PXResultset<ARAdjust>();
                                    ARAdjustsToRelease.AddRange(pair.Value);

                                    //parameter "i" is irrelevant, it has been left for backward compatibility
                                    ReleaseDocProc(je, ref doc, res, i);

                                    i--;
                                }
                            }
                            finally
                            {
                                ARAdjustsToRelease = null;
                            }

                            //increment default for AdjNbr
                            doc.AdjCntr++;
                        }
                        else
                        {
                            if (doc.DocType != ARDocType.CashSale && doc.DocType != ARDocType.CashReturn)
                            {
                                SegregateBatch(je, doc.BranchID, doc.CuryID, payment.AdjDate, payment.AdjFinPeriodID, payment.DocDesc, (CurrencyInfo)res);
                            }
                            ReleaseDocProc(je, ref doc, res);
                        }
                        //ensure correct PXDBDefault behaviour on ARAdjust persisting
                        ARPayment_DocType_RefNbr.Current = (ARPayment)res;
                    }
                    if (doc.DocType == ARDocType.VoidPayment)
                    {
                        //create deposit rgol reverse batch
                        ARPayment voidPayment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<ARDocType.voidPayment>, And<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(je, doc.RefNbr);
                        ARPayment origPayment = PXSelect<ARPayment, Where<ARPayment.docType, Equal<ARDocType.payment>, And<ARPayment.refNbr, Equal<Required<ARRegister.refNbr>>>>>.Select(je, doc.RefNbr);
                        if (origPayment != null && origPayment.Deposited == true)
                        {
                            CADeposit deposit = PXSelect<CADeposit, Where<CADeposit.refNbr, Equal<Required<ARPayment.depositNbr>>>>.Select(je, origPayment.DepositNbr);
                            if (deposit != null)
                            {
                                CADepositDetail detail = PXSelect<CADepositDetail, Where<CADepositDetail.refNbr, Equal<Required<CADeposit.refNbr>>,
                                And<CADepositDetail.origRefNbr, Equal<Required<ARPayment.refNbr>>,
                                And<CADepositDetail.origDocType, Equal<ARDocType.payment>>>>>.Select(je, deposit.RefNbr, origPayment.RefNbr);
                                if (detail != null)
                                {
                                    decimal rgol = Math.Round((detail.OrigAmtSigned.Value - detail.TranAmt.Value), 3);
                                    if (rgol != Decimal.Zero)
                                    {
                                        if (deposit.CashAccountID == voidPayment.CashAccountID)
                                        {
                                            CashAccount depositCashacct = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(je, deposit.CashAccountID);
                                            GLTran rgol_tran = new GLTran();
                                            rgol_tran.DebitAmt = Decimal.Zero;
                                            rgol_tran.CreditAmt = Decimal.Zero;
                                            rgol_tran.AccountID = depositCashacct.AccountID;
                                            rgol_tran.SubID = depositCashacct.SubID;
                                            rgol_tran.BranchID = depositCashacct.BranchID;
                                            rgol_tran.TranDate = doc.DocDate;
                                            rgol_tran.FinPeriodID = doc.FinPeriodID;
                                            rgol_tran.TranPeriodID = doc.TranPeriodID;
                                            rgol_tran.TranType = CATranType.CATransferRGOL;
                                            rgol_tran.RefNbr = doc.RefNbr;
                                            rgol_tran.TranDesc = Messages.ReversingRGOLTanDescription;
                                            rgol_tran.Released = true;
                                            rgol_tran.CuryInfoID = doc.CuryInfoID;


                                            rgol_tran.DebitAmt += ((origPayment.DrCr == CADrCr.CACredit) == rgol > 0 ? Decimal.Zero : Math.Abs(rgol));
                                            rgol_tran.CreditAmt += ((origPayment.DrCr == CADrCr.CACredit) == rgol > 0 ? Math.Abs(rgol) : Decimal.Zero);

                                            Currency rgol_cury = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(je, deposit.CuryID);

                                            decimal rgolAmt = (decimal)(rgol_tran.DebitAmt - rgol_tran.CreditAmt);
                                            int sign = Math.Sign(rgolAmt);
                                            rgolAmt = Math.Abs(rgolAmt);

                                            if ((rgolAmt) != Decimal.Zero)
                                            {
                                                GLTran tran = (GLTran)je.Caches[typeof(GLTran)].CreateCopy(rgol_tran);
                                                tran.CuryDebitAmt = Decimal.Zero;
                                                tran.CuryCreditAmt = Decimal.Zero;
                                                if (doc.DocType == CATranType.CADeposit)
                                                {
                                                    tran.AccountID = (sign < 0) ? rgol_cury.RealLossAcctID : rgol_cury.RealGainAcctID;
                                                    tran.SubID = (sign < 0)
                                                        ? GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID, rgol_cury)
                                                        : GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury);
                                                }
                                                else
                                                {
                                                    tran.AccountID = (sign < 0) ? rgol_cury.RealGainAcctID : rgol_cury.RealLossAcctID;
                                                    tran.SubID = (sign < 0)
                                                        ? GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury)
                                                        : GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID, rgol_cury);
                                                }

                                                tran.DebitAmt = sign < 0 ? rgolAmt : Decimal.Zero;
                                                tran.CreditAmt = sign < 0 ? Decimal.Zero : rgolAmt;
                                                tran.TranType = CATranType.CATransferRGOL;
                                                tran.RefNbr = doc.RefNbr;
                                                tran.TranDesc = Messages.ReversingRGOLTanDescription;
                                                tran.TranDate = rgol_tran.TranDate;
                                                tran.FinPeriodID = rgol_tran.FinPeriodID;
                                                tran.TranPeriodID = rgol_tran.TranPeriodID;
                                                tran.Released = true;
                                                tran.CuryInfoID = origPayment.CuryInfoID;
                                                tran = je.GLTranModuleBatNbr.Insert(tran);

                                                rgol_tran.CuryDebitAmt = Decimal.Zero;
                                                rgol_tran.DebitAmt = (sign > 0) ? rgolAmt : Decimal.Zero;
                                                rgol_tran.CreditAmt = (sign > 0) ? Decimal.Zero : rgolAmt;
                                                je.GLTranModuleBatNbr.Insert(rgol_tran);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    doc.Released = true;
                    //when doc is loaded in ARInvoiceEntry, ARPaymentEntry it will set Selected = 0 and document will dissappear from 
                    //list of processing items.
                    doc.Selected = true;

                    UpdateARBalances(doc);

                    if (_IsIntegrityCheck == false)
                    {
                        if (je.GLTranModuleBatNbr.Cache.IsInsertedUpdatedDeleted)
                        {
                            je.Save.Press();
                        }

                        if (!je.BatchModule.Cache.IsDirty && string.IsNullOrEmpty(doc.BatchNbr))
                        {
                            doc.BatchNbr = ((Batch)je.BatchModule.Current).BatchNbr;
                        }
                    }

                    #region Auto Commit/Post document to avalara.

                    ARInvoice arInvoice = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>, And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>.Select(this, doc.DocType, doc.RefNbr);
                    if (arInvoice != null && AvalaraMaint.IsExternalTax(this, arInvoice.TaxZoneID) && doc.IsTaxValid == true && arInvoice.InstallmentNbr == null)
                    {
                        TXAvalaraSetup avalaraSetup = PXSelect<TXAvalaraSetup>.Select(this);
                        if (avalaraSetup != null)
                        {
                            TaxSvc service = new TaxSvc();
                            AvalaraMaint.SetupService(this, service);

                            CommitTaxRequest request = new CommitTaxRequest();
                            request.CompanyCode = AvalaraMaint.CompanyCodeFromBranch(this, doc.BranchID);
                            request.DocCode = string.Format("AR.{0}.{1}", doc.DocType, doc.RefNbr);

                            if (doc.DocType == ARDocType.CreditMemo)
                                request.DocType = DocumentType.ReturnInvoice;
                            else
                                request.DocType = DocumentType.SalesInvoice;


                            CommitTaxResult result = service.CommitTax(request);
                            if (result.ResultCode == SeverityLevel.Success)
                            {
                                doc.IsTaxPosted = true;
                            }
                            else
                            {
                                //Avalara retuned an error - The given document is already marked as posted on the avalara side.
                                if (result.ResultCode == SeverityLevel.Error && result.Messages.Count == 1 &&
                                    result.Messages[0].Details == "Expected Posted")
                                {
                                    //ignore this error - everything is cool
                                }
                                else
                                {
                                    //show as warning.
                                    StringBuilder sb = new StringBuilder();
                                    foreach (Avalara.AvaTax.Adapter.Message msg in result.Messages)
                                    {
                                        sb.AppendLine(msg.Name + ": " + msg.Details);
                                    }

                                    if (sb.Length > 0)
                                    {
                                        ardoc.WarningMessage = PXMessages.LocalizeFormatNoPrefixNLA(Messages.PostingToAvalaraFailed, sb.ToString());
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    doc = (ARRegister)ARDocument.Cache.Update(doc);

                    PXCache<ARRegister>.RestoreCopy(ardoc, doc);

                    PXTimeStampScope.DuplicatePersisted(ARDocument.Cache, doc, typeof(ARInvoice));
                    PXTimeStampScope.DuplicatePersisted(ARDocument.Cache, doc, typeof(ARPayment));

                    if (doc.DocType == ARDocType.CreditMemo)
                    {
                        if (Released)
                        {
                            ARPayment_DocType_RefNbr.Cache.SetStatus(ARPayment_DocType_RefNbr.Current, PXEntryStatus.Notchanged);
                        }
                        else
                        {
                            ARPayment crmemo = (ARPayment)ARPayment_DocType_RefNbr.Cache.Extend<ARRegister>(doc);
                            crmemo.CreatedByID = doc.CreatedByID;
                            crmemo.CreatedByScreenID = doc.CreatedByScreenID;
                            crmemo.CreatedDateTime = doc.CreatedDateTime;
                            crmemo.CashAccountID = null;
                            crmemo.AdjDate = crmemo.DocDate;
                            crmemo.AdjTranPeriodID = crmemo.TranPeriodID;
                            crmemo.AdjFinPeriodID = crmemo.FinPeriodID;
                            ARPayment_DocType_RefNbr.Cache.Update(crmemo);
                            ARDocument.Cache.SetStatus(doc, PXEntryStatus.Notchanged);
                        }
                    }
                    else
                    {
                        if (ARDocument.Cache.ObjectsEqual(doc, ARPayment_DocType_RefNbr.Current))
                        {
                            ARPayment_DocType_RefNbr.Cache.SetStatus(ARPayment_DocType_RefNbr.Current, PXEntryStatus.Notchanged);
                        }
                    }

                    foreach (ARPayment item in ARPayment_DocType_RefNbr.Cache.Updated)
                    {
                        PXTimeStampScope.DuplicatePersisted(ARPayment_DocType_RefNbr.Cache, item, typeof(ARRegister));
                    }

                    List<ProcessInfo<Batch>> batchList;
                    PM.RegisterRelease.ReleaseWithoutPost(pmDocList, false, out batchList);
                    foreach (ProcessInfo<Batch> processInfo in batchList)
                    {
                        pmBatchList.AddRange(processInfo.Batches);
                    }

                    this.Actions.PressSave();

                    // Finding some known data inconsistency problems,
                    // if any, the process will be stopped.
                    // 
                    if (_IsIntegrityCheck != true)
                    {
                        // We need this condition to prevent applications verification,
                        // until the ARPayment part will not be created.
                        //
                        bool isUnreleasedCreditMemo = doc.DocType == ARDocType.CreditMemo && !Released;

                        new DataIntegrityValidator<ARRegister>(
                            je, ARDocument.Cache, doc, BatchModule.AR, doc.CustomerID, doc.Released, arsetup.DataInconsistencyHandlingMode)
                            .CheckTransactionsExistenceForUnreleasedDocument()
                            .CheckTransactionsExistenceForReleasedDocument()
                            .CheckBatchAndTransactionsSumsForDocument()
                            .CheckApplicationsReleasedForDocument<ARAdjust, ARAdjust.adjgDocType, ARAdjust.adjgRefNbr, ARAdjust.released>(disableCheck: isUnreleasedCreditMemo)
                            .CheckDocumentHasNonNegativeBalance()
                            .CheckDocumentTotalsConformToCurrencyPrecision()
                            .Commit();
                    }

                    if (onreleasecomplete != null)
                    {
                        onreleasecomplete(ardoc);
                    }

                    ts.Complete(this);
                }
            }
            PXCache<ARRegister>.RestoreCopy(ardoc, doc);

            if (ardoc is ARInvoice)
            {
                PXTimeStampScope.DuplicatePersisted(Caches[typeof(ARInvoice)], ardoc, typeof(ARRegister));
            }
            if (ardoc is ARPayment)
            {
                PXTimeStampScope.DuplicatePersisted(Caches[typeof(ARPayment)], ardoc, typeof(ARRegister));
            }

            return ret;
        }

        public override void Persist()
        {
            using (PXTransactionScope ts = new PXTransactionScope())
            {
                ARPayment_DocType_RefNbr.Cache.Persist(PXDBOperation.Insert);
                ARPayment_DocType_RefNbr.Cache.Persist(PXDBOperation.Update);

                ARDocument.Cache.Persist(PXDBOperation.Update);
                ARTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Insert);
                ARTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Update);
                ARTaxTran_TranType_RefNbr.Cache.Persist(PXDBOperation.Update);
                SVATConversionHistory.Cache.Persist(PXDBOperation.Insert);
                SVATConversionHistory.Cache.Persist(PXDBOperation.Update);
                intranselect.Cache.Persist(PXDBOperation.Update);
                ARPaymentChargeTran_DocType_RefNbr.Cache.Persist(PXDBOperation.Update);

                ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Insert);
                ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Update);
                ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persist(PXDBOperation.Delete);

                ARDoc_SalesPerTrans.Cache.Persist(PXDBOperation.Insert);
                ARDoc_SalesPerTrans.Cache.Persist(PXDBOperation.Update);

                Caches[typeof(ARHist)].Persist(PXDBOperation.Insert);

                Caches[typeof(CuryARHist)].Persist(PXDBOperation.Insert);

                Caches[typeof(ARBalances)].Persist(PXDBOperation.Insert);

                Caches[typeof(CADailySummary)].Persist(PXDBOperation.Insert);

                this.Caches[typeof(PMCommitment)].Persist(PXDBOperation.Insert);
                this.Caches[typeof(PMCommitment)].Persist(PXDBOperation.Update);
                this.Caches[typeof(PMCommitment)].Persist(PXDBOperation.Delete);
                this.Caches[typeof(PMHistoryAccum)].Persist(PXDBOperation.Insert);
                this.Caches[typeof(PMProjectStatusAccum)].Persist(PXDBOperation.Insert);

                ts.Complete(this);
            }

            ARPayment_DocType_RefNbr.Cache.Persisted(false);
            ARDocument.Cache.Persisted(false);
            ARTran_TranType_RefNbr.Cache.Persisted(false);
            ARTaxTran_TranType_RefNbr.Cache.Persisted(false);
            intranselect.Cache.Persisted(false);
            ARAdjust_AdjgDocType_RefNbr_CustomerID.Cache.Persisted(false);

            Caches[typeof(ARHist)].Persisted(false);

            Caches[typeof(CuryARHist)].Persisted(false);

            Caches[typeof(ARBalances)].Persisted(false);

            ARDoc_SalesPerTrans.Cache.Persisted(false);

            Caches[typeof(CADailySummary)].Persisted(false);
        }

        protected bool _IsIntegrityCheck = false;

        protected virtual int SortCustDocs(PXResult<ARRegister> a, PXResult<ARRegister> b)
        {
            return ((IComparable)((ARRegister)a).SortOrder).CompareTo(((ARRegister)b).SortOrder);
        }

        public virtual void IntegrityCheckProc(Customer cust, string startPeriod)
        {
            _IsIntegrityCheck = true;
            JournalEntry je = PXGraph.CreateInstance<JournalEntry>();
            je.SetOffline();

            Caches[typeof(Customer)].Current = cust;

            using (new PXConnectionScope())
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    string minPeriod = "190001";

                    ARHistory maxHist = (ARHistory)PXSelectGroupBy<ARHistory, Where<ARHistory.customerID, Equal<Current<Customer.bAccountID>>, And<ARHistory.detDeleted, Equal<True>>>, Aggregate<Max<ARHistory.finPeriodID>>>.Select(this);

                    if (maxHist != null && maxHist.FinPeriodID != null)
                    {
                        minPeriod = FinPeriodIDAttribute.PeriodPlusPeriod(this, maxHist.FinPeriodID, 1);
                    }

                    if (string.IsNullOrEmpty(startPeriod) == false && string.Compare(startPeriod, minPeriod) > 0)
                    {
                        minPeriod = startPeriod;
                    }

                    foreach (CuryARHist old_hist in PXSelectReadonly<CuryARHist, Where<CuryARHist.customerID, Equal<Required<CuryARHist.customerID>>, And<CuryARHist.finPeriodID, GreaterEqual<Required<CuryARHist.finPeriodID>>>>>.Select(this, cust.BAccountID, minPeriod))
                    {
                        CuryARHist hist = new CuryARHist();
                        hist.BranchID = old_hist.BranchID;
                        hist.AccountID = old_hist.AccountID;
                        hist.SubID = old_hist.SubID;
                        hist.CustomerID = old_hist.CustomerID;
                        hist.FinPeriodID = old_hist.FinPeriodID;
                        hist.CuryID = old_hist.CuryID;

                        hist = (CuryARHist)Caches[typeof(CuryARHist)].Insert(hist);

                        hist.FinPtdRevalued += old_hist.FinPtdRevalued;

                        ARHist basehist = new ARHist();
                        basehist.BranchID = old_hist.BranchID;
                        basehist.AccountID = old_hist.AccountID;
                        basehist.SubID = old_hist.SubID;
                        basehist.CustomerID = old_hist.CustomerID;
                        basehist.FinPeriodID = old_hist.FinPeriodID;

                        basehist = (ARHist)Caches[typeof(ARHist)].Insert(basehist);

                        basehist.FinPtdRevalued += old_hist.FinPtdRevalued;
                    }

                    PXDatabase.Delete<ARHistory>(
                        new PXDataFieldRestrict("CustomerID", PXDbType.Int, 4, cust.BAccountID, PXComp.EQ),
                        new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
                        );

                    PXDatabase.Delete<CuryARHistory>(
                        new PXDataFieldRestrict("CustomerID", PXDbType.Int, 4, cust.BAccountID, PXComp.EQ),
                        new PXDataFieldRestrict("FinPeriodID", PXDbType.Char, 6, minPeriod, PXComp.GE)
                        );

                    PXDatabase.Update<ARBalances>(
                        new PXDataFieldAssign<ARBalances.totalOpenOrders>(0m),
                        new PXDataFieldAssign<ARBalances.unreleasedBal>(0m),
                        new PXDataFieldAssign("CurrentBal", 0m),
                        new PXDataFieldAssign("OldInvoiceDate", null),
                        new PXDataFieldRestrict("CustomerID", PXDbType.Int, 4, cust.BAccountID, PXComp.EQ)
                    );

                    PXRowInserting ARHist_RowInserting = delegate (PXCache sender, PXRowInsertingEventArgs e)
                    {
                        if (string.Compare(((ARHist)e.Row).FinPeriodID, minPeriod) < 0)
                        {
                            e.Cancel = true;
                        }
                    };

                    PXRowInserting CuryARHist_RowInserting = delegate (PXCache sender, PXRowInsertingEventArgs e)
                    {
                        if (string.Compare(((CuryARHist)e.Row).FinPeriodID, minPeriod) < 0)
                        {
                            e.Cancel = true;
                        }
                    };

                    this.RowInserting.AddHandler<ARHist>(ARHist_RowInserting);
                    this.RowInserting.AddHandler<CuryARHist>(CuryARHist_RowInserting);

                    PXResultset<ARRegister> custdocs = PXSelect<ARRegister, Where<ARRegister.customerID, Equal<Current<Customer.bAccountID>>, And<ARRegister.released, Equal<True>, And<Where<ARRegister.finPeriodID, GreaterEqual<Required<ARRegister.finPeriodID>>, Or<ARRegister.closedFinPeriodID, GreaterEqual<Required<ARRegister.finPeriodID>>>>>>>>.Select(this, minPeriod, minPeriod);
                    PXResultset<ARRegister> other = PXSelectJoinGroupBy<ARRegister,
                        InnerJoin<ARAdjust, On2<
                            Where<ARAdjust.adjgDocType, Equal<ARRegister.docType>,
                                Or<ARAdjust.adjgDocType, Equal<ARDocType.payment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>,
                                Or<ARAdjust.adjgDocType, Equal<ARDocType.prepayment>, And<ARRegister.docType, Equal<ARDocType.voidPayment>>>>>>,
                            And<ARAdjust.adjgRefNbr, Equal<ARRegister.refNbr>>>,
                        InnerJoin<Standalone.ARRegister2, On<Standalone.ARRegister2.docType, Equal<ARAdjust.adjdDocType>,
                            And<Standalone.ARRegister2.refNbr, Equal<ARAdjust.adjdRefNbr>>>>>,
                        Where<ARRegister.customerID, Equal<Current<Customer.bAccountID>>,
                            And2<Where<Standalone.ARRegister2.closedFinPeriodID, GreaterEqual<Required<Standalone.ARRegister2.closedFinPeriodID>>,
                                Or<ARAdjust.adjdFinPeriodID, GreaterEqual<Required<ARAdjust.adjdFinPeriodID>>>>,
                            And<ARAdjust.released, Equal<True>,
                            And<ARRegister.finPeriodID, Less<Required<ARRegister.finPeriodID>>,
                            And<ARRegister.released, Equal<True>,
                            And<Where<ARRegister.closedFinPeriodID, Less<Required<ARRegister.closedFinPeriodID>>, Or<ARRegister.closedFinPeriodID, IsNull>>>>>>>>,
                        Aggregate<GroupBy<ARRegister.docType,
                            GroupBy<ARRegister.refNbr,
                            GroupBy<ARRegister.createdByID,
                            GroupBy<ARRegister.lastModifiedByID,
                            GroupBy<ARRegister.released,
                            GroupBy<ARRegister.openDoc,
                            GroupBy<ARRegister.hold,
                            GroupBy<ARRegister.scheduled,
                            GroupBy<ARRegister.isTaxValid,
                            GroupBy<ARRegister.isTaxSaved,
                            GroupBy<ARRegister.isTaxPosted,
                            GroupBy<ARRegister.voided>>>>>>>>>>>>>>.Select(this, minPeriod, minPeriod, minPeriod, minPeriod);

                    custdocs.AddRange(other);
                    custdocs.Sort(SortCustDocs);

                    foreach (ARRegister custdoc in custdocs)
                    {
                        je.Clear();

                        ARRegister doc = custdoc;

                        //mark as updated so that doc will not expire from cache and update with Released = 1 will not override balances/amount in document
                        ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

                        doc.Released = false;

                        foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res in ARInvoice_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr))
                        {
                            //must check for CM application in different period
                            if ((bool)doc.Released == false)
                            {
                                SegregateBatch(je, doc.BranchID, doc.CuryID, doc.DocDate, doc.FinPeriodID, doc.DocDesc, (CurrencyInfo)res);
                            }
                            if (doc.DocType == ARDocType.SmallCreditWO)
                            {
                                doc.AdjCntr = -1;
                                ReleaseSmallCreditProc(je, ref doc, res);
                            }
                            else
                            {
                                PM.PMRegister pmDoc;
                                ReleaseDocProc(je, ref doc, res, out pmDoc);
                            }
                            doc.Released = true;
                        }

                        foreach (PXResult<ARPayment, CurrencyInfo, Currency, Customer, CashAccount> res in ARPayment_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr, doc.CustomerID))
                        {
                            ARPayment payment = (ARPayment)res;
                            SegregateBatch(je, doc.BranchID, doc.CuryID, payment.AdjDate, payment.AdjFinPeriodID, payment.DocDesc, (CurrencyInfo)res);

                            int OrigAdjCntr = (int)doc.AdjCntr;
                            doc.AdjCntr = 0;

                            while (doc.AdjCntr < OrigAdjCntr)
                            {
                                ReleaseDocProc(je, ref doc, res);
                                doc.Released = true;
                            }
                            ARAdjust reversal = ARAdjust_AdjgDocType_RefNbr_CustomerID.Select(doc.DocType, doc.RefNbr, OrigAdjCntr);
                            if (reversal != null)
                            {
                                doc.OpenDoc = true;
                            }
                        }

                        ARDocument.Cache.Update(doc);
                    }

                    foreach (ARRegister custdoc in custdocs)
                    {
                        je.Clear();

                        ARRegister doc = custdoc;
                        ARDocument.Cache.SetStatus(doc, PXEntryStatus.Updated);

                        foreach (PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res in ARInvoice_DocType_RefNbr.Select((object)doc.DocType, doc.RefNbr))
                        {
                            ARInvoice invoice = res;

                            SegregateBatch(je, doc.BranchID, doc.CuryID, doc.DocDate, doc.FinPeriodID, doc.DocDesc, (CurrencyInfo)res);

                            var adjustments = PXSelectJoin<ARAdjust,
                                InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<ARAdjust.adjdCuryInfoID>>,
                                InnerJoin<Currency, On<Currency.curyID, Equal<CurrencyInfo.curyID>>,
                                LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<ARAdjust.adjdDocType>, And<ARInvoice.refNbr, Equal<ARAdjust.adjdRefNbr>>>,
                                LeftJoin<ARPayment, On<ARPayment.docType, Equal<ARAdjust.adjgDocType>, And<ARPayment.refNbr, Equal<ARAdjust.adjgRefNbr>>>>>>>,
                                Where<ARAdjust.adjdDocType, Equal<Required<ARAdjust.adjdDocType>>,
                                    And<ARAdjust.adjdRefNbr, Equal<Required<ARAdjust.adjdRefNbr>>,
                                    And<ARAdjust.adjdCustomerID, Equal<Required<ARAdjust.adjdCustomerID>>,
                                    And<ARAdjust.adjdCustomerID, NotEqual<ARAdjust.customerID>>>>>>.Select(this, invoice.DocType, invoice.RefNbr, invoice.CustomerID);

                            ProcessAdjustmentsOnlyAdjusted(je, adjustments);
                        }

                        ARDocument.Cache.Update(doc);
                    }

                    Caches[typeof(ARBalances)].Clear();

                    foreach (ARRegister ardoc in ARDocument.Cache.Updated)
                    {
                        ARDocument.Cache.PersistUpdated(ardoc);
                    }

                    foreach (SOOrder order in PXSelectReadonly<SOOrder, Where<SOOrder.customerID, Equal<Required<SOOrder.customerID>>, And<SOOrder.completed, Equal<False>, And<SOOrder.cancelled, Equal<False>>>>>.Select(this, cust.BAccountID))
                    {
                        ARReleaseProcess.UpdateARBalances(this, order, order.UnbilledOrderTotal, order.OpenOrderTotal);
                    }

                    foreach (ARRegister ardoc in PXSelectReadonly<ARRegister, Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>, And<ARRegister.released, Equal<True>, And<ARRegister.openDoc, Equal<True>>>>>.Select(this, cust.BAccountID))
                    {
                        ARReleaseProcess.UpdateARBalances(this, ardoc, ardoc.DocBal);
                        UpdateARBalancesDates(ardoc);
                    }

                    foreach (ARRegister ardoc in PXSelectReadonly<ARRegister, Where<ARRegister.customerID, Equal<Required<ARRegister.customerID>>, And<ARRegister.released, Equal<False>, And<ARRegister.hold, Equal<False>, And<ARRegister.voided, Equal<False>, And<ARRegister.scheduled, Equal<False>>>>>>>.Select(this, cust.BAccountID))
                    {
                        ARReleaseProcess.UpdateARBalances(this, ardoc, ardoc.OrigDocAmt);
                    }

                    foreach (ARInvoice ardoc in PXSelectReadonly<ARInvoice, Where<ARInvoice.customerID, Equal<Required<ARInvoice.customerID>>, And<ARInvoice.creditHold, Equal<True>, And<ARInvoice.released, Equal<False>, And<ARInvoice.hold, Equal<False>, And<ARInvoice.voided, Equal<False>, And<ARInvoice.scheduled, Equal<False>>>>>>>>.Select(this, cust.BAccountID))
                    {
                        ardoc.CreditHold = false;
                        ARReleaseProcess.UpdateARBalances(this, ardoc, -ardoc.OrigDocAmt);
                    }

                    this.RowInserting.RemoveHandler<ARHist>(ARHist_RowInserting);
                    this.RowInserting.RemoveHandler<CuryARHist>(CuryARHist_RowInserting);

                    Caches[typeof(ARHist)].Persist(PXDBOperation.Insert);

                    Caches[typeof(CuryARHist)].Persist(PXDBOperation.Insert);

                    Caches[typeof(ARBalances)].Persist(PXDBOperation.Insert);

                    ts.Complete(this);
                }

                ARDocument.Cache.Persisted(false);

                Caches[typeof(ARHist)].Persisted(false);

                Caches[typeof(CuryARHist)].Persisted(false);

                Caches[typeof(ARBalances)].Persisted(false);
            }
        }

        protected static void Copy(ARSalesPerTran aDest, ARAdjust aAdj)
        {
            aDest.AdjdDocType = aAdj.AdjdDocType;
            aDest.AdjdRefNbr = aAdj.AdjdRefNbr;
            aDest.AdjNbr = aAdj.AdjNbr;
            aDest.BranchID = aAdj.AdjdBranchID;
            aDest.Released = true;
        }

        protected static void Copy(ARSalesPerTran aDest, ARRegister aReg)
        {
            aDest.DocType = aReg.DocType;
            aDest.RefNbr = aReg.RefNbr;
        }

        protected static void CopyShare(ARSalesPerTran aDest, ARSalesPerTran aSrc, decimal aRatio, short aPrecision)
        {
            aDest.SalespersonID = aSrc.SalespersonID;
            aDest.CuryInfoID = aSrc.CuryInfoID; //We will use currency Info of the orifginal invoice for the commission calculations
            aDest.CommnPct = aSrc.CommnPct;
            aDest.CuryCommnblAmt = Math.Round((decimal)(aRatio * aSrc.CuryCommnblAmt), aPrecision);
            aDest.CuryCommnAmt = Math.Round((decimal)(aRatio * aSrc.CuryCommnAmt), aPrecision);
        }
    }
}

namespace PX.Objects.AR.Overrides.ARDocumentRelease
{
    public interface IBaseARHist
    {
        Boolean? DetDeleted
        {
            get;
            set;
        }

        Boolean? FinFlag
        {
            get;
            set;
        }
        Decimal? PtdCrAdjustments
        {
            get;
            set;
        }
        Decimal? PtdDrAdjustments
        {
            get;
            set;
        }
        Decimal? PtdSales
        {
            get;
            set;
        }
        Decimal? PtdPayments
        {
            get;
            set;
        }
        Decimal? PtdDiscounts
        {
            get;
            set;
        }
        Decimal? YtdBalance
        {
            get;
            set;
        }
        Decimal? BegBalance
        {
            get;
            set;
        }
        Decimal? PtdCOGS
        {
            get;
            set;
        }
        Decimal? PtdRGOL
        {
            get;
            set;
        }
        Decimal? PtdFinCharges
        {
            get;
            set;
        }
        Decimal? PtdDeposits
        {
            get;
            set;
        }
        Decimal? YtdDeposits
        {
            get;
            set;
        }
        Decimal? PtdItemDiscounts
        {
            get;
            set;
        }
    }

    public interface ICuryARHist
    {
        Decimal? CuryPtdCrAdjustments
        {
            get;
            set;
        }
        Decimal? CuryPtdDrAdjustments
        {
            get;
            set;
        }
        Decimal? CuryPtdSales
        {
            get;
            set;
        }
        Decimal? CuryPtdPayments
        {
            get;
            set;
        }
        Decimal? CuryPtdDiscounts
        {
            get;
            set;
        }
        Decimal? CuryPtdFinCharges
        {
            get;
            set;
        }
        Decimal? CuryYtdBalance
        {
            get;
            set;
        }
        Decimal? CuryBegBalance
        {
            get;
            set;
        }
        Decimal? CuryPtdDeposits
        {
            get;
            set;
        }
        Decimal? CuryYtdDeposits
        {
            get;
            set;
        }
    }

    [PXAccumulator(new Type[] {
                typeof(CuryARHistory.finYtdBalance),
                typeof(CuryARHistory.tranYtdBalance),
                typeof(CuryARHistory.curyFinYtdBalance),
                typeof(CuryARHistory.curyTranYtdBalance),
                typeof(CuryARHistory.finYtdBalance),
                typeof(CuryARHistory.tranYtdBalance),
                typeof(CuryARHistory.curyFinYtdBalance),
                typeof(CuryARHistory.curyTranYtdBalance),
                typeof(CuryARHistory.finYtdDeposits),
                typeof(CuryARHistory.tranYtdDeposits),
                typeof(CuryARHistory.curyFinYtdDeposits),
                typeof(CuryARHistory.curyTranYtdDeposits)
                },
                    new Type[] {
                typeof(CuryARHistory.finBegBalance),
                typeof(CuryARHistory.tranBegBalance),
                typeof(CuryARHistory.curyFinBegBalance),
                typeof(CuryARHistory.curyTranBegBalance),
                typeof(CuryARHistory.finYtdBalance),
                typeof(CuryARHistory.tranYtdBalance),
                typeof(CuryARHistory.curyFinYtdBalance),
                typeof(CuryARHistory.curyTranYtdBalance),
                typeof(CuryARHistory.finYtdDeposits),
                typeof(CuryARHistory.tranYtdDeposits),
                typeof(CuryARHistory.curyFinYtdDeposits),
                typeof(CuryARHistory.curyTranYtdDeposits)
                }
            )]
    [Serializable]
    [PXHidden]
    public partial class CuryARHist : CuryARHistory, ICuryARHist, IBaseARHist
    {
        #region BranchID
        public new abstract class branchID : PX.Data.IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        public override Int32? BranchID
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
        #region AccountID
        public new abstract class accountID : PX.Data.IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? AccountID
        {
            get
            {
                return this._AccountID;
            }
            set
            {
                this._AccountID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? SubID
        {
            get
            {
                return this._SubID;
            }
            set
            {
                this._SubID = value;
            }
        }
        #endregion
        #region CustomerID
        public new abstract class customerID : PX.Data.IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        public override Int32? CustomerID
        {
            get
            {
                return this._CustomerID;
            }
            set
            {
                this._CustomerID = value;
            }
        }
        #endregion
        #region CuryID
        public new abstract class curyID : PX.Data.IBqlField
        {
        }
        [PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
        [PXDefault()]
        public override String CuryID
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
        #region FinPeriodID
        public new abstract class finPeriodID : PX.Data.IBqlField
        {
        }
        [PXDBString(6, IsKey = true, IsFixed = true)]
        [PXDefault()]
        public override String FinPeriodID
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
    }

    [PXAccumulator(new Type[] {
                typeof(ARHistory.finYtdBalance),
                typeof(ARHistory.tranYtdBalance),
                typeof(ARHistory.finYtdBalance),
                typeof(ARHistory.tranYtdBalance),
                typeof(ARHistory.finYtdDeposits),
                typeof(ARHistory.tranYtdDeposits)
                },
                    new Type[] {
                typeof(ARHistory.finBegBalance),
                typeof(ARHistory.tranBegBalance),
                typeof(ARHistory.finYtdBalance),
                typeof(ARHistory.tranYtdBalance),
                typeof(ARHistory.finYtdDeposits),
                typeof(ARHistory.tranYtdDeposits)
                }
            )]
    [Serializable]
    [PXHidden]
    public partial class ARHist : ARHistory, IBaseARHist
    {
        #region BranchID
        public new abstract class branchID : IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        public override int? BranchID
        {
            get;
            set;
        }
        #endregion
        #region AccountID
        public new abstract class accountID : IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        public override int? AccountID
        {
            get;
            set;
        }
        #endregion
        #region SubID
        public new abstract class subID : IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        public override int? SubID
        {
            get;
            set;
        }
        #endregion
        #region CustomerID
        public new abstract class customerID : IBqlField
        {
        }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        public override int? CustomerID
        {
            get;
            set;
        }
        #endregion
        #region FinPeriodID
        public new abstract class finPeriodID : IBqlField
        {
        }
        [PXDBString(6, IsKey = true, IsFixed = true)]
        [PXDefault]
        public override string FinPeriodID
        {
            get;
            set;
        }
        #endregion
    }

    public class ARBalAccumAttribute : PXAccumulatorAttribute
    {
        public ARBalAccumAttribute()
        {
            base._SingleRecord = true;
        }
        protected override bool PrepareInsert(PXCache sender, object row, PXAccumulatorCollection columns)
        {
            if (!base.PrepareInsert(sender, row, columns))
            {
                return false;
            }

            ARBalances bal = (ARBalances)row;

            columns.Update<ARBalances.lastInvoiceDate>(bal.LastInvoiceDate, PXDataFieldAssign.AssignBehavior.Maximize);
            columns.Update<ARBalances.numberInvoicePaid>(bal.NumberInvoicePaid, PXDataFieldAssign.AssignBehavior.Summarize);
            if (bal.DatesUpdated == true)
            {
                columns.Update<ARBalances.oldInvoiceDate>(bal.OldInvoiceDate, PXDataFieldAssign.AssignBehavior.Replace);
                columns.Restrict<ARBalances.Tstamp>(PXComp.LE, bal.tstamp ?? sender.Graph.TimeStamp);
            }
            else
            {
                columns.Update<ARBalances.oldInvoiceDate>(bal.OldInvoiceDate, PXDataFieldAssign.AssignBehavior.Minimize);
            }
            columns.Update<ARBalances.paidInvoiceDays>(bal.PaidInvoiceDays, PXDataFieldAssign.AssignBehavior.Summarize);
            columns.Update<ARBalances.lastModifiedByID>(bal.LastModifiedByID, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ARBalances.lastModifiedByScreenID>(bal.LastModifiedByScreenID, PXDataFieldAssign.AssignBehavior.Replace);
            columns.Update<ARBalances.lastModifiedDateTime>(bal.LastModifiedDateTime, PXDataFieldAssign.AssignBehavior.Replace);

            return bal.LastInvoiceDate != null
                || bal.NumberInvoicePaid != null
                || bal.DatesUpdated == true
                || bal.OldInvoiceDate != null
                || bal.PaidInvoiceDays != null
                || bal.CuryID != null
                || bal.CurrentBal != 0m
                || bal.UnreleasedBal != 0m
                || bal.TotalOpenOrders != 0
                || bal.TotalPrepayments != 0
                || bal.TotalQuotations != 0
                || bal.TotalShipped != 0;
        }
    }
}