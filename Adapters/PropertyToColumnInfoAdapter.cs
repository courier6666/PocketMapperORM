using PocketMapperORM.Annotations;
using PocketMapperORM.DatatypeToSqlMapper;
using PocketMapperORM.Interfaces;
using System.Reflection;

namespace PocketMapperORM.Adapters
{
    public class PropertyToColumnInfoAdapter : IColumnInfo
    {
        private readonly PropertyInfo _propertyInfo;
        public PropertyToColumnInfoAdapter(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        public PropertyInfo PropertyInfo
        {
            get
            {
                return _propertyInfo;
            }
        }
        public string Name
        {
            get { return _propertyInfo.Name; }
        }
        public string DataType
        { 
            get
            {
                var dataType =
                    _propertyInfo.GetCustomAttribute<ColumnDataAttribute>()?.DataType;

                if(dataType != null)
                    return dataType;

                var propertyType = Nullable.GetUnderlyingType(_propertyInfo.PropertyType) ?? _propertyInfo.PropertyType;

                return DatatypeToSqlServerMapper.Instance[propertyType];
            }
        }
        public bool IsNullable
        { 
            get
            {
                if(_propertyInfo.GetCustomAttribute<PrimaryKeyAttribute>() is not null)
                    return false;

                return Nullable.
                    GetUnderlyingType(_propertyInfo.PropertyType) is not null;
            }
        }
        public bool isAutoIncremented
        {
            get
            {
                return _propertyInfo.GetCustomAttribute<AutoIncrementedAttribute>() is not null;
            }
        }
        public bool isUnique
        {
            get
            {
                return _propertyInfo.GetCustomAttribute<UniqueAttribute>() is not null
                       || _propertyInfo.GetCustomAttribute<PrimaryKeyAttribute>() is not null;
            }
        }
    }
}
