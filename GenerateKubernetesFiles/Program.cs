using System;

namespace GenerateKubernetesFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to K8s Generator!");
            Console.WriteLine("Currently supported project types: [ASP.NET Core 3.1 web application]");
            Console.WriteLine("Please enter the path to the project you wish to dockerize.");

            var projectPath = Console.ReadLine();

            var k8sUtility = new K8sUtility();

            k8sUtility.GenerateK8sFiles(projectPath);

            Console.WriteLine("K8s Generator complete. You can press any key to exit!");

            Console.WriteLine("Please enter the subscription id:");

            var subscriptionId = Console.ReadLine();

            Console.WriteLine("Please enter the resource group name:");

            var resourceGroupName = Console.ReadLine();

            Console.WriteLine("Please enter the cluster name:");

            var aksName = Console.ReadLine();

            k8sUtility.CreateCluster(subscriptionId, resourceGroupName, aksName);

            Console.WriteLine("Cluster created successfully. You can press any key to exit!");

            Console.ReadLine();
        }
    }
}
