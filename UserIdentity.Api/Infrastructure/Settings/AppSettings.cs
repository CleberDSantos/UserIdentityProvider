using UserIdentity.Services.Models;

namespace UserIdentity.WebUI.Infrastructure.Settings
{
    public class AppSettings
    {
        public SmtpSettings SmtpSettings { get; set; }
        public string UploadsFolder { get; set; }
        public string Domain { get; set; }
    }
}