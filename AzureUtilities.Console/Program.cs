using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AzureUtilities
{
    class Program
    {
        static AzureAdapter Azure = null;
        static TeamProject SelectedProject;
        static BuildPipeline SelectedPipeline;
        static BuildInfo SelectedBuild;
        static TestRun SelectedTestRun;


        static void CreatePlaylist(string PathToFile, List<Test> Tests, bool IncludePassed = false)
        {
            XmlWriter writer = XmlWriter.Create(PathToFile);
            writer.WriteStartDocument();
            
            // Write Playlist Root
            writer.WriteStartElement("Playlist");
            writer.WriteAttributeString("Version", "1.0");

            foreach(var test in Tests)
            {
                if(IncludePassed || test.Result == "Failed")
                {
                    writer.WriteStartElement("Add");
                    writer.WriteAttributeString("Test", test.FullyQualifiedName);
                    writer.WriteEndElement();
                }
            }

            // Write End Of Playlist Root
            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Close();
        }

        static void ExportAllTestRunsInSelectedBuild(bool IncludePassed = false)
        {
            var TestRuns = Azure.Tests.GetAllTestRuns(SelectedBuild);
            foreach(var run in TestRuns)
            {
                var tests = Azure.Tests.GetAllTests(run);
                string path = String.Format("{0}.playlist", run.Title);
                CreatePlaylist(path, tests);
                Console.WriteLine("Exported Playlist {0}", Path.GetFullPath(path));
            }
        }
        static void ShowProjectSelectionMenu()
        {
            Console.Clear();
            Console.WriteLine("CONNECTED({0}) > PROJECTS |)",Azure.Collection);

            var Projects = Azure.Projects.GetAllProjects();
            if(Projects.Count == 0)
            {
                Console.WriteLine("There are no projects.");
                Environment.Exit(1);
            }
            for (int i = 0; i < Projects.Count; i++)
            {
                TeamProject Project = Projects[i];
                Console.WriteLine("{0}. {1}", i + 1, Project.TeamProjectName);
            }
            Console.Write("Select Project: ");
            int choice = Convert.ToInt32(Console.ReadLine());
            SelectedProject = Projects[choice - 1];
        }

        static void ShowPipelineSelectionMenu()
        {
            Console.Clear();
            Console.WriteLine("CONNECTED({0}) > PROJECT({1}) |)", 
                Azure.Collection, SelectedProject.TeamProjectName);

            var Pipelines = Azure.Builds.GetBuildPipelines(SelectedProject);
            if (Pipelines.Count == 0)
            {
                Console.WriteLine("There are no Pipelines.");
                Environment.Exit(1);
            }
            for (int i = 0; i < Pipelines.Count; i++)
            {
                var Pipeline = Pipelines[i];
                Console.WriteLine("{0}. {1}", i + 1, Pipeline.Name);
            }
            Console.Write("Select Project: ");
            int choice = Convert.ToInt32(Console.ReadLine());
            SelectedPipeline = Pipelines[choice - 1];
        }

        static void ShowBuildSelectionMenu()
        {
            Console.Clear();
            Console.WriteLine("CONNECTED({0}) > PROJECT({1}) > PIPELINE({2}) |)", 
                Azure.Collection, SelectedProject.TeamProjectName, SelectedPipeline.Name );

            var Builds = Azure.Builds.GetAllBuilds(SelectedPipeline);
            if (Builds.Count == 0)
            {
                Console.WriteLine("There are no Builds.");
                Environment.Exit(1);
            }
            for (int i = 0; i < Builds.Count; i++)
            {
                var Build = Builds[i];
                Console.WriteLine("{0}. {1}", i + 1, Build.BuildNumber);
            }
            Console.Write("Select Project: ");
            int choice = Convert.ToInt32(Console.ReadLine());
            SelectedBuild = Builds[choice - 1];
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("CONNECT\n");
                Console.Write("Azure DevOps Organization Url: ");
                string Url = Console.ReadLine();
                Azure = new AzureAdapter(Url);
            }
            else
            {
                Azure = new AzureAdapter(args[0]);
            }

            // Make User To Select Project
            ShowProjectSelectionMenu();

            // Make User To Select Build Pipeline
            ShowPipelineSelectionMenu();

            //Make User To Select Build
            ShowBuildSelectionMenu();

            //Export Playlists
            ExportAllTestRunsInSelectedBuild();

            Console.ReadKey();
        }
    }
}
