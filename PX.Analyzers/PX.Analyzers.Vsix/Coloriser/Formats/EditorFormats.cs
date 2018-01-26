using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.Windows;

namespace PX.Analyzers.Coloriser
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BQLOperatorFormat)]
    [Name(ColoringConstants.BQLOperatorFormat)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.Priority)]
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
	[ClassificationType(ClassificationTypeNames = ColoringConstants.DacFormat)]
	[Name(ColoringConstants.DacFormat)]
	[UserVisible(true)]
    [Order(After = ColoringConstants.BQLOperatorFormat)]
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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.DacExtensionFormat)]
    [Name(ColoringConstants.DacExtensionFormat)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.DacFormat)]
    internal sealed class DacExtensionFormat : ClassificationFormatDefinition
    {
        public DacExtensionFormat()
        {
            ForegroundColor = Color.FromRgb(r: 255, g: 255, b: 78);
            DisplayName = Labels.DacExtensionFormatLabel;       // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = ColoringConstants.DacFieldFormat)]
	[Name(ColoringConstants.DacFieldFormat)]
	[UserVisible(true)]
    [Order(After = ColoringConstants.DacExtensionFormat)]
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
	[ClassificationType(ClassificationTypeNames = ColoringConstants.BQLParameterFormat)]
	[Name(ColoringConstants.BQLParameterFormat)]
	[UserVisible(true)]
    [Order(After = ColoringConstants.DacFieldFormat)]
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
	[ClassificationType(ClassificationTypeNames = ColoringConstants.BQLConstantPrefixFormat)]
	[Name(ColoringConstants.BQLConstantPrefixFormat)]
	[UserVisible(true)]
	[Order(After = ColoringConstants.DacFieldFormat)]
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
	[ClassificationType(ClassificationTypeNames = ColoringConstants.BQLConstantEndingFormat)]
	[Name(ColoringConstants.BQLConstantEndingFormat)]
	[UserVisible(true)]
	[Order(After = ColoringConstants.BQLConstantPrefixFormat)]
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
