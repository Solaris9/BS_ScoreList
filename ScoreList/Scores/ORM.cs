using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ScoreList.Scores
{
    public abstract class Model<T>
    {
        private static SQLiteClient SQLiteInstance;

        protected Task Save()
        {
            if (SQLiteInstance == null) throw new Exception("Model is not initialized");

            var connection = SQLiteInstance.GetConnection();

            var command = connection.CreateCommand();
            command.CommandText = Utils.CreateInsertStatement(this);

            command.Prepare();
            return command.ExecuteNonQueryAsync();
        }

        protected static async Task<T> Get(object key)
        {
            if (SQLiteInstance == null) throw new ArgumentException("Model has not been loaded yet");
            var type = typeof(T);

            var connection = SQLiteInstance.GetConnection();

            var tableAttr = (Table)Attribute.GetCustomAttribute(type, typeof(Table));
            var tableName = tableAttr.Name ?? type.Name;

            var properties = type.GetProperties();
            var primaryField = properties.First(f => Attribute.GetCustomAttribute((MemberInfo) f, typeof(PrimaryKey)) != null);

            var command = connection.CreateCommand();

            var escapedKey = key is string ? $"\"{key}\"" : key;
            command.CommandText = $"SELECT * FROM {tableName} WHERE {primaryField.Name} = {escapedKey}";

            var reader = await command.ExecuteReaderAsync();
            var model = Activator.CreateInstance(type);

            if (reader.HasRows && await reader.ReadAsync())
            {
                for (var i = 0; i < properties.Count(); i++)
                {
                    var readValue = reader.GetValue(i);
                    var prop = properties[i];

                    var propertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    var value = Utils.ReadColumn(readValue, propertyType);

                    type.GetProperty(properties[i].Name).SetValue(model, value, null);
                }
            }
            else return default;

            return (T)model;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Table : Attribute
    {
        public readonly string Name;

        public Table(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryKey : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class NotNull : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class Unique : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class Indexed : Attribute { }

    public class SQLiteClient
    {
        private readonly SQLiteConnection connection;

        public SQLiteClient(string path)
        {
            connection = new SQLiteConnection($"Data Source={path}");
        }

        public Task Connect() => connection.OpenAsync();

        public SQLiteConnection GetConnection() => connection;

        public async Task<List<T>> Query<T>(string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;

            var reader = await command.ExecuteReaderAsync();

            var type = typeof(T);
            var properties = type.GetProperties();

            var records = new List<object>();


            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var model = Activator.CreateInstance(type);

                    for (int i = 0; i < properties.Count(); i++)
                    {
                        var readValue = reader.GetValue(i);
                        var value = Convert.ChangeType(readValue, properties[i].PropertyType);

                        type.GetProperty(properties[i].Name).SetValue(model, value);
                    }

                    records.Add(model);
                }
            }

            return records.Cast<T>().ToList();
        }

        public async Task CreateTable(params Type[] types)
        {
            foreach (Type type in types)
            {
                type.BaseType.GetField("SQLiteInstance", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public).SetValue(null, this);

                var tableAttr = (Table)Attribute.GetCustomAttribute(type, typeof(Table));
                var tableName = tableAttr.Name ?? type.Name;

                var properties = type.GetProperties().Select(Utils.CreateTableFieldData).ToList();
                if (properties.Count < 1) throw new ArgumentException("At least one column is required");

                var command = connection.CreateCommand();
                command.CommandText = Utils.CreateTableStatement(tableName, properties);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public class TableFieldData
    {
        public string Name;
        public Type Type;

        public bool PrimaryKey;
        public bool NotNull;
        public bool Unique;
        public bool Indexed;

        public override string ToString()
        {
            var field = $"\"{Name}\" {Utils.ParseType(Type)}";

            if (PrimaryKey) field += " PRIMARY KEY";
            if (NotNull) field += " NOT NULL";
            if (Unique) field += " UNIQUE";

            return field;
        }
    }

    public static class Utils
    {
        public static string CreateTableStatement(string name, List<TableFieldData> fields) =>
            $"CREATE TABLE IF NOT EXISTS \"{name}\" ({string.Join(", ", fields.Select(f => f.ToString()))});";

        public static TableFieldData CreateTableFieldData(PropertyInfo field)
        {
            var hasPrimaryKey = Attribute.GetCustomAttribute(field, typeof(PrimaryKey)) != null;
            var notNull = Attribute.GetCustomAttribute(field, typeof(NotNull)) != null;
            var isUnique = Attribute.GetCustomAttribute(field, typeof(Unique)) != null;
            var hasIndexed = Attribute.GetCustomAttribute(field, typeof(Indexed)) != null;

            return new TableFieldData
            {
                Name = field.Name,
                Type = field.PropertyType,

                PrimaryKey = hasPrimaryKey,
                NotNull = notNull,
                Unique = isUnique,
                Indexed = hasIndexed
            };
        }

        public static string ParseType(Type type)
        {
            if (type == typeof(int) || type == typeof(int?)) return "INTEGER";
            if (type == typeof(float) || type == typeof(float?)) return "FLOAT";
            if (type == typeof(double) || type == typeof(double?)) return "FLOAT";
            if (type == typeof(bool) || type == typeof(bool?)) return "INTEGER";
            if (type == typeof(DateTime) || type == typeof(DateTime?)) return "DATETIME";
            if (type == typeof(string)) return "VARCHAR";
            throw new ArgumentException($"Invalid type {type.FullName}");
        }

        public static string CreateInsertStatement(object model)
        {
            var tableAttr = (Table)Attribute.GetCustomAttribute(model.GetType(), typeof(Table));
            var tableName = tableAttr.Name ?? model.GetType().Name;

            var query = "INSERT OR IGNORE INTO \"{0}\" ({1}) VALUES ({2});";
            var properties = model.GetType().GetProperties();

            var columns = new List<string>();
            var values = new List<object>();

            foreach (var prop in properties)
            {
                var value = SerializeValue(prop.PropertyType, prop.GetValue(model));

                if (value != null)
                {
                    columns.Add(prop.Name);
                    values.Add(value);
                }
            }

            return string.Format(query, tableName, string.Join(",", columns), string.Join(",", values));
        }

        private static object SerializeValue(Type type, object value)
        {
            if (value == null) return null;

            if (type == typeof(DateTime)) return ((DateTime)value).ToString("\\\'yyyy-MM-dd\\\'");
            if (type == typeof(DateTime?)) return ((DateTime)value).ToString("\\\'yyyy-MM-dd\\\'");
            if (type == typeof(bool)) return (bool)value == true ? 1 : 0;
            if (type == typeof(string)) return $"'{((string)value).Replace("'", "''")}'";

            return value;
        }

        public static object ReadColumn(object value, Type propertyType)
        {
            if (value is DBNull) return null;
            if (propertyType == typeof(bool)) return (long)value == 1;

            var intTypes = new Type[] { typeof(uint), typeof(long), typeof(ulong) };
            if (intTypes.Contains(propertyType)) return (int)value;

            return Convert.ChangeType(value, propertyType);
        }
    }
}
