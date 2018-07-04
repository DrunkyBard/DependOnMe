using CompilationUnit;
using DependOnMe.VsExtension.Messaging;
using DependOnMe.VsExtension.ModuleAdornment.UI;
using DslAst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DependOnMe.VsExtension
{
    internal static class Extensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> addFunc)
        {
            if (dict.TryGetValue(key, out var val))
            {
                return val;
            }

            var newValue = addFunc(key);
            dict.Add(key, newValue);

            return newValue;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> act)
        {
            foreach (var item in source)
            {
                act(item);
            }
        }

        public static ObservableCollection<TValue> ToObservable<TValue>(this IEnumerable<TValue> source)
            => new ObservableCollection<TValue>(source);

        public static Extension.ValidModulesContainer OnlyValidModules(this ModuleCompilationUnit cUnit)
            => Extension.OnlyValidModules(cUnit);

        public static Extension.ValidTestsContainer OnlyValidTests(this TestCompilationUnit cUnit)
            => Extension.OnlyValidTests(cUnit);

        public static ObservableCollection<PlainDependency> ToViewModels(this IEnumerable<CommonDslAst.ClassRegistration> classRegistrations)
            => classRegistrations
                .Select(cr => new PlainDependency(cr.Dependency, cr.Implementation))
                .ToObservable();

        public static ObservableCollection<DependencyModule> CollectSubModules(this CommonDslAst.ModuleRegistration[] modules)
            => modules
                .Select(m => ModuleHub.Instance.ModulePool.TryRequest(m.Name))
                .Where(sm => sm.IsSome)
                .Select(sm => sm.Value)
                .ToObservable();

        public static (IEnumerable<T> leftUnique, IEnumerable<(T leftIntersection, T rightIntersection)> intersection, IEnumerable<T> rightUnique) Split<T>(this ICollection<T> left, ICollection<T> right, IEqualityComparer<T> comparer)
        {
            var leftHashset  = new HashSet<T>(left, comparer);
            var rightHashset = new HashSet<T>(right, comparer);
            var intersection = new List<(T, T)>();

            foreach (var leftItem in left)
            {
                if (rightHashset.TryGetValue(leftItem, out var rightItem))
                {
                    intersection.Add((leftItem, rightItem));
                }
            }

            leftHashset.ExceptWith(intersection.Select(x => x.Item1));
            rightHashset.ExceptWith(intersection.Select(x => x.Item1));

            return (leftHashset.AsEnumerable(), intersection.AsEnumerable(), rightHashset.AsEnumerable());
        }
    }

    public class Func
    {
        public static Action Id = () => { };
    }
}
