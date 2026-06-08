using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SolicitudServidores.Models;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Services.Implementaciones
{
    public class PdfService : IPdfService
    {
        private static readonly string AzulOscuro = "#1a3a5c";
        private static readonly string AzulClaro  = "#2563eb";
        private static readonly string GrisClaro  = "#f1f5f9";
        private static readonly string GrisBorde  = "#cbd5e1";

        public byte[] GenerarSolicitudPdf(Solicitud sol)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(36);
                    page.DefaultTextStyle(t => t.FontSize(9).FontFamily("Arial"));

                    page.Header().Element(c => Encabezado(c, sol));
                    page.Content().Column(col =>
                    {
                        col.Spacing(6);
                        SeccionI(col, sol);
                        SeccionII(col, sol);
                        SeccionIII(col, sol);
                        SeccionIV(col, sol);
                        SeccionV(col, sol);
                        SeccionVI(col, sol);
                    });
                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();
        }

        // ── Encabezado ────────────────────────────────────────────────────────

        private static void Encabezado(IContainer c, Solicitud sol)
        {
            c.BorderBottom(2).BorderColor(AzulOscuro).PaddingBottom(8).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("SOLICITUD DE SERVICIOS GESTIONADOS DE TI")
                        .Bold().FontSize(13).FontColor(AzulOscuro);
                    col.Item().Text("Dirección de Tecnologías de la Información")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                });
                row.ConstantItem(160).Column(col =>
                {
                    col.Item().AlignRight().Text($"Folio: {sol.Folio}")
                        .Bold().FontSize(9).FontColor(AzulClaro);
                    col.Item().AlignRight().Text($"Fecha: {sol.CreatedAt:dd/MM/yyyy}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignRight().Text($"Estatus: {sol.Estatus.ToUpper()}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        }

        // ── Secciones ─────────────────────────────────────────────────────────

        private static void SeccionI(ColumnDescriptor col, Solicitud sol)
        {
            var dep = sol.Dependency;
            col.Item().Element(c => TituloSeccion(c, "I. Área requirente"));
            col.Item().Element(c => GridCampos(c, new[]
            {
                ("Sector",       dep?.Sector      ?? "—"),
                ("Dependencia",  dep?.Name        ?? "—"),
                ("Responsable",  dep?.Responsable ?? "—"),
                ("Cargo",        dep?.Cargo       ?? "—"),
                ("Teléfono",     dep?.Phone       ?? "—"),
                ("Correo",       dep?.Email       ?? "—"),
            }));
        }

        private static void SeccionII(ColumnDescriptor col, Solicitud sol)
        {
            var adm = sol.AdminContact;
            col.Item().Element(c => TituloSeccion(c, "II. Administrador del servidor"));
            col.Item().Element(c => GridCampos(c, new[]
            {
                ("Proveedor",   adm?.Proveedor     ?? "—"),
                ("Dependencia", adm?.Dependency?.Name ?? "—"),
                ("Responsable", adm?.AdminServidor  ?? "—"),
                ("Cargo",       adm?.Cargo          ?? "—"),
                ("Teléfono",    adm?.Phone          ?? "—"),
                ("Correo",      adm?.Email          ?? "—"),
            }));
        }

        private static void SeccionIII(ColumnDescriptor col, Solicitud sol)
        {
            col.Item().Element(c => TituloSeccion(c, "III. Descripción del servidor"));
            col.Item().Element(c => GridCampos(c, new[]
            {
                ("Descripción",             sol.DescripcionUso),
                ("Nombre del servidor",     sol.NombreServidor),
                ("Nombre de la aplicación", sol.NombreAplicacion ?? "—"),
                ("Fecha de arranque",       sol.FechaArranqueDeseada?.ToString("dd/MM/yyyy") ?? "—"),
                ("Tipo de uso",             sol.TipoUso),
                ("Vigencia",                $"{sol.VigenciaMeses} meses"),
                ("Características especiales", sol.CaracteristicasEspeciales ?? "—"),
            }));
        }

        private static void SeccionIV(ColumnDescriptor col, Solicitud sol)
        {
            col.Item().Element(c => TituloSeccion(c, "IV. Especificaciones técnicas"));

            var modalidad = sol.EsClonacion ? "clonación" : "nuevo";
            var discos = sol.Servidor?.Storages.Select((d, i) =>
                ($"Disco {i + 1}", $"{d.StorageCapacity} GB — {d.Type ?? "HDD"}{(string.IsNullOrWhiteSpace(d.Description) ? "" : $" ({d.Description})")}"))
                .ToArray() ?? Array.Empty<(string, string)>();

            var campos = new List<(string, string)>
            {
                ("Tipo de requerimiento", sol.TipoRequerimiento),
                ("Modalidad",            modalidad),
                ("Sistema operativo",    sol.SistemaOperativo ?? sol.Servidor?.SistemaOperativo ?? "—"),
                ("vCores",               (sol.Servidor?.Nucleos ?? sol.VcpuSolicitado).ToString()),
                ("Memoria RAM (GB)",     (sol.Servidor?.Ram     ?? sol.RamSolicitadaGb).ToString()),
            };
            campos.AddRange(discos);
            campos.AddRange(new[]
            {
                ("Motor de base de datos",   sol.MotorBaseDatos        ?? "—"),
                ("Puertos",                  sol.ReglasFirewall        ?? "—"),
                ("Integraciones",            sol.IntegracionesExternas ?? "—"),
                ("Otras especificaciones",   sol.ConectividadOtras     ?? "—"),
            });

            col.Item().Element(c => GridCampos(c, campos.ToArray()));
        }

        private static void SeccionV(ColumnDescriptor col, Solicitud sol)
        {
            col.Item().Element(c => TituloSeccion(c, "V. Infraestructura"));

            var subdominios = sol.Servidor?.ServerSubdominios
                .Select((ss, i) => new[]
                {
                    ($"Subdominio {i + 1}", ss.Subdominio?.RequestedName ?? "—"),
                    ($"Puerto {i + 1}",     ss.Subdominio?.Port?.ToString() ?? "—"),
                })
                .SelectMany(x => x)
                .ToArray();

            if (subdominios == null || subdominios.Length == 0)
                subdominios = new[] { ("Subdominios", "Sin subdominios registrados") };

            col.Item().Element(c => GridCampos(c, subdominios));
        }

        private static void SeccionVI(ColumnDescriptor col, Solicitud sol)
        {
            var carta = sol.Carta;
            col.Item().Element(c => TituloSeccion(c, "VI. Responsiva"));
            col.Item().Element(c => GridCampos(c, new[]
            {
                ("Firmante",            carta?.FirmanteDependenciaNombre  ?? "—"),
                ("Número de empleado",  carta?.FirmanteDependenciaEmpleado ?? "—"),
                ("Puesto",              carta?.FirmanteDependenciaPuesto   ?? "—"),
                ("Acepta términos",     carta?.FirmaDependenciaAt != null ? "Sí" : "No"),
                ("Fecha de firma",      carta?.FirmaDependenciaAt?.ToString("dd/MM/yyyy") ?? "—"),
            }));
        }

        // ── Helpers de layout ─────────────────────────────────────────────────

        private static void TituloSeccion(IContainer c, string titulo)
        {
            c.Background(AzulOscuro)
             .Padding(5)
             .Text(titulo)
             .Bold()
             .FontSize(9)
             .FontColor(Colors.White);
        }

        private static void GridCampos(IContainer c, (string Label, string Value)[] campos)
        {
            c.Border(1).BorderColor(GrisBorde).Table(tabla =>
            {
                tabla.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(4);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(4);
                });

                for (int i = 0; i < campos.Length; i++)
                {
                    bool esUltimoPar = (i == campos.Length - 1) && (campos.Length % 2 != 0);

                    tabla.Cell().Background(GrisClaro).BorderBottom(1).BorderColor(GrisBorde)
                         .PaddingHorizontal(5).PaddingVertical(3)
                         .Text(campos[i].Label).Bold().FontSize(8);

                    if (esUltimoPar)
                    {
                        tabla.Cell().ColumnSpan(3).BorderBottom(1).BorderColor(GrisBorde)
                             .PaddingHorizontal(5).PaddingVertical(3)
                             .Text(campos[i].Value).FontSize(8);
                    }
                    else
                    {
                        tabla.Cell().BorderBottom(1).BorderColor(GrisBorde)
                             .PaddingHorizontal(5).PaddingVertical(3)
                             .Text(campos[i].Value).FontSize(8);

                        if (i + 1 < campos.Length)
                        {
                            i++;
                            tabla.Cell().Background(GrisClaro).BorderBottom(1).BorderColor(GrisBorde)
                                 .PaddingHorizontal(5).PaddingVertical(3)
                                 .Text(campos[i].Label).Bold().FontSize(8);
                            tabla.Cell().BorderBottom(1).BorderColor(GrisBorde)
                                 .PaddingHorizontal(5).PaddingVertical(3)
                                 .Text(campos[i].Value).FontSize(8);
                        }
                        else
                        {
                            tabla.Cell().BorderBottom(1).BorderColor(GrisBorde).Text("");
                            tabla.Cell().BorderBottom(1).BorderColor(GrisBorde).Text("");
                        }
                    }
                }
            });
        }
    }
}
