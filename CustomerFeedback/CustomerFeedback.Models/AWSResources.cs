using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerFeedback.Models;

public class AWSResources
{
    public string? FeedbackQueueUrl { get; set; }

    public string? FeedbackTopicArn { get; set; }
}
