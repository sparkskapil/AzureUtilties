using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.VisualStudio.Services.TestResults.WebApi;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable InconsistentNaming

namespace AzureUtilities
{
    public struct TeamProject
    {
        public string TeamProjectName;
        public string Url;
    }
    public struct BuildPipeline
    {
        public string Name;
        public string Path;
        public string Url;
        public int Id;
        public TeamProject Project;

        public BuildPipeline(DefinitionReference definitionRef)
        {
            Name = definitionRef.Name;
            Path = definitionRef.Path;
            Url = definitionRef.Url;
            Id = definitionRef.Id;
            Project = new TeamProject
            {
                TeamProjectName = definitionRef.Project.Name,
                Url = definitionRef.Url
            };
        }
    }
    public enum BuildStatus
    {
        None = 0,
        InProgress = 1,
        Completed = 1 << 1,
        Cancelling = 1 << 2,
        Postponed = 1 << 3,
        NotStarted = 1 << 4,
        All = 47
    }
    public struct BuildInfo
    {
        public string BuildNumber;
        public string Url;
        public Uri Uri;
        public BuildPipeline Pipeline;
        public DateTime? StarTime;
        public DateTime? FinishTime;
        public DateTime? QueueTime;
        public bool Success;
        public BuildStatus Status;
    }
    public struct TestRun
    {
        public string Title;
        public string BuildUrl;
        public int Id;
        public string ProjectName;
        public int TotalTests;
        public int PassedTests;
        public int SkippedTests;
        public int FailedTests;
        public int IncompleteTests;
        public bool IsAutomated;
    }
    public struct Test
    {
        public string Title;
        public int TestRunId;
        public string BuildNumber;
        public int PipelineId;
        public DateTime Start;
        public DateTime Finish;
        public string FullyQualifiedName;
        public string Result;
        public string Error;
        public string StackTrace;
    }

    public class Projects
    {
        public Projects(TfsTeamProjectCollection collection)
        {
            m_collection = collection;
        }
        public List<TeamProject> GetAllProjects()
        {
            var service = m_collection.GetService<ICommonStructureService>();
            var projects = service.ListAllProjects();

            var projectsList = new List<TeamProject>();
            foreach (var project in projects)
            {
                var teamProject = new TeamProject
                {
                    TeamProjectName = project.Name,
                    Url = project.Uri
                };
                projectsList.Add(teamProject);
            }

            return projectsList;
        }

        private readonly TfsTeamProjectCollection m_collection;

    }

    public class Builds
    {
        public Builds(TfsTeamProjectCollection collection)
        {
            m_collection = collection;
        }

        public List<BuildPipeline> GetBuildPipelines(string TeamProjectName)
        {
            var client = m_collection.GetClient<BuildHttpClient>();

            // Get all build definitions for the project
            var definitions = client.GetDefinitionsAsync(TeamProjectName).Result;
            var definitionList = new List<BuildPipeline>();
            foreach (var def in definitions)
            {
                var pipeline = new BuildPipeline
                {
                    Name = def.Name,
                    Path = def.Path,
                    Id = def.Id,
                    Url = def.Url,
                    Project = new TeamProject
                    {
                        TeamProjectName = def.Project.Name,
                        Url = def.Project.Url
                    }
                };
                definitionList.Add(pipeline);
            };

            return definitionList;
        }
        public List<BuildPipeline> GetBuildPipelines(TeamProject project)
        {
            return GetBuildPipelines(project.TeamProjectName);
        }

        public List<BuildInfo> GetAllBuilds(string TeamProjectName, List<int> BuildPipelineIds = null)
        {
            var client = m_collection.GetClient<BuildHttpClient>();
            var builds = client.GetBuildsAsync(TeamProjectName, BuildPipelineIds).GetAwaiter().GetResult();
            var buildInfos = new List<BuildInfo>();

            foreach (var build in builds)
            {
                var buildInfo = new BuildInfo
                {
                    BuildNumber = build.BuildNumber,
                    Url = build.Url,
                    Uri = build.Uri,
                    Pipeline = new BuildPipeline(build.Definition),
                    StarTime = build.StartTime,
                    FinishTime = build.FinishTime,
                    QueueTime = build.QueueTime,
                    Success = (build.Result == BuildResult.Succeeded),
                    Status = (BuildStatus)build.Status
                };
                buildInfos.Add(buildInfo);
            }

            return buildInfos;
        }
        public List<BuildInfo> GetAllBuilds(TeamProject project, List<int> BuildPipelineIds = null)
        {
            return GetAllBuilds(project.TeamProjectName, BuildPipelineIds);
        }
        public List<BuildInfo> GetAllBuilds(BuildPipeline pipeline)
        {
            var pipelineIds = new List<int>
            {
                pipeline.Id
            };
            return GetAllBuilds(pipeline.Project, pipelineIds);
        }

        private readonly TfsTeamProjectCollection m_collection;
    }

    public class Tests
    {
        public Tests(TfsTeamProjectCollection collection)
        {
            m_collection = collection;
            m_service = collection.GetService<ITestManagementService>();
        }

        public List<TestRun> GetAllTestRuns(TeamProject project)
        {
            var tmProject = m_service.GetTeamProject(project.TeamProjectName);
            var testRuns = tmProject.TestRuns.Query("select * From TestRun").OrderByDescending(x => x.DateCompleted);
            
            var testRunList = new List<TestRun>();
            foreach (var run in testRuns)
            {
                TestRun testRun = ConvertToTestRun(run);
                testRunList.Add(testRun);
            }
            return testRunList;
        }
        public List<TestRun> GetAllTestRuns(BuildInfo build)
        {
            var tmProject = m_service.GetTeamProject(build.Pipeline.Project.TeamProjectName);
            var testRuns = tmProject.TestRuns.ByBuild(build.Uri);
            var testRunList = new List<TestRun>();
            foreach (var run in testRuns)
            {
                TestRun testRun = ConvertToTestRun(run);
                testRunList.Add(testRun);
            }
            return testRunList;
        }

        public List<Test> GetAllTests(TestRun run)
        {   
            var client = m_collection.GetClient<TestResultsHttpClient>();
            var res = client.GetTestResultsAsync(run.ProjectName, run.Id).Result;

            var testList = new List<Test>();
            foreach (var test in res)
            {
                var T = new Test
                {
                    TestRunId = test.Id,
                    BuildNumber = test.Build.Name,
                    Finish = test.CompletedDate,
                    Start = test.StartedDate,
                    Title = test.TestCaseTitle,
                    PipelineId = Convert.ToInt32(test.Build.Id),
                    FullyQualifiedName = test.AutomatedTestName,
                    Result = test.Outcome,
                    Error = test.ErrorMessage ?? "",
                    StackTrace = test.StackTrace ?? ""
                };
                
                testList.Add(T);
            }
            return testList;
        }


        private TestRun ConvertToTestRun(ITestRun run)
        {
            return new TestRun
            {
                Title = run.Title,
                BuildUrl = run.BuildUri.ToString(),
                Id = run.Id,
                ProjectName = run.Project.TeamProjectName,
                TotalTests = run.TotalTests,
                PassedTests = run.PassedTests,
                SkippedTests = run.NotApplicableTests,
                FailedTests = run.UnanalyzedTests,
                IncompleteTests = run.IncompleteTests,
                IsAutomated = run.IsAutomated
            };
        }

        private readonly ITestManagementService m_service;
        private readonly TfsTeamProjectCollection m_collection;
    }
    public class AzureAdapter
    {
        public Projects Projects { get; }
        public Builds Builds { get; }
        public Tests Tests { get; }

        public string Collection { get { return m_collection.DisplayName; } }
        public AzureAdapter(string AzureCollectionUrl)
        {
            m_collectionUri = new Uri(AzureCollectionUrl);
            m_collection = new TfsTeamProjectCollection(m_collectionUri);
            Projects = new Projects(m_collection);
            Builds = new Builds(m_collection);
            Tests = new Tests(m_collection);
            
        }

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Uri m_collectionUri;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly TfsTeamProjectCollection m_collection;
    }
}
