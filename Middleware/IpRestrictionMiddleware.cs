namespace VN_API.Middleware
{
    public class IpRestrictionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly List<string> _allowedIps;

        public IpRestrictionMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _allowedIps = configuration.GetSection("AllowedIPs").Get<List<string>>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            if (!_allowedIps.Contains(remoteIp.ToString()))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden.");
                return;
            }

            await _next(context);
        }
    }
}
