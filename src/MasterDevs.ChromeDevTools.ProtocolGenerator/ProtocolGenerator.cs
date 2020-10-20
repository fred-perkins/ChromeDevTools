using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MasterDevs.ChromeDevTools.ProtocolGenerator
{
    public interface IProtocolGenerator
    {
        void Generate(ProtocolDefinition protocol, string directory);
    }

    public class ProtocolGenerator : IProtocolGenerator
    {
        private const string CommandAttribute = "Command";
        private const string CommandResponseAttribute = "CommandResponse";
        private const string EventAttribute = "Event";
        private const string ProtocolNameClass = "ProtocolName";
        private const string RootNamespace = "MasterDevs.ChromeDevTools.Protocol";
        private const string CommandSubclass = "Command";
        private const string CommandResponseSubclass = CommandSubclass + "Response";
        private const string EventSubclass = "Event";

        private static readonly string ObjectTypeDefintion = nameof(Object).ToLower();
        private static readonly string ArrayTypeDefintion = nameof(Array).ToLower();

        private const char Tab = '\t';

        private static Dictionary<string, Dictionary<string, string>> domainPropertyTypes = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, List<string>> domainCommands = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> domainEvents = new Dictionary<string, List<string>>();
        private static Dictionary<string, string> simpleTypes = new Dictionary<string, string>();

        public void Generate(ProtocolDefinition protocol, string directory)
        {
            string alias = protocol.Version.ToString();
            domainPropertyTypes.Clear();
            domainCommands.Clear();
            domainEvents.Clear();
            simpleTypes.Clear();

            var domains = protocol.Domains;
            var directoryInfo = new DirectoryInfo(directory);

            foreach (var domain in domains)
            {
                AddPropertyTypes(domain.Name, domain.Types);
            }

            foreach (var domain in domains)
            {
                var domainName = domain.Name;

                domainCommands[domainName] = new List<string>();
                domainEvents[domainName] = new List<string>();

                Console.WriteLine($"Writing Domain: {domainName}");
                WriteProtocolClasses(directoryInfo, alias, domain);
            }

            WriteMethodConstants(directoryInfo, alias);
        }

        private static void AddPropertyTypes(string domain, IEnumerable<ProtocolType> types)
        {
            var domainDictionary = new Dictionary<string, string>();
            domainPropertyTypes[domain] = domainDictionary;
            foreach (var type in types)
            {
                var propertyType = type.Kind;
                var typeName = type.Name;
                if (type.IsEnum()
                    || type.IsClass()
                    || type.IsObject())
                {
                    propertyType = domain + "." + typeName;
                }
                if ("Network" == domain && "Headers" == typeName)
                {
                    domainDictionary[typeName] = "Dictionary<string, string>";
                }
                else
                {
                    domainDictionary[typeName] = GeneratePropertyType(propertyType);
                }
                if ("array" == propertyType)
                {
                    AddArrayPropertyType(domainDictionary, domain, type);
                }
            }
        }

        private static void AddArrayPropertyType(Dictionary<string, string> domainDictionary, string domain, ProtocolType type)
        {
            var items = type.Items;
            if (null == items) return;
            var itemsType = GeneratePropertyType(items.Kind);
            if (String.IsNullOrEmpty(itemsType))
            {
                itemsType = items.TypeReference;
            }
            if (IsGeneratedNativeType(itemsType))
                domainDictionary[type.Name] = itemsType + "[]";
            else
                domainDictionary[type.Name] = domain + "." + itemsType + "[]";
        }

        private static void WriteProtocolClasses(DirectoryInfo directory, string ns, ProtocolDomain domain)
        {
            var domainDirectoryInfo = CreateDomainFolder(directory, domain.Name);
            foreach (var type in domain.Types)
            {
                Console.WriteLine($"    Writing Type: {type.Name}");
                try
                {
                    WriteType(domainDirectoryInfo, ns, type);

                }
                catch (Exception e)
                {
                    Console.WriteLine($"      Writing Type Failed: {e.Message}");
                }
            }
            foreach (var command in domain.Commands)
            {
                Console.WriteLine($"    Writing Command: {command.Name}");
                try
                {
                    WriteCommand(domainDirectoryInfo, ns, command);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"      Writing Command Failed: {e.Message}");
                }

            }
            foreach (var evnt in domain.Events)
            {
                Console.WriteLine($"    Writing Event: {evnt.Name}");
                try
                {
                    WriteEvent(domainDirectoryInfo, ns, evnt);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"      Writing Event Failed: {e.Message}");
                }
            }
        }

        private static void WriteMethodConstants(DirectoryInfo domainDirectoryInfo, string ns)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("using MasterDevs.ChromeDevTools;");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("namespace {0}.{1}", RootNamespace, ns);
            sb.AppendLine();
            sb.AppendLine("{");
            sb.AppendLine($"{Tab}public static class {ProtocolNameClass}");
            sb.AppendLine();
            sb.AppendLine($"{Tab}{{");

            var domains = domainCommands.Keys.Union(domainEvents.Keys).Distinct();
            foreach (var domain in domains)
            {
                sb.AppendLine($"{Tab.Repeat(2)}public static class {domain}");
                sb.AppendLine();
                sb.AppendLine($"{Tab.Repeat(2)}{{");
                List<string> commands;
                if (domainCommands.TryGetValue(domain, out commands))
                {
                    foreach (var commandName in commands)
                    {
                        sb.AppendLine($"{Tab.Repeat(3)}public const string {ToCamelCase(commandName)} = \"{domain}.{commandName}\";");
                        sb.AppendLine();
                    }
                }
                List<string> events;
                if (domainEvents.TryGetValue(domain, out events))
                {
                    foreach (var eventName in events)
                    {
                        sb.AppendLine($"{Tab.Repeat(3)}public const string {ToCamelCase(eventName)} = \"{domain}.{eventName}\";");
                        sb.AppendLine();
                    }
                }
                sb.AppendLine($"{Tab.Repeat(2)}}}");
                sb.AppendLine();
            }

            sb.AppendLine($"{Tab}}}");
            sb.AppendLine("}");
            WriteToFile(domainDirectoryInfo, ProtocolNameClass, sb.ToString());
        }

        private static void WriteEvent(DirectoryInfo domainDirectoryInfo, string ns, ProtocolEvent evnt)
        {
            if (null == evnt) return;
            var eventName = evnt.Name;
            var description = evnt.Description;
            var parameters = evnt.Parameters;
            // ignoreing "handlers" ... i'm not sure what they are for yet
            domainEvents[domainDirectoryInfo.Name].Add(eventName);
            WriteEvent(domainDirectoryInfo, ns, eventName, description, parameters, evnt.SupportedBy);
        }

        private static void WriteEvent(DirectoryInfo domainDirectoryInfo, string ns, string eventName, string description, IEnumerable<ProtocolProperty> parameters, IEnumerable<string> supportedBy)
        {
            var className = ToCamelCase(eventName) + EventSubclass;
            var sb = new StringBuilder();
            sb.AppendLine("using MasterDevs.ChromeDevTools;");
            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0}.{1}.{2}", RootNamespace, ns, domainDirectoryInfo.Name);
            sb.AppendLine();
            sb.AppendLine("{");
            WriteSummary(sb, description);
            sb.AppendLine($"{Tab}[{EventAttribute}({ProtocolNameClass}.{domainDirectoryInfo.Name}.{ToCamelCase(eventName)})]");
            sb.AppendLine();
            WriteSupportedBy(sb, supportedBy);
            sb.AppendLine($"{Tab}public class {className}");
            sb.AppendLine();
            sb.AppendLine($"{Tab}{{");
            foreach (var parameterProperty in parameters)
            {
                WriteProperty(sb, domainDirectoryInfo.Name, className, parameterProperty);
            }
            sb.AppendLine($"{Tab}}}");
            sb.AppendLine("}");
            WriteToFile(domainDirectoryInfo, className, sb.ToString());
        }

        private static void WriteCommand(DirectoryInfo domainDirectoryInfo, string ns, ProtocolCommand command)
        {
            if (null == command) return;
            var commandName = command.Name;
            var description = command.Description;
            var parameters = command.Parameters;
            var returnObject = command.Returns;
            domainCommands[domainDirectoryInfo.Name].Add(commandName);
            WriteCommand(domainDirectoryInfo, ns, commandName, description, command.IsDeprecated, parameters, command.SupportedBy);
            WriteCommandResponse(domainDirectoryInfo, ns, commandName, description, command.IsDeprecated, returnObject, command.SupportedBy);
        }

        private static void WriteCommandResponse(DirectoryInfo domainDirectoryInfo, string ns, string commandName, string description, bool isDeprecated, IEnumerable<ProtocolProperty> returnObject, IEnumerable<string> supportedBy)
        {
            var className = ToCamelCase(commandName) + CommandResponseSubclass;
            var sb = new StringBuilder();
            sb.AppendLine("using MasterDevs.ChromeDevTools;");
            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0}.{1}.{2}", RootNamespace, ns, domainDirectoryInfo.Name);
            sb.AppendLine();
            sb.AppendLine("{");
            WriteSummary(sb, description);
            if (isDeprecated)
                WriteObsoleteAttribute(sb, description);
            sb.AppendLine($"{Tab}[{CommandResponseAttribute}({ProtocolNameClass}.{domainDirectoryInfo.Name}.{ToCamelCase(commandName)})]");
            sb.AppendLine();
            WriteSupportedBy(sb, supportedBy);
            sb.AppendLine($"{Tab}public class {className}");
            sb.AppendLine();
            sb.AppendLine($"{Tab}{{");
            foreach (var returnObjectProperty in returnObject)
            {
                WriteProperty(sb, domainDirectoryInfo.Name, className, returnObjectProperty);
            }
            sb.AppendLine($"{Tab}}}");
            sb.AppendLine("}");
            WriteToFile(domainDirectoryInfo, className, sb.ToString());
        }

        private static void WriteCommand(DirectoryInfo domainDirectoryInfo, string ns, string commandName, string description, bool isDeprecated, IEnumerable<ProtocolProperty> parameters, IEnumerable<string> supportedBy)
        {
            var className = ToCamelCase(commandName) + CommandSubclass;
            var responseClassName = ToCamelCase(commandName) + CommandResponseSubclass;
            var sb = new StringBuilder();
            sb.AppendLine("using MasterDevs.ChromeDevTools;");
            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0}.{1}.{2}", RootNamespace, ns, domainDirectoryInfo.Name);
            sb.AppendLine();
            sb.AppendLine("{");
            WriteSummary(sb, description);
            if (isDeprecated)
                WriteObsoleteAttribute(sb, description);
            sb.AppendFormat("\t[{0}({1}.{2}.{3})]", CommandAttribute, ProtocolNameClass, domainDirectoryInfo.Name, ToCamelCase(commandName));
            sb.AppendLine();
            WriteSupportedBy(sb, supportedBy);
            sb.AppendFormat("\tpublic class {0}: ICommand<{1}>", className, responseClassName);
            sb.AppendLine();
            sb.AppendLine("\t{");
            foreach (var parameterProperty in parameters)
            {
                WriteProperty(sb, domainDirectoryInfo.Name, className, parameterProperty);
            }
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            WriteToFile(domainDirectoryInfo, className, sb.ToString());
        }

        private static void WriteSummary(StringBuilder sb, string description)
        {
            if (!String.IsNullOrEmpty(description))
            {
                sb.AppendLine("\t/// <summary>");
                sb.AppendFormat("\t/// {0}", description);
                sb.AppendLine();
                sb.AppendLine("\t/// </summary>");
            }
        }

        private static void WriteObsoleteAttribute(StringBuilder sb, string description)
        {
            if (String.IsNullOrEmpty(description) || !description.StartsWith("Deprecated"))
                sb.AppendLine("\t[Obsolete]");
            else
            {
                sb.AppendFormat("\t[Obsolete(\"{0}\")]", description);
                sb.AppendLine();
            }
        }

        private static void WriteType(DirectoryInfo domainDirectoryInfo, string ns, ProtocolType type)
        {
            if (null == type) return;
            if (type.Enum.Any()) WriteTypeEnum(domainDirectoryInfo, ns, type);
            /*if (type.Properties.Any())*/
            WriteTypeClass(domainDirectoryInfo, ns, type);
            WriteTypeSimple(domainDirectoryInfo, type);
        }

        private static void WriteTypeSimple(DirectoryInfo domainDirectoryInfo, ProtocolType type)
        {
            simpleTypes[type.Name] = type.Kind;
        }

        private static void WriteTypeClass(DirectoryInfo domainDirectoryInfo, string ns, ProtocolType type)
        {
            if ("object" != type.Kind) return;
            var className = type.Name;
            var sb = new StringBuilder();
            sb.AppendFormat("using MasterDevs.ChromeDevTools;");
            sb.AppendLine();
            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendFormat("namespace {0}.{1}.{2}", RootNamespace, ns, domainDirectoryInfo.Name);
            sb.AppendLine();
            sb.AppendLine("{");
            WriteSummary(sb, type.Description);
            WriteSupportedBy(sb, type);
            sb.AppendFormat("\tpublic class {0}", className);
            sb.AppendLine();
            sb.AppendLine("\t{");
            foreach (var propertyDescription in type.Properties)
            {
                WriteProperty(sb, domainDirectoryInfo.Name, className, propertyDescription);
            }
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            WriteToFile(domainDirectoryInfo, className, sb.ToString());
        }

        private static void WriteProperty(StringBuilder sb, string domain, string className, ProtocolProperty property)
        {
            var propertyName = GeneratePropertyName(property.Name);
            string propertyType = property.Kind;
            if (null != property.TypeReference)
            {
                propertyType = GeneratePropertyTypeFromReference(domain, property.TypeReference);
            }
            else if (propertyType == ArrayTypeDefintion)
            {
                var arrayDescription = property.Items;
                if (arrayDescription?.TypeReference != null)
                {
                    propertyType = GeneratePropertyTypeFromReference(domain, arrayDescription.TypeReference) + "[]";
                }
                else
                {
                    var arrayType = arrayDescription?.Kind;
                    if (arrayType == null || arrayType == ObjectTypeDefintion)
                    {
                        var internalClassName = ToCamelCase(propertyName) + "Array";
                        propertyType = internalClassName + "[]";
                        sb.AppendFormat("\t\tpublic class {0}", internalClassName);
                        sb.AppendLine();
                        sb.AppendLine("\t\t{");
                        foreach (var internalProperty in arrayDescription?.Properties ?? Enumerable.Empty<ProtocolProperty>())
                        {
                            WriteProperty(sb, domain, internalClassName, internalProperty);
                        }
                        sb.AppendLine("\t\t}");
                        sb.AppendLine();
                    }
                    else
                    {
                        propertyType = GeneratePropertyType(arrayDescription.Kind) + "[]";
                    }
                }
            }
            else
            {
                propertyType = GeneratePropertyType(propertyType.ToString());
            }

            string[] referenceTypes = new string[] { "long", "bool" };

            // If the property is optional, but a value type in .NET, make it nullable,
            // so that the property becomes optional.
            if (property.Optional && referenceTypes.Contains(propertyType))
            {
                propertyType += "?";
            }

            sb.AppendLine("\t\t/// <summary>");
            sb.AppendFormat("\t\t/// Gets or sets {0}", property.Description ?? propertyName);
            sb.AppendLine();
            sb.AppendLine("\t\t/// </summary>");
            if (className == propertyName)
            {
                sb.AppendFormat("\t\t[JsonProperty(\"{0}\")]", property.Name);
                sb.AppendLine();
                propertyName += "Child";
            }
            else if (property.Optional)
            {
                sb.AppendLine("\t\t[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]");
            }
            sb.AppendFormat("\t\tpublic {0} {1} {{ get; set; }}", propertyType, propertyName);
            sb.AppendLine();
        }

        private static string GeneratePropertyTypeFromReference(string domain, string propertyRef)
        {
            if (null == propertyRef) return null;
            var propertyPaths = propertyRef.Split('.');
            if (1 == propertyPaths.Length)
            {
                Dictionary<string, string> domainDictionary;
                string inDomainType;
                if (domainPropertyTypes.TryGetValue(domain, out domainDictionary)
                    && domainDictionary.TryGetValue(propertyPaths[0], out inDomainType))
                {
                    if (inDomainType.StartsWith(domain + "."))
                    {
                        return inDomainType.Substring(inDomainType.IndexOf('.') + 1);
                    }
                    return inDomainType;
                }
                return propertyPaths[0];
            }
            else
            {
                domain = propertyPaths[0];
                var name = propertyPaths[1];
                return domainPropertyTypes[domain][name];
            }
        }

        private static string GeneratePropertyType(string propertyType)
        {
            switch (propertyType)
            {
                case "number": return "double";
                case "integer": return "long";
                case "boolean": return "bool";
                case "any": return "object";
                default: return propertyType;
            }
        }

        private static bool IsGeneratedNativeType(string propertyType)
        {
            switch (propertyType)
            {
                case "double":
                case "long":
                case "bool":
                case "object":
                    return true;
                default:
                    return false;
            }
        }

        private static string GeneratePropertyName(string propertyName)
        {
            return ToCamelCase(propertyName);
        }

        private static string ToCamelCase(string propertyName)
        {
            return Char.ToUpper(propertyName[0]).ToString() + propertyName.Substring(1);
        }

        private static void WriteTypeEnum(DirectoryInfo domainDirectoryInfo, string ns, ProtocolType type)
        {
            var enumName = type.Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using MasterDevs.ChromeDevTools;");
            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine("using Newtonsoft.Json.Converters;");
            sb.AppendLine("using System.Runtime.Serialization;");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("namespace {0}.{1}.{2}", RootNamespace, ns, domainDirectoryInfo.Name);
            sb.AppendLine("{");
            sb.AppendLine("\t/// <summary>");
            sb.AppendFormat("\t/// {0}", type.Description);
            sb.AppendLine();
            sb.AppendLine("\t/// </summary>");
            sb.AppendLine("\t[JsonConverter(typeof(StringEnumConverter))]");
            sb.AppendFormat("\tpublic enum {0}", enumName);
            sb.AppendLine();
            sb.AppendLine("\t{");
            foreach (var enumValueName in type.Enum)
            {
                if (enumValueName.Contains("-"))
                {
                    sb.AppendFormat("\t\t\t[EnumMember(Value = \"{0}\")]", enumValueName);
                    sb.AppendLine();
                }

                sb.AppendFormat("\t\t\t{0},", ToCamelCase(enumValueName.Replace("-", "_")));
                sb.AppendLine();
            }
            sb.AppendLine($"{Tab}}}");
            sb.AppendLine("}");
            WriteToFile(domainDirectoryInfo, enumName, sb.ToString());
        }

        private static void WriteSupportedBy(StringBuilder sb, ProtocolItem type)
        {
            WriteSupportedBy(sb, type.SupportedBy);
        }

        private static void WriteSupportedBy(StringBuilder sb, IEnumerable<string> supportedBy)
        {
            foreach (var browser in supportedBy)
            {
                sb.AppendLine($"{Tab}[SupportedBy(\"{browser}\")]");
            }
        }

        private static void WriteToFile(DirectoryInfo domainDirectoryInfo, string fileName, string fileContents)
        {
            var fullPath = Path.Combine(domainDirectoryInfo.FullName, fileName + ".cs");
            if (File.Exists(fullPath)) File.Delete(fullPath);
            File.WriteAllText(fullPath, fileContents);
        }

        private static DirectoryInfo CreateDomainFolder(DirectoryInfo parentDirectory, string domainName)
        {
            return parentDirectory.CreateSubdirectory(domainName);
        }
    }
}
