using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WastedgeApi
{
    public class RecordSetChanges
    {
        public static RecordSetChanges Create(RecordSet original, RecordSet modified)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));
            if (modified == null)
                throw new ArgumentNullException(nameof(modified));

            var originalMap = GetMap(original);
            var modifiedMap = GetMap(modified);

            var deleted = originalMap.Keys.Where(p => !modifiedMap.ContainsKey(p)).ToList();
            var newRecords = new List<Record>();
            var modifiedRecords = new List<Record>();

            foreach (var modifiedRecord in modified)
            {
                string id = GetId(modifiedRecord);

                Record originalRecord;
                if (id != null && originalMap.TryGetValue(id, out originalRecord))
                {
                    if (!AreEqual(originalRecord, modifiedRecord))
                        modifiedRecords.Add(modifiedRecord);
                }
                else
                {
                    newRecords.Add(modifiedRecord);
                }
            }

            return new RecordSetChanges(new RecordSet(newRecords), new RecordSet(modifiedRecords), deleted);
        }

        private static bool AreEqual(Record original, Record modified)
        {
            foreach (string fieldName in modified.FieldNames)
            {
                if (!original.ContainsField(fieldName))
                    return false;

                if (!AreValuesEqual(original[fieldName], modified[fieldName]))
                    return false;
            }

            return true;
        }

        private static bool AreValuesEqual(object original, object modified)
        {
            if (original != null && original.Equals(""))
                original = null;
            if (modified != null && modified.Equals(""))
                modified = null;

            if (original == null && modified == null)
                return true;
            if (original == null || modified == null)
                return false;

            if (original.GetType() == modified.GetType())
                return original.Equals(modified);

            original = SimplifyNumber(original);
            modified = SimplifyNumber(modified);

            return original.Equals(modified);
        }

        private static object SimplifyNumber(object value)
        {
            if (value is short)
                return (decimal)(short)value;
            if (value is ushort)
                return (decimal)(ushort)value;
            if (value is int)
                return (decimal)(int)value;
            if (value is uint)
                return (decimal)(uint)value;
            if (value is long)
                return (decimal)(long)value;
            if (value is ulong)
                return (decimal)(ulong)value;
            if (value is float)
                return (decimal)(float)value;
            if (value is double)
                return (decimal)(double)value;
            return value;
        }

        private static Dictionary<string, Record> GetMap(RecordSet recordSet)
        {
            var map = new Dictionary<string, Record>();

            foreach (var record in recordSet)
            {
                string id = GetId(record);
                if (id != null)
                    map.Add(id, record);
            }

            return map;
        }

        private static string GetId(Record record)
        {
            if (!record.ContainsField("$id"))
                return null;

            object id = record["$id"];
            if (id == null || id.Equals(""))
                return null;

            return id.ToString();
        }

        public RecordSet New { get; }

        public RecordSet Modified { get; }

        public IList<string> Deleted { get; }

        private RecordSetChanges(RecordSet @new, RecordSet modified, IList<string> deleted)
        {
            New = @new;
            Modified = modified;
            Deleted = deleted;
        }
    }
}
