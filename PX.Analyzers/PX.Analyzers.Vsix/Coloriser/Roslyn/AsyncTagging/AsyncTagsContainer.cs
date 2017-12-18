using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;

namespace PX.Analyzers.Coloriser
{
    public class AsyncTagsContainer 
    {
        private readonly List<IClassificationTag> tags = new List<IClassificationTag>();
    }
}
