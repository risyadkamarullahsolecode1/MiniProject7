using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.FileProviders;
using MiniProject7.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serve the "Uploads" folder as static files
var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:5173") // Replace with your React app's URL
            .AllowCredentials() // Allow cookies
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddCookiePolicy(options =>
{
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureInfrastructure(builder.Configuration); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsFolder),
    RequestPath = "/Uploads"
});
app.MapControllers();

app.Run();
