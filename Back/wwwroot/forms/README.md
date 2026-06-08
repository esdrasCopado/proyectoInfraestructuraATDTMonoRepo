# Formularios de Carta Responsiva

Archivos creados:

- `CartaResponsiva.html` — Formulario principal basado en el formato "Carta Responsiva". Incluye campos para datos del solicitante, administrador del servidor, descripción del requerimiento, especificaciones técnicas, infraestructura y sección de responsiva. Permite añadir servidores adicionales (anexos) dinámicamente.

- `CartaResponsivaMultiple.html` — Formato orientado a solicitudes que agrupan varios servidores; permite añadir varios anexos y enviarlos en una sola solicitud.

Uso:

1. Abra el navegador y vaya a `http://localhost:5000/forms/CartaResponsiva.html` o `.../CartaResponsivaMultiple.html`.
2. Complete los campos y presione **Enviar solicitud**. El formulario hace un `POST` a `/api/requests` con un JSON compatible con el DTO de creación de solicitudes del backend.

Notas:

- El endpoint `/api/requests` puede requerir autenticación (JWT). Si su backend exige token, abra las herramientas de desarrollador y añada el header `Authorization: Bearer <token>` en la petición `fetch` dentro del archivo HTML (línea marcada con `fetch('/api/requests'...`).
- Estos formularios son plantillas HTML sencillas; puede convertirlos en PDFs imprimibles usando la función de impresión del navegador o adaptarlos a un generador de documentos (docx/pdf) si desea plantillas descargables.
