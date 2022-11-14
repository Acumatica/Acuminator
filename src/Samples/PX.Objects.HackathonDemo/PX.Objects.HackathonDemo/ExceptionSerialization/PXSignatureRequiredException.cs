using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PX.Data;

namespace PX.Objects.HackathonDemo.ExceptionSerialization
{
	public sealed class PXSignatureRequiredException : PXBaseRedirectException
	{
		/// <summary>
		/// Identificator of the document that should be signed
		/// </summary>
		public Guid FileId { get; private set; }
		/// <summary>
		/// View name from which sign action is called
		/// </summary>
		public string ViewName { get; private set; }
		/// <summary>
		/// Connected graph
		/// </summary>
		public PXGraph Graph { get; private set; }

		public PXSignatureRequiredException(PXGraph graph, string viewName, Guid fileId) : base(fileId.ToString())
		{
			Graph = graph;
			ViewName = viewName;
			FileId = fileId;
		}
	}
}
