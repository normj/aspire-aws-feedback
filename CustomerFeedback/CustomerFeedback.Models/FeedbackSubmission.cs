namespace CustomerFeedback.Models;

public class FeedbackSubmission
{
    /// <summary>
    /// The product name
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Feedback for the product
    /// </summary>
    public string? Feedback { get; set; }
}
