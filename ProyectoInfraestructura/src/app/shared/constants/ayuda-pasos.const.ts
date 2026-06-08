import { AyudaPaso } from '../components/help-panel/help-panel.component';

export const AYUDA_PASOS: Record<string, AyudaPaso> = {

  contacto: {
    titulo: 'Datos de contacto y ente solicitante',
    descripcion: 'En este paso debes proporcionar los datos del responsable del área que solicita el servidor y los datos del administrador técnico que gestionará el servidor.',
    campos: [
      { nombre: 'Sector', requerido: true, descripcion: 'Sector al que pertenece la dependencia solicitante.', ejemplo: 'Educación, Salud, Seguridad' },
      { nombre: 'Dependencia', requerido: true, descripcion: 'Nombre oficial de la institución o dependencia que realiza la solicitud.', ejemplo: 'Secretaría de Educación Pública del Estado de Sonora' },
      { nombre: 'Responsable', requerido: true, descripcion: 'Nombre completo del servidor público que firma y es responsable de la solicitud.' },
      { nombre: 'Cargo', requerido: true, descripcion: 'Puesto oficial del responsable dentro de la dependencia.', ejemplo: 'Director de Tecnologías de la Información' },
      { nombre: 'Teléfono de contacto', requerido: true, descripcion: 'Número de 10 dígitos donde se pueda localizar al responsable.', ejemplo: '6621234567' },
      { nombre: 'Correo electrónico', requerido: true, descripcion: 'Correo institucional del responsable para recibir notificaciones del proceso.', ejemplo: 'responsable@sonora.gob.mx' },
      { nombre: 'Proveedor', requerido: true, descripcion: 'Nombre de la empresa o persona que administrará técnicamente el servidor.' },
      { nombre: 'Administrador del servidor', requerido: true, descripcion: 'Nombre completo de la persona que tendrá acceso y administración directa del servidor virtual.' },
    ]
  },

  descripcion: {
    titulo: 'Descripción general del requerimiento',
    descripcion: 'Describe el propósito del servidor y los datos generales de la solicitud. Esta información se usará para evaluar y clasificar tu solicitud.',
    campos: [
      { nombre: 'Descripción del servidor', requerido: true, descripcion: 'Explica para qué se usará el servidor. Sé específico sobre las aplicaciones o servicios que alojará.', ejemplo: 'Servidor para alojar el sistema de control escolar de nivel básico' },
      { nombre: 'Nombre del servidor', requerido: true, descripcion: 'Nombre técnico propuesto para identificar el servidor dentro del centro de datos.', ejemplo: 'SRV-SEP-CTRL-01' },
      { nombre: 'Nombre de la aplicación', requerido: true, descripcion: 'Nombre del sistema o aplicación que correrá en el servidor.', ejemplo: 'Sistema de Control Escolar Sonora' },
      { nombre: 'Tipo de uso', requerido: true, descripcion: 'Indica si el servidor será de uso interno (solo accesible dentro de la red del gobierno) o publicado (accesible desde internet).' },
      { nombre: 'Fecha de arranque', requerido: true, descripcion: 'Fecha en que necesitas que el servidor esté disponible para su uso.' },
      { nombre: 'Vigencia', requerido: true, descripcion: 'Período durante el cual necesitas el servidor.', ejemplo: '1 año, 6 meses' },
      { nombre: 'Características especiales', requerido: false, descripcion: 'Cualquier requerimiento adicional que no encaje en los campos anteriores.' },
    ]
  },

  specs: {
    titulo: 'Especificaciones técnicas',
    descripcion: 'Define los recursos de cómputo que necesita tu servidor. Si no tienes certeza de los valores, consulta con tu área de TI o selecciona el requerimiento estándar.',
    campos: [
      { nombre: 'Tipo de requerimiento', requerido: true, descripcion: 'Estándar usa configuraciones predefinidas. Específico permite definir recursos personalizados con licenciamiento propio.' },
      { nombre: 'Modalidad', requerido: true, descripcion: 'Nuevo crea un servidor desde cero. Clonación copia un servidor existente. Server base usa una plantilla predefinida por la ATDT.' },
      { nombre: 'Sistema operativo', requerido: true, descripcion: 'Sistema operativo que se instalará en el servidor virtual.' },
      { nombre: 'vCores', requerido: true, descripcion: 'Número de núcleos virtuales de procesador. El estándar es 2 vCores para la mayoría de aplicaciones.', ejemplo: '2' },
      { nombre: 'Memoria RAM', requerido: true, descripcion: 'Cantidad de memoria RAM en GB. Windows Server requiere mínimo 4 GB, Linux Ubuntu 2 GB.', ejemplo: '4' },
      { nombre: 'Almacenamiento', requerido: true, descripcion: 'Espacio en disco duro en GB. El estándar es 50 GB.', ejemplo: '50' },
      { nombre: 'Motor de base de datos', requerido: false, descripcion: 'Si tu aplicación usa base de datos, indica cuál.', ejemplo: 'MySQL 8.0, PostgreSQL 14' },
      { nombre: 'Puertos', requerido: false, descripcion: 'Lista los puertos que necesita abrir tu aplicación.', ejemplo: '80, 443, 3306' },
    ]
  },

  infraestructura: {
    titulo: 'Infraestructura y acceso VPN',
    descripcion: 'Define cómo se accederá al servidor y los datos del responsable de la VPN. La VPN es el único método de acceso remoto autorizado.',
    campos: [
      { nombre: 'Subdominio solicitado', requerido: false, descripcion: 'Subdominio bajo el cual se publicará tu aplicación si es de tipo publicado.', ejemplo: 'miapp.sonora.gob.mx' },
      { nombre: 'Puerto', requerido: false, descripcion: 'Puerto específico que usará la aplicación publicada.', ejemplo: '8080, 443' },
      { nombre: 'Requiere certificado SSL', requerido: false, descripcion: 'Indica si necesitas que la comunicación sea segura mediante HTTPS.' },
      { nombre: 'Responsable VPN', requerido: true, descripcion: 'Persona que será titular del acceso VPN. El acceso es personal e intransferible.' },
      { nombre: 'Correo electrónico VPN', requerido: true, descripcion: 'Correo donde se enviarán las credenciales de acceso VPN.', ejemplo: 'admin@sonora.gob.mx' },
    ]
  },

  responsiva: {
    titulo: 'Responsabilidades y firma',
    descripcion: 'Al firmar esta carta, el responsable se compromete a usar el servidor exclusivamente para los fines descritos, mantener las credenciales seguras y notificar cualquier incidente de seguridad.',
    campos: [
      { nombre: 'Nombre completo', requerido: true, descripcion: 'Nombre completo del servidor público que asume la responsabilidad legal del servidor.' },
      { nombre: 'Número de empleado', requerido: true, descripcion: 'Clave o número de empleado del firmante dentro de la dependencia.' },
      { nombre: 'Puesto', requerido: true, descripcion: 'Cargo oficial del firmante.' },
      { nombre: 'Acepto los términos', requerido: true, descripcion: 'Confirmas haber leído y aceptado todas las responsabilidades descritas en la Carta Responsiva de la ATDT.' },
    ]
  }
};
