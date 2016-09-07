using System.Collections.Generic;

namespace Swashbuckle.SwaggerUi.Application
{
    public class SwaggerUiOptions
    {
        public SwaggerUiOptions()
        {
            SwaggerEndpoints = new List<SwaggerEndpoint>();
        }

        public void SwaggerEndpoint(string url, string description)
        {
            SwaggerEndpoints.Add(new SwaggerEndpoint { url = url, description = description });
        }

        internal IList<SwaggerEndpoint> SwaggerEndpoints { get; private set; }
    }

    public class SwaggerEndpoint
    {
        public string url;
        public string description;
    }
}
