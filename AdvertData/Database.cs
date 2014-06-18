using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Npgsql;
using NpgsqlTypes;

namespace AdvertData
{
    /// <summary>
    ///     Класс для работы с базой данных
    /// </summary>
    public class Database
    {
        private const string AdvertdataTable = "advertdata";
        private const string Settings = "settings";
        private const string ReportView = "report";
        private const string StarttimeView = "starttime";
        private const string EndtimeView = "endtime";
        private const string StarttimeColumn = "starttime";
        private const string NameColumn = "name";
        private const string ValueColumn = "value";
        private const string ReportProp = "report";

        public Database()
        {
            ConnectionString = "User Id=postgres;Password=postgres;Host=localhost;Database=advertdata;";
            Connection = new NpgsqlConnection(ConnectionString);
            Semaphore = new object();
        }

        /// <summary>
        ///     флаг «Вести отчет» устанавливает значение в True или False переменной Report отдельной таблицы проекта и
        ///     на данный отчет или методы никак не влияет. Данный фильтр нужен для других механизмов проекта, но управляется из
        ///     этого интерфейса. DLL должна содержать методы управления флагом, а так же метод определения состояния флага.
        /// </summary>
        public bool Report
        {
            get
            {
                string value = GetSetting(ReportProp);
                return !string.IsNullOrEmpty(value) && Convert.ToBoolean(value);
            }
            set { SetSetting(ReportProp, value.ToString()); }
        }

        /// <summary>
        ///     Семафор для блокирования одновременного доступа к классу из параллельных процессов
        ///     Использование lock(database.Semaphore) { ... }
        /// </summary>
        public object Semaphore { get; set; }

        /// <summary>
        ///     Строка для подключения к базе данных
        /// </summary>
        private string ConnectionString { get; set; }

        /// <summary>
        ///     Коннектор к базе данных
        /// </summary>
        private NpgsqlConnection Connection { get; set; }

        /// <summary>
        ///     Метод инициализации к базе данных
        /// </summary>
        public void Connect()
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Connection.Open();
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        ///     Метод отключения от базы данных
        /// </summary>
        public void Disconnect()
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Connection.Close();
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        ///     Очищает всю БД отчета.
        /// </summary>
        public void ClearAllReport()
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            string text = string.Format("TRUNCATE TABLE {0}", AdvertdataTable);
            Debug.WriteLine(text);
            using (var command = new NpgsqlCommand(text, Connection))
                command.ExecuteNonQuery();
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        ///     Метод удаляет StartTime и все связи от этого StartTime к остальным полям с каскадным удалением этих полей.
        ///     Т.е. удаление этих полей, если на них больше никто не ссылается.
        ///     Все исходные данные хранятся в одной таблице, поэтому список StartTime-ов является всего лишь выборой уникальных
        ///     значений StartTime из этой таблицы
        /// </summary>
        /// <param name="dateTime1">Минимальная дата и время с учетом миллисекунд. Допустимо значение null.</param>
        /// <param name="dateTime2">Максимальная дата и время с учетом миллисекунд. Допустимо значение null.</param>
        public void RemoveStartTime(string dateTime1 = null, string dateTime2 = null)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(dateTime1);
            Debug.WriteLine(dateTime2);
            var sb = new StringBuilder();
            sb.Append(string.Format("DELETE FROM {0} WHERE (1=1)", AdvertdataTable));
            if (!string.IsNullOrEmpty(dateTime1))
                sb.Append(string.Format(" AND ({0}_{1}>=:{1}1)", AdvertdataTable, StarttimeColumn));
            if (!string.IsNullOrEmpty(dateTime2))
                sb.Append(string.Format(" AND ({0}_{1}<=:{1}2)", AdvertdataTable, StarttimeColumn));
            string text = sb.ToString();
            Debug.WriteLine(text);
            using (var command = new NpgsqlCommand(text, Connection))
            {
                if (!string.IsNullOrEmpty(dateTime1))
                {
                    NpgsqlParameter param = command.Parameters.Add(string.Format("{0}1", StarttimeColumn),
                        NpgsqlDbType.Timestamp);
                    param.Value = Convert.ToDateTime(dateTime1);
                }
                if (!string.IsNullOrEmpty(dateTime2))
                {
                    NpgsqlParameter param = command.Parameters.Add(string.Format("{0}2", StarttimeColumn),
                        NpgsqlDbType.Timestamp);
                    param.Value = Convert.ToDateTime(dateTime2);
                }
                command.ExecuteNonQuery();
            }
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        ///     Создает новую запись StartTime.
        ///     Данный метод является заглушкой - то есть не выполняет никаких действий.
        ///     Все исходные данные хранятся в одной таблице, поэтому список StartTime-ов является всего лишь выборой уникальных
        ///     значений StartTime из этой таблицы
        /// </summary>
        /// <param name="dateTime">Дата и время с учетом миллисекунд</param>
        public void AddStartTime(string dateTime)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(dateTime);
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        ///     Проверка существования записей с указанной датой
        /// </summary>
        /// <param name="dateTime">Дата и время с учетом миллисекунд</param>
        /// <returns>Признак существования записей с указанной датой</returns>
        public bool Exists(string dateTime)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(dateTime);
            string text = string.Format("SELECT * FROM {0}_{1} WHERE {0}_{2}=:{2} LIMIT 1", AdvertdataTable,
                StarttimeView, StarttimeColumn);
            Debug.WriteLine(text);
            bool exists;
            using (var command = new NpgsqlCommand(text, Connection))
            {
                NpgsqlParameter param = command.Parameters.Add(StarttimeColumn, NpgsqlDbType.Timestamp);
                param.Value = Convert.ToDateTime(dateTime);
                using (NpgsqlDataReader dr = command.ExecuteReader())
                    exists = dr.HasRows;
            }
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            return exists;
        }

        /// <summary>
        ///     Получение значения настроечного параметра
        /// </summary>
        /// <param name="name">Имя настроечного параметра</param>
        /// <returns>Значение настроечного параметра</returns>
        public string GetSetting(string name)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(name);
            string text = string.Format("SELECT {0}_{3} FROM {0}_{1} WHERE {0}_{2}=:{2} LIMIT 1", AdvertdataTable,
                Settings, NameColumn, ValueColumn);
            Debug.WriteLine(text);
            string value = string.Empty;
            using (var command = new NpgsqlCommand(text, Connection))
            {
                NpgsqlParameter param = command.Parameters.Add(NameColumn, NpgsqlDbType.Varchar);
                param.Value = name;
                using (NpgsqlDataReader dr = command.ExecuteReader())
                    while (dr.Read())
                    {
                        value = dr.GetString(0);
                    }
            }
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            return value;
        }

        public IEnumerable<string> GetStartDates()
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            string text = string.Format("SELECT {0}_{2} FROM {0}_{1} ORDER BY {0}_{2}", AdvertdataTable,
                StarttimeView, StarttimeColumn);
            Debug.WriteLine(text);
            var list = new List<string>();
            using (var command = new NpgsqlCommand(text, Connection))
            using (NpgsqlDataReader dr = command.ExecuteReader())
                while (dr.Read())
                    list.Add(Convert.ToDateTime(dr[0]).ToString("s"));
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            return list;
        }

        /// <summary>
        ///     Добавление записи в таблицу
        /// </summary>
        /// <param name="record">Добавляемая запись</param>
        private void Add(Record record)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            IEnumerable<string> list =
                record.GetType()
                    .GetProperties()
                    .Where(propInfo => propInfo.GetCustomAttribute(typeof (ValueAttribute), false) != null)
                    .Select(propInfo => propInfo.Name.ToLower());
            string text = string.Format("INSERT INTO {0}({1}) VALUES({2})", AdvertdataTable,
                string.Join(",", list.Select(s => string.Format("{0}_{1}", AdvertdataTable, s))),
                string.Join(",", list.Select(s => string.Format(":{0}", s))));
            Debug.WriteLine(text);
            using (var command = new NpgsqlCommand(text, Connection))
            {
                foreach (string s in list)
                {
                    PropertyInfo propertyInfo = record.GetType()
                        .GetProperty(s, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    var customAttribute =
                        (DbTypeAttribute)
                            propertyInfo.GetCustomAttribute(typeof (DbTypeAttribute), false);
                    NpgsqlParameter param = command.Parameters.Add(s, customAttribute.SqlType);
                    param.Value = propertyInfo.GetValue(record, null);
                }
                command.ExecuteNonQuery();
            }
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        ///     Добавление записи в таблицу
        ///     http://stackoverflow.com/questions/1109061/insert-on-duplicate-update-in-postgresql
        /// </summary>
        /// <param name="settings">Добавляемая запись</param>
        private void Add(Settings settings)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            IEnumerable<string> keys =
                settings.GetType()
                    .GetProperties()
                    .Where(propInfo => propInfo.GetCustomAttribute(typeof (KeyAttribute), false) != null)
                    .Select(propInfo => propInfo.Name.ToLower());
            IEnumerable<string> values =
                settings.GetType()
                    .GetProperties()
                    .Where(propInfo => propInfo.GetCustomAttribute(typeof (ValueAttribute), false) != null)
                    .Select(propInfo => propInfo.Name.ToLower());
            IEnumerable<string> list = keys.Union(values);
            string text =
                string.Format(
                    "UPDATE {0}_{1} SET {2} WHERE {5};INSERT INTO {0}_{1}({3}) SELECT {4} WHERE NOT EXISTS (SELECT 1 FROM {0}_{1} WHERE {5});",
                    AdvertdataTable, Settings,
                    string.Join(",", values.Select(s => string.Format("{0}_{1}=:{1}", AdvertdataTable, s))),
                    string.Join(",", list.Select(s => string.Format("{0}_{1}", AdvertdataTable, s))),
                    string.Join(",", list.Select(s => string.Format(":{0}", s))),
                    string.Join(" AND ", keys.Select(s => string.Format("({0}_{1}=:{1})", AdvertdataTable, s))));
            Debug.WriteLine(text);
            using (var command = new NpgsqlCommand(text, Connection))
            {
                foreach (string s in list)
                {
                    PropertyInfo propertyInfo = settings.GetType()
                        .GetProperty(s, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    var postgreSqlType =
                        (DbTypeAttribute)
                            propertyInfo.GetCustomAttribute(typeof (DbTypeAttribute), false);
                    NpgsqlParameter param = command.Parameters.Add(s, postgreSqlType.SqlType);
                    param.Value = propertyInfo.GetValue(settings, null);
                }
                command.ExecuteNonQuery();
            }
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
        }

        /// <summary>
        ///     Метод находит запись со значением  StartTime и к ней добавляет остальные данные. В поле Time прописывает текущее
        ///     время записи. Возвращает False если StartTime не найдена.
        /// </summary>
        /// <param name="startTime">Время начала выполнения</param>
        /// <param name="key"></param>
        /// <param name="dll"></param>
        /// <param name="resource"></param>
        /// <param name="url"></param>
        /// <param name="user"></param>
        /// <param name="result"></param>
        /// <param name="ad"></param>
        /// <returns>Признак существования записей с указанной датой</returns>
        public bool SetData(string startTime, string key, string dll, string resource, string url, string user,
            string result, string ad = null)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(startTime);
            Debug.WriteLine(key);
            Debug.WriteLine(dll);
            Debug.WriteLine(resource);
            Debug.WriteLine(url);
            Debug.WriteLine(user);
            Debug.WriteLine(result);
            Debug.WriteLine(ad);
            bool exists = Exists(startTime);
            Add(new Record
            {
                StartTime = Convert.ToDateTime(startTime),
                EndTime = DateTime.Now,
                Key = key,
                Dll = dll,
                Resource = resource,
                Url = url,
                User = user,
                Result = result,
                Ad = ad
            });
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            return exists;
        }

        /// <summary>
        ///     Получение списка записей, находящихся в указанном диапазоне дат (включительно)
        ///     фильтр «показать отчет с … по…» если не задан, то таблица отображает все данные. Если задан одно значение «показать
        ///     отчет с…», то таблица отображает данные с указанного периода до конца значений;
        ///     Если задан одно значение «показать отчет по…», то таблица отображает данные с самого начала до указанного значения;
        ///     Если заданны оба значения, то таблица отображает данные входящие в этот период;
        ///     Данный фильтр ведется только по полю StartTime, т.е отображаются данные StartTime попавшие в фильтр и все
        ///     привязанные к нему данные даже если Time привязанных записей выпадает за пределы фильтра.
        /// </summary>
        /// <param name="dateTime1">Минимальная дата и время с учетом миллисекунд. Допустимо значение null.</param>
        /// <param name="dateTime2">Максимальная дата и время с учетом миллисекунд. Допустимо значение null.</param>
        /// <returns>Список записей</returns>
        public IEnumerable<Record> GetData(string dateTime1, string dateTime2)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(dateTime1);
            Debug.WriteLine(dateTime2);
            var sb = new StringBuilder();
            sb.Append(string.Format("SELECT * FROM {0}_{1} WHERE (1=1)", AdvertdataTable,
                ReportView));
            if (!string.IsNullOrEmpty(dateTime1))
                sb.Append(string.Format(" AND ({0}_{1}>=:{1}1)", AdvertdataTable,
                    StarttimeColumn));
            if (!string.IsNullOrEmpty(dateTime2))
                sb.Append(string.Format(" AND ({0}_{1}<=:{1}2)", AdvertdataTable,
                    StarttimeColumn));
            string text = sb.ToString();
            Debug.WriteLine(text);
            var list = new List<Record>();
            using (var command = new NpgsqlCommand(text, Connection))
            {
                if (!string.IsNullOrEmpty(dateTime1))
                {
                    NpgsqlParameter param1 = command.Parameters.Add(string.Format("{0}1", StarttimeColumn),
                        NpgsqlDbType.Timestamp);
                    param1.Value = Convert.ToDateTime(dateTime1);
                    Debug.WriteLine(param1.Value);
                }
                if (!string.IsNullOrEmpty(dateTime2))
                {
                    NpgsqlParameter param2 = command.Parameters.Add(string.Format("{0}2", StarttimeColumn),
                        NpgsqlDbType.Timestamp);
                    param2.Value = Convert.ToDateTime(dateTime2);
                    Debug.WriteLine(param2.Value);
                }
                using (NpgsqlDataReader dr = command.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var record = new Record();
                        foreach (PropertyInfo propertyInfo in record.GetType().GetProperties())
                        {
                            propertyInfo.SetValue(record,
                                Convert.ChangeType(
                                    dr[string.Format("{0}_{1}", AdvertdataTable, propertyInfo.Name.ToLower())],
                                    propertyInfo.PropertyType));
                        }
                        list.Add(record);
                    }
                }
            }
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            return list;
        }

        /// <summary>
        ///     Установка значения настроечного параметра
        /// </summary>
        /// <param name="name">Имя настроечного параметра</param>
        /// <param name="value">Значение настроечного параметра</param>
        public void SetSetting(string name, string value)
        {
            Debug.WriteLine("Begin {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
            Debug.WriteLine(name);
            Debug.WriteLine(value);
            Add(new Settings {Name = name, Value = value});
            Debug.WriteLine("End {0}::{1}", GetType().Name, MethodBase.GetCurrentMethod().Name);
        }
    }
}