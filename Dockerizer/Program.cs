using System;

namespace Dockerizer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Dockerizer utility!");
            Console.WriteLine("Currently supported project types: [ASP.NET Core 3.1 web application]");
            Console.WriteLine("Please enter the path to the project you wish to dockerize.");
            
            var projectPath = Console.ReadLine();

            Console.WriteLine("Please enter the path for the docker build context.");

            var dockerContext = Console.ReadLine();

            var dockerizer = new Dockerizer();
            dockerizer.Dockerize(
                projectPath,
                dockerContext);

            Console.WriteLine("Dockerization complete. You can press any key to exit!");
            Console.ReadLine();
        }
    }
}
