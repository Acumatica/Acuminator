using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;


namespace Acuminator.Vsix.Coloriser
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_1_Format)]
    [Name(ColoringConstants.BraceLevel_1_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BQLConstantEndingFormat)]
    internal sealed class BraceLevel_1_Format : EditorFormatBase
    {
        public BraceLevel_1_Format()
        {
            DisplayName = VSIXResource.BraceLevel_1_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_2_Format)]
    [Name(ColoringConstants.BraceLevel_2_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_1_Format)]
    internal sealed class BraceLevel_2_Format : EditorFormatBase
    {
        public BraceLevel_2_Format()
        {
            DisplayName = VSIXResource.BraceLevel_2_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_3_Format)]
    [Name(ColoringConstants.BraceLevel_3_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_2_Format)]
    internal sealed class BraceLevel_3_Format : EditorFormatBase
    {
        public BraceLevel_3_Format()
        {            
            DisplayName = VSIXResource.BraceLevel_3_FormatLabel; // Human readable version of the name		
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
    internal sealed class BraceLevel_4_Format : EditorFormatBase
    {
        public BraceLevel_4_Format()
        {           
            DisplayName = VSIXResource.BraceLevel_4_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_5_Format)]
    [Name(ColoringConstants.BraceLevel_5_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_4_Format)]
    internal sealed class BraceLevel_5_Format : EditorFormatBase
    {
        public BraceLevel_5_Format()
        {        
            DisplayName = VSIXResource.BraceLevel_5_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_6_Format)]
    [Name(ColoringConstants.BraceLevel_6_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_5_Format)]
    internal sealed class BraceLevel_6_Format : EditorFormatBase
    {
        public BraceLevel_6_Format()
        {            
            DisplayName = VSIXResource.BraceLevel_6_FormatLabel; // Human readable version of the name		
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
    internal sealed class BraceLevel_7_Format : EditorFormatBase
    {
        public BraceLevel_7_Format()
        {
            DisplayName = VSIXResource.BraceLevel_7_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_8_Format)]
    [Name(ColoringConstants.BraceLevel_8_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_7_Format)]
    internal sealed class BraceLevel_8_Format : EditorFormatBase
    {
        public BraceLevel_8_Format()
        {
            DisplayName = VSIXResource.BraceLevel_8_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_9_Format)]
    [Name(ColoringConstants.BraceLevel_9_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_8_Format)]
    internal sealed class BraceLevel_9_Format : EditorFormatBase
    {
        public BraceLevel_9_Format()
        {
            DisplayName = VSIXResource.BraceLevel_9_FormatLabel; // Human readable version of the name		
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
    internal sealed class BraceLevel_10_Format : EditorFormatBase
    {
        public BraceLevel_10_Format()
        {
            DisplayName = VSIXResource.BraceLevel_10_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_11_Format)]
    [Name(ColoringConstants.BraceLevel_11_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_10_Format)]
    internal sealed class BraceLevel_11_Format : EditorFormatBase
    {
        public BraceLevel_11_Format()
        {
            DisplayName = VSIXResource.BraceLevel_11_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_12_Format)]
    [Name(ColoringConstants.BraceLevel_12_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_11_Format)]
    internal sealed class BraceLevel_12_Format : EditorFormatBase
    {
        public BraceLevel_12_Format()
        {
            DisplayName = VSIXResource.BraceLevel_12_FormatLabel; // Human readable version of the name		
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
    internal sealed class BraceLevel_13_Format : EditorFormatBase
    {
        public BraceLevel_13_Format()
        {
            DisplayName = VSIXResource.BraceLevel_13_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ColoringConstants.BraceLevel_14_Format)]
    [Name(ColoringConstants.BraceLevel_14_Format)]
    [UserVisible(true)]
    [Order(After = ColoringConstants.BraceLevel_13_Format)]
    internal sealed class BraceLevel_14_Format : EditorFormatBase
    {
        public BraceLevel_14_Format()
        {
            DisplayName = VSIXResource.BraceLevel_14_FormatLabel; // Human readable version of the name		
            ForegroundCustomizable = true;
            BackgroundCustomizable = true;
        }
    }
}
