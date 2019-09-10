using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace ServiceB.Controllers {
    [ApiController]
    [Route("serviceb/api")]
    public class ValuesController : ControllerBase {
        private readonly ITracer _tracer;
        private readonly IHttpClientFactory _httpClientFactory;

        public ValuesController(ITracer tracer, IHttpClientFactory httpClientFactory) {
            _tracer = tracer;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("languages")]
        public async Task<ActionResult<IEnumerable>> GetLanguages() {
            var headers = Request.Headers.ToDictionary(k => k.Key, v => v.Value.First());
            using(var scope = StartServerSpan(_tracer, headers, "get-languages")) {
                var client = _httpClientFactory.CreateClient("serviceC");
                return JsonConvert.DeserializeObject<List<string>>(
                    await client.GetStringAsync("languages")
                );
            }
        }

        [HttpGet("frameworks")]
        public async Task<ActionResult<IEnumerable>> GetFrameworks() {
            var headers = Request.Headers.ToDictionary(k => k.Key, v => v.Value.First());
            using(var scope = StartServerSpan(_tracer, headers, "get-frameworks")) {
                var client = _httpClientFactory.CreateClient("serviceD");
                return JsonConvert.DeserializeObject<List<string>>(
                    await client.GetStringAsync("frameworks")
                );
            }
        }

        public static IScope StartServerSpan(ITracer tracer, IDictionary<string, string> headers, string operationName) {
            ISpanBuilder spanBuilder;
            try {
                var parentSpanCtx = tracer.Extract(BuiltinFormats.HttpHeaders, new TextMapExtractAdapter(headers));

                spanBuilder = tracer.BuildSpan(operationName);
                if (parentSpanCtx != null) {
                    spanBuilder = spanBuilder.AsChildOf(parentSpanCtx);
                }
            } catch (Exception) {
                spanBuilder = tracer.BuildSpan(operationName);
            }

            // TODO could add more tags like http.url
            return spanBuilder.WithTag(Tags.SpanKind, Tags.SpanKindServer).StartActive(true);
        }
    }
}