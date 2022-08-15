using PX.Data;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Acuminator.Tests.Sources
{
	public class ProcessingGraph : PXGraph<ProcessingGraph>
	{
		[PXHidden]
		public class SomeDAC : IBqlTable
		{
		}

		public PXAction<SomeDAC> DirectReassignmentAction;

		[PXUIField(DisplayName = "Direct Reassignment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable directReassignmentAction(PXAdapter adapter)
		{
			adapter = NewAdapter();
			PXLongOperation.StartOperation(this, () => adapter.Get());                  // No diagnostic
			return adapter.Get();
		}

		public PXAction<SomeDAC> SomeAction;

		[PXUIField(DisplayName = "Some Action", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable someAction(PXAdapter adapter)
		{
			TestAdapterReassignedAlwaysBeforeCapture(adapter);                          // No diagnosic
			TestAdapterReassignedNullAlwaysBeforeCapture(adapter);                      // No diagnosic
			TestAdapterReassignedViaOutAlwaysBeforeCapture(adapter);                    // No diagnosic
			TestAdapterReassignedAndUsedLater(adapter);									// No diagnosic
			TestAdapterReassignedWithLocalFunction(adapter);							// No diagnosic

			TestAdapterNotReassignedWithLocalFunction(adapter);                         // Show diagnosic
			TestAdapterNotReassignedWithLocalFunctionRedefineParameter(adapter);        // Show diagnosic
			TestAdapterNotReassignedWithLambdaNotInvoked(adapter);						// Show diagnosic
			TestAdapterReassignedConditionallyBeforeCapture(adapter, true);             // Show diagnosic
			TestAdapterReassignedInOtherClauseThanCapture(adapter, true);				// Show diagnosic

			return adapter.Get();
		}


		private void TestAdapterReassignedAlwaysBeforeCapture(PXAdapter adapter)
		{
			adapter = new PXAdapter(
				new PXView(this, isReadOnly: true, PXSelect<SomeDAC>.GetCommand()));
			PXLongOperation.StartOperation(this, () => adapter.Get());					// This reassignment happens always and should not be reported
		}

		private void TestAdapterReassignedNullAlwaysBeforeCapture(PXAdapter adapter)
		{
			adapter = null;
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
		}

		private void TestAdapterReassignedViaOutAlwaysBeforeCapture(PXAdapter adapter)
		{
			Reassign(out adapter);
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
		}

		private void TestAdapterReassignedAndUsedLater(PXAdapter adapter)
		{
			adapter = NewAdapter();                                     // This reassignment happens always and should not be reported
			RunAdapter(adapter);
		}

		private void TestAdapterReassignedWithLocalFunction(PXAdapter adapter)
		{
			void Local()
			{
				adapter = NewAdapter();
			}

			Local();
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
		}

		private void TestAdapterNotReassignedWithLocalFunction(PXAdapter adapter)
		{
			void Local()
			{
				adapter = NewAdapter();
			}

			PXLongOperation.StartOperation(this, () => adapter.Get());  // No reassignment happens, this should be reported
		}

		private void TestAdapterNotReassignedWithLocalFunctionRedefineParameter(PXAdapter adapter)
		{
			void Local(PXAdapter adapter)
			{
				adapter = NewAdapter();
			}

			Local(NewAdapter());
			PXLongOperation.StartOperation(this, () => adapter.Get());  // No reassignment happens, this should be reported
		}

		private void TestAdapterNotReassignedWithLambdaNotInvoked(PXAdapter adapter)
		{
			Action redefineLambda = () => adapter = NewAdapter();
			Action redefineDelegate = delegate { adapter = NewAdapter(); };

			PXLongOperation.StartOperation(this, () => adapter.Get());  // No reassignment happens, this should be reported
		}


		private void TestAdapterReassignedConditionallyBeforeCapture(PXAdapter adapter, bool condition)
		{
			if (condition)
			{
				adapter = NewAdapter();
			}
			
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment does not happen always and should be reported
		}

		private void TestAdapterReassignedInOtherClauseThanCapture(PXAdapter adapter, bool condition)
		{
			if (condition)
			{
				adapter = NewAdapter();
			}
			else
			{
				PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment does not happen always and should be reported
			}
		}

		private static PXAdapter NewAdapter() => 
			new PXAdapter(new PXView(this, isReadOnly: true, PXSelect<SomeDAC>.GetCommand()));

		private void Reassign(out PXAdapter adapter)
		{
			adapter = NewAdapter();
		}

		private void RunAdapter(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, () => adapter.Get());
		}
	}
}