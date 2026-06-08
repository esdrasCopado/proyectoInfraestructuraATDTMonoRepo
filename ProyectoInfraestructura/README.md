# ProyectoInfraestructura — Guía de despliegue en producción

Sistema de administración de infraestructura para ATDT. SPA en Angular 21 + backend REST.

---

## Requisitos previos

| Herramienta | Versión mínima |
|---|---|
| Node.js | 20 LTS |
| npm | 10+ |
| Angular CLI | 21 (`npm install -g @angular/cli@21`) |

---

## Configuración

Antes de compilar, edita el archivo de entorno de producción:

```
src/environments/environment.prod.ts
```

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://TU-DOMINIO.gob.mx/api',    // ← URL real del backend
  imgUrl: 'https://TU-DOMINIO.gob.mx/uploads'  // ← URL real de imágenes
};
```

> El backend debe tener habilitado CORS para el dominio del frontend.

---

## Build

```bash
npm install
ng build --configuration production
```

La salida queda en:

```
dist/ProyectoInfraestructura/browser/
```

Sube el contenido de esa carpeta al servidor web.

---

## Configuración del servidor web

El SPA usa HTML5 routing. El servidor debe redirigir cualquier ruta que no sea un archivo estático a `index.html`.

### Nginx

```nginx
server {
    listen 80;
    server_name tu-dominio.gob.mx;
    root /var/www/infraestructura;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

### Apache (.htaccess)

```apache
Options -MultiViews
RewriteEngine On
RewriteCond %{REQUEST_FILENAME} !-f
RewriteRule ^ index.html [QL]
```

### IIS (web.config)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Angular Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

---

## Roles del sistema

| Rol | Descripción |
|---|---|
| `admin-general` | Acceso completo a todos los módulos |
| `admin-cd` | Gestión del centro de datos y expedientes |
| `admin-infraestructura` | Gestión de servidores e infraestructura |
| `admin-vulnerabilidades` | Módulo de análisis de vulnerabilidades |
| `dependencia` | Usuario de dependencia gubernamental (acceso restringido) |

Los roles se asignan al crear el usuario desde el módulo **Crear usuario**.

---

## Primer acceso (mustChangePassword)

Cuando un usuario es creado por el administrador, el sistema genera una contraseña temporal. Al iniciar sesión por primera vez:

1. El backend devuelve `mustChangePassword: true` en la respuesta del login.
2. El sistema redirige automáticamente a `/cambio-password`.
3. El usuario establece su contraseña definitiva (mínimo 8 caracteres).
4. Tras el cambio, es redirigido al dashboard.

Mientras no cambie la contraseña, el guard bloquea el acceso a cualquier otra ruta.

---

## Checklist pre-despliegue

- [ ] `apiUrl` apunta a la URL real del backend (no `localhost`)
- [ ] `imgUrl` apunta a la URL real de imágenes/uploads
- [ ] CORS habilitado en el backend para el dominio del frontend
- [ ] El servidor web está configurado con redirect a `index.html`
- [ ] HTTPS activo con certificado SSL válido
- [ ] El backend está corriendo y accesible desde el servidor del frontend
- [ ] Se probó el login con un usuario real tras el despliegue

---

## Scripts disponibles

| Comando | Descripción |
|---|---|
| `npm install` | Instala dependencias |
| `ng serve` | Servidor de desarrollo en `localhost:4200` |
| `ng build` | Build de desarrollo |
| `ng build --configuration production` | Build optimizado para producción |
