using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace ActosAdultosAPI.Services.Implementation
{
    public class InscripcionPdfService : IInscripcionPdfService
    {
        public byte[] GenerarPdf(
            CargoAsignacionPdfDto datos)
        {
            var culture =
                CultureInfo.GetCultureInfo("es-AR");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(30);

                    page.DefaultTextStyle(x =>
                        x.FontSize(9));

                    // HEADER
                    page.Header()
                        .Column(column =>
                        {
                            column.Spacing(3);

                            column.Item()
                                .Text(
                                    "MINISTERIO DE EDUCACIÓN")
                                .FontSize(10)
                                .SemiBold();

                            column.Item()
                                .Text(
                                    "GOBIERNO DE TIERRA DEL FUEGO")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);

                            column.Item()
                                .PaddingTop(10)
                                .Text("ACTOS PÚBLICOS JÓVENES Y ADULTOS")
                                .FontSize(13)
                                .Bold();

                            column.Item()
                                .PaddingTop(8)
                                .Text(
                                    "RESULTADO DE ASIGNACIÓN")
                                .FontSize(16)
                                .Bold();

                            column.Item()
                                .PaddingBottom(8)
                                .LineHorizontal(1)
                                .LineColor(
                                    Colors.Grey.Lighten1);
                        });

                    // CONTENIDO
                    page.Content()
                        .PaddingVertical(10)
                        .Column(column =>
                        {
                            column.Spacing(12);

                            // ACTO
                            column.Item()
                                .Element(c =>
                                    SeccionTitulo(
                                        c,
                                        "DETALLE DEL ACTO"));

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns
                                                .RelativeColumn();

                                            columns
                                                .RelativeColumn();
                                        });

                                    FilaDetalle(
                                        table,
                                        "Número",
                                        datos.Acto.Numero);

                                    FilaDetalle(
                                        table,
                                        "Descripción",
                                        datos.Acto.Descripcion);

                                    FilaDetalle(
                                        table,
                                        "Inicio",
                                        datos.Acto.Inicio
                                            .ToString(
                                                "dd/MM/yyyy HH:mm"));

                                    FilaDetalle(
                                        table,
                                        "Fin",
                                        datos.Acto.Fin
                                            .ToString(
                                                "dd/MM/yyyy HH:mm"));
                                });

                            // CARGO
                            column.Item()
                                .PaddingTop(4)
                                .Element(c =>
                                    SeccionTitulo(
                                        c,
                                        "DETALLE DEL CARGO"));

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns
                                                .RelativeColumn();

                                            columns
                                                .RelativeColumn();
                                        });

                                    FilaDetalle(
                                        table,
                                        "Institución",
                                        datos.Cargo
                                            .InstitucionNombre);

                                    FilaDetalle(
                                        table,
                                        "Ciudad",
                                        datos.Cargo
                                            .CiudadNombre);

                                    FilaDetalle(
                                        table,
                                        "Cargo",
                                        datos.Cargo
                                            .Descripcion);

                                    FilaDetalle(
                                        table,
                                        "Orientación",
                                        datos.Cargo
                                            .OrientacionDescripcion);

                                    FilaDetalle(
                                        table,
                                        "Código",
                                        datos.Cargo.Codigo
                                            .ToString());

                                    FilaDetalle(
                                        table,
                                        "N.º llamado",
                                        datos.Cargo
                                            .NumeroLlamado
                                            .ToString());

                                    FilaDetalle(
                                        table,
                                        "Turno",
                                        FormatearTurno(
                                            datos.Cargo.Turno));

                                    FilaDetalle(
                                        table,
                                        "Revista",
                                        FormatearRevista(
                                            datos.Cargo.Revista));

                                    FilaDetalle(
                                        table,
                                        "Curso / División",
                                        $"{datos.Cargo.Curso}° - " +
                                        $"{datos.Cargo.Division}");

                                    FilaDetalle(
                                        table,
                                        "Plaza SUNA",
                                        datos.Cargo.PlazaSUNA);

                                    FilaDetalle(
                                        table,
                                        "Carga horaria",
                                        datos.Cargo
                                            .CargaHoraria);

                                    FilaDetalle(
                                        table,
                                        "Código Junta",
                                        datos.Cargo
                                            .CodigoJunta);
                                });

                            // HORARIOS
                            if (datos.Cargo.Horarios.Count > 0)
                            {
                                column.Item()
                                    .PaddingTop(3)
                                    .Text("Horarios")
                                    .SemiBold();

                                foreach (
                                    var horario in
                                    datos.Cargo.Horarios)
                                {
                                    column.Item()
                                        .Text(
                                            $"{FormatearDia(horario.DiaSemana)} " +
                                            $"{horario.HoraInicio:HH\\:mm} - " +
                                            $"{horario.HoraFin:HH\\:mm}");
                                }
                            }

                            // INSCRIPTOS
                            column.Item()
                                .PaddingTop(6)
                                .Element(c =>
                                    SeccionTitulo(
                                        c,
                                        "LISTADO DE INSCRIPTOS"));

                            column.Item()
                                .Text(
                                    $"Total de inscriptos: " +
                                    $"{datos.Inscriptos.Count}")
                                .FontSize(8)
                                .FontColor(
                                    Colors.Grey.Darken1);

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns
                                                .ConstantColumn(65);

                                            columns
                                                .RelativeColumn(2);

                                            columns
                                                .RelativeColumn(2);

                                            columns
                                                .RelativeColumn(1.3f);

                                            columns
                                                .ConstantColumn(60);

                                            columns
                                                .ConstantColumn(70);
                                        });

                                    // HEADER
                                    table.Header(header =>
                                    {
                                        HeaderCelda(
                                            header,
                                            "DNI");

                                        HeaderCelda(
                                            header,
                                            "Apellido");

                                        HeaderCelda(
                                            header,
                                            "Nombre");

                                        HeaderCelda(
                                            header,
                                            "Carácter");

                                        HeaderCelda(
                                            header,
                                            "Puntaje");

                                        HeaderCelda(
                                            header,
                                            "Estado");
                                    });

                                    foreach (
                                        var inscripto in
                                        datos.Inscriptos)
                                    {
                                        var fondo =
                                            inscripto.EsAsignado
                                                ? Colors.Green.Lighten4
                                                : Colors.White;

                                        Celda(
                                            table,
                                            inscripto.Dni
                                                .ToString(),
                                            fondo,
                                            inscripto
                                                .EsAsignado);

                                        Celda(
                                            table,
                                            inscripto.Apellidos,
                                            fondo,
                                            inscripto
                                                .EsAsignado);

                                        Celda(
                                            table,
                                            inscripto.Nombres,
                                            fondo,
                                            inscripto
                                                .EsAsignado);

                                        Celda(
                                            table,
                                            inscripto.Caracter?
                                                .ToString()
                                                ?? "-",
                                            fondo,
                                            inscripto
                                                .EsAsignado);

                                        Celda(
                                            table,
                                            inscripto.PuntajeJunta
                                                ?.ToString(
                                                    "0.##",
                                                    culture)
                                                ?? "-",
                                            fondo,
                                            inscripto
                                                .EsAsignado);

                                        Celda(
                                            table,
                                            inscripto.EsAsignado
                                                ? "ASIGNADO"
                                                : "Inscripto",
                                            fondo,
                                            inscripto
                                                .EsAsignado);
                                    }
                                });
                        });

                    // FOOTER
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.DefaultTextStyle(
                                x => x
                                    .FontSize(8)
                                    .FontColor(
                                        Colors.Grey.Darken1));

                            text.Span("Página ");

                            text.CurrentPageNumber();

                            text.Span(" de ");

                            text.TotalPages();
                        });
                });
            })
            .GeneratePdf();
        }

        private static void SeccionTitulo(
            IContainer container,
            string titulo)
        {
            container
                .Background(Colors.Grey.Lighten3)
                .PaddingVertical(6)
                .PaddingHorizontal(8)
                .Text(titulo)
                .FontSize(10)
                .SemiBold();
        }

        private static void FilaDetalle(
            TableDescriptor table,
            string titulo,
            string? valor)
        {
            table.Cell()
                .BorderBottom(1)
                .BorderColor(
                    Colors.Grey.Lighten2)
                .Padding(5)
                .Text(titulo)
                .SemiBold();

            table.Cell()
                .BorderBottom(1)
                .BorderColor(
                    Colors.Grey.Lighten2)
                .Padding(5)
                .Text(
                    string.IsNullOrWhiteSpace(valor)
                        ? "-"
                        : valor);
        }

        private static void HeaderCelda(
            TableCellDescriptor header,
            string texto)
        {
            header.Cell()
                .Background(Colors.Grey.Darken2)
                .Padding(5)
                .Text(texto)
                .FontColor(Colors.White)
                .SemiBold();
        }

        private static void Celda(
            TableDescriptor table,
            string texto,
            string fondo,
            bool destacado)
        {
            var celda = table.Cell()
                .Background(fondo)
                .BorderBottom(1)
                .BorderColor(
                    Colors.Grey.Lighten2)
                .Padding(5);

            if (destacado)
            {
                celda
                    .Text(texto)
                    .Bold();
            }
            else
            {
                celda
                    .Text(texto);
            }
        }

        private static string FormatearDia(
            DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => dia.ToString()
            };
        }

        private static string FormatearTurno(
            Turno turno)
        {
            return turno switch
            {
                Turno.MAÑANA => "Mañana",
                Turno.TARDE => "Tarde",
                Turno.VESPERTINO => "Vespertino",
                _ => turno.ToString()
            };
        }

        private static string FormatearRevista(
            Revista revista)
        {
            return revista switch
            {
                Revista.TITULAR => "Titular",
                Revista.INTERINO => "Interino",
                Revista.SUPLENTE => "Suplente",
                _ => revista.ToString()
            };
        }
    }
}
