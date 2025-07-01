namespace BandCommunity.WebApi.Middleware;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;
    
    public JwtCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Cookies["ACCESS_TOKEN"];
        if (!string.IsNullOrEmpty(token))
        {
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }
        
        //* Call the next middleware in the pipeline
        await _next(context);
    }
}