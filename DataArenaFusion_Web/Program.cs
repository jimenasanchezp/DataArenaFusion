using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Capturar excepciones que matan el proceso (Exit Code -1)
AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
    Console.WriteLine($"[FATAL CRASH] {(e.ExceptionObject as Exception)?.Message}");
    Console.WriteLine((e.ExceptionObject as Exception)?.StackTrace);
};

// --- CONFIGURACIÓN GLOBAL DE LÍMITES DE SUBIDA (Streaming Safe) ---
builder.WebHost.ConfigureKestrel(options =>
{
    // Límite físico de la petición (100 MB)
    options.Limits.MaxRequestBodySize = 104857600; 
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; 
});

// ---------------------------------------------------------


builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });

// Inyectamos GestorDatos como Singleton
builder.Services.AddSingleton<DataArenaFusion.Core.Services.GestorDatos>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();