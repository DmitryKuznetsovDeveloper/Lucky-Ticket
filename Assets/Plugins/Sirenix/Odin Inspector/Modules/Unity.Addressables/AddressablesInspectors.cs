//-----------------------------------------------------------------------
// <copyright file="AddressablesInspectors.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Plugins.Sirenix.Odin_Inspector.Modules.Unity.Addressables;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;

[assembly: RegisterAssetReferenceAttributeForwardToChild(typeof(InlineEditorAttribute))]
[assembly: RegisterAssetReferenceAttributeForwardToChild(typeof(PreviewFieldAttribute))]
#endif
namespace Plugins.Sirenix.Odin_Inspector.Modules.Unity.Addressables
{
    /// <summary>
    /// <para>DisallowAddressableSubAssetField is used on AssetReference properties, and disallows and prevents assigned sub-assets to the asset reference.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// [DisallowAddressableSubAssetField]
    /// public AssetReference Reference;
    /// </code>
    /// </example>
    /// <seealso cref="RegisterAssetReferenceAttributeForwardToChildAttribute" />
    /// <seealso cref="AssetReferenceUILabelRestriction" />
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    public class DisallowAddressableSubAssetFieldAttribute : Attribute
    {
    }

    /// <summary>
    /// <para>Registers an attribute to be applied to an AssetRefenece property, to be forwarded and applied to the AssetReference's child instead.</para>
    /// <para>This allows attributes designed for use on UnityEngine.Objects to be used on AssetReference properties as well.</para>
    /// <para>By default, <c>InlineEditorAttribute</c> and <c>PreviewFieldAttribute</c> are registered for forwarding.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// [assembly: Sirenix.OdinInspector.Modules.RegisterAssetReferenceAttributeForwardToChild(typeof(InlineEditorAttribute))]
    /// [assembly: Sirenix.OdinInspector.Modules.RegisterAssetReferenceAttributeForwardToChild(typeof(PreviewFieldAttribute))]
    /// </code>
    /// </example>
    /// <seealso cref="DisallowAddressableSubAssetFieldAttribute" />
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterAssetReferenceAttributeForwardToChildAttribute : Attribute // TODO: Should this be a global attribute?
    {
        /// <summary>
        /// The type of the attribute to forward.
        /// </summary>
        public readonly Type AttributeType;

        /// <summary>
        /// Registers the specified attribute to be copied and applied to the AssetReference's UnityEngine.Object child instead.
        /// </summary>
        /// <param name="attributeType">The attribute type to forward.</param>
        public RegisterAssetReferenceAttributeForwardToChildAttribute(Type attributeType)
        {
            this.AttributeType = attributeType;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draws an AssetReference property.
    /// </summary>
    /// <typeparam name="T">The concrete type of AssetReference to be drawn. For example, <c>AssetReferenceTexture</c>.</typeparam>
    [DrawerPriority(0, 1, 0)]
    public class AssetReferenceDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
        where T: AssetReference
    {
        private bool hideAssetReferenceField;
        private Type targetType;
        private string NoneSelectedLabel;
        //private string[] labelRestrictions;
        private bool disallowSubAssets;
        private bool showSubAssetField;

        private bool updateShowSubAssetField;

        private List<AssetReferenceUIRestriction> restrictions;

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return property.GetAttribute<DrawWithUnityAttribute>() == null;
        }

        protected override void Initialize()
        {
            // If a child exists, we draw that child instead of the AssetReference field.
            if (this.Property.Children.Count > 0)
            {
                this.hideAssetReferenceField = true;
                return;
            }

            this.targetType = OdinAddressableUtility.GetAssetReferenceTargetType(typeof(T));

            if (this.targetType == typeof(UnityEngine.Object))
            {
                this.NoneSelectedLabel = "None (Addressable Asset)";
            }
            else
            {
                this.NoneSelectedLabel = $"None (Addressable {this.targetType.GetNiceName()})";
            }

            this.restrictions = new List<AssetReferenceUIRestriction>();
            foreach (var attr in this.Property.Attributes)
            {
                if (attr is AssetReferenceUIRestriction r)
                {
                    this.restrictions.Add(r);
                }
            }

            this.disallowSubAssets = Property.GetAttribute<DisallowAddressableSubAssetFieldAttribute>() != null;

            this.updateShowSubAssetField = true;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.hideAssetReferenceField == false)
            {
                var value = ValueEntry.SmartValue;

                // Update showSubAssetField.
                if (this.updateShowSubAssetField && Event.current.type == EventType.Layout)
                {
                    if (value == null || value.AssetGUID == null || value.editorAsset == null)
                    {
                        this.showSubAssetField = false;
                    }
                    else if (string.IsNullOrEmpty(value.SubObjectName) == false)
                    {
                        this.showSubAssetField = true;
                    }
                    else if (this.disallowSubAssets)
                    {
                        this.showSubAssetField = false;
                    }
                    else
                    {
                        var path = AssetDatabase.GUIDToAssetPath(value.AssetGUID);

                        if (path == null)
                        {
                            this.showSubAssetField = false;
                        }
                        else
                        {
                            this.showSubAssetField = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Length > 0;
                        }
                    }

                    this.updateShowSubAssetField = false;
                }

                var rect = SirenixEditorGUI.GetFeatureRichControlRect(label, out var controlId, out var _, out var valueRect);

                Rect mainRect = valueRect;
                Rect subRect = default, subPickerRect = default;

                if (this.showSubAssetField)
                {
                    subRect = mainRect.Split(1, 2).AddX(1);
                    mainRect = mainRect.Split(0, 2).SubXMax(1);
                    subPickerRect = subRect.AlignRight(16);
                }

                var mainPickerRect = mainRect.AlignRight(16);

                // Cursor
                EditorGUIUtility.AddCursorRect(mainPickerRect, MouseCursor.Link);
                if (showSubAssetField)
                {
                    EditorGUIUtility.AddCursorRect(subPickerRect, MouseCursor.Link);
                }

                // Selector
                if (GUI.Button(mainPickerRect, "", SirenixGUIStyles.None))
                {
                    OpenMainAssetSelector(valueRect);
                }
                if (showSubAssetField && GUI.Button(subPickerRect, "", SirenixGUIStyles.None))
                {
                    OpenSubAssetSelector(valueRect);
                }

                // Ping
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && mainRect.Contains(Event.current.mousePosition) && value != null && value.editorAsset != null)
                {
                    EditorGUIUtility.PingObject(value.editorAsset);
                }

                // Drag and drop
                EditorGUI.BeginChangeCheck();
                var drop = DragAndDropUtilities.DropZone<T>(rect, null, false, controlId);
                if (EditorGUI.EndChangeCheck())
                {
                    if (this.disallowSubAssets && string.IsNullOrEmpty(drop.SubObjectName) == false)
                    {
                        drop.SubObjectName = null;
                    }

                    this.updateShowSubAssetField = true; 
                    this.ValueEntry.SmartValue = drop;
                }

                // Drawing
                if (Event.current.type == EventType.Repaint)
                {
                    GUIContent valueLabel;
                    if (value == null || string.IsNullOrEmpty(value.AssetGUID) || value.editorAsset == null)
                    {
                        valueLabel = GUIHelper.TempContent(NoneSelectedLabel);
                    }
                    else
                    {
                        valueLabel = GUIHelper.TempContent(value.editorAsset.name, AssetPreview.GetMiniThumbnail(value.editorAsset));
                    }

                    GUI.Label(mainRect, valueLabel, EditorStyles.objectField);
                    SdfIcons.DrawIcon(mainPickerRect.SetWidth(12), SdfIconType.Record2);

                    if (this.showSubAssetField)
                    {
                        if (string.IsNullOrEmpty(value.SubObjectName) || value.editorAsset == null)
                        {
                            valueLabel = GUIHelper.TempContent("<none>");
                        }
                        else
                        {
                            valueLabel = GUIHelper.TempContent(value.SubObjectName);
                        }

                        GUI.Label(subRect, valueLabel, EditorStyles.objectField);
                        SdfIcons.DrawIcon(subPickerRect.SetWidth(12), SdfIconType.Record2);
                    }
                }
            }
            else
            {
                this.Property.Children[0].Draw(label);
            }
        }

        private void OpenMainAssetSelector(Rect rect)
        {
            var selector = new AddressableSelector("Select", this.targetType, this.restrictions, typeof(T));

            selector.SelectionChanged += OnMainAssetSelect;
            selector.SelectionConfirmed += OnMainAssetSelect;

            selector.ShowInPopup(rect);
        }

        private void OpenSubAssetSelector(Rect rect)
        {
            if (this.ValueEntry.SmartValue == null || this.ValueEntry.SmartValue.AssetGUID == null)
                return;

            var path = AssetDatabase.GUIDToAssetPath(this.ValueEntry.SmartValue.AssetGUID);
            if (path == null)
                return;

            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            var items = new GenericSelectorItem<UnityEngine.Object>[subAssets.Length + 1];
            items[0] = new GenericSelectorItem<UnityEngine.Object>("<none>", null);
            for (int i = 0; i < subAssets.Length; i++)
            {
                items[i + 1] = new GenericSelectorItem<UnityEngine.Object>(subAssets[i].name + " : " + subAssets[i].GetType().GetNiceName(), subAssets[i]);
            }

            var selector = new GenericSelector<UnityEngine.Object>("Select Sub Asset", false, items);

            selector.SelectionChanged += OnSubAssetSelect;
            selector.SelectionConfirmed += OnSubAssetSelect;

            selector.ShowInPopup(rect);
        }

        private void OnMainAssetSelect(IEnumerable<AddressableAssetEntry> selection)
        {
            var selected = selection.FirstOrDefault();
            this.ValueEntry.SmartValue = CreateAssetReferenceFrom(selected);
            this.updateShowSubAssetField = true;
        }

        private void OnSubAssetSelect(IEnumerable<UnityEngine.Object> selection)
        {
            if (this.ValueEntry == null || this.ValueEntry.SmartValue.AssetGUID == null)
                return;

            var selected = selection.FirstOrDefault();

            this.ValueEntry.SmartValue.SetEditorSubObject(selected);
        }

        private T CreateAssetReferenceFrom(AddressableAssetEntry entry)
        {
            if (entry != null)
            {
                return CreateAssetReferenceFrom(entry.TargetAsset);
            }
            else
            {
                return null;
            }
        }

        private T CreateAssetReferenceFrom(UnityEngine.Object obj)
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));

            if (guid == null) return null;

            var instance = (T)Activator.CreateInstance(typeof(T), guid);

            if (this.disallowSubAssets == false && AssetDatabase.IsSubAsset(obj))
            {
                instance.SetEditorSubObject(obj);
            }

            return instance;
        }

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            genericMenu.AddItem(new GUIContent("Set To Null"), false, () =>
            {
                this.ValueEntry.WeakSmartValue = null;
                this.updateShowSubAssetField = true;
            });

            if (this.ValueEntry.SmartValue != null && string.IsNullOrEmpty(this.ValueEntry.SmartValue.SubObjectName) == false)
            {
                genericMenu.AddItem(new GUIContent("Remove Sub Asset"), false, () =>
                {
                    if (this.ValueEntry.SmartValue != null)
                    {
                        this.ValueEntry.SmartValue.SetEditorSubObject(null);
                    }
                    this.updateShowSubAssetField = true;
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Remove Sub Asset"));
            }

            genericMenu.AddItem(new GUIContent("Open Groups Window"), false, OdinAddressableUtility.OpenGroupsWindow);
        }
    }

    /// <summary>
    /// Draws an AssetLabelReference field.
    /// </summary>
    [DrawerPriority(0, 1, 0)]
    public class AssetLabelReferenceDrawer : OdinValueDrawer<AssetLabelReference>, IDefinesGenericMenuItems
    {
        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return property.GetAttribute<DrawWithUnityAttribute>() == null;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var rect = SirenixEditorGUI.GetFeatureRichControlRect(label, out var controlId, out var hasKeyboardFocus, out var valueRect);

            string valueLabel;
            if (this.ValueEntry.SmartValue == null || string.IsNullOrEmpty(this.ValueEntry.SmartValue.labelString))
            {
                valueLabel = "<none>";
            }
            else
            {
                valueLabel = this.ValueEntry.SmartValue.labelString;
            }

            if (GUI.Button(valueRect, valueLabel, EditorStyles.popup))
            {
                var selector = new AddressableLabelSelector();

                selector.SelectionChanged += SetLabel;
                selector.SelectionConfirmed += SetLabel;

                selector.ShowInPopup(valueRect);
            }
        }

        private void SetLabel(IEnumerable<string> selection)
        {
            var selected = selection.FirstOrDefault();
            this.ValueEntry.SmartValue = new AssetLabelReference()
            {
                labelString = selected,
            };
        }

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            genericMenu.AddItem(new GUIContent("Set To Null"), false, () => property.ValueEntry.WeakSmartValue = null);
            genericMenu.AddItem(new GUIContent("Open Label Window"), false, () => OdinAddressableUtility.OpenLabelsWindow());
        }
    }

    /// <summary>
    /// Odin Selector for Addressables.
    /// </summary>
    public class AddressableSelector : OdinSelector<AddressableAssetEntry>
    {
        //private static EditorPrefBool flatten = new EditorPrefBool("AddressablesSelector.Flatten", false);

        private static EditorPrefEnum<SelectorListMode> listMode = new EditorPrefEnum<SelectorListMode>("AddressablesSelector.ListMode", SelectorListMode.Group);

        private readonly string title;
        private readonly Type filterType;
        private readonly List<AssetReferenceUIRestriction> restrictions;
        private readonly AssetReference assetReferenceForValidating;

        public override string Title => this.title;

        /// <summary>
        /// Initializes a AddressableSelector.
        /// </summary>
        /// <param name="title">The title of the selector. Set to null for no title.</param>
        /// <param name="filterType">The type of UnityEngine.Object to be selectable. For example, UnityEngine.Texture. For no restriction, set to UnityEngine.Object.</param>
        /// <param name="labelRestrictions">The Addressable labels to restrict the selector to. Set to null for no label restrictions.</param>
        /// <exception cref="ArgumentNullException">Throws if the filter type is null.</exception>
        public AddressableSelector(string title, Type filterType, List<AssetReferenceUIRestriction> restrictions, Type assetReferenceType)
        {
            this.title = title;
            this.filterType = filterType ?? throw new ArgumentNullException(nameof(filterType));
            this.restrictions = restrictions;

            if (assetReferenceType != null)
            {
                if (assetReferenceType.InheritsFrom<AssetReference>() == false)
                {
                    throw new ArgumentException("Must inherit AssetReference", nameof(assetReferenceType));
                }
                else if (assetReferenceType.IsAbstract)
                {
                    throw new ArgumentException("Cannot be abstract type.", nameof(assetReferenceType));
                }

                this.assetReferenceForValidating = (AssetReference)FormatterServices.GetUninitializedObject(assetReferenceType);
            }
        }

        protected override void DrawToolbar()
        {
            bool drawTitle = !string.IsNullOrEmpty(this.Title);
            bool drawSearchToolbar = this.SelectionTree.Config.DrawSearchToolbar;
            bool drawButton = this.DrawConfirmSelectionButton;

            if (drawTitle || drawSearchToolbar || drawButton)
            {
                SirenixEditorGUI.BeginHorizontalToolbar(this.SelectionTree.Config.SearchToolbarHeight);
                {
                    DrawToolbarTitle();
                    DrawToolbarSearch();
                    EditorGUI.DrawRect(GUILayoutUtility.GetLastRect().AlignLeft(1), SirenixGUIStyles.BorderColor);

                    SdfIconType icon;
                    if (listMode.Value == SelectorListMode.Path)
                        icon = SdfIconType.ListNested;
                    else if (listMode.Value == SelectorListMode.Group)
                        icon = SdfIconType.ListStars;
                    else if (listMode.Value == SelectorListMode.Flat)
                        icon = SdfIconType.List;
                    else
                        icon = SdfIconType.X;

                    if (SirenixEditorGUI.ToolbarButton(icon, true))
                    {
                        int m = (int)listMode.Value + 1;

                        if (m >= (int)SelectorListMode.Max)
                        {
                            m = 0;
                        }

                        listMode.Value = (SelectorListMode)m;

                        this.RebuildMenuTree();
                    }

                    if (SirenixEditorGUI.ToolbarButton(SdfIconType.GearFill, true))
                    {
                        OdinAddressableUtility.OpenGroupsWindow();
                    }

                    DrawToolbarConfirmButton();
                }
                SirenixEditorGUI.EndHorizontalToolbar();
            }
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            OdinMenuItem noneItem;
            if (filterType == typeof(UnityEngine.Object))
            {
                noneItem = new OdinMenuItem(tree, "<none> (Addressable Asset)", null);
            }
            else
            {
                noneItem = new OdinMenuItem(tree, $"<none> (Addressable {filterType.GetNiceName()})", null);
            }

            noneItem.SdfIcon = SdfIconType.X;
            tree.MenuItems.Add(noneItem);
            tree.Config.SelectMenuItemsOnMouseDown = true;

            if (AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;

                foreach (var group in settings.groups)
                {
                    if (group == null || group.name == "Built In Data") continue;

                    foreach (var entry in group.entries)
                    {
                        AddEntriesToTree(tree, group.name, entry);
                    }
                }
            }

            foreach (var i in tree.EnumerateTree().Skip(1))
            {
                if (i.Value == null)
                {
                    i.SdfIcon = SdfIconType.Folder;
                }
            }
        }

        private void AddEntriesToTree(OdinMenuTree tree, string groupName, AddressableAssetEntry entry)
        {
            var asset = entry.TargetAsset;

            if (entry.IsFolder == false && asset.GetType().InheritsFrom(filterType) && PassesRestrictions(entry))
            {
                string name;
                if (listMode.Value == SelectorListMode.Group)
                {
                    name = entry.address;
                }
                else if (listMode.Value == SelectorListMode.Path)
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(entry.AssetPath);
                }
                else if (listMode.Value == SelectorListMode.Flat)
                {
                    name = entry.address;
                }
                else
                {
                    throw new Exception("Unsupported list mode: " + listMode.Value);
                }

                var item = new OdinMenuItem(tree, name, entry)
                {
                    Icon = AssetPreview.GetMiniThumbnail(asset)
                };

                if (listMode.Value == SelectorListMode.Group)
                {
                    OdinMenuItem groupItem = tree.GetMenuItem(groupName);

                    if (groupItem == null)
                    {
                        groupItem = new OdinMenuItem(tree, groupName, null);
                        tree.MenuItems.Add(groupItem);
                    }

                    if (entry.ParentEntry != null && entry.ParentEntry.IsFolder)
                    {
                        OdinMenuItem folderItem = null;

                        for (int i = 0; i < groupItem.ChildMenuItems.Count; i++)
                        {
                            if (groupItem.ChildMenuItems[i].Name == entry.ParentEntry.address)
                            {
                                folderItem = groupItem.ChildMenuItems[i];
                                break;
                            }
                        }

                        if (folderItem == null)
                        {
                            folderItem = new OdinMenuItem(tree, entry.ParentEntry.address, null);
                            groupItem.ChildMenuItems.Add(folderItem);
                        }

                        folderItem.ChildMenuItems.Add(item);
                    }
                    else
                    {
                        groupItem.ChildMenuItems.Add(item);
                    }
                }
                else if (listMode.Value == SelectorListMode.Path)
                {
                    tree.AddMenuItemAtPath(System.IO.Path.GetDirectoryName(entry.AssetPath), item);
                }
                else if (listMode.Value == SelectorListMode.Flat)
                {
                    tree.MenuItems.Add(item);

                }
            }

            if (entry.IsFolder)
            {
                foreach (var e in entry.SubAssets)
                {
                    AddEntriesToTree(tree, groupName, e);
                }
            }
        }

        private bool PassesRestrictions(AddressableAssetEntry entry)
        {
            if (restrictions == null) return true;

            return OdinAddressableUtility.ValidateAssetReferenceRestrictions(restrictions, entry.MainAsset);

            //for (int i = 0; i < this.restrictions.Count; i++)
            //{
            //    if (this.restrictions[i].ValidateAsset(entry.AssetPath) == false)
            //    {
            //        return false;
            //    }
            //}

            //return true;

            /* If for whatever reason Unity haven't actually implemented their restriction methods, then we can use this code to atleast implement label restriction. */
            //if (this.labelRestrictions == null) return true;

            //for (int i = 0; i < labelRestrictions.Length; i++)
            //{
            //    if (entry.labels.Contains(labelRestrictions[i])) return true;
            //}

            //return false;
        }

        private enum SelectorListMode
        {
            Group,
            Path,
            Flat,

            Max,
        }
    }

    public class AddressableLabelSelector : OdinSelector<string>
    {
        protected override void DrawToolbar()
        {
            bool drawTitle = !string.IsNullOrEmpty(this.Title);
            bool drawSearchToolbar = this.SelectionTree.Config.DrawSearchToolbar;
            bool drawButton = this.DrawConfirmSelectionButton;

            if (drawTitle || drawSearchToolbar || drawButton)
            {
                SirenixEditorGUI.BeginHorizontalToolbar(this.SelectionTree.Config.SearchToolbarHeight);
                {
                    DrawToolbarTitle();
                    DrawToolbarSearch();
                    EditorGUI.DrawRect(GUILayoutUtility.GetLastRect().AlignLeft(1), SirenixGUIStyles.BorderColor);

                    if (SirenixEditorGUI.ToolbarButton(SdfIconType.GearFill, true))
                    {
                        OdinAddressableUtility.OpenLabelsWindow();
                    }

                    DrawToolbarConfirmButton();
                }
                SirenixEditorGUI.EndHorizontalToolbar();
            }
        }
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            IList<string> labels = null;

            if (AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                labels = settings.GetLabels();
            }

            if (labels == null) labels = Array.Empty<string>();

            tree.MenuItems.Add(new OdinMenuItem(tree, "<none>", null));

            for (int i = 0; i < labels.Count; i++)
            {
                tree.MenuItems.Add(new OdinMenuItem(tree, labels[i], labels[i]));
            }
        }
    }

    /// <summary>
    /// Resolves children for AssetReference properties, and implements the <c>RegisterAssetReferenceAttributeForwardToChild</c> behaviour.
    /// </summary>
    /// <typeparam name="T">The concrete type of AssetReference to be drawn. For example, <c>AssetReferenceTexture</c>.</typeparam>
    public class AssetReferencePropertyResolver<T> : OdinPropertyResolver<T>
        where T : AssetReference
    {
        private static readonly Type[] attributesToForward;

        static AssetReferencePropertyResolver()
        {
            attributesToForward = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetCustomAttributes<RegisterAssetReferenceAttributeForwardToChildAttribute>())
                .Cast<RegisterAssetReferenceAttributeForwardToChildAttribute>()
                .Select(x => x.AttributeType)
                .ToArray();
        }

        public override int ChildNameToIndex(string name)
        {
            return 0;
        }

        public override int ChildNameToIndex(ref StringSlice name)
        {
            return 0;
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            var targetType = OdinAddressableUtility.GetAssetReferenceTargetType(typeof(T));
            var getterSetterType = typeof(AssetReferenceValueGetterSetter<>).MakeGenericType(typeof(T), targetType);

            var getterSetter = Activator.CreateInstance(getterSetterType) as IValueGetterSetter;

            List<Attribute> attributes = new List<Attribute>
            {
                new ShowInInspectorAttribute(),
            };

            foreach (var type in attributesToForward)
            {
                var attr = this.Property.Attributes.FirstOrDefault(x => x.GetType() == type);
                if (attr != null)
                {
                    attributes.Add(attr);
                }
            }

            string label = "Asset";

            return InspectorPropertyInfo.CreateValue(label, 0, SerializationBackend.None, getterSetter, attributes);
        }

        protected override int GetChildCount(T value)
        {
            foreach (var attr in attributesToForward)
            {
                if (this.Property.Attributes.Any(x => x.GetType() == attr))
                {
                    return 1;
                }
            }

            return 0;
        }

        private class AssetReferenceValueGetterSetter<TTarget> : IValueGetterSetter<T, TTarget>
            where TTarget : UnityEngine.Object
        {
            public bool IsReadonly => false;

            public Type OwnerType => typeof(T);

            public Type ValueType => typeof(TTarget);

            public TTarget GetValue(ref T owner)
            {
                var v = owner.editorAsset;
                return v as TTarget;
            }

            public object GetValue(object owner)
            {
                var v = (owner as T)?.editorAsset;
                return v as TTarget;
            }

            public void SetValue(ref T owner, TTarget value)
            {
                owner.SetEditorAsset(value);
            }

            public void SetValue(object owner, object value)
            {
                (owner as T).SetEditorAsset(value as TTarget);
            }
        }
    }

    /// <summary>
    /// Processes attributes for AssetReference properties.
    /// </summary>
    /// <typeparam name="T">The concrete type of AssetReference to be drawn. For example, <c>AssetReferenceTexture</c>.</typeparam>
    public class AssetReferenceAttributeProcessor<T> : OdinAttributeProcessor<T>
        where T : AssetReference
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new DoNotDrawAsReferenceAttribute());
            attributes.Add(new HideReferenceObjectPickerAttribute());
            attributes.Add(new SuppressInvalidAttributeErrorAttribute()); // TODO: Remove this with proper attribute forwarding support.
        }
    }

    /// <summary>
    /// Processes attributes for AssetLabelReference properties.
    /// </summary>
    public class AssetLabelReferenceAttributeProcessor : OdinAttributeProcessor<AssetLabelReference>
    {
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new DoNotDrawAsReferenceAttribute());
            attributes.Add(new HideReferenceObjectPickerAttribute());
        }
    }

    /// <summary>
    /// Implements conversion behaviour for addressables.
    /// </summary>
    [InitializeOnLoad]
    internal class AssetReferenceConverter : ConvertUtility.ICustomConverter
    {
        private readonly Type type_AssetEntryTreeViewItem;
        private WeakValueGetter<AddressableAssetEntry> get_AssetEntryTreeViewItem_entry;

        static AssetReferenceConverter()
        {
            ConvertUtility.AddCustomConverter(new AssetReferenceConverter());
        }

        public AssetReferenceConverter()
        {
            this.type_AssetEntryTreeViewItem = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.GUI.AssetEntryTreeViewItem") ?? throw new Exception("Failed to find UnityEditor.AddressableAssets.GUI.AddressableAssetEntryTreeViewItem type.");
            var field_AssetEntryTreeViewItem_entry = type_AssetEntryTreeViewItem.GetField("entry", Flags.AllMembers) ?? throw new Exception("Failed to find entry field in UnityEditor.AddressableAssets.GUI.AddressableAssetEntryTreeViewItem type.");
            this.get_AssetEntryTreeViewItem_entry = EmitUtilities.CreateWeakInstanceFieldGetter<AddressableAssetEntry>(type_AssetEntryTreeViewItem, field_AssetEntryTreeViewItem_entry);
        }

        // UnityEngine.Object > AssetReference/T
        // AddressableAssetEntry > AssetReference
        // AssetReference/T > UnityEngine.Object
        // AssetReference/T > AssetReference/T
        // AddressableAssetEntry > UnityEngine.Object

        public bool CanConvert(Type from, Type to)
        {
            var comparer = FastTypeComparer.Instance;

            if (to.InheritsFrom(typeof(AssetReference)))
            {
                if (comparer.Equals(from, typeof(AddressableAssetEntry)) || comparer.Equals(from, type_AssetEntryTreeViewItem))
                {
                    return true;
                }
                else if (from.InheritsFrom<UnityEngine.Object>())
                {
                    if (to.InheritsFrom(typeof(AssetReferenceT<>)))
                    {
                        var baseType = to.GetGenericBaseType(typeof(AssetReferenceT<>));

                        var targetType = baseType.GetGenericArguments()[0];

                        return from.InheritsFrom(targetType);
                    }
                    else
                    {
                        return true;
                    }
                }
                else if (from.InheritsFrom(typeof(AssetReference)))
                {
                    return to.InheritsFrom(from);
                }
                else
                {
                    return false;
                }
            }
            else if (from.InheritsFrom(typeof(AssetReference)) && to.InheritsFrom<UnityEngine.Object>())
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        public bool TryConvert(object obj, Type to, out object result)
        {
            if (obj == null)
            {
                result = null;
                return false;
            }

            var comparer = FastTypeComparer.Instance;

            // AssetEntryTreeViewItems is a UI element container for AddressableAssetEntry.
            // With this we can just treat AssetEntryTreeViewItems as an AddressableAssetEntry.
            if (comparer.Equals(obj.GetType(), type_AssetEntryTreeViewItem))
            {
                obj = get_AssetEntryTreeViewItem_entry(ref obj);
            }

            if (to.InheritsFrom(typeof(AssetReference)))
            {
                Type assetType;
                if (to.InheritsFrom(typeof(AssetReferenceT<>)))
                {
                    var baseType = to.GetGenericBaseType(typeof(AssetReferenceT<>));
                    assetType = baseType.GetGenericArguments()[0];
                }
                else
                {
                    assetType = typeof(UnityEngine.Object);
                }

                if (obj is UnityEngine.Object uObj)
                {
                    if (obj.GetType().InheritsFrom(assetType))
                    {
                        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(uObj));

                        if (string.IsNullOrEmpty(guid) == false)
                        {

                            result = CreateReference(to, uObj);
                            return true;
                        }
                        else
                        {
                            result = null;
                            return false;
                        }
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else if (obj is AddressableAssetEntry entry)
                {
                    if (entry.TargetAsset.GetType().InheritsFrom(assetType))
                    {
                        result = CreateReference(to, entry.TargetAsset);
                        return true;

                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else if (obj is AssetReference reference)
                {
                    if (TryGetReferencedAsset(reference, assetType, out var asset))
                    {
                        result = CreateReference(to, asset);
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            else if (to.InheritsFrom(typeof(UnityEngine.Object)) && obj is AssetReference reference)
            {
                if (TryGetReferencedAsset(reference, to, out var asset))
                {
                    result = asset;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            else if (to.InheritsFrom(typeof(UnityEngine.Object)) && obj is AddressableAssetEntry entry)
            {
                var target = entry.TargetAsset;
                if (target == null)
                {
                    result = null;
                    return false;
                }
                else if (target.GetType().InheritsFrom(to))
                {
                    result = target;
                    return true;
                }
                else if (ConvertUtility.TryWeakConvert(target, to, out var converted))
                {
                    result = converted;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            else
            {
                result = null;
                return false;
            }
        }

        private bool TryGetReferencedAsset(AssetReference reference, Type to, out UnityEngine.Object asset)
        {
            if (reference.AssetGUID == null)
            {
                asset = null;
                return false;
            }

            var path = AssetDatabase.GUIDToAssetPath(reference.AssetGUID);

            if (reference.SubObjectName != null)
            {
                asset = null;
                var subassets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

                for (int i = 0; i < subassets.Length; i++)
                {
                    if (subassets[i].name == reference.SubObjectName)
                    {
                        asset = subassets[i];
                        break;
                    }
                }
            }
            else
            {
                asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }

            if (asset != null)
            {
                if (asset.GetType().InheritsFrom(to))
                {
                    return true;
                }
                else if (ConvertUtility.TryWeakConvert(asset, to, out var converted))
                {
                    asset = (UnityEngine.Object)converted;
                    return true;
                }
                else
                {
                    asset = null;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private AssetReference CreateReference(Type type, UnityEngine.Object obj)
        {
            var reference = (AssetReference)Activator.CreateInstance(type, new string[] { AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)) });
            if (AssetDatabase.IsSubAsset(obj))
            {
                reference.SetEditorAsset(obj);
            }

            return reference;
        }
    }

    /// <summary>
    /// Odin Inspector utility methods for working with addressables.
    /// </summary>
    public static class OdinAddressableUtility
    {
        private readonly static Action openAddressableWindowAction;

        static OdinAddressableUtility()
        {
            var type = TwoWaySerializationBinder.Default.BindToType("UnityEditor.AddressableAssets.GUI.AddressableAssetsWindow") ?? throw new Exception("");
            var method = type.GetMethod("Init", Flags.AllMembers) ?? throw new Exception("");
            openAddressableWindowAction = (Action)Delegate.CreateDelegate(typeof(Action), method);

        }

        /// <summary>
        /// Opens the addressables group settings window.
        /// </summary>
        public static void OpenGroupsWindow()
        {
            openAddressableWindowAction();
        }

        /// <summary>
        /// Opens the addressables labels settings window.
        /// </summary>
        public static void OpenLabelsWindow()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists) return;
            EditorWindow.GetWindow<LabelWindow>().Intialize(AddressableAssetSettingsDefaultObject.Settings);
        }

        /// <summary>
        /// Converts the specified object into an addressable.
        /// </summary>
        /// <param name="obj">The object to make addressable.</param>
        /// <param name="group">The addressable group to add the object to.</param>
        public static void MakeAddressable(UnityEngine.Object obj, AddressableAssetGroup group)
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists) return;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = AssetDatabase.GUIDToAssetPath(guid);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryCreated, entry, false, true);
        }

        /// <summary>
        /// Gets the type targeted by an AssetReference type. For example, returns Texture for AssetReferenceTexture.
        /// Returns UnityEngine.Object for AssetReference type.
        /// </summary>
        /// <param name="assetReferenceType">A type of AssetReference, for example, AssetReferenceTexture.</param>
        /// <returns>
        /// If the given type inherits AssetRefernceT&lt;T&gt;, then the method returns the generic T argument.
        /// If the given type is AssetReference, then the method returns UnityEngine.Object.
        /// </returns>
        /// <exception cref="ArgumentNullException">Throws if given parameter is null.</exception>
        /// <exception cref="ArgumentException">Throws if the given type does not inherit or is AssetReference.</exception>
        public static Type GetAssetReferenceTargetType(Type assetReferenceType)
        {
            if (assetReferenceType == null) throw new ArgumentNullException(nameof(assetReferenceType));

            if (assetReferenceType.InheritsFrom(typeof(AssetReferenceT<>)))
            {
                var genericBase = assetReferenceType.GetGenericBaseType(typeof(AssetReferenceT<>));
                return genericBase.GetGenericArguments()[0];
            }
            else
            {
                return typeof(UnityEngine.Object);
            }
        }

        /// <summary>
        /// Validate an asset against a list of AssetReferenceUIRestrictions.
        /// </summary>
        /// <param name="restrictions">The restrictions to apply.</param>
        /// <param name="asset">The asset to validate.</param>
        /// <returns>Returns true if the asset passes all restrictions. Otherwise false.</returns>
        /// <exception cref="Exception">Throws if Addressable Settings have not been created.</exception>
        /// <exception cref="ArgumentNullException">Throws if restrictions or asset is null.</exception>
        public static bool ValidateAssetReferenceRestrictions(List<AssetReferenceUIRestriction> restrictions, UnityEngine.Object asset)
        {
            return ValidateAssetReferenceRestrictions(restrictions, asset, out _);
        }

        /// <summary>
        /// Validate an asset against a list of AssetReferenceUIRestrictions.
        /// </summary>
        /// <param name="restrictions">The restrictions to apply.</param>
        /// <param name="asset">The asset to validate.</param>
        /// <param name="failedRestriction">The first failed restriction. <c>null</c> if no restrictions failed.</param>
        /// <returns>Returns true if the asset passes all restrictions. Otherwise false.</returns>
        /// <exception cref="Exception">Throws if Addressable Settings have not been created.</exception>
        /// <exception cref="ArgumentNullException">Throws if restrictions or asset is null.</exception>
        public static bool ValidateAssetReferenceRestrictions(List<AssetReferenceUIRestriction> restrictions, UnityEngine.Object asset, out AssetReferenceUIRestriction failedRestriction)
        {
            if (AddressableAssetSettingsDefaultObject.SettingsExists == false) throw new Exception("Addressable Settings have not been created.");

            _ = restrictions ?? throw new ArgumentNullException(nameof(restrictions));
            _ = asset ?? throw new ArgumentNullException(nameof(asset));

            for (int i = 0; i < restrictions.Count; i++)
            {
                if (restrictions[i] is AssetReferenceUILabelRestriction labels)
                {
                    /* Unity, in all its wisdom, have apparently decided not to implement their AssetReferenceRestriction attributes in some versions(?)
                     * So, to compensate, we're going to manually validate the label restriction attribute, so atleast that works. */

                    var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));

                    var entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guid, true);

                    if (entry.labels.Any(x => labels.m_AllowedLabels.Contains(x)) == false)
                    {
                        failedRestriction = labels;
                        return false;
                    }
                }
                else if (restrictions[i].ValidateAsset(asset) == false)
                {
                    failedRestriction = restrictions[i];
                    return false;
                }
            }

            failedRestriction = null;
            return true;
        }
    }
#endif
}