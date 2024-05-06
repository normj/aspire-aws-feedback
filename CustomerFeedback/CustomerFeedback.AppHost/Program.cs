using Amazon;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig()
                        .WithProfile("default")
                        .WithRegion(RegionEndpoint.USWest2);

var awsResources = builder.AddAWSCloudFormationTemplate("CustomerFeedbackAppResources", "aws-resources.template")
                        .WithReference(awsConfig);

builder.AddProject<Projects.CustomerFeedback_Frontend>("customerfeedback-frontend")
       .WithReference(awsResources);

builder.AddProject<Projects.CustomerFeedback_Processor>("customerfeedback-processor")
       .WithReference(awsResources);

builder.Build().Run();
