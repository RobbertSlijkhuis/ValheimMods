using System;

namespace WizshBoneTwitchIntegration.GuiOld
{
    /// <summary>
    /// Non-generic view of a <see cref="BoundField{T}"/> so <see cref="ObjectEditor"/>/
    /// <see cref="FieldUIBuilder"/> can detect and read/write it without knowing T.
    /// </summary>
    internal interface IBoundField
    {
        object GetValue();
        void SetValue(object value);
        Type WrappedType { get; }
    }

    /// <summary>
    /// A field on a narrowed "view" class that proxies reads/writes to a field on some
    /// other real data object (captured by the get/set delegates), instead of holding its
    /// own storage. Lets a view class expose a subset of another class's fields to the
    /// reflection-driven editor while edits still land live on the real object.
    /// </summary>
    internal sealed class BoundField<T> : IBoundField
    {
        private readonly Func<T> m_get;
        private readonly Action<T> m_set;

        public BoundField(Func<T> get, Action<T> set)
        {
            m_get = get;
            m_set = set;
        }

        public T Value
        {
            get => m_get();
            set => m_set(value);
        }

        public Type WrappedType => typeof(T);

        object IBoundField.GetValue() => Value;
        void IBoundField.SetValue(object value) => Value = (T)value;
    }
}
