using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace VietlifeStore.ChucNang.ChatAIs
{
    public class GeminiProviderService : IAIProviderService, ITransientDependency
    {
        private readonly HttpClient _http;
        private readonly GeminiOptions _opts;

        public GeminiProviderService(IHttpClientFactory factory, IOptions<GeminiOptions> opts)
        {
            _http = factory.CreateClient("Gemini");
            _opts = opts.Value;
        }

        public async IAsyncEnumerable<StreamChunk> StreamChatAsync(
    StreamChatRequest request,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var model = string.IsNullOrWhiteSpace(request.Model)
                ? _opts.DefaultModel
                : request.Model;

            var body = JsonSerializer.Serialize(new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = request.SystemPrompt } }
                },
                contents = BuildContents(request.Messages),
                generationConfig = new
                {
                    maxOutputTokens = request.MaxTokens,
                    temperature = 0.75,
                    topP = 0.95
                }
            });

            var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_opts.ApiKey}";

            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            using var response = await _http.SendAsync(req, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var node = JsonNode.Parse(json);

            var text =
            node?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>() ?? "";

            foreach (var ch in text)
            {
                yield return new StreamChunk(ch.ToString(), false, null, null);
                await Task.Delay(5, cancellationToken); // tốc độ typing
            }

            yield return new StreamChunk("", true, null, null);
        }
        /// <summary>
        /// Build contents cho Gemini (chỉ bao gồm lịch sử user & model)
        /// System prompt được xử lý riêng ở systemInstruction
        /// </summary>
        private static List<object> BuildContents(IReadOnlyList<MessageDto> messages)
        {
            var contents = new List<object>();

            foreach (var msg in messages)
            {
                var role = msg.Role.ToLowerInvariant() == "user" ? "user" : "model";

                contents.Add(new
                {
                    role = role,
                    parts = new[] { new { text = msg.Content } }
                });
            }

            return contents;
        }
    }

    // Giữ nguyên class này
    public class GeminiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gemini-2.5-flash";
    }
}