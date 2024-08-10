#nullable enable

namespace Acuminator.Utilities.ForbiddenApi.Model
{
	public enum ApiKind : byte
	{
		Undefined,
		Namespace,
		Type,
		Field,
		Property,
		Event,
		Method
	}
}
