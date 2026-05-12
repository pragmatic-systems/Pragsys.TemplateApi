///////////////////////////////////////////////////////////////////////////////
// ADDINS
///////////////////////////////////////////////////////////////////////////////
#addin nuget:?package=Cake.Json&version=7.0.1
#addin nuget:?package=Cake.Docker&version=1.3.0
#addin nuget:?package=Cake.SonarQube&version=4.0.0

///////////////////////////////////////////////////////////////////////////////
// TOOLS
///////////////////////////////////////////////////////////////////////////////
#tool dotnet:?package=GitVersion.Tool&version=5.12.0

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");

var configuration = Argument("configuration", "Release");

// Nuget Params
var nugetPackageSource = Argument<string>("Source", null)			// Input from cmd args to Cake 
	?? EnvironmentVariable<string>("INPUT_SOURCE", null);			// Input from GHA to Cake

var nugetApiKey = Argument<string>("ApiKey", null)					// Input from cmd args to Cake 
	?? EnvironmentVariable<string>("INPUT_APIKEY", null);			// Input from GHA to Cake
	
var versionNumber = Argument<string>("VersionOverride", null)		// Input from cmd args to Cake 
	?? EnvironmentVariable<string>("INPUT_VERSIONOVERRIDE", null);	// Input from GHA to Cake
	
// Container Params
var containerRegistry = Argument<string>("ContainerRegistry", null) 
	?? EnvironmentVariable<string>("INPUT_CONTAINERREGISTRY", null);
	
var containerRegistryToken = Argument<string>("ContainerRegistryToken", null) 
	?? EnvironmentVariable<string>("INPUT_CONTAINERREGISTRYTOKEN", null);

var containerRegistryUserName = Argument<string>("ContainerRegistryUserName", null)
	?? EnvironmentVariable<string>("INPUT_CONTAINERREGISTRYUSERNAME", null);

// Sonar Params
var sonarProjectKey = Argument<string>("SonarProjectKey", null)
    ?? EnvironmentVariable<string>("INPUT_SONARPROJECTKEY", null);

var sonarProjectName = Argument<string>("SonarProjectName", null)
    ?? EnvironmentVariable<string>("INPUT_SONARPROJECTNAME", null);

var sonarHostUrl = Argument<string>("SonarHostUrl", null)
    ?? EnvironmentVariable<string>("INPUT_SONARHOSTURL", null)
		?? "http://localhost:9000";

var sonarToken = Argument<string>("SonarToken", null)
    ?? EnvironmentVariable<string>("INPUT_SONARTOKEN", null);

var artifactsFolder = "./artifacts";
var packagesFolder = System.IO.Path.Combine(artifactsFolder, "packages");
var swaggerFolder = System.IO.Path.Combine(artifactsFolder, "swagger");
var postmanFolder = System.IO.Path.Combine(artifactsFolder, "postman");

BuildManifest buildManifest;

///////////////////////////////////////////////////////////////////////////////
// Setup / Teardown
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
	var cakeMixFile = "build.cakemix";

	// Load BuildManifest
	if (!System.IO.File.Exists(cakeMixFile))
	{
		Warning("No cakemix file found, creating...");

		var manifest = new BuildManifest
		{
			NugetPackages = new string[0],
			DockerPackages = System.IO.Directory.GetFiles("./src/", "Dockerfile", SearchOption.AllDirectories),
			Tests = System.IO.Directory.GetFiles(".", "*Tests.csproj", SearchOption.AllDirectories),
			Benchmarks = System.IO.Directory.GetFiles(".", "*.Benchmark.csproj", SearchOption.AllDirectories),
		};
		SerializeJsonToPrettyFile(cakeMixFile, manifest);
	}

	buildManifest = DeserializeJsonFromFile<BuildManifest>(cakeMixFile);

	// Clean artifacts
	if (System.IO.Directory.Exists(artifactsFolder))
		System.IO.Directory.Delete(artifactsFolder, true);
});

Teardown(context =>
{
    
});

///////////////////////////////////////////////////////////////////////////////
// Tasks
///////////////////////////////////////////////////////////////////////////////
Task("__NugetArgsCheck")
	.Does(() => {
		if (string.IsNullOrEmpty(nugetPackageSource))
			throw new ArgumentException("NugetPackageSource is required");

		if (string.IsNullOrEmpty(nugetApiKey))
			throw new ArgumentException("NugetApiKey is required");
	});

Task("__ContainerArgsCheck")
	.Does(() => {
		if (string.IsNullOrEmpty(containerRegistryToken))
			throw new ArgumentException("ContainerRegistryToken is required");
			
		if (string.IsNullOrEmpty(containerRegistryUserName))
			throw new ArgumentException("ContainerRegistryUserName is required");
			
		if (string.IsNullOrEmpty(containerRegistry))
			throw new ArgumentException("ContainerRegistry is required");
	});

Task("__SonarArgsCheck")
	.Does(() => {
		if (string.IsNullOrEmpty(sonarProjectKey))
			throw new ArgumentException("SonarProjectKey is required");
			
		if (string.IsNullOrEmpty(sonarProjectName))
			throw new ArgumentException("SonarProjectName is required");
			
		if (string.IsNullOrEmpty(sonarToken))
			throw new ArgumentException("SonarToken is required");
	});

Task("__UnitTest")
	.Does(() => {

		foreach(var test in buildManifest.Tests)
		{
			Information($"Testing {test}...");

			var testName = System.IO.Path.GetFileNameWithoutExtension(test);

			var settings = new DotNetTestSettings
			{
				Configuration = configuration,
				ResultsDirectory = artifactsFolder
			};

			// Console log for build agent
			settings.Loggers.Add("console;verbosity=normal");
		
			// Logging for trx test report artifact
			settings.Loggers.Add($"trx;logfilename={testName}.trx");

			DotNetTest(test, settings);
		}
	});

Task("__Benchmark")
	.Does(() => {

		foreach(var benchmark in buildManifest.Benchmarks)
		{
			Information($"Benchmarking {benchmark}...");
			var benchName = System.IO.Path.GetFileNameWithoutExtension(benchmark);

			var settings = new DotNetRunSettings
			{
				Configuration = "Release", 
				ArgumentCustomization = args => {
					return args
						.Append("--artifacts")
						.AppendQuoted(System.IO.Path.Combine(artifactsFolder, benchName));
				}
			};

			DotNetRun(benchmark, settings);
		}
	});

Task("__LintCheck")
    .Does(() =>
    {
        Information("Running lint check with dotnet format...");
        // Run `dotnet format --verify-no-changes`
        var result = StartProcess("dotnet", "format --verify-no-changes");
        if (result != 0)
        {
            throw new Exception("Lint check failed: code formatting violations detected. Run `dotnet format`");
        }
        Information("Lint check passed – no formatting changes required.");
    });

Task("__VersionInfo")
	.Does(() => {

		if (string.IsNullOrEmpty(versionNumber))
		{
			var version = GitVersion();
			Information("GitVersion Info: " + SerializeJsonPretty(version));
			versionNumber = version.SemVer;
		}

		Information("Version Number: " + versionNumber);
	});

Task("__SonarQubeAnalysis")
	.Does(() => {
		Information("Running SonarQube analysis...");

      var sonarSettings = new SonarQubeSettings
      {
          ProjectKey = sonarProjectKey,
          ProjectName = sonarProjectName,
          SourceEncoding = "UTF-8",
					HostUrl = sonarHostUrl,
					Token = sonarToken
      };

      foreach (var project in buildManifest.NugetPackages)
      {
          Information($"Analyzing {project}...");
          SonarQubeAnalysis(project, sonarSettings);
      }
	});

Task("__NugetPack")
	.Does(() => {

		foreach(var package in buildManifest.NugetPackages)
		{
			Information($"Packing {package}...");
			var settings = new DotNetMSBuildSettings
			{
				PackageVersion = versionNumber
			};

			var packSettings = new DotNetPackSettings
			{
				Configuration = "Release",
				OutputDirectory = packagesFolder,
				MSBuildSettings = settings
			};
			DotNetPack(package, packSettings);
		}
	});

Task("__NugetPush")
	.Does(() => {

		if (!System.IO.Directory.Exists(packagesFolder))
		{
			Information("No packages to push in the packages folder");
			return;
		}

		var packedArtifacts = System.IO.Directory.EnumerateFiles(packagesFolder);
		foreach(var package in packedArtifacts)
		{
			Information($"Pushing {package}...");
			var pushSettings = new DotNetNuGetPushSettings
			{
				Source = nugetPackageSource,
				ApiKey = nugetApiKey
			};
			DotNetNuGetPush(package, pushSettings);
		}
	});

Task("__DockerLogin")
	.Does(() => {
		
		Information($"Logging into registry: {containerRegistry}...");

		var loginSettings = new DockerRegistryLoginSettings
		{ 
			Password = containerRegistryToken, 
			Username = containerRegistryUserName
		};

		DockerLogin(loginSettings, containerRegistry);  
	});

Task("__DockerPack")
	.IsDependentOn("__VersionInfo")
	.Does(() => {

		foreach(var package in buildManifest.DockerPackages)
		{
			Information($"Packing Docker: {package}...");
			var directoryName = System.IO.Path.GetDirectoryName(package);
			Information($"Directory Name: {directoryName}");
			var parts = directoryName.Split(System.IO.Path.DirectorySeparatorChar);
			Information($"Parts: {parts.Length}");
			Information($"Last Part: {parts.Last()}");
			var packageName = parts.Last().ToLower();
			packageName = $"{containerRegistry}/{packageName}".ToLower();	
			
			Information($"Packing: {packageName}...");
			var settings = new DockerImageBuildSettings
				{
					Tag = new[] { $"{packageName}:{versionNumber}" },
					File = package
				};

			DockerBuild(settings, ".");
		}
	});

Task("__DockerPush")
	.Does(() => {

		foreach(var package in buildManifest.DockerPackages)
		{
			Information($"Pushing Docker: {package}...");
			var directoryName = System.IO.Path.GetDirectoryName(package);
			Information($"Directory Name: {directoryName}");
			var parts = directoryName.Split(System.IO.Path.DirectorySeparatorChar);
			Information($"Parts: {parts.Length}");
			Information($"Last Part: {parts.Last()}");
			var packageName = parts.Last().ToLower();
			packageName = $"{containerRegistry}/{packageName}".ToLower();

			var settings = new DockerImagePushSettings
			{ 
				AllTags = true 
			};
		
			Information($"Pushing: {packageName}...");

			DockerPush(settings, $"{packageName}");
		}
	});

Task("BuildAndTest")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__UnitTest");

Task("BuildAndBenchmark")
	.IsDependentOn("__Benchmark");

Task("NugetPackAndPush")
	.IsDependentOn("__NugetArgsCheck")
	.IsDependentOn("__VersionInfo")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__SonarQubeAnalysis")
	.IsDependentOn("__UnitTest")
	.IsDependentOn("__Benchmark")
	.IsDependentOn("__NugetPack")
	.IsDependentOn("__NugetPush");

Task("DockerPackAndPush")
	.IsDependentOn("__ContainerArgsCheck")
	.IsDependentOn("__VersionInfo")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__SonarQubeAnalysis")
	.IsDependentOn("__UnitTest")
	.IsDependentOn("__Benchmark")
	.IsDependentOn("__DockerLogin")
	.IsDependentOn("__DockerPack")
	.IsDependentOn("__DockerPush");

Task("FullPackAndPush")
	.IsDependentOn("__NugetArgsCheck")
	.IsDependentOn("__ContainerArgsCheck")
	.IsDependentOn("__VersionInfo")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__SonarQubeAnalysis")
	.IsDependentOn("__UnitTest")
	.IsDependentOn("__Benchmark")
	.IsDependentOn("__NugetPack")
	.IsDependentOn("__DockerLogin")
	.IsDependentOn("__DockerPack")
	.IsDependentOn("__NugetPush")
	.IsDependentOn("__DockerPush");

Task("Default")
	.IsDependentOn("__LintCheck")
	.IsDependentOn("__SonarQubeAnalysis")
	.IsDependentOn("__UnitTest")
	.IsDependentOn("__Benchmark");

RunTarget(target);

public class BuildManifest
{
	public string[] NugetPackages { get; set; }
	public string[] DockerPackages { get; set; }
	public string[] Tests { get; set; }
	public string[] Benchmarks { get; set; }
	public Dictionary<string, string> ApiSpecs { get; set; }
}