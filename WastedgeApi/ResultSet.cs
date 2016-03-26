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
        private readonly ApiPager _pager;
        private readonly List<object[]> _rows;
        private object[] _row;
        private int _offset;
        private readonly List<EntityTypedField> _fields;
        private readonly Dictionary<string, int> _fieldsByName;
        private readonly Dictionary<EntityTypedField, int> _fieldsByField;
        private readonly string _nextResult;

        public EntitySchema Entity { get; }

        public object this[int index] => _row[index];

        public object this[string index] => this[_fieldsByName[index]];

        public object this[EntityPhysicalField index] => this[_fieldsByField[index]];

        public int FieldCount => _fields.Count;

        public int RowCount => _rows.Count;

        public bool HasMore { get; }

        internal ResultSet(EntitySchema entity, JObject results, ApiPager pager)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (results == null)
                throw new ArgumentNullException(nameof(results));
            if (pager == null)
                throw new ArgumentNullException(nameof(pager));

            _pager = pager;
            Entity = entity;

            Reset();

            HasMore = (bool)results["has_more"];
            _nextResult = (string)results["next_result"];

            var resultArray = (JArray)results["result"];
            var headers = (JArray)resultArray[0];

            _fields = headers.Select(p => (EntityTypedField)entity.Members[(string)p]).ToList();
            _fieldsByName = _fields.ToDictionary(p => p.Name, p => _fields.IndexOf(p));
            _fieldsByField = _fields.ToDictionary(p => p, p => _fields.IndexOf(p));

            _rows = new List<object[]>(resultArray.Count - 1);

            for (int i = 1; i < resultArray.Count; i++)
            {
                var resultRow = (JArray)resultArray[i];
                var row = new object[resultRow.Count];
                _rows.Add(row);

                for (int j = 0; j < resultRow.Count; j++)
                {
                    var value = ((JValue)resultRow[j]).Value;
                    switch (_fields[j].DataType)
                    {
                        case EntityDataType.Date:
                            value = Api.ParseDate((string)value);
                            break;
                        case EntityDataType.DateTime:
                            value = Api.ParseDateTime((string)value);
                            break;
                        case EntityDataType.DateTimeTz:
                            value = Api.ParseDateTimeOffset((string)value);
                            break;
                    }
                    row[j] = value;
                }
            }
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

        public ResultSet GetNext()
        {
            return GetNext(null);
        }

        public ResultSet GetNext(int? count)
        {
            if (!HasMore)
                throw new InvalidOperationException("No more records to retrieve");

            return _pager.GetNext(_nextResult, count);
        }

        public async Task<ResultSet> GetNextAsync()
        {
            return await GetNextAsync(null);
        }

        public async Task<ResultSet> GetNextAsync(int? count)
        {
            if (!HasMore)
                throw new InvalidOperationException("No more records to retrieve");

            return await _pager.GetNextAsync(_nextResult, count);
        }
    }
}
