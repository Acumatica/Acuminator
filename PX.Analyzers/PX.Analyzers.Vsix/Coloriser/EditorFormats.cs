using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows;

namespace PX.Analyzers.Coloriser
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BQLOperatorFormat)]
    [Name(Constants.BQLOperatorFormat)]
    [UserVisible(true)]
    [Order(After = Constants.Priority)]
    internal sealed class BQLOperatorFormat : ClassificationFormatDefinition
    {
        public BQLOperatorFormat()
        {
            ForegroundColor = Colors.BlueViolet;
            DisplayName = Labels.BQLOperatorFormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.DacFormat)]
	[Name(Constants.DacFormat)]
	[UserVisible(true)]
    [Order(After = Constants.BQLOperatorFormat)]
    internal sealed class DACFormat : ClassificationFormatDefinition
	{
		public DACFormat()
		{
			ForegroundColor = Colors.Maroon;
			DisplayName = Labels.DacFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.DacFieldFormat)]
	[Name(Constants.DacFieldFormat)]
	[UserVisible(true)]
    [Order(After = Constants.DacFormat)]
    internal sealed class DACFieldFormat : ClassificationFormatDefinition
	{
		public DACFieldFormat()
		{
			ForegroundColor = Colors.DarkGreen;
			DisplayName = Labels.DacFieldFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.BQLParameterFormat)]
	[Name(Constants.BQLParameterFormat)]
	[UserVisible(true)]
    [Order(After = Constants.DacFieldFormat)]
    internal sealed class BQLParameterFormat : ClassificationFormatDefinition
	{
		public BQLParameterFormat()
		{
			ForegroundColor = Colors.IndianRed;
			DisplayName = Labels.BQLParameterFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.BQLConstantPrefixFormat)]
	[Name(Constants.BQLConstantPrefixFormat)]
	[UserVisible(true)]
	[Order(After = Constants.DacFieldFormat)]
	internal sealed class BQLConstantPrefixFormat : ClassificationFormatDefinition
	{
		public BQLConstantPrefixFormat()
		{
			ForegroundColor = Colors.Firebrick;
			DisplayName = Labels.BQLConstantPrefixFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.BQLConstantEndingFormat)]
	[Name(Constants.BQLConstantEndingFormat)]
	[UserVisible(true)]
	[Order(After = Constants.BQLConstantPrefixFormat)]
	internal sealed class BQLConstantEndingFormat : ClassificationFormatDefinition
	{
		public BQLConstantEndingFormat()
		{
			ForegroundColor = Colors.Teal;
			DisplayName = Labels.BQLConstantEndingFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}
}
