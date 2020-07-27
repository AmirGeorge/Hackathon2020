using System;
using System.Collections.Generic;
using System.Text;

namespace Dockerizer.Helpers
{
    public static class DockerizerConstants
    {
        public const string NetCoreApp31 = "netcoreapp3.1";

        public const string BaseImagePlaceholder = "{{BaseImage}}";
        public const string BuildImagePlaceholder = "{{BuildImage}}";
        public const string ProjectNamePlaceholder = "{{ProjectName}}";

        public static readonly Dictionary<string, string> BaseImages = new Dictionary<string, string>
        {
            { NetCoreApp31, "mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim" }
        };

        public static readonly Dictionary<string, string> BuildImages = new Dictionary<string, string>
        {
            { NetCoreApp31, "mcr.microsoft.com/dotnet/core/sdk:3.1-buster" }
        };
    }
}
