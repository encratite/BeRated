namespace BeRated
{
    public class Configuration
    {
        public string ServerUrl { get; set; }

        public string ConnectionString { get; set; }

        public string TemplatePath { get; set; }

        public Configuration()
        {
            TemplatePath = "Templates";
        }
    }
}
