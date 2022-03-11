﻿using PX.Data;

using System.Collections.Generic;

public class SOItemProcessing : PXGraph<SOItemProcessing>
{
	public class SomeDAC : IBqlTable
	{
	}

	private readonly Processor processor = new Processor();

	private Processor ProcessorProperty => processor;

	private static Processor processorStatic = new Processor();

	public PXProcessing<SomeDAC> Processing;

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

	public void MemberFunc(List<SomeDAC> list)
	{ }

	public void MainProcessingHandler(SomeDAC dac)
	{
	}

	public void FinallyHandler()
	{
	}

	private class Processor
	{
		public void MemberFunc(List<SomeDAC> list)
		{ }

		public static void StaticFunc(List<SomeDAC> list)
		{ }
	}
}