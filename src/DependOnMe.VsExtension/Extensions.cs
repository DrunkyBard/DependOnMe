using Compilation;
using CompilationUnit;
using DependOnMe.VsExtension.Messaging;
using DependOnMe.VsExtension.ModuleAdornment.UI;
using DslAst;
using Microsoft.VisualStudio.Text;
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

        public static (bool hasName, string name) TryGetName(this ModuleDslAst.ModuleDeclaration declaration)
            => Extension.TryGetName(declaration).ToValueTuple();

        public static ObservableCollection<PlainDependency> ToViewModels(this IEnumerable<CommonDslAst.ClassRegistration> classRegistrations)
            => classRegistrations
                .Select(cr => new PlainDependency(cr.Dependency, cr.Implementation))
                .ToObservable();

        public static ObservableCollection<DependencyModule> CollectSubModules(this CommonDslAst.ModuleRegistration[] modules)
            => modules
                .Where(m => RefTable.Instance.HasWithoutDuplicates(m.Name))
                .Select(m => ModuleHub.Instance.ModulePool.Request(m.Name))
                .ToObservable();

        public static (IEnumerable<T> leftUnique, IEnumerable<(T leftIntersection, T rightIntersection)> intersection, IEnumerable<T> rightUnique) 
            Split<T>(this ICollection<T> left, ICollection<T> right, IEqualityComparer<T> comparer)
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

        public static (IReadOnlyCollection<Occurence<T>> leftUnique, IReadOnlyCollection<(Occurence<T> leftIntersection, Occurence<T> rightIntersection)> intersection, IReadOnlyCollection<Occurence<T>> rightUnique) 
            SplitD<T>(this ICollection<T> left, ICollection<T> right, IEqualityComparer<T> comparer)
        {
            var leftLookup  = left
                .GroupBy(x => x, comparer)
                .ToDictionary(x => x.Key, x => x.ToList().AsReadOnly(), comparer);
            var rightLookup = right
                .GroupBy(x => x, comparer)
                .ToDictionary(x => x.Key, x => x.ToList().AsReadOnly(), comparer);

            var intersection = new List<(Occurence<T> left, Occurence<T> right)>();
            var leftUnique   = new List<Occurence<T>>();

            foreach (var leftItem in leftLookup)
            {
                var leftOcc = new Occurence<T>(leftItem.Key, leftItem.Value);

                if (rightLookup.TryGetValue(leftItem.Key, out var rightItem))
                {
                    var rightOcc = new Occurence<T>(rightItem.First(), rightItem);
                    intersection.Add((leftOcc, rightOcc));
                }
                else
                {
                    leftUnique.Add(leftOcc);
                }
            }

            var rightUnique = rightLookup
                .Where(x => !leftLookup.ContainsKey(x.Key))
                .Select(x => new Occurence<T>(x.Key, x.Value))
                .ToList().AsReadOnly();
            
            return (leftUnique.AsReadOnly(), intersection.AsReadOnly(), rightUnique);
        }

        public static Some<string> TryGetFileName(this ITextBuffer buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument))
            {
                return Some<string>.Create(textDocument.FilePath);
            }

            return Some<string>.None;
        }
    }

    public struct Occurence<T>
    {
        public readonly T Key;
        public readonly IReadOnlyCollection<T> Occurences;

        public Occurence(T key, IReadOnlyCollection<T> occurences)
        {
            Key = key;
            Occurences = occurences;
        }
    }

    public class Func
    {
        public static Action Id = () => { };
    }
}
