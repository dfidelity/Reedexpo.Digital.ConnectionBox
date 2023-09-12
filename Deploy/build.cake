#addin nuget:?package=AWSSDK.AutoScaling&version=3.7.4.7
#addin nuget:?package=AWSSDK.CloudFormation&version=3.7.3.9
#addin nuget:?package=AWSSDK.Core&version=3.7.0.44&loaddependencies=true
#addin nuget:?package=AWSSDK.EC2&version=3.7.17
#addin nuget:?package=AWSSDK.ElasticBeanstalk&version=3.7.0.42
#addin nuget:?package=AWSSDK.S3&version=3.7.1.14
#addin nuget:?package=AWSSDK.SecurityToken&version=3.7.1.32
#addin nuget:?package=AWSSDK.IdentityManagement&version=3.7.2.21
#addin nuget:?package=AWSSDK.Lambda&version=3.7.1.5
#addin nuget:?package=AWSSDK.KeyManagementService&version=3.7.4
#addin nuget:?package=SharpZipLib&version=1.3.3
#addin nuget:?package=Firefly.CrossPlatformZip&version=0.5.0
#addin nuget:?package=YamlDotNet&version=6.0.0
#addin nuget:?package=Cake.FileHelpers&version=5.0.0
#addin nuget:?package=Cake.Http&version=2.0.0
#addin nuget:?package=Newtonsoft.Json&version=13.0.1

#addin nuget:?package=ReedExpo.Cake.Base&version=1.0.7
#addin nuget:?package=ReedExpo.Cake.AWS.Base&version=3.7.29
#addin nuget:?package=ReedExpo.Cake.AWS.EC2&version=3.7.15
#addin nuget:?package=ReedExpo.Cake.AWS.ElasticBeanstalk&version=3.7.143
#addin nuget:?package=ReedExpo.Cake.AWS.CloudFormation&version=3.7.116
#addin nuget:?package=ReedExpo.Cake.AWS.BuildAuthentication&version=3.7.32
#addin nuget:?package=ReedExpo.Cake.CrossPlatformZip&version=1.0.18
#addin nuget:?package=ReedExpo.Cake.CycloneDX&version=1.0.12
#addin nuget:?package=ReedExpo.Cake.ServiceNow&version=1.0.43
#addin nuget:?package=Firefly.EmbeddedResourceLoader&version=0.1.3
#addin nuget:?package=Cake.AWS.S3&version=0.6.8
#addin nuget:?package=MimeTypesMap&Version=1.0.7
#addin nuget:?package=ReedExpo.Cake.AWS.Lambda&version=3.7.39
#addin nuget:?package=Polly.Contrib.WaitAndRetry&version=1.1.1
#addin nuget:?package=Polly&version=7.2.3

using Amazon.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Arguments
var dbMasterPassword = Argument<string>("dbMasterPassword");
var jwtSecretKey = Argument<string>("jwtSecretKey");
var deployVersion = Argument<string>("deployVersion");
var bambooBuildNumber = Argument<string>("buildNumber");
var deployColleqtQrLambda = EnvironmentVariableStrict("bamboo_DEPLOY_COLLEQT_QR_LAMBDA");
var apigeeApiKey = EnvironmentVariableStrict("bamboo_APIGEE_API_KEY");
var UrlShortenerApiKey = EnvironmentVariableStrict("bamboo_URL_SHORTENER_API_KEY");
var CheckboxClientId = EnvironmentVariableStrict("bamboo_CHECKBOX_CLIENT_ID");
var CheckboxClientSecret = EnvironmentVariableStrict("bamboo_CHECKBOX_CLIENT_SECRET");

var PartyboxClientId = EnvironmentVariableStrict("bamboo_PARTYBOX_CLIENT_ID");
var PartyboxClientSecret = EnvironmentVariableStrict("bamboo_PARTYBOX_CLIENT_SECRET");

var ExboxClientId = EnvironmentVariableStrict("bamboo_EXBOX_CLIENT_ID");
var ExboxClientSecret = EnvironmentVariableStrict("bamboo_EXBOX_CLIENT_SECRET");
var EntitlementServiceClientId = EnvironmentVariableStrict("bamboo_ENTITLEMENT_CLIENT_ID");
var EntitlementServiceClientSecret = EnvironmentVariableStrict("bamboo_ENTITLEMENT_CLIENT_SECRET");

var WatchboxClientId = EnvironmentVariableStrict("bamboo_WATCHBOX_CLIENT_ID");
var WatchboxClientSecret = EnvironmentVariableStrict("bamboo_WATCHBOX_CLIENT_SECRET");

var ExhibitorWriteClientId = EnvironmentVariableStrict("bamboo_EXHIBITOR_WRITE_CLIENT_ID");
var ExhibitorWriteClientSecret = EnvironmentVariableStrict("bamboo_EXHIBITOR_WRITE_CLIENT_SECRET");

var AthenaOutputLocation = EnvironmentVariableStrict("bamboo_ATHENA_OUTPUT_LOCATION");
var AthenaDatabaseName = EnvironmentVariableStrict("bamboo_ATHENA_DATABASE_NAME");
var AthenaRatBoxDatabaseName = EnvironmentVariableStrict("bamboo_ATHENA_RATBOX_DATABASE_NAME");

var enableEvents = Argument<bool>("enableEvents", false);
var entitlementsSplitEventEditionId = EnvironmentVariableStrict("bamboo_ENTITLEMENTS_SPLIT_EVENT_EDITION_ID");
var appEntitlementProductCode = EnvironmentVariableStrict("bamboo_APP_ENTITLEMENT_PRODUCT_CODE");
var appEntitlementProductNumber = EnvironmentVariableStrict("bamboo_APP_ENTITLEMENT_PRODUCT_NUMBER");
var readerEntitlementProductCode = EnvironmentVariableStrict("bamboo_READER_ENTITLEMENT_PRODUCT_CODE");
var readerEntitlementProductNumber = EnvironmentVariableStrict("bamboo_READER_ENTITLEMENT_PRODUCT_NUMBER");
var appName = Argument<string>("appName", "connectionbox");
var environmentName = ArgumentThenEnvironmentVariableThenDefault("environment", "bamboo_ENVIRONMENT", "local");
var forceCloudFormation = ArgumentThenEnvironmentVariableThenDefault("forceCloudFormation", "bamboo_FORCE_CLOUD_FORMATION", "0");
var enableEventBus = ArgumentThenEnvironmentVariableThenDefault("ENABLE_EVENT_BUS", "bamboo_ENABLE_EVENT_BUS", "false");
var enableEntitlementHandlerJob = ArgumentThenEnvironmentVariableThenDefault("ENABLE_ENTITLEMENT_HANDLER_JOB", "bamboo_ENABLE_ENTITLEMENT_HANDLER_JOB", "false");
var amiName = ArgumentThenEnvironmentVariableThenDefault("amiName", "bamboo_AMI_ID", "elasticbeanstalk-amzn2-core-dotnetcore-*");
var dbMasterUsername = Argument<string>("dbMasterUsername", "connectionboxdbapplicationuser");
var target = Argument("target", "Default");
var nlogConfigFile = Argument("nlogConfigFile", "");
var logPath = Argument("loggingPath",  @"/tmp");
var splunkMinLevel = Argument("splunkMinLevel", "Info");
var fileMinLevel = Argument("loggingMinFileLevel", "Info");
var fileEnabled = !string.IsNullOrWhiteSpace(Argument("loggingFileEnabled", ""));
var connectionTimeout = Argument("connectionTimeout", 0);
var bomFile = File("./bom.xml");
var dataLakeAccountId = EnvironmentVariableStrict("bamboo_DATALAKE_ACCOUNT_ID");
var regionSystemName = EnvironmentVariableOrDefault("bamboo_REGION", "eu-west-1");
var deployAurora = EnvironmentVariableOrDefault("bamboo_DEPLOY_AURORA", "yes");
var snapshotIdentifier = EnvironmentVariableOrDefault("bamboo_SNAPSHOT_IDENTIFIER", "none");

// Config
//var region = RegionEndpoint.EUWest1;
var region = RegionEndpoint.GetBySystemName(regionSystemName);
var beanstalkEnvironmentName =  "css-" + appName + "-" + environmentName;
var environmentConfig = GetEnvironmentConfiguration(environmentName);
var temp = Directory("./temp");
var lambdaHandlerZip = File("sns_eventdestination_creation.zip");
var appSettingsPath = temp + File("webPackage/appsettings.json");

var deploymentPackageZip = File("./bundle.zip");
var assetsZip = File("./assets.zip");
var configDir = Directory("config");
var appSettings = new JObject();
var isRunningInBamboo = Environment.GetEnvironmentVariables().Keys.Cast<string>().Any(k => k.StartsWith("bamboo_", StringComparison.OrdinalIgnoreCase));
var lambdaFunctionArn = "sns_eventdestination_creation-"+environmentName;

string amiFilter;
string changeRequestNumber;
string accountId;

AWSCredentials awsCredentials;

TagsInfoBuilder baseTagsBuilder = TagsInfoBuilder.Create()
    .WithEnvironmentName(environmentName)
    .WithFinanceEntityId("0092")
    .WithFinanceActivityId("8001")
    .WithFinanceManagementCentreId("99450")
    .WithPmProgramme("digital transformation")
    .WithPmProjectCode("PRJ0093")
    .WithJiraProjectCode("CSS")
    .WithServiceName(appName);
TagsInfo baseTagsInfo = baseTagsBuilder;
TagsInfo tagsInfo = baseTagsBuilder.WithEnvironmentName(environmentName);

//////////////////////////////////////////////////////////////////////
// Cloudformation Config
//////////////////////////////////////////////////////////////////////

// Will hold CloudFormation outputs
IDictionary<string, string> cloudFormationOutputs;

// To be determined later as stack names are inconsitent across enviroments
string cloudFormationStackName;
var cloudFormationTemplatePath = configDir + File("cloudFormationTemplate.yaml");

//////////////////////////////////////////////////////////////////////
// PRE-DEPLOY
//////////////////////////////////////////////////////////////////////

Task("Initialise")
    .Does(() =>
    {
        //////////////////////////////////////////////////////////////////////
        // Authenticate with AWS.
        // See
        // https://bitbucket.org/coreshowservices/reedexpo.cake.aws.buildauthentication
        //////////////////////////////////////////////////////////////////////
        awsCredentials = GetBuildAwsCredentials(region: region);
        accountId = GetAwsAccountId(awsCredentials);
        CleanDirectory(temp);
    });

Task("UnpackageWebService")
    .Does(() =>
    {
        Unzip(deploymentPackageZip, temp + Directory("webPackage"));
    });

Task("UnpackageAssets")
    .Does(() =>
    {
        Unzip(assetsZip, temp + Directory("assets"));
    });

Task("UploadAssetsToS3")
    .IsDependentOn("UnpackageAssets")
    .Does(async () => {
        await upload_assets_to_s3(awsCredentials, region);
    });

Task("UploadHandlersToCodeBucket")
    .Does(async () => {
        var handlersToUpload = new Dictionary<string, string> {
            {lambdaHandlerZip , "custom-resource-handlers/sns_eventdestination_creation.zip"}
        };
        var codeBucketName = $"ratboxcodebucket-{environmentName}-{regionSystemName}-{accountId}";
        await upload_to_code_bucket(awsCredentials, region, codeBucketName, handlersToUpload);
    });

Task("UpdateEbExtensionsForEnvironment")
    .IsDependentOn("UnpackageWebService")
    .IsDependentOn("PublishLambda") // Stack outpus are needed here
    .Does(() =>
    {
        // Copy over the ebextensions configuration
        var webPackageDir = temp + Directory("webPackage");
        var packageEbExtensions = webPackageDir + Directory(".ebextensions");
        EnsureDirectoryExists(packageEbExtensions);

        var targetFilePath = packageEbExtensions + File("101.config");

        CopyFile(configDir + Directory(environmentName) + File("elasticbeanstalk.yaml"), targetFilePath );

        // Build list of instance security groups - initially with the cloudFormation output
        var instanceSecurityGroups = new List<string>
        {
            // This will not need to be edited if you have done renames in the CloudFormation template as described in
            // https://bitbucket.org/coreshowservices/api-service-template/src/master/README.md
            cloudFormationOutputs[$"{appName}PostgresDbAccessSecurityGroup"]
        };

        // Append security groups with any that were set in plan vars
        var userSecurityGroups = EnvironmentVariable("bamboo_SECURITY_GROUPS");

        if (!string.IsNullOrWhiteSpace(userSecurityGroups))
        {
            instanceSecurityGroups.AddRange(userSecurityGroups.Split(new [] {',', ';'}, StringSplitOptions.RemoveEmptyEntries));
        }

        // Any substitution in elasticBeanstalk.yaml that has no default and is not a bamboo variable must be added here
        var elasticBeanstalkParameters = new Dictionary<string, string>
        {
            { "cloudformationOutput_AWS_IAM_INSTANCE_PROFILE", cloudFormationOutputs[$"{appName}InstanceProfile"] },
            { "cloudformationOutput_ELASTIC_BEANSTALK_SERVICE_ROLE", cloudFormationOutputs[$"{appName}ElasticBeanstalkServiceRole"] },
            { "precomputed_SECURITY_GROUPS", string.Join(",", instanceSecurityGroups.Select(g => g.ToLower()).Distinct()) },
            { "precomputed_AMI_IMAGE_ID", GetAwsImageId(amiName, awsCredentials, region.SystemName) }
        };

        ReplaceTokensInElasticBeanstalkConfigFile(targetFilePath, elasticBeanstalkParameters);

        Information("UPDATED EB EXTENSIONS FILE");
        Information("======== BEGIN =========");
        Information(FileReadText(targetFilePath));
        Information("======== END =========");
    });

Task("RepackageWebService")
    .Does(() =>
    {
        // Re-zip the web deployment package, which targets a Windows Elastic Beanstalk stack.
        ZipX(new ZipSettings {
            ZipFile = deploymentPackageZip,
            Artifacts = (temp + Directory("webPackage")).ToString(),
            TargetPlatform = ZipPlatform.Unix
        });
    });

Task("UpdateAppVersionNumber")
    .Does(() => {
        appSettings["Version"] = deployVersion;
    });

Task("UpdateJwt")
    .Does(() => {
        appSettings["Jwt"]["SecretKey"] = environmentConfig.jwtSecretKey ?? jwtSecretKey;
        appSettings["Jwt"]["Issuer"] = environmentConfig.jwtIssuer;
    });

Task("UpdateAppSettings")
    .Does(() =>{
        appSettings["AppSettings"]["ServiceName"] = environmentConfig.ServiceName;
        appSettings["AppSettings"]["AppVersion"] = deployVersion;
    });

Task("UpdateRXLogging")
    .Does(() => {
        appSettings["RXLogging"]["Loglevel"]["Default"] = environmentConfig.SplunkLogLevel;
        appSettings["RXLogging"]["splunkLogLevel"] = environmentConfig.SplunkLogLevel;
        appSettings["RXLogging"]["splunkTarget"] = environmentConfig.SplunkTarget;
        appSettings["RXLogging"]["splunkUrl"] = environmentConfig.SplunkUrl;
    });

Task("UpdateSenderEmailAddress")
    .Does(() => {
        appSettings["SenderEmail"] = environmentConfig.senderEmail;
    });

Task("UpdateXSESConfigurationSet")
    .Does(() => {
        appSettings["X-SES-CONFIGURATION-SET"] = environmentConfig.XSESConfigurationSet;
    });

Task("UpdateDataLakeAccountId")
    .Does(() => {
        appSettings["DataLakeAccountId"] = environmentConfig.DataLakeAccountId ?? dataLakeAccountId;
    });

Task("UpdateVisitorSMSUrls")
    .Does(() => {
        appSettings["UrlShortenerEndpoint"] = environmentConfig.UrlShortenerEndpoint;
        appSettings["UrlShortenerApiKey"] = environmentConfig.UrlShortenerApiKey ?? UrlShortenerApiKey;
        appSettings["visitorAcknowledgementReport"] = environmentConfig.visitorAcknowledgementReport;
    });

Task("UpdateExternalUrls")
    .Does(() => {
        appSettings["AppStoreUrl"] = environmentConfig.appStoreUrl;
        appSettings["PlayStoreUrl"] = environmentConfig.playStoreUrl;
        appSettings["AssetsS3BucketUrl"] = environmentConfig.assetsS3BucketUrl;
        appSettings["AssetsS3BucketRootUrl"] = environmentConfig.assetsS3BucketRootUrl;
        appSettings["AssetsS3Bucket"] = environmentConfig.assetsS3Bucket;
    });

Task("UpdateExternalHostUrl")
    .Does(() => {
        appSettings["ExternalHostUrl"] = environmentConfig.externalHostUrl;
    });

Task("UpdateApigeeApiKey")
    .Does(() => {
        appSettings["ApigeeApiKey"] = environmentConfig.apigeeApiKey ?? apigeeApiKey;
    });

Task("UpdateAuthorityUrl")
    .Does(() => {
        appSettings["AuthorityUrl"] = environmentConfig.authorityUrl;
        });

Task("UpdateShowServiceUrl")
    .Does(() => {
        appSettings["showServiceUrl"] = environmentConfig.showServiceUrl;
        });

Task("UpdateEmitEventsforDataLake")
    .Does(() => {
        appSettings["emitEventsforDataLake"] = enableEvents;
        });

Task("UpdateEnvironmentName")
    .Does(() => {
        appSettings["environmentName"] = environmentConfig.environmentName;
        });

Task("UpdateRegboxEnvironmentName")
    .Does(() => {
        appSettings["regBoxEnvironmentName"] = environmentConfig.regBoxEnvironmentName;
        });

Task("UpdateExboxServiceUrl")
    .Does(() => {
        appSettings["ExboxServiceUrl"] = environmentConfig.exboxServiceUrl;
        });

Task("UpdateRegboxUrl")
    .Does(() => {
        appSettings["RegboxUrl"] = environmentConfig.RegboxUrl;
        });

Task("UpdateEntitlementsUrl")
    .Does(() => {
        appSettings["EntitlementsUrl"] = environmentConfig.EntitlementsUrl;
        });

Task("UpdatePoliceboxUrl")
    .Does(() => {
        appSettings["PoliceboxUrl"] = environmentConfig.PoliceboxUrl;
        });

Task("UpdateShowExhibitorHubUrl")
    .Does(() => {
        appSettings["showExhibitorHubUrl"] = environmentConfig.showExhibitorHubUrl;
       });

Task("UpdateShowSettings")
        .Does(() => {
            appSettings["ShowActiveDays"] = environmentConfig.showActiveDays;
        });

Task("UpdateClientId&SecretKey")
        .Does(() => {
            appSettings["CheckboxClientId"] = environmentConfig.CheckboxClientId ?? CheckboxClientId;
            appSettings["CheckboxClientSecret"] = environmentConfig.CheckboxClientSecret ?? CheckboxClientSecret;
        });

Task("UpdateUserServiceUrl")
        .Does(() => {
            appSettings["UserServiceUrl"] = environmentConfig.UserServiceUrl;
        });

Task("UpdateServiceName")
        .Does(() => {
            appSettings["ServiceName"] = environmentConfig.ServiceName;
        });

Task("UpdateSnsServiceURL")
        .Does(() => {
            appSettings["SnsEndpoint"] = environmentConfig.SnsEndpoint;
        });

Task("UpdateEnableEventBus")
        .Does(() => {
            appSettings["EnableEventBus"] = Boolean.Parse(enableEventBus) || environmentConfig.EnableEventBus;
        });

Task("UpdateMaxDbRetryCount")
        .Does(() => {
            appSettings["maxDbRetryCount"] = environmentConfig.maxDbRetryCount;
        });

Task("UpdateEnableEntitlementHandlerJob")
        .Does(() => {
            appSettings["EnableEntitlementHandlerJob"] = Boolean.Parse(enableEntitlementHandlerJob) || environmentConfig.EnableEntitlementHandlerJob;
        });

Task("UpdatePartyboxClientId")
    .Does(() => {
        appSettings["PartyboxClientId"] = environmentConfig.PartyboxClientId ?? PartyboxClientId;
        });

Task("UpdatePartyboxClientSecret")
    .Does(() => {
        appSettings["PartyboxClientSecret"] = environmentConfig.PartyboxClientSecret ?? PartyboxClientSecret;
        });

Task("UpdateWatchboxClientId")
    .Does(() => {
        appSettings["WatchboxClientId"] = environmentConfig.WatchboxClientId ?? WatchboxClientId;
        });

Task("UpdateWatchboxClientSecret")
    .Does(() => {
        appSettings["WatchboxClientSecret"] = environmentConfig.WatchboxClientSecret ?? WatchboxClientSecret;
        });

Task("UpdateExboxClientId")
    .Does(() => {
        appSettings["ExboxClientId"] = environmentConfig.ExboxClientId ?? ExboxClientId;
        });

Task("UpdateExhibitorWriteCredentials")
    .Does(() => {
        appSettings["ExhibitorWriteClientId"] = environmentConfig.ExhibitorWriteClientId ?? ExhibitorWriteClientId;
        appSettings["ExhibitorWriteClientSecret"] = environmentConfig.ExhibitorWriteClientSecret ?? ExhibitorWriteClientSecret;
        });

Task("UpdateExboxClientSecret")
    .Does(() => {
        appSettings["ExboxClientSecret"] = environmentConfig.ExboxClientSecret ?? ExboxClientSecret;
        });
Task("UpdateEntitlementServiceClientId")
    .Does(() => {
        appSettings["EntitlementServiceClientId"] = environmentConfig.EntitlementServiceClientId ?? EntitlementServiceClientId;
        });

Task("UpdateEntitlementsSplitEventEditionId")
    .Does(() => {
        appSettings["entitlementsSplitEventEditionId"] = environmentConfig.entitlementsSplitEventEditionId ?? entitlementsSplitEventEditionId;
        });

Task("UpdateAppEntitlementProductCode")
    .Does(() => {
        appSettings["appEntitlementProductCode"] = appEntitlementProductCode;
        });

Task("UpdateReaderEntitlementProductCode")
    .Does(() => {
        appSettings["readerEntitlementProductCode"] = readerEntitlementProductCode;
        });

Task("UpdateAppEntitlementProductNumber")
    .Does(() => {
        appSettings["appEntitlementProductNumber"] = appEntitlementProductNumber;
        });

Task("UpdateReaderEntitlementProductNumber")
    .Does(() => {
        appSettings["readerEntitlementProductNumber"] = readerEntitlementProductNumber;
        });


Task("UpdateEntitlementServiceClientSecret")
    .Does(() => {
        appSettings["EntitlementServiceClientSecret"] = environmentConfig.EntitlementServiceClientSecret ?? EntitlementServiceClientSecret;
        });

Task("UpdateRingboxServiceUrl")
        .Does(() => {
            appSettings["RingboxServiceUrl"] = environmentConfig.RingboxServiceUrl;
        });

Task("UpdateWatchBoxServiceUrl")
        .Does(() => {
            appSettings["WatchBoxServiceUrl"] = environmentConfig.WatchBoxServiceUrl;
        });

Task("UpdateJobCronExp")
        .Does(() => {
            appSettings["ExhibitorStatsJobCronExp"] = environmentConfig.ExhibitorStatsJobCronExp;
            appSettings["PublishVisitorEventsJobCronExp"] = environmentConfig.PublishVisitorEventsJobCronExp;
            appSettings["PPSRefreshJobCronExp"] = environmentConfig.PPSRefreshJobCronExp;
            appSettings["LeadSummaryDigestEmailJobCronExp"] = environmentConfig.LeadSummaryDigestEmailJobCronExp;
        });

Task("UpdateConnectionString")
    .Does(() => {
        var connectionString = "Server=" + environmentConfig.postgresDbUrl +
                               ";Database=" + environmentConfig.dbName + ";" +
                               (connectionTimeout > 0 ? "Timeout=" + connectionTimeout + ";" : "") +
                               "User Id=" + environmentConfig.dbMasterUsername +
                               ";Password=" + dbMasterPassword;
        appSettings["ConnectionStrings"]["PostgresConnectionsDatabase"] = connectionString;
    });

Task("UpdateAwsRegion")
    .Does(()=>{
        appSettings["AwsRegion"] = regionSystemName;
    });

Task("UpdateAthenaConfig")
        .Does(() => {
            appSettings["AthenaOutputLocation"] = AthenaOutputLocation;
            appSettings["AthenaDatabaseName"] = AthenaDatabaseName;
            appSettings["AthenaRatBoxDatabaseName"] = AthenaRatBoxDatabaseName;
        });

Task("UpdateFeatureToggles")
    .Does(() => {

        if (environmentConfig.featureToggle != null)
        {
            // Override/add any features defined for this environment
            foreach (var kv in environmentConfig.featureToggle)
            {
                //((JValue)(kv.Value)).Value.GetType().Name.Dump();
                appSettings["featureToggle"][kv.Key] = (bool)((JValue)(kv.Value)).Value;
            }
        }

        // Get any feature toggles from plan variable.
        var planFeatures = EnvironmentVariable("bamboo_FEATURE_TOGGLES")?.Split(new [] { ',', ';'}, StringSplitOptions.RemoveEmptyEntries);

        if (planFeatures != null)
        {
            // Override/add any features defined by plan variables (highest precedence)
            // The feature name existing in the plan variable implies its value is True
            foreach (var f in planFeatures)
            {
                appSettings["featureToggle"][f] = true;
            }
        }
    });

Task("CreateNlogConfig")
    .Does(() => {
        // Transform square brackets eg [toTransform]. Curley brackets are used by nlog.
        string transformedText = TransformTextFile("config/nlog.config.template", "[", "]")
            .WithToken("logPath", logPath)
            .WithToken("splunkMinLevel", splunkMinLevel)
            .WithToken("fileMinLevel", fileMinLevel)
            .WithToken("fileEnabled", fileEnabled.ToString().ToLower())
            .WithToken("environment", environmentName)
            .ToString();

        string outputFile = string.IsNullOrWhiteSpace(nlogConfigFile) ? temp + File("webPackage/nlog.config") : nlogConfigFile;
        System.IO.File.WriteAllText(outputFile, transformedText);
    });


//////////////////////////////////////////////////////////////////////
// CLOUDFORMATION
//////////////////////////////////////////////////////////////////////

Task("DetermineCloudFormationStackName")
    .Does(() => {

        switch (accountId)
        {
            case "915203318988":

                // platformrefresh
                cloudFormationStackName = $"ConnectionBox-{environmentName}";
                break;

            case "324811521787":
            case "612155760304":

                // preprod, prod
                cloudFormationStackName = $"connectionbox-{environmentName}";
                break;

            case "950034897475":

                 // drx
                cloudFormationStackName = $"connectionbox-{environmentName}";
                break;

            default:

                throw new Exception($"Unrecoginised AWS account {accountId}");
        }

        Information($"CloudFormation Stack to be updated: {cloudFormationStackName}");
    });

// Note that we run this BEFORE UpdateEbExtensionsForEnvironment
// as stack outputs are used there.
Task("RunCloudFormation")
    .IsDependentOn("DetermineCloudFormationStackName")
    .Does(() => {
        // Database credentials intentionally omitted from parameter list as stack will use existing values.
        // Will only be an issue in unlikely event a new stack needs to be created.
        var result = RunCloudFormation(
            new RunCloudFormationSettings
            {
                Capabilities = new List<Capability> { Capability.CAPABILITY_AUTO_EXPAND, Capability.CAPABILITY_NAMED_IAM },
                Credentials = awsCredentials,
                Region = region,
                StackName = cloudFormationStackName,
                TemplatePath = cloudFormationTemplatePath,
                Parameters = new Dictionary<string, string>
                {
                    { "BillingEnvironmentName", environmentName },
                    { "ForceCloudFormation", forceCloudFormation },
                    { "DeployAurora", deployAurora},
                    { "SnapshotIdentifier", snapshotIdentifier},
                    { "DeployColleqtQrLambda", deployColleqtQrLambda },
                    { "ApigeeApiKey", apigeeApiKey },
                    { "BambooBuildNumber", bambooBuildNumber }
                },
                Tags = tagsInfo
            }
        );

        Information($"Stack update result was {result}");

        // Gather stack outputs
        cloudFormationOutputs = GetCloudFormationStackOutputs(awsCredentials, cloudFormationStackName, region: region);
    });

Task("PublishLambda")
    .IsDependentOn("RunCloudFormation")
    //.IsDependentOn("UploadHandlersToCodeBucket")
    .Description("Publishes a version of your function")
    .Does(() => {
        DeployLambda(new DeployLambdaSettings {
            Credentials = awsCredentials,
            DeploymentPackage = lambdaHandlerZip,
            FunctionName = lambdaFunctionArn,
            Region = region
        });
    });


//////////////////////////////////////////////////////////////////////
// DEPLOYMENT
//////////////////////////////////////////////////////////////////////

Task("InitiateWebDeploy")
    .IsDependentOn("UploadBillOfMaterials")
    .Does(() =>
    {
        try
        {
            DeployToElasticBeanstalk(
                new RunElasticBeanstalkSettings {
                    Credentials = awsCredentials,
                    DeploymentPackage = deploymentPackageZip.ToString(),
                    ApplicationName = appName,
                    EnvironmentName = beanstalkEnvironmentName,
                    Tags = tagsInfo,
                    TargetOperatingSystem = "64bit Amazon Linux 2", // <- New envs will deploy with latest EB solution stack for this operating system
                    TargetPlatform = ".NET Core",
                    Region = region
                }
            );
        }
        catch (CakeException e) when (e.InnerException != null)
        {
            DumpExceptionDetail(e.InnerException);
            throw;
        }
        catch (Exception e)
        {
            DumpExceptionDetail(e);
            throw;
        }
    });

Task("DeleteDefaultAppSettings")
	.Does(() =>
	{
		DeleteFile(appSettingsPath);
		Verbose("Delete default appSettings.config");
	});

Task("InitializeDefaultAppSettings")
	.Does(() =>
	{
		appSettings = GetAppSettings();
		Verbose("Initialize default app settings");
	});

Task("SaveAppSettings")
    .Does(() => {
        Verbose("Save appSettings: " + appSettings);
        UpdateAppSettings(appSettings);
    });

Task("UploadBillOfMaterials")
    .WithCriteria(environmentName == "prod")
    .Does(() => {
        UploadBillOfMaterials(
            new UploadBillOfMaterialsSettings {
                ApplicationName = appName,
                BomFile = File(bomFile),
                EnvironmentName = environmentName
            }
        );
    });

Task("RaiseChangeRequest")
    .WithCriteria(environmentName == "prod")
    .Does(() => {
        changeRequestNumber = RaiseChangeRequest(new ChangeRequestSettings());
    });

//////////////////////////////////////////////////////////////////////
// POST DEPLOYMENT
//////////////////////////////////////////////////////////////////////

Teardown(context => {
        // Teardown is EXECUTED by -WhatIf. Only permit execution when running in Bamboo
        if (isRunningInBamboo && !string.IsNullOrWhiteSpace(changeRequestNumber))
        {
            CloseChangeRequest(new ChangeRequestSettings
            {
                SystemId = changeRequestNumber,
                Success  = context.Successful,
            });
        }
 });

//////////////////////////////////////////////////////////////////////
// ENTRIES
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Initialise")
    .IsDependentOn("UnpackageWebService")
    .IsDependentOn("RaiseChangeRequest")
    .IsDependentOn("CreateNlogConfig")
    .IsDependentOn("UpdateEbExtensionsForEnvironment")
	.IsDependentOn("InitializeDefaultAppSettings")
    .IsDependentOn("UpdateAppVersionNumber")
    .IsDependentOn("UpdateConnectionString")
    .IsDependentOn("UpdateJwt")
    .IsDependentOn("UpdateAppSettings")
    .IsDependentOn("UpdateRXLogging")
    .IsDependentOn("UpdateSenderEmailAddress")
    .IsDependentOn("UpdateXSESConfigurationSet")
    .IsDependentOn("UpdateVisitorSMSUrls")
    .IsDependentOn("UpdateExternalUrls")
    .IsDependentOn("UpdateExternalHostUrl")
    .IsDependentOn("UpdateApigeeApiKey")
    .IsDependentOn("UpdateShowSettings")
    .IsDependentOn("UpdateShowServiceUrl")
    .IsDependentOn("UpdateExboxServiceUrl")
    .IsDependentOn("UpdateRegboxUrl")
    .IsDependentOn("UpdateEmitEventsforDataLake")
    .IsDependentOn("UpdateEnvironmentName")
    .IsDependentOn("UpdateRegBoxEnvironmentName")
    .IsDependentOn("UpdateAuthorityUrl")
    .IsDependentOn("UpdateClientId&SecretKey")
    .IsDependentOn("UpdateUserServiceUrl")
    .IsDependentOn("UpdateServiceName")
    .IsDependentOn("UpdateSnsServiceURL")
    .IsDependentOn("UpdateEnableEventBus")
    .IsDependentOn("UpdateMaxDbRetryCount")
    .IsDependentOn("UpdateEnableEntitlementHandlerJob")
    .IsDependentOn("UpdatePartyboxClientId")
    .IsDependentOn("UpdatePartyboxClientSecret")
    .IsDependentOn("UpdateWatchboxClientId")
    .IsDependentOn("UpdateWatchboxClientSecret")
    .IsDependentOn("UpdateExboxClientId")
    .IsDependentOn("UpdateExboxClientSecret")
    .IsDependentOn("UpdateEntitlementServiceClientId")
    .IsDependentOn("UpdateEntitlementsSplitEventEditionId")
    .IsDependentOn("UpdateEntitlementServiceClientSecret")
    .IsDependentOn("UpdateExhibitorWriteCredentials")
    .IsDependentOn("UpdateReaderEntitlementProductNumber")
    .IsDependentOn("UpdateAppEntitlementProductNumber")
    .IsDependentOn("UpdateReaderEntitlementProductCode")
    .IsDependentOn("UpdateAppEntitlementProductCode")
    .IsDependentOn("UpdateRingboxServiceUrl")
    .IsDependentOn("UpdateWatchBoxServiceUrl")
    .IsDependentOn("UpdateDataLakeAccountId")
    .IsDependentOn("UpdateJobCronExp")
    .IsDependentOn("UpdateAwsRegion")
    .IsDependentOn("UpdateAthenaConfig")
    .IsDependentOn("UpdateFeatureToggles")
    .IsDependentOn("UploadAssetsToS3")
    .IsDependentOn("SaveAppSettings")
    .IsDependentOn("RepackageWebService")
    .IsDependentOn("InitiateWebDeploy")
    .IsDependentOn("UploadBillOfMaterials");

RunTarget(target);

//////////////////////////////////////////////////////////////////////
// HELPER FUNCTIONS/CLASSES
//////////////////////////////////////////////////////////////////////

void DumpExceptionDetail(Exception e)
{
    Information($"Exception: {e.GetType().Name}");
    Information($"Stack Trace:\n{e.StackTrace}");
}

class EnvironmentConfiguration
{
    public string healthcheckUrl { get; set; }
    public string uploadedFilesBucketName { get; set; }
    public string jwkCacheTimeHours { get; set; }
    public string jwtSecretKey { get; set; }
    public string jwtIssuer { get; set; }
    public string postgresDbUrl { get; set; }
    public string dbName { get; set; }
    public string dbMasterUsername { get; set; }
    public string senderEmail { get; set; }
    public string playStoreUrl { get; set; }
    public string appStoreUrl { get; set; }
    public string externalHostUrl { get; set; }
    public string apigeeApiKey { get; set; }
    public string assetsS3BucketUrl { get; set; }
    public string assetsS3BucketRootUrl { get; set; }
    public string assetsS3Bucket { get; set; }
    public string showActiveDays { get; set; }
    public string authorityUrl { get; set; }
    public string showServiceUrl { get; set; }
    public string exboxServiceUrl { get; set; }
    public string RegboxUrl { get; set; }
    public string EntitlementsUrl { get; set; }
    public string PoliceboxUrl { get; set; }
    public bool emitEventsforDataLake { get; set; }
    public string entitlementsSplitEventEditionId { get; set; }
    public string environmentName { get; set; }
    public string regBoxEnvironmentName { get; set; }
    public bool showExhibitorHubUrl { get; set; }
    public string CheckboxClientId { get; set; }
    public string CheckboxClientSecret { get; set; }
    public string PartyboxClientId { get; set; }
    public string PartyboxClientSecret { get; set; }
    public string WatchboxClientId { get; set; }
    public string WatchboxClientSecret { get; set; }
    public string ExboxClientId { get; set; }
    public string ExboxClientSecret { get; set; }
    public string EntitlementServiceClientId { get; set; }
    public string EntitlementServiceClientSecret { get; set; }
    public string RingboxServiceUrl { get; set; }
    public string WatchBoxServiceUrl { get; set; }
    public string ServiceName { get; set; }
    public string UserServiceUrl { get; set; }
    public string SnsEndpoint { get; set; }
    public bool EnableEventBus { get; set; }
    public bool EnableEntitlementHandlerJob { get; set; }
    public string minPool {get; set;}
    public string maxPool {get; set;}
    public int maxDbRetryCount {get; set;}
  	public JObject featureToggle { get; set; }
  	public string XSESConfigurationSet { get; set; }
  	public string visitorAcknowledgementReport { get; set; }
  	public string UrlShortenerEndpoint { get; set; }
  	public string UrlShortenerApiKey { get; set; }
  	public string ExhibitorStatsJobCronExp { get; set; }
  	public string PublishVisitorEventsJobCronExp { get; set; }
  	public string PPSRefreshJobCronExp { get; set; }
  	public string LeadSummaryDigestEmailJobCronExp { get; set; }
  	public string ExhibitorWriteClientSecret {get; set;}
  	public string ExhibitorWriteClientId {get; set;}
  	public string SplunkLogLevel { get; set; }
  	public string SplunkTarget { get; set; }
  	public string SplunkUrl { get; set; }
  	public string DataLakeAccountId {get; set;}
}

EnvironmentConfiguration GetEnvironmentConfiguration(string environment)
{
    return JsonConvert.DeserializeObject<EnvironmentConfiguration>(System.IO.File.ReadAllText("config/" + environmentName + "/environment.json"));
}

JObject GetAppSettings()
{
    return JObject.Parse(System.IO.File.ReadAllText(appSettingsPath));
}

void UpdateAppSettings(JObject appSettings)
{
    var serialized = JsonConvert.SerializeObject(appSettings);
    FileWriteText(appSettingsPath, serialized);
}

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

string EnvironmentVariableOrDefault(string key, string defaultValue)
{
    var value = EnvironmentVariable(key);
    return value ?? defaultValue;
}


async Task upload_to_code_bucket(AWSCredentials awsCredentials, Amazon.RegionEndpoint region, string codeBucketName,
                          Dictionary<string, string> filesToUpload){
    var immutableCredentials = awsCredentials.GetCredentials();

    // https://github.com/SharpeRAD/Cake.AWS.S3
    var uploadSettings = new UploadSettings {
        AccessKey = immutableCredentials.AccessKey,
        SecretKey = immutableCredentials.SecretKey,
        SessionToken = immutableCredentials.Token,
        Region = region,
        BucketName = codeBucketName
    };

    foreach (var kv in filesToUpload)
    {
        Information($"Uploading {kv.Key} to s3://{codeBucketName}/{kv.Value}");

        await S3Upload(kv.Key, kv.Value, uploadSettings);

        Information("Upload code success");
    }
}

async Task upload_assets_to_s3(AWSCredentials awsCredentials, Amazon.RegionEndpoint region) {
    var immutableCredentials = awsCredentials.GetCredentials();
    var assetsDir = temp + Directory("assets");
    var assets = GetFiles($"{assetsDir}/*.*");
    var bucketName = environmentConfig.assetsS3Bucket;

    // https://github.com/SharpeRAD/Cake.AWS.S3
    var uploadSettings = new UploadSettings {
        AccessKey = immutableCredentials.AccessKey,
        SecretKey = immutableCredentials.SecretKey,
        SessionToken = immutableCredentials.Token,
        Region = region,
        BucketName = bucketName,
        CannedACL = S3CannedACL.PublicRead,
    };

    foreach (var kv in assets)
    {
        Information($"Uploading {kv} to s3://{bucketName}/assets/build/{kv}");

        await S3Upload(kv, "assets/build/"+kv.GetFilename(), uploadSettings);

        Information($"Asset {kv.GetFilename()} upload success");
    }
}
