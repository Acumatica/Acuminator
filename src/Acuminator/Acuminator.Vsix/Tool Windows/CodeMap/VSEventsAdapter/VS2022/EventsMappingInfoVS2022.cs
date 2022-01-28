using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	
    /// <summary>
    /// The events mapping information for VS 2022.
    /// </summary>
	internal class EventsMappingInfoVS2022
	{
		public IReadOnlyDictionary<string, string> EventTypeNamesByEventNames { get; }

		public IReadOnlyList<string> EventTypeNames { get; }

		public EventsMappingInfoVS2022()
		{
			EventTypeNamesByEventNames = new Dictionary<string, string>
			{
				[nameof(EnvDTE.SolutionEvents.AfterClosing)]            = typeof(EnvDTE.SolutionEvents).FullName,
				[nameof(EnvDTE.DocumentEvents.DocumentClosing)]         = typeof(EnvDTE.DocumentEvents).FullName,
				[nameof(EnvDTE.WindowEvents.WindowActivated)]           = typeof(EnvDTE.WindowEvents).FullName,
				[nameof(EnvDTE80.WindowVisibilityEvents.WindowShowing)] = typeof(EnvDTE80.WindowVisibilityEvents).FullName,
				[nameof(EnvDTE80.WindowVisibilityEvents.WindowHiding)]  = typeof(EnvDTE80.WindowVisibilityEvents).FullName
			};

			EventTypeNames = EventTypeNamesByEventNames.Values.Distinct().ToList();
		}
	}
}
