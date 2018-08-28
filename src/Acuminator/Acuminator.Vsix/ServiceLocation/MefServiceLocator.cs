using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;

namespace Acuminator.Vsix.ServiceLocation
{
	class MefServiceLocator : ServiceLocatorImplBase
	{
		private readonly ExportProvider _provider;

		public MefServiceLocator(ExportProvider provider)
		{
			_provider = provider;
		}

		protected override object DoGetInstance(Type serviceType, string key)
		{
			var export = _provider.GetExports(serviceType, null, key).FirstOrDefault();

			if (export != null)
				return export.Value;

			throw new ActivationException($"Could not locate any instances of contract {key} for service type {serviceType.FullName}");
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			return _provider.GetExports(serviceType, null, null).Select(export => export.Value);
		}
	}
}
