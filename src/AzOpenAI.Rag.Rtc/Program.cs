using AzOpenAI.Rag.Rtc.Endpoints;
using AzOpenAI.Rag.Rtc.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMandetoryServices(builder.Configuration);
builder.Services.AddMandetoryServicesForWebAPI();
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

var apiGroup = app.MapGroup("/api");
var sessionApiGroup = apiGroup.MapGroup("/session");

sessionApiGroup.MapPost("/begin", new SessionEndpoint().HandleAsync).WithOpenApi();
app.Run();
