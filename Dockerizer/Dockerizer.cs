using Dockerizer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Dockerizer
{
    public class Dockerizer
    {
        /// <summary>
        /// Dockerize.
        /// </summary>
        /// <param name="projectPath">Example: C:\Work\Hackathon2020\Hackathon2020\AspNetCoreApp</param>
        /// <param name="dockerContext"> The docker build context. Example: C:\Work\Hackathon2020\Hackathon2020</param>
        public void Dockerize(string projectPath, string dockerContext)
        {
            GenerateDockerFile(projectPath);

            var imageRepositoryAndTag = BuildDockerImage(projectPath, dockerContext);

            Thread.Sleep(2000);

            //RunDockerImage(imageRepositoryAndTag);
            
            PushDockerImage(imageRepositoryAndTag);
        }

        public void GenerateDockerFile(string projectPath)
        {
            if (!Directory.Exists(projectPath))
            {
                throw new Exception($"Invalid project path: {projectPath}");
            }

            var csprojFilepathAndProjectName = GetCsprojFilepathAndProjectName(projectPath);
            var csprojFilePath = csprojFilepathAndProjectName.CsprojFilePath;
            var projectName = csprojFilepathAndProjectName.ProjectName;

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

            var dockerfileContent = File.ReadAllText("../../../Helpers/DockerfileTemplate");
            dockerfileContent = dockerfileContent
                .Replace(DockerizerConstants.BaseImagePlaceholder, baseImage)
                .Replace(DockerizerConstants.BuildImagePlaceholder, buildImage)
                .Replace(DockerizerConstants.ProjectNamePlaceholder, projectName);

            var dockerfileOutputPath = Path.Combine(projectPath, "Dockerfile");
            File.WriteAllText(dockerfileOutputPath, dockerfileContent);
        }

        public string BuildDockerImage(string projectPath, string dockerContext)
        {
            var dockerfilePath = Path.Combine(projectPath, "Dockerfile");
            var projectName = GetCsprojFilepathAndProjectName(projectPath).ProjectName;
            
            var imageRepositoryName = "hhazem/test-repo";
            var imageTagName = "latest";
            var imageRepositoryAndTag = $"{imageRepositoryName}:{imageTagName}";

            RunProcessInternal(
                "docker",
                $"build -f {dockerfilePath} -t {imageRepositoryAndTag} {dockerContext}");

            return imageRepositoryAndTag;
        }

        public void RunDockerImage(string imageRepositoryAndTag)
        {
            var externalHttpPort = "32841";
            var internalHttpPort = "80";

            var imageRepositoryName = imageRepositoryAndTag.Split(':')[0];

            RunProcessInternal(
                "docker",
                $"run -d -p {externalHttpPort}:{internalHttpPort} --name {imageRepositoryName}-bydockerizer {imageRepositoryAndTag}");

            Console.WriteLine($"Docker image now up and running at port {externalHttpPort}.");
        }

        public void PushDockerImage(string imageRepositoryAndTag)
        {
            var externalHttpPort = "32841";
            var internalHttpPort = "80";

            var imageRepositoryName = imageRepositoryAndTag.Split(':')[0];

            RunProcessInternal(
                "docker",
                $"push {imageRepositoryAndTag}");

            Console.WriteLine($"Docker image now up and running at port {externalHttpPort}.");
        }

        private static void RunProcessInternal(string processName, string arguments)
        {
            var processInfo = new ProcessStartInfo(processName, arguments);

            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = true;

            int exitCode;
            using (var process = new Process())
            {
                process.StartInfo = processInfo;

                process.Start();
                process.EnableRaisingEvents = true;
                process.WaitForExit(5000);

                if (!process.HasExited)
                {
                    process.Kill();
                }

                exitCode = process.ExitCode;

                process.Close();
            }
        }

        private static CsprojFilePathAndProjectName GetCsprojFilepathAndProjectName(string projectPath)
        {
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

            var projectName = csprojFilePath
                .Replace(projectPath, string.Empty)
                .Replace("\\", string.Empty)
                .Replace("/", string.Empty)
                .Replace(".csproj", string.Empty);

            return new CsprojFilePathAndProjectName
            {
                CsprojFilePath = csprojFilePath,
                ProjectName = projectName
            };
        }

        private class CsprojFilePathAndProjectName
        {
            public string CsprojFilePath { get; set; }

            public string ProjectName { get; set; }
        }
    }
}
