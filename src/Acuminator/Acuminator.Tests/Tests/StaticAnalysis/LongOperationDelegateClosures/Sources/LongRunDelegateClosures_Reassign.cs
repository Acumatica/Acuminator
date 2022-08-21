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
			ReassignedAlwaysBeforeCapture(adapter);                          // No diagnostic
			ReassignedNullAlwaysBeforeCapture(adapter);                      // No diagnostic
			ReassignedViaOutAlwaysBeforeCapture(adapter);                    // No diagnostic
			ReassignedAndUsedLater(adapter);                                 // No diagnostic
			ReassignedWithLocalFunction(adapter);                            // No diagnostic

			NotReassignedWithLocalFunction(adapter);                        // Show diagnostic
			NotReassignedWithLocalFunctionRedefineParameter(adapter);       // Show diagnostic
			NotReassignedWithLambdaNotInvoked(adapter);						// Show diagnostic
			ReassignedConditionallyBeforeCapture(adapter, true);            // Show diagnostic
			ReassignedInOtherClauseThanCapture(adapter, true);				// Show diagnostic

			ReassignedAlwaysInIf(adapter, true);                            // No diagnostic
			ReassignedAlwaysInSwitch(adapter, true);                        // No diagnostic
			ReassignedConditionallyInSwitch(adapter, true);                 // Show diagnostic

			NestedLocalFunctionAlwaysReassign(adapter);                     // No diagnostic
			NestedLocalFunctionNoReassign(adapter);							// Show diagnostic
			NestedLocalFunctionConditionalReassign(adapter);                // Show diagnostic

			return adapter.Get();
		}


		private void ReassignedAlwaysBeforeCapture(PXAdapter adapter)
		{
			adapter = new PXAdapter(
				new PXView(this, isReadOnly: true, PXSelect<SomeDAC>.GetCommand()));
			PXLongOperation.StartOperation(this, () => adapter.Get());					// This reassignment happens always and should not be reported
		}

		private void ReassignedNullAlwaysBeforeCapture(PXAdapter adapter)
		{
			adapter = null;
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
		}

		private void ReassignedViaOutAlwaysBeforeCapture(PXAdapter adapter)
		{
			Reassign(out adapter);
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
		}

		private void ReassignedAndUsedLater(PXAdapter adapter)
		{
			adapter = NewAdapter();                                     // This reassignment happens always and should not be reported
			RunAdapter(adapter);
		}

		private void ReassignedWithLocalFunction(PXAdapter adapter)
		{
			void Local()
			{
				adapter = NewAdapter();
			}

			Local();
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
		}

		private void NotReassignedWithLocalFunction(PXAdapter adapter)
		{
			void Local()
			{
				adapter = NewAdapter();
			}

			PXLongOperation.StartOperation(this, () => adapter.Get());  // No reassignment happens, this should be reported
		}

		private void NotReassignedWithLocalFunctionRedefineParameter(PXAdapter adapter)
		{
			void Local(PXAdapter adapter)
			{
				adapter = NewAdapter();
			}

			Local(NewAdapter());
			PXLongOperation.StartOperation(this, () => adapter.Get());  // No reassignment happens, this should be reported
		}

		private void NotReassignedWithLambdaNotInvoked(PXAdapter adapter)
		{
			Action redefineLambda = () => adapter = NewAdapter();
			Action redefineDelegate = delegate { adapter = NewAdapter(); };

			PXLongOperation.StartOperation(this, () => adapter.Get());  // No reassignment happens, this should be reported
		}


		private void ReassignedConditionallyBeforeCapture(PXAdapter adapter, bool condition)
		{
			if (condition)
			{
				adapter = NewAdapter();
			}
			
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment does not happen always and should be reported
		}

		private void ReassignedInOtherClauseThanCapture(PXAdapter adapter, bool condition)
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

		private void ReassignedAlwaysInIf(PXAdapter adapter, bool condition)
		{
			if (condition)
			{
				adapter = NewAdapter();

				{
					PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
				}
			}
		}

		private void ReassignedAlwaysInSwitch(PXAdapter adapter, bool condition)
		{
			switch (condition)
			{
				case true:
					adapter = NewAdapter();

					if (adapter != null)
					{
						PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
					}

					break;

				default:
					break;
			}
		}

		private void ReassignedConditionallyInSwitch(PXAdapter adapter, bool condition)
		{
			switch (condition)
			{
				case true:
					adapter = NewAdapter();
					break;

				default:
					break;
			}

			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment does not happen always and should be reported
		}

		private void NestedLocalFunctionAlwaysReassign(PXAdapter adapter)
		{
			Local(true, adapter);

			void Local(bool flag, PXAdapter adapterToCheck)
			{
				NestedLocal(flag, adapter);

				void NestedLocal(bool flag2, PXAdapter adapterToCheck2)
				{
					NestedNestedLocal(flag2, adapterToCheck2);

					void NestedNestedLocal(bool flag3, PXAdapter adapterToCheck3)
					{
						adapter = NewAdapter();
					}
				}
			}

			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens always and should not be reported
		}

		private void NestedLocalFunctionNoReassign(PXAdapter adapter)
		{
			Local(true, adapter);
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment doesn't happen and should be reported

			void Local(bool flag, PXAdapter adapterToCheck)
			{
				NestedLocal(flag, adapter);

				void NestedLocal(bool flag2, PXAdapter adapterToCheck2)
				{
					void NestedNestedLocal(bool flag3, PXAdapter adapterToCheck3)
					{
						adapter = NewAdapter();
					}
				}
			}
		}

		private void NestedLocalFunctionConditionalReassign(PXAdapter adapter)
		{
			void Local(bool flag, PXAdapter adapterToCheck)
			{
				NestedLocal(flag, adapter);

				void NestedLocal(bool flag2, PXAdapter adapterToCheck2)
				{
					if (flag2)
						NestedNestedLocal(flag2, adapterToCheck2);

					void NestedNestedLocal(bool flag3, PXAdapter adapterToCheck3)
					{
						adapter = NewAdapter();
					}
				}
			}

			Local(true, adapter);
			PXLongOperation.StartOperation(this, () => adapter.Get());  // This reassignment happens only in some cases and should be reported		
		}

		private static PXAdapter NewAdapter() => 
			new PXAdapter(new PXView(new PXGraph(), isReadOnly: true, PXSelect<SomeDAC>.GetCommand()));

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