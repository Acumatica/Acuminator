using System;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.ExceptionSerialization.Sources
{
	[Serializable]
	internal class PXWorkspaceNotExistsException : PXException      // Show PX1063 but not PX1064
	{
		public PXWorkspaceNotExistsException(Guid workspaceId)
			 : base(string.Format("The {0} workspace does not exist or is not available for the current user.", workspaceId))
		{
		}
	}

	public class PXConstantAndStaticDataException : PXException     // Show PX1063 but not PX1064
	{
		public const string MessageConst = "Test";

		public static int StaticField = 3;

		public static string StaticAutoProperty { get; set; } = "Test";

		public static string StaticCalcedProperty => "Test";

		public PXConstantAndStaticDataException(Guid workspaceId)
			 : base(MessageConst)
		{
		}
	}

	public class PXCalcedPropertyException : PXException	// Show PX1063 but not PX1064
	{
		public static int StaticField = 3;

		public string CalcedProperty1 => StaticField.ToString();

		public int CalcedProperty2
		{ 
			get 
			{
				return 3;
			}
		}

		public int CalcedProperty3
		{
			get => 5;
		}

		public PXCalcedPropertyException(Guid workspaceId)
			 : base("Test123")
		{
		}
	}

	public class PXNonSerializedNewDataException : PXException        // Show PX1063 but not PX1064
	{
		[NonSerialized]
		public int NonSerializedField = 3;

		[field: NonSerialized]
		public int NonSerializedAutoProperty { get; set; }

		public int CalcedProperty
		{
			get 
			{
				return NonSerializedField;
			}
			set 
			{
				NonSerializedField = value;
			}
		}

		public PXNonSerializedNewDataException(Guid workspaceId)
			 : base("Test123")
		{
		}
	}
}
