using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GenerateKubernetesFiles
{
    public class K8sUtility
    {
        private const string ProjectNamePlaceHolder = "__projectname__";
        private string _projectPath;
        private string _projectName;

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

            _projectPath = projectPath;

            var csprojFilePath = csprojFilePaths[0];
            _projectName = csprojFilePath
                .Replace(_projectPath, string.Empty)
                .Replace("\\", string.Empty)
                .Replace("/", string.Empty)
                .Replace(".csproj", string.Empty).ToLower();

            var replacements = new Dictionary<string, string>
            {
                { ProjectNamePlaceHolder, _projectName }
            };

            // Generate Deployment File.
            GenerateTargetFile(projectPath, _projectName, "deployment", "yaml", replacements, addInsideTemplates: true);

            // Generate Values Files.
            GenerateTargetFile(projectPath, _projectName, "values", "yaml", replacements, addInsideTemplates: false);

            // Generate Chart Files.
            GenerateTargetFile(projectPath, _projectName, "Chart", "yaml", replacements, addInsideTemplates: false);

            // Generate Helpers.
            GenerateTargetFile(projectPath, _projectName, "_helpers", "tpl", replacements, addInsideTemplates: true);
        }

        public void CreateCluster(string azSubscription, string resourceGroupName, string clusterName)
        {
            SetAzContext();

            SetAzureSubscription(azSubscription);

            Console.WriteLine($"starting to create the AKS cluster in subscription: {azSubscription}, resourceGroup: {resourceGroupName}, clusterName: {clusterName}");

            CreateAKS(azSubscription, resourceGroupName, clusterName);

            Console.WriteLine($"Create AKS cluster successfully");

            SetKubectlContext(resourceGroupName, clusterName);

            Console.WriteLine($"Initializing Helm.");

            HelmInit();

            Console.WriteLine($"Running Helm.");

            RunHelm();

            Console.WriteLine($"Helm completed.");

            ExtractIp();

            OpenHomePage();
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

        private void SetAzContext()
        {
            RunProcessInternal(
                "powershell.exe",
                $"az login");
        }

        private void SetAzureSubscription(string azSubscription)
        {
            RunProcessInternal(
                "powershell.exe",
                $"az account set --subscription {azSubscription}");
        }

        private void CreateAKS(string azSubscription, string resourceGroupName, string aksName)
        {
            RunProcessInternal(
                "powershell.exe",
                $" New-AzAks -Force -ResourceGroupName {resourceGroupName} -Name {aksName} -location westeurope -KubernetesVersion 1.17.7");
        }

        private void SetKubectlContext(string resourceGroupName, string aksName)
        {
            RunProcessInternal(
                "powershell.exe",
                $" Import-AzAksCredential -Force -ResourceGroupName {resourceGroupName} -Name {aksName}");
        }

        private void HelmInit()
        {
            RunProcessInternal(
                "powershell.exe",
                $"helm init --wait");
        }

        private void RunHelm()
        {
            RunProcessInternal(
                "powershell.exe",
                @$"helm install --wait -n {_projectName} '{_projectPath}\charts\{_projectName}\'");
        }

        private void ExtractIp()
        {
            RunProcessInternal(
                "powershell.exe",
                $@"(kubectl get service {_projectName})[1] | Out-File -filePath C:\Files\LUIS\Files\Hackaton\ip.txt");
        }

        private void OpenHomePage()
        {
            var ipAsString = File.ReadAllText(@"C:\Files\LUIS\Files\Hackaton\ip.txt");
            var ip = ipAsString.Split(' ').Select(s => s.Trim()).ToList()[9];
            Process.Start(new ProcessStartInfo("cmd", $"/c start http://{ip}/weatherforecast") { CreateNoWindow = true });
        }

        private static void RunProcessInternal(string processName, string arguments)
        {
            var processInfo = new ProcessStartInfo(processName, arguments);

            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;

            int exitCode;
            using (var process = new Process())
            {
                process.StartInfo = processInfo;

                process.Start();
                process.EnableRaisingEvents = true;

                Console.WriteLine(process.StandardOutput.ReadToEnd());

                process.WaitForExit();

                if (!process.HasExited)
                {
                    process.Kill();
                }

                exitCode = process.ExitCode;

                process.Close();
            }
        }
    }
}
