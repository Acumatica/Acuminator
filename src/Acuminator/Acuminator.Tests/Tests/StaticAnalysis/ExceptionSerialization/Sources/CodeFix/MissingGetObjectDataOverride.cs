using System;
using System.Collections.Generic;
using System.Linq;

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

		public PXNewSerializableAutoPropertiesException(string viewName, Guid fileId) : base(fileId.ToString())
		{
			ViewName = viewName;
			FileId = fileId;
		}

		private PXNewSerializableAutoPropertiesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			FileId = (Guid)info.GetValue(nameof(FileId), typeof(Guid));
			ViewName = info.GetString(nameof(ViewName));
		}

		private bool Foo() => true;
	}
}
