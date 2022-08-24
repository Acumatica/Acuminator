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

		public PXAction<SomeDAC> SomeAction;

		[PXUIField(DisplayName = "Some Action", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable someAction(PXAdapter adapter)
		{
			NestedLocalFunctionNoCapture(adapter);                          // No diagnostic
			NestedLocalFunctionRedefineParameterNoCapture(adapter);         // No diagnostic

			LocalCapture();                                                 // Show diagnostic		
			NestedLocalFunctionWithCapture(adapter);                        // Show diagnostic
			NestedLocalFunctionRedefineParameterCapture(adapter);			// Show diagnostic
			NestedStaticLocalFunctionWithCaptureFromParameter(adapter);     // Show diagnostic

			return adapter.Get();


			void LocalCapture()
			{
				PXLongOperation.StartOperation(this, () => adapter.Get());  // Show diagnostic
			}		
		}

		private void NestedLocalFunctionNoCapture(PXAdapter adapter)
		{
			Local(true, adapter);

			void Local(bool flag, PXAdapter adapterToCheck)
			{
				NestedLocal(flag, adapter);

				void NestedLocal(bool flag2, PXAdapter adapterToCheck2)
				{
					void NestedNestedLocal(bool flag3, PXAdapter adapterToCheck3)
					{
						PXLongOperation.StartOperation(this, () => adapter.Get());
					}
				}
			}
		}

		private void NestedLocalFunctionRedefineParameterNoCapture(PXAdapter adapter)
		{
			Local(true, adapter);

			void Local(bool flag, PXAdapter adapterToCheck)
			{
				NestedLocal(flag, adapter);

				void NestedLocal(bool flag2, PXAdapter adapterToCheck2)
				{
					NestedNestedLocal(flag2, null);

					void NestedNestedLocal(bool flag3, PXAdapter adapter)
					{
						PXLongOperation.StartOperation(this, () => adapter.Get());
					}
				}
			}
		}

		private void NestedLocalFunctionRedefineParameterCapture(PXAdapter adapter)
		{
			Local(true, adapter);

			void Local(bool flag, PXAdapter adapterToCheck)
			{
				NestedLocal(flag, adapter);

				void NestedLocal(bool flag2, PXAdapter adapterToCheck2)
				{
					NestedNestedLocal(flag2, adapterToCheck2);

					void NestedNestedLocal(bool flag3, PXAdapter adapter)
					{
						PXLongOperation.StartOperation(this, () => adapter.Get());
					}
				}
			}
		}

		private void NestedLocalFunctionWithCapture(PXAdapter adapter)
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
						PXLongOperation.StartOperation(this, () => adapter.Get());  
					}
				}
			}			
		}

		private void NestedStaticLocalFunctionWithCaptureFromParameter(PXAdapter adapter)
		{
			Local(true, adapter);

			void Local(bool flag, PXAdapter adapterToCheck)
			{
				NestedLocal(flag, adapter);

				void NestedLocal(bool flag2, PXAdapter adapterToCheck2)
				{
					NestedNestedLocal(this, flag2, adapterToCheck);

					static void NestedNestedLocal(PXGraph graph,  bool flag3, PXAdapter adapterToCheck3)
					{
						PXLongOperation.StartOperation(graph, () => adapterToCheck3.Get());
					}
				}
			}
		}

		private static PXAdapter NewAdapter() => 
			new PXAdapter(new PXView(new PXGraph(), isReadOnly: true, PXSelect<SomeDAC>.GetCommand()));
	}
}