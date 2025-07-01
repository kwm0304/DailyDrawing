using Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddRateLimiter(builder.Configuration);
var app = builder.Build();
app.UseRateLimiter();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.MapControllers();
app.Run();