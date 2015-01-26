using System;
using System.Text;

namespace Libclang.Core.Common
{
    public class Version
    {
        public Version()
        {
        }

        public Version(int major, int minor, int subminor)
        {
            this.Major = major;
            this.Minor = minor;
            this.SubMinor = subminor;
        }

        /// <summary>
        /// The major version number, e.g., the '10' in '10.7.3'. A negative
        /// value indicates that there is no version number at all.
        /// </summary>
        public int Major { get; private set; }

        /// <summary>
        /// The minor version number, e.g., the '7' in '10.7.3'. This value
        /// will be negative if no minor version number was provided, e.g., for 
        /// version '10'.
        /// </summary>
        public int Minor { get; private set; }

        /// <summary>
        /// The subminor version number, e.g., the '3' in '10.7.3'. This value
        /// will be negative if no minor or subminor version number was provided,
        /// e.g., in version '10' or '10.7'.
        /// </summary>
        public int SubMinor { get; private set; }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            if (Major >= 0)
            {
                text.Append(Major);
            }
            if (Minor >= 0)
            {
                text.Append(".");
                text.Append(Minor);
            }
            if (SubMinor >= 0)
            {
                text.Append(".");
                text.Append(SubMinor);
            }
            return text.ToString();
        }

        public static implicit operator Version(NClang.ClangVersion version)
        {
            Version newVersion = new Version(version.Major, version.Minor, version.SubMinor);
            return newVersion;
        }

        public static bool IsSet(Version version)
        {
            return version != null && version.Major >= 0;
        }
    }
}
