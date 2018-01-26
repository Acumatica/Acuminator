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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_1_Format)]
    [Name(ColoringConstants.BraceLevel_1_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BQLConstantEndingFormat)]
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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_2_Format)]
    [Name(ColoringConstants.BraceLevel_2_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_1_Format)]
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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_3_Format)]
    [Name(ColoringConstants.BraceLevel_3_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_2_Format)]
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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_4_Format)]
    [Name(ColoringConstants.BraceLevel_4_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_3_Format)]
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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_5_Format)]
    [Name(ColoringConstants.BraceLevel_5_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_4_Format)]
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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_6_Format)]
    [Name(ColoringConstants.BraceLevel_6_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_5_Format)]
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
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_7_Format)]
    [Name(ColoringConstants.BraceLevel_7_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_6_Format)]
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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_8_Format)]
    [Name(ColoringConstants.BraceLevel_8_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_7_Format)]
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
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_9_Format)]
    [Name(ColoringConstants.BraceLevel_9_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_8_Format)]
    internal sealed class BraceLevel_9_Format : ClassificationFormatDefinition
    {
        public BraceLevel_9_Format()
        {
            DisplayName = Labels.BraceLevel_9_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    //********************************************************************************************************************* 
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_10_Format)]
    [Name(ColoringConstants.BraceLevel_10_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_9_Format)]
    internal sealed class BraceLevel_10_Format : ClassificationFormatDefinition
    {
        public BraceLevel_10_Format()
        {
            DisplayName = Labels.BraceLevel_10_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_11_Format)]
    [Name(ColoringConstants.BraceLevel_11_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_10_Format)]
    internal sealed class BraceLevel_11_Format : ClassificationFormatDefinition
    {
        public BraceLevel_11_Format()
        {
            DisplayName = Labels.BraceLevel_11_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_12_Format)]
    [Name(ColoringConstants.BraceLevel_12_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_11_Format)]
    internal sealed class BraceLevel_12_Format : ClassificationFormatDefinition
    {
        public BraceLevel_12_Format()
        {
            DisplayName = Labels.BraceLevel_12_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    //********************************************************************************************************************* 
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_13_Format)]
    [Name(ColoringConstants.BraceLevel_13_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_12_Format)]
    internal sealed class BraceLevel_13_Format : ClassificationFormatDefinition
    {
        public BraceLevel_13_Format()
        {
            DisplayName = Labels.BraceLevel_13_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_14_Format)]
    [Name(ColoringConstants.BraceLevel_14_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_13_Format)]
    internal sealed class BraceLevel_14_Format : ClassificationFormatDefinition
    {
        public BraceLevel_14_Format()
        {
            DisplayName = Labels.BraceLevel_14_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }
}
