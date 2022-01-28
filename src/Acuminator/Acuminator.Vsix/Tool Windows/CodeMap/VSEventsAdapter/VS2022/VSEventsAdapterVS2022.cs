using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;

using Acuminator.Utilities;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;
using System.Diagnostics;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	internal abstract partial class VSEventsAdapter
	{
		/// <summary>
		/// A VS events adapter class required to support subscription on VS events for VS 2022.
		/// </summary>
		private partial class VSEventsAdapterVS2022 : VSEventsAdapter
		{
			private const int NON_INITIALIZED = 0;
			private const int INITIALIZED = 1;
			private static int _areVSServicesInitialized = NON_INITIALIZED;

			private static readonly EventsMappingInfoVS2022 _mappingInfo = new EventsMappingInfoVS2022();			
			private static readonly Dictionary<string, EventInfo> _vsEventInfos = new Dictionary<string, EventInfo>();

			private static dynamic _dte;	// DTE must be dynamic for proper runtime binding to different DTE types in different VS

			private Dictionary<string, object> _eventObjectsByEventTypeName;
			private readonly Dictionary<string, Delegate> _eventHandlers = new Dictionary<string, Delegate>();

			protected override bool TryInitialize()
			{
				InitializeAndCacheVSServicesAndTypes();

				if (_dte?.Events == null || _vsEventInfos.Count != _mappingInfo.EventNames.Count)
					return false;

				//Store reference to COM events objects from DTE to prevent them from being GC-ed and to use them in Reflection
				_eventObjectsByEventTypeName = new Dictionary<string, object>
				{
					 [typeof(EnvDTE.SolutionEvents).FullName]           = _dte.Events.SolutionEvents,
					 [typeof(EnvDTE.DocumentEvents).FullName]           = _dte.Events.DocumentEvents,
					 [typeof(EnvDTE.WindowEvents).FullName]             = _dte.Events.WindowEvents,
					 [typeof(EnvDTE80.WindowVisibilityEvents).FullName] = _dte.Events.WindowVisibilityEvents,
				};

				return _eventObjectsByEventTypeName.Values.All(eventObj => eventObj != null);
			}

			private static void InitializeAndCacheVSServicesAndTypes()
			{
				if (Interlocked.CompareExchange(ref _areVSServicesInitialized, INITIALIZED, NON_INITIALIZED) == NON_INITIALIZED)
				{
					Assembly interopAssembly = GetInteropAssembly();

					if (interopAssembly == null)
						return;

					_dte = GetDTE(interopAssembly);
	
					FillVSEventInfos(interopAssembly);
				}
			}

			private static Assembly GetInteropAssembly()
			{
				const string dteAssemblyVS2022 = "Microsoft.VisualStudio.Interop";
				return AppDomain.CurrentDomain.GetAssemblies()
											  .FirstOrDefault(assembly => assembly.GetName().Name == dteAssemblyVS2022);
			}

			private static object GetDTE(Assembly interopAssembly)
			{
				string dteTypeName = typeof(EnvDTE.DTE).FullName;
				Type dteType = interopAssembly.ExportedTypes.FirstOrDefault(t => t.FullName == dteTypeName);

				if (dteType == null)
					return null;

				var serviceProvider = AcuminatorVSPackage.Instance as IServiceProvider;
				return serviceProvider.GetService(dteType);
			}

			private static void FillVSEventInfos(Assembly interopAssembly)
			{
				Dictionary<string, Type> vsEventTypes = GetEventTypesForVS2022(interopAssembly);

				foreach (string eventName in _mappingInfo.EventNames)
				{
					string vsEventTypeName = _mappingInfo.EventTypeNamesByEventNames[eventName];

					if (!vsEventTypes.TryGetValue(vsEventTypeName, out Type eventTypeVS2022))
						continue;

					EventInfo eventInfo = GetEventInfoFromComWrapperType(eventTypeVS2022, eventName);

					if (eventInfo != null)
					{
						_vsEventInfos.Add(eventName, eventInfo);
					}
				}
			}

			private static Dictionary<string, Type> GetEventTypesForVS2022(Assembly interopAssembly) =>
				 interopAssembly.ExportedTypes
								.Where(typeVS2022 => _mappingInfo.EventTypeNames.Contains(typeVS2022.FullName))
								.ToDictionary(type => type.FullName);

			private static EventInfo GetEventInfoFromComWrapperType(Type eventType, string eventName)
			{
				EventInfo eventInfo = eventType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);

				if (eventInfo != null)
					return eventInfo;

				var interfaceTypes = eventType.GetInterfaces();

				foreach (Type interfaceType in interfaceTypes)
				{
					eventInfo = interfaceType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);

					if (eventInfo != null)
						return eventInfo;
				}

				return null;
			}

			protected override bool TrySubscribeAdapterOnVSEvents()
			{
				bool subscribedAll = true;

				foreach (string eventName in _mappingInfo.EventNames)
				{		
					subscribedAll = TrySubscribeOnEvent(eventName) && subscribedAll;
				}
				
				return subscribedAll;
			}

			private bool TrySubscribeOnEvent(string eventName)
			{
				string eventTypeName = _mappingInfo.EventTypeNamesByEventNames[eventName];

				if (!_mappingInfo.EventNamesToAdapterEventHandlers.TryGetValue(eventName, out MethodInfo adapterHandlerMethodInfo) ||
					!_eventObjectsByEventTypeName.TryGetValue(eventTypeName,out object eventObject) ||
					!_vsEventInfos.TryGetValue(eventName, out EventInfo eventInfo))
				{
					return false;
				}

				// Following objects should not be null. Null will indicate a corrupted state so ArgumentNullException will be thrown here. 
				// If these ogjects are null the adapter mechanism will log ArgumentNullException in VS ActivityLog
				adapterHandlerMethodInfo.ThrowOnNull(nameof(adapterHandlerMethodInfo));
				eventObject.ThrowOnNull(nameof(eventObject));
				eventInfo.ThrowOnNull(nameof(eventInfo));	

				try
				{
					Delegate eventHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType, firstArgument: this, method: adapterHandlerMethodInfo);

					if (eventHandler == null)
						return false;

					eventInfo.AddEventHandler(eventObject, eventHandler);
					_eventHandlers.Add(eventName, eventHandler);
					return true;
				}
				catch (Exception e)
				{
					AcuminatorVSPackage.Instance.AcuminatorLogger.LogException(e, logOnlyFromAcuminatorAssemblies: true, LogMode.Error);
					return false;
				}		
			}

			public override bool TryUnsubscribeAdapterFromVSEvents()
			{
				bool unsubscribedAll = true;

				foreach (string eventName in _mappingInfo.EventNames)
				{
					unsubscribedAll = TryUnsubscribeFromEvent(eventName) && unsubscribedAll;
				}

				return unsubscribedAll;
			}

			private bool TryUnsubscribeFromEvent(string eventName)
			{
				string eventTypeName = _mappingInfo.EventTypeNamesByEventNames[eventName];

				if (!_eventObjectsByEventTypeName.TryGetValue(eventTypeName, out object eventObject) ||
					!_vsEventInfos.TryGetValue(eventName, out EventInfo eventInfo) ||
					!_eventHandlers.TryGetValue(eventName, out Delegate eventHandler))
				{
					return false;
				}

				if (eventObject == null || eventInfo == null || eventHandler == null)
					return false;

				try
				{
					eventInfo.RemoveEventHandler(eventObject, eventHandler);
					_eventHandlers.Remove(eventName);
					return true;
				}
				catch (Exception e)
				{
					AcuminatorVSPackage.Instance.AcuminatorLogger.LogException(e, logOnlyFromAcuminatorAssemblies: true, LogMode.Error);
					return false;
				}

			}
		}
	}
}
