using PX.Data;
using System.Collections.Generic;

public class SomeGraph : PXGraph<SomeGraph>
{
    public class SomeDAC : IBqlTable
    {
    }
    public PXProcessing<SomeDAC> Processing;

    public SomeGraph()
    {
        object filter = null;

        Processing.SetProcessDelegate(delegate (SomeGraph graph, SomeDAC applicationProjection)
        {
            this.Clear();
        });
        Processing.SetProcessDelegate(delegate (SomeGraph graph, SomeDAC applicationProjection)
        {
            List<SomeDAC> list = new List<SomeDAC>();
            StaticFunc(list);
        });

        Processing.SetProcessDelegate(MemberFunc);
        Processing.SetProcessDelegate(StaticFunc);

        Processing.SetProcessDelegate(list => MemberFunc(list));
        Processing.SetProcessDelegate(list => StaticFunc(filter, list, false));
    }

    public static void StaticFunc(object filter, List<SomeDAC> list, bool markOnly)
    { }

    public static void StaticFunc(List<SomeDAC> list)
    { }

    public void MemberFunc(List<SomeDAC> list)
    { }
}