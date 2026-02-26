namespace ExpenseTracker.Models;


/// View model used by the Error view to display the request id and determine visibility.
public class ErrorViewModel
{

    /// The request identifier for the current HTTP request, if available.

    public string? RequestId { get; set; }

    /// Indicates whether the RequestId should be shown on the Error view.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
