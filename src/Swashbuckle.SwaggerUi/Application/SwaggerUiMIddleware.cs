using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
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
        private readonly string _swaggerPath;
        private readonly Assembly _resourceAssembly;

        public SwaggerUiMiddleware(
            RequestDelegate next,
            string uiBasePath,
            string swaggerPath
        )
        {
            _next = next;
            _requestMatcher = new TemplateMatcher(TemplateParser.Parse(uiBasePath), new RouteValueDictionary());
            _swaggerPath = swaggerPath;
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
            var content = GenerateContent(template, httpContext.Request.PathBase);
            RespondWithContentHtml(httpContext.Response, content);
        }

        private bool RequestingSwaggerUi(HttpRequest request)
        {
            if (request.Method != "GET") return false;

			return _requestMatcher.TryMatch(request.Path, new RouteValueDictionary());
        }

        private Stream GenerateContent(Stream template, string requestPathBase)
        {
            var configData = new
            {
                swaggerUrl = requestPathBase + "/" + _swaggerPath
            };

            var placeholderValues = new Dictionary<string, string>
            {
                { "%(ConfigData)", JsonConvert.SerializeObject(configData) }
            };

            var templateText = new StreamReader(template).ReadToEnd();
            var contentBuilder = new StringBuilder(templateText);
            foreach (var entry in placeholderValues)
            {
                contentBuilder.Replace(entry.Key, entry.Value);
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(contentBuilder.ToString()));
        }

        private void RespondWithContentHtml(HttpResponse response, Stream content)
        {
            response.StatusCode = 200;
            response.ContentType = "text/html";
            content.CopyTo(response.Body);
        }
    }
}
