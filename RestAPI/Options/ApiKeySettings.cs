namespace RestAPI.Options
{
    public class ApiKeySettings
    {
        public int ExpirationTimeInMinutes { get; set; }

        public int ApiKeyLimit { get; set; }
    }
}