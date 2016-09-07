using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.SwaggerUi.Application;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class SwaggerUiBuilderExtensions
    {
        public static IApplicationBuilder  UseSwaggerUi(
            this IApplicationBuilder app,
            string uiRoutePrefix = "swagger",
            Action<SwaggerUiOptions> setupAction = null)
        {
            // Ensure routePrefix is valid
            uiRoutePrefix = uiRoutePrefix.Trim('/');

            // Redirect routePrefix to index
            var indexPath = uiRoutePrefix + "/index.html";
            app.UseMiddleware<RedirectMiddleware>(uiRoutePrefix, indexPath);

            // Serve index via middleware
            var swaggerUiOptions = new SwaggerUiOptions();
            setupAction?.Invoke(swaggerUiOptions);
            app.UseMiddleware<SwaggerUiMiddleware>(indexPath, swaggerUiOptions);

            // Serve all other (embedded) swagger-ui content as static files
            var fileServerOptions = new FileServerOptions();
            fileServerOptions.RequestPath = "/" + uiRoutePrefix;
            fileServerOptions.EnableDefaultFiles = false;
            fileServerOptions.StaticFileOptions.ContentTypeProvider = new FileExtensionContentTypeProvider();
            fileServerOptions.FileProvider = new EmbeddedFileProvider(typeof(SwaggerUiBuilderExtensions).GetTypeInfo().Assembly,
                "Swashbuckle.SwaggerUi.bower_components.swagger_ui.dist");
            app.UseFileServer(fileServerOptions);

            return app;
        }
    }
}
