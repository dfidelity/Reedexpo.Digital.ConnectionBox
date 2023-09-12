#module nuget:?package=Cake.DotNetTool.Module&version=0.3.0         // dotnet tool nuget package loader - needs bootstrap - see build.ps1 at the end
#addin nuget:?package=Newtonsoft.Json&version=13.0.1
#addin nuget:?package=Cake.Http&version=0.6.1
#addin nuget:?package=Cake.FileHelpers&version=5.0.0
#addin nuget:?package=Cake.Sonar&version=1.1.30
#addin nuget:?package=ReedExpo.Cake.Coverlet&Version=1.0.10
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool&version=4.8.0
#tool dotnet:?package=CycloneDX&version=2.3.0                      // will be installed at .\tools\dotnet-CycloneDX.exe
#addin nuget:?package=Cake.Coverlet&version=2.5.4
#tool  dotnet:?package=coverlet.console&version=3.1.2  // For netcore2.1 use version=1.4.1

using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var appName = Argument<string>("appName", "connectionbox");
var rebuild = Argument("rebuild", true);
var buildNumber = Argument("buildNumber", "buildNumber");
var createArtifact = Argument("createArtifact", false);
var integrationTests= Argument("integrationTests", true);
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var buildmode = Argument("buildmode", "default");
var isCI = buildmode == "CI";
var environmentName = ArgumentThenEnvironmentVariableThenDefault("environment", "bamboo_ENVIRONMENT", "local");

var nugetConfigFile = "../NuGet.config";
var buildDir = Directory(".");
var deployDir = Directory("../Deploy");
var sourceRoot = Directory("../src");
var buildOutputDir = Directory("../output");
var assetsDir = Directory("../assets/images");
var artifactDir = buildOutputDir + Directory("artifact");
var testDir = Directory("../test");
var testResultsDir =   MakeAbsolute(testDir + Directory("TestResults"));
var appBundleZip = File("bundle.zip");
var assetsZip = File("assets.zip");
var customHandlerZip = File("sns_eventdestination_creation.zip");
var lambdaHandlerDir = Directory("../lambdaHandlers/sns_eventdestination_creation");
var QRCodeLambdaDir = Directory("../lambdaHandlers/colleqt_qrcode_generation");
var QRCodeLambdaZip = File("colleqt_qrcode_generation.zip");

var tempWebServiceDir = buildOutputDir + Directory("temp-webService");
var cyclonePath = GetFiles($"./**/dotnet-CycloneDX{(isWindows ? ".exe" : string.Empty)}").FirstOrDefault();
var isBamboo = Environment.GetEnvironmentVariables().Keys.Cast<string>().Any(k => k.StartsWith("bamboo_", StringComparison.OrdinalIgnoreCase));

//////////////////////////////////////////////////////////////////////
// CONFIGURE COVERLET
//////////////////////////////////////////////////////////////////////

var coverageResultsDir = MakeAbsolute(testDir + Directory("CoverageResults"));
var loggersArgument = "--logger:ReportPortal --logger:trx";
var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

// For coverlet, we need to pre-determine all the opencover output files.
// This list is used to select all the test projects we intend to run here
var unitTestProjects = new List<string> {
    "Reedexpo.Digital.ConnectionBox.Unit.Test"
};

var coverletHelper = GetCoverletHelper(unitTestProjects, Directory("../test"), coverageResultsDir);
//////////////////////////////////////////////////////////////////////


//////////////////////////////////////////////////////////////////////
// CONFIGURE CycloneDX
//////////////////////////////////////////////////////////////////////

if (cyclonePath == null)
{
    throw new CakeException("Can't find CycloneDX tool");
}
else
{
    Information($"Found CycloneDX: {cyclonePath}");
}


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(rebuild)
    .Does(() =>
{
    CleanDirectories("../src/**/bin/" + configuration);
    CleanDirectories(testResultsDir.ToString());
    CleanDirectories(coverageResultsDir.ToString());
    CleanDirectory(buildOutputDir);
    DeleteFile("../test/Reedexpo.Digital.ConnectionBox.Unit.Test/appsettings.test.json");
    CopyFile($"../Deploy/config/{environmentName}/environment.json", "../test/Reedexpo.Digital.ConnectionBox.Unit.Test/appsettings.test.json");
});

Task("NuGetConfig")
    .WithCriteria(isBamboo)
    .Does(() =>
{
    var progetUsername = EnvironmentVariableStrict("bamboo_ATLAS_PROGET_USERNAME");
    var progetPassword = EnvironmentVariableStrict("bamboo_ATLAS_PROGET_PASSWORD");
    var progetUb = new UriBuilder(EnvironmentVariableStrict("bamboo_PROGET_URL"));

    progetUb.Path = "/nuget/Default/v3/index.json";
    CopyFile("../NuGet.config.template", nugetConfigFile);

    ReplaceTextInFiles(nugetConfigFile, "{PROGET_USERNAME}", progetUsername);
    ReplaceTextInFiles(nugetConfigFile, "{PROGET_PASSWORD}", progetPassword);
    ReplaceTextInFiles(nugetConfigFile, "{PROGET_URL}", progetUb.Uri.AbsoluteUri);
});

Task("RestoreNuGetPackages")
    .WithCriteria(rebuild)
    //.IsDependentOn("NuGetConfig")
    .Does(() =>
{
        var restoreSettings = new DotNetRestoreSettings
         {
            NoCache = true
         };
         DotNetRestore("../", restoreSettings);
});

Task("ReportPortalConfig")
    .Does(() =>
{
    var reportPortalConfigFile = "../test/Reedexpo.Digital.ConnectionBox.Unit.Test/ReportPortal.config.json";
    var reportPortalUrl = EnvironmentVariableStrict("bamboo_REPORTPORTAL_URL");
    var uuid = EnvironmentVariableStrict("bamboo_REPORTPORTAL_UUID");

    CopyFile("../test/Reedexpo.Digital.ConnectionBox.Unit.Test/ReportPortal.config.json.template", reportPortalConfigFile);
    ReplaceTextInFiles(reportPortalConfigFile, "{bamboo_REPORTPORTAL_URL}", reportPortalUrl);
    ReplaceTextInFiles(reportPortalConfigFile, "{bamboo_REPORTPORTAL_UUID}", uuid);
});

Task("Build")
    .IsDependentOn("RestoreNuGetPackages")
    .Does(() =>
{
    var buildSettings = new DotNetMSBuildSettings();
    DotNetBuild("../Reedexpo.Digital.ConnectionBox.sln", new DotNetBuildSettings {
        Configuration = configuration,
        MSBuildSettings = buildSettings
    });
});

Task("TestConfig")
  .Does(() => {
	EnsureDirectoryExists(testResultsDir);
    EnsureDirectoryExists(coverageResultsDir);
});

Task("UnitTests")
    .IsDependentOn("TestConfig")   // your own dependencies or criteria
	.Does(() => {
	    // Argument customisation may not work with future versions of dotnet.exe as the -xml
        // is an XUnit specific switch.
        // See https://github.com/dotnet/cli/issues/4921
        // Using XUnit2 command failed because it cannot find xunit.dll alongside the test dll.
        foreach(var testProjectPath in coverletHelper.GetTestProjectFilePaths())
        {
            var coverletOutputName = coverletHelper.GetCoverageOutputFilenameForProject(testProjectPath);
            Information("UnitTests PATH: " + testProjectPath);
            Information("coverletOutputName : " + coverletOutputName);
            DotNetTest(testProjectPath.FullPath, new DotNetTestSettings {
                Configuration = "Debug",   // <- IMPORTANT
                ArgumentCustomization = args => args.Append (loggersArgument)
            });

            Coverlet(testProjectPath, new CoverletSettings {
                    CollectCoverage = true,
                    CoverletOutputFormat = CoverletOutputFormat.opencover,
                    CoverletOutputDirectory = coverageResultsDir,
                    CoverletOutputName = coverletOutputName
            });
            Information("Coverlet INIT STEP DONE");
        }
});

Task("AddWebServiceToArtifact")
    .WithCriteria(createArtifact)
    .Does(() =>
{
    DotNetPublish("../src/Reedexpo.Digital.ConnectionBox", new DotNetPublishSettings
        {
            Configuration = configuration,
            OutputDirectory = tempWebServiceDir
        });

    //http://docs.aws.amazon.com/elasticbeanstalk/latest/dg/dotnet-manifest.html
    EnsureDirectoryExists(artifactDir);
    CopyFile(buildDir + File("aws-windows-deployment-manifest.json"), tempWebServiceDir + File("aws-windows-deployment-manifest.json"));
    CopyFile(nugetConfigFile, artifactDir + File("NuGet.config"));
    Zip(tempWebServiceDir , artifactDir + appBundleZip);
});

Task ("AddQRGenerationUnzippedCodeToArtifact")
    .WithCriteria (createArtifact)
    .Does (() => {
        EnsureDirectoryExists (artifactDir);
        CopyDirectory(QRCodeLambdaDir, artifactDir + Directory("colleqt_qrcode_generation"));
    });

Task ("AddQRGenerationCodeToArtifact")
    .WithCriteria (createArtifact)
    .Does (() => {
        EnsureDirectoryExists (artifactDir);
        Zip (QRCodeLambdaDir, artifactDir + QRCodeLambdaZip);
    });

Task ("AddCustomHandlerToArtifact")
    .WithCriteria (createArtifact)
    .Does (() => {
        EnsureDirectoryExists (artifactDir);
        Zip (lambdaHandlerDir, artifactDir + customHandlerZip);
    });

Task("AddDeploymentScriptsToArtifact")
    .WithCriteria(createArtifact)
    .Does(() =>
    {
        CopyDirectory(deployDir, artifactDir);
    });

Task("AddAssetsToArtifact")
    .WithCriteria(createArtifact)
    .Does(() =>
    {
        EnsureDirectoryExists (artifactDir);
        Zip (assetsDir, artifactDir + assetsZip);
    });

Task("CreateBillOfMaterials")
    .WithCriteria(isCI)
    .Does(() => {
        EnsureDirectoryExists(artifactDir);

        var githubUser = EnvironmentVariableStrict("bamboo_GITHUB_USERNAME");
                var githubToken = EnvironmentVariableStrict("bamboo_GITHUB_ACCESS_TOKEN_SECRET");

                DotNetTool("../Reedexpo.Digital.ConnectionBox.sln", new DotNetToolSettings {
                        ToolPath = cyclonePath,
                        ArgumentCustomization = args => args.Append($" -o {artifactDir} --github-username {githubUser} --github-token {githubToken} --disable-github-licenses"),
                        EnvironmentVariables = new Dictionary<string, string> {
                            { "DOTNET_ROOT", "/opt/microsoft/dotnet"}
                        }
                    }
                );
    });
    
Task("FixBillOfMaterials")
    .WithCriteria(isCI)
    .Does(() => {
        EnsureDirectoryExists(artifactDir);
        
        XmlDocument xmlDoc = new XmlDocument();
        var nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
        nsMgr.AddNamespace("bom", "http://cyclonedx.org/schema/bom/1.3");
        xmlDoc.Load($"{artifactDir}/bom.xml");
        XmlNode? node = xmlDoc.SelectSingleNode("//bom:component[@bom-ref='pkg:nuget/Hangfire.PostgreSql@1.19.12']", nsMgr);
        XmlNode? nodeToUpdate = node?.FirstChild;
        
        if (nodeToUpdate is { Name: "publisher" }) {
            Console.WriteLine("nodeToUpdate InnerText");
            Console.WriteLine(nodeToUpdate.InnerText);
            nodeToUpdate.InnerText = "Frank Hommers and others";
        } else {
            Console.WriteLine("node not found");
        }
   
        xmlDoc.Save($"{artifactDir}/bom.xml");
    });

 Task("SonarBegin")
   .WithCriteria(isCI)
   .Does(() => {
     var sonarQubeUsername = EnvironmentVariableStrict("bamboo_ATLAS_SONARQUBE_USERNAME");
     var sonarQubePassword = EnvironmentVariableStrict("bamboo_ATLAS_SONARQUBE_PASSWORD");

     SonarBegin(new SonarBeginSettings{
         Url = EnvironmentVariableStrict("bamboo_SONARQUBE_URL"),
         Login = sonarQubeUsername,
         Password = sonarQubePassword,
         Verbose = false,
         Key = "ReedexpoDigitalConnection",
         Name = "Reedexpo.Digital.ConnectionBox",
         Version = "1.0",
         VsTestReportsPath = @"../**/TestResults/*.trx",
         OpenCoverReportsPath =  coverletHelper.GetCoverageOutputFilesForSonarQube(),
         Exclusions = "**/*Migrations/**",
         DuplicationExclusions = "**/*.cshtml"
     });
  });

 Task("SonarEnd")
   .WithCriteria(isCI)
   .Does(() => {
     var sonarQubeUsername = EnvironmentVariableStrict("bamboo_ATLAS_SONARQUBE_USERNAME");
     var sonarQubePassword = EnvironmentVariableStrict("bamboo_ATLAS_SONARQUBE_PASSWORD");

      SonarEnd(new SonarEndSettings{
         Login = sonarQubeUsername,
         Password = sonarQubePassword
      });
  });
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("CreateArtifact")
    .WithCriteria(createArtifact)
    .IsDependentOn("AddCustomHandlerToArtifact")
    .IsDependentOn("AddQRGenerationCodeToArtifact")
    .IsDependentOn("AddQRGenerationUnzippedCodeToArtifact")
    .IsDependentOn("AddAssetsToArtifact")
    .IsDependentOn("AddWebServiceToArtifact")
    .IsDependentOn("AddDeploymentScriptsToArtifact");

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("SonarBegin")
    .IsDependentOn("ReportPortalConfig")
    .IsDependentOn("Build")
    .IsDependentOn("UnitTests")
    .IsDependentOn("CreateBillOfMaterials")
    .IsDependentOn ("FixBillOfMaterials")
    .IsDependentOn("SonarEnd")
    .IsDependentOn("CreateArtifact");

Task("LocalBuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("CreateArtifact");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);


//////////////////////////////////////////////////////////////////////
// UTILITIES
//////////////////////////////////////////////////////////////////////

string EnvironmentVariableStrict(string key)
{
  var value = EnvironmentVariable(key);
  if (value == null){ throw new Exception("Environment Variable not found: " + key); }
  return value;
}

string ArgumentThenEnvironmentVariableThenDefault(string argumentName, string environmentVariable, string defaultValue)
{
    return Argument<string>(argumentName, EnvironmentVariable(environmentVariable) ?? defaultValue);
}

//////////////////////////////////////////////////////////////////////
