﻿using PX.Data;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Acuminator.Tests.Sources
{
	public class ProcessingGraph : PXGraph<ProcessingGraph>
	{
		private static readonly Guid ID = Guid.NewGuid();

		[PXHidden]
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
			PXLongOperation.StartOperation(this, processorStatic.MemberFunc);       //No diagnostic

			//Recursive analysis test
			//Test recursive analyis	
			RunLongRunWithGraph(this);                                              //Should be diagnostic

			var localGraph = PXGraph.CreateInstance<ProcessingGraph>();
			RunLongRunWithGraph(localGraph);                                        //No diagnostic

			processor.TestGraphCaptured(someFlag: true, this);										//Should be diagnostic
			ProcessorProperty.TestGraphCapturedInHelper(someFlag: true, this);						//Should be diagnostic
			processor.TestGraphCapturedInArray(someFlag: true, this, new[] { this });				//Should be diagnostic
			processorStatic.TestGraphNotCaptured(someFlag: true, this);								//No diagnostic

			processor.TestGraphCaptured(someFlag: true, localGraph);								//No diagnostic
			ProcessorProperty.TestGraphCapturedInHelper(someFlag: true, localGraph);				//No diagnostic
			processor.TestGraphCapturedInArray(someFlag: true, this, new[] { localGraph });			//No diagnostic

			// Test capturing graph via member access
			AnotherProcessor.RunConditional(this);								//Should be diagnostic
			AnotherProcessor.RunConditional(localGraph);						//No diagnostic

			AnotherProcessor.RunNull(this);										//No diagnostic
			AnotherProcessor.RunNull(localGraph);								//No diagnostic

			AnotherProcessor.RunProcessor(this);								//Should be diagnostic
			AnotherProcessor.RunProcessor(localGraph);							//No diagnostic

			AnotherProcessor.RunDelegate(this);									//Should be diagnostic
			AnotherProcessor.RunDelegate(localGraph);							//No diagnostic

			return adapter.Get();
		}

		public static void RunLongRunWithGraph(ProcessingGraph graph)
		{
			PXLongOperation.StartOperation(graph, () => graph.MemberFunc());
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

		public PXAction<SomeDAC> TestAdapterAction;

		[PXUIField(DisplayName = "Test Adapter Action", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable testAdapterAction(PXAdapter adapter)
		{
			object[] args = new object[] { 1, 2 };
			
			PXLongOperation.StartOperation(ID, arguments => adapter.Get(), args);						//Should be diagnostic
			PXLongOperation.StartOperation(ID, arguments => new PXAdapter(view: null).Get(), args);		//No diagnostic

			PXLongOperation.StartOperation(ID, () => UseAdapter(adapter));								//Should be diagnostic
			PXLongOperation.StartOperation(ID, delegate													//Should be diagnostic
			{
				UseAdapter(adapter);
			});

			PXLongOperation.StartOperation(ID, () => typeof(PXAdapter).Name.ToString());				//No diagnostic
			PXLongOperation.StartOperation(ID, delegate													//No diagnostic
			{
				typeof(PXAdapter).Name.ToString();
			});

			//Test recursive analyis
			RunLongRunWithAdapter(adapter);                                                         //Should be diagnostic

			processor.TestAdapterCapture(adapter);													//Should be diagnostic
			ProcessorProperty.TestAdapterCapturedInHelper(someFlag: true, adapter);					//Should be diagnostic
			processor.TestAdapterCapturedInList(someFlag: true, new List<object> { adapter });		//Should be diagnostic

			return adapter.Get();
		}

		private static void UseAdapter(PXAdapter adapter) { }

		public void RunLongRunWithAdapter(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(this, () => UseAdapter(adapter));
		}

		private class Processor
		{
			public PXToggleAsyncDelegate MemberFuncDelegate { get; }

			public Processor()
			{
				MemberFuncDelegate = MemberFunc;
			}

			public void MemberFunc()
			{ }

			public static void StaticFunc()
			{ }

			public void TestGraphCaptured(bool someFlag, ProcessingGraph graph)
			{
				PXLongOperation.StartOperation(graph, () =>
				{
					if (someFlag)
						graph.MemberFunc();
				});
			}

			public void TestGraphCapturedInHelper(bool someFlag, ProcessingGraph graph)
			{
				if (someFlag)
					AnotherProcessor.Run(graph);
			}

			public void TestGraphNotCaptured(bool someFlag, ProcessingGraph graph)
			{
				PXLongOperation.StartOperation(graph, () =>
				{
					var graphToUse = PXGraph.CreateInstance<ProcessingGraph>();

					if (someFlag)
						graphToUse.MemberFunc();
				});
			}

			public void TestGraphCapturedInArray(bool someFlag, ProcessingGraph graph, params object[] graphs)
			{
				PXLongOperation.StartOperation(graph, () =>
				{
					if (someFlag)
						(graphs[0] as ProcessingGraph)?.MemberFunc();
				});
			}

			public void TestAdapterCapture(PXAdapter adapter)
			{
				PXLongOperation.StartOperation(new Guid(), () => adapter.Get());
			}

			public void TestAdapterCapturedInHelper(bool someFlag, PXAdapter a)
			{
				if (someFlag)
					AnotherProcessor.Run(a);
			}

			public void TestAdapterCapturedInList(bool someFlag, List<object> adapters)
			{
				if (someFlag)
					PXLongOperation.StartOperation(new Guid(), () => adapters[0]?.ToString());
			}
		}


		private static class AnotherProcessor
		{
			public static void Run(ProcessingGraph graph) =>
				PXLongOperation.StartOperation(graph, graph.MemberFunc);

			public static void RunProcessor(ProcessingGraph graph) =>
				PXLongOperation.StartOperation(graph, graph.ProcessorProperty.MemberFunc);

			public static void RunDelegate(ProcessingGraph graph) =>
				PXLongOperation.StartOperation(graph, graph.processor.MemberFuncDelegate);

			public static void RunConditional(ProcessingGraph graph)
			{
				PXLongOperation.StartOperation(graph, graph.processor?.MemberFuncDelegate);
				PXLongOperation.StartOperation(graph, graph?.processor?.MemberFuncDelegate);
			}

			public static void RunNull(ProcessingGraph graph) => PXLongOperation.StartOperation(graph, null);


			public static void Run(PXAdapter adapterToTest)
			{
				PXLongOperation.StartOperation(new Guid(), delegate { adapterToTest.Get(); });
			}
		}
	}
}