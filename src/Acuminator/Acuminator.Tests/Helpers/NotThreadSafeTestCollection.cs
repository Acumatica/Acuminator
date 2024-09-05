#nullable enable

using System;

using Xunit;

namespace Acuminator.Tests.Helpers
{
	[CollectionDefinition(nameof(NotThreadSafeTestCollection), DisableParallelization = true)]
	public class NotThreadSafeTestCollection
	{
	}
}
