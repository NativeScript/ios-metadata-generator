using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libclang.Core.Common
{
    public class PlatformAvailability
    {
        /// <summary>
        /// A string that describes the platform for which this structure
        /// provides availability information.
        /// </summary>
        /// <remarks>
        /// Possible values are "ios" or "macosx".
        /// </remarks>
        public string Platform { get; private set; }

        /// <summary>
        /// The version number in which this entity was introduced.
        /// </summary>
        public Version Introduced { get; private set; }

        /// <summary>
        /// The version number in which this entity was deprecated (but is
        /// still available).
        /// </summary>
        public Version Deprecated { get; private set; }

        /// <summary>
        /// The version number in which this entity was obsoleted, and therefore
        /// is no longer available.
        /// </summary>
        public Version Obsoleted { get; private set; }

        /// <summary>
        /// Whether the entity is unconditionally unavailable on this platform.
        /// </summary>
        public bool IsUnavailable { get; private set; }

        /// <summary>
        /// An optional message to provide to a user of this API, e.g., to
        /// suggest replacement APIs.
        /// </summary>
        public string Message { get; private set; }

        public override string ToString()
        {
            return
                string.Format(
                    "Platform: {0} Introduced: {1} Deprecated: {2} Obsoleted: {3} IsUnavailable: {4} Message: {5}",
                    this.Platform, this.Introduced, this.Deprecated, this.Obsoleted, this.IsUnavailable, this.Message);
        }

        public static implicit operator PlatformAvailability(NClang.ClangPlatformAvailability availability)
        {
            if (availability == null)
                return null;

            PlatformAvailability newAvailability = new PlatformAvailability()
            {
                Platform = availability.Platform,
                Introduced = availability.Introduced,
                Deprecated = availability.Deprecated,
                Obsoleted = availability.Obsoleted,
                IsUnavailable = availability.IsUnavailable,
                Message = availability.Message
            };
            return newAvailability;
        }
    }
}
