﻿namespace ControleFluxoCaixa.API.Tracing
{
    public class OpenTelemetryExtensions
    {
        public static string ServiceName { get; }
        public static string ServiceVersion { get; }

        static OpenTelemetryExtensions()
        {
            ServiceName = typeof(OpenTelemetryExtensions).Assembly.GetName().Name!;
            ServiceVersion = typeof(OpenTelemetryExtensions).Assembly.GetName().Version!.ToString();
        }
    }
}
