using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class Record : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public int Count => _values.Count;

        public IEnumerable<string> FieldNames => _values.Keys;

        public object this[string fieldName]
        {
            get { return _values[fieldName]; }
            set { _values[fieldName] = value; }
        }

        public void Add(string fieldName, object value)
        {
            _values.Add(fieldName, value);
        }

        public bool Remove(string fieldName)
        {
            return _values.Remove(fieldName);
        }

        public bool ContainsField(string fieldName)
        {
            return _values.ContainsKey(fieldName);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal JObject ToJson()
        {
            var result = new JObject();

            foreach (var item in this)
            {
                result.Add(item.Key, new JValue(item.Value));
            }

            return result;
        }
    }
}
