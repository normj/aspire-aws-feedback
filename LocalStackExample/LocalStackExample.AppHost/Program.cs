using Amazon.CloudFormation;
using Amazon.SimpleNotificationService;
using Amazon.SQS;


var builder = DistributedApplication.CreateBuilder(args);

var localStackUri = new Uri("http://localhost:4566");

var cfConfig = new AmazonCloudFormationConfig
{
    ServiceURL = localStackUri.ToString()
};
var cfClient = new AmazonCloudFormationClient(cfConfig);

var localStack = builder.AddContainer("localstack", "localstack/localstack")
                        .WithEndpoint(port: localStackUri.Port, targetPort: 4566, scheme: "http")
                        .WithEnvironment("DEBUG", "1");

var awsResources = builder.AddAWSCloudFormationTemplate("LocalStackExample-Stack", "aws-resources.template");

awsResources.Resource.CloudFormationClient = cfClient;

builder.AddProject<Projects.LocalStackExample_Frontend>("LocalStackExample-frontend")
       .WithReference(awsResources)
       .WithEnvironment($"AWS_ENDPOINT_URL_{AmazonSimpleNotificationServiceConfig.ServiceId}", localStackUri.ToString())
       .WithEnvironment($"AWS_ENDPOINT_URL_{AmazonSQSConfig.ServiceId}", localStackUri.ToString());

builder.AddProject<Projects.LocalStackExample_Processor>("LocalStackExample-processor")
       .WithReference(awsResources)
       .WithEnvironment($"AWS_ENDPOINT_URL_{AmazonSimpleNotificationServiceConfig.ServiceId}", localStackUri.ToString())
       .WithEnvironment($"AWS_ENDPOINT_URL_{AmazonSQSConfig.ServiceId}", localStackUri.ToString());

builder.Build().Run();
 