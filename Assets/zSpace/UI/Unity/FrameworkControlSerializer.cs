////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2013 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;
using zSpace.Common;

namespace zSpace.UI.Unity
{
    // todo: Replace JSON.NET with something internal.

    /// <summary>
    /// Serializes/deserializes the public fields of the FrameworkControl.
    /// </summary>
    public static class FrameworkControlSerializer
    {
        public static string Serialize(FrameworkControl frameworkControl)
        {
            var settings = GetJsonSerializerSettings();
            string frameworkControlAsString = JsonConvert.SerializeObject(frameworkControl, settings);
            return frameworkControlAsString;
        }

        public static FrameworkControl Deserialize(string data)
        {
            var settings = GetJsonSerializerSettings();
            FrameworkControl frameworkControl = (FrameworkControl)JsonConvert.DeserializeObject<FrameworkControl>(data, settings);
            return frameworkControl;
        }

        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            settings.TypeNameHandling = TypeNameHandling.Objects;
            settings.Binder = new JsonSerializationBinder();
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new FrameworkControlContractResolver();
            return settings;
        }

        internal class JsonSerializationBinder : System.Runtime.Serialization.SerializationBinder
        {
#if WEBSERVER
            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = serializedType.FullName;
            }
#endif
            static string GetAssemblyNameContainingType(string typeName)
            {
                Type type = zSpace.Common.Utility.FindType(typeName);
                return (type != null) ? type.Assembly.FullName : null;
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                string realAssemblyName = (Type.GetType(typeName) != null) ?
                    assemblyName :
                    GetAssemblyNameContainingType(typeName);

                //UnityEngine.Debug.Log("LOADING TYPE " + typeName + " FROM ASSEMBLY " + realAssemblyName + " NOT " + assemblyName);
                return Assembly.Load(realAssemblyName).GetType(typeName);
            }
        }

        private class FrameworkControlContractResolver : DefaultContractResolver
        {
            public FrameworkControlContractResolver()
            {
                this.DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty jsonProperty = null;
                if (member.MemberType == MemberTypes.Field)
                {
                    FieldInfo field = (FieldInfo)member;
                    if (field.IsStatic == false)
                    {
                        // Serialize all public fields.
                        if (field.IsPublic)
                        {
                            jsonProperty = base.CreateProperty(member, memberSerialization);
                        }
                        // Special treatment for specific fields.
                        else if (field.Name == "_layoutAttributes")
                        {
                            jsonProperty = base.CreateProperty(member, memberSerialization);
                            jsonProperty.PropertyName = "LayoutAttributes";
                        }
                        else if (field.Name == "_transformAttributes")
                        {
                            jsonProperty = base.CreateProperty(member, memberSerialization);
                            jsonProperty.PropertyName = "TransformAttributes";
                        }
                        else if (field.Name == "_visible")
                        {
                            jsonProperty = base.CreateProperty(member, memberSerialization);
                            jsonProperty.PropertyName = "Visible";
                        }
                    }
                }

                return jsonProperty;
            }
        }
    }
}

