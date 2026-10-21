var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var storeName = app.Configuration["StoreSettings:StoreName"] ?? "Unknown Store";
var supportedApiVersion = app.Configuration["StoreSettings:SupportedApiVersion"] ?? "v1";

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Store-Name"] = storeName;
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {context.Request.Method} {context.Request.Path}");
    await next();
});
app.Map("/store-info", storeInfoApp =>
{
    storeInfoApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            storeName = storeName,
            supportedApiVersion = supportedApiVersion
        });
    });
});
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-Api-Version", out var version) ||
        version != supportedApiVersion)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Invalid or missing X-Api-Version header.",
            expectedVersion = supportedApiVersion,
            receivedVersion = version.ToString() ?? "(none)"
        });
        return;
    }
    await next();
});
app.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status200OK;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new
    {
        message = "Welcome to the Store API! Your request is valid."
    });
});
app.Run();
