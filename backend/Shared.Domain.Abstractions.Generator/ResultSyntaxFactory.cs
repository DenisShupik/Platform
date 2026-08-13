using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shared.Generator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Shared.Domain.Abstractions.Generator;

internal static class ResultSyntaxFactory
{
    private const string Namespace = "Shared.Domain.Abstractions.Results";

    public static CompilationUnitSyntax CreateRuntime() => CreateCompilationUnit(
        CreateResultState(),
        CreateSuccessOrFactory());

    public static CompilationUnitSyntax CreateFailure(int errorCount) =>
        CreateCompilationUnit(CreateFailureDeclaration(errorCount));

    public static CompilationUnitSyntax CreateResult(int errorCount) =>
        CreateCompilationUnit(CreateResultDeclaration(errorCount));

    public static CompilationUnitSyntax CreateSuccessOr(int errorCount) =>
        CreateCompilationUnit(CreateSuccessOrDeclaration(errorCount));

    private static CompilationUnitSyntax CreateCompilationUnit(params MemberDeclarationSyntax[] members) =>
        CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Diagnostics.CodeAnalysis")),
                UsingDirective(ParseName("System.Runtime.CompilerServices")),
                UsingDirective(ParseName("Shared.Domain.Abstractions")),
                UsingDirective(ParseName("Shared.Domain.Abstractions.Errors")))
            .AddMembers(
                FileScopedNamespaceDeclaration(ParseName(Namespace))
                    .AddMembers(members))
            .ApplyGeneratorDefaults();

    private static ClassDeclarationSyntax CreateResultState()
    {
        var successField = FieldDeclaration(
                VariableDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                    .AddVariables(
                        VariableDeclarator("Success")
                            .WithInitializer(EqualsValueClause(
                                ImplicitObjectCreationExpression().WithArgumentList(ArgumentList())))))
            .AddModifiers(
                Token(SyntaxKind.InternalKeyword),
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.ReadOnlyKeyword));

        return ClassDeclaration("ResultState")
            .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword))
            .AddMembers(
                successField,
                CreateThrowMethod(
                    "ThrowUninitialized",
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    "A default Result or SuccessOr is not initialized."),
                CreateThrowMethod(
                    "ThrowInvalidFailure",
                    IdentifierName("T"),
                    "A default or invalid Failure cannot be propagated.",
                    TypeParameter("T")));
    }

    private static MethodDeclarationSyntax CreateThrowMethod(
        string name,
        TypeSyntax returnType,
        string message,
        TypeParameterSyntax? typeParameter = null)
    {
        var declaration = MethodDeclaration(returnType, name)
            .AddAttributeLists(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("DoesNotReturn")))))
            .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword))
            .WithExpressionBody(ArrowExpressionClause(
                ThrowExpression(
                    ObjectCreationExpression(IdentifierName("InvalidOperationException"))
                        .AddArgumentListArguments(Argument(StringLiteral(message))))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        return typeParameter is null
            ? declaration
            : declaration.AddTypeParameterListParameters(typeParameter);
    }

    private static ClassDeclarationSyntax CreateSuccessOrFactory()
    {
        var successProperty = PropertyDeclaration(IdentifierName("Success"), "Success")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .WithExpressionBody(ArrowExpressionClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression)))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        return ClassDeclaration("SuccessOr")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .AddMembers(successProperty);
    }

    private static StructDeclarationSyntax CreateFailureDeclaration(int errorCount)
    {
        var declaration = StructDeclaration("Failure")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword))
            .AddTypeParameterListParameters(ErrorTypeParameters(errorCount))
            .AddConstraintClauses(ErrorConstraints(errorCount))
            .AddMembers(CreateErrorField());

        if (errorCount > 1)
            declaration = declaration.AddMembers(CreateErrorIndexField());
        else
            declaration = declaration.AddMembers(CreateSingleErrorIndexProperty());

        declaration = declaration.AddMembers(
            CreateFailureConstructor(errorCount),
            CreateFailureFactory(errorCount),
            CreateGetErrorOrThrow(),
            CreateTryGetError());

        for (var index = 1; index <= errorCount; index++)
            declaration = declaration.AddMembers(CreateErrorConversion("Failure", errorCount, index));

        return declaration.AddMembers(CreateFailureMatch(errorCount));
    }

    private static StructDeclarationSyntax CreateResultDeclaration(int errorCount)
    {
        var declaration = StructDeclaration("Result")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword))
            .AddTypeParameterListParameters(
                new[] { TypeParameter("TValue") }.Concat(ErrorTypeParameters(errorCount)).ToArray())
            .AddBaseListTypes(SimpleBaseType(ResultContractType()))
            .AddConstraintClauses(ValueConstraint())
            .AddConstraintClauses(ErrorConstraints(errorCount))
            .AddMembers(
                CreateField(IdentifierName("TValue"), "_value"),
                CreateField(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))), "_state"));

        if (errorCount > 1)
            declaration = declaration.AddMembers(CreateErrorIndexField("_errorIndex", SyntaxKind.PrivateKeyword));

        declaration = declaration.AddMembers(
            CreateResultValueConstructor(errorCount),
            CreateErrorConstructor("Result", errorCount),
            CreateValueConversion(errorCount));

        for (var index = 1; index <= errorCount; index++)
            declaration = declaration.AddMembers(CreateErrorConversion("Result", errorCount, index));

        declaration = declaration.AddMembers(CreateFailureConversions("Result", errorCount).ToArray());

        return declaration.AddMembers(
            CreateIsSuccessProperty(),
            CreateIsFailureProperty(),
            CreateTryGetValue(errorCount, includeFailure: false),
            CreateTryGetValue(errorCount, includeFailure: true),
            CreateTryGetFailure(errorCount),
            CreateResultMatch(errorCount),
            CreateResultSwitch(errorCount),
            CreateMap(errorCount),
            CreateBind(errorCount),
            CreateGetFailure(errorCount),
            CreateEnsureInitialized());
    }

    private static StructDeclarationSyntax CreateSuccessOrDeclaration(int errorCount)
    {
        var declaration = StructDeclaration("SuccessOr")
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword))
            .AddTypeParameterListParameters(ErrorTypeParameters(errorCount))
            .AddBaseListTypes(SimpleBaseType(IdentifierName("IResult")))
            .AddConstraintClauses(ErrorConstraints(errorCount))
            .AddMembers(CreateField(NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))), "_state"));

        if (errorCount > 1)
            declaration = declaration.AddMembers(CreateErrorIndexField("_errorIndex", SyntaxKind.PrivateKeyword));

        declaration = declaration.AddMembers(
            CreateSuccessOrSuccessConstructor(errorCount),
            CreateErrorConstructor("SuccessOr", errorCount),
            CreateSuccessConversion(errorCount));

        for (var index = 1; index <= errorCount; index++)
            declaration = declaration.AddMembers(CreateErrorConversion("SuccessOr", errorCount, index));

        declaration = declaration.AddMembers(CreateFailureConversions("SuccessOr", errorCount).ToArray());

        return declaration.AddMembers(
            CreateIsSuccessProperty(),
            CreateIsFailureProperty(),
            CreateTryGetFailure(errorCount),
            CreateSuccessOrMatch(errorCount),
            CreateSuccessOrSwitch(errorCount),
            CreateEnsureInitialized());
    }

    private static FieldDeclarationSyntax CreateErrorField() =>
        CreateField(NullableType(IdentifierName("Error")), "Error", SyntaxKind.InternalKeyword);

    private static FieldDeclarationSyntax CreateErrorIndexField(
        string name = "Index",
        SyntaxKind accessibility = SyntaxKind.InternalKeyword) =>
        CreateField(PredefinedType(Token(SyntaxKind.ByteKeyword)), name, accessibility);

    private static FieldDeclarationSyntax CreateField(
        TypeSyntax type,
        string name,
        SyntaxKind accessibility = SyntaxKind.PrivateKeyword) =>
        FieldDeclaration(VariableDeclaration(type).AddVariables(VariableDeclarator(name)))
            .AddModifiers(Token(accessibility), Token(SyntaxKind.ReadOnlyKeyword));

    private static PropertyDeclarationSyntax CreateSingleErrorIndexProperty() =>
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.ByteKeyword)), "Index")
            .AddModifiers(Token(SyntaxKind.InternalKeyword))
            .WithExpressionBody(ArrowExpressionClause(
                ConditionalExpression(
                    IsPatternExpression(
                        IdentifierName("Error"),
                        ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                    ByteLiteral(0),
                    ByteLiteral(1))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static ConstructorDeclarationSyntax CreateFailureConstructor(int errorCount)
    {
        var constructor = ConstructorDeclaration("Failure")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("error")).WithType(IdentifierName("Error")),
                Parameter(Identifier("index")).WithType(PredefinedType(Token(SyntaxKind.ByteKeyword))))
            .AddBodyStatements(
                ThrowIfNull("error"),
                Assign("Error", IdentifierName("error")));

        return errorCount > 1
            ? constructor.AddBodyStatements(Assign("Index", IdentifierName("index")))
            : constructor;
    }

    private static MethodDeclarationSyntax CreateFailureFactory(int errorCount) =>
        MethodDeclaration(FailureType(errorCount), "Create")
            .AddModifiers(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("error")).WithType(IdentifierName("Error")),
                Parameter(Identifier("index")).WithType(PredefinedType(Token(SyntaxKind.ByteKeyword))))
            .WithExpressionBody(ArrowExpressionClause(
                ImplicitObjectCreationExpression()
                    .AddArgumentListArguments(Argument(IdentifierName("error")), Argument(IdentifierName("index")))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateGetErrorOrThrow() =>
        MethodDeclaration(IdentifierName("Error"), "GetErrorOrThrow")
            .AddModifiers(Token(SyntaxKind.InternalKeyword))
            .WithExpressionBody(ArrowExpressionClause(
                BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    IdentifierName("Error"),
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("ResultState"),
                            GenericName("ThrowInvalidFailure")
                                .AddTypeArgumentListArguments(IdentifierName("Error"))))
                )))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateTryGetError()
    {
        var parameter = Parameter(Identifier("error"))
            .AddAttributeLists(NotNullWhenAttribute(true))
            .AddModifiers(Token(SyntaxKind.OutKeyword))
            .WithType(NullableType(IdentifierName("TError")));

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), "TryGet")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddTypeParameterListParameters(TypeParameter("TError"))
            .AddConstraintClauses(
                TypeParameterConstraintClause("TError")
                    .AddConstraints(TypeConstraint(IdentifierName("Error"))))
            .AddParameterListParameters(parameter)
            .AddBodyStatements(
                Assign("error", BinaryExpression(
                    SyntaxKind.AsExpression,
                    IdentifierName("Error"),
                    IdentifierName("TError"))),
                ReturnStatement(IsNotNull("error")));
    }

    private static ConversionOperatorDeclarationSyntax CreateErrorConversion(
        string targetName,
        int errorCount,
        int errorIndex)
    {
        var arguments = new List<ArgumentSyntax> { Argument(IdentifierName("error")) };
        if (targetName == "Failure" || errorCount > 1) arguments.Add(Argument(ByteLiteral(errorIndex)));

        return ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword), UnionType(targetName, errorCount))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("error")).WithType(ErrorType(errorIndex)))
            .WithExpressionBody(ArrowExpressionClause(
                ImplicitObjectCreationExpression().AddArgumentListArguments(arguments.ToArray())))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    private static MethodDeclarationSyntax CreateFailureMatch(int errorCount)
    {
        var arms = Enumerable.Range(1, errorCount)
            .Select(index => SwitchExpressionArm(
                ConstantPattern(NumericLiteral(index)),
                Invoke($"error{index}", Cast(ErrorType(index), Invoke("GetErrorOrThrow")))))
            .Append(CreateInvalidFailureArm(IdentifierName("TResult")));

        return MethodDeclaration(IdentifierName("TResult"), "Match")
            .AddAttributeLists(AggressiveInliningAttribute())
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddTypeParameterListParameters(TypeParameter("TResult"))
            .AddParameterListParameters(ErrorDelegateParameters(errorCount, "Func", IdentifierName("TResult")))
            .WithExpressionBody(ArrowExpressionClause(
                SwitchExpression(IdentifierName("Index")).WithArms(SeparatedList(arms))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    private static ConstructorDeclarationSyntax CreateResultValueConstructor(int errorCount)
    {
        var statements = new List<StatementSyntax>
        {
            ThrowIfNull("value"),
            Assign("_value", IdentifierName("value")),
            Assign("_state", IdentifierName("ResultState").Member("Success"))
        };
        if (errorCount > 1) statements.Add(Assign("_errorIndex", ByteLiteral(0)));

        return ConstructorDeclaration("Result")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(Parameter(Identifier("value")).WithType(IdentifierName("TValue")))
            .AddBodyStatements(statements.ToArray());
    }

    private static ConstructorDeclarationSyntax CreateSuccessOrSuccessConstructor(int errorCount)
    {
        var statements = new List<StatementSyntax>
        {
            Assign("_state", IdentifierName("ResultState").Member("Success"))
        };
        if (errorCount > 1) statements.Add(Assign("_errorIndex", ByteLiteral(0)));

        return ConstructorDeclaration("SuccessOr")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(Parameter(Identifier("success")).WithType(IdentifierName("Success")))
            .AddBodyStatements(statements.ToArray());
    }

    private static ConstructorDeclarationSyntax CreateErrorConstructor(string typeName, int errorCount)
    {
        var statements = new List<StatementSyntax> { ThrowIfNull("error") };
        if (typeName == "Result")
            statements.Add(Assign(
                "_value",
                PostfixUnaryExpression(
                    SyntaxKind.SuppressNullableWarningExpression,
                    LiteralExpression(SyntaxKind.DefaultLiteralExpression))));
        statements.Add(Assign("_state", IdentifierName("error")));
        if (errorCount > 1) statements.Add(Assign("_errorIndex", IdentifierName("errorIndex")));

        var constructor = ConstructorDeclaration(typeName)
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddParameterListParameters(Parameter(Identifier("error")).WithType(IdentifierName("Error")));
        if (errorCount > 1)
            constructor = constructor.AddParameterListParameters(
                Parameter(Identifier("errorIndex")).WithType(PredefinedType(Token(SyntaxKind.ByteKeyword))));

        return constructor.AddBodyStatements(statements.ToArray());
    }

    private static ConversionOperatorDeclarationSyntax CreateValueConversion(int errorCount) =>
        ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword), ResultType(errorCount))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(Parameter(Identifier("value")).WithType(IdentifierName("TValue")))
            .WithExpressionBody(ArrowExpressionClause(
                ImplicitObjectCreationExpression()
                    .AddArgumentListArguments(Argument(IdentifierName("value")))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static ConversionOperatorDeclarationSyntax CreateSuccessConversion(int errorCount) =>
        ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword), SuccessOrType(errorCount))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(Parameter(Identifier("success")).WithType(IdentifierName("Success")))
            .WithExpressionBody(ArrowExpressionClause(
                ImplicitObjectCreationExpression()
                    .AddArgumentListArguments(Argument(IdentifierName("success")))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static IEnumerable<MemberDeclarationSyntax> CreateFailureConversions(string targetName, int errorCount)
    {
        for (var length = 1; length <= errorCount; length++)
        for (var start = 1; start <= errorCount - length + 1; start++)
            yield return CreateFailureConversion(targetName, errorCount, start, length);
    }

    private static ConversionOperatorDeclarationSyntax CreateFailureConversion(
        string targetName,
        int errorCount,
        int start,
        int length)
    {
        var sourceErrorTypes = Enumerable.Range(start, length).Select(ErrorType).ToArray();
        var arms = Enumerable.Range(1, length)
            .Select(sourceIndex =>
            {
                var arguments = new List<ArgumentSyntax>
                {
                    Argument(InvokeMember("failure", "GetErrorOrThrow"))
                };
                if (errorCount > 1) arguments.Add(Argument(ByteLiteral(start + sourceIndex - 1)));

                return SwitchExpressionArm(
                    ConstantPattern(NumericLiteral(sourceIndex)),
                    ImplicitObjectCreationExpression().AddArgumentListArguments(arguments.ToArray()));
            })
            .Append(CreateInvalidFailureArm(UnionType(targetName, errorCount)));

        return ConversionOperatorDeclaration(Token(SyntaxKind.ImplicitKeyword), UnionType(targetName, errorCount))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("failure")).WithType(GenericName("Failure")
                    .AddTypeArgumentListArguments(sourceErrorTypes)))
            .WithExpressionBody(ArrowExpressionClause(
                SwitchExpression(IdentifierName("failure").Member("Index"))
                    .WithArms(SeparatedList(arms))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    private static PropertyDeclarationSyntax CreateIsSuccessProperty() =>
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), "IsSuccess")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(Block(
                        ExpressionStatement(Invoke("EnsureInitialized")),
                        ReturnStatement(IsSuccessExpression()))));

    private static PropertyDeclarationSyntax CreateIsFailureProperty() =>
        PropertyDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), "IsFailure")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithExpressionBody(ArrowExpressionClause(
                PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, IdentifierName("IsSuccess"))))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateTryGetValue(int errorCount, bool includeFailure)
    {
        var method = MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), "TryGetValue")
            .AddAttributeLists(AggressiveInliningAttribute())
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("value"))
                    .AddAttributeLists(NotNullWhenAttribute(true))
                    .AddModifiers(Token(SyntaxKind.OutKeyword))
                    .WithType(NullableType(IdentifierName("TValue"))));

        if (includeFailure)
            method = method.AddParameterListParameters(
                Parameter(Identifier("failure"))
                    .AddModifiers(Token(SyntaxKind.OutKeyword))
                    .WithType(FailureType(errorCount)));

        var successStatements = new List<StatementSyntax> { Assign("value", IdentifierName("_value")) };
        if (includeFailure)
            successStatements.Add(Assign("failure", LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
        successStatements.Add(ReturnStatement(LiteralExpression(SyntaxKind.TrueLiteralExpression)));

        var statements = new List<StatementSyntax>
        {
            IfStatement(IdentifierName("IsSuccess"), Block(successStatements)),
            Assign("value", LiteralExpression(SyntaxKind.DefaultLiteralExpression))
        };
        if (includeFailure) statements.Add(Assign("failure", CreateFailureExpression(errorCount)));
        statements.Add(ReturnStatement(LiteralExpression(SyntaxKind.FalseLiteralExpression)));

        return method.AddBodyStatements(statements.ToArray());
    }

    private static MethodDeclarationSyntax CreateTryGetFailure(int errorCount) =>
        MethodDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)), "TryGetFailure")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("failure"))
                    .AddModifiers(Token(SyntaxKind.OutKeyword))
                    .WithType(FailureType(errorCount)))
            .AddBodyStatements(
                IfStatement(
                    IdentifierName("IsSuccess"),
                    Block(
                        Assign("failure", LiteralExpression(SyntaxKind.DefaultLiteralExpression)),
                        ReturnStatement(LiteralExpression(SyntaxKind.FalseLiteralExpression)))),
                Assign("failure", CreateFailureExpression(errorCount)),
                ReturnStatement(LiteralExpression(SyntaxKind.TrueLiteralExpression)));

    private static MethodDeclarationSyntax CreateResultMatch(int errorCount) =>
        MethodDeclaration(IdentifierName("TResult"), "Match")
            .AddAttributeLists(AggressiveInliningAttribute())
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddTypeParameterListParameters(TypeParameter("TResult"))
            .AddParameterListParameters(
                new[]
                {
                    Parameter(Identifier("success")).WithType(FuncType(IdentifierName("TValue"), IdentifierName("TResult")))
                }.Concat(ErrorDelegateParameters(errorCount, "Func", IdentifierName("TResult"))).ToArray())
            .AddBodyStatements(
                ExpressionStatement(Invoke("EnsureInitialized")),
                IfStatement(
                    IsSuccessExpression(),
                    ReturnStatement(Invoke("success", IdentifierName("_value")))),
                ReturnStatement(
                    InvokeMember(
                        Invoke("GetFailure"),
                        "Match",
                        Enumerable.Range(1, errorCount).Select(i => (ExpressionSyntax)IdentifierName($"error{i}")).ToArray())));

    private static MethodDeclarationSyntax CreateSuccessOrMatch(int errorCount) =>
        MethodDeclaration(IdentifierName("TResult"), "Match")
            .AddAttributeLists(AggressiveInliningAttribute())
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddTypeParameterListParameters(TypeParameter("TResult"))
            .AddParameterListParameters(
                new[]
                {
                    Parameter(Identifier("success")).WithType(GenericName("Func")
                        .AddTypeArgumentListArguments(IdentifierName("TResult")))
                }.Concat(ErrorDelegateParameters(errorCount, "Func", IdentifierName("TResult"))).ToArray())
            .AddBodyStatements(
                ExpressionStatement(Invoke("EnsureInitialized")),
                IfStatement(IsSuccessExpression(), ReturnStatement(Invoke("success"))),
                ReturnStatement(
                    InvokeMember(
                        CreateFailureExpression(errorCount),
                        "Match",
                        Enumerable.Range(1, errorCount).Select(i => (ExpressionSyntax)IdentifierName($"error{i}")).ToArray())));

    private static MethodDeclarationSyntax CreateResultSwitch(int errorCount) =>
        CreateSwitchMethod(
            errorCount,
            Parameter(Identifier("success")).WithType(ActionType(IdentifierName("TValue"))),
            new StatementSyntax[]
            {
                ExpressionStatement(Invoke("success", IdentifierName("_value"))),
                ReturnStatement()
            });

    private static MethodDeclarationSyntax CreateSuccessOrSwitch(int errorCount) =>
        CreateSwitchMethod(
            errorCount,
            Parameter(Identifier("success")).WithType(IdentifierName("Action")),
            new StatementSyntax[]
            {
                ExpressionStatement(Invoke("success")),
                ReturnStatement()
            });

    private static MethodDeclarationSyntax CreateSwitchMethod(
        int errorCount,
        ParameterSyntax successParameter,
        IReadOnlyList<StatementSyntax> successStatements)
    {
        var sections = Enumerable.Range(1, errorCount)
            .Select(index => SwitchSection()
                .AddLabels(CaseSwitchLabel(NumericLiteral(index)))
                .AddStatements(
                    ExpressionStatement(Invoke(
                        $"error{index}",
                        Cast(ErrorType(index), PostfixUnaryExpression(
                            SyntaxKind.SuppressNullableWarningExpression,
                            IdentifierName("_state"))))),
                    ReturnStatement()))
            .Append(SwitchSection()
                .AddLabels(DefaultSwitchLabel())
                .AddStatements(ThrowStatement(
                    ObjectCreationExpression(IdentifierName("InvalidOperationException"))
                        .WithArgumentList(ArgumentList()))));

        var failureDispatch = errorCount == 1
            ? new StatementSyntax[]
            {
                ExpressionStatement(Invoke(
                    "error1",
                    Cast(
                        ErrorType(1),
                        PostfixUnaryExpression(
                            SyntaxKind.SuppressNullableWarningExpression,
                            IdentifierName("_state"))))),
                ReturnStatement()
            }
            : new StatementSyntax[]
            {
                SwitchStatement(ErrorIndexExpression(errorCount)).AddSections(sections.ToArray())
            };

        return MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "Switch")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddParameterListParameters(
                new[] { successParameter }
                    .Concat(ErrorDelegateParameters(errorCount, "Action", resultType: null))
                    .ToArray())
            .AddBodyStatements(
                new StatementSyntax[]
                {
                    ExpressionStatement(Invoke("EnsureInitialized")),
                    IfStatement(IsSuccessExpression(), Block(successStatements))
                }.Concat(failureDispatch).ToArray());
    }

    private static MethodDeclarationSyntax CreateMap(int errorCount) =>
        MethodDeclaration(ResultType(errorCount, IdentifierName("TNext")), "Map")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddTypeParameterListParameters(TypeParameter("TNext"))
            .AddConstraintClauses(NotNullConstraint("TNext"))
            .AddParameterListParameters(
                Parameter(Identifier("selector")).WithType(FuncType(IdentifierName("TValue"), IdentifierName("TNext"))))
            .AddBodyStatements(
                ThrowIfNull("selector"),
                IfStatement(
                    InvocationExpression(IdentifierName("TryGetValue"))
                        .AddArgumentListArguments(
                            OutVarArgument("value"),
                            OutVarArgument("failure")),
                    ReturnStatement(Invoke("selector", IdentifierName("value")))),
                ReturnStatement(IdentifierName("failure")));

    private static MethodDeclarationSyntax CreateBind(int errorCount) =>
        MethodDeclaration(ResultType(errorCount, IdentifierName("TNext")), "Bind")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddTypeParameterListParameters(TypeParameter("TNext"))
            .AddConstraintClauses(NotNullConstraint("TNext"))
            .AddParameterListParameters(
                Parameter(Identifier("selector")).WithType(
                    FuncType(IdentifierName("TValue"), ResultType(errorCount, IdentifierName("TNext")))))
            .AddBodyStatements(
                ThrowIfNull("selector"),
                IfStatement(
                    InvocationExpression(IdentifierName("TryGetValue"))
                        .AddArgumentListArguments(
                            OutVarArgument("value"),
                            OutVarArgument("failure")),
                    ReturnStatement(Invoke("selector", IdentifierName("value")))),
                ReturnStatement(IdentifierName("failure")));

    private static MethodDeclarationSyntax CreateGetFailure(int errorCount) =>
        MethodDeclaration(FailureType(errorCount), "GetFailure")
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .WithExpressionBody(ArrowExpressionClause(CreateFailureExpression(errorCount)))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

    private static MethodDeclarationSyntax CreateEnsureInitialized() =>
        MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "EnsureInitialized")
            .AddAttributeLists(AggressiveInliningAttribute())
            .AddModifiers(Token(SyntaxKind.PrivateKeyword))
            .AddBodyStatements(
                IfStatement(
                    IsPatternExpression(
                        IdentifierName("_state"),
                        ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                    ExpressionStatement(InvokeMember("ResultState", "ThrowUninitialized"))));

    private static InvocationExpressionSyntax CreateFailureExpression(int errorCount) =>
        InvocationExpression(FailureType(errorCount).Member("Create"))
            .AddArgumentListArguments(
                Argument(Cast(
                    IdentifierName("Error"),
                    PostfixUnaryExpression(
                        SyntaxKind.SuppressNullableWarningExpression,
                        IdentifierName("_state")))),
                Argument(ErrorIndexExpression(errorCount)));

    private static TypeParameterSyntax[] ErrorTypeParameters(int count) =>
        Enumerable.Range(1, count).Select(index => TypeParameter($"TError{index}")).ToArray();

    private static TypeParameterConstraintClauseSyntax[] ErrorConstraints(int count) =>
        Enumerable.Range(1, count)
            .Select(index => TypeParameterConstraintClause($"TError{index}")
                .AddConstraints(TypeConstraint(IdentifierName("Error"))))
            .ToArray();

    private static TypeParameterConstraintClauseSyntax ValueConstraint() => NotNullConstraint("TValue");

    private static TypeParameterConstraintClauseSyntax NotNullConstraint(string typeParameter) =>
        TypeParameterConstraintClause(typeParameter)
            .AddConstraints(TypeConstraint(IdentifierName("notnull")));

    private static TypeSyntax ErrorType(int index) => IdentifierName($"TError{index}");

    private static TypeSyntax FailureType(int errorCount) =>
        GenericName("Failure").AddTypeArgumentListArguments(
            Enumerable.Range(1, errorCount).Select(ErrorType).ToArray());

    private static TypeSyntax ResultType(int errorCount, TypeSyntax? valueType = null) =>
        GenericName("Result").AddTypeArgumentListArguments(
            new[] { valueType ?? IdentifierName("TValue") }
                .Concat(Enumerable.Range(1, errorCount).Select(ErrorType))
                .ToArray());

    private static TypeSyntax SuccessOrType(int errorCount) =>
        GenericName("SuccessOr").AddTypeArgumentListArguments(
            Enumerable.Range(1, errorCount).Select(ErrorType).ToArray());

    private static TypeSyntax UnionType(string name, int errorCount) => name switch
    {
        "Failure" => FailureType(errorCount),
        "Result" => ResultType(errorCount),
        "SuccessOr" => SuccessOrType(errorCount),
        _ => throw new System.ArgumentOutOfRangeException(nameof(name))
    };

    private static TypeSyntax ResultContractType() =>
        GenericName("IResult").AddTypeArgumentListArguments(IdentifierName("TValue"));

    private static ParameterSyntax[] ErrorDelegateParameters(
        int errorCount,
        string delegateName,
        TypeSyntax? resultType)
    {
        return Enumerable.Range(1, errorCount)
            .Select(index => Parameter(Identifier($"error{index}"))
                .WithType(resultType is null
                    ? ActionType(ErrorType(index))
                    : FuncType(ErrorType(index), resultType)))
            .ToArray();
    }

    private static TypeSyntax FuncType(TypeSyntax input, TypeSyntax output) =>
        GenericName("Func").AddTypeArgumentListArguments(input, output);

    private static TypeSyntax ActionType(TypeSyntax input) =>
        GenericName("Action").AddTypeArgumentListArguments(input);

    private static AttributeListSyntax AggressiveInliningAttribute() =>
        AttributeList(SingletonSeparatedList(
            Attribute(IdentifierName("MethodImpl"))
                .AddArgumentListArguments(AttributeArgument(
                    IdentifierName("MethodImplOptions").Member("AggressiveInlining")))));

    private static AttributeListSyntax NotNullWhenAttribute(bool value) =>
        AttributeList(SingletonSeparatedList(
            Attribute(IdentifierName("NotNullWhen"))
                .AddArgumentListArguments(AttributeArgument(
                    LiteralExpression(value
                        ? SyntaxKind.TrueLiteralExpression
                        : SyntaxKind.FalseLiteralExpression)))));

    private static SwitchExpressionArmSyntax CreateInvalidFailureArm(TypeSyntax returnType) =>
        SwitchExpressionArm(
            DiscardPattern(),
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("ResultState"),
                    GenericName("ThrowInvalidFailure")
                        .AddTypeArgumentListArguments(returnType))));

    private static StatementSyntax ThrowIfNull(string identifier) =>
        IfStatement(
            IsPatternExpression(
                IdentifierName(identifier),
                ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
            ThrowStatement(
                ObjectCreationExpression(IdentifierName("ArgumentNullException"))
                    .AddArgumentListArguments(Argument(
                        InvocationExpression(IdentifierName("nameof"))
                            .AddArgumentListArguments(Argument(IdentifierName(identifier)))))));

    private static ExpressionStatementSyntax Assign(string target, ExpressionSyntax value) =>
        ExpressionStatement(AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(target),
            value));

    private static ExpressionSyntax IsSuccessExpression() =>
        Invoke("ReferenceEquals", IdentifierName("_state"), IdentifierName("ResultState").Member("Success"));

    private static ExpressionSyntax IsNotNull(string identifier) =>
        IsPatternExpression(
            IdentifierName(identifier),
            UnaryPattern(
                ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))));

    private static ExpressionSyntax ErrorIndexExpression(int errorCount) =>
        errorCount > 1 ? IdentifierName("_errorIndex") : ByteLiteral(1);

    private static CastExpressionSyntax Cast(TypeSyntax type, ExpressionSyntax expression) =>
        CastExpression(type, expression);

    private static InvocationExpressionSyntax Invoke(string method, params ExpressionSyntax[] arguments) =>
        InvocationExpression(IdentifierName(method))
            .AddArgumentListArguments(arguments.Select(Argument).ToArray());

    private static InvocationExpressionSyntax InvokeMember(
        string target,
        string method,
        params ExpressionSyntax[] arguments) =>
        InvokeMember(IdentifierName(target), method, arguments);

    private static InvocationExpressionSyntax InvokeMember(
        ExpressionSyntax target,
        string method,
        params ExpressionSyntax[] arguments) =>
        InvocationExpression(target.Member(method))
            .AddArgumentListArguments(arguments.Select(Argument).ToArray());

    private static ArgumentSyntax OutVarArgument(string name) =>
        Argument(DeclarationExpression(IdentifierName("var"), SingleVariableDesignation(Identifier(name))))
            .WithRefKindKeyword(Token(SyntaxKind.OutKeyword));

    private static LiteralExpressionSyntax StringLiteral(string value) =>
        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));

    private static LiteralExpressionSyntax NumericLiteral(int value) =>
        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    private static CastExpressionSyntax ByteLiteral(int value) =>
        CastExpression(PredefinedType(Token(SyntaxKind.ByteKeyword)), NumericLiteral(value));
}
