using System;
using System.ComponentModel;
using NpgsqlTypes;

namespace AdvertData
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Record
    {
        /// <summary>
        ///     Идентификатор записи
        /// </summary>
        [DbType(NpgsqlDbType.Integer)]
        [Description("Идентификатор записи")]
        [Browsable(false)]
        [Key]
        public string Id { get; set; }

        /// <summary>
        ///     Время начала выполнения
        /// </summary>
        [DbType(NpgsqlDbType.Timestamp)]
        [Description("Время начала выполнения")]
        [Value]
        public DateTime StartTime { get; set; }

        /// <summary>
        ///     Время окончания выполнения
        /// </summary>
        [DbType(NpgsqlDbType.Timestamp)]
        [Description("Время окончания выполнения")]
        [Value]
        public DateTime EndTime { get; set; }

        [DbType(NpgsqlDbType.Varchar)]
        [Value]
        public string Key { get; set; }

        [DbType(NpgsqlDbType.Varchar)]
        [Value]
        public string Dll { get; set; }

        [DbType(NpgsqlDbType.Text)]
        [Value]
        public string Resource { get; set; }

        [DbType(NpgsqlDbType.Text)]
        [Value]
        public string Url { get; set; }

        [DbType(NpgsqlDbType.Varchar)]
        [Value]
        public string User { get; set; }

        [DbType(NpgsqlDbType.Varchar)]
        [Value]
        public string Result { get; set; }

        [DbType(NpgsqlDbType.Varchar)]
        [Value]
        public string Ad { get; set; }

        /// <summary>
        ///     Длительность выполнения
        ///     Вычисляемое поле (в БД не хранится) показывающее время с учетом миллисекунд как разницу между временем StartTime к
        ///     которому привязана запись и EndTime записи
        /// </summary>
        [DbType(NpgsqlDbType.Interval)]
        [Description("Длительность выполнения")]
        public TimeSpan Long { get; set; }
    }
}