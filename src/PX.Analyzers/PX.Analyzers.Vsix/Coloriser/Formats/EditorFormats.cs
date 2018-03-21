using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.PlatformUI;
using PX.Analyzers.Vsix;


namespace PX.Analyzers.Coloriser
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BQLOperatorFormat)]
    [Name(ColoringConstants.BQLOperatorFormat)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.Priority)]
    internal sealed class BQLOperatorFormat : EditorFormatBase
    {      
        public BQLOperatorFormat()
        {

            DisplayName = VSIXResource.BQLOperatorFormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;           
        }     
    }

    [Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = ColoringConstants.DacFormat)]
	[Name(ColoringConstants.DacFormat)]
	[UserVisible(true)]
    [Order(After = ColoringConstants.BQLOperatorFormat)]
    internal sealed class DACFormat : EditorFormatBase
	{
		public DACFormat()
		{
            DisplayName = VSIXResource.DacFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.DacExtensionFormat)]
    [Name(ColoringConstants.DacExtensionFormat)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.DacFormat)]
    internal sealed class DacExtensionFormat : EditorFormatBase
    {
        public DacExtensionFormat()
        {
            DisplayName = VSIXResource.DacExtensionFormatLabel;       // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = ColoringConstants.DacFieldFormat)]
	[Name(ColoringConstants.DacFieldFormat)]
	[UserVisible(true)]
    [Order(After = ColoringConstants.DacExtensionFormat)]
    internal sealed class DACFieldFormat : EditorFormatBase
	{
		public DACFieldFormat()
		{
            DisplayName = VSIXResource.DacFieldFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = ColoringConstants.BQLParameterFormat)]
	[Name(ColoringConstants.BQLParameterFormat)]
	[UserVisible(true)]
    [Order(After = ColoringConstants.DacFieldFormat)]
    internal sealed class BQLParameterFormat : EditorFormatBase
	{
		public BQLParameterFormat()
		{
            DisplayName = VSIXResource.BQLParameterFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = ColoringConstants.BQLConstantPrefixFormat)]
	[Name(ColoringConstants.BQLConstantPrefixFormat)]
	[UserVisible(true)]
	[Order(After = ColoringConstants.DacFieldFormat)]
	internal sealed class BQLConstantPrefixFormat : EditorFormatBase
	{
		public BQLConstantPrefixFormat()
		{
            DisplayName = VSIXResource.BQLConstantPrefixFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = ColoringConstants.BQLConstantEndingFormat)]
	[Name(ColoringConstants.BQLConstantEndingFormat)]
	[UserVisible(true)]
	[Order(After = ColoringConstants.BQLConstantPrefixFormat)]
	internal sealed class BQLConstantEndingFormat : EditorFormatBase
	{
		public BQLConstantEndingFormat()
		{
            DisplayName = VSIXResource.BQLConstantEndingFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
			BackgroundCustomizable = true;
		}
	}


    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.PXGraphFormat)]
    [Name(ColoringConstants.PXGraphFormat)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BQLConstantEndingFormat)]
    internal sealed class PXGraphFormat : EditorFormatBase
    {
        public PXGraphFormat()
        {
            DisplayName = VSIXResource.PXGraphFormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }


    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.PXActionFormat)]
    [Name(ColoringConstants.PXActionFormat)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.PXGraphFormat)]
    internal sealed class PXActionFormat : EditorFormatBase
    {
        public PXActionFormat()
        {
            DisplayName = VSIXResource.PXActionFormatLabel;   // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }
}
