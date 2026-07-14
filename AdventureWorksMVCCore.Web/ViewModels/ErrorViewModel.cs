namespace AdventureWorksMVCCore.Web.ViewModels
{
    /// <summary>
    /// View model for error page
    /// </summary>
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
