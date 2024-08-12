#nullable enable

namespace Acuminator.Utilities.BannedApi.Model
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
