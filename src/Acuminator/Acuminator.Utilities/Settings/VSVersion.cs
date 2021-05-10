#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
    /// <summary>
    /// The Visual Studio version information.
    /// </summary>
    public class VSVersion
	{
        private const int VS2005 = 8;
        private const int VS2008 = 9;
        private const int VS2010 = 10;
        private const int VS2012 = 11;
        private const int VS2013 = 12;
        private const int VS2015 = 14;
        private const int VS2017 = 15;
        private const int VS2019 = 16;

		public Version FullVersion { get; }

        public bool VS2013OrOlder => FullVersion.Major <= VS2013; 

        public bool VS2015OrOlder => FullVersion.Major <= VS2015;

        public bool VS2017OrNewer => FullVersion.Major >= VS2017;

        public bool VS2019OrNewer => FullVersion.Major >= VS2019;

        public bool IsVs2005 => FullVersion.Major == VS2005; 
        
        public bool IsVs2008 => FullVersion.Major == VS2008; 
        
        public bool IsVs2010 => FullVersion.Major == VS2010; 
        
        public bool IsVs2012 => FullVersion.Major == VS2012;

        public bool IsVS2013 => FullVersion.Major == VS2013;

        public bool IsVS2015 => FullVersion.Major == VS2015;

        public bool IsVS2017 => FullVersion.Major == VS2017;

        public bool IsVS2019 => FullVersion.Major == VS2019;

        public VSVersion(Version version)
		{
            FullVersion = version.CheckIfNull(nameof(version));
		}   
    }
}
