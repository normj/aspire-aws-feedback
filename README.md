## AWS experiments with Aspire

This repository demos projects combining AWS and .NET Aspire together. Feedback and suggestions
for AWS and .NET Aspire are welcome. Please open an issue or discussion to discuss ideas.


### Customer Feedback project (Demonstrate provisioning app resources with CloudFormation)

The project simulates a product feedback page which submits feedback submission to a backend 
processor for analysis. Currently the analysis only does a sentiment analysis and logs the
sentiment. A full sample would record the analysis in the backend and 
possibly trigger action based on the sentiment.

The project uses a Blazor frontend that publishes messages using the [AWS.Messaging](https://github.com/awslabs/aws-dotnet-messaging)
library. The `CustomerFeedback.Processor` processing messages again using the [AWS.Messaging](https://github.com/awslabs/aws-dotnet-messaging)
library.

Opentelemetry instrumentation has been enabled for both AWS SDK and the AWS.Messaging library
in the `ZipCodes.ServiceDefaults`.

```
tracing.AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddAWSInstrumentation()
    .AddAWSMessagingInstrumentation();
```

The project requires an Amazon Simple Queue Service (SQS) queue and Amazon Simple Notification Service (SNS)
topic for communicating between the layers. To create these AWS application resources the [Aspire.Hosting.AWS](https://www.nuget.org/packages/Aspire.Hosting.AWS)
package is used. In the `CustomerFeedback.AppHost` project the `aws-resources.template` CloudFormation template defines
the required AWS resources used by the project. The project then uses the `AddAWSCloudFormationTemplate` method
to create a CloudFormation stack defined by the CloudFormation template.

```
var awsConfig = builder.AddAWSSDKConfig()
                        .WithProfile("default")
                        .WithRegion(RegionEndpoint.USWest2);

var awsResources = builder.AddAWSCloudFormationTemplate("CustomerFeedbackAppResources", "aws-resources.template")
                          .WithReference(awsConfig);
```

Projects that use these AWS resources declare their dependency using the `WithReference` method.

```
builder.AddProject<Projects.CustomerFeedback_Frontend>("customerfeedback-frontend")
       .WithReference(awsResources);
```

The `WithReference` will cause the output parameters of the CloudFormation stack to be assigned the 
project using environment variables.

### ZipCode lookup (Demonstrate AWS DynamoDB Local)

The project uses a Blazor frontend talking to a WEB API project for looking up US postal zip codes. In the search
box you can search by:
* Code like `98052` for Redmond Washington
* State 2 letter acronym like `WA` for Washington
* City like `Redmond`

The `ZipCodes.DataLoader` project ensures the table exists and loads the table from a csv file.

This project demonstrates how AWS [DynamoDB Local](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html) 
can be integrated into the AppHost. DynamoDB Local is the emulator for the AWS DynamoDB service
that can be run as a container. The project uses some experimental API in the AppHost's `Aspire.Hosting.AWS.Experimental`
folder for streamlining add the DynamoDB Local container and referencing it to projects. It would be really 
helpful to hear if these APIs are worth adding to the `Aspire.Hosting.AWS` package.


### LocalStack example

This is a copy of the Customer Feedback demo but using LocalStack configured in the AppHost. There are no plans to add
first class support for LocalStack to the `Aspire.Hosting.AWS` package. Could be interesting for somebody else
to create a `Aspire.Hosting.LocalStack` package that simplifies what I'm doing in this sample. I'm open to make sure
`Aspire.Hosting.AWS` has the right extension points to make this easier.