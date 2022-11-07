using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(FileId), FileId);
			info.AddValue(nameof(ViewName), ViewName);

			base.GetObjectData(info, context);
		}

		private bool Foo() => true;
	}
}
