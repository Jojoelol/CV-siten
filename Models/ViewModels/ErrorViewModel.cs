namespace CV_siten.Models.ViewModels
{
    public class ErrorViewModel
    {
        // Denna behövs för HomeController.cs rad 44
        public string? RequestId { get; set; }

        // Denna behövs för Error.cshtml rad 11
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}