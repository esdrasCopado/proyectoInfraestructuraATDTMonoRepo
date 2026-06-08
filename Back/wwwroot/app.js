async function api(path, opts = {}) {
  const token = localStorage.getItem('token');
  const headers = { 'Content-Type': 'application/json', ...(opts.headers || {}) };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(path, { ...opts, headers });

  if (res.status === 401) {
    localStorage.clear();
    showLogin();
    throw new Error('Sesión expirada');
  }

  const contentType = res.headers.get('content-type') || '';
  const payload = contentType.includes('application/json') ? await res.json() : await res.text();

  if (!res.ok) {
    const message = typeof payload === 'string'
      ? payload
      : payload?.message || payload?.title || `${res.status}: ${res.statusText}`;
    throw new Error(message);
  }

  return payload;
}

const loginView = document.getElementById('loginView');
const appView = document.getElementById('appView');
const loginForm = document.getElementById('loginForm');
const requestForm = document.getElementById('requestForm');
const userForm = document.getElementById('userForm');
const btnLogout = document.getElementById('btnLogout');
const userDisplay = document.getElementById('userDisplay');
const systemInfo = document.getElementById('systemInfo');
const navItems = document.querySelectorAll('.nav-item');
const sections = document.querySelectorAll('.content-section');
const serversDiv = document.getElementById('servers');
const addServerBtn = document.getElementById('addServer');
const requestUserId = document.getElementById('requestUserId');
const userRole = document.getElementById('userRole');
const requestsList = document.getElementById('requestsList');
const usersList = document.getElementById('usersList');
const dashboardSummary = document.getElementById('dashboardSummary');
const dashboardRequestStages = document.getElementById('dashboardRequestStages');
const dashboardServerStages = document.getElementById('dashboardServerStages');
const reportSummary = document.getElementById('reportSummary');
const reportDetails = document.getElementById('reportDetails');
const reportSearch = document.getElementById('reportSearch');
const reportStageFilter = document.getElementById('reportStageFilter');
const reportStatusFilter = document.getElementById('reportStatusFilter');
const reportOnlyPending = document.getElementById('reportOnlyPending');
const exportReportCsv = document.getElementById('exportReportCsv');
const cartasList = document.getElementById('cartasList');

let serverIndex = 0;
let cachedUsers = [];
let cachedRequestsData = [];
let cachedServersById = new Map();

navItems.forEach(item => {
  item.addEventListener('click', event => {
    event.preventDefault();
    const sectionId = item.dataset.section;
    navItems.forEach(i => i.classList.remove('active'));
    sections.forEach(section => section.style.display = 'none');
    item.classList.add('active');
    document.getElementById(sectionId).style.display = 'block';
  });
});

loginForm.addEventListener('submit', async event => {
  event.preventDefault();
  const email = document.getElementById('email').value.trim();
  const password = document.getElementById('password').value;

  try {
    const data = await api('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });

    localStorage.setItem('token', data.token || '');
    localStorage.setItem('currentUser', JSON.stringify(data.user || {}));
    showApp();
  } catch (error) {
    alert(`Login falló: ${error.message}`);
    document.getElementById('password').value = '';
  }
});

btnLogout.addEventListener('click', () => {
  localStorage.clear();
  loginForm.reset();
  showLogin();
});

document.getElementById('refreshRequests').addEventListener('click', loadRequests);
document.getElementById('refreshUsers').addEventListener('click', loadUsers);
document.getElementById('refreshReports').addEventListener('click', loadReports);
reportSearch?.addEventListener('input', () => loadReports());
reportStageFilter?.addEventListener('change', loadReports);
reportStatusFilter?.addEventListener('change', loadReports);
reportOnlyPending?.addEventListener('change', loadReports);
exportReportCsv?.addEventListener('click', exportReportsCsv);

addServerBtn.addEventListener('click', event => {
  event.preventDefault();
  addServerEntry();
});

serversDiv.addEventListener('click', event => {
  const removeButton = event.target.closest('.removeServer');
  if (!removeButton) return;
  event.preventDefault();
  removeButton.closest('.server-entry').remove();
  updateServerNumbers();
});

requestsList.addEventListener('click', async event => {
  const markReadButton = event.target.closest('.mark-read-btn');
  if (markReadButton) {
    try {
      await api(`/api/solicitud/${markReadButton.dataset.id}/notificacion-leida`, { method: 'PUT' });
      await Promise.all([loadRequests(), loadDashboard(), loadReports()]);
    } catch (error) {
      alert(`No se pudo marcar la notificación: ${error.message}`);
    }
    return;
  }

  const editRequestButton = event.target.closest('.edit-request-btn');
  if (editRequestButton) {
    await editRequestWorkflow(Number(editRequestButton.dataset.id));
    return;
  }

  const editServerButton = event.target.closest('.edit-server-btn');
  if (editServerButton) {
    await editServerWorkflow(Number(editServerButton.dataset.id));
  }
});

usersList.addEventListener('click', async event => {
  const editButton = event.target.closest('.edit-user-btn');
  if (editButton) {
    await editUser(Number(editButton.dataset.id));
    return;
  }

  const deleteButton = event.target.closest('.delete-user-btn');
  if (deleteButton) {
    await deleteUser(Number(deleteButton.dataset.id));
  }
});

requestForm.addEventListener('submit', async event => {
  event.preventDefault();

  const serverEntries = serversDiv.querySelectorAll('.server-entry');
  if (serverEntries.length === 0) {
    alert('Debes agregar al menos un servidor.');
    return;
  }

  const payload = {
    idUsuario: Number(requestUserId.value),
    titulo: document.getElementById('requestTitle').value.trim(),
    folio: document.getElementById('requestFolio').value.trim() || null,
    arquitectura: document.getElementById('architecture').value,
    descripcion: document.getElementById('description').value.trim(),
    servicios: document.getElementById('requiredServices').value.trim(),
    estado: document.getElementById('requestStatus').value,
    etapaActual: document.getElementById('requestStage').value,
    prioridad: document.getElementById('requestPriority').value,
    responsableActual: document.getElementById('requestOwner').value.trim() || null,
    fechaRequerida: document.getElementById('requestRequiredDate').value || null,
    cartaResponsivaFolio: document.getElementById('requestCartaFolio').value.trim() || null,
    comentariosSeguimiento: document.getElementById('requestComments').value.trim() || null,
    notificacionNueva: document.getElementById('requestNotification').checked,
    tareasPendientes: document.getElementById('requestTasks').value.trim(),
    servidores: Array.from(serverEntries).map(collectServerEntry)
  };

  try {
    const created = await api('/api/solicitud', {
      method: 'POST',
      body: JSON.stringify(payload)
    });

    alert(`Solicitud creada correctamente con ID ${created.id}.`);
    requestForm.reset();
    serversDiv.innerHTML = '';
    serverIndex = 0;
    addServerEntry();
    await loadRequests();
    await loadReports();
  } catch (error) {
    alert(`Error al crear la solicitud: ${error.message}`);
  }
});

userForm.addEventListener('submit', async event => {
  event.preventDefault();

  const rol = userRole.value || 'Dependencia / Cliente';
  const payload = {
    nombreCompleto: document.getElementById('userName').value.trim(),
    correo: document.getElementById('userEmail').value.trim(),
    password: document.getElementById('userPassword').value,
    rol,
    permisos: rol,
    puesto: document.getElementById('userPuesto').value.trim(),
    celular: document.getElementById('userCelular').value.trim(),
    numeroPuesto: document.getElementById('userNumeroPuesto').value.trim()
  };

  try {
    await api('/api/usuario', {
      method: 'POST',
      body: JSON.stringify(payload)
    });

    alert('Usuario creado correctamente.');
    userForm.reset();
    document.getElementById('userPassword').value = 'admin123';
    await loadUsers();
  } catch (error) {
    alert(`Error al crear usuario: ${error.message}`);
  }
});

function showApp() {
  const currentUser = JSON.parse(localStorage.getItem('currentUser') || '{}');
  const displayName = currentUser.nombreCompleto || currentUser.NombreCompleto || currentUser.nombre || 'Usuario';
  const roleName = currentUser.rol || currentUser.Rol || 'Sin rol';

  userDisplay.textContent = `${displayName} (${roleName})`;
  loginView.style.display = 'none';
  appView.style.display = 'block';

  applyRoleVisibility(roleName);
  sections.forEach((section, index) => section.style.display = index === 0 ? 'block' : 'none');
  navItems.forEach((item, index) => item.classList.toggle('active', index === 0));

  bootstrapDashboard();
}

function showLogin() {
  loginView.style.display = 'block';
  appView.style.display = 'none';
}

function applyRoleVisibility(roleName) {
  const isAdmin = /administrador/i.test(roleName || '');
  const userNav = document.querySelector('.nav-item[data-section="users-admin"]');
  if (userNav) userNav.style.display = isAdmin ? 'flex' : 'none';
}

async function bootstrapDashboard() {
  addServerEntryIfMissing();
  await Promise.all([loadSystemInfo(), loadRoles(), loadUsers(), loadRequests(), loadDashboard(), loadReports(), loadCartas()]);
}

async function loadSystemInfo() {
  try {
    const health = await api('/health');
    systemInfo.textContent = `Proveedor: ${health.provider} • UTC: ${new Date(health.utc).toLocaleString('es-MX')}`;
  } catch {
    systemInfo.textContent = 'No fue posible consultar /health';
  }
}

async function loadRoles() {
  const roles = await api('/api/usuario/roles');
  const options = roles.map(role => `<option value="${role}">${role}</option>`).join('');
  userRole.innerHTML = options;
}

async function loadUsers() {
  try {
    cachedUsers = await api('/api/usuario/todos');

    requestUserId.innerHTML = cachedUsers.length
      ? cachedUsers.map(user => `<option value="${user.id}">${user.nombreCompleto} - ${user.correo}</option>`).join('')
      : '<option value="1">Administrador General - admin@local</option>';

    usersList.innerHTML = cachedUsers.length
      ? cachedUsers.map(user => `
          <div class="request-item">
            <div class="request-header">
              <div>
                <div class="request-title">${user.nombreCompleto}</div>
                <div style="font-size:0.9em;color:#666;">${user.correo}</div>
              </div>
              <span class="status-badge status-Pendiente">${user.rol || user.permisos || 'Sin rol'}</span>
            </div>
            <div class="request-details">
              <div class="detail-item"><div class="detail-label">Puesto</div><div class="detail-value">${user.puesto || 'N/D'}</div></div>
              <div class="detail-item"><div class="detail-label">Celular</div><div class="detail-value">${user.celular || 'N/D'}</div></div>
              <div class="detail-item"><div class="detail-label">Número de puesto</div><div class="detail-value">${user.numeroPuesto || 'N/D'}</div></div>
            </div>
            <div class="toolbar-row" style="margin-top:0.75rem;">
              <button class="btn-secondary edit-user-btn" data-id="${user.id}"><i class="bi bi-pencil"></i> Editar</button>
              <button class="btn-secondary delete-user-btn" data-id="${user.id}"><i class="bi bi-trash"></i> Eliminar</button>
            </div>
          </div>
        `).join('')
      : '<p>No hay usuarios registrados.</p>';
  } catch (error) {
    usersList.innerHTML = `<p style="color:#e74c3c;">${error.message}</p>`;
  }
}

async function editUser(userId) {
  const user = cachedUsers.find(item => Number(item.id) === Number(userId));
  if (!user) return;

  const nombreCompleto = prompt('Nombre completo', user.nombreCompleto || '');
  if (nombreCompleto === null) return;
  const correo = prompt('Correo', user.correo || '');
  if (correo === null) return;
  const rol = prompt('Rol', user.rol || user.permisos || 'Dependencia / Cliente');
  if (rol === null) return;
  const puesto = prompt('Puesto', user.puesto || '');
  if (puesto === null) return;
  const celular = prompt('Celular', user.celular || '');
  if (celular === null) return;
  const numeroPuesto = prompt('Número de puesto', user.numeroPuesto || '');
  if (numeroPuesto === null) return;

  try {
    await api(`/api/usuario/${userId}`, {
      method: 'PUT',
      body: JSON.stringify({ nombreCompleto, correo, rol, permisos: rol, puesto, celular, numeroPuesto })
    });
    await loadUsers();
  } catch (error) {
    alert(`No se pudo actualizar el usuario: ${error.message}`);
  }
}

async function deleteUser(userId) {
  const user = cachedUsers.find(item => Number(item.id) === Number(userId));
  if (!user) return;
  if (!confirm(`¿Eliminar al usuario ${user.nombreCompleto}?`)) return;

  try {
    await api(`/api/usuario/${userId}`, { method: 'DELETE' });
    await loadUsers();
  } catch (error) {
    alert(`No se pudo eliminar el usuario: ${error.message}`);
  }
}

async function editRequestWorkflow(requestId) {
  const request = cachedRequestsData.find(item => Number(item.id) === Number(requestId));
  if (!request) return;

  const currentUser = JSON.parse(localStorage.getItem('currentUser') || '{}');
  const actor = currentUser.nombreCompleto || currentUser.NombreCompleto || currentUser.nombre || 'Usuario actual';
  const estado = prompt('Estado de la solicitud', request.estado || 'Pendiente');
  if (estado === null) return;
  const etapaActual = prompt('Etapa actual', request.etapaActual || 'Registro');
  if (etapaActual === null) return;
  const prioridad = prompt('Prioridad', request.prioridad || 'Media');
  if (prioridad === null) return;
  const responsableActual = prompt('Responsable actual', request.responsableActual || actor);
  if (responsableActual === null) return;
  const tareasPendientes = prompt('Tareas pendientes', request.tareasPendientes || '');
  if (tareasPendientes === null) return;
  const comentariosSeguimiento = prompt('Comentarios de seguimiento', request.comentariosSeguimiento || '');
  if (comentariosSeguimiento === null) return;

  try {
    await api(`/api/solicitud/${requestId}`, {
      method: 'PUT',
      body: JSON.stringify({ estado, etapaActual, prioridad, responsableActual, usuarioUltimaActualizacion: actor, fechaActualizacion: new Date().toISOString(), tareasPendientes, comentariosSeguimiento })
    });
    await Promise.all([loadRequests(), loadDashboard(), loadReports()]);
  } catch (error) {
    alert(`No se pudo actualizar la solicitud: ${error.message}`);
  }
}

async function editServerWorkflow(serverId) {
  const server = cachedServersById.get(Number(serverId));
  if (!server) return;

  const estado = prompt('Estado del servidor', server.estado || 'Pendiente');
  if (estado === null) return;
  const etapaOperativa = prompt('Etapa operativa', server.etapaOperativa || 'Provisionamiento');
  if (etapaOperativa === null) return;
  const responsableInfraestructura = prompt('Responsable de infraestructura', server.responsableInfraestructura || '');
  if (responsableInfraestructura === null) return;
  const ip = prompt('IP asignada', server.ip || '');
  if (ip === null) return;
  const etapaVulnerabilidades = prompt('Etapa de vulnerabilidades', server.etapaVulnerabilidades || 'Pendiente');
  if (etapaVulnerabilidades === null) return;
  const observacionesSeguimiento = prompt('Observaciones de seguimiento', server.observacionesSeguimiento || '');
  if (observacionesSeguimiento === null) return;

  try {
    const currentUser = JSON.parse(localStorage.getItem('currentUser') || '{}');
    const actor = currentUser.nombreCompleto || currentUser.NombreCompleto || currentUser.nombre || 'Usuario actual';
    const comunicacionValidada = confirm('¿Comunicaciones validadas?');
    const parchesAplicados = confirm('¿Actualizaciones y parches aplicados?');
    const xdrInstalado = confirm('¿XDR y agente instalados?');
    const credencialesEntregadas = confirm('¿Credenciales entregadas?');
    const wafConfigurado = confirm('¿WAF configurado?');
    const evidenciaValidada = confirm('¿Evidencias aprobadas/validadas?');
    const solicitudPublicacion = confirm('¿Solicitud lista para publicación?');
    const now = new Date().toISOString();

    await api(`/api/servidor/${serverId}`, {
      method: 'PUT',
      body: JSON.stringify({
        estado,
        ip: ip || null,
        etapaOperativa,
        responsableInfraestructura: responsableInfraestructura || actor,
        usuarioUltimaActualizacion: actor,
        fechaUltimaActualizacion: now,
        observacionesSeguimiento,
        etapaVulnerabilidades,
        comunicacionValidada,
        fechaValidacionComunicacion: comunicacionValidada ? now : null,
        usuarioValidacionComunicacion: comunicacionValidada ? actor : null,
        parchesAplicados,
        fechaParches: parchesAplicados ? now : null,
        usuarioParches: parchesAplicados ? actor : null,
        xdrInstalado,
        fechaXdr: xdrInstalado ? now : null,
        usuarioXdr: xdrInstalado ? actor : null,
        credencialesEntregadas,
        fechaEntregaCredenciales: credencialesEntregadas ? now : null,
        usuarioCredenciales: credencialesEntregadas ? actor : null,
        wafConfigurado,
        fechaConfiguracionWaf: wafConfigurado ? now : null,
        usuarioWaf: wafConfigurado ? actor : null,
        evidenciaValidada,
        fechaValidacionEvidencia: evidenciaValidada ? now : null,
        usuarioValidacionEvidencia: evidenciaValidada ? actor : null,
        solicitudPublicacion,
        fechaPublicacion: solicitudPublicacion ? now : null,
        usuarioPublicacion: solicitudPublicacion ? actor : null,
        fechaVulnerabilidades: now,
        usuarioVulnerabilidades: actor,
        vpns: server.vpns || server.vpNs || [],
        subdominios: server.subdominios || [],
        wafs: server.wafs || server.waFs || [],
        evidenciasPruebas: server.evidenciasPruebas || []
      })
    });
    await Promise.all([loadRequests(), loadDashboard(), loadReports()]);
  } catch (error) {
    alert(`No se pudo actualizar el servidor: ${error.message}`);
  }
}

async function loadDashboard() {
  try {
    const [solicitudes, servidores] = await Promise.all([
      api('/api/solicitud/dashboard/resumen'),
      api('/api/servidor/dashboard/resumen')
    ]);

    dashboardSummary.innerHTML = `
      <div class="report-card"><h3>${solicitudes.total}</h3><p>Solicitudes</p></div>
      <div class="report-card"><h3>${solicitudes.nuevas}</h3><p>Nuevas</p></div>
      <div class="report-card"><h3>${servidores.total}</h3><p>Servidores</p></div>
      <div class="report-card"><h3>${servidores.publicados}</h3><p>Publicados</p></div>
      <div class="report-card"><h3>${servidores.credencialesEntregadas}</h3><p>Credenciales entregadas</p></div>
      <div class="report-card"><h3>${servidores.wafConfigurado}</h3><p>WAF configurado</p></div>
    `;

    dashboardRequestStages.innerHTML = renderStageList('Etapas de solicitudes', solicitudes.porEtapa || [], 'etapa');
    dashboardServerStages.innerHTML = renderStageList('Etapas operativas', servidores.porEtapa || [], 'etapa');
  } catch (error) {
    dashboardSummary.innerHTML = `<p style="color:#e74c3c;">${error.message}</p>`;
    dashboardRequestStages.innerHTML = '';
    dashboardServerStages.innerHTML = '';
  }
}

async function loadCartas() {
  try {
    const cartas = await api('/api/cartas');
    cartasList.innerHTML = cartas.length
      ? cartas.slice(0, 8).map(carta => `
          <div class="request-item">
            <div class="request-header">
              <div>
                <div class="request-title">${carta.folio}</div>
                <div style="font-size:0.9em;color:#666;">${new Date(carta.creadoEn).toLocaleString('es-MX')}</div>
              </div>
              <span class="status-badge status-Pendiente">${carta.solicitudFolio || 'Sin solicitud'}</span>
            </div>
            <div class="toolbar-row" style="margin-top:0.75rem;">
              <a class="btn-secondary" target="_blank" href="/api/cartas/${carta.id}/pdf"><i class="bi bi-file-earmark-pdf"></i> Descargar PDF</a>
            </div>
          </div>
        `).join('')
      : '<p>No hay cartas registradas todavía.</p>';
  } catch (error) {
    cartasList.innerHTML = `<p style="color:#e74c3c;">${error.message}</p>`;
  }
}

async function loadRequests() {
  try {
    const requests = await api('/api/solicitud/todas');
    cachedRequestsData = requests;
    cachedServersById = new Map();
    requests.forEach(request => (request.servidores || []).forEach(server => cachedServersById.set(Number(server.id), server)));

    if (!requests.length) {
      requestsList.innerHTML = '<p style="text-align:center;color:#777;">No hay solicitudes todavía.</p>';
      return;
    }

    requestsList.innerHTML = requests.map(request => {
      const servidores = request.servidores || [];
      const usuario = request.usuario?.nombreCompleto || 'Usuario no asignado';
      const nueva = request.notificacionNueva === true;
      const folio = request.folio || `SOL-${request.id}`;

      return `
        <div class="request-item">
          <div class="request-header">
            <div>
              <div class="request-title">${folio} - ${request.titulo || 'Sin título'}</div>
              <div style="font-size:0.9em;color:#666;">${usuario} • ${request.arquitectura || 'Sin arquitectura'} • ${request.etapaActual || 'Registro'}</div>
            </div>
            <span class="status-badge status-${slugify(request.estado || 'Pendiente')}">${request.estado || 'Pendiente'}</span>
          </div>

          <div class="request-details">
            <div class="detail-item"><div class="detail-label">Prioridad</div><div class="detail-value">${request.prioridad || 'Media'}</div></div>
            <div class="detail-item"><div class="detail-label">Responsable actual</div><div class="detail-value">${request.responsableActual || 'Sin asignar'}</div></div>
            <div class="detail-item"><div class="detail-label">Servicios</div><div class="detail-value">${request.servicios || 'N/D'}</div></div>
            <div class="detail-item"><div class="detail-label">Fecha requerida</div><div class="detail-value">${request.fechaRequerida ? new Date(request.fechaRequerida).toLocaleDateString('es-MX') : 'N/D'}</div></div>
            <div class="detail-item"><div class="detail-label">Carta responsiva</div><div class="detail-value">${request.cartaResponsivaFolio || 'N/D'}</div></div>
            <div class="detail-item"><div class="detail-label">Servidores</div><div class="detail-value">${servidores.length}</div></div>
            <div class="detail-item"><div class="detail-label">Descripción</div><div class="detail-value">${request.descripcion || 'N/D'}</div></div>
            <div class="detail-item"><div class="detail-label">Tareas pendientes</div><div class="detail-value">${request.tareasPendientes || 'N/D'}</div></div>
          </div>

          ${request.comentariosSeguimiento ? `<div class="detail-value" style="margin-top:0.75rem;"><strong>Seguimiento:</strong> ${request.comentariosSeguimiento}</div>` : ''}
          <div class="toolbar-row" style="margin-top:0.75rem;">
            <button class="btn-secondary edit-request-btn" data-id="${request.id}"><i class="bi bi-pencil-square"></i> Editar seguimiento</button>
            ${nueva ? `<button class="btn-secondary mark-read-btn" data-id="${request.id}"><i class="bi bi-bell-slash"></i> Marcar notificación leída</button>` : ''}
          </div>

          <details style="margin-top:1rem;">
            <summary style="cursor:pointer;font-weight:600;color:var(--primary);">Ver servidores (${servidores.length})</summary>
            <div style="margin-top:1rem;display:grid;gap:0.75rem;">
              ${servidores.map(server => {
                const vpns = server.serverVpns?.map(sv => sv.vpn).filter(Boolean) || [];
                const subdominios = server.serverSubdominios?.map(ss => ss.subdominio).filter(Boolean) || [];
                const vpnRows = vpns.length
                  ? vpns.map(v => `
                      <tr>
                        <td>${v.folio || '—'}</td>
                        <td>${v.empresa || '—'}</td>
                        <td>${v.responsable || '—'}</td>
                        <td>${v.email || '—'}</td>
                        <td>${v.estado || '—'}</td>
                        <td>${server.ip || '—'}</td>
                        <td>${v.externalId || '—'}</td>
                        <td>${v.fechaAsignacion || '—'}</td>
                        <td>${v.fechaExpiracion || '—'}</td>
                        <td>${v.vigenciaDias != null ? v.vigenciaDias + ' días' : '—'}</td>
                        <td>${v.vpnType || '—'}</td>
                      </tr>`).join('')
                  : '<tr><td colspan="11" style="color:#888;font-style:italic;">Sin VPNs registradas</td></tr>';
                const subRows = subdominios.length
                  ? subdominios.map(s => `
                      <tr>
                        <td>${s.approvedName || s.requestedName || '—'}</td>
                        <td>${s.status || '—'}</td>
                        <td>${s.assignedAt ? new Date(s.assignedAt).toLocaleDateString('es-MX') : '—'}</td>
                        <td>${s.expiresAt ? new Date(s.expiresAt).toLocaleDateString('es-MX') : '—'}</td>
                        <td>${s.sslRequired ? 'Sí' : 'No'}</td>
                      </tr>`).join('')
                  : '<tr><td colspan="5" style="color:#888;font-style:italic;">Sin subdominios registrados</td></tr>';
                return `
                <div style="background:#f9f9f9;padding:0.9rem;border-radius:8px;">
                  <strong>${server.hostname || 'Sin hostname'}</strong> • ${server.tipoUso || 'Interno'} • ${server.sistemaOperativo || 'N/D'}<br/>
                  Función: ${server.funcion || 'N/D'} • Etapa: ${server.etapaOperativa || 'Provisionamiento'}<br/>
                  Responsable: ${server.responsableInfraestructura || 'Sin asignar'}<br/>
                  IP: ${server.ip || 'Pendiente'} • Recursos: ${server.nucleos || 0} CPU / ${server.ram || 0} GB / ${server.almacenamiento || 0} GB
                  ${renderTimelineChecklist(server)}

                  <details style="margin-top:0.75rem;">
                    <summary style="cursor:pointer;font-weight:600;color:var(--primary);">VPNs (${vpns.length})</summary>
                    <div style="overflow-x:auto;margin-top:0.5rem;">
                      <table style="width:100%;border-collapse:collapse;font-size:0.82em;">
                        <thead>
                          <tr style="background:#e8ecf0;text-align:left;">
                            <th style="padding:4px 8px;">Identificador</th>
                            <th style="padding:4px 8px;">Dependencia/Proveedor</th>
                            <th style="padding:4px 8px;">Responsable</th>
                            <th style="padding:4px 8px;">Contacto</th>
                            <th style="padding:4px 8px;">Estatus</th>
                            <th style="padding:4px 8px;">IP Servidor</th>
                            <th style="padding:4px 8px;">Usuario asignado</th>
                            <th style="padding:4px 8px;">Fecha creación</th>
                            <th style="padding:4px 8px;">Fecha vencimiento</th>
                            <th style="padding:4px 8px;">Vigencia</th>
                            <th style="padding:4px 8px;">Tipo VPN</th>
                          </tr>
                        </thead>
                        <tbody>${vpnRows}</tbody>
                      </table>
                    </div>
                  </details>

                  <details style="margin-top:0.5rem;">
                    <summary style="cursor:pointer;font-weight:600;color:var(--primary);">Subdominios (${subdominios.length})</summary>
                    <div style="overflow-x:auto;margin-top:0.5rem;">
                      <table style="width:100%;border-collapse:collapse;font-size:0.82em;">
                        <thead>
                          <tr style="background:#e8ecf0;text-align:left;">
                            <th style="padding:4px 8px;">URL / Nombre</th>
                            <th style="padding:4px 8px;">Estado</th>
                            <th style="padding:4px 8px;">Fecha asignación</th>
                            <th style="padding:4px 8px;">Fecha expiración</th>
                            <th style="padding:4px 8px;">SSL</th>
                          </tr>
                        </thead>
                        <tbody>${subRows}</tbody>
                      </table>
                    </div>
                  </details>

                  <div class="toolbar-row" style="margin-top:0.75rem;">
                    <button class="btn-secondary edit-server-btn" data-id="${server.id}"><i class="bi bi-sliders"></i> Actualizar flujo</button>
                  </div>
                </div>`;
              }).join('')}
            </div>
          </details>
        </div>
      `;
    }).join('');
  } catch (error) {
    requestsList.innerHTML = `<p style="color:#e74c3c;">${error.message}</p>`;
  }
}

async function loadReports() {
  try {
    const query = buildReportQuery();
    const [vulnerabilidades, revisionAnual, vpns, notificaciones, detalle] = await Promise.all([
      api('/api/servidor/reportes/vulnerabilidades-pendientes'),
      api('/api/servidor/reportes/revision-anual'),
      api('/api/servidor/reportes/vpns-por-expirar?dias=30'),
      api('/api/solicitud/notificaciones/nuevas'),
      api(`/api/servidor/reportes/detallado?${query}`)
    ]);

    reportSummary.innerHTML = `
      <div class="report-card"><h3>${vulnerabilidades.length}</h3><p>Vulnerabilidades pendientes</p></div>
      <div class="report-card"><h3>${revisionAnual.length}</h3><p>Revisión anual pendiente</p></div>
      <div class="report-card"><h3>${vpns.length}</h3><p>VPNs por expirar</p></div>
      <div class="report-card"><h3>${notificaciones.length}</h3><p>Nuevas solicitudes</p></div>
      <div class="report-card"><h3>${detalle.length}</h3><p>Resultados filtrados</p></div>
    `;

    reportDetails.innerHTML = [
      renderReportBlock('Reporte filtrado actual', detalle, item => `${item.hostname || 'Sin hostname'} • ${item.etapaOperativa || 'Sin etapa'} • ${item.solicitud?.folio || 'Sin folio'}`),
      renderReportBlock('Servidores en etapa de vulnerabilidades', vulnerabilidades, item => `${item.hostname || 'Sin hostname'} • ${item.etapaVulnerabilidades || 'Pendiente'}`),
      renderReportBlock('Servidores con revisión anual pendiente', revisionAnual, item => item.hostname || 'Sin hostname'),
      renderReportBlock('Solicitudes con notificación nueva', notificaciones, item => `${item.folio || `Solicitud #${item.id}`} • ${item.titulo || 'Sin título'}`)
    ].join('');
  } catch (error) {
    reportSummary.innerHTML = `<p style="color:#e74c3c;">${error.message}</p>`;
    reportDetails.innerHTML = '';
  }
}

function renderReportBlock(title, items, getLabel) {
  return `
    <div class="request-item">
      <div class="request-header">
        <div class="request-title">${title}</div>
        <span class="status-badge status-Pendiente">${items.length}</span>
      </div>
      <div class="detail-value">
        ${items.length ? `<ul class="report-list">${items.slice(0, 12).map(item => `<li>${getLabel(item)}</li>`).join('')}</ul>` : 'Sin elementos pendientes.'}
      </div>
    </div>
  `;
}

function renderStageList(title, items, key) {
  return `
    <div class="request-item">
      <div class="request-header">
        <div class="request-title">${title}</div>
        <span class="status-badge status-Pendiente">${items.length}</span>
      </div>
      <div class="detail-value">
        ${items.length ? `<ul class="report-list">${items.map(item => `<li>${item[key]}: <strong>${item.total}</strong></li>`).join('')}</ul>` : 'Sin datos.'}
      </div>
    </div>
  `;
}

function renderTimelineChecklist(server) {
  const checks = [
    ['Comunicaciones', server.comunicacionValidada],
    ['Parches', server.parchesAplicados],
    ['XDR', server.xdrInstalado],
    ['Credenciales', server.credencialesEntregadas],
    ['WAF', server.wafConfigurado],
    ['Evidencia', server.evidenciaValidada],
    ['Publicación', server.solicitudPublicacion]
  ];

  return `
    <div class="timeline-checklist">
      ${checks.map(([label, ok]) => `<span class="mini-chip ${ok ? 'ok' : 'pending'}">${label}: ${ok ? '✓' : '•'}</span>`).join('')}
    </div>
  `;
}

function buildReportQuery() {
  const params = new URLSearchParams();
  if (reportSearch?.value.trim()) params.set('buscar', reportSearch.value.trim());
  if (reportStageFilter?.value) params.set('etapa', reportStageFilter.value);
  if (reportStatusFilter?.value) params.set('estado', reportStatusFilter.value);
  if (reportOnlyPending?.checked) params.set('soloPendientes', 'true');
  return params.toString();
}

function exportReportsCsv() {
  const query = buildReportQuery();
  window.open(`/api/servidor/reportes/exportar-csv?${query}`, '_blank');
}

function addServerEntryIfMissing() {
  if (!serversDiv.children.length) addServerEntry();
}

function addServerEntry() {
  const idx = serverIndex++;
  const wrapper = document.createElement('div');
  wrapper.className = 'server-entry';
  wrapper.innerHTML = `
    <div class="server-entry-header">
      <h4><i class="bi bi-server"></i> Servidor #${idx + 1}</h4>
      <button type="button" class="btn-secondary removeServer"><i class="bi bi-trash"></i> Eliminar</button>
    </div>

    <div class="form-grid">
      <div class="form-group"><label>Hostname *</label><input name="hostname_${idx}" required /></div>
      <div class="form-group"><label>IP (opcional)</label><input name="ip_${idx}" placeholder="Se asigna después si aplica" /></div>
      <div class="form-group"><label>Función *</label><input name="funcion_${idx}" placeholder="Web, BD, API..." required /></div>
      <div class="form-group">
        <label>Tipo de uso *</label>
        <select name="tipoUso_${idx}">
          <option value="Interno">Interno</option>
          <option value="Publicado">Publicado</option>
        </select>
      </div>
      <div class="form-group"><label>Sistema operativo *</label><input name="os_${idx}" placeholder="Windows / Ubuntu / Otro" required /></div>
      <div class="form-group"><label>Plantilla recursos</label><select name="plantilla_${idx}"><option value="General">General</option><option value="Básico">Básico</option><option value="Estándar">Estándar</option><option value="Avanzado">Avanzado</option></select></div>
      <div class="form-group"><label>Núcleos *</label><input name="nucleos_${idx}" type="number" min="1" value="2" required /></div>
      <div class="form-group"><label>RAM GB *</label><input name="ram_${idx}" type="number" min="1" value="8" required /></div>
      <div class="form-group"><label>Almacenamiento GB *</label><input name="almacenamiento_${idx}" type="number" min="10" value="100" required /></div>
      <div class="form-group checkbox-group"><label><input name="requiereLlave_${idx}" type="checkbox" /> Requiere llave de licencia</label></div>
      <div class="form-group"><label>Llave OS</label><input name="llave_${idx}" placeholder="Sólo si aplica" /></div>
      <div class="form-group"><label>Etapa operativa</label><select name="etapaOperativa_${idx}"><option value="Provisionamiento">Provisionamiento</option><option value="Comunicaciones">Comunicaciones</option><option value="Parches">Parches</option><option value="XDR">XDR</option><option value="Credenciales">Credenciales</option><option value="WAF">WAF</option><option value="Validación">Validación</option><option value="Publicación">Publicación</option></select></div>
      <div class="form-group"><label>Responsable infraestructura</label><input name="responsableInfra_${idx}" placeholder="Administrador de Infraestructura" /></div>
      <div class="form-group"><label>Fecha asignación IP</label><input name="fechaIp_${idx}" type="date" /></div>
      <div class="form-group"><label>VPN(s)</label><input name="vpnTipo_${idx}" placeholder="OpenVPN, WireGuard (separadas por coma)" /></div>
      <div class="form-group"><label>Expiración VPN principal</label><input name="vpnExp_${idx}" type="date" /></div>
      <div class="form-group"><label>Subdominio(s)</label><input name="subdominio_${idx}" placeholder="app.sonora.gob.mx, api.sonora.gob.mx" /></div>
      <div class="form-group"><label>WAF / observaciones</label><input name="wafObs_${idx}" placeholder="Configuración inicial / reglas WAF" /></div>
      <div class="form-group"><label>Evidencia PDF / ruta(s)</label><input name="evidencia_${idx}" placeholder="/evidencias/prueba1.pdf, /evidencias/prueba2.pdf" /></div>
      <div class="form-group"><label>Etapa vulnerabilidades</label><input name="vulnerabilidades_${idx}" placeholder="Pendiente / En revisión / Completado" /></div>
      <div class="form-group"><label>Última revisión anual</label><input name="revision_${idx}" type="date" /></div>
      <div class="form-group checkbox-group"><label><input name="comunicacion_${idx}" type="checkbox" /> Comunicaciones validadas</label></div>
      <div class="form-group checkbox-group"><label><input name="parches_${idx}" type="checkbox" /> Parches aplicados</label></div>
      <div class="form-group checkbox-group"><label><input name="xdr_${idx}" type="checkbox" /> XDR/agente instalado</label></div>
      <div class="form-group checkbox-group"><label><input name="credenciales_${idx}" type="checkbox" /> Credenciales entregadas</label></div>
      <div class="form-group checkbox-group"><label><input name="waf_${idx}" type="checkbox" /> WAF configurado</label></div>
      <div class="form-group checkbox-group"><label><input name="evidenciaValidada_${idx}" type="checkbox" /> Evidencias validadas</label></div>
      <div class="form-group checkbox-group"><label><input name="publicacion_${idx}" type="checkbox" /> Solicitud de publicación</label></div>
      <div class="form-group full-width"><label>Descripción / tareas pendientes</label><textarea name="descripcion_${idx}" placeholder="Descripción del servidor y pendientes..."></textarea></div>
    </div>
  `;

  serversDiv.appendChild(wrapper);
  updateServerNumbers();
}

function updateServerNumbers() {
  serversDiv.querySelectorAll('.server-entry-header h4').forEach((title, index) => {
    title.innerHTML = `<i class="bi bi-server"></i> Servidor #${index + 1}`;
  });
}

function collectServerEntry(entry) {
  const value = name => entry.querySelector(`[name^="${name}_"]`)?.value?.trim() || '';
  const checked = name => entry.querySelector(`[name^="${name}_"]`)?.checked || false;
  const vpnTipos = value('vpnTipo').split(',').map(item => item.trim()).filter(Boolean);
  const vpnExp = value('vpnExp');
  const subdominios = value('subdominio').split(',').map(item => item.trim()).filter(Boolean);
  const evidencias = value('evidencia').split(',').map(item => item.trim()).filter(Boolean);
  const wafObs = value('wafObs');

  return {
    hostname: value('hostname'),
    ip: value('ip') || null,
    estado: 'Pendiente',
    tipoUso: value('tipoUso') || 'Interno',
    funcion: value('funcion'),
    sistemaOperativo: value('os'),
    requiereLlaveLicencia: checked('requiereLlave'),
    llaveOS: value('llave') || null,
    nucleos: Number(value('nucleos') || 2),
    ram: Number(value('ram') || 8),
    almacenamiento: Number(value('almacenamiento') || 100),
    descripcion: value('descripcion') || null,
    plantillaRecursos: value('plantilla') || 'General',
    etapaOperativa: value('etapaOperativa') || 'Provisionamiento',
    responsableInfraestructura: value('responsableInfra') || null,
    fechaAsignacionIp: value('fechaIp') || null,
    tareasPendientes: value('descripcion') || null,
    observacionesSeguimiento: wafObs || value('descripcion') || null,
    etapaVulnerabilidades: value('vulnerabilidades') || null,
    requiereRevisionAnual: true,
    ultimaRevisionAnual: value('revision') || null,
    comunicacionValidada: checked('comunicacion'),
    fechaValidacionComunicacion: checked('comunicacion') ? new Date().toISOString() : null,
    parchesAplicados: checked('parches'),
    fechaParches: checked('parches') ? new Date().toISOString() : null,
    xdrInstalado: checked('xdr'),
    fechaXdr: checked('xdr') ? new Date().toISOString() : null,
    credencialesEntregadas: checked('credenciales'),
    fechaEntregaCredenciales: checked('credenciales') ? new Date().toISOString() : null,
    wafConfigurado: checked('waf'),
    fechaConfiguracionWaf: checked('waf') ? new Date().toISOString() : null,
    evidenciaValidada: checked('evidenciaValidada'),
    fechaValidacionEvidencia: checked('evidenciaValidada') ? new Date().toISOString() : null,
    solicitudPublicacion: checked('publicacion'),
    fechaPublicacion: checked('publicacion') ? new Date().toISOString() : null,
    vpns: vpnTipos.map(tipo => ({ tipo, fechaExpiracion: vpnExp || null, estado: 'Pendiente' })),
    subdominios: subdominios.map(nombreUrl => ({ nombreUrl, estado: 'Activo' })),
    wafs: wafObs ? [{ estado: checked('waf') ? 'Configurado' : 'Pendiente', observaciones: wafObs, fecha: new Date().toISOString() }] : [],
    evidenciasPruebas: evidencias.map(rutaPdf => ({ rutaPdf, fecha: new Date().toISOString(), estadoValidacion: checked('evidenciaValidada') ? 'Aprobada' : 'Pendiente' }))
  };
}

function slugify(text) {
  return String(text || 'pendiente').toLowerCase().replace(/\s+/g, '-');
}

if (localStorage.getItem('token')) {
  showApp();
} else {
  showLogin();
}

addServerEntryIfMissing();
