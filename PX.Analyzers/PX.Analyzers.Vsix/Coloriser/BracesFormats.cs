using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PX.Analyzers.Coloriser
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_1_Format)]
    [Name(Constants.BraceLevel_1_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BQLConstantEndingFormat)]
    internal sealed class BraceLevel_1_Format : ClassificationFormatDefinition
    {
        public BraceLevel_1_Format()
        {
            DisplayName = Labels.BraceLevel_1_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_2_Format)]
    [Name(Constants.BraceLevel_2_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BraceLevel_1_Format)]
    internal sealed class BraceLevel_2_Format : ClassificationFormatDefinition
    {
        public BraceLevel_2_Format()
        {
            DisplayName = Labels.BraceLevel_2_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_3_Format)]
    [Name(Constants.BraceLevel_3_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BraceLevel_2_Format)]
    internal sealed class BraceLevel_3_Format : ClassificationFormatDefinition
    {
        public BraceLevel_3_Format()
        {            
            DisplayName = Labels.BraceLevel_3_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    //*********************************************************************************************************************
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_4_Format)]
    [Name(Constants.BraceLevel_4_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BraceLevel_3_Format)]
    internal sealed class BraceLevel_4_Format : ClassificationFormatDefinition
    {
        public BraceLevel_4_Format()
        {           
            DisplayName = Labels.BraceLevel_4_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_5_Format)]
    [Name(Constants.BraceLevel_5_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BraceLevel_4_Format)]
    internal sealed class BraceLevel_5_Format : ClassificationFormatDefinition
    {
        public BraceLevel_5_Format()
        {        
            DisplayName = Labels.BraceLevel_5_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_6_Format)]
    [Name(Constants.BraceLevel_6_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BraceLevel_5_Format)]
    internal sealed class BraceLevel_6_Format : ClassificationFormatDefinition
    {
        public BraceLevel_6_Format()
        {            
            DisplayName = Labels.BraceLevel_6_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    //*********************************************************************************************************************
    // 
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_7_Format)]
    [Name(Constants.BraceLevel_7_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BraceLevel_6_Format)]
    internal sealed class BraceLevel_7_Format : ClassificationFormatDefinition
    {
        public BraceLevel_7_Format()
        {
            DisplayName = Labels.BraceLevel_7_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_8_Format)]
    [Name(Constants.BraceLevel_8_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BraceLevel_7_Format)]
    internal sealed class BraceLevel_8_Format : ClassificationFormatDefinition
    {
        public BraceLevel_8_Format()
        {
            DisplayName = Labels.BraceLevel_8_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.BraceLevel_9_Format)]
    [Name(Constants.BraceLevel_9_Format)]
    [UserVisible(true)]
    [Order(After = Constants.BraceLevel_8_Format)]
    internal sealed class BraceLevel_9_Format : ClassificationFormatDefinition
    {
        public BraceLevel_9_Format()
        {
            DisplayName = Labels.BraceLevel_9_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }
}
