using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class Api
    {
        private Schema _schema;
        private readonly Dictionary<string, EntitySchema> _entities = new Dictionary<string, EntitySchema>();

        public ApiCredentials Credentials { get; }

        public Api(ApiCredentials credentials)
        {
            if (credentials == null)
                throw new ArgumentNullException(nameof(credentials));

            Credentials = credentials;
        }

        public Schema GetSchema()
        {
            if (_schema == null)
                _schema = LoadSchema((JObject)ExecuteJson(null, "$meta", "GET", null));

            return _schema;
        }

        public async Task<Schema> GetSchemaAsync()
        {
            if (_schema == null)
                _schema = LoadSchema((JObject)await ExecuteJsonAsync(null, "$meta", "GET", null));

            return _schema;
        }

        private Schema LoadSchema(JObject json)
        {
            return new Schema(json["entities"].Select(p => (string)p).ToList());
        }

        public void CacheFullEntitySchema()
        {
            LoadFullEntitySchema((JObject)ExecuteJson(null, "$meta=all", "GET", null));
        }

        public async Task CacheFullEntitySchemaAsync()
        {
            LoadFullEntitySchema((JObject)await ExecuteJsonAsync(null, "$meta=all", "GET", null));
        }

        private void LoadFullEntitySchema(JObject schema)
        {
            if (_schema == null)
                _schema = new Schema(((JObject)schema["entities"]).Properties().Select(p => p.Name).ToList());

            foreach (var property in ((JObject)schema["entities"]).Properties())
            {
                if (!_entities.ContainsKey(property.Name))
                    _entities.Add(property.Name, new EntitySchema(property.Name, (JObject)property.Value));
            }
        }

        public EntitySchema GetEntitySchema(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            EntitySchema schema;
            if (!_entities.TryGetValue(name, out schema))
            {
                schema = new EntitySchema(name, (JObject)ExecuteJson(name, "$meta", "GET", null));
                _entities.Add(name, schema);
            }

            return schema;
        }

        public async Task<EntitySchema> GetEntitySchemaAsync(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            EntitySchema schema;
            if (!_entities.TryGetValue(name, out schema))
            {
                schema = new EntitySchema(name, (JObject)await ExecuteJsonAsync(name, "$meta", "GET", null));
                _entities[name] = schema;
            }

            return schema;
        }

        public ApiQuery CreateQuery(EntitySchema entity)
        {
            return new ApiQuery(this, entity);
        }

        public ApiUpdate CreateCreate(EntitySchema entity)
        {
            return new ApiUpdate(this, entity, null, ApiUpdateMode.Create);
        }

        public ApiUpdate CreateCreate(EntitySchema entity, RecordSet records)
        {
            return new ApiUpdate(this, entity, records, ApiUpdateMode.Create);
        }

        public ApiUpdate CreateUpdate(EntitySchema entity)
        {
            return new ApiUpdate(this, entity, null, ApiUpdateMode.Update);
        }

        public ApiUpdate CreateUpdate(EntitySchema entity, RecordSet records)
        {
            return new ApiUpdate(this, entity, records, ApiUpdateMode.Update);
        }

        public ApiDelete CreateDelete(EntitySchema entity)
        {
            return new ApiDelete(this, entity, null);
        }

        public ApiDelete CreateDelete(EntitySchema entity, IEnumerable<string> ids)
        {
            return new ApiDelete(this, entity, ids);
        }

        internal JToken ExecuteJson(string path, string parameters, string method, JToken request)
        {
            return Execute(
                path,
                parameters,
                method,
                p =>
                {
                    if (request != null)
                        p.Write(SerializeJson(request));
                },
                p => ParseJson(p.ReadToEnd())
            );
        }

        internal async Task<JToken> ExecuteJsonAsync(string path, string parameters, string method, JToken request)
        {
            return await ExecuteAsync(
                path,
                parameters,
                method,
                async p =>
                {
                    if (request != null)
                        await p.WriteAsync(SerializeJson(request));
                },
                async p => ParseJson(await p.ReadToEndAsync())
            );
        }

        public string ExecuteRaw(string path, string parameters, string method, string request)
        {
            return Execute(
                path,
                parameters,
                method,
                p =>
                {
                    if (request != null)
                        p.Write(request);
                },
                p => p.ReadToEnd()
            );
        }

        public async Task<string> ExecuteRawAsync(string path, string parameters, string method, string request)
        {
            return await ExecuteAsync(
                path,
                parameters,
                method,
                async p =>
                {
                    if (request != null)
                        await p.WriteAsync(request);
                },
                async p => await p.ReadToEndAsync()
            );
        }

        internal T Execute<T>(string path, string parameters, string method, Action<StreamWriter> requestWriter, Func<StreamReader, T> responseReader)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var webRequest = BuildRequest(path, parameters, method);

            // TODO: Once we support property verbs, this needs to exclude DELETE too.

            if (method != "GET")
            {
                using (var stream = webRequest.GetRequestStream())
                using (var writer = new StreamWriter(stream))
                {
                    requestWriter(writer);
                }
            }

            try
            {
                using (var response = webRequest.GetResponse())
                using (var stream = GetResponseStream(response))
                using (var reader = new StreamReader(stream))
                {
                    return responseReader(reader);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    throw;

                using (var response = ex.Response)
                using (var stream = GetResponseStream(response))
                using (var reader = new StreamReader(stream))
                {
                    throw ParseErrors(ex, reader.ReadToEnd());
                }
            }
        }

        internal async Task<T> ExecuteAsync<T>(string path, string parameters, string method, Func<StreamWriter, Task> requestWriter, Func<StreamReader, Task<T>> responseReader)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            var webRequest = BuildRequest(path, parameters, method);

            // TODO: Once we support property verbs, this needs to exclude DELETE too.

            if (method != "GET")
            {
                using (var stream = webRequest.GetRequestStream())
                using (var writer = new StreamWriter(stream))
                {
                    await requestWriter(writer);
                }
            }

            try
            {
                using (var response = await webRequest.GetResponseAsync())
                using (var stream = GetResponseStream(response))
                using (var reader = new StreamReader(stream))
                {
                    return await responseReader(reader);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                    throw;

                using (var response = ex.Response)
                using (var stream = GetResponseStream(response))
                using (var reader = new StreamReader(stream))
                {
                    throw ParseErrors(ex, await reader.ReadToEndAsync());
                }
            }
        }

        private Exception ParseErrors(WebException exception, string error)
        {
            JObject obj;

            try
            {
                obj = (JObject)JToken.Parse(error);
            }
            catch
            {
                // If we cannot parse the error response, we're just rethrowing the
                // original exception.

                return exception;
            }

            // First check whether it's a simple response.

            if (obj["message"] != null)
            {
                var message = obj["message"];

                if (message.Type == JTokenType.String)
                    return new ApiException((string)message, (string)obj["call_stack"], exception);

                if (message.Type == JTokenType.Array && message.Count() > 0)
                    return new ApiException((string)((JArray)message)[0], (string)obj["call_stack"], exception);

                // If we can't make sense of the response, throw the original exception.

                return exception;
            }

            // Next, see whether we got an error collection.

            if (obj["errors"] != null && obj["errors"].Type == JTokenType.Array)
                return new ApiException("Validation failed", exception, ApiRowErrorsCollection.FromJson((JArray)obj["errors"]));

            // If all else fails, rethrow the original exception.

            return exception;
        }

        private Stream GetResponseStream(WebResponse response)
        {
            if (response.Headers["Content-Encoding"] == "gzip")
                return new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);

            return response.GetResponseStream();
        }

        private JToken ParseJson(string input)
        {
            if (input.Length == 0)
                return null;

            using (var reader = new StringReader(input))
            using (var json = new JsonTextReader(reader))
            {
                json.DateParseHandling = DateParseHandling.None;
                json.FloatParseHandling = FloatParseHandling.Decimal;

                return JToken.ReadFrom(json);
            }
        }

        private string SerializeJson(JToken input)
        {
            using (var writer = new StringWriter())
            {
                using (var json = new JsonTextWriter(writer))
                {
                    input.WriteTo(json);
                }

                return writer.GetStringBuilder().ToString();
            }
        }

        private HttpWebRequest BuildRequest(string path, string parameters, string method)
        {
            var url = new StringBuilder();

            url.Append(Credentials.Url);

            if (!Credentials.Url.EndsWith("/"))
                url.Append('/');

            url.Append("api/rest");
            if (!String.IsNullOrEmpty(path))
                url.Append('/').Append(path);

            // This is a work around because webspeed doesn't accept PUT or DELETE.

            if (method != "GET" && method != "POST")
            {
                if (parameters != null)
                    parameters += "&";
                parameters += "$method=" + method;
                method = "POST";
            }

            if (parameters != null)
                url.Append('?').Append(parameters);

            var webRequest = (HttpWebRequest)WebRequest.Create(url.ToString());

            webRequest.Method = method;

            var authorization = Encoding.UTF8.GetBytes(Credentials.Company + "\\" + Credentials.UserName + ":" + Credentials.Password);

            webRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(authorization));

            return webRequest;
        }
    }
}
