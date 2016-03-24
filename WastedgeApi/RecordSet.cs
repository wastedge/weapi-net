using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class RecordSet : Collection<Record>
    {
        public RecordSet()
        {
        }

        public RecordSet(IList<Record> list)
            : base(list)
        {
        }

        public void AddResultSet(ResultSet resultSet)
        {
            if (resultSet == null)
                throw new ArgumentNullException(nameof(resultSet));

            resultSet.Reset();

            while (resultSet.Next())
            {
                var record = new Record();
                Add(record);

                for (int i = 0; i < resultSet.FieldCount; i++)
                {
                    record[resultSet.GetFieldName(i)] = resultSet[i];
                }
            }
        }

        internal JArray ToJson()
        {
            var array = new JArray();

            foreach (var record in this)
            {
                array.Add(record.ToJson());
            }

            return array;
        }
    }
}
