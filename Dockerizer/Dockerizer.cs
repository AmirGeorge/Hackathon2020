using Dockerizer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dockerizer
{
    public class Dockerizer
    {
        /// <summary>
        /// Dockerize.
        /// </summary>
        /// <param name="projectPath">Example: C:\Work\Hackathon2020\Hackathon2020\AspNetCoreApp</param>
        public void Dockerize(string projectPath)
        {
            if (!Directory.Exists(projectPath))
            {
                throw new Exception($"Invalid project path: {projectPath}");
            }

            var csprojFilePaths = Directory.GetFiles(projectPath, "*.csproj");
            
            if (csprojFilePaths.Length == 0)
            {
                throw new Exception($"No csproj files found in {projectPath}");
            }

            if (csprojFilePaths.Length > 1)
            {
                throw new Exception($"More than 1 csproj file found in {projectPath}");
            }

            var csprojFilePath = csprojFilePaths[0];

            var csprojFileContent = File.ReadAllLines(csprojFilePath);
            var targetFrameowork = csprojFileContent
                .Single(l => Regex.IsMatch(l, "<TargetFramework>.+</TargetFramework>"))
                .Trim()
                .Replace("<TargetFramework>", string.Empty)
                .Replace("</TargetFramework>", string.Empty);

            if (targetFrameowork != DockerizerConstants.NetCoreApp31)
            {
                throw new NotSupportedException($"Unsupported target framework {targetFrameowork}");
            }

            var baseImage = DockerizerConstants.BaseImages[targetFrameowork];
            var buildImage = DockerizerConstants.BuildImages[targetFrameowork];

            var projectName = csprojFilePath
                .Replace(projectPath, string.Empty)
                .Replace("\\", string.Empty)
                .Replace("/", string.Empty)
                .Replace(".csproj", string.Empty);

            var dockerfileContent = File.ReadAllText("../../../Helpers/DockerfileTemplate");
            dockerfileContent = dockerfileContent
                .Replace(DockerizerConstants.BaseImagePlaceholder, baseImage)
                .Replace(DockerizerConstants.BuildImagePlaceholder, buildImage)
                .Replace(DockerizerConstants.ProjectNamePlaceholder, projectName);

            var dockerfileOutputPath = Path.Combine(projectPath, "Dockerfile");
            File.WriteAllText(dockerfileOutputPath, dockerfileContent);
        }
    }
}
