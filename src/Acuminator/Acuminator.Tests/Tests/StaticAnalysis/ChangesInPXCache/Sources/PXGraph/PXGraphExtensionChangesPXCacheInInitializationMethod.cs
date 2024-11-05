using PX.Data;
using PX.Objects.AP;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization.Sources
{
	public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
	{
		public override void Initialize()
		{
		}
	}

	public class APInvoiceEntryExtDerived : APInvoiceEntryExt
	{
		public override void Initialize()
		{
			int count = Base.Document.Select().Count;

			if (count > 0)
			{
				Base.Document.Cache.Insert(Base.Document.Current);
				Base.Document.Cache.Update(Base.Document.Current);
				Base.Document.Cache.Delete(Base.Document.Current);

				Base.Document.Insert(Base.Document.Current);
				Base.Document.Update(Base.Document.Current);
				Base.Document.Delete(Base.Document.Current);
			}
		}
	}
}
