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
                _schema = new Schema((JObject)ExecuteJsonRequest(null, "$meta", "GET", null));

            return _schema;
        }

        public async Task<Schema> GetSchemaAsync()
        {
            if (_schema == null)
                _schema = new Schema((JObject)await ExecuteJsonRequestAsync(null, "$meta", "GET", null));

            return _schema;
        }

        public EntitySchema GetEntitySchema(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            EntitySchema schema;
            if (!_entities.TryGetValue(name, out schema))
            {
                schema = new EntitySchema(name, (JObject)ExecuteJsonRequest(name, "$meta", "GET", null));
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
                schema = new EntitySchema(name, (JObject)await ExecuteJsonRequestAsync(name, "$meta", "GET", null));
                _entities[name] = schema;
            }

            return schema;
        }

        public ResultSet Query(EntitySchema entity, IEnumerable<Filter> filters)
        {
            return Query(entity, filters, null, null);
        }

        public ResultSet Query(EntitySchema entity, IEnumerable<Filter> filters, int? offset, int? count)
        {
            var parameters = BuildQueryParameters(filters, offset, count, OutputFormat.Compact);

            return new ResultSet(entity, (JObject)ExecuteJsonRequest(entity.Name, parameters.Parameters, "GET", null), new ApiPager(this, entity, parameters.BaseParameters));
        }

        public async Task<ResultSet> QueryAsync(EntitySchema entity, IEnumerable<Filter> filters)
        {
            return await QueryAsync(entity, filters, null, null);
        }

        public async Task<ResultSet> QueryAsync(EntitySchema entity, IEnumerable<Filter> filters, int? offset, int? count)
        {
            var parameters = BuildQueryParameters(filters, offset, count, OutputFormat.Compact);

            return new ResultSet(entity, (JObject)await ExecuteJsonRequestAsync(entity.Name, parameters.Parameters, "GET", null), new ApiPager(this, entity, parameters.BaseParameters));
        }

        public async Task<List<string>> CreateAsync(EntitySchema entity, RecordSet recordSet)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (recordSet == null)
                throw new ArgumentNullException(nameof(recordSet));

            if (recordSet.Count == 0)
                return new List<string>();

            var response = (JObject)await ExecuteJsonRequestAsync(entity.Name, null, "PUT", recordSet.ToJson());

            return ((JArray)response["result"]).Select(p => (string)p).ToList();
        }

        public async Task<List<string>> UpdateAsync(EntitySchema entity, RecordSet recordSet)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (recordSet == null)
                throw new ArgumentNullException(nameof(recordSet));

            if (recordSet.Count == 0)
                return new List<string>();

            var response = (JObject)await ExecuteJsonRequestAsync(entity.Name, null, "POST", recordSet.ToJson());

            return ((JArray)response["result"]).Select(p => (string)p).ToList();
        }

        public async Task DeleteAsync(EntitySchema entity, IList<string> ids)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            if (ids.Count == 0)
                return;

            string path = entity.Name + "/" + String.Join(",", ids);

            await ExecuteJsonRequestAsync(path, null, "DELETE", null);
        }

        private QueryParameters BuildQueryParameters(IEnumerable<Filter> filters, int? offset, int? count, OutputFormat outputFormat)
        {
            var sb = new StringBuilder();

            foreach (var filter in filters)
            {
                if (sb.Length > 0)
                    sb.Append('&');

                sb.Append(Uri.EscapeDataString(filter.Field.Name)).Append('=');

                switch (filter.Type)
                {
                    case FilterType.IsNull:
                        sb.Append("is.null");
                        break;
                    case FilterType.NotIsNull:
                        sb.Append("not.is.null");
                        break;
                    case FilterType.IsTrue:
                        sb.Append("is.true");
                        break;
                    case FilterType.NotIsTrue:
                        sb.Append("not.is.true");
                        break;
                    case FilterType.IsFalse:
                        sb.Append("is.false");
                        break;
                    case FilterType.NotIsFalse:
                        sb.Append("not.is.false");
                        break;
                    case FilterType.In:
                        sb.Append("in.");
                        AppendList(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.NotIn:
                        sb.Append("not.in.");
                        AppendList(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.Like:
                        sb.Append("like.");
                        Append(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.NotLike:
                        sb.Append("not.like.");
                        Append(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.Equal:
                        sb.Append("eq.");
                        Append(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.NotEqual:
                        sb.Append("ne.");
                        Append(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.GreaterThan:
                        sb.Append("gt.");
                        Append(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.GreaterEqual:
                        sb.Append("gte.");
                        Append(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.LessThan:
                        sb.Append("lt.");
                        Append(sb, filter.Value, filter.Field.DataType);
                        break;
                    case FilterType.LessEqual:
                        sb.Append("lte.");
                        Append(sb, filter.Value, filter.Field.DataType);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            switch (outputFormat)
            {
                case OutputFormat.Verbose:
                    if (sb.Length > 0)
                        sb.Append('&');
                    sb.Append("$output=verbose");
                    break;
                case OutputFormat.Compact:
                    if (sb.Length > 0)
                        sb.Append('&');
                    sb.Append("$output=compact");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outputFormat), outputFormat, null);
            }

            string baseParameters = sb.ToString();

            if (offset.HasValue)
            {
                if (sb.Length > 0)
                    sb.Append('&');
                sb.Append("$offset=").Append(offset.Value);
            }
            if (count.HasValue)
            {
                if (sb.Length > 0)
                    sb.Append('&');
                sb.Append("$count=").Append(count.Value);
            }

            return new QueryParameters(sb.ToString(), baseParameters);
        }

        internal ResultSet QueryNext(ApiPager pager, string start, int? count)
        {
            if (pager == null)
                throw new ArgumentNullException(nameof(pager));
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            var parameters = BuildNextParameters(pager, start, count);

            return new ResultSet(pager.Entity, (JObject)ExecuteJsonRequest(pager.Entity.Name, parameters, "GET", null), pager);
        }

        internal async Task<ResultSet> QueryNextAsync(ApiPager pager, string start, int? count)
        {
            if (pager == null)
                throw new ArgumentNullException(nameof(pager));
            if (start == null)
                throw new ArgumentNullException(nameof(start));

            var parameters = BuildNextParameters(pager, start, count);

            return new ResultSet(pager.Entity, (JObject)await ExecuteJsonRequestAsync(pager.Entity.Name, parameters, "GET", null), pager);
        }

        private static string BuildNextParameters(ApiPager pager, string start, int? count)
        {
            var sb = new StringBuilder(pager.Parameters);
            if (sb.Length > 0)
                sb.Append('&');
            sb.Append("$start=").Append(Uri.EscapeDataString(start));
            if (count.HasValue)
                sb.Append("&$count=").Append(count.Value);
            return sb.ToString();
        }

        private void AppendList(StringBuilder sb, object value, EntityDataType dataType)
        {
            throw new NotImplementedException();
        }

        private void Append(StringBuilder sb, object value, EntityDataType dataType)
        {
            sb.Append(Uri.EscapeDataString(Serialize(value, dataType)));
        }

        private string Serialize(object value, EntityDataType dataType)
        {
            if (value == null)
                return "";
            if (value is string)
                return (string)value;
            if (value is DateTime)
            {
                switch (dataType)
                {
                    case EntityDataType.Date:
                        return ApiUtils.PrintDate((DateTime)value);
                    case EntityDataType.DateTime:
                        return ApiUtils.PrintDateTime((DateTime)value);
                    case EntityDataType.DateTimeTz:
                        return ApiUtils.PrintDateTimeOffset((DateTime)value);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
            if (value is DateTimeOffset)
            {
                switch (dataType)
                {
                    case EntityDataType.Date:
                        return ApiUtils.PrintDate((DateTimeOffset)value);
                    case EntityDataType.DateTime:
                        return ApiUtils.PrintDateTime((DateTimeOffset)value);
                    case EntityDataType.DateTimeTz:
                        return ApiUtils.PrintDateTimeOffset((DateTimeOffset)value);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
            if (value is int)
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            if (value is long)
                return ((long)value).ToString(CultureInfo.InvariantCulture);
            if (value is float)
                return ((float)value).ToString(CultureInfo.InvariantCulture);
            if (value is double)
                return ((double)value).ToString(CultureInfo.InvariantCulture);
            if (value is decimal)
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);

            throw new ArgumentOutOfRangeException(nameof(value));
        }

        private JToken ExecuteJsonRequest(string path, string parameters, string method, JToken request)
        {
            var webRequest = BuildRequest(path, parameters, method);

            if (request != null)
            {
                using (var stream = webRequest.GetRequestStream())
                using (var writer = new StreamWriter(stream))
                using (var json = new JsonTextWriter(writer))
                {
                    request.WriteTo(json);
                }
            }

            try
            {
                using (var response = webRequest.GetResponse())
                using (var stream = GetResponseStream(response))
                using (var reader = new StreamReader(stream))
                {
                    return ParseJson(reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                using (var response = ex.Response)
                using (var stream = GetResponseStream(response))
                using (var reader = new StreamReader(stream))
                {
                    throw new ApiException(reader.ReadToEnd());
                }
            }
        }

        private async Task<JToken> ExecuteJsonRequestAsync(string path, string parameters, string method, JToken request)
        {
            var webRequest = BuildRequest(path, parameters, method);

            if (request != null || method != "GET")
            {
                using (var stream = await webRequest.GetRequestStreamAsync())
                using (var writer = new StreamWriter(stream))
                {
                    if (request != null)
                    {
                        using (var json = new JsonTextWriter(writer))
                        {
                            request.WriteTo(json);
                        }
                    }
                }
            }

            try
            {
                using (var response = await webRequest.GetResponseAsync())
                using (var stream = GetResponseStream(response))
                using (var reader = new StreamReader(stream))
                {
                    return ParseJson(await reader.ReadToEndAsync());
                }
            }
            catch (WebException ex)
            {
                using (var response = ex.Response)
                using (var stream = GetResponseStream(response))
                using (var reader = new StreamReader(stream))
                {
                    throw new ApiException(reader.ReadToEnd());
                }
            }
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

        public string ExecuteRaw(string path, string parameters, string method, string request)
        {
            var webRequest = BuildRequest(path, parameters, method);

            if (request != null)
            {
                using (var stream = webRequest.GetRequestStream())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(request);
                }
            }

            using (var response = webRequest.GetResponse())
            using (var stream = GetResponseStream(response))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private class QueryParameters
        {
            public string Parameters { get; }
            public string BaseParameters { get; }

            public QueryParameters(string parameters, string baseParameters)
            {
                Parameters = parameters;
                BaseParameters = baseParameters;
            }
        }
    }
}
