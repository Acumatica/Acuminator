﻿using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Tests.Sources
{
    [Serializable]
    public class DacExtensionWithMethodsUsage : PXCacheExtension<IIGPOALCLandedCost>
    {
        public abstract class selected : IBqlField { }
        protected bool? _selected;
        [PXDBBool()]
        [PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true)]
        public virtual bool? Selected
        {
            get
            {
                return this._selected;
            }
            set
            {
                this._selected = value;
                this.SetStatus();
                UpdateViaDelegate();
                Update();
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

	public class IIGPOALCLandedCost : IBqlTable
	{
		//DAC is required for correct collection of DAC semantic model for DAC extension
	}
}
