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
		}
	}

	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = Constants.DacFieldFormat)]
	[Name(Constants.DacFieldFormat)]
	[UserVisible(true)]
	[Order(After = Priority.Default)]
	internal sealed class DACFieldFormat : ClassificationFormatDefinition
	{
		public DACFieldFormat()
		{
			DisplayName = Labels.DacFieldFormatLabel;
			ForegroundColor = Colors.DarkGreen;
			BackgroundColor = Colors.White;
			DisplayName = Labels.DacFieldFormatLabel; // Human readable version of the name		
			ForegroundCustomizable = true;
		}
	}

	//[Export(typeof(EditorFormatDefinition))]
	//[ClassificationType(ClassificationTypeNames = Constants.ExtensionMethodFormat)]
	//[Name(Constants.ExtensionMethodFormat)]
	//[UserVisible(true)]
	//[Order(After = Priority.Default)]
	//internal sealed class RoslynExtensionMethodFormat : ClassificationFormatDefinition
	//{
	//	public RoslynExtensionMethodFormat()
	//	{
	//		this.DisplayName = "Roslyn Extension Method";
	//		this.IsItalic = true;
	//	}
	//}
}
