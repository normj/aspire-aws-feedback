using Microsoft.Extensions.Logging;
using AWS.Messaging;
using CustomerFeedback.Models;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;

namespace CustomerFeedback.Processor;

public class FeedbackHandler(ILogger<FeedbackHandler> logger, IAmazonComprehend comprehend)  : IMessageHandler<FeedbackSubmission>
{
    public async Task<MessageProcessStatus> HandleAsync(MessageEnvelope<FeedbackSubmission> messageEnvelope, CancellationToken token = default(CancellationToken))
    {
        logger.LogInformation("Processing feedback for {product}", messageEnvelope.Message.ProductName);

        var detectRequest = new DetectSentimentRequest
        {
            Text = messageEnvelope.Message.Feedback,
            LanguageCode = "en"
        };

        var detectResponse = await comprehend.DetectSentimentAsync(detectRequest);

        logger.LogInformation("Sentiment of feedback is {sentiment}", detectResponse.Sentiment);

        // TODO: Store the sentiment analysis in some backend for viewing purposes.

        return MessageProcessStatus.Success();
    }
}
