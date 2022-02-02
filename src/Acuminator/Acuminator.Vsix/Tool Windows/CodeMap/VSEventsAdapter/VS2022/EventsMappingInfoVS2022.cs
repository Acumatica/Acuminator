using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	internal abstract partial class VSEventsAdapter
	{
		private partial class VSEventsAdapterVS2022 : VSEventsAdapter
		{
			/// <summary>
			/// The events mapping information for VS 2022.
			/// </summary>
			private class EventsMappingInfoVS2022
			{
				private readonly Dictionary<string, string> _eventTypeNamesByEventNames;

				public IReadOnlyDictionary<string, string> EventTypeNamesByEventNames => _eventTypeNamesByEventNames;

				public IReadOnlyCollection<string> EventNames => _eventTypeNamesByEventNames.Keys;

				public IReadOnlyList<string> EventTypeNames { get; }

				public IReadOnlyDictionary<string, MethodInfo> EventNamesToAdapterEventHandlers { get; }

				public EventsMappingInfoVS2022()
				{
					_eventTypeNamesByEventNames = new Dictionary<string, string>
					{
						[nameof(EnvDTE.SolutionEvents.AfterClosing)] = typeof(EnvDTE.SolutionEvents).FullName,
						[nameof(EnvDTE.DocumentEvents.DocumentClosing)] = typeof(EnvDTE.DocumentEvents).FullName,
						[nameof(EnvDTE.WindowEvents.WindowActivated)] = typeof(EnvDTE.WindowEvents).FullName,
						[nameof(EnvDTE80.WindowVisibilityEvents.WindowShowing)] = typeof(EnvDTE80.WindowVisibilityEvents).FullName,
						[nameof(EnvDTE80.WindowVisibilityEvents.WindowHiding)] = typeof(EnvDTE80.WindowVisibilityEvents).FullName
					};

					EventTypeNames = _eventTypeNamesByEventNames.Values.Distinct().ToList();

					Type adapterType = typeof(VSEventsAdapter);
					BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
					EventNamesToAdapterEventHandlers = new Dictionary<string, MethodInfo>
					{
						[nameof(EnvDTE.SolutionEvents.AfterClosing)]            = adapterType.GetMethod(nameof(RaiseAfterSolutionClosingEvent), bindingFlags),
						[nameof(EnvDTE.DocumentEvents.DocumentClosing)]         = adapterType.GetMethod(nameof(RaiseDocumentClosingEvent), bindingFlags),
						[nameof(EnvDTE.WindowEvents.WindowActivated)]           = adapterType.GetMethod(nameof(RaiseWindowActivatedEvent), bindingFlags),
						[nameof(EnvDTE80.WindowVisibilityEvents.WindowShowing)] = adapterType.GetMethod(nameof(RaiseWindowShowingEvent), bindingFlags),
						[nameof(EnvDTE80.WindowVisibilityEvents.WindowHiding)]  = adapterType.GetMethod(nameof(RaiseWindowHidingEvent), bindingFlags)
					};
				}
			}
		}
	}
}
