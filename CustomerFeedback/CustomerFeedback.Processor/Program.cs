using CustomerFeedback.Models;
using CustomerFeedback.Processor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var awsResources = new AWSResources();
builder.Configuration.Bind("AWS:Resources", awsResources);

if (string.IsNullOrEmpty(awsResources.FeedbackQueueUrl))
{
    throw new ApplicationException("Missing required configuration for feedback queue url");
}

builder.Services.AddAWSMessageBus(builder =>
{
    builder.AddSQSPoller(awsResources.FeedbackQueueUrl);

    builder.AddMessageHandler<FeedbackHandler, FeedbackSubmission>();
});

builder.Services.AddAWSService<Amazon.Comprehend.IAmazonComprehend>();

builder.Build().Run();