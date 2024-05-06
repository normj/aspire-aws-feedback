using Amazon;

var builder = DistributedApplication.CreateBuilder(args);

var awsSdkConfig = builder.AddAWSSDKConfig()
                          .WithRegion(RegionEndpoint.USWest2);

var ddbLocal = builder.AddAWSDynamoDBLocal("ddblocal");

builder.AddProject<Projects.ZipCodes_DataLoader>("zipcodes-dataloader")
       .WithEnvironment("AWS__Resources__ZipCodesTable", "LocalTable")
       .WithReference(awsSdkConfig)
       .WithReference(ddbLocal);

var apiProject = builder.AddProject<Projects.ZipCodes_API>("zipcodes-api")
                        .WithEnvironment("AWS__Resources__ZipCodesTable", "LocalTable")
                        .WithReference(awsSdkConfig)
                        .WithReference(ddbLocal);

builder.AddProject<Projects.ZipCodes_Frontend>("zipcodes-frontend")
        .WithReference(apiProject);


builder.Build().Run();
