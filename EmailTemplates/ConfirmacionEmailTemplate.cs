using System.Text.Encodings.Web;

namespace ActosAdultosAPI.EmailTemplates
{
    public static class ConfirmacionEmailTemplate
    {
        public static string Generar(
            string confirmationUrl,
            string logoUrl)
        {
            var safeConfirmationUrl =
                HtmlEncoder.Default.Encode(
                    confirmationUrl);

            var safeLogoUrl =
                HtmlEncoder.Default.Encode(
                    logoUrl);

            return $"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport"
                      content="width=device-width, initial-scale=1.0">
                <title>Confirmación de correo electrónico</title>
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
                                            background-color: #ffffff;
                                        "
                                    >

                                        <img
                                            src="{safeLogoUrl}"
                                            alt="Gobierno de Tierra del Fuego"
                                            width="250"
                                            style="
                                                display: block;
                                                max-width: 250px;
                                                width: 100%;
                                                height: auto;
                                                border: 0;
                                                margin: 0 auto 24px;
                                            "
                                        >

                                        <div
                                            style="
                                                font-size: 15px;
                                                font-weight: 600;
                                                color: #4b5563;
                                                margin-bottom: 8px;
                                            "
                                        >
                                            ACTOS PÚBLICOS
                                        </div>

                                        <div
                                            style="
                                                font-size: 24px;
                                                line-height: 1.3;
                                                font-weight: 700;
                                                color: #111827;
                                            "
                                        >
                                            Jóvenes y Adultos
                                        </div>

                                    </td>
                                </tr>

                                <!-- CONTENIDO -->
                                <tr>
                                    <td
                                        style="
                                            padding: 10px 40px 36px;
                                        "
                                    >

                                        <h1
                                            style="
                                                margin: 0 0 20px;
                                                font-size: 22px;
                                                line-height: 1.4;
                                                color: #111827;
                                                text-align: center;
                                            "
                                        >
                                            Confirmá tu correo electrónico
                                        </h1>

                                        <p
                                            style="
                                                margin: 0 0 16px;
                                                font-size: 15px;
                                                line-height: 1.7;
                                                color: #374151;
                                            "
                                        >
                                            Gracias por registrarte en el sistema
                                            de <strong>Actos Públicos Jóvenes y Adultos</strong>.
                                        </p>

                                        <p
                                            style="
                                                margin: 0 0 24px;
                                                font-size: 15px;
                                                line-height: 1.7;
                                                color: #374151;
                                            "
                                        >
                                            Para completar el registro y habilitar
                                            tu cuenta, necesitamos verificar tu
                                            dirección de correo electrónico.
                                        </p>

                                        <!-- BOTÓN -->
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
                                                        href="{safeConfirmationUrl}"
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
                                                        Confirmar mi correo electrónico
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <p
                                            style="
                                                margin: 0 0 10px;
                                                font-size: 14px;
                                                line-height: 1.6;
                                                color: #6b7280;
                                            "
                                        >
                                            Si el botón no funciona, podés copiar
                                            y pegar el siguiente enlace en tu navegador:
                                        </p>

                                        <p
                                            style="
                                                margin: 0;
                                                padding: 12px;
                                                background-color: #f9fafb;
                                                border: 1px solid #e5e7eb;
                                                border-radius: 6px;
                                                font-size: 12px;
                                                line-height: 1.5;
                                                word-break: break-all;
                                            "
                                        >
                                            <a
                                                href="{safeConfirmationUrl}"
                                                style="
                                                    color: #0b6fa4;
                                                    text-decoration: none;
                                                "
                                            >
                                                {safeConfirmationUrl}
                                            </a>
                                        </p>

                                        <div
                                            style="
                                                margin-top: 28px;
                                                padding-top: 20px;
                                                border-top: 1px solid #e5e7eb;
                                            "
                                        >
                                            <p
                                                style="
                                                    margin: 0;
                                                    font-size: 13px;
                                                    line-height: 1.6;
                                                    color: #6b7280;
                                                "
                                            >
                                                Si no realizaste este registro,
                                                no es necesario que hagas nada.
                                                Podés ignorar este correo.
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

                                        <p
                                            style="
                                                margin: 0 0 6px;
                                                font-size: 13px;
                                                font-weight: 600;
                                                color: #374151;
                                            "
                                        >
                                            Ministerio de Educación
                                        </p>

                                        <p
                                            style="
                                                margin: 0;
                                                font-size: 12px;
                                                color: #6b7280;
                                            "
                                        >
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
