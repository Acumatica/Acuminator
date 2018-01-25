using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PX.Analyzers.Coloriser
{
	internal static class ClassificationTypeDefinitions
	{
		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.DacFormat)]
		internal static ClassificationTypeDefinition DACType = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.DacExtensionFormat)]
        internal static ClassificationTypeDefinition DACExtensionType = null;

        [Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.DacFieldFormat)]
		internal static ClassificationTypeDefinition DACFieldType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.BQLParameterFormat)]
		internal static ClassificationTypeDefinition BQLParameterType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.BQLOperatorFormat)]
		internal static ClassificationTypeDefinition BQLOperatorType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.BQLConstantPrefixFormat)]
		internal static ClassificationTypeDefinition BQLConstantPrefixType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.BQLConstantEndingFormat)]
		internal static ClassificationTypeDefinition BQLConstantEndingType = null;




        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_1_Format)]
        internal static ClassificationTypeDefinition BraceLevel_1_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_2_Format)]
        internal static ClassificationTypeDefinition BraceLevel_2_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_3_Format)]
        internal static ClassificationTypeDefinition BraceLevel_3_Type = null;



        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_4_Format)]
        internal static ClassificationTypeDefinition BraceLevel_4_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_5_Format)]
        internal static ClassificationTypeDefinition BraceLevel_5_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_6_Format)]
        internal static ClassificationTypeDefinition BraceLevel_6_Type = null;





        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_7_Format)]
        internal static ClassificationTypeDefinition BraceLevel_7_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_8_Format)]
        internal static ClassificationTypeDefinition BraceLevel_8_Type = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.BraceLevel_9_Format)]
        internal static ClassificationTypeDefinition BraceLevel_9_Type = null;
    }
}
