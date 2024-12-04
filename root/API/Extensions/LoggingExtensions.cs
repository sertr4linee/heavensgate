using Microsoft.Extensions.Logging;
using System.Text;

namespace API.Extensions
{
    public static class LoggingExtensions
    {
        private static int CalculateBoxWidth(string title, string[] messages)
        {
            var maxLength = Math.Max(title.Length + 4, // +4 spaces for title
                messages.Max(m => m.Length) + 4); // +4 spaces for messages

            return (maxLength + 1) / 2 * 2;
        }

        private static string CenterText(string text, int width)
        {
            var spaces = width - text.Length;
            var padLeft = spaces / 2;
            return text.PadLeft(padLeft + text.Length).PadRight(width);
        }

        public static void LogBox(this ILogger logger, LogLevel level, string title, params string[] messages)
        {
            var boxWidth = CalculateBoxWidth(title, messages);
            var box = new StringBuilder();

            box.AppendLine("╔" + new string('═', boxWidth - 2) + "╗");
            box.AppendLine("║ " + CenterText(title, boxWidth - 4) + " ║");
            box.AppendLine("╠" + new string('═', boxWidth - 2) + "╣");

            //msg
            foreach (var message in messages)
            {
                var cleanMessage = message.Replace("║", "").Trim();
                box.AppendLine("║ " + cleanMessage.PadRight(boxWidth - 4) + " ║");
            }

            box.AppendLine("╚" + new string('═', boxWidth - 2) + "╝");

            switch (level)
            {
                case LogLevel.Information:
                    logger.LogInformation(box.ToString());
                    break;
                case LogLevel.Warning:
                    logger.LogWarning(box.ToString());
                    break;
                case LogLevel.Error:
                    logger.LogError(box.ToString());
                    break;
                default:
                    logger.LogInformation(box.ToString());
                    break;
            }
        }

        public static void LogRequest(this ILogger logger, HttpContext context, string message)
        {
            logger.LogBox(LogLevel.Information, 
                "Requête HTTP",
                $"Méthode: {context.Request.Method}",
                $"PATH: {context.Request.Path}",
                $"STATUS: {context.Response.StatusCode}",
                $"MESSAGE: {message}"
            );
        }

        public static void LogTransaction(this ILogger logger, string path, string status)
        {
            logger.LogBox(LogLevel.Information,
                "DB TRANSACTION",
                $"PATH: {path}",
                $"STATUS: {status}"
            );
        }

        public static void LogError(this ILogger logger, Exception ex)
        {
            var stackTraceLines = ex.StackTrace?
                .Split('\n')
                .Take(3)
                .Select(s => s.Trim())
                .ToArray() ?? Array.Empty<string>();

            var messages = new List<string>
            {
                $"Type: {ex.GetType().Name}",
                $"Message: {ex.Message}",
                "StackTrace:"
            };
            messages.AddRange(stackTraceLines);

            logger.LogBox(LogLevel.Error, "ERREUR", messages.ToArray());
        }

        public static void LogStartup(this ILogger logger, string environment)
        {
            logger.LogBox(LogLevel.Information,
                "APP STARTING...",
                $"ENV: {environment}",
                $"Date: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                $"Version: {typeof(LoggingExtensions).Assembly.GetName().Version}"
            );
        }

        public static void LogAuthentication(this ILogger logger, string username, bool success)
        {
            var level = success ? LogLevel.Information : LogLevel.Warning;
            logger.LogBox(level,
                "AUTHENTIFICATION",
                $"USER: {username}",
                $"STATUS: {(success ? "SUCCESS" : "FAILURE")}",
                $"DATE: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"
            );
        }
    }
} 