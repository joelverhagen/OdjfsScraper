using System;

namespace OdjfsScraper.Export
{
    public struct SrdsAttribute<TEntity>
    {
        private readonly Func<TEntity, string> _valueGetter;

        public SrdsAttribute(string name, string typeName, Func<TEntity, string> valueGetter)
            : this()
        {
            Name = name;
            TypeName = typeName;
            _valueGetter = valueGetter;
        }

        public SrdsAttribute(string name, string typeName, Func<TEntity, object> valueGetter)
            : this(name, typeName, e =>
            {
                object value = valueGetter(e);
                return value != null ? value.ToString() : string.Empty;
            })
        {
        }

        public SrdsAttribute(string name, string typeName, Func<TEntity, DateTime> valueGetter)
            : this(name, typeName, e => valueGetter(e).ToString("r"))
        {
        }

        public SrdsAttribute(string name, string typeName, Func<TEntity, DateTime?> valueGetter)
            : this(name, typeName, e =>
            {
                DateTime? value = valueGetter(e);
                return value != null ? value.Value.ToString("r") : string.Empty;
            })
        {
        }

        public string Name { get; private set; }
        public string TypeName { get; set; }

        public string GetValue(TEntity entity)
        {
            return _valueGetter(entity);
        }
    }
}