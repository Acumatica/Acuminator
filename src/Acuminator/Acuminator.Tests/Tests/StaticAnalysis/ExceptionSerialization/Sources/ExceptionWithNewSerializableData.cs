using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.ExceptionSerialization.Sources
{
	public sealed class PXNewSerializableAutoPropertiesException : PXBaseRedirectException
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

		public PXNewSerializableAutoPropertiesException(PXGraph graph, string viewName, Guid fileId) : base(fileId.ToString())
		{
			Graph = graph;
			ViewName = viewName;
			FileId = fileId;
		}
	}

	public sealed class PXNewSerializableFieldsException : PXBaseRedirectException
	{
		/// <summary>
		/// Identificator of the document that should be signed
		/// </summary>
		public Guid FileId;
		/// <summary>
		/// View name from which sign action is called
		/// </summary>
		public string ViewName;
		/// <summary>
		/// Connected graph
		/// </summary>
		public PXGraph Graph;

		public PXNewSerializableFieldsException(PXGraph graph, string viewName, Guid fileId) : base(fileId.ToString())
		{
			Graph = graph;
			ViewName = viewName;
			FileId = fileId;
		}
	}
}
