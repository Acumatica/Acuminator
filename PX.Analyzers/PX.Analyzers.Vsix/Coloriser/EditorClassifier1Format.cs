//------------------------------------------------------------------------------
// <copyright file="EditorClassifier1Format.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace VSIXProjectTest
{
	/// <summary>
	/// Defines an editor format for the EditorClassifier1 type that has a purple background
	/// and is underlined.
	/// </summary>
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "EditorClassifier1")]
	[Name("EditorClassifier1")]
	[UserVisible(true)] // This should be visible to the end user
	[Order(After = "High Priority")] // Set the priority to be after the default classifiers
	internal sealed class EditorClassifier1Format : ClassificationFormatDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorClassifier1Format"/> class.
		/// </summary>
		public EditorClassifier1Format()
		{
			this.DisplayName = "EditorClassifier1"; // Human readable version of the name
			this.ForegroundColor = Colors.BurlyWood;
			this.BackgroundColor = Colors.White;
			this.ForegroundCustomizable = true;
		}
	}
}
