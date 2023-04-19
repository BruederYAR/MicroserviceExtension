using MicroserviceTemplate.Base.Definition;
using Microsoft.IdentityModel.Logging;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddDefinitions(builder, typeof(Program));

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        IdentityModelEventSource.ShowPII = true;
    }
    
    app.UseDefinitions();

    app.Run();
}
catch (Exception ex)
{
    
}