using Api;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

DemoRouteBuilder.Configure(app);

app.Run();