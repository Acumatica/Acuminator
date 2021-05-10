#nullable enable

using System;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities
{
    /// <summary>
    /// The Visual Studio version information.
    /// </summary>
    public class VSVersion
	{
        public const int UnknownVersion = 0;

        public const int VS2005 = 8;
        public const int VS2008 = 9;
        public const int VS2010 = 10;
        public const int VS2012 = 11;
        public const int VS2013 = 12;
        public const int VS2015 = 14;
        public const int VS2017 = 15;
        public const int VS2019 = 16;

		public Version FullVersion { get; }

        public bool VS2013OrOlder => FullVersion.Major <= VS2013 && FullVersion.Major != UnknownVersion; 

        public bool VS2015OrOlder => FullVersion.Major <= VS2015 && FullVersion.Major != UnknownVersion;

        public bool VS2017OrNewer => FullVersion.Major >= VS2017;

        public bool VS2019OrNewer => FullVersion.Major >= VS2019;

        public bool IsUnknownVersion => FullVersion.Major == UnknownVersion;

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
