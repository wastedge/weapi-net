using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class ApiUpdate
    {
        private readonly Api _api;

        public EntitySchema Entity { get; }
        public ApiUpdateMode Mode { get; }
        public RecordSet Records { get; }

        internal ApiUpdate(Api api, EntitySchema entity, RecordSet records, ApiUpdateMode mode)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _api = api;
            Entity = entity;
            Mode = mode;
            Records = records ?? new RecordSet();
        }

        public StringCollection Execute()
        {
            string method = Mode == ApiUpdateMode.Create ? "PUT" : "POST";

            var response = (JObject)_api.ExecuteJson(Entity.Name, null, method, Records.ToJson());

            return BuildResponse(response);
        }

        public async Task<StringCollection> ExecuteAsync()
        {
            string method = Mode == ApiUpdateMode.Create ? "PUT" : "POST";

            var response = (JObject)await _api.ExecuteJsonAsync(Entity.Name, null, method, Records.ToJson());

            return BuildResponse(response);
        }

        private StringCollection BuildResponse(JObject response)
        {
            var result = new StringCollection();

            foreach (string id in (JArray)response["result"])
            {
                result.Add(id);
            }

            return result;
        }
    }
}
