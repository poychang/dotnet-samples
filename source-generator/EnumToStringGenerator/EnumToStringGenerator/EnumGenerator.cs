using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EnumToStringGenerator
{
    [Generator]
    public class EnumGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static (context) => PostInitializationOutput(context));

            IncrementalValuesProvider<EnumGeneratorModel?> model = context.SyntaxProvider.ForAttributeWithMetadataName(
                    "EnumToStringGenerator.EnumToStringAttribute",
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (context, _) => GetOutputContext(context)
                ).Where(static model => model is not null);

            context.RegisterSourceOutput(model, static (context, source) => ExecuteOutput(context, source));
        }

        private static void ExecuteOutput(SourceProductionContext context, EnumGeneratorModel? source)
        {
            var model = source.Value;
            var builder = new System.Text.StringBuilder();
            builder.Append($$"""
            namespace {{model.Namespace}}
            {
               public static partial class {{model.TypeName}}
               {
                    public static string FasterToString(this {{model.EnumTypeName}} value)
                    {
                        return value switch
                        {
                            {{string.Join($",\n{new string(' ', 16)}", model.Fields.Select(m => $"{model.EnumTypeName}.{m} => \"{m}\""))}},
                            _ => value.ToString()
                        };
                    }
               }
            }
            """);
            context.AddSource($"{model.Namespace}.{model.TypeName}.g.cs", builder.ToString());
        }

        private static EnumGeneratorModel? GetOutputContext(GeneratorAttributeSyntaxContext context)
        {
            if (context.TargetNode is not ClassDeclarationSyntax classDeclaration
                 || (!classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))))
            {
                return null;
            }
            if (context.TargetSymbol is not INamedTypeSymbol classSymbol || classSymbol.TypeKind != TypeKind.Class
                || (!classSymbol.IsStatic))
            {
                return null;
            }

            string @namespace = classSymbol.ContainingNamespace.ToDisplayString();
            string typeName = classSymbol.Name;
            // 取得 EnumToStringGenerator.EnumToStringAttribute (無須比對名稱，因為已經在 ForAttributeWithMetadataName 中指定了)
            AttributeData attribute = context.Attributes[0];

            /* 如果很害怕不比對會出包的話，以下有兩種方式可以比對是否為 EnumToStringGenerator.EnumToStringAttribute
             * 
             * (1) 這是用名稱比較是否為 EnumToStringGenerator.EnumToStringAttribute 
             *     AttributeData attribute = context.Attributes.First(a => a.AttributeClass?.ToDisplayString() == "EnumToStringGenerator.EnumToStringAttribute"); 
             *
             * (2) 這是使用 ISymbol 比較是否為 EnumToStringGenerator.EnumToStringAttribute
             *     INamedTypeSymbol symbol = context.SemanticModel.Compilation.GetTypeByMetadataName("EnumToStringGenerator.EnumToStringAttribute");
             *      AttributeData attribute = context.Attributes.First(a => SymbolEqualityComparer.Default.Equals(symbol, a.AttributeClass));
            */


            // TypedConstant.Value 取得Attribute建構式引數 (因為是 typeof(enum type) 所以是 INamedTypeSymbol)
            INamedTypeSymbol arg = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
            if (arg is null || arg.TypeKind != TypeKind.Enum)
            {
                return null;
            }
            //  取得 arg (Enum) 的型別名稱   
            string enumTypeName = arg.ToDisplayString();
            // 取得 arg (Enum) 的成員 (這邊開始連 IsConst 都不比對了)
            var fields = arg.GetMembers().OfType<IFieldSymbol>().Select(m => m.Name).ToArray();
            return new EnumGeneratorModel(@namespace, typeName, enumTypeName, fields);


        }

        private static void PostInitializationOutput(IncrementalGeneratorPostInitializationContext context)
        {
            string content = """
    using System;
    namespace EnumToStringGenerator
    {
        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        public sealed class EnumToStringAttribute : Attribute
        {
            public EnumToStringAttribute(Type enumType)
            {
                EnumType = enumType;
            }
            public Type EnumType { get; }
        }
    }
    """;
            context.AddSource("EnumToStringAttribute.g.cs", content);
        }
    }


}
