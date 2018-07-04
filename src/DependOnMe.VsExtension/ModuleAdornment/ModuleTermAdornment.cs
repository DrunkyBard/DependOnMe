using DependOnMe.VsExtension.Messaging;
using DependOnMe.VsExtension.ModuleAdornment.UI;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace DependOnMe.VsExtension.ModuleAdornment
{
    internal sealed class ModuleTermAdornment
    {
		private readonly IAdornmentLayer _btnLayer;
        private readonly object _syncObject = new object();


        private readonly IWpfTextView _view;
        private readonly ITagAggregator<ModuleTermTag> _tagger;

        private readonly Dictionary<int, List<(string moduleName, ModuleTree dep, ModuleButton modBtn)>> _lineAdornments
            = new Dictionary<int, List<(string, ModuleTree, ModuleButton)>>();

        private readonly Dictionary<string, (IDisposable onCreate, IDisposable onRemove)> _moduleSubscriptions 
            = new Dictionary<string, (IDisposable, IDisposable)>(StringComparer.OrdinalIgnoreCase);

        public ModuleTermAdornment(IWpfTextView view, ITagAggregator<ModuleTermTag> tagger)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            _btnLayer = view.GetAdornmentLayer("DrtModuleBtnTermAdornment");

            _view = view;
            _tagger = tagger;
            _view.LayoutChanged += OnLayoutChanged;
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                var tagSpans   = _tagger.GetTags(line.ExtentAsMappingSpan).ToArray();

                if (tagSpans.Length == 0)
                {
                    continue;
                }

                var lineNumber = line.Snapshot.GetLineNumberFromPosition(line.Start);

                CreateVisuals(lineNumber, tagSpans);
            }
        }

        private IDisposable SubscribeOnNewModule(int lineNumber, IMappingTagSpan<ModuleTermTag> tag)
            => ModuleHub
                .Instance
                .NewModulesStream
                .Where(x => x.CreatedModule.ModuleName.Equals(tag.Tag.ModuleName, StringComparison.OrdinalIgnoreCase))
                .Synchronize(_syncObject)
                .Subscribe(@event =>
                {
                    var depView    = new ModuleTree(@event.CreatedModule);
                    var btnView    = new ModuleButton(10, 10, depView);
                    var adornments = _lineAdornments.GetOrAdd(lineNumber, _ => new List<(string moduleName, ModuleTree dep, ModuleButton modBtn)>());

                    Render(depView, btnView, tag);
                    adornments.Add((tag.Tag.ModuleName, depView, btnView));
                });

        private IDisposable SubscribeOnRemovedModule(int lineNumber, string moduleName)
            => ModuleHub
                .Instance
                .RemovedModulesStream
                .Where(x => x.RemovedModule.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                .Synchronize(_syncObject)
                .Subscribe(@event =>
                {
                    var modName      = @event.RemovedModule.ModuleName;
                    var subscription = _moduleSubscriptions[modName];
                    _moduleSubscriptions.Remove(modName);
                    subscription.onCreate.Dispose();
                    subscription.onRemove.Dispose();

                    if (_lineAdornments.TryGetValue(lineNumber, out var adornments))
                    {
                        adornments
                            .Where(x => x.moduleName.Equals(modName, StringComparison.OrdinalIgnoreCase))
                            .ForEach(x =>
                            {
                                _btnLayer.RemoveAdornmentsByTag(modName);
                            });
                        
                        adornments.RemoveAll(x => x.moduleName.Equals(modName, StringComparison.OrdinalIgnoreCase));
                    }
                });

        private void CreateVisuals(int lineNumber, IMappingTagSpan<ModuleTermTag>[] tagSpans)
        {
            if (tagSpans.Length == 0 && _lineAdornments.ContainsKey(lineNumber))
            {
                _lineAdornments.Remove(lineNumber);

                foreach (var tagSpan in tagSpans)
                {
                    var moduleName = tagSpan.Tag.ModuleName;
                    var subscription = _moduleSubscriptions[moduleName];
                    subscription.onCreate.Dispose();
                    subscription.onRemove.Dispose();
                    _moduleSubscriptions.Remove(tagSpan.Tag.ModuleName);
                }
            }

            if (_lineAdornments.TryGetValue(lineNumber, out var adornmentDefs))
            {
                adornmentDefs
                    .RemoveAll(x => !tagSpans.Any(y => x.moduleName.Equals(y.Tag.ModuleName, StringComparison.OrdinalIgnoreCase)));
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
                    _btnLayer.RemoveAdornment(tuple.dep);
                    _btnLayer.RemoveAdornment(tuple.modBtn);
                    Render(tuple.dep, tuple.modBtn, tag);
                }

                foreach (var newModuleTag in newModuleTags)
                {
                    var moduleName = newModuleTag.Tag.ModuleName;
                    ModuleHub.Instance.ModulePool
                        .TryRequest(moduleName)
                        .WhenHasValueThen(module =>
                        {
                            var depView = new ModuleTree(module);
                            var btnView = new ModuleButton(10, 10, depView);
                            Render(depView, btnView, newModuleTag);
                            adornmentDefs.Add((moduleName, depView, btnView));
                        });

                    var onCreateSubscription = SubscribeOnNewModule(lineNumber, newModuleTag);
                    var onRemoveSubscription = SubscribeOnRemovedModule(lineNumber, newModuleTag.Tag.ModuleName);
                    _moduleSubscriptions.Add(newModuleTag.Tag.ModuleName, (onCreateSubscription, onRemoveSubscription));
                }
            }
            else
            {
                var newAdornments = new List<(string moduleName, ModuleTree dep, ModuleButton modBtn)>();

                foreach (var tag in tagSpans)
                {
                    var moduleName = tag.Tag.ModuleName;

                    ModuleHub.Instance.ModulePool
                        .TryRequest(moduleName)
                        .WhenHasValueThen(module =>
                        {
                            var depView = new ModuleTree(module);
                            var btnView = new ModuleButton(10, 10, depView);
                            Render(depView, btnView, tag);
                            newAdornments.Add((tag.Tag.ModuleName, depView, btnView));
                        });

                    var onCreateSubscription = SubscribeOnNewModule(lineNumber, tag);
                    var onRemoveSubscription = SubscribeOnRemovedModule(lineNumber, tag.Tag.ModuleName);
                    _moduleSubscriptions.Add(tag.Tag.ModuleName, (onCreateSubscription, onRemoveSubscription));
                }

                _lineAdornments.Add(lineNumber, newAdornments);
            }
        }

        private void Render(ModuleTree depView, ModuleButton btnView, IMappingTagSpan<ModuleTermTag> tag)
        {
            var textViewLines = _view.TextViewLines;
            var tagSpan       = tag.Span.GetSpans(_view.TextBuffer)[0];
            var geometry      = textViewLines.GetMarkerGeometry(tagSpan);
            
            Canvas.SetLeft(btnView, geometry.Bounds.Left);
            Canvas.SetTop(btnView, geometry.Bounds.Top - 10);

            Canvas.SetLeft(depView, geometry.Bounds.Right);
            Canvas.SetTop(depView, geometry.Bounds.Bottom);
            
            _btnLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, tagSpan, tag.Tag.ModuleName, btnView, null);
            _btnLayer.AddAdornment(AdornmentPositioningBehavior.OwnerControlled, tagSpan, tag.Tag.ModuleName, depView, null);
        }
    }
}
