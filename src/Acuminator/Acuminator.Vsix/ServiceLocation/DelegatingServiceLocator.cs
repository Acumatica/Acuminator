using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;

namespace Acuminator.Vsix.ServiceLocation
{
	class DelegatingServiceLocator : IServiceLocator
	{
		private readonly IEnumerable<IServiceLocator> _innerLocators;

		/// <param name="inner">Inner service locators, ordered by priority</param>
		public DelegatingServiceLocator(params IServiceLocator[] inner)
		{
			_innerLocators = inner;
		}

		public object GetService(Type serviceType)
		{
			return Get(inner => inner.GetService(serviceType));
		}

		public object GetInstance(Type serviceType)
		{
			return Get(inner => inner.GetInstance(serviceType));
		}

		public object GetInstance(Type serviceType, string key)
		{
			return Get(inner => inner.GetInstance(serviceType, key));
		}

		public IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return GetAll(inner => inner.GetAllInstances(serviceType));
		}

		public TService GetInstance<TService>()
		{
			return Get(inner => inner.GetInstance<TService>());
		}

		public TService GetInstance<TService>(string key)
		{
			return Get(inner => inner.GetInstance<TService>(key));
		}

		public IEnumerable<TService> GetAllInstances<TService>()
		{
			return GetAll(inner => inner.GetAllInstances<TService>());
		}


		private T Get<T>(Func<IServiceLocator, T> getter)
		{
			var exceptions = new List<Exception>();

			foreach (IServiceLocator locator in _innerLocators)
			{
				try
				{
					T instance = getter(locator);
					if (instance != null)
						return instance;
				}
				catch (ActivationException ex)
				{
					exceptions.Add(ex);
				}
			}

			throw new ActivationException("An error occured during the service resolution. Please see InnerException property for more details.",
				new AggregateException(exceptions));
		}

		private IEnumerable<T> GetAll<T>(Func<IServiceLocator, IEnumerable<T>> getter)
		{
			foreach (IServiceLocator locator in _innerLocators)
			{
				var instances = getter(locator)?.ToArray();
				if (instances != null && instances.Length > 0)
					return instances;
			}

			return Enumerable.Empty<T>();
		}
	}
}
