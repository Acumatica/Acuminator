using PX.Data;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Acuminator.Tests.Sources
{
	public class ProcessingGraph : PXGraph<ProcessingGraph>
	{
		private static readonly Guid ID = Guid.NewGuid();

		public class SomeDAC : IBqlTable
		{
		}

		private readonly Processor processor = new Processor();

		private Processor ProcessorProperty => processor;

		private static Processor processorStatic = new Processor();

		public PXAction<SomeDAC> SomeAction;

		[PXUIField(DisplayName = "Some Action", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable someAction(PXAdapter adapter)
		{
			object filter = null;
			object[] args = new object[] { 1, 2 };
			List<SomeDAC> inputList = new List<SomeDAC> 
			{ 
				new SomeDAC() 
			}; 

			PXLongOperation.StartOperation(ID, arguments => MemberFunc(), args);							//Should be diagnostic
			PXLongOperation.StartOperation(ID, arguments => StaticFunc(filter, inputList, false), args);	//No diagnostic

			PXLongOperation.StartOperation(ID, () => MemberFunc());					//Should be diagnostic
			PXLongOperation.StartOperation(ID, delegate								//Should be diagnostic
			{ 
				Clear(); 
			});

			PXLongOperation.StartOperation(ID, () => StaticFuncWithNoInput());		//No diagnostic
			PXLongOperation.StartOperation(ID, delegate								//No diagnostic
			{
				StaticFuncWithNoInput();
			});

			PXLongOperation.StartOperation(this, MemberFunc);						//Should be diagnostic
			PXLongOperation.StartOperation(this, StaticFuncWithNoInput);			//No diagnostic

			//Test helper static function
			PXLongOperation.StartOperation(this, Processor.StaticFunc);				//No diagnostic

			//test fields and properties
			PXLongOperation.StartOperation(this, processor.MemberFunc);				//Should be diagnostic
			PXLongOperation.StartOperation(this, ProcessorProperty.MemberFunc);		//Should be diagnostic
			PXLongOperation.StartOperation(this, processorStatic.MemberFunc);		//No diagnostic

			return adapter.Get();
		}

		public static void StaticFunc(object filter, List<SomeDAC> list, bool markOnly)
		{ }

		public static void StaticFunc(List<SomeDAC> list)
		{ }

		public void MemberFunc(List<SomeDAC> list)
		{ }

		public void MemberFunc()
		{ }

		public static void StaticFuncWithNoInput()
		{

		}

		private class Processor
		{
			public void MemberFunc()
			{ }

			public static void StaticFunc()
			{ }
		}
	}
}