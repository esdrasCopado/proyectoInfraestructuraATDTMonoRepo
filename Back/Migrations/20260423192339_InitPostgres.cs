using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SolicitudServidores.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "dependency",
                schema: "public",
                columns: table => new
                {
                    dependency_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sector = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    responsable = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dependency", x => x.dependency_id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "public",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "subdominio",
                schema: "public",
                columns: table => new
                {
                    subdominio_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    requested_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    approved_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    port = table.Column<int>(type: "integer", nullable: true),
                    ssl_required = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "solicitado"),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subdominio", x => x.subdominio_id);
                });

            migrationBuilder.CreateTable(
                name: "vpn",
                schema: "public",
                columns: table => new
                {
                    vpn_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vpn_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsable = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    vpn_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    external_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    empresa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    vigencia_dias = table.Column<int>(type: "integer", nullable: true),
                    perfil_anterior = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vpn", x => x.vpn_id);
                });

            migrationBuilder.CreateTable(
                name: "admin_dep_contact_information",
                schema: "public",
                columns: table => new
                {
                    admin_contact_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dependency_id = table.Column<int>(type: "integer", nullable: false),
                    proveedor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    admin_servidor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_dep_contact_information", x => x.admin_contact_id);
                    table.ForeignKey(
                        name: "FK_admin_dep_contact_information_dependency_dependency_id",
                        column: x => x.dependency_id,
                        principalSchema: "public",
                        principalTable: "dependency",
                        principalColumn: "dependency_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "servidor",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dependency_id = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    expiracion = table.Column<DateTime>(type: "date", nullable: true),
                    hostname = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ip = table.Column<string>(type: "text", nullable: true),
                    tipo_uso = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    funcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    sistemaOperativo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    requiere_llave_licencia = table.Column<bool>(type: "boolean", nullable: false),
                    llaveOS = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    nucleos = table.Column<int>(type: "integer", nullable: false),
                    ram = table.Column<int>(type: "integer", nullable: false),
                    almacenamiento = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    plantilla_recursos = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    etapa_operativa = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    responsable_infraestructura = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    usuario_ultima_actualizacion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    fecha_ultima_actualizacion = table.Column<DateTime>(type: "date", nullable: true),
                    fecha_asignacion_ip = table.Column<DateTime>(type: "date", nullable: true),
                    tareas_pendientes = table.Column<string>(type: "text", nullable: true),
                    observaciones_seguimiento = table.Column<string>(type: "text", nullable: true),
                    etapa_vulnerabilidades = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    requiere_revision_anual = table.Column<bool>(type: "boolean", nullable: false),
                    ultima_revision_anual = table.Column<DateTime>(type: "date", nullable: true),
                    comunicacion_validada = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_validacion_comunicacion = table.Column<DateTime>(type: "date", nullable: true),
                    usuario_validacion_comunicacion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    parches_aplicados = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_parches = table.Column<DateTime>(type: "date", nullable: true),
                    usuario_parches = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    xdr_instalado = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_xdr = table.Column<DateTime>(type: "date", nullable: true),
                    usuario_xdr = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    credenciales_entregadas = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_entrega_credenciales = table.Column<DateTime>(type: "date", nullable: true),
                    usuario_credenciales = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    waf_configurado = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_configuracion_waf = table.Column<DateTime>(type: "date", nullable: true),
                    usuario_waf = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    evidencia_validada = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_validacion_evidencia = table.Column<DateTime>(type: "date", nullable: true),
                    usuario_validacion_evidencia = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    solicitud_publicacion = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "date", nullable: true),
                    usuario_publicacion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    fecha_vulnerabilidades = table.Column<DateTime>(type: "date", nullable: true),
                    usuario_vulnerabilidades = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servidor", x => x.id);
                    table.ForeignKey(
                        name: "FK_servidor_dependency_dependency_id",
                        column: x => x.dependency_id,
                        principalSchema: "public",
                        principalTable: "dependency",
                        principalColumn: "dependency_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    dependency_id = table.Column<int>(type: "integer", nullable: true),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    numero_empleado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_users_dependency_dependency_id",
                        column: x => x.dependency_id,
                        principalSchema: "public",
                        principalTable: "dependency",
                        principalColumn: "dependency_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_users_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "server_subdominio",
                schema: "public",
                columns: table => new
                {
                    server_subdominio_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subdominio_id = table.Column<int>(type: "integer", nullable: false),
                    server_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_server_subdominio", x => x.server_subdominio_id);
                    table.ForeignKey(
                        name: "FK_server_subdominio_servidor_server_id",
                        column: x => x.server_id,
                        principalSchema: "public",
                        principalTable: "servidor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_server_subdominio_subdominio_subdominio_id",
                        column: x => x.subdominio_id,
                        principalSchema: "public",
                        principalTable: "subdominio",
                        principalColumn: "subdominio_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "server_vpn",
                schema: "public",
                columns: table => new
                {
                    server_vpn_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vpn_id = table.Column<int>(type: "integer", nullable: false),
                    server_id = table.Column<long>(type: "bigint", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_server_vpn", x => x.server_vpn_id);
                    table.ForeignKey(
                        name: "FK_server_vpn_servidor_server_id",
                        column: x => x.server_id,
                        principalSchema: "public",
                        principalTable: "servidor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_server_vpn_vpn_vpn_id",
                        column: x => x.vpn_id,
                        principalSchema: "public",
                        principalTable: "vpn",
                        principalColumn: "vpn_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage",
                schema: "public",
                columns: table => new
                {
                    storage_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    server_id = table.Column<long>(type: "bigint", nullable: false),
                    storage_capacity = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage", x => x.storage_id);
                    table.ForeignKey(
                        name: "FK_storage_servidor_server_id",
                        column: x => x.server_id,
                        principalSchema: "public",
                        principalTable: "servidor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "public",
                columns: table => new
                {
                    notification_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recipient_user_id = table.Column<long>(type: "bigint", nullable: false),
                    sender_user_id = table.Column<long>(type: "bigint", nullable: true),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    entity_id = table.Column<long>(type: "bigint", nullable: true),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: false),
                    leida = table.Column<bool>(type: "boolean", nullable: false),
                    leida_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_notifications_users_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notifications_users_sender_user_id",
                        column: x => x.sender_user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "solicitud",
                schema: "public",
                columns: table => new
                {
                    solicitud_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    folio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dependency_id = table.Column<int>(type: "integer", nullable: false),
                    admin_contact_id = table.Column<int>(type: "integer", nullable: true),
                    server_id = table.Column<long>(type: "bigint", nullable: true),
                    descripcion_uso = table.Column<string>(type: "text", nullable: false),
                    nombre_servidor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nombre_aplicacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tipo_uso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_arranque_deseada = table.Column<DateTime>(type: "date", nullable: true),
                    vigencia_meses = table.Column<int>(type: "integer", nullable: false),
                    caracteristicas_especiales = table.Column<string>(type: "text", nullable: true),
                    tipo_requerimiento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    es_clonacion = table.Column<bool>(type: "boolean", nullable: false),
                    ip_servidor_base = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    nombre_servidor_base = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sistema_operativo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ram_solicitada_gb = table.Column<int>(type: "integer", nullable: false),
                    vcpu_solicitado = table.Column<int>(type: "integer", nullable: false),
                    almacenamiento_solicitado_gb = table.Column<int>(type: "integer", nullable: false),
                    motor_base_datos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reglas_firewall = table.Column<string>(type: "text", nullable: true),
                    integraciones_externas = table.Column<string>(type: "text", nullable: true),
                    conectividad_otras = table.Column<string>(type: "text", nullable: true),
                    estatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "pendiente"),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitud", x => x.solicitud_id);
                    table.ForeignKey(
                        name: "FK_solicitud_admin_dep_contact_information_admin_contact_id",
                        column: x => x.admin_contact_id,
                        principalSchema: "public",
                        principalTable: "admin_dep_contact_information",
                        principalColumn: "admin_contact_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_solicitud_dependency_dependency_id",
                        column: x => x.dependency_id,
                        principalSchema: "public",
                        principalTable: "dependency",
                        principalColumn: "dependency_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_solicitud_servidor_server_id",
                        column: x => x.server_id,
                        principalSchema: "public",
                        principalTable: "servidor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_solicitud_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "waf_config",
                schema: "public",
                columns: table => new
                {
                    waf_config_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    server_id = table.Column<long>(type: "bigint", nullable: false),
                    configurado = table.Column<bool>(type: "boolean", nullable: false),
                    reglas_aplicadas = table.Column<string>(type: "text", nullable: true),
                    configured_by = table.Column<long>(type: "bigint", nullable: true),
                    configured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waf_config", x => x.waf_config_id);
                    table.ForeignKey(
                        name: "FK_waf_config_servidor_server_id",
                        column: x => x.server_id,
                        principalSchema: "public",
                        principalTable: "servidor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_waf_config_users_configured_by",
                        column: x => x.configured_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "analisis_vulnerabilidades",
                schema: "public",
                columns: table => new
                {
                    analisis_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    solicitud_id = table.Column<long>(type: "bigint", nullable: false),
                    ronda = table.Column<int>(type: "integer", nullable: false),
                    solicitud_publicacion_by = table.Column<long>(type: "bigint", nullable: true),
                    solicitud_publicacion_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pendiente"),
                    hallazgos = table.Column<string>(type: "text", nullable: true),
                    recomendaciones = table.Column<string>(type: "text", nullable: true),
                    analyzed_by = table.Column<long>(type: "bigint", nullable: true),
                    analyzed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    publicado_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    publicado_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analisis_vulnerabilidades", x => x.analisis_id);
                    table.ForeignKey(
                        name: "FK_analisis_vulnerabilidades_solicitud_solicitud_id",
                        column: x => x.solicitud_id,
                        principalSchema: "public",
                        principalTable: "solicitud",
                        principalColumn: "solicitud_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_analisis_vulnerabilidades_users_analyzed_by",
                        column: x => x.analyzed_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_analisis_vulnerabilidades_users_publicado_by",
                        column: x => x.publicado_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_analisis_vulnerabilidades_users_solicitud_publicacion_by",
                        column: x => x.solicitud_publicacion_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "carta",
                schema: "public",
                columns: table => new
                {
                    carta_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    solicitud_id = table.Column<long>(type: "bigint", nullable: false),
                    folio_carta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    firmante_dependencia_nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    firmante_dependencia_puesto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    firmante_dependencia_empleado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    firma_dependencia_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    firmante_atdt_nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    firmante_atdt_puesto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    firma_atdt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pdf_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    pdf_generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carta", x => x.carta_id);
                    table.ForeignKey(
                        name: "FK_carta_solicitud_solicitud_id",
                        column: x => x.solicitud_id,
                        principalSchema: "public",
                        principalTable: "solicitud",
                        principalColumn: "solicitud_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_carta_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evidencia",
                schema: "public",
                columns: table => new
                {
                    evidencia_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    solicitud_id = table.Column<long>(type: "bigint", nullable: false),
                    proposito = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ronda = table.Column<int>(type: "integer", nullable: false),
                    archivo_nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    archivo_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    archivo_size_kb = table.Column<int>(type: "integer", nullable: true),
                    uploaded_by = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado_validacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pendiente"),
                    motivo_rechazo = table.Column<string>(type: "text", nullable: true),
                    validated_by = table.Column<long>(type: "bigint", nullable: true),
                    validated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidencia", x => x.evidencia_id);
                    table.ForeignKey(
                        name: "FK_evidencia_solicitud_solicitud_id",
                        column: x => x.solicitud_id,
                        principalSchema: "public",
                        principalTable: "solicitud",
                        principalColumn: "solicitud_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_evidencia_users_uploaded_by",
                        column: x => x.uploaded_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_evidencia_users_validated_by",
                        column: x => x.validated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "seguimiento",
                schema: "public",
                columns: table => new
                {
                    seguimiento_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    solicitud_id = table.Column<long>(type: "bigint", nullable: false),
                    etapa_numero = table.Column<int>(type: "integer", nullable: false),
                    etapa_nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pendiente"),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_completado = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completado_by = table.Column<long>(type: "bigint", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seguimiento", x => x.seguimiento_id);
                    table.ForeignKey(
                        name: "FK_seguimiento_solicitud_solicitud_id",
                        column: x => x.solicitud_id,
                        principalSchema: "public",
                        principalTable: "solicitud",
                        principalColumn: "solicitud_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_seguimiento_users_completado_by",
                        column: x => x.completado_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_dep_contact_information_dependency_id",
                schema: "public",
                table: "admin_dep_contact_information",
                column: "dependency_id");

            migrationBuilder.CreateIndex(
                name: "IX_analisis_vulnerabilidades_analyzed_by",
                schema: "public",
                table: "analisis_vulnerabilidades",
                column: "analyzed_by");

            migrationBuilder.CreateIndex(
                name: "IX_analisis_vulnerabilidades_publicado_by",
                schema: "public",
                table: "analisis_vulnerabilidades",
                column: "publicado_by");

            migrationBuilder.CreateIndex(
                name: "IX_analisis_vulnerabilidades_solicitud_id_ronda",
                schema: "public",
                table: "analisis_vulnerabilidades",
                columns: new[] { "solicitud_id", "ronda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_analisis_vulnerabilidades_solicitud_publicacion_by",
                schema: "public",
                table: "analisis_vulnerabilidades",
                column: "solicitud_publicacion_by");

            migrationBuilder.CreateIndex(
                name: "IX_carta_created_by",
                schema: "public",
                table: "carta",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_carta_solicitud_id",
                schema: "public",
                table: "carta",
                column: "solicitud_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_evidencia_solicitud_id",
                schema: "public",
                table: "evidencia",
                column: "solicitud_id");

            migrationBuilder.CreateIndex(
                name: "IX_evidencia_uploaded_by",
                schema: "public",
                table: "evidencia",
                column: "uploaded_by");

            migrationBuilder.CreateIndex(
                name: "IX_evidencia_validated_by",
                schema: "public",
                table: "evidencia",
                column: "validated_by");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_created_at",
                schema: "public",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_user_id",
                schema: "public",
                table: "notifications",
                column: "recipient_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_recipient_user_id_leida",
                schema: "public",
                table: "notifications",
                columns: new[] { "recipient_user_id", "leida" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_sender_user_id",
                schema: "public",
                table: "notifications",
                column: "sender_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_nombre",
                schema: "public",
                table: "roles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seguimiento_completado_by",
                schema: "public",
                table: "seguimiento",
                column: "completado_by");

            migrationBuilder.CreateIndex(
                name: "IX_seguimiento_solicitud_id_etapa_numero",
                schema: "public",
                table: "seguimiento",
                columns: new[] { "solicitud_id", "etapa_numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_server_subdominio_server_id",
                schema: "public",
                table: "server_subdominio",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "IX_server_subdominio_subdominio_id_server_id",
                schema: "public",
                table: "server_subdominio",
                columns: new[] { "subdominio_id", "server_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_server_vpn_server_id",
                schema: "public",
                table: "server_vpn",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "IX_server_vpn_vpn_id_server_id",
                schema: "public",
                table: "server_vpn",
                columns: new[] { "vpn_id", "server_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_servidor_dependency_id",
                schema: "public",
                table: "servidor",
                column: "dependency_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_admin_contact_id",
                schema: "public",
                table: "solicitud",
                column: "admin_contact_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_created_at",
                schema: "public",
                table: "solicitud",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_created_by",
                schema: "public",
                table: "solicitud",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_dependency_id",
                schema: "public",
                table: "solicitud",
                column: "dependency_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_estatus",
                schema: "public",
                table: "solicitud",
                column: "estatus");

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_folio",
                schema: "public",
                table: "solicitud",
                column: "folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_solicitud_server_id",
                schema: "public",
                table: "solicitud",
                column: "server_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_storage_server_id",
                schema: "public",
                table: "storage",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_created_by",
                schema: "public",
                table: "users",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_users_dependency_id",
                schema: "public",
                table: "users",
                column: "dependency_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "public",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                schema: "public",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_updated_by",
                schema: "public",
                table: "users",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_waf_config_configured_by",
                schema: "public",
                table: "waf_config",
                column: "configured_by");

            migrationBuilder.CreateIndex(
                name: "IX_waf_config_server_id",
                schema: "public",
                table: "waf_config",
                column: "server_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analisis_vulnerabilidades",
                schema: "public");

            migrationBuilder.DropTable(
                name: "carta",
                schema: "public");

            migrationBuilder.DropTable(
                name: "evidencia",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "seguimiento",
                schema: "public");

            migrationBuilder.DropTable(
                name: "server_subdominio",
                schema: "public");

            migrationBuilder.DropTable(
                name: "server_vpn",
                schema: "public");

            migrationBuilder.DropTable(
                name: "storage",
                schema: "public");

            migrationBuilder.DropTable(
                name: "waf_config",
                schema: "public");

            migrationBuilder.DropTable(
                name: "solicitud",
                schema: "public");

            migrationBuilder.DropTable(
                name: "subdominio",
                schema: "public");

            migrationBuilder.DropTable(
                name: "vpn",
                schema: "public");

            migrationBuilder.DropTable(
                name: "admin_dep_contact_information",
                schema: "public");

            migrationBuilder.DropTable(
                name: "servidor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "dependency",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");
        }
    }
}
