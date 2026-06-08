# SRIS — Sistema de Resguardo de Infraestructura de Servidores

API REST desarrollada en **.NET 8** con PostgreSQL. Gestiona el ciclo completo de aprovisionamiento de servidores virtuales: solicitudes, asignación de recursos, VPNs, subdominios, evidencias y vulnerabilidades.

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- PostgreSQL 14+
- Cuenta SMTP (Gmail, Outlook o servidor propio) para envío de credenciales

---

## Configuración

### 1. Copiar el archivo de variables de entorno

```bash
cp variables.env.example variables.env
```

Editar `variables.env` con los valores reales. Ver [variables.env.example](variables.env.example) para descripción de cada variable.

Variables mínimas para correr:

| Variable | Descripción |
|---|---|
| `POSTGRESQL_CONNECTION` | Cadena de conexión a PostgreSQL |
| `JWT__key` | Clave secreta JWT (mínimo 32 caracteres) |
| `ADMIN_EMAIL` | Correo del primer Administrador General |
| `ADMIN_PASSWORD` | Contraseña del primer Administrador General |
| `SMTP_HOST` / `SMTP_USER` / `SMTP_PASSWORD` | Configuración de correo saliente |

### 2. Aplicar migraciones y primer arranque

```bash
dotnet run
```

Al arrancar por primera vez el sistema:
1. Aplica las migraciones a la base de datos automáticamente
2. Crea los 5 roles del sistema
3. Crea el Administrador General con las credenciales de `ADMIN_EMAIL` / `ADMIN_PASSWORD`

El admin ya puede iniciar sesión y crear al resto de usuarios desde el panel.

---

## Desarrollo local

Con `SEED_DEMO_USERS=true` en `variables.env`, al arrancar se crean automáticamente usuarios de prueba para cada rol. Ver credenciales en [SEED_USUARIOS.md](SEED_USUARIOS.md).

```bash
dotnet run
# Swagger disponible en: http://localhost:<puerto>/swagger
```

---

## Producción

Antes de desplegar, verificar en `variables.env`:

```env
SEED_DEMO_USERS=false   # desactiva usuarios de prueba
JWT__key=<clave segura aleatoria>
ADMIN_EMAIL=<correo real>
ADMIN_PASSWORD=<contraseña segura>
```

> El bloque `SeedDemoUsers` en `Program.cs` está marcado con `TODO: ELIMINAR ANTES DE PRODUCCIÓN` y puede borrarse una vez que el sistema esté estable.

---

## Roles del sistema

| Rol | Descripción |
|---|---|
| Administrador General | Acceso completo. Gestiona usuarios y configuración. |
| Administrador de Centro de Datos | Valida solicitudes, asigna recursos virtuales y supervisa el aprovisionamiento. |
| Administrador de Infraestructura | Configura VPNs, subdominios y valida evidencias. |
| Administrador de Vulnerabilidades | Analiza vulnerabilidades antes de la publicación. |
| Dependencia / Cliente | Genera solicitudes y da seguimiento hasta la puesta en marcha. |

---

## Gestión de usuarios

No hay autoregistro. El flujo es:

1. El **Administrador General** crea el usuario desde el panel (`POST /api/usuario`)
2. El sistema genera una contraseña temporal y la envía por correo al usuario
3. La respuesta del endpoint también devuelve la contraseña para que el admin pueda compartirla manualmente si el correo no llega
4. Al primer ingreso, el sistema obliga al usuario a cambiar su contraseña (`mustChangePassword = true`)

---

## Documentación de la API

Disponible en Swagger al correr en modo desarrollo:

```
http://localhost:<puerto>/swagger
```

Endpoints principales:

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/api/auth/login` | Autenticación, devuelve JWT |
| GET | `/api/usuario` | Lista usuarios (paginado) |
| POST | `/api/usuario` | Crear usuario + enviar credenciales por correo |
| PATCH | `/api/usuario/{id}/password` | Cambiar contraseña (resetea mustChangePassword) |
| GET | `/api/usuario/roles` | Lista de roles disponibles |
| GET | `/health` | Health check |

---

## Estructura del proyecto

```
Back/
├── Controller/          # Controladores de la API
├── DBContext/           # Contexto de Entity Framework
├── DTOs/                # Objetos de transferencia de datos
├── Migrations/          # Migraciones de base de datos
├── Models/              # Entidades de la base de datos
├── Repositories/        # Acceso a datos (interfaces e implementaciones)
├── Services/            # Lógica de negocio y servicios externos (email, PDF)
├── Utilities/           # JWT, encriptación
├── wwwroot/             # Frontend estático y archivos subidos
├── variables.env        # Variables de entorno locales (NO subir al repo)
└── variables.env.example # Plantilla de variables documentada
```

---

## Archivos relacionados

- [variables.env.example](variables.env.example) — todas las variables con descripción
- [SEED_USUARIOS.md](SEED_USUARIOS.md) — credenciales de usuarios demo
- [requisitos.md](requisitos.md) — requerimientos funcionales del sistema
