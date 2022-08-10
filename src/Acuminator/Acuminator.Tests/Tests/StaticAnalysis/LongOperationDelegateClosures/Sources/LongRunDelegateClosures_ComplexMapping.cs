using PX.Data;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Acuminator.Tests.Sources
{
	public class ProcessingGraph_ComplexMapping : PXGraph<ProcessingGraph_ComplexMapping>
	{
		[PXHidden]
		public class SomeDAC : IBqlTable
		{
		}

		private readonly Processor processor = new Processor();

		public PXAction<SomeDAC> ComplexMappingTest;

		[PXUIField(DisplayName = "Complex Mapping Test", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable complexMappingTest(PXAdapter adapter)
		{
			var localGraph = PXGraph.CreateInstance<ProcessingGraph_ComplexMapping>();

			// Params check
			processor.TestGraphCaptured_ParamsNotCaptured(true, 1, this, adapter, null);									//Show diagnostic

			processor.TestGraphCaptured_ParamsNotCaptured(true, 1, localGraph, adapter, "arg1", "arg2");					//No diagnostic
			processor.TestGraphCaptured_ParamsNotCaptured(true, 1, localGraph, adapter, this);								//No diagnostic
			processor.TestGraphCaptured_ParamsNotCaptured(true, 1, localGraph, adapter, new object[] { "arg", this });		//No diagnostic

			processor.TestGraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, "arg1", "arg2");						//No diagnostic
			processor.TestGraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, "arg1", "arg2", this);					//Show diagnostic
			processor.TestGraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, this);									//Show diagnostic
			processor.TestGraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, new object[] { "arg", this });			//Show diagnostic
			processor.TestGraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, new object[] { "arg", localGraph });	//No diagnostic

			// Named parameters check - names in position
			processor.TestGraphCaptured_ParamsCaptured(someFlag: true, count: 1, graph: this, adapter: adapter, args: null);	//Show diagnostic
			processor.TestGraphCaptured_ParamsCaptured(someFlag: true, count: 1, graph: localGraph, adapter, "arg1", "arg2");	//No diagnostic
			processor.TestGraphCaptured_ParamsCaptured(someFlag: true, count: 1, graph: localGraph, adapter, localGraph, this); //Show diagnostic

			// Named parameters check - names out of position
			processor.TestGraphCaptured_ParamsCaptured(someFlag: true, count: 1, adapter: adapter, graph: localGraph, args: localGraph);	//No diagnostic
			processor.TestGraphCaptured_ParamsCaptured(true, 1, adapter: adapter, graph: localGraph, args: localGraph);						//No diagnostic
			processor.TestGraphCaptured_ParamsCaptured(someFlag: true, count: 1, adapter: adapter, graph: localGraph, args: this);			//Show diagnostic
			processor.TestGraphCaptured_ParamsCaptured(someFlag: true, count: 1, adapter: adapter, graph: this, args: localGraph);			//Show diagnostic
			processor.TestGraphCaptured_ParamsCaptured(true, 1, adapter: adapter, graph: this, args: new[] { localGraph });					//Show diagnostic
			processor.TestGraphCaptured_ParamsCaptured(true, 1, adapter: adapter, graph: localGraph, args: new[] { localGraph });			//No diagnostic
			processor.TestGraphCaptured_ParamsCaptured(true, 1, adapter: adapter, graph: localGraph, args: new[] { localGraph, this });		//Show diagnostic

			return adapter.Get();
		}

		public void MemberFunc()
		{

		}
		
		private class Processor
		{
			public IEnumerable TestGraphCaptured_ParamsCaptured_OptionalArgs(PXAdapter adapter, ProcessingGraph_ComplexMapping graph, int count = 1,
																			 bool someFlag = true, params object[] args)
			{
				return TestGraphCaptured_ParamsCaptured(someFlag, count, graph, adapter, args);
			}

			public IEnumerable TestGraphCaptured_ParamsNotCaptured(bool someFlag, int count, ProcessingGraph_ComplexMapping graph, 
																	PXAdapter adapter, params object[] args)
			{
				PXLongOperation.StartOperation(graph, args =>
				{
					if (args.Length == count)
						return;

					if (someFlag)
						graph.MemberFunc();
				}, args);

				return adapter.Get();
			}

			public IEnumerable TestGraphCaptured_ParamsCaptured(bool someFlag, int count, ProcessingGraph_ComplexMapping graph,
																PXAdapter adapter, params object[] args)
			{
				PXLongOperation.StartOperation(graph, () =>
				{
					if (args.Length == count)
						return;

					if (someFlag)
						graph.MemberFunc();
				});

				return adapter.Get();
			}
		}
	}
}