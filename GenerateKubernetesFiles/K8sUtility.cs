using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GenerateKubernetesFiles
{
    public class K8sUtility
    {
        private const string ProjectNamePlaceHolder = "__projectname__";

        /// <summary>
        /// Generate K8s Files.
        /// </summary>
        /// <param name="projectPath">Example: C:\Work\Hackathon2020\Hackathon2020\AspNetCoreApp</param>
        public void GenerateK8sFiles(string projectPath)
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
            var projectName = csprojFilePath
                .Replace(projectPath, string.Empty)
                .Replace("\\", string.Empty)
                .Replace("/", string.Empty)
                .Replace(".csproj", string.Empty);

            var replacements = new Dictionary<string, string>
            {
                { ProjectNamePlaceHolder, projectName.ToLower() }
            };

            // Generate Deployment File.
            GenerateTargetFile(projectPath, projectName, "deployment", "yaml", replacements, addInsideTemplates: true);

            // Generate Values Files.
            GenerateTargetFile(projectPath, projectName, "values", "yaml", replacements, addInsideTemplates: false);

            // Generate Chart Files.
            GenerateTargetFile(projectPath, projectName, "Chart", "yaml", replacements, addInsideTemplates: false);

            // Generate Helpers.
            GenerateTargetFile(projectPath, projectName, "_helpers", "tpl", replacements, addInsideTemplates: true);
        }

        private void GenerateTargetFile(string projectPath, string projectName, string targetFile, string targetExt, Dictionary<string, string> replacements, bool addInsideTemplates)
        {
            var templatesSubPath = string.Empty;
            if (addInsideTemplates)
            {
                templatesSubPath = "templates/";
            }

            var targetFileContent = File.ReadAllText($"Templates/{targetFile}Template.{targetExt}");

            foreach (var kvPair in replacements)
            {
                var placeholder = kvPair.Key;
                var value = kvPair.Value;

                targetFileContent = targetFileContent
                .Replace(placeholder, value);
            }

            var filePath = Path.Combine(projectPath, $"charts/{projectName}/{templatesSubPath}");
            FileInfo file = new FileInfo(filePath);
            file.Directory.Create();

            var targetFileOutputPath = Path.Combine(filePath, $"{targetFile}.{targetExt}");
            File.WriteAllText(targetFileOutputPath, targetFileContent);
        }


    }
}
