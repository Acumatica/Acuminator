#nullable enable

using System;
using System.Runtime.Serialization;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.ForbiddenApi.Errors
{
	/// <summary>
	/// A special exception class for reading api.
	/// </summary>
	[Serializable]
	public sealed class ApiReaderException : Exception
	{
		private const string DefaultError = "An error happened during the reading of the API list.";

		public ApiReaderException() : base(DefaultError)
		{
		}

		public ApiReaderException(string? message) : base(message.NullIfWhiteSpace() ?? DefaultError)
		{
		}

		public ApiReaderException(string? message, Exception innerException) : base(message.NullIfWhiteSpace() ?? DefaultError, innerException)
		{
		}

		private ApiReaderException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
