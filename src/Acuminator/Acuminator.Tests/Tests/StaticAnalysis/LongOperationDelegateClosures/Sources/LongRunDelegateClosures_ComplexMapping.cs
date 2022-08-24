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
			processor.GraphCaptured_ParamsNotCaptured(true, 1, this, adapter, null);									//Show diagnostic

			processor.GraphCaptured_ParamsNotCaptured(true, 1, localGraph, adapter, "arg1", "arg2");					//No diagnostic
			processor.GraphCaptured_ParamsNotCaptured(true, 1, localGraph, adapter, this);								//No diagnostic
			processor.GraphCaptured_ParamsNotCaptured(true, 1, localGraph, adapter, new object[] { "arg", this });		//No diagnostic

			processor.GraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, "arg1", "arg2");						//No diagnostic
			processor.GraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, "arg1", "arg2", this);					//Show diagnostic
			processor.GraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, this);									//Show diagnostic
			processor.GraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, new object[] { "arg", this });			//Show diagnostic
			processor.GraphCaptured_ParamsCaptured(true, 1, localGraph, adapter, new object[] { "arg", localGraph });	//No diagnostic

			// Named parameters check - names in position
			processor.GraphCaptured_ParamsCaptured(someFlag: true, count: 1, graph: this, adapter: adapter, args: null);	//Show diagnostic
			processor.GraphCaptured_ParamsCaptured(someFlag: true, count: 1, graph: localGraph, adapter, "arg1", "arg2");	//No diagnostic
			processor.GraphCaptured_ParamsCaptured(someFlag: true, count: 1, graph: localGraph, adapter, localGraph, this); //Show diagnostic

			// Named parameters check - names out of position
			processor.GraphCaptured_ParamsCaptured(someFlag: true, count: 1, adapter: adapter, graph: localGraph, args: localGraph);	//No diagnostic
			processor.GraphCaptured_ParamsCaptured(true, 1, adapter: adapter, graph: localGraph, args: localGraph);						//No diagnostic
			processor.GraphCaptured_ParamsCaptured(someFlag: true, count: 1, adapter: adapter, graph: localGraph, args: this);			//Show diagnostic
			processor.GraphCaptured_ParamsCaptured(someFlag: true, count: 1, adapter: adapter, graph: this, args: localGraph);			//Show diagnostic
			processor.GraphCaptured_ParamsCaptured(true, 1, adapter: adapter, graph: this, args: new[] { localGraph });					//Show diagnostic
			processor.GraphCaptured_ParamsCaptured(true, 1, adapter: adapter, graph: localGraph, args: new[] { localGraph });			//No diagnostic
			processor.GraphCaptured_ParamsCaptured(true, 1, adapter: adapter, graph: localGraph, args: new[] { localGraph, this });		//Show diagnostic

			// Named parameters check - optional parameters
			processor.GraphCaptured_ParamsCaptured_OptionalArgs(adapter, this);						//Show diagnostic
			processor.GraphCaptured_ParamsCaptured_OptionalArgs(adapter, localGraph);				//No diagnostic

			processor.GraphCaptured_ParamsCaptured_OptionalArgs(adapter, graph: localGraph);						//No diagnostic
			processor.GraphCaptured_ParamsCaptured_OptionalArgs(adapter, graph: localGraph, someFlag: true);		//No diagnostic
			processor.GraphCaptured_ParamsCaptured_OptionalArgs(adapter, graph: localGraph, args: this);			//Show diagnostic

			processor.GraphCaptured_ParamsCaptured_OptionalArgs(graph: this, adapter: adapter, someFlag: true, args: localGraph);	//Show diagnostic
			processor.GraphCaptured_ParamsCaptured_OptionalArgs(adapter: adapter, this, count: 1, args: "asd");						//Show diagnostic

			return adapter.Get();
		}

		public void MemberFunc()
		{

		}
		
		private class Processor
		{
			public IEnumerable GraphCaptured_ParamsCaptured_OptionalArgs(PXAdapter adapter, ProcessingGraph_ComplexMapping graph, int count = 1,
																		 bool someFlag = true, params object[] args)
			{
				return GraphCaptured_ParamsCaptured(someFlag, count, graph, adapter, args);
			}

			public IEnumerable GraphCaptured_ParamsNotCaptured(bool someFlag, int count, ProcessingGraph_ComplexMapping graph, 
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
			
			public IEnumerable GraphCaptured_ParamsCaptured(bool someFlag, int count, ProcessingGraph_ComplexMapping graph,
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