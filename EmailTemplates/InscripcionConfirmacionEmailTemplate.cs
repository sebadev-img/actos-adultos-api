using System.Net;

namespace ActosAdultosAPI.EmailTemplates
{
    public static class InscripcionConfirmacionEmailTemplate
    {
        public static string Generar(
            string logoUrl,
            string nombre,
            string cargo,
            string institucion,
            string acto,
            int numeroLlamado,
            int curso,
            string division,
            string turno,
            string revista,
            decimal? puntaje,
            string? caracter)
        {
            var safeLogoUrl = WebUtility.HtmlEncode(logoUrl);
            var safeNombre = WebUtility.HtmlEncode(nombre);
            var safeCargo = WebUtility.HtmlEncode(cargo);
            var safeInstitucion = WebUtility.HtmlEncode(institucion);
            var safeActo = WebUtility.HtmlEncode(acto);
            var safeDivision = WebUtility.HtmlEncode(division);
            var safeTurno = WebUtility.HtmlEncode(turno);
            var safeRevista = WebUtility.HtmlEncode(revista);
            var safeCaracter = WebUtility.HtmlEncode(
                string.IsNullOrWhiteSpace(caracter)
                    ? "Sin datos"
                    : caracter
            );

            var puntajeTexto =
                puntaje.HasValue
                    ? puntaje.Value.ToString("0.##")
                    : "Sin datos";

            return $"""
            <!DOCTYPE html>
            <html lang="es">

            <head>
                <meta charset="UTF-8">
                <meta name="viewport"
                      content="width=device-width, initial-scale=1.0">

                <title>Confirmación de inscripción</title>
            </head>

            <body style="
                margin:0;
                padding:0;
                background:#f3f4f6;
                font-family:Arial, Helvetica, sans-serif;
                color:#1f2937;
            ">

                <table
                    width="100%"
                    cellspacing="0"
                    cellpadding="0"
                    border="0"
                    role="presentation"
                >
                    <tr>
                        <td
                            align="center"
                            style="padding:40px 16px;"
                        >

                            <table
                                width="100%"
                                cellspacing="0"
                                cellpadding="0"
                                border="0"
                                role="presentation"
                                style="
                                    max-width:620px;
                                    background:#ffffff;
                                    border:1px solid #e5e7eb;
                                    border-radius:12px;
                                    overflow:hidden;
                                "
                            >

                                <!-- HEADER -->
                                <tr>
                                    <td
                                        align="center"
                                        style="padding:32px 30px 24px;"
                                    >

                                        <img
                                            src="{safeLogoUrl}"
                                            alt="Gobierno de Tierra del Fuego"
                                            width="250"
                                            style="
                                                width:100%;
                                                max-width:250px;
                                                height:auto;
                                                display:block;
                                                margin:0 auto 24px;
                                            "
                                        >

                                        <div style="
                                            font-size:15px;
                                            font-weight:600;
                                            color:#4b5563;
                                            margin-bottom:8px;
                                        ">
                                            ACTOS PÚBLICOS
                                        </div>

                                        <div style="
                                            font-size:24px;
                                            font-weight:700;
                                            color:#111827;
                                        ">
                                            Jóvenes y Adultos
                                        </div>

                                    </td>
                                </tr>

                                <!-- CUERPO -->
                                <tr>
                                    <td style="padding:10px 40px 36px;">

                                        <h1 style="
                                            margin:0 0 20px;
                                            font-size:22px;
                                            text-align:center;
                                            color:#111827;
                                        ">
                                            Inscripción realizada correctamente
                                        </h1>

                                        <p style="
                                            font-size:15px;
                                            line-height:1.7;
                                            color:#374151;
                                        ">
                                            Hola <strong>{safeNombre}</strong>,
                                        </p>

                                        <p style="
                                            font-size:15px;
                                            line-height:1.7;
                                            color:#374151;
                                        ">
                                            Te confirmamos que tu inscripción
                                            fue registrada correctamente en el
                                            sistema de
                                            <strong>
                                                Actos Públicos Jóvenes y Adultos
                                            </strong>.
                                        </p>

                                        <div style="
                                            margin:26px 0;
                                            padding:20px;
                                            background:#f8fafc;
                                            border:1px solid #e5e7eb;
                                            border-radius:8px;
                                        ">

                                            <p style="margin:0 0 12px;">
                                                <strong>Cargo:</strong>
                                                {safeCargo}
                                            </p>

                                            <p style="margin:0 0 12px;">
                                                <strong>Institución:</strong>
                                                {safeInstitucion}
                                            </p>

                                            <p style="margin:0 0 12px;">
                                                <strong>Acto:</strong>
                                                {safeActo}
                                            </p>

                                            <p style="margin:0 0 12px;">
                                                <strong>Número de llamado:</strong>
                                                {numeroLlamado}
                                            </p>

                                            <p style="margin:0 0 12px;">
                                                <strong>Curso / División:</strong>
                                                {curso}° - {safeDivision}
                                            </p>

                                            <p style="margin:0 0 12px;">
                                                <strong>Turno:</strong>
                                                {safeTurno}
                                            </p>

                                            <p style="margin:0 0 12px;">
                                                <strong>Revista:</strong>
                                                {safeRevista}
                                            </p>

                                            <p style="margin:0 0 12px;">
                                                <strong>Puntaje Junta:</strong>
                                                {puntajeTexto}
                                            </p>

                                            <p style="margin:0;">
                                                <strong>Carácter:</strong>
                                                {safeCaracter}
                                            </p>

                                        </div>

                                        <p style="
                                            margin:0;
                                            font-size:13px;
                                            line-height:1.6;
                                            color:#6b7280;
                                        ">
                                            Conservá este correo como
                                            comprobante de tu inscripción.
                                        </p>

                                    </td>
                                </tr>

                                <!-- FOOTER -->
                                <tr>
                                    <td
                                        align="center"
                                        style="
                                            padding:24px 30px;
                                            background:#f8fafc;
                                            border-top:1px solid #e5e7eb;
                                        "
                                    >

                                        <p style="
                                            margin:0 0 6px;
                                            font-size:13px;
                                            font-weight:600;
                                            color:#374151;
                                        ">
                                            Ministerio de Educación
                                        </p>

                                        <p style="
                                            margin:0;
                                            font-size:12px;
                                            color:#6b7280;
                                        ">
                                            Dirección Provincial de Sistemas Administrativos
                                        </p>

                                    </td>
                                </tr>

                            </table>

                        </td>
                    </tr>
                </table>

            </body>
            </html>
            """;
        }
    }
}
