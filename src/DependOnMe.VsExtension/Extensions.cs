using CompilationUnit;
using DependOnMe.VsExtension.Messaging;
using DependOnMe.VsExtension.ModuleAdornment.UI;
using DslAst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

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

        public static ObservableCollection<TValue> ToObservable<TValue>(this IEnumerable<TValue> source)
            => new ObservableCollection<TValue>(source);

        public static Extension.ValidModuleRegistration[] OnlyValidModules(this ModuleCompilationUnit cUnit)
            => DslAst.Extension.OnlyValidModules(cUnit);

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
    }

    public class Func
    {
        public static Action Id = () => { };
    }
}
