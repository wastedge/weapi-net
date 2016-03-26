using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace WastedgeApi
{
    public class EntitySchema
    {
        public string Name { get; }
        public string Comments { get; }
        public IList<EntityPhysicalField> IdField { get; }
        public EntityField KeyField { get; }
        public EntityField LabelField { get; }
        public bool CanRead { get; }
        public bool CanCreate { get; }
        public bool CanUpdate { get; }
        public bool CanDelete { get; }
        public IKeyedCollection<string, EntityMember> Members { get; }

        internal EntitySchema(string name, JObject schema)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            Name = name;

            var members = new MemberCollection();

            members.Add(new EntityIdField());

            foreach (var field in (JObject)schema["fields"])
            {
                int? decimals = field.Value["decimals"] != null ? (int?)(long)field.Value["decimals"] : null;

                switch ((string)field.Value["type"])
                {
                    case "field":
                        members.Add(new EntityField(
                            field.Key,
                            (string)field.Value["comments"],
                            ParseDataType((string)field.Value["data_type"]),
                            decimals,
                            (bool)field.Value["mandatory"]
                        ));
                        break;

                    case "foreign":
                        members.Add(new EntityForeign(
                            field.Key,
                            (string)field.Value["comments"],
                            (string)field.Value["link_table"],
                            ParseDataType((string)field.Value["data_type"]),
                            decimals,
                            (bool)field.Value["mandatory"]
                        ));
                        break;

                    case "foreign_child":
                        members.Add(new EntityForeignChild(
                            field.Key,
                            (string)field.Value["comments"],
                            (string)field.Value["link_table"],
                            (string)field.Value["link_field"]
                        ));
                        break;

                    case "calculated":
                        members.Add(new EntityCalculatedField(
                            field.Key,
                            (string)field.Value["comments"],
                            ParseDataType((string)field.Value["data_type"]),
                            decimals
                        ));
                        break;
                }
            }

            Members = new ReadOnlyKeyedCollection<string, EntityMember>(members);

            var idFields = new List<EntityPhysicalField>();

            var ids = schema["id"];
            if (ids is JArray)
                idFields.AddRange(ids.Select(p => (EntityPhysicalField)members[(string)p]));
            else
                idFields.Add((EntityPhysicalField)members[(string)ids]);

            IdField = new ReadOnlyCollection<EntityPhysicalField>(idFields);

            string keyField = (string)schema["key"];
            if (keyField != null)
                KeyField = (EntityField)members[keyField];
            string labelField = (string)schema["label"];
            if (labelField != null)
                LabelField = (EntityField)members[labelField];

            Comments = (string)schema["comments"];

            foreach (string allow in schema["actions"])
            {
                switch (allow)
                {
                    case "read": CanRead = true; break;
                    case "create": CanCreate = true; break;
                    case "update": CanUpdate = true; break;
                    case "delete": CanDelete = true; break;
                }
            }
        }

        private EntityDataType ParseDataType(string dataType)
        {
            switch (dataType)
            {
                case "bytes": return EntityDataType.Bytes;
                case "string": return EntityDataType.String;
                case "date": return EntityDataType.Date;
                case "datetime": return EntityDataType.DateTime;
                case "datetime-tz": return EntityDataType.DateTimeTz;
                case "decimal": return EntityDataType.Decimal;
                case "long": return EntityDataType.Long;
                case "int": return EntityDataType.Int;
                case "bool": return EntityDataType.Bool;
                default: throw new ArgumentOutOfRangeException(nameof(dataType));
            }
        }

        private class MemberCollection : KeyedCollection<string, EntityMember>
        {
            protected override string GetKeyForItem(EntityMember item)
            {
                return item.Name;
            }
        }
    }
}
