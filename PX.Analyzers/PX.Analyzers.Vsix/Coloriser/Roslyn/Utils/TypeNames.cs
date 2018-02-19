﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using PX.Data;
using PX.Analyzers.Vsix.Utilities;

namespace PX.Analyzers.Coloriser
{
    internal static class TypeNames
    {
        public static readonly string IBqlTable = typeof(IBqlTable).Name;

        public static readonly string BqlCommand = typeof(BqlCommand).Name;

        public static readonly string IBqlField = typeof(IBqlField).Name;

        public static readonly string IBqlParameter = typeof(IBqlParameter).Name;

        public static readonly string IBqlJoin = nameof(PX.Data.IBqlJoin);

        public static readonly string IBqlOrderBy = typeof(IBqlOrderBy).Name;

        public static readonly string IBqlAggregate = typeof(IBqlAggregate).Name;

        public static readonly string IBqlFunction = typeof(IBqlFunction).Name;

        public static readonly string IBqlSortColumn = typeof(IBqlSortColumn).Name;

        public static readonly string IBqlComparison = typeof(IBqlComparison).Name;

        public static readonly string IBqlCreator = typeof(IBqlCreator).Name;

        public static readonly string IBqlPredicateChain = typeof(IBqlPredicateChain).Name;

        public static readonly string IBqlOn = typeof(IBqlOn).Name;

        public static readonly string PXSelectBaseType = typeof(PXSelectBase).Name;

		public static readonly string Constant = typeof(Constant).Name;

        public static readonly string PXCacheExtension = typeof(PXCacheExtension).Name;

        public static readonly string PXGraph = typeof(PXGraph).Name;

        public static readonly string PXAction = typeof(PXAction).Name;

        public static Dictionary<string, ColoredCodeType> TypeNamesToColoredCodeTypesForIdentifier { get; } =
            new Dictionary<string, ColoredCodeType>
            {
                [IBqlTable] = ColoredCodeType.Dac,
                [IBqlField] = ColoredCodeType.DacField,
                [PXCacheExtension] = ColoredCodeType.DacExtension,
                [IBqlParameter] = ColoredCodeType.BqlParameter,
                [Constant] = ColoredCodeType.BQLConstantEnding,
                [IBqlCreator] = ColoredCodeType.BqlOperator,
                [IBqlJoin] = ColoredCodeType.BqlOperator,
                [PXGraph] = ColoredCodeType.PXGraph
            };
    }
}
