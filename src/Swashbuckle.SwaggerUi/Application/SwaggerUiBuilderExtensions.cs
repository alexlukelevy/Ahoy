using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.SwaggerUi.Application;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static IApplicationBuilder  UseSwaggerUi(
            this IApplicationBuilder app,
            string uiBasePath = "swagger",
            string swaggerPath = "swagger/v1/swagger.json")
        {
            // Remove slashes to simplify url construction and routing
            uiBasePath = uiBasePath.Trim('/');
            swaggerPath = swaggerPath.Trim('/');

            // Redirect uiBasePath to index.html
            var indexPath = uiBasePath + "/index.html";
            app.UseMiddleware<RedirectMiddleware>(uiBasePath, indexPath);

            // Serve index via middleware
            app.UseMiddleware<SwaggerUiMiddleware>(indexPath, swaggerPath);

            // Serve all other (embedded) swagger-ui content as static files
            var options = new FileServerOptions();
            options.RequestPath = "/" + uiBasePath;
            options.EnableDefaultFiles = false;
            options.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            options.FileProvider = new EmbeddedFileProvider(typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.SwaggerUi.bower_components.swagger_ui.dist");
            app.UseFileServer(options);

            return app;
        }
    }
}
