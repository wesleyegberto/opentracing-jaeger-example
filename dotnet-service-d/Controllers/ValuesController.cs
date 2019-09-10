using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;

namespace ServiceD.Controllers {
    [ApiController]
    [Route("serviced/api")]
    public class ValuesController : ControllerBase {
        private readonly ITracer _tracer;

        public ValuesController(ITracer tracer) {
            _tracer = tracer;
        }

        [HttpGet("languages")]
        public ActionResult<IEnumerable<string>> GetLanguages() {
            var headers = Request.Headers.ToDictionary(k => k.Key, v => v.Value.First());
            using(var scope = StartServerSpan(_tracer, headers, "get-languages")) {
                Thread.Sleep(100);
                return new string[] {
                    "Java",
                    "C#",
                    "Ruby"
                };
            }
        }

        [HttpGet("frameworks")]
        public ActionResult<IEnumerable<string>> GetFrameworks() {
            var headers = Request.Headers.ToDictionary(k => k.Key, v => v.Value.First());
            using(var scope = StartServerSpan(_tracer, headers, "get-frameworks")) {
                Thread.Sleep(100);
                return new string[] {
                    "Spring",
                    "Aspnet",
                    "Rails"
                };
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