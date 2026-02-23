namespace ExpenseTracker.Models;

/// <summary>
/// View model used by the Error view to display the request id and determine visibility.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// The request identifier for the current HTTP request, if available.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Indicates whether the RequestId should be shown on the Error view.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
