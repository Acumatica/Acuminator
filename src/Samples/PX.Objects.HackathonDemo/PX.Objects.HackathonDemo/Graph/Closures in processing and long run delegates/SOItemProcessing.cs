using System;
using PX.Data;

using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.HackathonDemo
{
	public class SOItemProcessing : PXGraph<SOItemProcessing>
	{
		private static readonly Guid ID = Guid.NewGuid();

		public class SomeDAC : IBqlTable
		{
		}

		private readonly Processor processor = new Processor();

		private Processor ProcessorProperty => processor;

		private static Processor processorStatic = new Processor();

		public PXProcessing<SomeDAC> Processing;

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

			PXLongOperation.StartOperation(ID, arguments => MemberFunc(), args);                            //Should be diagnostic
			PXLongOperation.StartOperation(ID, arguments => StaticFunc(filter, inputList, false), args);    //No diagnostic

			PXLongOperation.StartOperation(ID, () => MemberFunc());                 //Should be diagnostic
			PXLongOperation.StartOperation(ID, delegate                             //Should be diagnostic
			{
				Clear();
			});

			PXLongOperation.StartOperation(ID, () => StaticFunc());					//No diagnostic
			PXLongOperation.StartOperation(ID, delegate                             //No diagnostic
			{
				StaticFunc();
			});

			PXLongOperation.StartOperation(this, MemberFunc);                       //Should be diagnostic
			PXLongOperation.StartOperation(this, StaticFunc);						//No diagnostic

			//Test helper static function
			PXLongOperation.StartOperation(this, Processor.StaticFunc);             //No diagnostic

			//test fields and properties
			PXLongOperation.StartOperation(this, processor.MemberFunc);             //Should be diagnostic
			PXLongOperation.StartOperation(this, ProcessorProperty.MemberFunc);     //Should be diagnostic
			PXLongOperation.StartOperation(this, processorStatic.MemberFunc);       //No diagnostic

			return adapter.Get();
		}

		public SOItemProcessing()
		{
			object filter = null;

			Processing.SetProcessDelegate(delegate (SOItemProcessing graph, SomeDAC applicationProjection) {
				this.Clear();
			});

			Processing.SetProcessDelegate(delegate (SOItemProcessing graph, SomeDAC applicationProjection) {
				List<SomeDAC> list = new List<SomeDAC>();
				StaticFunc(list);
			});

			Processing.SetProcessDelegate(MemberFunc);
			Processing.SetProcessDelegate(StaticFunc);

			Processing.SetProcessDelegate(list => MemberFunc(list));
			Processing.SetProcessDelegate(list => StaticFunc(filter, list, false));

			//Test helper static function
			Processing.SetProcessDelegate(Processor.StaticFunc);      //No diagnostic

			//test fields and properties
			Processing.SetProcessDelegate(processor.MemberFunc);          //Should be diagnostic
			Processing.SetProcessDelegate(ProcessorProperty.MemberFunc);  //Should be diagnostic
			Processing.SetProcessDelegate(processorStatic.MemberFunc);    //No diagnostic

			Processing.SetProcessDelegate<SOItemProcessing>((graph, dac) => graph.MainProcessingHandler(dac), graph => graph.FinallyHandler());    //No diagnostic
			Processing.SetProcessDelegate<SOItemProcessing>((graph, dac) => MainProcessingHandler(dac), graph => FinallyHandler());                //Should be diagnostic
			Processing.SetProcessDelegate<SOItemProcessing>((graph, dac) => graph.MainProcessingHandler(dac), graph => FinallyHandler());    //Should be diagnostic
		}

		public static void StaticFunc(object filter, List<SomeDAC> list, bool markOnly)
		{ }

		public static void StaticFunc(List<SomeDAC> list)
		{ }

		public static void StaticFunc()
		{ }

		public void MemberFunc(List<SomeDAC> list)
		{ }

		public void MemberFunc()
		{ }

		public void MainProcessingHandler(SomeDAC dac)
		{
		}

		public void FinallyHandler()
		{
		}

		private class Processor
		{
			public void MemberFunc()
			{ }

			public void MemberFunc(List<SomeDAC> list)
			{ }

			public static void StaticFunc(List<SomeDAC> list)
			{ }

			public static void StaticFunc()
			{ }
		}
	}
}