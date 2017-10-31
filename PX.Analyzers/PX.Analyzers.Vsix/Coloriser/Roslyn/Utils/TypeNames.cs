using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Analyzers.Coloriser
{
    internal static class TypeNames
    {
        public static string IBqlTable => typeof(IBqlTable).Name;

        public static string BqlCommand => typeof(BqlCommand).Name;

        public static string IBqlField => typeof(IBqlField).Name;

        public static string IBqlParameter => typeof(IBqlParameter).Name;

        public static string IBqlJoin => typeof(IBqlJoin).Name;

        public static string IBqlComparison => typeof(IBqlComparison).Name;

        public static string IBqlCreator => typeof(IBqlCreator).Name;

        public static string PXSelectBaseType => typeof(PXSelectBase).Name;

		public static string Constant => typeof(Constant).Name;
    }
}
