using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Some<T> Create(T value) => new Some<T>(value);

        public static Some<T> None => new Some<T>();

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TU ContinueWith<TU>(Func<T, TU> ifSome, Func<TU> ifNone)
        {
            if (IsSome)
            {
                return ifSome(Value);
            }

            return ifNone();
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WhenHasValueThen(Action<T> ifSome)
        {
            if (IsSome)
            {
                ifSome(Value);
            }
        }
    }
}
