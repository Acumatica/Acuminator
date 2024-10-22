#nullable enable
using Acuminator.Utilities.BannedApi.Model;

using Xunit;
using FluentAssertions;

namespace Acuminator.Tests.Tests.StaticAnalysis.BannedApi
{
	public class ApiParsingTests
	{
		[Theory]
		[InlineData("T:System-AppDomain ISV Usage of the AppDomain type is forbidden in Acumatica customizations.", "System.AppDomain",
					ApiKind.Type, ApiBanKind.ISV, "Usage of the AppDomain type is forbidden in Acumatica customizations.")]
		[InlineData("N:System.Data.Entity ISV Usage of this namespace is forbidden in Acumatica customizations.", "",
					ApiKind.Namespace, ApiBanKind.ISV, "Usage of this namespace is forbidden in Acumatica customizations.")]
		[InlineData("M:System-Math.Round(System.Double,System.Int32)" + 
					" General Math.Round uses Banker's Rounding by default, which rounds to the closest even number. Usually, this is not the desired rounding behavior." + 
					" Use the Math.Round overload with the MidpointRounding parameter to explicitly specify the desired rounding behavior.",
					"System.Math", ApiKind.Method, ApiBanKind.General, 
					"Math.Round uses Banker's Rounding by default, which rounds to the closest even number. Usually, this is not the desired rounding behavior." + 
					" Use the Math.Round overload with the MidpointRounding parameter to explicitly specify the desired rounding behavior.")]
		[InlineData("M:System.Linq-ParallelEnumerable.AsParallel``1(System.Collections.Generic.IEnumerable{``0}) General Usage of Parallel LINQ is forbidden in Acumatica" + 
					" because it may lead to deadlocks and a loss of Acumatica synchronization context. It also prevents Acumatica request profiler" + 
					" and resource governor from correctly managing system resources.",
					"System.Linq.ParallelEnumerable", ApiKind.Method, ApiBanKind.General,
					"Usage of Parallel LINQ is forbidden in Acumatica because it may lead to deadlocks and a loss of Acumatica synchronization context." + 
					" It also prevents Acumatica request profiler and resource governor from correctly managing system resources.")]
		public void Api_Creation_FromRawString(string rawApiData, string expectedFullTypeName, ApiKind expectedApiKind,
											   ApiBanKind expectedBanKind, string expectedBanReason)
		{
			var parsedApi = new Api(rawApiData);

			parsedApi.RawApiData.Should().Be(rawApiData);
			parsedApi.FullTypeName.Should().Be(expectedFullTypeName);
			parsedApi.Kind.Should().Be(expectedApiKind);
			parsedApi.BanKind.Should().Be(expectedBanKind);
			parsedApi.BanReason.Should().Be(expectedBanReason);
		}
	}
}
