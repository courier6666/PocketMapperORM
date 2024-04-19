using System.Reflection;
using PocketMapperORM.Annotations;

namespace PocketMapperORM.Extensions;

public static class PropertyInfoExtensions
{
    public static string GetDbTableColumnName(this PropertyInfo propertyInfo)
    {
        ColumnDataAttribute? columnDataAttribute = propertyInfo.GetCustomAttribute<ColumnDataAttribute>();
        return columnDataAttribute?.Name ?? propertyInfo.Name;
    }
}