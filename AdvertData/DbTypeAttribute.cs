using System;
using NpgsqlTypes;

namespace AdvertData
{
    internal class DbTypeAttribute : Attribute
    {
        public DbTypeAttribute(NpgsqlDbType sqlType)
        {
            SqlType = sqlType;
        }

        public NpgsqlDbType SqlType { get; set; }
    }
}