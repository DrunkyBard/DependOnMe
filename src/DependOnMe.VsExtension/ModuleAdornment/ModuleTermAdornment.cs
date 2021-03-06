﻿using Compilation;
using DependOnMe.VsExtension.Messaging;
using DependOnMe.VsExtension.ModuleAdornment.UI;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Controls;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermAdornment
    {
        private static readonly IEqualityComparer<(string, string)> TwoStringComparer = EqualityComparerFactory.Create<(string, string)>(
            (x, y) => string.Equals(x.Item1, y.Item1, StringComparison.OrdinalIgnoreCase) &&
                      string.Equals(x.Item2, y.Item2, StringComparison.OrdinalIgnoreCase),
            x => x.Item1.ToUpperInvariant().GetHashCode() ^ x.Item2.ToUpperInvariant().GetHashCode());

		private readonly IAdornmentLayer _btnLayer;
        private readonly object _syncObject = new object();

        private readonly IWpfTextView _view;
        private readonly ITagAggregator<ModuleTermTag> _tagger;

        private readonly Dictionary<int, List<(string testName, string moduleName, UI.ModuleTree dep, ModuleButton modBtn)>> _lineAdornments
            = new Dictionary<int, List<(string, string, UI.ModuleTree, ModuleButton)>>();

        private Dictionary<(string test, string module), (int lineNumber, IDisposable subscription)> _onCreateModuleSubscriptions 
            = new Dictionary<(string test, string module), (int lineNumber, IDisposable subscription)>(TwoStringComparer);

        private Dictionary<(string test, string module), (int lineNumber, IDisposable subscription)> _onRemoveModuleSubscriptions 
            = new Dictionary<(string test, string module), (int lineNumber, IDisposable subscription)>(TwoStringComparer);

        private Dictionary<(string test, string module), (int lineNumber, IDisposable subscription)> _onDuplicateModuleSubscriptions 
            = new Dictionary<(string test, string module), (int lineNumber, IDisposable subscription)>(TwoStringComparer);

        public ModuleTermAdornment(IWpfTextView view, ITagAggregator<ModuleTermTag> tagger)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            _btnLayer = view.GetAdornmentLayer("DrtModuleBtnTermAdornment");
            
            _view   = view;
            _tagger = tagger;
            _view.LayoutChanged += OnLayoutChanged;
        }

        private IDisposable SubscribeOnNewModule(int lineNumber, IMappingTagSpan<ModuleTermTag> tag, double lineTop)
            => ModuleHub
                .Instance
                .NewModulesStream
                .Where(x => x.CreatedModule.ModuleName.Equals(tag.Tag.ModuleName, StringComparison.OrdinalIgnoreCase))
                .Synchronize(_syncObject)
                .Subscribe(@event =>
                {
                    var testName       = tag.Tag.TestName;
                    var moduleName     = tag.Tag.ModuleName;
                    var testModulePair = (testName, moduleName);
                    var depView        = new UI.ModuleTree(@event.CreatedModule);
                    var btnView        = new ModuleButton(10, 10, depView);
                    var adornments     = _lineAdornments.GetOrAdd(lineNumber, _ => new List<(string testName, string moduleName, UI.ModuleTree dep, ModuleButton modBtn)>());

                    _onCreateModuleSubscriptions[testModulePair].subscription.Dispose();
                    var success = _onCreateModuleSubscriptions.Remove(testModulePair);
                    Debug.Assert(success, $"{testName} : {moduleName} not exists in _onCreateModuleSubscriptions");

                    var onRemoveSubscription = SubscribeOnRemovedModule(lineNumber, tag, lineTop);
                    _onRemoveModuleSubscriptions.Add(testModulePair, (lineNumber, onRemoveSubscription));
                    var onDuplicateSubscription = SubscribeOnDuplicatedModule(lineNumber, tag, lineTop);
                    _onDuplicateModuleSubscriptions.Add(testModulePair, (lineNumber, onDuplicateSubscription));

                    UpdateView(lineNumber, lineTop);
                    //_view.DisplayTextLineContainingBufferPosition(line.Start, line.Top, ViewRelativePosition.Top);

                    Render(depView, btnView, tag);
                    adornments.Add((testName, moduleName, depView, btnView));
                });

        private IDisposable SubscribeOnDuplicatedModule(int lineNumber, IMappingTagSpan<ModuleTermTag> tag, double lineTop)
            => ModuleHub
                .Instance
                .DuplicatedModulesStream
                .Where(x => x.DuplicatedModule.Equals(tag.Tag.ModuleName, StringComparison.OrdinalIgnoreCase))
                .Synchronize(_syncObject)
                .Subscribe(@event =>
                {
                    var testName = tag.Tag.TestName;
                    var moduleName = tag.Tag.ModuleName;
                    var testModulePair = (testName, moduleName);

                    _onDuplicateModuleSubscriptions[testModulePair].subscription.Dispose();
                    var success = _onDuplicateModuleSubscriptions.Remove(testModulePair);
                    Debug.Assert(success, $"{testName} : {moduleName} not exists in _onDuplicateModuleSubscriptions");

                    _onRemoveModuleSubscriptions[testModulePair].subscription.Dispose();
                    var createSuccess = _onRemoveModuleSubscriptions.Remove(testModulePair);
                    Debug.Assert(createSuccess, $"{testName} : {moduleName} not exists in _onRemoveModuleSubscriptions");

                    var onCreateSubscription = SubscribeOnNewModule(lineNumber, tag, lineTop);
                    _onCreateModuleSubscriptions.Add(testModulePair, (lineNumber, onCreateSubscription));

                    if (_lineAdornments.TryGetValue(lineNumber, out var adornments))
                    {
                        adornments
                            .Where(x => x.moduleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                            .ForEach(x =>
                            {
                                _btnLayer.RemoveAdornment(x.dep);
                                _btnLayer.RemoveAdornment(x.modBtn);
                            });

                        UpdateView(lineNumber, lineTop);

                        adornments.RemoveAll(x => x.moduleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

                        if (adornments.Count == 0)
                        {
                            _lineAdornments.Remove(lineNumber);
                        }
                    }
                });

        private IDisposable SubscribeOnRemovedModule(int lineNumber, IMappingTagSpan<ModuleTermTag> tag, double lineTop)
            => ModuleHub
                .Instance
                .RemovedModulesStream
                .Where(x => x.RemovedModule.ModuleName.Equals(tag.Tag.ModuleName, StringComparison.OrdinalIgnoreCase))
                .Synchronize(_syncObject)
                .Subscribe(@event =>
                {
                    var testName       = tag.Tag.TestName;
                    var moduleName     = tag.Tag.ModuleName;
                    var testModulePair = (testName, moduleName);

                    _onRemoveModuleSubscriptions[testModulePair].subscription.Dispose();
                    var removeSuccess = _onRemoveModuleSubscriptions.Remove(testModulePair);
                    Debug.Assert(removeSuccess, $"{testName} : {moduleName} not exists in _onRemoveModuleSubscriptions");

                    _onDuplicateModuleSubscriptions[testModulePair].subscription.Dispose();
                    var duplicateSuccess = _onDuplicateModuleSubscriptions.Remove(testModulePair);
                    Debug.Assert(duplicateSuccess, $"{testName} : {moduleName} not exists in _onDuplicateModuleSubscriptions");

                    var onCreateSubscription = SubscribeOnNewModule(lineNumber, tag, lineTop);
                    _onCreateModuleSubscriptions.Add(testModulePair, (lineNumber, onCreateSubscription));
                    var onDuplicateSubscription = SubscribeOnDuplicatedModule(lineNumber, tag, lineTop);
                    _onDuplicateModuleSubscriptions.Add(testModulePair, (lineNumber, onDuplicateSubscription));

                    if (_lineAdornments.TryGetValue(lineNumber, out var adornments))
                    {
                        adornments
                            .Where(x => x.moduleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                            .ForEach(x =>
                            {
                                _btnLayer.RemoveAdornment(x.dep);
                                _btnLayer.RemoveAdornment(x.modBtn);
                            });

                        UpdateView(lineNumber, lineTop);
                        //_view.DisplayTextLineContainingBufferPosition(line.Start, line.Top, ViewRelativePosition.Top);
                        //_tagger.GetTags(tag.Span);

                        adornments.RemoveAll(x => x.moduleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

                        if (adornments.Count == 0)
                        {
                            _lineAdornments.Remove(lineNumber);
                        }
                    }
                });

        //private void UpdateView(ITextViewLine line, int lineNumber)
        private void UpdateView(int lineNumber, double lineTop)
        {
            var ctx = SynchronizationContext.Current;
                        
            if (ctx != null)
            {
                ctx.Post(_ =>
                {
                    var point = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
                    _view.DisplayTextLineContainingBufferPosition(point.Start, lineTop, ViewRelativePosition.Top);
                }, null);
            }
            else
            {
                var point = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
                _view.DisplayTextLineContainingBufferPosition(point.Start, lineTop, ViewRelativePosition.Top);
            }
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                var lineNumber = line.Snapshot.GetLineNumberFromPosition(line.Start);
                var tagSpans   = _tagger
                    .GetTags(line.ExtentAsMappingSpan)
                    .GroupBy(x => x.Tag)
                    .Select(x => x.First())
                    .Where(x => !_lineAdornments.Any(y => y.Key != lineNumber && y.Value.Any(
                        z => z.testName.Equals(x.Tag.TestName, StringComparison.OrdinalIgnoreCase) &&
                             z.moduleName.Equals(x.Tag.ModuleName, StringComparison.OrdinalIgnoreCase))) )
                    .ToArray();

                //ClearAdornments(lineNumber);

                if (_lineAdornments.TryGetValue(lineNumber, out var adornmentDefs))
                {
                    ClearAdornmentsProperly(lineNumber, tagSpans, adornmentDefs);
                    RemoveSubscriptionExcept(lineNumber, adornmentDefs.Select(x => (x.testName, x.moduleName)).ToHashSet(TwoStringComparer));
                }
                else
                {
                    RemoveSubscriptionOnEmptyAdornments(lineNumber);
                }

                if (tagSpans.Length == 0)
                {
                    continue;
                }

                CreateVisuals(lineNumber, tagSpans, line);
            }
        }

        private void ClearAdornments(int lineNumber)
        {
            if (_lineAdornments.TryGetValue(lineNumber, out var adornments))
            {
                foreach (var a in adornments)
                {                    
                    _btnLayer.RemoveAdornment(a.dep);
                    _btnLayer.RemoveAdornment(a.modBtn);
                }

                _lineAdornments.Remove(lineNumber);
            }

            var onCreate = _onCreateModuleSubscriptions.Where(x => x.Value.lineNumber == lineNumber).ToArray();
            onCreate.ForEach(x => x.Value.subscription.Dispose());
            onCreate.ForEach(x => _onCreateModuleSubscriptions.Remove(x.Key));

            var onRemove = _onRemoveModuleSubscriptions.Where(x => x.Value.lineNumber == lineNumber).ToArray();
            onRemove.ForEach(x => x.Value.subscription.Dispose());
            onRemove.ForEach(x => _onRemoveModuleSubscriptions.Remove(x.Key));

            var onDuplicate = _onDuplicateModuleSubscriptions.Where(x => x.Value.lineNumber == lineNumber).ToArray();
            onDuplicate.ForEach(x => x.Value.subscription.Dispose());
            onDuplicate.ForEach(x => _onDuplicateModuleSubscriptions.Remove(x.Key));
        }

        private void ClearAdornmentsProperly(
            int lineNumber, 
            IMappingTagSpan<ModuleTermTag>[] tagSpans,
            List<(string testName, string moduleName, UI.ModuleTree dep, ModuleButton modBtn)> adornmentDefs)
        {
            List<(string testName, string moduleName, UI.ModuleTree dep, ModuleButton modBtn)> forRemoveAdornments;

            if (tagSpans.Length > 0)
            {
                forRemoveAdornments = adornmentDefs
                    .Where(x => !tagSpans.Any(y => x.moduleName.Equals(y.Tag.ModuleName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                adornmentDefs.RemoveAll(x => !tagSpans.Any(y => x.moduleName.Equals(y.Tag.ModuleName, StringComparison.OrdinalIgnoreCase)));

                if (adornmentDefs.Count == 0)
                {
                    _lineAdornments.Remove(lineNumber);
                }
            }
            else
            {
                forRemoveAdornments = adornmentDefs;
                _lineAdornments.Remove(lineNumber);
            }

            foreach (var forRemoveAdornment in forRemoveAdornments)
            {
                _btnLayer.RemoveAdornment(forRemoveAdornment.dep);
                _btnLayer.RemoveAdornment(forRemoveAdornment.modBtn);

                var onCreate = _onCreateModuleSubscriptions
                    .Where(x => x.Value.lineNumber == lineNumber &&
                                x.Key.test.Equals(forRemoveAdornment.testName, StringComparison.OrdinalIgnoreCase) &&
                                x.Key.module.Equals(forRemoveAdornment.moduleName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                onCreate.ForEach(x => x.Value.subscription.Dispose());
                onCreate.ForEach(x => _onCreateModuleSubscriptions.Remove(x.Key));

                var onRemove = _onRemoveModuleSubscriptions
                    .Where(x => x.Value.lineNumber == lineNumber &&
                                x.Key.test.Equals(forRemoveAdornment.testName, StringComparison.OrdinalIgnoreCase) &&
                                x.Key.module.Equals(forRemoveAdornment.moduleName, StringComparison.OrdinalIgnoreCase))
                    .ToArray(); ;
                onRemove.ForEach(x => x.Value.subscription.Dispose());
                onRemove.ForEach(x => _onRemoveModuleSubscriptions.Remove(x.Key));

                var onDuplicate = _onDuplicateModuleSubscriptions
                    .Where(x => x.Value.lineNumber == lineNumber &&
                                x.Key.test.Equals(forRemoveAdornment.testName, StringComparison.OrdinalIgnoreCase) &&
                                x.Key.module.Equals(forRemoveAdornment.moduleName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                onDuplicate.ForEach(x => x.Value.subscription.Dispose());
                onDuplicate.ForEach(x => _onDuplicateModuleSubscriptions.Remove(x.Key));
            }
        }

        private void RemoveSubscriptionOnEmptyAdornments(int lineNumber)
        {
            var onCreate = _onCreateModuleSubscriptions
                .Where(x => x.Value.lineNumber == lineNumber)
                .ToArray();
            onCreate.ForEach(x => x.Value.subscription.Dispose());
            onCreate.ForEach(x => _onCreateModuleSubscriptions.Remove(x.Key));

            var onRemove = _onRemoveModuleSubscriptions
                .Where(x => x.Value.lineNumber == lineNumber)
                .ToArray(); ;
            onRemove.ForEach(x => x.Value.subscription.Dispose());
            onRemove.ForEach(x => _onRemoveModuleSubscriptions.Remove(x.Key));

            var onDuplicate = _onDuplicateModuleSubscriptions
                .Where(x => x.Value.lineNumber == lineNumber)
                .ToArray();
            onDuplicate.ForEach(x => x.Value.subscription.Dispose());
            onDuplicate.ForEach(x => _onDuplicateModuleSubscriptions.Remove(x.Key));
        }

        private void RemoveSubscriptionExcept(int lineNumber, HashSet<(string testName, string moduleName)> exceptSubs)
        {
            var onCreate = _onCreateModuleSubscriptions
                .Where(x => x.Value.lineNumber == lineNumber && !exceptSubs.Contains((x.Key.test, x.Key.module)))
                .ToArray();
            onCreate.ForEach(x => x.Value.subscription.Dispose());
            onCreate.ForEach(x => _onCreateModuleSubscriptions.Remove(x.Key));

            var onRemove = _onRemoveModuleSubscriptions
                .Where(x => x.Value.lineNumber == lineNumber && !exceptSubs.Contains((x.Key.test, x.Key.module)))
                .ToArray();
            onRemove.ForEach(x => x.Value.subscription.Dispose());
            onRemove.ForEach(x => _onRemoveModuleSubscriptions.Remove(x.Key));

            var onDuplicate = _onDuplicateModuleSubscriptions
                .Where(x => x.Value.lineNumber == lineNumber && !exceptSubs.Contains((x.Key.test, x.Key.module)))
                .ToArray();
            onDuplicate.ForEach(x => x.Value.subscription.Dispose());
            onDuplicate.ForEach(x => _onDuplicateModuleSubscriptions.Remove(x.Key));
        }

        private void CreateVisuals(int lineNumber, IMappingTagSpan<ModuleTermTag>[] tagSpans, ITextViewLine line)
        {
            UpdateView(lineNumber, line.Top);

            if (_lineAdornments.TryGetValue(lineNumber, out var adornmentDefs))
            {
                var existingAdornments = adornmentDefs
                    .Join(
                        tagSpans,
                        x => x.moduleName,
                        x => x.Tag.ModuleName,
                        (tuple, tag) => (tuple, tag),
                        StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                var newModuleTags = tagSpans
                    .Where(x => !adornmentDefs.Any(y => x.Tag.ModuleName.Equals(y.moduleName, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                foreach (var (tuple, tag) in existingAdornments)
                {
                    Render(tuple.dep, tuple.modBtn, tag);
                }

                foreach (var newModuleTag in newModuleTags)
                {
                    var testName   = newModuleTag.Tag.TestName;
                    var moduleName = newModuleTag.Tag.ModuleName;

                    if (RefTable.Instance.HasDuplicates(moduleName))
                    {
                        var subscription = SubscribeOnNewModule(lineNumber, newModuleTag, line.Top);
                        _onCreateModuleSubscriptions.Add((testName, moduleName), (lineNumber, subscription));
                    }
                    else if (RefTable.Instance.HasDefinition(moduleName))
                    {
                        var module = ModuleHub.Instance.ModulePool.Request(moduleName);
                        var depView = new UI.ModuleTree(module);
                        var btnView = new ModuleButton(10, 10, depView);
                        var onRemoveSubscription = SubscribeOnRemovedModule(lineNumber, newModuleTag, line.Top);
                        _onRemoveModuleSubscriptions.Add((testName, moduleName), (lineNumber, onRemoveSubscription));
                        var onDuplicateSubscription = SubscribeOnDuplicatedModule(lineNumber, newModuleTag, line.Top);
                        _onDuplicateModuleSubscriptions.Add((testName, moduleName), (lineNumber, onDuplicateSubscription));

                        Render(depView, btnView, newModuleTag);
                        adornmentDefs.Add((testName, moduleName, depView, btnView));
                    }
                    else
                    {
                        var subscription = SubscribeOnNewModule(lineNumber, newModuleTag, line.Top);
                        _onCreateModuleSubscriptions.Add((testName, moduleName), (lineNumber, subscription));
                    }
                }
            }
            else
            {
                var newAdornments = new List<(string testName, string moduleName, UI.ModuleTree dep, ModuleButton modBtn)>();

                foreach (var tag in tagSpans)
                {
                    var testName   = tag.Tag.TestName;
                    var moduleName = tag.Tag.ModuleName;

                    if (RefTable.Instance.HasDuplicates(moduleName))
                    {
                        var subscription = SubscribeOnNewModule(lineNumber, tag, line.Top);
                        _onCreateModuleSubscriptions.Add((testName, moduleName), (lineNumber, subscription));
                    }
                    else if (RefTable.Instance.HasDefinition(moduleName))
                    {
                        var module = ModuleHub.Instance.ModulePool.Request(moduleName);
                        var depView = new UI.ModuleTree(module);
                        var btnView = new ModuleButton(10, 10, depView);
                        var onRemoveSubscription = SubscribeOnRemovedModule(lineNumber, tag, line.Top);
                        _onRemoveModuleSubscriptions.Add((testName, moduleName), (lineNumber, onRemoveSubscription));
                        var onDuplicateSubscription = SubscribeOnDuplicatedModule(lineNumber, tag, line.Top);
                        _onDuplicateModuleSubscriptions.Add((testName, moduleName), (lineNumber, onDuplicateSubscription));

                        Render(depView, btnView, tag);
                        newAdornments.Add((testName, moduleName, depView, btnView));
                    }
                    else
                    {
                        var subscription = SubscribeOnNewModule(lineNumber, tag, line.Top);
                        _onCreateModuleSubscriptions.Add((testName, moduleName), (lineNumber, subscription));
                    }
                }

                if (newAdornments.Count != 0)
                {
                    _lineAdornments.Add(lineNumber, newAdornments);
                }
            }
        }

        private void Render(UI.ModuleTree depView, ModuleButton btnView, IMappingTagSpan<ModuleTermTag> tag)
        {
            var textViewLines = _view.TextViewLines;
            var tagSpan       = tag.Span.GetSpans(_view.TextBuffer)[0];
            var geometry      = textViewLines.GetMarkerGeometry(tagSpan);
            
            Canvas.SetLeft(btnView, geometry.Bounds.Left);
            Canvas.SetTop(btnView, geometry.Bounds.Top - 10);

            Canvas.SetLeft(depView, geometry.Bounds.Right);
            Canvas.SetTop(depView, geometry.Bounds.Bottom);
            
            _btnLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, tagSpan, null, btnView, null);
            _btnLayer.AddAdornment(AdornmentPositioningBehavior.OwnerControlled, tagSpan, null, depView, null);
        }
    }
}
