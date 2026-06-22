using ChatIA.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    // Mantém os nomes das propriedades como estão (PascalCase: Pergunta, Resposta, Ativo, VideoUrl, Id),
    // igual ao contrato do backend Node antigo — o admin.html lê assim.
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

// CORS: reflete a origem (o widget roda em outro host/porta que o backend).
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod()));

// Serviços (singletons — cache interno próprio; SqlConnection é criada por chamada).
builder.Services.AddSingleton<Db>();
builder.Services.AddSingleton<FaqStore>();
builder.Services.AddSingleton<ChatLogStore>();
builder.Services.AddSingleton<AdminAuth>();
builder.Services.AddSingleton<ClaudeService>();

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles();   // chat.html via raiz, se configurado
app.UseStaticFiles();    // serve wwwroot (widget.js, admin.html, chat.html, favicon)

app.MapControllers();
app.MapGet("/health", (ClaudeService c) => Results.Json(new { ok = true, model = c.ModelId }));

app.Run();
