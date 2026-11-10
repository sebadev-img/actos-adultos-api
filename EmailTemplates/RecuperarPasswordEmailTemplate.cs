using System.Text.Encodings.Web;

namespace ActosAdultosAPI.EmailTemplates
{
    public static class RecuperarPasswordEmailTemplate
    {
        public static string Generar(
            string resetUrl,
            string logoUrl)
        {
            var safeResetUrl =
                HtmlEncoder.Default.Encode(resetUrl);

            var safeLogoUrl =
                HtmlEncoder.Default.Encode(logoUrl);

            return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport"
                      content="width=device-width, initial-scale=1.0">

                <title>Restablecer contraseña</title>
            </head>

            <body style="
                margin: 0;
                padding: 0;
                background-color: #f3f4f6;
                font-family: Arial, Helvetica, sans-serif;
                color: #1f2937;
            ">

                <table
                    role="presentation"
                    width="100%"
                    cellspacing="0"
                    cellpadding="0"
                    border="0"
                    style="background-color: #f3f4f6;"
                >
                    <tr>
                        <td
                            align="center"
                            style="padding: 40px 16px;"
                        >

                            <table
                                role="presentation"
                                width="100%"
                                cellspacing="0"
                                cellpadding="0"
                                border="0"
                                style="
                                    max-width: 620px;
                                    background-color: #ffffff;
                                    border-radius: 12px;
                                    overflow: hidden;
                                    border: 1px solid #e5e7eb;
                                "
                            >

                                <!-- HEADER -->
                                <tr>
                                    <td
                                        align="center"
                                        style="
                                            padding: 32px 30px 24px;
                                        "
                                    >

                                        <img
                                            src="{safeLogoUrl}"
                                            alt="Gobierno de Tierra del Fuego"
                                            width="250"
                                            style="
                                                display: block;
                                                width: 100%;
                                                max-width: 250px;
                                                height: auto;
                                                margin: 0 auto 24px;
                                                border: 0;
                                            "
                                        >

                                        <div style="
                                            font-size: 15px;
                                            font-weight: 600;
                                            color: #4b5563;
                                            margin-bottom: 8px;
                                        ">
                                            ACTOS PÚBLICOS
                                        </div>

                                        <div style="
                                            font-size: 24px;
                                            font-weight: 700;
                                            color: #111827;
                                        ">
                                            Jóvenes y Adultos
                                        </div>

                                    </td>
                                </tr>

                                <!-- CONTENIDO -->
                                <tr>
                                    <td style="
                                        padding: 10px 40px 36px;
                                    ">

                                        <h1 style="
                                            margin: 0 0 20px;
                                            font-size: 22px;
                                            text-align: center;
                                            color: #111827;
                                        ">
                                            Restablecer contraseña
                                        </h1>

                                        <p style="
                                            margin: 0 0 16px;
                                            font-size: 15px;
                                            line-height: 1.7;
                                            color: #374151;
                                        ">
                                            Recibimos una solicitud para
                                            restablecer la contraseña de tu
                                            cuenta de
                                            <strong>
                                                Actos Públicos Jóvenes y Adultos
                                            </strong>.
                                        </p>

                                        <p style="
                                            margin: 0 0 24px;
                                            font-size: 15px;
                                            line-height: 1.7;
                                            color: #374151;
                                        ">
                                            Para elegir una nueva contraseña,
                                            hacé clic en el siguiente botón.
                                        </p>

                                        <table
                                            role="presentation"
                                            width="100%"
                                            cellspacing="0"
                                            cellpadding="0"
                                            border="0"
                                        >
                                            <tr>
                                                <td
                                                    align="center"
                                                    style="
                                                        padding: 8px 0 28px;
                                                    "
                                                >
                                                    <a
                                                        href="{safeResetUrl}"
                                                        style="
                                                            display: inline-block;
                                                            padding: 14px 26px;
                                                            background-color: #0b6fa4;
                                                            color: #ffffff;
                                                            text-decoration: none;
                                                            font-size: 15px;
                                                            font-weight: 600;
                                                            border-radius: 7px;
                                                        "
                                                    >
                                                        Restablecer contraseña
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <p style="
                                            margin: 0 0 10px;
                                            font-size: 14px;
                                            line-height: 1.6;
                                            color: #6b7280;
                                        ">
                                            Si el botón no funciona, copiá y
                                            pegá el siguiente enlace en tu
                                            navegador:
                                        </p>

                                        <p style="
                                            margin: 0;
                                            padding: 12px;
                                            background-color: #f9fafb;
                                            border: 1px solid #e5e7eb;
                                            border-radius: 6px;
                                            font-size: 12px;
                                            line-height: 1.5;
                                            word-break: break-all;
                                        ">
                                            <a
                                                href="{safeResetUrl}"
                                                style="
                                                    color: #0b6fa4;
                                                    text-decoration: none;
                                                "
                                            >
                                                {safeResetUrl}
                                            </a>
                                        </p>

                                        <div style="
                                            margin-top: 28px;
                                            padding-top: 20px;
                                            border-top: 1px solid #e5e7eb;
                                        ">
                                            <p style="
                                                margin: 0;
                                                font-size: 13px;
                                                line-height: 1.6;
                                                color: #6b7280;
                                            ">
                                                Si no solicitaste un cambio
                                                de contraseña, ignorá este
                                                correo. Tu contraseña actual
                                                seguirá siendo válida.
                                            </p>
                                        </div>

                                    </td>
                                </tr>

                                <!-- FOOTER -->
                                <tr>
                                    <td
                                        align="center"
                                        style="
                                            padding: 24px 30px;
                                            background-color: #f8fafc;
                                            border-top: 1px solid #e5e7eb;
                                        "
                                    >

                                        <p style="
                                            margin: 0 0 6px;
                                            font-size: 13px;
                                            font-weight: 600;
                                            color: #374151;
                                        ">
                                            Ministerio de Educación
                                        </p>

                                        <p style="
                                            margin: 0;
                                            font-size: 12px;
                                            color: #6b7280;
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
