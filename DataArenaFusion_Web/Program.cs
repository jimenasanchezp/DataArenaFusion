var builder = WebApplication.CreateBuilder(args);

// --- INICIO DE LOS LÍMITES DE SUBIDA DE ARCHIVOS ---
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});
// --- FIN DE LOS LÍMITES DE SUBIDA DE ARCHIVOS ---
// Límites para Kestrel (si corres por consola)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600;
});

// ¡NUEVO! Límites para IIS Express (si corres desde Visual Studio)
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100 MB
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });

// Inyectamos GestorDatos como Singleton para que actúe como la memoria RAM central 
// persistente (como lo haría un Form en Windows Forms).
builder.Services.AddSingleton<DataArenaFusion.Services.GestorDatos>();

// ¡Importante! El código nuevo debe ir antes de esta línea:
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Desactivamos la redirección forzada a HTTPS en desarrollo 
// para evitar el error "Failed to fetch" por certificados no confiables.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllers();

app.Run();