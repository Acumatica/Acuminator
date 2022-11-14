using System;
using PX.Data;
using System.Runtime.Serialization;
using PX.Common;

namespace Acuminator.Tests.Sources
{
	public class DetailNonLocalizableBypassedException : PXException
	{
		private const string MessageConst = "Non Localizable String, Ups!";

		private static string MessageField = "Non Localizable String, Ups!";

		private static string MessageProperty { get; } = "Non Localizable String, Ups!";

		private static string MessageMethod() => "Non Localizable String, Ups!";

		public object ItemToBypass { get; }

		public DetailNonLocalizableBypassedException(object itemToBypass)
			: base(MessageConst)
		{
			ItemToBypass = itemToBypass;
		}

		public DetailNonLocalizableBypassedException(Guid itemToBypass)
			: base(MessageField)
		{
			ItemToBypass = itemToBypass;
		}

		public DetailNonLocalizableBypassedException(string itemToBypass)
			: base(MessageProperty)
		{
			ItemToBypass = itemToBypass;
		}

		public DetailNonLocalizableBypassedException(int itemToBypass)
			: base(MessageMethod())
		{
			ItemToBypass = itemToBypass;
		}

		public DetailNonLocalizableBypassedException(PXException exception)
			: base(exception.Message)												//no alert
		{
			ItemToBypass = exception;
		}

		protected DetailNonLocalizableBypassedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			ReflectionSerializer.RestoreObjectProps(this, info);
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			ReflectionSerializer.GetObjectData(this, info);
			base.GetObjectData(info, context);
		}
	}
}