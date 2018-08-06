using PX.Data;
using System;

namespace Acuminator.Tests.Sources
{
    [Serializable]
    public class IIGPOALCLandedCost : IBqlTable
    {
        public abstract class hold : IBqlField { }
        protected bool? _Hold;
        [PXDBBool()]
        [PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true)]
        public virtual bool? Hold
        {
            get
            {
                return this._Hold;
            }
            set
            {
                this._Hold = value;
                this.SetStatus();
            }
        }

        protected virtual void SetStatus()
        {
        }

        public static void Update()
        {
        }
    }
}
