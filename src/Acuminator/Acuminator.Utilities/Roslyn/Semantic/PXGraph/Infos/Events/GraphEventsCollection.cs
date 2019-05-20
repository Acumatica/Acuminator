using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	internal class GraphEventsCollection<TEventInfoType> : Dictionary<string, TEventInfoType>
	where TEventInfoType : GraphEventInfoBase<TEventInfoType>
	{
		public IEnumerable<TEventInfoType> Items => Values;

		public GraphEventsCollection() : base()
		{
		}

		public void AddEventInfo(string key, TEventInfoType eventInfo)
		{
			if (TryGetValue(key, out TEventInfoType existingEventInfo))
			{
				if (!existingEventInfo.Equals(eventInfo))
				{
					eventInfo.SetBaseEvent(existingEventInfo);
					base[key] = eventInfo;
				}
			}
			else
			{
				Add(key, eventInfo);
			}
		}
	}
}
