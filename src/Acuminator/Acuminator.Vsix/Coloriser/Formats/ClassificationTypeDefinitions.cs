using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Acuminator.Vsix.Coloriser
{
	internal static class ClassificationTypeDefinitions
	{
		[Export(typeof(ClassificationTypeDefinition))]
		[Name(ColoringConstants.DacFormat)]
		internal static ClassificationTypeDefinition DACType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.DacExtensionFormat)]
        internal static ClassificationTypeDefinition DACExtensionType = null;

        [Export(typeof(ClassificationTypeDefinition))]
		[Name(ColoringConstants.DacFieldFormat)]
		internal static ClassificationTypeDefinition DACFieldType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(ColoringConstants.BQLParameterFormat)]
		internal static ClassificationTypeDefinition BQLParameterType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(ColoringConstants.BQLOperatorFormat)]
		internal static ClassificationTypeDefinition BQLOperatorType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(ColoringConstants.BQLConstantPrefixFormat)]
		internal static ClassificationTypeDefinition BQLConstantPrefixType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(ColoringConstants.BQLConstantEndingFormat)]
		internal static ClassificationTypeDefinition BQLConstantEndingType = null;


        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.PXGraphFormat)]
        internal static ClassificationTypeDefinition PXGraphType = null;


        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.PXActionFormat)]
        internal static ClassificationTypeDefinition PXActionType = null;
        //*******************************************************************************************************************************************************************************
        //*******************************************************************************************************************************************************************************
        //*******************************************************************************************************************************************************************************
        #region Braces      
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_1_Format)]
        internal static ClassificationTypeDefinition BraceLevel_1_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_2_Format)]
        internal static ClassificationTypeDefinition BraceLevel_2_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_3_Format)]
        internal static ClassificationTypeDefinition BraceLevel_3_Type = null;



        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_4_Format)]
        internal static ClassificationTypeDefinition BraceLevel_4_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_5_Format)]
        internal static ClassificationTypeDefinition BraceLevel_5_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_6_Format)]
        internal static ClassificationTypeDefinition BraceLevel_6_Type = null;




        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_7_Format)]
        internal static ClassificationTypeDefinition BraceLevel_7_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_8_Format)]
        internal static ClassificationTypeDefinition BraceLevel_8_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_9_Format)]
        internal static ClassificationTypeDefinition BraceLevel_9_Type = null;



        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_10_Format)]
        internal static ClassificationTypeDefinition BraceLevel_10_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_11_Format)]
        internal static ClassificationTypeDefinition BraceLevel_11_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_12_Format)]
        internal static ClassificationTypeDefinition BraceLevel_12_Type = null;




        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_13_Format)]
        internal static ClassificationTypeDefinition BraceLevel_13_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ColoringConstants.BraceLevel_14_Format)]
        internal static ClassificationTypeDefinition BraceLevel_14_Type = null;
        #endregion
    }
}
