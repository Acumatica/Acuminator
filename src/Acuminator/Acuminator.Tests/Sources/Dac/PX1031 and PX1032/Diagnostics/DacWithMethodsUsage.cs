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
                UpdateViaDelegate();
                Status = new string('A', 5);
            }
        }

        public abstract class selected : IBqlField { }
        protected string _Status;
        [PXString]
        public virtual string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
            }
        }

        private Action UpdateViaDelegate = Update;

        protected virtual void SetStatus()
        {
        }

        public static void Update()
        {
        }
    }
}
