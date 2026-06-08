using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Implementaciones;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Services.Implementaciones;
using SolicitudServidores.Services.Interfaces;
using SolicitudServidores.Utilities;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ── Variables de entorno ──────────────────────────────────────────────────────
var envPath = new[]
{
    Path.Combine(builder.Environment.ContentRootPath, "variables.env"),
    Path.Combine(Directory.GetCurrentDirectory(), "variables.env"),
    Path.Combine(AppContext.BaseDirectory, "variables.env")
}.FirstOrDefault(File.Exists);

if (!string.IsNullOrWhiteSpace(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($"Variables de entorno cargadas desde: {envPath}");
}

// ── Conexión PostgreSQL ───────────────────────────────────────────────────────
var postgresConnection = (Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION")
    ?? builder.Configuration.GetConnectionString("PostgreSQLConnection"))?.Trim();

if (string.IsNullOrWhiteSpace(postgresConnection))
    throw new Exception("No se encontró la cadena de conexión PostgreSQL (POSTGRESQL_CONNECTION).");

// ── SMTP ──────────────────────────────────────────────────────────────────────
var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
if (!string.IsNullOrWhiteSpace(smtpHost))
{
    builder.Configuration["SMTP:Host"]     = smtpHost;
    builder.Configuration["SMTP:Port"]     = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587";
    builder.Configuration["SMTP:User"]     = Environment.GetEnvironmentVariable("SMTP_USER") ?? string.Empty;
    builder.Configuration["SMTP:Password"] = (Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? string.Empty).Replace(" ", "");
    builder.Configuration["SMTP:From"]     = Environment.GetEnvironmentVariable("SMTP_FROM") ?? string.Empty;
    builder.Configuration["SMTP:FromName"] = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "Sistema ATDT";
}

// ── JWT ───────────────────────────────────────────────────────────────────────
var jwtKey = (Environment.GetEnvironmentVariable("JWT__key")
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["JWT:key"]
    ?? builder.Configuration["JWT_SECRET"])?.Trim();

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("No se encontró la llave JWT en variables de entorno o appsettings.");

builder.Configuration["JWT:key"] = jwtKey;

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ── Controladores ─────────────────────────────────────────────────────────────
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "SRIS API",
        Version     = "v1",
        Description = "Sistema de Resguardo de Infraestructura de Servidores.",
        Contact     = new OpenApiContact { Name = "Centro de Datos", Email = "soporte@centrodatos.gob.mx" }
    });

    c.TagActionsBy(api =>
    {
        var controller = api.ActionDescriptor.RouteValues["controller"];
        return controller switch
        {
            "Reporte"    => ["Reportes"],
            "Dependency" => ["Dependencia"],
            _            => [controller]
        };
    });

    c.DocInclusionPredicate((_, _) => true);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Ingresa el token JWT. Ejemplo: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// ── Base de datos (PostgreSQL) ────────────────────────────────────────────────
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(postgresConnection));

// ── Repositorios y servicios ──────────────────────────────────────────────────
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ISolicitudRepository, SolicitudRepository>();
builder.Services.AddScoped<IServidorRepository, ServidorRepository>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<ISeguimientoRepository, SeguimientoRepository>();
builder.Services.AddScoped<ISeguimientoService, SeguimientoService>();
builder.Services.AddScoped<IVpnRepository, VpnRepository>();
builder.Services.AddScoped<IVpnService, VpnService>();
builder.Services.AddScoped<ICartaRepository, CartaRepository>();
builder.Services.AddScoped<ICartaService, CartaService>();
builder.Services.AddScoped<IEvidenciaRepository, EvidenciaRepository>();
builder.Services.AddScoped<IEvidenciaService, EvidenciaService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISubdominioRepository, SubdominioRepository>();
builder.Services.AddScoped<IDependencyRepository, DependencyRepository>();
builder.Services.AddScoped<IAnalisisVulnerabilidadRepository, AnalisisVulnerabilidadRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

// ── Autenticación JWT ─────────────────────────────────────────────────────────
builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer           = false,
        ValidateAudience         = false,
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ── Utilidades ────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<CrearJWT>();
builder.Services.AddSingleton<Encriptar>();
builder.Services.AddScoped<GlobalExceptionFilter>();

var app = builder.Build();

// ── Migraciones y seed ────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    try
    {
        Console.WriteLine("Aplicando migraciones PostgreSQL...");
        db.Database.Migrate();
        SeedRoles(db);
        SeedAdminFromEnv(db);
        if (Environment.GetEnvironmentVariable("SEED_DEMO_USERS") == "true")
            SeedDemoUsers(db); // TODO: eliminar antes de producción — SEED_DEMO_USERS debe ser false o no estar definido
        Console.WriteLine("Base de datos lista.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al inicializar la base de datos: {ex.Message}");
        throw;
    }
}

// ── Pipeline HTTP ─────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "SRIS API v1");
        ui.DocumentTitle           = "SRIS — API Docs";
        ui.DefaultModelsExpandDepth(-1);
        ui.EnableFilter();
    });
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok", utc = DateTime.UtcNow }));
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

// ── Seed de roles (siempre corre) ────────────────────────────────────────────
static void SeedRoles(DataContext db)
{
    var roles = new[]
    {
        new { Nombre = "Administrador General",              Descripcion = "Responsable de la gestión global del sistema." },
        new { Nombre = "Administrador de Centro de Datos",   Descripcion = "Responsable de validar las solicitudes entrantes, asignar recursos virtuales y supervisar el ciclo completo de aprovisionamiento." },
        new { Nombre = "Dependencia / Cliente",              Descripcion = "Personal de las dependencias gubernamentales que genera la solicitud de aprovisionamiento de recursos computacionales y da seguimiento al proceso hasta la puesta en marcha." },
        new { Nombre = "Administrador de Infraestructura",   Descripcion = "Encargado de la configuración de accesos VPN, asignación de subdominios y validación de evidencias de funcionamiento." },
        new { Nombre = "Administrador de Vulnerabilidades",  Descripcion = "Responsable del análisis de vulnerabilidades previo a la publicación del servidor." },
    };

    foreach (var r in roles)
    {
        if (!db.Roles.Any(x => x.Nombre == r.Nombre))
        {
            db.Roles.Add(new Roles { Nombre = r.Nombre, Descripcion = r.Descripcion });
        }
    }
    db.SaveChanges();
}

// ── Seed del admin inicial desde variables de entorno ────────────────────────
static void SeedAdminFromEnv(DataContext db)
{
    var email    = Environment.GetEnvironmentVariable("ADMIN_EMAIL")?.Trim().ToLower();
    var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
    var nombre   = Environment.GetEnvironmentVariable("ADMIN_NOMBRE")   ?? "Admin";
    var apellidos = Environment.GetEnvironmentVariable("ADMIN_APELLIDOS") ?? "General";

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine("ADMIN_EMAIL / ADMIN_PASSWORD no definidos — se omite seed del admin inicial.");
        return;
    }

    if (db.Usuarios.Any(u => u.Email == email))
        return;

    var rol = db.Roles.FirstOrDefault(r => r.Nombre == "Administrador General");
    if (rol == null) return;

    db.Usuarios.Add(new Usuario
    {
        Nombre         = nombre,
        Apellidos      = apellidos,
        Email          = email,
        PasswordHash   = Encriptar.EncriptarSHA256(password),
        RoleId         = rol.RoleId,
        Activo         = true,
        MustChangePassword = false
    });
    db.SaveChanges();
    Console.WriteLine($"Admin inicial creado: {email}");
}

// ── Seed de usuarios demo (solo desarrollo) ──────────────────────────────────
// TODO: ELIMINAR ANTES DE PRODUCCIÓN
// Para desactivar: SEED_DEMO_USERS=false (o no definir la variable)
static void SeedDemoUsers(DataContext db)
{
    var demos = new[]
    {
        new { Rol = "Administrador de Centro de Datos",  Nombre = "Admin", Apellidos = "Centro Datos",   Email = "admincd@local",       Password = "AdminCD#2024",       Cargo = "Administrador de Centro de Datos",  Phone = "6440000001", NumeroEmpleado = "1001" },
        new { Rol = "Dependencia / Cliente",             Nombre = "Usuario", Apellidos = "Dependencia",  Email = "dependencia@local",   Password = "Dependencia#2024",   Cargo = "Responsable de Dependencia",        Phone = "6440000002", NumeroEmpleado = "1002" },
        new { Rol = "Administrador de Infraestructura",  Nombre = "Admin", Apellidos = "Infraestructura", Email = "admininf@local",      Password = "AdminInf#2024",      Cargo = "Administrador de Infraestructura",  Phone = "6440000003", NumeroEmpleado = "1003" },
        new { Rol = "Administrador de Vulnerabilidades", Nombre = "Admin", Apellidos = "Vulnerabilidades", Email = "adminvul@local",    Password = "AdminVul#2024",      Cargo = "Administrador de Vulnerabilidades", Phone = "6440000004", NumeroEmpleado = "1004" },
    };

    foreach (var d in demos)
    {
        if (db.Usuarios.Any(u => u.Email == d.Email)) continue;
        var rol = db.Roles.FirstOrDefault(r => r.Nombre == d.Rol);
        if (rol == null) continue;
        db.Usuarios.Add(new Usuario
        {
            Nombre         = d.Nombre,
            Apellidos      = d.Apellidos,
            Email          = d.Email,
            PasswordHash   = Encriptar.EncriptarSHA256(d.Password),
            RoleId         = rol.RoleId,
            Cargo          = d.Cargo,
            Phone          = d.Phone,
            NumeroEmpleado = d.NumeroEmpleado,
            Activo         = true
        });
    }
    db.SaveChanges();
}
