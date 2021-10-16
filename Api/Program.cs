using Api;

var builder = WebApplication.CreateBuilder(args);

// Registering modules endpoints using IModule
builder.Services.RegisterModules();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Api", Version = "v1" });
});

var app = builder.Build();

// Mapping endpoints using IModule
app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
}

app.Run();