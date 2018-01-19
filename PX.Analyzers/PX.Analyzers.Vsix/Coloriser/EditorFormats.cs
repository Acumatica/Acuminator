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
            ForegroundColor = Color.FromRgb(r: 181, g: 121, b: 255);
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
			ForegroundColor = Color.FromRgb(r: 255, g: 153, b: 0);
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
			ForegroundColor = Color.FromRgb(r: 76, g: 255, b: 79);
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
			ForegroundColor = Color.FromRgb(r: 255, g: 79, b: 255);
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
			ForegroundColor = Color.FromRgb(r: 255, g: 153, b: 153);
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
			ForegroundColor = Color.FromRgb(r: 1, g: 153, b: 153);
            DisplayName = Labels.BQLConstantEndingFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}
}
