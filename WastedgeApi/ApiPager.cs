using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WastedgeApi
{
    internal class ApiPager
    {
        private readonly Api _api;

        public EntitySchema Entity { get; }
        public string Parameters { get; }

        public ApiPager(Api api, EntitySchema entity, string parameters)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            _api = api;
            Entity = entity;
            Parameters = parameters;
        }

        public ResultSet GetNext(string start, int? count)
        {
            return _api.QueryNext(this, start, count);
        }

        public async Task<ResultSet> GetNextAsync(string start, int? count)
        {
            return await _api.QueryNextAsync(this, start, count);
        }
    }
}
