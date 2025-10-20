using DiscordBot.Core.Services;

namespace DiscordBot.Blazor.Middleware
{
    /// <summary>
    /// Middleware that intercepts requests and redirects to setup if not complete.
    /// </summary>
    public class FirstTimeSetupMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FirstTimeSetupMiddleware> _logger;

        // Paths that are always allowed (even during setup)
        private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/setup/welcome",
            "/setup/admin",
            "/setup/seed",
            "/setup/complete",
            "/setup/error",
            "/_framework",  // Blazor framework files
            "/_blazor",     // Blazor SignalR hub
            "/css",         // Static CSS
            "/js",          // Static JS
            "/lib",         // Library files
            "/images",      // Static images
            "/favicon.ico"
        };

        public FirstTimeSetupMiddleware(
            RequestDelegate next,
            ILogger<FirstTimeSetupMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IFirstTimeSetupService setupService)
        {
            // Check if path is allowed without setup
            var path = context.Request.Path.Value ?? string.Empty;
            if (IsPathAllowed(path))
            {
                await _next(context);
                return;
            }

            // Check if setup is complete
            var isSetupComplete = await setupService.IsSetupCompleteAsync();

            if (!isSetupComplete)
            {
                _logger.LogInformation("Setup incomplete. Redirecting to setup workflow.");
                context.Response.Redirect("/setup/welcome");
                return;
            }

            // Setup is complete, continue to next middleware
            await _next(context);
        }

        private static bool IsPathAllowed(string path)
        {
            return AllowedPaths.Any(allowed =>
                path.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Extension method for registering the middleware.
    /// </summary>
    public static class FirstTimeSetupMiddlewareExtensions
    {
        public static IApplicationBuilder UseFirstTimeSetup(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirstTimeSetupMiddleware>();
        }
    }
}
