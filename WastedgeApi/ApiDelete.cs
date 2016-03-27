using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class ApiDelete
    {
        private readonly Api _api;

        public EntitySchema Entity { get; }
        public StringCollection Ids { get; }

        internal ApiDelete(Api api, EntitySchema entity, IEnumerable<string> ids)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _api = api;
            Entity = entity;
            Ids = new StringCollection();

            if (ids != null)
            {
                foreach (string id in ids)
                {
                    Ids.Add(id);
                }
            }
        }

        public void Execute()
        {
            _api.ExecuteJson(BuildPath(), null, "DELETE", null);
        }

        public async Task ExecuteAsync()
        {
            await _api.ExecuteJsonAsync(BuildPath(), null, "DELETE", null);
        }

        private string BuildPath()
        {
            return Entity.Name + "/" + String.Join(",", Ids);
        }
    }
}
