using System;

namespace DependOnMe.VsExtension
{
    internal struct Some<T>
    {
        public bool IsSome { get; }

        public T Value { get; }

        private Some(T value)
        {
            IsSome = true;
            Value  = value;
        }

        public static Some<T> Create(T value) => new Some<T>(value);

        public static Some<T> None => new Some<T>();

        public TU ContinueWith<TU>(Func<T, TU> ifSome, Func<TU> ifNone)
        {
            if (IsSome)
            {
                return ifSome(Value);
            }

            return ifNone();
        }

        public void ContinueWith(Action<T> ifSome, Action ifNone)
        {
            if (IsSome)
            {
                ifSome(Value);
            }
            else
            {
                ifNone();
            }
        }
    }
}
