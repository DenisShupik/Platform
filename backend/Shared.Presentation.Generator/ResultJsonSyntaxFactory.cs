using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shared.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Shared.Presentation.Generator;

internal static class ResultJsonSyntaxFactory
{
    private const string Namespace = "Shared.Presentation.Convertors";

    public static CompilationUnitSyntax CreateFactory(int maximumErrorCount) =>
        CreateCompilationUnit(
            CreatePropertyNamesDeclaration(),
            CreateFactoryDeclaration(maximumErrorCount));

    public static CompilationUnitSyntax CreateConverter(int errorCount) =>
        CreateCompilationUnit(CreateConverterDeclaration(errorCount));

    private static CompilationUnitSyntax CreateCompilationUnit(params MemberDeclarationSyntax[] members) =>
        CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Text.Json")),
                UsingDirective(ParseName("System.Text.Json.Serialization")),
                UsingDirective(ParseName("Shared.Domain.Abstractions.Errors")),
                UsingDirective(ParseName("Shared.Domain.Abstractions.Results")))
            .AddMembers(
                FileScopedNamespaceDeclaration(ParseName(Namespace))
                    .AddMembers(members))
            .ApplyGeneratorDefaults();

    private static ClassDeclarationSyntax CreatePropertyNamesDeclaration() =>
        ClassDeclaration("ResultJsonPropertyNames")
            .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword))
            .AddMembers(
                CreateEncodedPropertyNameField("Value", "value"),
                CreateEncodedPropertyNameField("Error", "error"));

    private static FieldDeclarationSyntax CreateEncodedPropertyNameField(string name, string value) =>
        FieldDeclaration(
                VariableDeclaration(IdentifierName("JsonEncodedText"))
                    .AddVariables(
                        VariableDeclarator(name)
                            .WithInitializer(EqualsValueClause(
                                Invoke(
                                    Member(IdentifierName("JsonEncodedText"), "Encode"),
                                    Argument(StringLiteral(value)))))))
            .AddModifiers(
                Token(SyntaxKind.InternalKeyword),
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.ReadOnlyKeyword));

    private static ClassDeclarationSyntax CreateFactoryDeclaration(int maximumErrorCount) =>
        ClassDeclaration("ResultJsonConverterFactory")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword))
            .AddBaseListTypes(SimpleBaseType(IdentifierName("JsonConverterFactory")))
            .AddMembers(
                CreateFactoryInstanceProperty(),
                ConstructorDeclaration("ResultJsonConverterFactory")
                    .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                    .WithBody(Block()),
                CreateCanConvertMethod(maximumErrorCount),
                CreateConverterFactoryMethod(maximumErrorCount));

    private static PropertyDeclarationSyntax CreateFactoryInstanceProperty() =>
        PropertyDeclaration(IdentifierName("ResultJsonConverterFactory"), "Instance")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .WithAccessorList(AccessorList(SingletonList(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))))
            .WithInitializer(EqualsValueClause(
                ImplicitObjectCreationExpression().WithArgumentList(ArgumentList())))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateCanConvertMethod(int maximumErrorCount)
    {
        var genericDefinition = IdentifierName("genericDefinition");
        var supportedDefinitions = Enumerable.Range(1, maximumErrorCount)
            .Select(errorCount => (ExpressionSyntax)BinaryExpression(
                SyntaxKind.EqualsExpression,
                genericDefinition,
                TypeOfExpression(ResultType(errorCount, unbound: true))))
            .Aggregate((left, right) => BinaryExpression(SyntaxKind.LogicalOrExpression, left, right));

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), "CanConvert")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("typeToConvert")).WithType(IdentifierName("Type")))
            .WithBody(Block(
                IfStatement(
                    PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        Member(IdentifierName("typeToConvert"), "IsGenericType")),
                    ReturnStatement(LiteralExpression(SyntaxKind.FalseLiteralExpression))),
                LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("var"))
                        .AddVariables(
                            VariableDeclarator("genericDefinition")
                                .WithInitializer(EqualsValueClause(
                                    Invoke(Member(
                                        IdentifierName("typeToConvert"),
                                        "GetGenericTypeDefinition")))))),
                ReturnStatement(supportedDefinitions)));
    }

    private static MethodDeclarationSyntax CreateConverterFactoryMethod(int maximumErrorCount)
    {
        var genericArguments = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .AddVariables(
                    VariableDeclarator("genericArguments")
                        .WithInitializer(EqualsValueClause(
                            Invoke(Member(IdentifierName("typeToConvert"), "GetGenericArguments"))))));

        var converterTypeArms = Enumerable.Range(1, maximumErrorCount)
            .Select(errorCount => SwitchExpressionArm(
                ConstantPattern(NumericLiteral(errorCount + 1)),
                TypeOfExpression(ConverterType(errorCount, unbound: true))))
            .Append(SwitchExpressionArm(
                DiscardPattern(),
                ThrowExpression(
                    ObjectCreationExpression(IdentifierName("InvalidOperationException"))
                        .AddArgumentListArguments(
                            Argument(StringLiteral("Unsupported Result arity."))))));

        var converterType = LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .AddVariables(
                    VariableDeclarator("converterType")
                        .WithInitializer(EqualsValueClause(
                            SwitchExpression(Member(IdentifierName("genericArguments"), "Length"))
                                .WithArms(SeparatedList(converterTypeArms))))));

        var closedConverterType = Invoke(
            Member(IdentifierName("converterType"), "MakeGenericType"),
            Argument(IdentifierName("genericArguments")));
        var createConverter = Invoke(
            Member(IdentifierName("Activator"), "CreateInstance"),
            Argument(closedConverterType),
            Argument(IdentifierName("options")));
        var returnConverter = ReturnStatement(
            CastExpression(
                IdentifierName("JsonConverter"),
                PostfixUnaryExpression(
                    SyntaxKind.SuppressNullableWarningExpression,
                    createConverter)));

        return MethodDeclaration(NullableType(IdentifierName("JsonConverter")), "CreateConverter")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("typeToConvert")).WithType(IdentifierName("Type")),
                Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions")))
            .WithBody(Block(genericArguments, converterType, returnConverter));
    }

    private static ClassDeclarationSyntax CreateConverterDeclaration(int errorCount)
    {
        var typeParameters = new[] { TypeParameter("TValue") }
            .Concat(ErrorTypeParameters(errorCount))
            .ToArray();

        return ClassDeclaration("ResultJsonConverter")
            .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.SealedKeyword))
            .AddTypeParameterListParameters(typeParameters)
            .AddBaseListTypes(SimpleBaseType(
                GenericName("JsonConverter")
                    .AddTypeArgumentListArguments(ResultType(errorCount))))
            .AddConstraintClauses(
                TypeParameterConstraintClause("TValue")
                    .AddConstraints(TypeConstraint(IdentifierName("notnull"))))
            .AddConstraintClauses(ErrorConstraints(errorCount))
            .AddMembers(CreateConverterFields(errorCount))
            .AddMembers(
                CreateConverterConstructor(errorCount),
                CreateReadMethod(errorCount),
                CreateWriteMethod(errorCount));
    }

    private static MemberDeclarationSyntax[] CreateConverterFields(int errorCount) =>
        new[] { CreateConverterField(IdentifierName("TValue"), "_valueConverter") }
            .Concat(Enumerable.Range(1, errorCount)
                .Select(index => CreateConverterField(ErrorType(index), $"_error{index}Converter")))
            .Cast<MemberDeclarationSyntax>()
            .ToArray();

    private static FieldDeclarationSyntax CreateConverterField(TypeSyntax type, string name) =>
        FieldDeclaration(
                VariableDeclaration(
                        GenericName("JsonConverter").AddTypeArgumentListArguments(type))
                    .AddVariables(VariableDeclarator(name)))
            .AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword));

    private static ConstructorDeclarationSyntax CreateConverterConstructor(int errorCount)
    {
        var assignments = new[]
            {
                CreateConverterAssignment(IdentifierName("TValue"), "_valueConverter")
            }
            .Concat(Enumerable.Range(1, errorCount)
                .Select(index => CreateConverterAssignment(
                    ErrorType(index),
                    $"_error{index}Converter")))
            .ToArray();

        return ConstructorDeclaration("ResultJsonConverter")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions")))
            .WithBody(Block(assignments));
    }

    private static StatementSyntax CreateConverterAssignment(TypeSyntax type, string fieldName) =>
        ExpressionStatement(AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(fieldName),
            CastExpression(
                GenericName("JsonConverter").AddTypeArgumentListArguments(type),
                Invoke(
                    Member(IdentifierName("options"), "GetConverter"),
                    Argument(TypeOfExpression(type))))));

    private static MethodDeclarationSyntax CreateReadMethod(int errorCount)
    {
        var resultType = ResultType(errorCount);
        var statements = new List<StatementSyntax>
        {
            IfStatement(
                BinaryExpression(
                    SyntaxKind.NotEqualsExpression,
                    Member(IdentifierName("reader"), "TokenType"),
                    Member(IdentifierName("JsonTokenType"), "StartObject")),
                ThrowJson("Expected a JSON object for Result.")),
            LocalDeclarationStatement(
                VariableDeclaration(NullableType(resultType))
                    .AddVariables(
                        VariableDeclarator("result")
                            .WithInitializer(EqualsValueClause(
                                LiteralExpression(SyntaxKind.NullLiteralExpression)))))
        };

        var loopStatements = new List<StatementSyntax>
        {
            IfStatement(
                ReaderTokenIs("EndObject"),
                ReturnStatement(
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        IdentifierName("result"),
                        ThrowExpression(NewJsonException(
                            "Result JSON must contain either 'value' or 'error'."))))),
            IfStatement(
                ReaderTokenIsNot("PropertyName"),
                ThrowJson("Expected a Result property name.")),
            CreateReadValueBranch(errorCount),
            CreateReadErrorBranch(errorCount),
            ReadOrThrow("reader", "Unexpected end of Result JSON."),
            ExpressionStatement(Invoke(Member(IdentifierName("reader"), "Skip")))
        };

        statements.Add(WhileStatement(
            Invoke(Member(IdentifierName("reader"), "Read")),
            Block(loopStatements)));
        statements.Add(ThrowJson("Unexpected end of Result JSON."));

        return MethodDeclaration(resultType, "Read")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("reader"))
                    .AddModifiers(Token(SyntaxKind.RefKeyword))
                    .WithType(IdentifierName("Utf8JsonReader")),
                Parameter(Identifier("typeToConvert")).WithType(IdentifierName("Type")),
                Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions")))
            .WithBody(Block(statements));
    }

    private static IfStatementSyntax CreateReadValueBranch(int errorCount)
    {
        var deserializeValue = PostfixUnaryExpression(
            SyntaxKind.SuppressNullableWarningExpression,
            ReadWithConverter(
                "_valueConverter",
                IdentifierName("TValue"),
                "reader"));

        return IfStatement(
            ReaderValueEquals(Utf8Literal("value")),
            Block(
                ThrowIfResultAlreadySet(),
                ReadOrThrow("reader", "Unexpected end of Result value."),
                ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("result"),
                    deserializeValue)),
                ContinueStatement()));
    }

    private static IfStatementSyntax CreateReadErrorBranch(int errorCount)
    {
        var statements = new List<StatementSyntax> { ThrowIfResultAlreadySet() };
        if (errorCount == 1)
        {
            statements.Add(ReadOrThrow("reader", "Unexpected end of Result error."));
            statements.Add(ExpressionStatement(AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName("result"),
                BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    ReadWithConverter("_error1Converter", ErrorType(1), "reader"),
                    ThrowExpression(NewJsonException("Result error cannot be null."))))));
        }
        else
        {
            statements.AddRange(CreateDiscriminatedErrorRead(errorCount));
        }

        statements.Add(ContinueStatement());
        return IfStatement(ReaderValueEquals(Utf8Literal("error")), Block(statements));
    }

    private static IEnumerable<StatementSyntax> CreateDiscriminatedErrorRead(int errorCount)
    {
        yield return LocalDeclarationStatement(
            VariableDeclaration(IdentifierName("var"))
                .AddVariables(
                    VariableDeclarator("checkpointReader")
                        .WithInitializer(EqualsValueClause(IdentifierName("reader")))));
        yield return ReadOrThrow("reader", "Unexpected end of Result error.");
        yield return IfStatement(
            ReaderTokenIsNot("StartObject"),
            ThrowJson("Result error must be a JSON object."));

        var invalidDiscriminator = BinaryExpression(
            SyntaxKind.LogicalOrExpression,
            PrefixUnaryExpression(
                SyntaxKind.LogicalNotExpression,
                Invoke(Member(IdentifierName("reader"), "Read"))),
            BinaryExpression(
                SyntaxKind.LogicalOrExpression,
                BinaryExpression(
                    SyntaxKind.NotEqualsExpression,
                    Member(IdentifierName("reader"), "TokenType"),
                    Member(IdentifierName("JsonTokenType"), "PropertyName")),
                PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    ReaderValueEquals(Utf8Literal("$type")))));
        yield return IfStatement(
            invalidDiscriminator,
            ThrowJson("The first Result error property must be '$type'."));
        yield return ReadOrThrow("reader", "Unexpected end of Result error discriminator.");
        yield return IfStatement(
            ReaderTokenIsNot("String"),
            ThrowJson("Result error discriminator must be a string."));

        StatementSyntax discriminator = ThrowJson("Unknown Result error discriminator.");
        for (var index = errorCount; index >= 1; index--)
        {
            var readError = Block(
                ExpressionStatement(Invoke(Member(IdentifierName("checkpointReader"), "Read"))),
                ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("result"),
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        ReadWithConverter(
                            $"_error{index}Converter",
                            ErrorType(index),
                            "checkpointReader"),
                        ThrowExpression(NewJsonException("Result error cannot be null."))))));
            discriminator = IfStatement(
                    ReaderValueEquals(Member(TypeOfExpression(ErrorType(index)), "Name")),
                    readError)
                .WithElse(ElseClause(discriminator));
        }

        yield return discriminator;
        yield return ExpressionStatement(AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName("reader"),
            IdentifierName("checkpointReader")));
    }

    private static MethodDeclarationSyntax CreateWriteMethod(int errorCount)
    {
        var success = Block(
            WritePropertyName("Value"),
            WriteWithConverter("_valueConverter", "value"));

        StatementSyntax failure = ThrowJson("Result contains an invalid failure.");
        for (var index = errorCount; index >= 1; index--)
        {
            var errorName = $"error{index}";
            failure = IfStatement(
                    Invoke(
                        GenericMember(IdentifierName("failure"), "TryGet", ErrorType(index)),
                        OutVariable(errorName)),
                    Block(
                        WritePropertyName("Error"),
                        WriteWithConverter($"_error{index}Converter", errorName)))
                .WithElse(ElseClause(failure));
        }

        var valueOrFailure = IfStatement(
                Invoke(
                    Member(IdentifierName("input"), "TryGetValue"),
                    OutVariable("value"),
                    OutVariable("failure")),
                success)
            .WithElse(ElseClause(failure));

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "Write")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("writer")).WithType(IdentifierName("Utf8JsonWriter")),
                Parameter(Identifier("input")).WithType(ResultType(errorCount)),
                Parameter(Identifier("options")).WithType(IdentifierName("JsonSerializerOptions")))
            .WithBody(Block(
                ExpressionStatement(Invoke(Member(IdentifierName("writer"), "WriteStartObject"))),
                valueOrFailure,
                ExpressionStatement(Invoke(Member(IdentifierName("writer"), "WriteEndObject")))));
    }

    private static IfStatementSyntax ThrowIfResultAlreadySet() =>
        IfStatement(
            IsPatternExpression(
                IdentifierName("result"),
                UnaryPattern(
                    Token(SyntaxKind.NotKeyword),
                    ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression)))),
            ThrowJson("Result JSON cannot contain duplicate value or error properties."));

    private static IfStatementSyntax ReadOrThrow(string readerName, string message) =>
        IfStatement(
            PrefixUnaryExpression(
                SyntaxKind.LogicalNotExpression,
                Invoke(Member(IdentifierName(readerName), "Read"))),
            ThrowJson(message));

    private static ExpressionStatementSyntax WritePropertyName(string name) =>
        ExpressionStatement(Invoke(
            Member(IdentifierName("writer"), "WritePropertyName"),
            Argument(Member(IdentifierName("ResultJsonPropertyNames"), name))));

    private static ExpressionStatementSyntax WriteWithConverter(
        string converterName,
        string valueName) =>
        ExpressionStatement(Invoke(
            Member(IdentifierName(converterName), "Write"),
            Argument(IdentifierName("writer")),
            Argument(IdentifierName(valueName)),
            Argument(IdentifierName("options"))));

    private static InvocationExpressionSyntax ReadWithConverter(
        string converterName,
        TypeSyntax type,
        string readerName) =>
        Invoke(
            Member(IdentifierName(converterName), "Read"),
            Argument(IdentifierName(readerName)).WithRefKindKeyword(Token(SyntaxKind.RefKeyword)),
            Argument(TypeOfExpression(type)),
            Argument(IdentifierName("options")));

    private static ExpressionSyntax ReaderTokenIs(string token) =>
        BinaryExpression(
            SyntaxKind.EqualsExpression,
            Member(IdentifierName("reader"), "TokenType"),
            Member(IdentifierName("JsonTokenType"), token));

    private static ExpressionSyntax ReaderTokenIsNot(string token) =>
        BinaryExpression(
            SyntaxKind.NotEqualsExpression,
            Member(IdentifierName("reader"), "TokenType"),
            Member(IdentifierName("JsonTokenType"), token));

    private static InvocationExpressionSyntax ReaderValueEquals(ExpressionSyntax value) =>
        Invoke(Member(IdentifierName("reader"), "ValueTextEquals"), Argument(value));

    private static ArgumentSyntax OutVariable(string name) =>
        Argument(DeclarationExpression(
                IdentifierName("var"),
                SingleVariableDesignation(Identifier(name))))
            .WithRefKindKeyword(Token(SyntaxKind.OutKeyword));

    private static ThrowStatementSyntax ThrowJson(string message) =>
        ThrowStatement(NewJsonException(message));

    private static ObjectCreationExpressionSyntax NewJsonException(string message) =>
        ObjectCreationExpression(IdentifierName("JsonException"))
            .AddArgumentListArguments(Argument(StringLiteral(message)));

    private static InvocationExpressionSyntax Invoke(
        ExpressionSyntax expression,
        params ArgumentSyntax[] arguments) =>
        InvocationExpression(expression).AddArgumentListArguments(arguments);

    private static MemberAccessExpressionSyntax Member(ExpressionSyntax expression, string name) =>
        MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            expression,
            IdentifierName(name));

    private static MemberAccessExpressionSyntax GenericMember(
        ExpressionSyntax expression,
        string name,
        params TypeSyntax[] typeArguments) =>
        MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            expression,
            GenericName(name).AddTypeArgumentListArguments(typeArguments));

    private static LiteralExpressionSyntax StringLiteral(string value) =>
        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));

    private static ExpressionSyntax Utf8Literal(string value) => ParseExpression($"{Literal(value)}u8");

    private static LiteralExpressionSyntax NumericLiteral(int value) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    private static TypeParameterSyntax[] ErrorTypeParameters(int errorCount) =>
        Enumerable.Range(1, errorCount)
            .Select(index => TypeParameter($"TError{index}"))
            .ToArray();

    private static TypeParameterConstraintClauseSyntax[] ErrorConstraints(int errorCount) =>
        Enumerable.Range(1, errorCount)
            .Select(index => TypeParameterConstraintClause($"TError{index}")
                .AddConstraints(TypeConstraint(IdentifierName("Error"))))
            .ToArray();

    private static TypeSyntax ErrorType(int index) => IdentifierName($"TError{index}");

    private static TypeSyntax ResultType(int errorCount, bool unbound = false) =>
        unbound
            ? UnboundGenericType("Result", errorCount + 1)
            : GenericName("Result").AddTypeArgumentListArguments(
                new[] { (TypeSyntax)IdentifierName("TValue") }
                    .Concat(Enumerable.Range(1, errorCount).Select(ErrorType))
                    .ToArray());

    private static TypeSyntax ConverterType(int errorCount, bool unbound = false) =>
        unbound
            ? UnboundGenericType("ResultJsonConverter", errorCount + 1)
            : GenericName("ResultJsonConverter").AddTypeArgumentListArguments(
                new[] { (TypeSyntax)IdentifierName("TValue") }
                    .Concat(Enumerable.Range(1, errorCount).Select(ErrorType))
                    .ToArray());

    private static TypeSyntax UnboundGenericType(string name, int arity)
    {
        var arguments = new List<SyntaxNodeOrToken>(arity * 2 - 1);
        for (var index = 0; index < arity; index++)
        {
            if (index > 0) arguments.Add(Token(SyntaxKind.CommaToken));
            arguments.Add(OmittedTypeArgument());
        }

        return GenericName(name).WithTypeArgumentList(
            TypeArgumentList(SeparatedList<TypeSyntax>(arguments)));
    }
}
