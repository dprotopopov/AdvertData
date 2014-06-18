using System.ComponentModel;
using NpgsqlTypes;

namespace AdvertData
{
    /// <summary>
    ///     Класс для хранения настроечных параметров модуля
    /// </summary>
    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class Settings
    {
        /// <summary>
        ///     Идентификатор записи
        /// </summary>
        [DbType(NpgsqlDbType.Varchar)]
        [Description("Идентификатор записи")]
        [Key]
        public string Name { get; set; }

        /// <summary>
        ///     Значение записи
        /// </summary>
        [DbType(NpgsqlDbType.Varchar)]
        [Description("Значение записи")]
        [Value]
        public string Value { get; set; }
    }
}