using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationDuringInitialization.Sources
{
    public class SOOrderMaint : PXGraph<SOOrderMaint, SOOrder>
    {
        public PXSelect<SOOrder> Orders;

        public SOOrderMaint()
        {
            PXButtonDelegate actionHandlerSimple = adapter =>
            {
                MethodWhichInitializeAnotherGraph(adapter);
                return adapter.Get();
            };

            Actions.Add("ActionSimple", new PXNamedAction<SOOrder>(this, "ActionSimple", actionHandlerSimple));

            PXButtonDelegate actionHandlerParenthesized = (adapter) =>
            {
                MethodWhichInitializeAnotherGraph(adapter);
                return adapter.Get();
            };

            Actions.Add("ActionParenthesized", new PXNamedAction<SOOrder>(this, "ActionParenthesized", actionHandlerSimple));
        }

        private void MethodWhichInitializeAnotherGraph(PXAdapter adapter)
        {
            SOOrderMaint maint = PXGraph.CreateInstance<SOOrderMaint>();

            maint.GetHashCode();
        }
    }

    public class SOOrder : IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}
