# Requerimientos Funcionales

## RF01 — Registro, lectura, actualización y borrado de usuarios en sistema

| Campo | Detalle |
|---|---|
| **Identificación** | RF01 |
| **Nombre** | Registro, lectura, actualización y borrado de usuarios en sistema. |
| **Características** | Registro, lectura, actualización y borrado de usuarios en sistema con roles de dependencia/cliente, administrador de centro de datos, administrador de infraestructura y administrador de vulnerabilidades. |
| **Descripción** | Permitir opciones CRUD para usuarios del sistema. |
| **Rol que ejecuta** | Administrador |
| **Prioridad** | Alta |

---

## RF02 — Registro de carta de aprovisionamiento

| Campo | Detalle |
|---|---|
| **Identificación** | RF02 |
| **Nombre** | Registro de carta de aprovisionamiento y envío por dependencia a validación por parte del administrador de centro de datos. |
| **Características** | Registro de documento de solicitud con base a documento "Carta Responsiva de Aprovisionamiento"; Validación de información mínima; Creación de expediente del servidor y asignación de folio. |
| **Descripción** | Permitir registrar la recepción de la carta responsiva o carta de aprovisionamiento enviada por una dependencia o cliente para solicitar la creación de un servidor virtual. |
| **Rol que ejecuta** | Dependencia / Administrador de Centro de Datos |
| **Prioridad** | Alta |

---

## RF03 — Validación, edición y asignación de recursos virtuales del servidor

| Campo | Detalle |
|---|---|
| **Identificación** | RF03 |
| **Nombre** | Validación, edición y asignación de recursos virtuales del servidor. |
| **Características** | Asignación de RAM; Asignación de almacenamiento; Asignación de vCPU; Asignación de dirección IP; Asociación con dependencia. |
| **Descripción** | Desplegar en dashboard de usuario administrador del centro de datos la nueva solicitud, permitir su selección y despliegue de la carta de aprovisionamiento, donde las características del servidor virtual solicitado incluyendo memoria RAM, almacenamiento y procesadores virtuales (vCore) así como su asignación de dirección IP serán o no editadas. |
| **Rol que ejecuta** | Administrador de Centro de Datos |
| **Prioridad** | Alta |

---

## RF04 — Asignación de estado de creación y aprovisionamiento del servidor virtual

| Campo | Detalle |
|---|---|
| **Identificación** | RF04 |
| **Nombre** | Asignación de estado de creación y aprovisionamiento del servidor virtual. |
| **Características** | Asignación de estado de creación y aprovisionamiento del servidor virtual en sistema. |
| **Descripción** | Permitir asignar el estado de creación a una solicitud en el dashboard al seleccionar del dashboard la solicitud correspondiente, guardando la fecha de actualización y el usuario. |
| **Rol que ejecuta** | Administrador de Centro de Datos |
| **Prioridad** | Alta |

---

## RF05 — Registro de atributo de verificación de comunicaciones

| Campo | Detalle |
|---|---|
| **Identificación** | RF05 |
| **Nombre** | Registro de atributo de verificación de comunicaciones. |
| **Características** | Registro de atributo de verificación de comunicaciones de un registro que ya tiene el estado de creación. |
| **Descripción** | Permitir asignar el atributo de revisión de comunicaciones, registrando la fecha y el usuario. |
| **Rol que ejecuta** | Administrador de Centro de Datos |
| **Prioridad** | Alta |

---

## RF06 — Registro de atributo de verificación de actualizaciones y parches

| Campo | Detalle |
|---|---|
| **Identificación** | RF06 |
| **Nombre** | Registro de atributo de verificación de actualizaciones y parches. |
| **Características** | Registro de atributo de verificación de actualizaciones y parches. |
| **Descripción** | Permitir asignar el atributo de actualizaciones y parches, registrando la fecha y el usuario. |
| **Rol que ejecuta** | Administrador de Centro de Datos |
| **Prioridad** | Alta |

---

## RF07 — Registro de atributo de verificación de XDR y agente

| Campo | Detalle |
|---|---|
| **Identificación** | RF07 |
| **Nombre** | Registro de atributo de verificación de XDR y agente. |
| **Características** | Registro de atributo de verificación de XDR y agente. |
| **Descripción** | Permitir asignar el atributo de XDR y agente, registrando la fecha y el usuario. |
| **Rol que ejecuta** | Administrador de Centro de Datos |
| **Prioridad** | Alta |

---

## RF08 — Registro y edición de atributo de VPN para acceso a servidor

| Campo | Detalle |
|---|---|
| **Identificación** | RF08 |
| **Nombre** | Registro y edición de atributo de VPN para acceso a servidor. |
| **Características** | Para solicitudes ya creadas y aprovisionadas con atributos de comunicaciones, actualizaciones y parches, y su XDR: el sistema deberá permitir añadir y editar uno o más registros para este atributo, es decir, cada servidor puede tener *n* número de VPN asignadas precargando la información de la carta responsiva correspondiente y añadiendo un check para validar su creación. |
| **Descripción** | Los registros para este atributo serán de tipo VPN para proveedor o servidor público; Vigencia de 30, 60 o 90 días para VPN de proveedor; Guardar el usuario y la fecha en la que se asignó la VPN. |
| **Rol que ejecuta** | Administrador de Infraestructura |
| **Prioridad** | Alta |

---

## RF09 — Registro y edición de atributo de subdominio para acceso a servidor

| Campo | Detalle |
|---|---|
| **Identificación** | RF09 |
| **Nombre** | Registro y edición de atributo de subdominio para acceso a servidor y envío de notificación al administrador de centro de datos. |
| **Características** | Para solicitudes ya creadas y aprovisionadas con atributos de comunicaciones, actualizaciones y parches, XDR y VPN: el sistema deberá permitir añadir y editar uno o más registros de subdominio para este atributo, es decir, cada servidor puede tener *n* número de subdominios asignados precargando la información de la carta responsiva correspondiente y añadiendo un check para validar su creación. |
| **Descripción** | Los registros para este atributo serán de tipo VPN para proveedor o servidor público; Vigencia de 30, 60 o 90 días para VPN de proveedor; Guardar el usuario y la fecha en la que se asignaron los subdominios, y envío de notificación al administrador de centro de datos. |
| **Rol que ejecuta** | Administrador de Infraestructura |
| **Prioridad** | Alta |

---

## RF10 — Asignación de atributo de entrega de credenciales y descarga de Carta Responsiva

| Campo | Detalle |
|---|---|
| **Identificación** | RF10 |
| **Nombre** | Asignación de atributo de entrega de credenciales y descarga de Carta Responsiva de Aprovisionamiento en formato PDF. |
| **Características** | Permitir seleccionar desde la notificación o el dashboard del administrador de centro de datos el registro con los atributos del proceso, asignando el atributo de entrega de credenciales y permitiendo la descarga en formato PDF de la Carta Responsiva de Aprovisionamiento. |
| **Descripción** | Permitir asignar el atributo y la descarga de la Carta con las características completas en formato PDF. |
| **Rol que ejecuta** | Administrador de Centro de Datos |
| **Prioridad** | Alta |

---

## RF11 — Establecimiento de atributo de Configuración del WAF

| Campo | Detalle |
|---|---|
| **Identificación** | RF11 |
| **Nombre** | Establecimiento de atributo de Configuración del WAF. |
| **Características** | Servidor que cuenta con el atributo de entrega de credenciales: la dependencia establecerá el atributo de configuración de WAF registrando la fecha y el usuario que realizó la configuración. |
| **Descripción** | Permitir el establecimiento de atributo de Configuración del WAF registrando el usuario y la fecha en que fue actualizado. |
| **Rol que ejecuta** | Dependencia / Cliente |
| **Prioridad** | Alta |

---

## RF12 — Asignación de atributo de Pruebas de funcionamiento y carga de evidencias

| Campo | Detalle |
|---|---|
| **Identificación** | RF12 |
| **Nombre** | Asignación de atributo de Pruebas de funcionamiento y carga de *n* número de evidencias en formato PDF. |
| **Características** | Debe permitir la asignación de atributo de Pruebas de funcionamiento y carga de evidencias en formato PDF, habilitando un botón para agregar nueva evidencia. |
| **Descripción** | Debe permitir la asignación de atributo de Pruebas de funcionamiento y carga de evidencias en formato PDF; registrando el usuario y la fecha, y enviando una notificación al administrador de centro de datos y al administrador de infraestructura. |
| **Rol que ejecuta** | Dependencia / Cliente |
| **Prioridad** | Alta |

---

## RF13 — Asignación de atributo de validación de evidencias

| Campo | Detalle |
|---|---|
| **Identificación** | RF13 |
| **Nombre** | Asignación de atributo de validación de evidencias. |
| **Características** | Debe permitir la asignación de atributo de validación de evidencias y enviar notificación a la dependencia de su aprobación o requerimiento de solventación. |
| **Descripción** | Debe permitir la asignación de atributo de validación de evidencias y enviar notificación a la dependencia de su aprobación o requerimiento de solventación; registrando el usuario y la fecha, y enviando una notificación si fue aprobado al administrador de vulnerabilidades y a la dependencia; si no fue aprobado, habilita el registro para una carga secundaria de evidencias por la dependencia. |
| **Rol que ejecuta** | Administrador de Centro de Datos / Administrador de Infraestructura |
| **Prioridad** | Alta |

---

## RF14 — Asignación de atributo de solicitud de publicación

| Campo | Detalle |
|---|---|
| **Identificación** | RF14 |
| **Nombre** | Asignación de atributo de solicitud de publicación. |
| **Características** | Debe permitir la asignación de atributo de solicitud de publicación y enviar notificación al administrador de vulnerabilidades. |
| **Descripción** | Debe permitir la asignación de atributo de solicitud de publicación y enviar notificación al administrador de vulnerabilidades; registrando el usuario y la fecha. |
| **Rol que ejecuta** | Dependencia |
| **Prioridad** | Alta |

---

## RF15 — Asignación de atributo de vulnerabilidades

| Campo | Detalle |
|---|---|
| **Identificación** | RF15 |
| **Nombre** | Asignación de atributo de vulnerabilidades. |
| **Características** | Debe permitir la asignación de atributo de vulnerabilidades y enviar notificación a la dependencia de su aprobación o requerimiento de solventación. |
| **Descripción** | Debe permitir la asignación de atributo de vulnerabilidades y enviar notificación a la dependencia de su aprobación o requerimiento de solventación; registrando el usuario y la fecha, y enviando una notificación del resultado del análisis de vulnerabilidades; si fue aprobado, se notifica a la dependencia y se ejecuta la publicación por defecto enviando una notificación al administrador de infraestructura para su publicación; si no fue aprobado, habilita el registro para una carga secundaria de evidencias de solventación por la dependencia enviando una notificación al administrador de vulnerabilidades para analizar nuevamente y confirmar si los hallazgos fueron resueltos. |
| **Rol que ejecuta** | Administrador de Vulnerabilidades |
| **Prioridad** | Alta |

---

## RF16 — Visualización de dashboard general

| Campo | Detalle |
|---|---|
| **Identificación** | RF16 |
| **Nombre** | Visualización de dashboard general. |
| **Características** | Visualización de un dashboard para representación de estado de las solicitudes en curso en cada etapa. |
| **Descripción** | El sistema debe permitir visualizar en un dashboard el seguimiento de cada una de las etapas diferenciando si la tarea o etapa ya se inició, está lista para el siguiente paso, o pendiente de alguna respuesta por alguna de las partes según aplique. |
| **Rol que ejecuta** | Administradores |
| **Prioridad** | Alta |

---

## RF17 — Generación de reportes

| Campo | Detalle |
|---|---|
| **Identificación** | RF17 |
| **Nombre** | Generación de reportes. |
| **Características** | Sección u opción para generación de reportes. |
| **Descripción** | El sistema debe permitir la generación de reportes de las solicitudes para visualización dentro del mismo sistema con opciones de filtro y búsqueda, y exportación de registros a hoja de cálculo. |
| **Rol que ejecuta** | Administradores |
| **Prioridad** | Alta |
