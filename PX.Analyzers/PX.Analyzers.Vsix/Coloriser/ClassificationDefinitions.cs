using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PX.Analyzers.Coloriser
{
	internal static class ClassificationTypes
	{
		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.DacFormat)]
		internal static ClassificationTypeDefinition DACType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.DacFieldFormat)]
		internal static ClassificationTypeDefinition DACFieldType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.BQLParameterFormat)]
		internal static ClassificationTypeDefinition BQLParameterType = null;

		[Export(typeof(ClassificationTypeDefinition))]
		[Name(Constants.BQLOperatorFormat)]
		internal static ClassificationTypeDefinition BQLOperatorType = null;
    }
}
