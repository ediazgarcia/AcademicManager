using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AcademicManager.Application.Services;

public class AiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    private string GetGeminiUrl()
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        // Forzamos gemini-2.5-flash ya que los modelos 2.0 nos tiran 429 quota 0 en esta key
        return $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
    }

    public async Task<(string inicio, string desarrollo, string cierre)> SugerirMomentosClaseAsync(string intencionPedagogica)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) 
            return ("Error: Falta el API Key de Gemini en la configuración.", "", "");

        var url = GetGeminiUrl();

        string prompt = $@"
Actúa como un profesor dominicano (MINERD).
Tengo esta intención pedagógica para mi clase diaria: '{intencionPedagogica}'

Genera 3 secciones:
1. INICIO: 2-3 viñetas cortas. Actividades de rescate de saberes previos y motivación.
2. DESARROLLO: 3-4 viñetas cortas. Construcción del conocimiento y práctica guiada.
3. CIERRE: 2 viñetas cortas. Evaluación formativa y síntesis.

Responde ÚNICAMENTE con un JSON con la siguiente estructura (NO markdown, no text, solo JSON crudo explícito):
{{
  ""inicio"": ""texto con viñetas"",
  ""desarrollo"": ""texto con viñetas"",
  ""cierre"": ""texto con viñetas""
}}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorStr = await response.Content.ReadAsStringAsync();
                return ($"Error de red o API: {response.StatusCode}", errorStr, "");
            }

            var jsonStr = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonStr);
            var textRes = doc.RootElement
                             .GetProperty("candidates")[0]
                             .GetProperty("content")
                             .GetProperty("parts")[0]
                             .GetProperty("text").GetString();

            if (textRes == null) return ("Respuesta vacía de la IA", "", "");

            textRes = textRes.Trim();
            if (textRes.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                textRes = textRes.Substring(7);
            if (textRes.EndsWith("```"))
                textRes = textRes.Substring(0, textRes.Length - 3);

            var resultDoc = JsonDocument.Parse(textRes.Trim());
            var init = resultDoc.RootElement.GetProperty("inicio").GetString() ?? "";
            var dev = resultDoc.RootElement.GetProperty("desarrollo").GetString() ?? "";
            var end = resultDoc.RootElement.GetProperty("cierre").GetString() ?? "";

            return (init, dev, end);
        }
        catch (Exception ex)
        {
            return ($"Error parseando IA: {ex.Message}", "", "");
        }
    }

    public async Task<Dictionary<string, string>> SugerirPlanAnualAsync(string titulo, string nivelCurso)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) 
            return new Dictionary<string, string> { { "Error", "Falta el API Key de Gemini en la configuración." } };

        var url = GetGeminiUrl();

        string prompt = $@"
Actúa como un profesor dominicano experto en el currículo MINERD.
Tengo este tema/título para mi planificación anual escolar: '{titulo}'
El curso o nivel es: '{nivelCurso}'

Genera los campos requeridos para la Planificación Anual. Responde ÚNICAMENTE con un objeto JSON crudo explícito (NO text, NO markdown). Usa exactamente estas llaves:
{{
  ""situacion_aprendizaje"": ""Descripción del problema del entorno que motiva el aprendizaje. Min 50 caracteres."",
  ""competencias_fundamentales"": ""Lista con viñetas de las competencias aplicables"",
  ""competencias_especificas"": ""Lista con viñetas de las competencias específicas"",
  ""conceptuales"": ""Lista con viñetas de conceptos"",
  ""procedimentales"": ""Lista con viñetas de procedimientos"",
  ""actitudinales"": ""Lista con viñetas de actitudes"",
  ""indicadores_logro"": ""Lista con viñetas de indicadores medibles y observables usando verbos pertinentes"",
  ""estrategias"": ""Lista con viñetas de estrategias de enseñanza"",
  ""recursos"": ""Lista de recursos separados por coma"",
  ""ejes"": ""Ejes transversales aplicables separados por coma""
}}";

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorStr = await response.Content.ReadAsStringAsync();
                return new Dictionary<string, string> { { "Error", $"Error de red o API: {response.StatusCode} - {errorStr}" } };
            }

            var jsonStr = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonStr);
            var textRes = doc.RootElement
                             .GetProperty("candidates")[0]
                             .GetProperty("content")
                             .GetProperty("parts")[0]
                             .GetProperty("text").GetString();

            if (textRes == null) return new Dictionary<string, string> { { "Error", "Respuesta vacía de la IA" } };

            textRes = textRes.Trim();
            if (textRes.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                textRes = textRes.Substring(7);
            if (textRes.EndsWith("```"))
                textRes = textRes.Substring(0, textRes.Length - 3);

            var resultDoc = JsonDocument.Parse(textRes.Trim());
            var dict = new Dictionary<string, string>();
            foreach(var prop in resultDoc.RootElement.EnumerateObject())
            {
                dict[prop.Name] = GetStringValue(prop.Value);
            }
            return dict;
        }
        catch (Exception ex)
        {
            return new Dictionary<string, string> { { "Error", $"Error parseando IA: {ex.Message}" } };
        }
    }

    public async Task<Dictionary<string, string>> SugerirPlanMensualAsync(string tituloUnidad, string temaAnual)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey)) 
            return new Dictionary<string, string> { { "Error", "Falta el API Key de Gemini en la configuración." } };

        var url = GetGeminiUrl();

        string prompt = $@"
Actúa como un profesor dominicano experto en el currículo MINERD.
Voy a crear una Planificación Mensual con el título de unidad: '{tituloUnidad}'.
Esto pertenece a una planificación anual sobre el tema general: '{temaAnual}'.

Genera los campos requeridos para la Planificación Mensual. Responde ÚNICAMENTE con un objeto JSON crudo explícito (NO text, NO markdown). Usa exactamente estas llaves:
{{
  ""situacion_aprendizaje"": ""Problema específico de este mes. Min 50 caracteres."",
  ""competencias_fundamentales"": ""Lista con viñetas"",
  ""competencias_especificas"": ""Lista con viñetas"",
  ""conceptuales"": ""Lista con viñetas"",
  ""procedimentales"": ""Lista con viñetas"",
  ""actitudinales"": ""Lista con viñetas"",
  ""indicadores_logro"": ""Lista con viñetas de indicadores aplicables a este mes"",
  ""estrategias"": ""Lista con viñetas de estrategias"",
  ""recursos"": ""Lista de recursos separados por coma"",
  ""ejes"": ""Ejes transversales separados por coma""
}}";

        return await SendPromptAndParseDictAsync(url, prompt);
    }

    public async Task<Dictionary<string, string>> SugerirPlanDiarioEstructuradoAsync(string intencionPedagogica, string temaMensual)
    {
        var url = GetGeminiUrl();

        string prompt = $@"
Actúa como un profesor dominicano MINERD.
Clase diaria sobre la intención: '{intencionPedagogica}'.
Pertenece al mes sobre la unidad: '{temaMensual}'.

Genera la estructura de la clase. Responde ÚNICAMENTE con JSON crudo explícito:
{{
  ""inicio"": ""Actividades de inicio (viñetas)"",
  ""desarrollo"": ""Actividades de desarrollo (viñetas)"",
  ""cierre"": ""Actividades de cierre (viñetas)"",
  ""estrategias"": ""Estrategias para esta clase (viñetas)"",
  ""organizacion"": ""Como organizar a los estudiantes (ej. Individual, Grupos de 4)"",
  ""vocabulario"": ""3-5 palabras clave"",
  ""recursos"": ""Recursos para la clase"",
  ""lecturas"": ""Materiales o páginas a leer""
}}";

        return await SendPromptAndParseDictAsync(url, prompt);
    }

    private async Task<Dictionary<string, string>> SendPromptAndParseDictAsync(string url, string prompt)
    {
        var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorStr = await response.Content.ReadAsStringAsync();
                return new Dictionary<string, string> { { "Error", $"Error: {response.StatusCode} - {errorStr}" } };
            }

            var jsonStr = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonStr);
            var textRes = doc.RootElement
                             .GetProperty("candidates")[0]
                             .GetProperty("content")
                             .GetProperty("parts")[0]
                             .GetProperty("text").GetString();

            if (textRes == null) return new Dictionary<string, string> { { "Error", "Respuesta vacía" } };

            textRes = textRes.Trim();
            if (textRes.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                textRes = textRes.Substring(7);
            if (textRes.EndsWith("```"))
                textRes = textRes.Substring(0, textRes.Length - 3);

            var resultDoc = JsonDocument.Parse(textRes.Trim());
            var dict = new Dictionary<string, string>();
            foreach (var prop in resultDoc.RootElement.EnumerateObject())
            {
                dict[prop.Name] = GetStringValue(prop.Value);
            }
            return dict;
        }
        catch (Exception ex)
        {
            return new Dictionary<string, string> { { "Error", $"Error parseando IA: {ex.Message}" } };
        }
    }

    private string GetStringValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
            return element.GetString() ?? "";
        if (element.ValueKind == JsonValueKind.Array)
        {
            var list = new List<string>();
            foreach (var item in element.EnumerateArray())
            {
                list.Add(GetStringValue(item));
            }
            return "- " + string.Join("\n- ", list);
        }
        if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
            return "";
        return element.ToString() ?? "";
    }
}
