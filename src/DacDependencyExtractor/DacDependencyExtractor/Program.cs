using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
namespace DacDependencyExtractor
{
    class Program
    {
        static Dictionary<Type, Dependency> dependencies = new Dictionary<Type, Dependency>();
        static void Main(string[] args)
        {
            var assemblies = new List<Assembly>();
            assemblies.Add(Assembly.LoadFrom("assemblies/PX.Data.dll"));
            assemblies.Add(Assembly.LoadFrom("assemblies/PX.Objects.dll"));

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        var baseType = type.BaseType;
                        if (baseType == null)
                            continue;

                        foreach (var inter in type.GetInterfaces())
                            if (inter.Name == "IBqlTable")
                                ProcessDac(type);

                        while ( baseType.Name != "PXGraph" && 
                                baseType != typeof(System.Object))
                            baseType = baseType.BaseType;

                        if (baseType.Name == "PXGraph")
                            ProcessGraph(type);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void ProcessDac(Type type)
        {
            if (type.Name == "RMColumn")
                ;
            foreach(var property in type.GetProperties(BindingFlags.Instance |
                                                    BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.DeclaredOnly))
            {
                try
                {
                    foreach (var attrib in property.GetCustomAttributes(true))
                    {
                        if (attrib.GetType().Name == "PXForeignReferenceAttribute")
                            ProcessForeignReference(attrib);
                        if (attrib.GetType().Name == "PXParentAttribute")
                            ProcessParent(type, property, attrib);
                        if (attrib.GetType().Name == "PXDefaultAttribute")
                            ProcessDefault(type, property, attrib);
                    }
                }
                catch
                {

                }
            }
        }

        private static void ProcessDefault(Type type, PropertyInfo property, object attrib)
        {
           Type sourceType = GetFieldValue(attrib, "_SourceType") as Type;
            if (sourceType != null)
            {
                var propName = property.Name;
                var fieldTypeName = char.ToLower(propName[0]) + propName.Substring(1);
                var fromField = type.GetNestedType(fieldTypeName);
                DependencyDetails d;
                d.fromDac = type;
                d.fromField = fromField;
                d.toField = sourceType.GetNestedType(GetFieldValue(attrib, "_SourceField") as string);
                d.toDac = sourceType;
                d.connection = Connection.Default;
            }
        }

        private static void ProcessParent(Type type, PropertyInfo property, object attrib)
        {
            var selectParent = GetFieldValue(attrib, "_SelectParent");
            var selectType = selectParent.GetType();
            var bqlCommandType = selectType.BaseType;
            while (bqlCommandType.Name != "BqlCommand" &&
                   bqlCommandType != typeof(System.Object))
                bqlCommandType = bqlCommandType.BaseType;
            var decomposition = bqlCommandType.GetMethod("Decompose").Invoke(null, new object[] { selectType });



            var ddl = new List<DependencyDetails>();
            var propName = property.Name;
            var fieldTypeName = char.ToLower(propName[0]) + propName.Substring(1);
            var fromField = type.GetNestedType(fieldTypeName);

            foreach (var dtp in (IEnumerable<Type>)decomposition)
            {
                if (dtp.GetInterface("IBqlTable") != null)
                {

                }
                else if (dtp.DeclaringType!=null &&
                         dtp.DeclaringType.GetInterface("IBqlTable") != null)
                {
                    if (dtp.DeclaringType == type)
                    {
                        fromField = dtp;
                        continue;
                    }
                    DependencyDetails d;
                    d.fromDac = type;
                    d.fromField = fromField;
                    d.toField = dtp;
                    d.toDac = dtp.DeclaringType;
                    d.connection = Connection.Parent;
                    ddl.Add(d);
                }
            }
            foreach(var d in ddl)
            {
                var nd = d;
                nd.fromField = fromField;
                AddOutDependency(nd.fromDac, nd);
                AddInDependency(nd.toDac, nd);
            }
            //BqlCommand.Decompose(view.BqlSelect.GetType()
        }

        private static void ProcessForeignReference(object attrib)
        {
            var refHolder = GetFieldValue(attrib, "_refHolder");
            var relation = GetPropValue(refHolder, "SelectOrFieldsRelationsContainer");
            var arguments = (relation as Type).GetGenericArguments();
            for (int i = 0; i < arguments.Count(); i += 2)
            {
                DependencyDetails d;
                d.fromField = arguments[i];
                d.fromDac = d.fromField.DeclaringType;
                d.toField = arguments[i + 1];
                d.toDac = d.toField.DeclaringType;
                d.connection = Connection.ForeignReference;
                AddOutDependency(d.fromDac, d);
                AddInDependency(d.toDac, d);
            }
        }

        private static Dependency GetDependency(Type key)
        {
            Dependency dependency;
            if (!dependencies.TryGetValue(key, out dependency))
            {
                dependencies[key] = dependency = new Dependency();
                dependency.outDependency = new List<DependencyDetails>();
                dependency.inDependency = new List<DependencyDetails>();
            }
            return dependency;
        }


        private static void AddOutDependency(Type key, DependencyDetails d)
        {
            var dependency = GetDependency(key);
            dependency.outDependency.Add(d);
        }

        private static void AddInDependency(Type key, DependencyDetails d)
        {
            var dependency = GetDependency(key);
            dependency.inDependency.Add(d);
        }
        private static void ProcessGraph(Type type)
        {
            Console.WriteLine(type.Name);
        }

        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static object GetFieldValue(object src, string propName)
        {
            return src.GetType().GetField(propName, BindingFlags.Instance |
                                                    BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.DeclaredOnly).GetValue(src);
        }
    }
}