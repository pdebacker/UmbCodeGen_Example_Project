﻿namespace Movies.Models
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ComponentModel.DataAnnotations;		// requires System.ComponentModel.DataAnnotations reference

    using umbraco.interfaces;
    using umbraco.MacroEngines;
    using umbraco.NodeFactory;

    using UmbCodeGen.CodeGen;

    public partial class ModelFactory
    {
        #region Public Methods

        public static T CreateModel<T>(INode node) where T : class, new()
        {
            var modelFactory = new ModelFactory();
            return modelFactory.CreateModelInternal<T>(node);
        }

        public static void FillModel(object model, INode node)
        {
            var modelFactory = new ModelFactory();
            modelFactory.FillModelInternal(model, node);
        }

        #endregion
        #region Protected Methods

        private T CreateModelInternal<T>(INode node) where T : class, new()
        {
            var model = new T();
            FillModelInternal(model, node);
            return model;
        }

        private void FillModelInternal(object model, INode node)
        {
            foreach (PropertyInfo property in model.GetType().GetProperties())
            {
                var propertyAttrib = property.GetCustomAttributes(true).FirstOrDefault(a => a.GetType().Name.Equals("PropertyAttribute")) as PropertyAttribute;
                if (propertyAttrib != null)
                {
                    string modelType = GetPropertyType(property);

                    if (property.PropertyType.IsClass == false || property.PropertyType.IsEnum || property.PropertyType.Name == "String")
                    {
                        FillModelProperty(model, property, modelType, propertyAttrib.Alias, node);
                    }
                    else if (property.PropertyType.IsClass && property.PropertyType.IsGenericType == false)
                    {
                        switch (modelType)
                        {
                            case "Hyperlink": FillHyperLinkValue(model, property, propertyAttrib.Alias, node); break;
                            case "HyperlinkList": FillHyperLinkListValue(model, property, propertyAttrib.Alias, node); break;
                            case "MediaInfo": FillMediaInfoValue(model, property, propertyAttrib.Alias, node); break;
                            case "DataGrid": FillDataGridValue(model, property, propertyAttrib.Alias, node); break;
                            default: FillModelPropertyClass(model, property, propertyAttrib.Alias, node); break;
                        }
                    }
                    else if (property.PropertyType.IsClass && property.PropertyType.IsGenericType == true)
                    {
                        FillModelPropertyList(model, property, propertyAttrib.Alias, node);
                    }
                }
                else
                {
                    var childrenAttrib = property.GetCustomAttributes(true).FirstOrDefault(a => a.GetType().Name.Equals("ChildrenAttribute")) as ChildrenAttribute;
                    if (childrenAttrib != null && (property.PropertyType.IsClass || property.PropertyType.IsInterface))
                    {
                        if (property.PropertyType.IsGenericType)
                        {
                            FillModelPropertyChildList(model, property, childrenAttrib.NodeAlias, childrenAttrib.AllDescendants, node);
                        }
                        else
                        {
                            FillModelPropertyChild(model, property, childrenAttrib.NodeAlias, childrenAttrib.AllDescendants, node);
                        }
                    }
                }
            }
        }

        private string GetPropertyType(PropertyInfo property)
        {
            switch (property.PropertyType.Name)
            {
                case "String": return "string";
                case "Int32": return "int";
                case "Int64": return "int";
                case "Boolean": return "bool";
                case "Double": return "double";
                case "DateTime": return "DateTime";
            }

            if (property.PropertyType.IsEnum)
                return "enum";

            if (property.PropertyType.BaseType != null && property.PropertyType.BaseType.Name.Equals("DataGrid"))
                return "DataGrid";

            return property.PropertyType.Name;
        }

        private void FillModelPropertyClass(object model, PropertyInfo property, string alias, INode node)
        {
            ConstructorInfo ctor = property.PropertyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], null);
            if (ctor != null)
            {
                var instance = ctor.Invoke(BindingFlags.Public, null, new object[0], null);
                if (instance != null)
                {
                    var referenceNode = new Node(GetPropertyInt(node, alias));
                    FillModelInternal(instance, referenceNode);
                    property.SetValue(model, instance, BindingFlags.SetProperty, null, null, null);
                }
            }
        }

        private void FillModelPropertyList(object model, PropertyInfo property, string alias, INode node)
        {
            if (property.PropertyType.GetGenericArguments().Length == 1)
            {
                Type type = property.PropertyType.GetGenericArguments()[0];
                if (type.IsEnum)
                {
                    FillEnumListValue(model, property, alias, node);
                }
                else
                {
                    switch (type.Name)
                    {
                        case "Hyperlink": this.FillHyperLinkList(model, property, alias, node); break;
                        case "DateTime": this.FillDateTimeList(model, property, alias, node); break;
                        case "String": this.FillStringList(model, property, alias, node); break;
                        default: this.FillIntList(model, property, alias, node); break;
                    }
                }
            }
        }

        private void FillIntList(object model, PropertyInfo property, string alias, INode node)
        {
            if (property.PropertyType.GetGenericArguments().Length == 1)
            {
                Type type = property.PropertyType.GetGenericArguments()[0];
                ConstructorInfo ctor = property.PropertyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], null);
                if (ctor != null)
                {
                    var instance = ctor.Invoke(BindingFlags.Public, null, new object[0], null) as IList;
                    if (instance != null)
                    {
                        ConstructorInfo ctorItem = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], null);
                        IntList intList = GetIntListValue(node, alias);
                        if (intList != null)
                        {
                            foreach (int id in intList)
                            {
                                if (type.IsClass)
                                {
                                    var instanceItem = ctorItem.Invoke(BindingFlags.Public, null, new object[0], null);
                                    var referenceNode = new Node(id);
                                    FillModelInternal(instanceItem, referenceNode);
                                    instance.Add(instanceItem);
                                }
                                else
                                {
                                    instance.Add(id);
                                }
                            }
                        }
                        property.SetValue(model, instance, BindingFlags.SetProperty, null, null, null);
                    }
                }
            }
        }

        private void FillStringList(object model, PropertyInfo property, string alias, INode node)
        {
            if (property.PropertyType.GetGenericArguments().Length == 1)
            {
                StringList stringList = GetStringListValue(node, alias);
                if (stringList != null)
                    property.SetValue(model, stringList, BindingFlags.SetProperty, null, null, null);
            }
        }

        private void FillHyperLinkList(object model, PropertyInfo property, string alias, INode node)
        {
            if (property.PropertyType.GetGenericArguments().Length == 1)
            {
                Type type = property.PropertyType.GetGenericArguments()[0];
                ConstructorInfo ctor = property.PropertyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], null);
                if (ctor != null)
                {
                    HyperlinkList hyperlinkList = this.GetHyperLinkList(node, alias);
                    var instance = ctor.Invoke(BindingFlags.Public, null, new object[0], null) as IList;
                    if (instance != null && hyperlinkList.Count > 0)
                    {
                        foreach (Hyperlink hyperlink in hyperlinkList)
                            instance.Add(hyperlink);

                        property.SetValue(model, instance, BindingFlags.SetProperty, null, null, null);
                    }
                }
            }
        }

        private void FillDateTimeList(object model, PropertyInfo property, string alias, INode node)
        {
            if (property.PropertyType.GetGenericArguments().Length == 1)
            {
                string values = GetPropertyString(node, alias);
                if (!string.IsNullOrEmpty(values))
                {
                    var result = new List<DateTime>();
                    string[] dates = values.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var date in dates)
                    {
                        DateTime dt;
                        if (DateTime.TryParseExact(date, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out dt))
                        {
                            result.Add(dt);
                        }
                    }
                    property.SetValue(model, result, BindingFlags.SetProperty, null, null, null);
                }
            }
        }

        private void FillModelPropertyChild(object model, PropertyInfo property, string alias, bool allDescendants, INode node)
        {
            ConstructorInfo ctor = property.PropertyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], null);
            if (ctor != null)
            {
                var instance = ctor.Invoke(BindingFlags.Public, null, new object[0], null);
                if (instance != null)
                {
                    INode childNode = this.GetDescendants(node, alias, allDescendants).FirstOrDefault();
                    if (childNode != null)
                    {
                        FillModelInternal(instance, childNode);
                        property.SetValue(model, instance, BindingFlags.SetProperty, null, null, null);
                    }
                }
            }
        }

        private void FillModelPropertyChildList(object model, PropertyInfo property, string alias, bool allDescendants, INode node)
        {
            if (property.PropertyType.GetGenericArguments().Length == 1)
            {
                Type type = property.PropertyType.GetGenericArguments()[0];
                ConstructorInfo ctor = property.PropertyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], null);
                if (ctor != null)
                {
                    var listInstance = ctor.Invoke(BindingFlags.Public, null, new object[0], null) as IList;
                    if (listInstance != null)
                    {
                        foreach (INode childNode in this.GetDescendants(node, alias, allDescendants))
                        {
                            ConstructorInfo ctorItem = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], null);
                            var instanceItem = ctorItem.Invoke(BindingFlags.Public, null, new object[0], null);
                            FillModelInternal(instanceItem, childNode);
                            listInstance.Add(instanceItem);
                        }
                        property.SetValue(model, listInstance, BindingFlags.SetProperty, null, null, null);
                    }
                }
            }
        }

        private IEnumerable<INode> GetDescendants(INode node, string alias, bool allDescendants)
        {
            foreach (INode child in node.ChildrenAsList)
            {
                if (child.NodeTypeAlias.Equals(alias))
                    yield return child;

                if (allDescendants)
                {
                    foreach (INode grandChild in GetDescendants(child, alias, allDescendants))
                    {
                        yield return grandChild;
                    }
                }
            }
            yield break;
        }

        private void FillModelProperty(object model, PropertyInfo property, string type, string alias, INode node)
        {
            switch (type)
            {
                case "string": FillStringValue(model, property, alias, node); break;
                case "int": FillIntValue(model, property, alias, node); break;
                case "bool": FillBoolValue(model, property, alias, node); break;
                case "double": FillDoubleValue(model, property, alias, node); break;
                case "DateTime": FillDateTimeValue(model, property, alias, node); break;
            }
            if (property.PropertyType.IsEnum)
                FillEnumValue(model, property, alias, node);

        }

        private void FillEnumValue(object model, PropertyInfo property, string alias, INode node)
        {
            string value;
            value = GetPropertyString(node, alias);
            if (!string.IsNullOrEmpty(value))
            {
                object enumValue = GetEnumValue(value, property.PropertyType);
                if (enumValue != null)
                    property.SetValue(model, enumValue, BindingFlags.SetProperty, null, null, null);
            }
        }

        private void FillEnumListValue(object model, PropertyInfo property, string alias, INode node)
        {
            string values;
            values = GetPropertyString(node, alias);
            if (!string.IsNullOrEmpty(values))
            {
                if (property.PropertyType.GetGenericArguments().Length == 1)
                {
                    Type type = property.PropertyType.GetGenericArguments()[0];
                    ConstructorInfo ctor = property.PropertyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[0], null);
                    if (ctor != null)
                    {
                        var instance = ctor.Invoke(BindingFlags.Public, null, new object[0], null) as IList;
                        if (instance != null)
                        {
                            string[] splittedValues = values.Split(new char[] { ',' });
                            foreach (string value in splittedValues)
                            {
                                object enumValue = GetEnumValue(value, type);
                                if (enumValue != null)
                                    instance.Add(enumValue);
                            }

                            property.SetValue(model, instance, BindingFlags.SetProperty, null, null, null);
                        }
                    }
                }
            }
        }

        private object GetEnumValue(string value, Type type)
        {
            if (!string.IsNullOrEmpty(value))
            {
                string enumValueString = Naming.IdentifierName(value);
                if (type.IsEnumDefined(enumValueString))
                {
                    object enumValue = Enum.Parse(type, enumValueString);
                    return enumValue;
                }
                else
                {
                    int enumValueInt = 0;
                    if (int.TryParse(value, out enumValueInt))
                        return enumValueInt;
                }
            }
            return null;
        }


        private void FillStringValue(object model, PropertyInfo property, string alias, INode node)
        {
            string value;
            if (alias.Equals("NodeName"))
                value = node.Name;
            else if (alias.Equals("NodeUrl"))
                value = node.NiceUrl;
            else
                value = GetPropertyString(node, alias);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private void FillIntValue(object model, PropertyInfo property, string alias, INode node)
        {
            int value;
            if (alias.Equals("NodeId"))
                value = node.Id;
            else
                value = GetPropertyInt(node, alias);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private void FillDoubleValue(object model, PropertyInfo property, string alias, INode node)
        {
            double value = GetPropertyDouble(node, alias);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private void FillBoolValue(object model, PropertyInfo property, string alias, INode node)
        {
            bool value = GetPropertyBool(node, alias);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private void FillDateTimeValue(object model, PropertyInfo property, string alias, INode node)
        {
            string format = GetDateTimeFormat(property);
            DateTime value = GetPropertyDateTime(node, alias, format);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private string GetDateTimeFormat(PropertyInfo property)
        {
            string format = null;
            if (property != null)
            {
                var dateTimeFormatAttrib = property.GetCustomAttributes(true).FirstOrDefault(a => a.GetType().Name.Equals("DateTimeFormatAttribute")) as DateTimeFormatAttribute;
                if (dateTimeFormatAttrib == null)
                {
                    MetadataTypeAttribute metadataType = property.DeclaringType.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault() as MetadataTypeAttribute;
                    if (metadataType != null)
                    {
                        var metaProperty = metadataType.MetadataClassType.GetProperty(property.Name);
                        if (metaProperty != null)
                        {
                            dateTimeFormatAttrib = metaProperty.GetCustomAttributes(true).FirstOrDefault(a => a.GetType().Name.Equals("DateTimeFormatAttribute")) as DateTimeFormatAttribute;
                            if (dateTimeFormatAttrib != null)
                            {
                                format = dateTimeFormatAttrib.DateTimeFormat;
                            }
                        }
                    }
                }
                else
                {
                    format = dateTimeFormatAttrib.DateTimeFormat;
                }
            }
            return format;
        }

        private void FillMediaInfoValue(object model, PropertyInfo property, string alias, INode node)
        {
            MediaInfo value = GetMedia(node, alias);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private void FillHyperLinkListValue(object model, PropertyInfo property, string alias, INode node)
        {
            HyperlinkList value = GetHyperLinkList(node, alias);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private void FillHyperLinkValue(object model, PropertyInfo property, string alias, INode node)
        {
            Hyperlink value = GetHyperLink(node, alias);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private void FillXmlTypeValue<T>(object model, PropertyInfo property, string alias, INode node) where T : class, new()
        {
            T value = GetXmlType<T>(node, alias);
            property.SetValue(model, value, BindingFlags.SetProperty, null, null, null);
        }

        private void FillDataGridValue(object model, PropertyInfo property, string alias, INode node)
        {
            string methodName = "Get" + property.PropertyType.Name;

            var grid = this.GetType().InvokeMember(methodName,
                                                    BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
                                                    null,
                                                    this,
                                                    new object[] { node, alias });
            if (grid != null)
            {
                property.SetValue(model, grid, BindingFlags.SetProperty, null, null, null);
            }
        }
        #endregion

        #region Complex Get methods

        private IntList GetIntListValue(INode node, string alias)
        {
            IntList intList = null;
            string xml = node.GetProperty(alias).Value;
            if (!string.IsNullOrEmpty(xml))
            {
                if (xml.StartsWith("<MultiNodePicker"))
                {
                    var picker = Deserialize<MultiNodePicker>(xml);
                    intList = picker.NodeIdList;
                }
            }
            return intList;
        }

        private StringList GetStringListValue(INode node, string alias)
        {
            StringList stringList = null;
            string xml = node.GetProperty(alias).Value;
            if (!string.IsNullOrEmpty(xml))
            {
                if (xml.StartsWith("<SqlCheckBoxList"))
                {
                    var checkboxlist = Deserialize<_SqlCheckBoxList>(xml);
                    stringList = checkboxlist.Values;
                }
            }
            return stringList;
        }
        private HyperlinkList GetHyperLinkList(INode node, string alias)
        {
            HyperlinkList linkList = null;
            string xml = node.GetProperty(alias).Value;
            if (!string.IsNullOrEmpty(xml))
            {
                if (xml.StartsWith("<multi-url-picker"))
                {
                    var picker = Deserialize<MultiUrlPicker>(xml);
                    if (picker != null && picker.Links != null)
                    {
                        linkList = new HyperlinkList();
                        foreach (_Hyperlink hyperlink in picker.Links)
                        {
                            linkList.Add(ConvertHyperLinkInteral(hyperlink));
                        }
                    }
                }
            }
            return linkList;
        }
        private Hyperlink GetHyperLink(INode node, string alias)
        {
            Hyperlink result = null;
            _Hyperlink data = null;
            string xml = node.GetProperty(alias).Value;
            if (!string.IsNullOrEmpty(xml))
            {
                data = Deserialize<_Hyperlink>(xml);
                result = ConvertHyperLinkInteral(data);
            }
            return result;
        }

        private Hyperlink ConvertHyperLinkInteral(_Hyperlink hyperlink)
        {
            Hyperlink result = null;
            if (hyperlink != null)
            {
                result = new Hyperlink();
                result.Url = hyperlink.Url;
                result.LinkTitle = hyperlink.LinkTitle;

                bool boolValue;
                if (bool.TryParse(hyperlink.NewWindow, out boolValue))
                    result.NewWindow = boolValue;

                int intValue = 0;
                if (int.TryParse(hyperlink.NodeId, out intValue))
                    result.NodeId = intValue;
            }
            return result;
        }
        private T GetXmlType<T>(INode node, string alias) where T : class, new()
        {
            T data = null;
            string xml = node.GetProperty(alias).Value;
            if (!string.IsNullOrEmpty(xml))
            {
                data = Deserialize<T>(xml);
            }
            return data;
        }
        private MediaInfo GetMedia(INode node, string alias)
        {
            MediaInfo mediaInfo = null;
            int Id = GetPropertyInt(node, alias);
            if (Id > 0)
            {
                DynamicMedia media = new DynamicMedia(Id);
                mediaInfo = new MediaInfo();
                mediaInfo.Id = Id;
                mediaInfo.Url = media.NiceUrl;
            }
            return mediaInfo;
        }

        private MovieDataGrid GetMovieDataGrid(INode node, string alias)
        {
            MovieDataGrid gridModel = null;
            string gridXml = node.GetProperty(alias).Value;
            if (!string.IsNullOrEmpty(gridXml))
            {
                gridModel = new MovieDataGrid();
                var gridItems = Deserialize<_MovieDataGrid>(gridXml);
                foreach (var gridItem in gridItems.Items)
                {
                    var item = new MovieDataGrid.Item();
                    item.TextField = gridItem.textField;
                    {
                        bool value = false;
                        if (gridItem.checkboxField != null)
                        {
                            if (gridItem.checkboxField == "1") value = true;
                            else if (gridItem.checkboxField.ToUpper() == "TRUE") value = true;
                        }
                        item.CheckboxField = value;
                    }
                    {
                        int value = 0;
                        if (gridItem.int_integerField != null)
                        {
                            int.TryParse(gridItem.int_integerField, out value);
                        }
                        item.IntegerField = value;
                    }
                    {
                        Hyperlink value = null;
                        _Hyperlink data = null;
                        if (!string.IsNullOrEmpty(gridItem.urlPickerField))
                        {
                            data = Deserialize<_Hyperlink>(gridItem.urlPickerField);
                            value = ConvertHyperLinkInteral(data);
                        }
                        item.UrlPickerField = value;
                    }
                    {
                        DropDownColors value;
                        if (gridItem.colorField != null)
                        {
                            if (Enum.TryParse(gridItem.colorField, false, out value))
                                item.ColorField = value;
                        }
                    }
                    {
                        DateTime value = DateTime.MinValue;
                        if (gridItem.dateTimePickerField != null)
                        {
                            string format = GetDateTimeFormat(item.GetType().GetProperty("DateTimePickerField"));
                            value = GetDateFromString(gridItem.dateTimePickerField, format);
                        }
                        item.DateTimePickerField = value;
                    }
                    {
                        DateTime value = DateTime.MinValue;
                        if (gridItem.dateTimeValueField_datetime != null)
                        {
                            string format = GetDateTimeFormat(item.GetType().GetProperty("DateTimeValueField"));
                            value = GetDateFromString(gridItem.dateTimeValueField_datetime, format);
                        }
                        item.DateTimeValueField = value;
                    }
                    gridModel.Items.Add(item);
                }
            }
            return gridModel;
        }


        #endregion


        #region Property Get Methods
        private string GetPropertyString(INode node, string propertyName)
        {
            IProperty property = null;
            property = node.GetProperty(propertyName);
            if (property != null) return property.Value;
            return null;
        }

        private int GetPropertyInt(INode node, string propertyName)
        {
            int intValue = 0;
            IProperty property = null;
            property = node.GetProperty(propertyName);
            if (property != null)
            {
                int.TryParse(property.Value, out intValue);
            }
            return intValue;
        }

        private double GetPropertyDouble(INode node, string propertyName)
        {
            double value = 0;
            IProperty property = null;
            property = node.GetProperty(propertyName);
            if (property != null)
            {
                double.TryParse(property.Value, out value);
            }
            return value;
        }

        private DateTime GetPropertyDateTime(INode node, string propertyName, string format)
        {
            DateTime value = DateTime.MinValue;
            IProperty property = null;
            property = node.GetProperty(propertyName);
            if (property != null)
            {
                value = GetDateFromString(property.Value, format);
            }
            return value;
        }
        private bool GetPropertyBool(INode node, string propertyName)
        {
            IProperty property = null;
            property = node.GetProperty(propertyName);
            if (property != null && !string.IsNullOrEmpty(property.Value))
            {
                if (property.Value.Equals("0")) return false;
                if (property.Value.Equals("1")) return true;
                if (property.Value.ToUpper().Equals("FALSE")) return false;
                if (property.Value.ToUpper().Equals("TRUE")) return true;
            }
            return false;
        }
        private DateTime GetDateFromString(string date, string format)
        {
            DateTime dt = DateTime.MinValue;
            if (string.IsNullOrEmpty(date)) return dt;
            date = date.Trim();

            if (!string.IsNullOrEmpty(format))
            {
                if (DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    return dt;
            }

            //
            // TODO: review this code: can't find documentation how Umbraco convert DateTime input to Xml string values. Check if this conversion depends on culture settings.
            //

            if (DateTime.TryParseExact(date, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            if (DateTime.TryParseExact(date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
            if (DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;

            try
            {
                dt = System.Xml.XmlConvert.ToDateTime(date, XmlDateTimeSerializationMode.Utc);
                return dt;
            }
            catch
            {
            }

            return DateTime.MinValue;
        }

        private T Deserialize<T>(string serial) where T : class, new()
        {
            T result = null;
            try
            {
                var sr = new DataContractSerializer(typeof(T));
                serial = EncloseTag<T>(serial);
                using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(serial)))
                {
                    result = sr.ReadObject(stream) as T;
                }
            }
            catch (Exception)
            {
                result = new T();
            }
            return result;
        }

        private string EncloseTag<T>(string serial)
        {
            string contractName = GetContactName<T>();

            if (serial.StartsWith("<" + contractName)) return serial;

            return "<" + contractName + ">" + serial + "</" + contractName + ">";
        }

        private string GetContactName<T>()
        {
            var contractAttribute = typeof(T).GetCustomAttributes(false)
                                             .FirstOrDefault(a => a.GetType().Name.Equals("DataContractAttribute"))
                                             as DataContractAttribute;
            if (contractAttribute != null) return contractAttribute.Name;

            return null;
        }
        #endregion
    }
}
