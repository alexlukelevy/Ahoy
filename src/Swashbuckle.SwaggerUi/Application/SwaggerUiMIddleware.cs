using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;

namespace Swashbuckle.SwaggerUi.Application
{
    public class SwaggerUiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TemplateMatcher _requestMatcher;
        private readonly SwaggerUiOptions _options;
        private readonly Assembly _resourceAssembly;

        public SwaggerUiMiddleware(
            RequestDelegate next,
            string uiIndexRoute,
            SwaggerUiOptions options)
        {
            _next = next;
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(uiIndexRoute), new RouteValueDictionary());
            _options = options;
            _resourceAssembly = GetType().GetTypeInfo().Assembly;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!RequestingSwaggerUi(httpContext.Request))
            {
                await _next(httpContext);
                return;
            }

            var template = _resourceAssembly.GetManifestResourceStream("Swashbuckle.SwaggerUi.CustomAssets.index.html");
            var templateData = GetTemplateData(httpContext.Request.PathBase);
            var content = GenerateContent(template, templateData);

            RespondWithHtmlContent(httpContext.Response, content);
        }

        private bool RequestingSwaggerUi(HttpRequest request)
        {
            return (request.Method == "GET") && _requestMatcher.TryMatch(request.Path, new RouteValueDictionary());
        }

        private IDictionary<string, string> GetTemplateData(string requestPathBase)
        {
            var jsConfig = new
            {
                swaggerEndpoints = (_options.SwaggerEndpoints.Any())
                    ? _options.SwaggerEndpoints
                    : DefaultSwaggerEndpoints(requestPathBase)
            };

            return new Dictionary<string, string>
            {
                {  "%(JSConfig)", JsonConvert.SerializeObject(jsConfig) }
            };
        }
        
        private IEnumerable<SwaggerEndpoint> DefaultSwaggerEndpoints(string requestBasePath)
        {
            return new[]
            {
                new SwaggerEndpoint { url = requestBasePath + "/swagger/v1/swagger.json", description = "V1 Docs" }
            };
        }

        private Stream GenerateContent(Stream template, IDictionary<string, string> templateData)
        {
            var templateText = new StreamReader(template).ReadToEnd();
            var contentBuilder = new StringBuilder(templateText);
            foreach (var entry in templateData)
            {
                contentBuilder.Replace(entry.Key, entry.Value);
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(contentBuilder.ToString()));
        }

        private void RespondWithHtmlContent(HttpResponse response, Stream content)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";
            content.CopyTo(response.Body);
        }
    }
}
