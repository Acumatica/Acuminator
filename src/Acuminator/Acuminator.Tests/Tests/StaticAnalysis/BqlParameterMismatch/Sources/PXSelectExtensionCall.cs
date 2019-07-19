using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	[PXHidden]
	public class Document : PXMappedCacheExtension
	{
		#region BranchID
		/// <exclude />
		public abstract class branchID : IBqlField
		{
		}

		public virtual string BranchID { get; set; }
		#endregion
		#region HeaderDocDate
		/// <exclude />
		public abstract class headerDocDate : IBqlField
		{
		}

		public virtual DateTime? HeaderDocDate { get; set; }
		#endregion
		#region HeaderTranPeriodID
		/// <exclude />
		public abstract class headerTranPeriodID : IBqlField
		{
		}

		public virtual string HeaderTranPeriodID { get; set; }
		#endregion
	}

	public abstract class DocumentWithLinesGraphExtension<TGraph, TDocument, TDocumentMapping> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	where TDocument : Document, new()
	where TDocumentMapping : IBqlMapping
	{
		public PXSelectExtension<TDocument> Documents;

		public TDocument GetDocument(int? docID)
		{
			return Documents.Select(docID)
							.RowCast<TDocument>()
							.FirstOrDefault();
		}
	}
}
