using System;

namespace PX.Objects
{
	public class NonDacClass 
	{
		public int Field = 0;

		public int Property => 0;

		public event EventHandler<EventArgs> FieldChanged;

        public NonDacClass(int field)
        {
            Field = field;
        }

		public void Foo() { }

		public class NestedType { }
	}

	public delegate void ActionWithoutDescription();

	public struct StructWithoutDescription 
	{
		public int Field;

		public int Property => 0;

		public event EventHandler<EventArgs> FieldChanged;

		public StructWithoutDescription(int field)
		{
			Field = field;
			FieldChanged = (sender, eventArgs) => Foo();
		}

		public static void Foo() { }

		public class NestedType { }
	}

	public interface InterfaceWithoutDescription 
	{
		public int Property { get; }

		public event EventHandler<EventArgs> FieldChanged;

		public void Foo();

		public class NestedType { }
	}

	public enum EnumWithoutDescription
	{
	}
}
