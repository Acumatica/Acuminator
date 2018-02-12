using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Analyzers.Coloriser
{
    internal static class TypeNames
    {
        public static readonly string IBqlTable = typeof(IBqlTable).Name;

        public static readonly string BqlCommand = typeof(BqlCommand).Name;

        public static readonly string IBqlField = typeof(IBqlField).Name;

        public static readonly string IBqlParameter = typeof(IBqlParameter).Name;

        public static readonly string IBqlJoin = nameof(PX.Data.IBqlJoin);

        public static readonly string IBqlOrderBy = typeof(PX.Data.IBqlOrderBy).Name;

        public static readonly string IBqlAggregate = typeof(PX.Data.IBqlAggregate).Name;

        public static readonly string IBqlFunction = typeof(PX.Data.IBqlFunction).Name;

        public static readonly string IBqlComparison = typeof(IBqlComparison).Name;

        public static readonly string IBqlCreator = typeof(IBqlCreator).Name;

        public static readonly string IBqlPredicateChain = typeof(IBqlPredicateChain).Name;

        public static readonly string IBqlOn = typeof(IBqlOn).Name;

        public static readonly string PXSelectBaseType = typeof(PXSelectBase).Name;

		public static readonly string Constant = typeof(Constant).Name;

        public static readonly string PXCacheExtension = typeof(PXCacheExtension).Name;      
    }
}
