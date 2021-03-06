﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class ResultSet
    {
        private readonly List<object[]> _rows;
        private object[] _row;
        private int _offset;
        private readonly List<EntityTypedField> _fields;
        private readonly Dictionary<string, int> _fieldsByName;
        private readonly Dictionary<EntityTypedField, int> _fieldsByField;

        public EntitySchema Entity { get; }

        public object this[int index] => _row[index];

        public object this[string index]
        {
            get
            {
                if (!_fieldsByName.ContainsKey(index))
                    throw new KeyNotFoundException(string.Format("Key not found (\"{0}\")", index));

                return this[_fieldsByName[index]];
            }
        }

        public object this[EntityTypedField index] => this[_fieldsByField[index]];

        public int FieldCount => _fields.Count;

        public int RowCount => _rows.Count;

        public bool HasMore { get; }

        public string NextResult { get; }

        internal ResultSet(EntitySchema entity, JObject results, OutputFormat outputFormat)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            Entity = entity;

            Reset();

            HasMore = (bool)results["has_more"];
            NextResult = (string)results["next_result"];

            var resultArray = (JArray)results["result"];
            if (outputFormat == OutputFormat.Compact)
            {
                // Parse compact output format, the field names are in the first item in the array
                var headers = (JArray)resultArray[0];
                _fields = headers.Select(p => (EntityTypedField)entity.Members[(string)p]).ToList();

                _rows = new List<object[]>(resultArray.Count - 1);
                for (int i = 1; i < resultArray.Count; i++)
                {
                    var resultRow = (JArray)resultArray[i];
                    var row = new object[resultRow.Count];
                    _rows.Add(row);

                    for (int j = 0; j < resultRow.Count; j++)
                    {
                        var value = ((JValue)resultRow[j]).Value;
                        row[j] = this.ParseData(_fields[j].DataType, value);
                    }
                }
            }
            else
            {
                // Parse normal / verbose output format, field names are in each element
                _rows = new List<object[]>(resultArray.Count);
                for (int i = 0; i < resultArray.Count; i++)
                {
                    var resultRow = (JObject)resultArray[i];
                    if (i == 0)
                    {
                        // Get field names from the first item
                        var headers = new List<string>();
                        foreach (var property in resultRow)
                        {
                            headers.Add(property.Key);
                        }
                        _fields = headers.Select(p => (EntityTypedField)entity.Members[(string)p]).ToList();
                    }
                    var row = new object[resultRow.Count];
                    _rows.Add(row);

                    int j = 0;
                    foreach (var property in resultRow)
                    {
                        var field = entity.Members[property.Key] as EntityTypedField;
                        if (field == null) { continue; }
                        if (property.Value is JValue)
                        {
                            var value = ((JValue)property.Value).Value;
                            row[j] = this.ParseData(field.DataType, value);
                        }
                        else
                        {
                            row[j] = property.Value.ToString();
                        }

                        j++;
                    }
                }
            }

            if (_fields != null)
            {
                _fieldsByName = _fields.ToDictionary(p => p.Name, p => _fields.IndexOf(p));
                _fieldsByField = _fields.ToDictionary(p => p, p => _fields.IndexOf(p));
            }
        }

        object ParseData(EntityDataType dataType, object value) {
            switch (dataType)
            {
                case EntityDataType.Date:
                    return ApiUtils.ParseDate((string)value);
                case EntityDataType.DateTime:
                    return ApiUtils.ParseDateTime((string)value);
                case EntityDataType.DateTimeTz:
                    return ApiUtils.ParseDateTimeOffset((string)value);
            }
            return value;
        }


        public void Reset()
        {
            _offset = -1;
        }

        public bool Next()
        {
            if (_offset + 1 < _rows.Count)
            {
                _row = _rows[++_offset];
                return true;
            }

            _row = null;
            return false;
        }

        public bool IsNull(int index)
        {
            return this[index] == null;
        }

        public EntityTypedField GetField(int index)
        {
            return _fields[index];
        }

        public string GetFieldName(int index)
        {
            return _fields[index].Name;
        }

        public byte[] GetBytes(int index)
        {
            var value = GetString(index);
            if (value == null)
                return null;
            return Convert.FromBase64String(value);
        }

        public string GetString(int index)
        {
            return (string)this[index];
        }

        public DateTime GetDateTime(int index)
        {
            return (DateTime)this[index];
        }

        public DateTimeOffset GetDateTimeOffset(int index)
        {
            return (DateTimeOffset)this[index];
        }

        public decimal GetDecimal(int index)
        {
            object value = this[index];
            if (value is long)
                return (long)value;
            return (decimal)value;
        }

        public long GetLong(int index)
        {
            object value = this[index];
            if (value is decimal)
                return (long)(decimal)value;
            return (long)value;
        }

        public bool GetBool(int index)
        {
            return (bool)this[index];
        }
    }
}
