using System.Reflection;
using Microsoft.Net.Http.Headers;

(string etag, string content)[] images = new[]{
    ("\"1\"", """<svg version="1.1" xmlns="http://www.w3.org/2000/svg" width="100" height="100"><text x="30" y="60" font-size="40" fill="green">1</text></svg>"""),
    ("\"2\"", """<svg version="1.1" xmlns="http://www.w3.org/2000/svg" width="100" height="100"><text x="30" y="60" font-size="40" fill="blue">2</text></svg>"""),
};
var currentImage = images[0];

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var api = app.MapGroup("/api");
api.MapGet("/", (HttpContext context) =>
{
    var ifNoneMatch = context.Request.GetTypedHeaders().IfNoneMatch;
    if (ifNoneMatch.Count == 1 && ifNoneMatch[0].Tag == currentImage.etag)
    {
        return (IResult)TypedResults.StatusCode(304);
    }
    var typedHeaders = context.Response.GetTypedHeaders();
    typedHeaders.ETag = new EntityTagHeaderValue(currentImage.etag);
    typedHeaders.CacheControl = new CacheControlHeaderValue
    {
        NoCache = true,
        //NoStore = true,
        //MustRevalidate = true,
        //Private = true,
    };
    return TypedResults.Content(currentImage.content, "image/svg+xml");
});

api.MapPut("/", () =>
{
    currentImage = currentImage == images[0] ? images[1] : images[0];
    return currentImage.etag;
});
app.Map("/", () => TypedResults.Stream(
    Assembly.GetEntryAssembly()!.GetManifestResourceStream("firefox_caching.index.html")!,
    "text/html"));
app.Run();
