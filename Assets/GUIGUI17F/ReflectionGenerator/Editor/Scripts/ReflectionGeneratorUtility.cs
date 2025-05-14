using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GUIGUI17F.ReflectionGenerator
{
    /// <summary>
    /// utility used to read/write plugin configs and generate reflection wrappers
    /// </summary>
    public class ReflectionGeneratorUtility
    {
        private const string ConfigName = "ReflectionGeneratorConfig";

        /// <summary>
        /// get current reflection generator plugin configs
        /// </summary>
        /// <returns>config data</returns>
        public static ReflectionGeneratorConfig GetGeneratorConfig()
        {
            ReflectionGeneratorConfig config = Resources.Load<ReflectionGeneratorConfig>(ConfigName);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ReflectionGeneratorConfig>();
                config.ModifyOriginName = true;
                config.GetMethodPrefix = "Get";
                config.SetMethodPrefix = "Set";
                config.WrapperSaveDirectory = "Assets/GUIGUI17F/ReflectionGenerator/Editor/Wrappers";
            }
            return config;
        }

        /// <summary>
        /// save reflection generator plugin configs to scriptable object
        /// </summary>
        /// <param name="modifyOriginName">whether auto modify the name from fields</param>
        /// <param name="getMethodPrefix">the prefix of "Get" method name</param>
        /// <param name="setMethodPrefix">the prefix of "Set" method name</param>
        /// <param name="getMethodPostfix">the postfix of "Get" method name</param>
        /// <param name="setMethodPostfix">the postfix of "Set" method name</param>
        /// <param name="wrapperDirectory">generated wrappers save directory</param>
        /// <param name="configDirectory">config data save directory</param>
        public static void SaveGeneratorConfig(bool modifyOriginName, string getMethodPrefix, string setMethodPrefix, string getMethodPostfix, string setMethodPostfix, string wrapperDirectory, string configDirectory)
        {
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }
            string configPath = Path.Combine(configDirectory, $"{ConfigName}.asset");
            ReflectionGeneratorConfig config = AssetDatabase.LoadAssetAtPath<ReflectionGeneratorConfig>(configPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ReflectionGeneratorConfig>();
                AssetDatabase.CreateAsset(config, configPath);
            }
            config.ModifyOriginName = modifyOriginName;
            config.GetMethodPrefix = getMethodPrefix;
            config.SetMethodPrefix = setMethodPrefix;
            config.GetMethodPostfix = getMethodPostfix;
            config.SetMethodPostfix = setMethodPostfix;
            config.WrapperSaveDirectory = wrapperDirectory;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// get the base type list in the inheritance hierarchy
        /// </summary>
        /// <param name="selfType">the type of current class</param>
        /// <returns>the base type list</returns>
        public static Type[] GetBaseTypes(Type selfType)
        {
            List<Type> typeList = new List<Type>();
            while (selfType != null)
            {
                if (selfType.FullName != "System.Object")
                {
                    typeList.Add(selfType);
                }
                selfType = selfType.BaseType;
            }
            return typeList.ToArray();
        }

        /// <summary>
        /// generate a reflection wrapper for a specific class type
        /// </summary>
        /// <param name="type">target type of the generated wrapper</param>
        /// <param name="fieldList">fields which the generated wrapper should wrap</param>
        /// <param name="config">configs used to modify the generation progress</param>
        public static void GenerateWrapper(Type type, List<FieldInfo> fieldList, ReflectionGeneratorConfig config)
        {
            if (!Directory.Exists(config.WrapperSaveDirectory))
            {
                Directory.CreateDirectory(config.WrapperSaveDirectory);
            }
            string wrapperName = $"{GetModifiedName(type.Name)}Extensions";
            using (FileStream fs = File.Open(Path.Combine(config.WrapperSaveDirectory, $"{wrapperName}.cs"), FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.WriteLine("using System;");
                    writer.WriteLine("using System.Reflection;");
                    writer.WriteLine();
                    //compatible with the classes has no namespace
                    string firstTab = string.Empty;
                    if (!string.IsNullOrEmpty(type.Namespace))
                    {
                        firstTab = "\t";
                        writer.Write("namespace ");
                        writer.WriteLine(type.Namespace);
                        writer.WriteLine('{');
                    }
                    writer.Write(firstTab);
                    writer.Write("public static class ");
                    writer.WriteLine(wrapperName);
                    writer.Write(firstTab);
                    writer.WriteLine('{');
                    writer.Write(firstTab);
                    writer.Write("\tprivate static readonly Type TargetType = typeof(");
                    writer.Write(type.Name);
                    writer.WriteLine(");");

                    fieldList.ForEach(field =>
                    {
                        //generate the field getter
                        writer.WriteLine();
                        writer.Write(firstTab);
                        writer.Write("\tpublic static ");
                        writer.Write(GetTypeFullName(field.FieldType));
                        writer.Write(' ');
                        writer.Write(config.GetMethodPrefix);
                        if (config.ModifyOriginName)
                        {
                            writer.Write(GetModifiedName(field.Name));
                        }
                        else
                        {
                            writer.Write(field.Name);
                        }
                        writer.Write(config.GetMethodPostfix);
                        writer.Write("(this ");
                        writer.Write(type.Name);
                        writer.WriteLine(" target)");

                        writer.Write(firstTab);
                        writer.WriteLine("\t{");

                        writer.Write(firstTab);
                        writer.Write("\t\tFieldInfo field = TargetType.GetField(\"");
                        writer.Write(field.Name);
                        writer.WriteLine("\", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);");

                        writer.Write(firstTab);
                        writer.Write("\t\treturn (");
                        writer.Write(GetTypeFullName(field.FieldType));
                        writer.WriteLine(") field.GetValue(target);");

                        writer.Write(firstTab);
                        writer.WriteLine("\t}");

                        //generate the field setter
                        writer.WriteLine();
                        writer.Write(firstTab);
                        writer.Write("\tpublic static void ");
                        writer.Write(config.SetMethodPrefix);
                        if (config.ModifyOriginName)
                        {
                            writer.Write(GetModifiedName(field.Name));
                        }
                        else
                        {
                            writer.Write(field.Name);
                        }
                        writer.Write(config.SetMethodPostfix);
                        writer.Write("(this ");
                        writer.Write(type.Name);
                        writer.Write(" target, ");
                        writer.Write(GetTypeFullName(field.FieldType));
                        writer.WriteLine(" value)");

                        writer.Write(firstTab);
                        writer.WriteLine("\t{");

                        writer.Write(firstTab);
                        writer.Write("\t\tFieldInfo field = TargetType.GetField(\"");
                        writer.Write(field.Name);
                        writer.WriteLine("\", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);");

                        writer.Write(firstTab);
                        writer.WriteLine("\t\tfield.SetValue(target, value);");

                        writer.Write(firstTab);
                        writer.WriteLine("\t}");
                    });

                    writer.Write(firstTab);
                    writer.WriteLine('}');
                    if (!string.IsNullOrEmpty(type.Namespace))
                    {
                        writer.WriteLine('}');
                    }
                }
            }
        }

        //get the modified name for method generation
        private static string GetModifiedName(string originName)
        {
            List<char> modifyName = new List<char>(originName);
            for (int i = 0; i < modifyName.Count; i++)
            {
                if (modifyName[i] == '_')
                {
                    modifyName.RemoveAt(i);
                    if (i < modifyName.Count)
                    {
                        modifyName[i] = char.ToUpper(modifyName[i]);
                    }
                }
            }
            modifyName[0] = char.ToUpper(modifyName[0]);
            return new string(modifyName.ToArray());
        }

        private static string GetTypeFullName(Type type)
        {
            if (type.IsGenericType)
            {
                var builder = new StringBuilder();
                var originName = type.FullName;
                builder.Append(originName.Substring(0, originName.IndexOf('`')));
                builder.Append('<');
                var arguments = type.GenericTypeArguments;
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(GetTypeFullName(arguments[i]));
                }
                builder.Append('>');
                return builder.ToString();
            }
            else
            {
                return type.FullName;
            }
        }
    }
}