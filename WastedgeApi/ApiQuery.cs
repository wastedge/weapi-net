using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class ApiQuery
    {
        private readonly Api _api;

        public EntitySchema Entity { get; }
        public FilterCollection Filters { get; }
        public string Query { get; set; }
        public int? Offset { get; set; }
        public int? Count { get; set; }
        public string Start { get; set; }
        public StringCollection Expand { get; }
        public QueryOrderCollection Order { get; }
        public OutputFormat Output { get; set; }

        internal ApiQuery(Api api, EntitySchema entity)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _api = api;
            Entity = entity;

            Filters = new FilterCollection();
            Expand = new StringCollection();
            Order = new QueryOrderCollection();
            Output = OutputFormat.Compact;
        }

        public RecordSet Execute()
        {
            var result = new RecordSet();

            result.AddResultSet(ExecuteReader());

            return result;
        }

        public async Task<RecordSet> ExecuteAsync()
        {
            var result = new RecordSet();

            result.AddResultSet(await ExecuteReaderAsync());

            return result;
        }

        public JObject ExecuteJson()
        {
            return (JObject)_api.ExecuteJson(Entity.Name, BuildQueryParameters(), "GET", null);
        }

        public async Task<JObject> ExecuteJsonAsync()
        {
            return (JObject)await _api.ExecuteJsonAsync(Entity.Name, BuildQueryParameters(), "GET", null);
        }

        public ResultSet ExecuteReader()
        {
            if (Output != OutputFormat.Compact)
                throw new ApiException("Output format must be compact");

            return new ResultSet(Entity, ExecuteJson());
        }

        public async Task<ResultSet> ExecuteReaderAsync()
        {
            if (Output != OutputFormat.Compact)
                throw new ApiException("Output format must be compact");

            return new ResultSet(Entity, await ExecuteJsonAsync());
        }

        private string BuildQueryParameters()
        {
            var sb = new StringBuilder();

            switch (Output)
            {
                case OutputFormat.Verbose:
                    sb.Append("$output=verbose");
                    break;
                case OutputFormat.Compact:
                    sb.Append("$output=compact");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var filter in Filters)
            {
                sb.Append('&').Append(Uri.EscapeDataString(filter.Field.Name)).Append('=');

                if (filter.Field is EntityIdField)
                {
                    if (filter.Type != FilterType.Equal)
                        throw new ApiException("ID field can only be compared equal");

                    Append(sb, filter.Value, filter.Field.DataType);
                    continue;
                }

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

            if (Order.Count > 0)
            {
                sb.Append("&$order=").Append(Uri.EscapeDataString(String.Join(
                    ",",
                    Order.Select(p => p.Field + (p.Direction == QueryOrderDirection.Ascending ? ".asc" : ".desc"))
                )));
            }

            if (Expand.Count > 0)
                sb.Append("&$expand=").Append(Uri.EscapeDataString(String.Join(",", Expand)));
            if (Query != null)
                sb.Append("&$query=").Append(Uri.EscapeDataString(Query));
            if (Offset.HasValue)
                sb.Append("&$offset=").Append(Offset.Value);
            if (Count.HasValue)
                sb.Append("&$count=").Append(Count.Value);
            if (Start != null)
                sb.Append("&$start=").Append(Uri.EscapeDataString(Start));

            return sb.ToString();
        }

        private void AppendList(StringBuilder sb, object value, EntityDataType dataType)
        {
            if (value == null)
                return;

            var enumerable = value as IEnumerable;
            if (enumerable == null)
                throw new ApiException("Expected parameter to the IN filter to be a collection");

            bool hadOne = false;

            foreach (object element in enumerable)
            {
                string serialized = ApiUtils.Serialize(element, dataType) ?? "";

                if (serialized.IndexOf('\'') != -1 || serialized.IndexOf(',') != -1)
                    serialized = "'" + serialized.Replace("'", "''") + "'";

                if (hadOne)
                    sb.Append(',');
                else
                    hadOne = true;

                sb.Append(Uri.EscapeDataString(serialized));
            }
        }

        private void Append(StringBuilder sb, object value, EntityDataType dataType)
        {
            sb.Append(Uri.EscapeDataString(ApiUtils.Serialize(value, dataType)));
        }
    }
}
