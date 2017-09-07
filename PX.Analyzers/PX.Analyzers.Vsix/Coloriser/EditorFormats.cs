using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows;

namespace PX.Analyzers.Coloriser
{
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.DacFormat)]
	[Name(Constants.DacFormat)]
	[UserVisible(true)]
	[Order(After = Constants.Priority)]
	internal sealed class DACFormat : ClassificationFormatDefinition
	{
		public DACFormat()
		{
			DisplayName = Labels.DacFormatLabel;
			ForegroundColor = Colors.BurlyWood;
			BackgroundColor = Colors.White;
			DisplayName = Labels.DacFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.DacFieldFormat)]
	[Name(Constants.DacFieldFormat)]
	[UserVisible(true)]
	[Order(After = Constants.Priority)]
	internal sealed class DACFieldFormat : ClassificationFormatDefinition
	{
		public DACFieldFormat()
		{
			DisplayName = Labels.DacFieldFormatLabel;
			ForegroundColor = Colors.DarkGreen;
			BackgroundColor = Colors.White;
			DisplayName = Labels.DacFieldFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.BQLParameterFormat)]
	[Name(Constants.BQLParameterFormat)]
	[UserVisible(true)]
	[Order(After = Constants.Priority)]
	internal sealed class BQLParameterFormat : ClassificationFormatDefinition
	{
		public BQLParameterFormat()
		{
			DisplayName = Labels.BQLParameterFormatLabel;
			ForegroundColor = Colors.IndianRed;
			BackgroundColor = Colors.White;
			DisplayName = Labels.DacFieldFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}
}
