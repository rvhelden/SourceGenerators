using Api;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

var demo = new Demo();
var demo2 = demo.Clone();

app.Run();