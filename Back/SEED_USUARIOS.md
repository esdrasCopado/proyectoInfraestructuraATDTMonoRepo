# Usuarios Seed — SRIS

Credenciales de los usuarios creados automáticamente al arrancar la aplicación por primera vez.
Estos usuarios sirven para desarrollo y pruebas. **No usar en producción.**

---

## Usuarios por rol

| # | Rol | Email | Contraseña | N° Empleado | Teléfono |
|---|-----|-------|-----------|-------------|----------|
| 1 | Administrador General | `admingen@local` | `admin123` | 1000 | 6440000000 |
| 2 | Administrador de Centro de Datos | `admincd@local` | `AdminCD#2024` | 1001 | 6440000001 |
| 3 | Dependencia / Cliente | `dependencia@local` | `Dependencia#2024` | 1002 | 6440000002 |
| 4 | Administrador de Infraestructura | `admininf@local` | `AdminInf#2024` | 1003 | 6440000003 |
| 5 | Administrador de Vulnerabilidades | `adminvul@local` | `AdminVul#2024` | 1004 | 6440000004 |

---

## Descripción de roles

### Administrador General
Responsable de la gestión global del sistema. Tiene acceso completo a todas las secciones.

### Administrador de Centro de Datos
Responsable de validar las solicitudes entrantes, asignar recursos virtuales (RAM, almacenamiento, vCPU, dirección IP) y supervisar el ciclo completo de aprovisionamiento.

### Dependencia / Cliente
Personal de las dependencias gubernamentales que genera la solicitud de aprovisionamiento de recursos computacionales y da seguimiento al proceso hasta la puesta en marcha.

### Administrador de Infraestructura
Encargado de la configuración de accesos VPN, asignación de subdominios y validación de evidencias de funcionamiento.

### Administrador de Vulnerabilidades
Responsable del análisis de vulnerabilidades previo a la publicación del servidor.

---

## Cómo funciona el seed

El seed se ejecuta automáticamente en `Program.cs` al iniciar la aplicación (`SeedAdminUser`).
Es **idempotente**: si el email ya existe en la base de datos, no se vuelve a crear.

```
Arranque → db.Database.Migrate() → SeedAdminUser(db)
```

## Nota sobre el rol en el JWT

El campo `role` del token JWT se toma directamente de `Roles.Nombre` en la base de datos.
Los `[Authorize(Roles = "...")]` en los controladores deben coincidir exactamente con esos nombres:

| Valor en [Authorize] | Email de prueba |
|----------------------|-----------------|
| `"Administrador General"` | admingen@local |
| `"Administrador de Centro de Datos"` | admincd@local |
| `"Dependencia / Cliente"` | dependencia@local |
| `"Administrador de Infraestructura"` | admininf@local |
| `"Administrador de Vulnerabilidades"` | adminvul@local |
