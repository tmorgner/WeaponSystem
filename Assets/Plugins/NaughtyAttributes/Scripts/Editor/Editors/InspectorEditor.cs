using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NaughtyAttributes.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class InspectorEditor : UnityEditor.Editor
    {
        private SerializedProperty script;

        private List<FieldInfo> fields;
        private readonly HashSet<FieldInfo> groupedFields;
        private readonly Dictionary<string, List<FieldInfo>> groupedFieldsByGroupName;
        private List<FieldInfo> nonSerializedFields;
        private List<PropertyInfo> nativeProperties;
        private List<MethodInfo> methods;

        private readonly Dictionary<string, SerializedProperty> serializedPropertiesByFieldName;

        private bool useDefaultInspector;

        bool FilterFields(FieldInfo f) => this.serializedObject.FindProperty(f.Name) != null;
        readonly Func<FieldInfo, bool> filterFieldsDelegate;
        
        // Cache non-serialized fields
        bool NonSerializedFieldsFilter(FieldInfo f)
        {
            return f.GetCustomAttribute<DrawerAttribute>(true) != null && this.serializedObject.FindProperty(f.Name) == null;
        }

        
        public InspectorEditor()
        {
            groupedFieldsByGroupName = new Dictionary<string, List<FieldInfo>>();
            serializedPropertiesByFieldName = new Dictionary<string, SerializedProperty>();
            groupedFields = new HashSet<FieldInfo>();
            fields = new List<FieldInfo>(100);
            nonSerializedFields = new List<FieldInfo>(100);
            nativeProperties = new List<PropertyInfo>(100);
            methods = new List<MethodInfo>(100);
            filterFieldsDelegate = FilterFields;
        }

        private void OnEnable()
        {
            try
            {
                this.script = this.serializedObject.FindProperty("m_Script");
            }
            catch
            {
                // ignore. Unity Bug causes NPE deep inside the unity classes.
                this.useDefaultInspector = true;
                return;
            }

            // Cache serialized fields
            this.fields = ReflectionUtility.GetAllFieldsEfficiently(this.target, filterFieldsDelegate, this.fields);
            // Cache serialized properties by field name
            this.serializedPropertiesByFieldName.Clear();
            foreach (var field in this.fields)
            {
                this.serializedPropertiesByFieldName[field.Name] = this.serializedObject.FindProperty(field.Name);
            }

            // If there are no NaughtyAttributes use default inspector
            if (this.fields.All(f => f.GetCustomAttribute<NaughtyAttribute>(true) == null))
            {
                this.useDefaultInspector = true;
            }
            else
            {
                this.useDefaultInspector = false;

                // Cache grouped fields
                this.groupedFields.Clear();
                foreach (var fi in fields)
                {
                    if (fi.GetCustomAttribute<GroupAttribute>(true) != null)
                    {
                        this.groupedFields.Add(fi);
                    }
                }
                
                // Cache grouped fields by group name
                groupedFieldsByGroupName.Clear();
                foreach (var groupedField in this.groupedFields)
                {
                    string groupName = (groupedField.GetCustomAttribute<GroupAttribute>(true)).Name;

                    if (this.groupedFieldsByGroupName.TryGetValue(groupName, out var list))
                    {
                        list.Add(groupedField);
                    }
                    else
                    {
                        this.groupedFieldsByGroupName[groupName] = new List<FieldInfo>()
                        {
                            groupedField
                        };
                    }
                }

            }

            this.nonSerializedFields = ReflectionUtility.GetAllFieldsEfficiently(this.target, NonSerializedFieldsFilter, nonSerializedFields);

            // Cache the native properties
            this.nativeProperties = ReflectionUtility.GetAllPropertiesEfficiently(
                this.target, p => p.GetCustomAttribute<DrawerAttribute>(true) != null, nativeProperties);

            // Cache methods with DrawerAttribute
            this.methods = ReflectionUtility.GetAllMethodsEfficiently(
                this.target, m => m.GetCustomAttribute<DrawerAttribute>(true) != null, methods);
        }

        private void OnDisable()
        {
            PropertyDrawerDatabase.ClearCache();
        }

        public override bool RequiresConstantRepaint()
        {
            return useDefaultInspector == false;
        }

        public override void OnInspectorGUI()
        {
            if (this.useDefaultInspector)
            {
                this.DrawDefaultInspector();
            }
            else
            {
                this.serializedObject.Update();

                if (this.script != null)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(this.script);
                    GUI.enabled = true;
                }

                // Draw fields
                HashSet<string> drawnGroups = new HashSet<string>();
                foreach (var field in this.fields)
                {
                    if (this.groupedFields.Contains(field))
                    {
                        // Draw grouped fields
                        string groupName = (field.GetCustomAttributes(typeof(GroupAttribute), true)[0] as GroupAttribute).Name;
                        if (!drawnGroups.Contains(groupName))
                        {
                            drawnGroups.Add(groupName);

                            PropertyGrouper grouper = this.GetPropertyGrouperForField(field);
                            if (grouper != null)
                            {
                                grouper.BeginGroup(groupName);

                                this.ValidateAndDrawFields(this.groupedFieldsByGroupName[groupName]);

                                grouper.EndGroup();
                            }
                            else
                            {
                                this.ValidateAndDrawFields(this.groupedFieldsByGroupName[groupName]);
                            }
                        }
                    }
                    else
                    {
                        // Draw non-grouped field
                        this.ValidateAndDrawField(field);
                    }
                }

                this.serializedObject.ApplyModifiedProperties();
            }

            // Draw non-serialized fields
            foreach (var field in this.nonSerializedFields)
            {
                DrawerAttribute drawerAttribute = (DrawerAttribute)field.GetCustomAttributes(typeof(DrawerAttribute), true)[0];
                FieldDrawer drawer = FieldDrawerDatabase.GetDrawerForAttribute(drawerAttribute.GetType());
                if (drawer != null)
                {
                    drawer.DrawField(this.target, field);
                }
            }

            // Draw native properties
            foreach (var property in this.nativeProperties)
            {
                DrawerAttribute drawerAttribute = (DrawerAttribute)property.GetCustomAttributes(typeof(DrawerAttribute), true)[0];
                NativePropertyDrawer drawer = NativePropertyDrawerDatabase.GetDrawerForAttribute(drawerAttribute.GetType());
                if (drawer != null)
                {
                    drawer.DrawNativeProperty(this.target, property);
                }
            }

            // Draw methods
            foreach (var method in this.methods)
            {
                DrawerAttribute drawerAttribute = (DrawerAttribute)method.GetCustomAttributes(typeof(DrawerAttribute), true)[0];
                MethodDrawer methodDrawer = MethodDrawerDatabase.GetDrawerForAttribute(drawerAttribute.GetType());
                if (methodDrawer != null)
                {
                    
                    if (methodDrawer.DrawMethod(this.target, method))
                    {
                        this.serializedObject.ApplyModifiedProperties();
                        this.serializedObject.Update();
                        PropertyDrawerDatabase.ClearCache();
                    }
                }
            }
        }

        private void ValidateAndDrawFields(IEnumerable<FieldInfo> fields)
        {
            foreach (var field in fields)
            {
                this.ValidateAndDrawField(field);
            }
        }

        private void ValidateAndDrawField(FieldInfo field)
        {
            this.ValidateField(field);
            this.ApplyFieldMeta(field);
            this.DrawField(field);
        }

        private void ValidateField(FieldInfo field)
        {
            ValidatorAttribute[] validatorAttributes = (ValidatorAttribute[])field.GetCustomAttributes(typeof(ValidatorAttribute), true);

            foreach (var attribute in validatorAttributes)
            {
                PropertyValidator validator = PropertyValidatorDatabase.GetValidatorForAttribute(attribute.GetType());
                if (validator != null)
                {
                    validator.ValidateProperty(this.serializedPropertiesByFieldName[field.Name]);
                }
            }
        }

        private void DrawField(FieldInfo field)
        {
            // Check if the field has draw conditions
            PropertyDrawCondition drawCondition = this.GetPropertyDrawConditionForField(field);
            if (drawCondition != null)
            {
                bool canDrawProperty = drawCondition.CanDrawProperty(this.serializedPropertiesByFieldName[field.Name]);
                if (!canDrawProperty)
                {
                    return;
                }
            }

            // Check if the field has HideInInspectorAttribute
            HideInInspector[] hideInInspectorAttributes = (HideInInspector[])field.GetCustomAttributes(typeof(HideInInspector), true);
            if (hideInInspectorAttributes.Length > 0)
            {
                return;
            }

            // Draw the field
            EditorGUI.BeginChangeCheck();
            PropertyDrawer drawer = this.GetPropertyDrawerForField(field);
            if (drawer != null)
            {
                drawer.DrawProperty(field, this.serializedPropertiesByFieldName[field.Name]);
            }
            else
            {
                EditorDrawUtility.DrawPropertyField(this.serializedPropertiesByFieldName[field.Name]);
            }

            if (EditorGUI.EndChangeCheck())
            {
                OnValueChangedAttribute[] onValueChangedAttributes = (OnValueChangedAttribute[])field.GetCustomAttributes(typeof(OnValueChangedAttribute), true);
                foreach (var onValueChangedAttribute in onValueChangedAttributes)
                {
                    PropertyMeta meta = PropertyMetaDatabase.GetMetaForAttribute(onValueChangedAttribute.GetType());
                    if (meta != null)
                    {
                        meta.ApplyPropertyMeta(this.serializedPropertiesByFieldName[field.Name], onValueChangedAttribute);
                    }
                }
            }
        }

        private void ApplyFieldMeta(FieldInfo field)
        {
            // Apply custom meta attributes
            MetaAttribute[] metaAttributes = field
                .GetCustomAttributes(typeof(MetaAttribute), true)
                .Where(attr => attr.GetType() != typeof(OnValueChangedAttribute))
                .Select(obj => obj as MetaAttribute)
                .ToArray();

            Array.Sort(metaAttributes, (x, y) =>
            {
                return x.Order - y.Order;
            });

            foreach (var metaAttribute in metaAttributes)
            {
                PropertyMeta meta = PropertyMetaDatabase.GetMetaForAttribute(metaAttribute.GetType());
                if (meta != null)
                {
                    meta.ApplyPropertyMeta(this.serializedPropertiesByFieldName[field.Name], metaAttribute);
                }
            }
        }

        private PropertyDrawer GetPropertyDrawerForField(FieldInfo field)
        {
            DrawerAttribute[] drawerAttributes = (DrawerAttribute[])field.GetCustomAttributes(typeof(DrawerAttribute), true);
            if (drawerAttributes.Length > 0)
            {
                PropertyDrawer drawer = PropertyDrawerDatabase.GetDrawerForAttribute(drawerAttributes[0].GetType());
                return drawer;
            }
            else
            {
                return null;
            }
        }

        private PropertyGrouper GetPropertyGrouperForField(FieldInfo field)
        {
            GroupAttribute[] groupAttributes = (GroupAttribute[])field.GetCustomAttributes(typeof(GroupAttribute), true);
            if (groupAttributes.Length > 0)
            {
                PropertyGrouper grouper = PropertyGrouperDatabase.GetGrouperForAttribute(groupAttributes[0].GetType());
                return grouper;
            }
            else
            {
                return null;
            }
        }

        private PropertyDrawCondition GetPropertyDrawConditionForField(FieldInfo field)
        {
            DrawConditionAttribute[] drawConditionAttributes = (DrawConditionAttribute[])field.GetCustomAttributes(typeof(DrawConditionAttribute), true);
            if (drawConditionAttributes.Length > 0)
            {
                PropertyDrawCondition drawCondition = PropertyDrawConditionDatabase.GetDrawConditionForAttribute(drawConditionAttributes[0].GetType());
                return drawCondition;
            }
            else
            {
                return null;
            }
        }
    }
}
