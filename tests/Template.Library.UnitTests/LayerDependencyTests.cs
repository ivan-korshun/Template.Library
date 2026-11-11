// Copyright © Korshun Ivan. All rights reserved.

using FluentAssertions;
using NetArchTest.Rules;
using NUnit.Framework;
using Template.Library.Common;

namespace Template.Library.UnitTests;

//         Common
//           ↑
//         Models
//           ↑
//  DataSource.Abstraction
//       ↑        ↑
//    WebApi  DataSource
//       ↑        ↑
//      ProgramSetup

[TestFixture]
public class LayerDependencyTests
{
    private const string Root = "Template.Library";

    private const string Common = $"{Root}.Common";
    private const string Models = $"{Root}.Models";
    private const string DataSource = $"{Root}.DataSource";
    private const string DataSourceAbstraction = $"{DataSource}.Abstraction";
    private const string WebApi = $"{Root}.WebApi";
    private const string ProgramSetup = $"{Root}.ProgramSetup";

    // Issues, discovered in legacy code, to be fixed later
    private static readonly List<string> s_baseline = [];

    private Types _types = null!;
    private string[] _dataSourceExceptAbstraction = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _types = Types.InAssembly(typeof(Program).Assembly);

        _dataSourceExceptAbstraction = _types.GetTypes()
            .Select(type => type.Namespace)
            .Where(ns => ns is not null
                && ns.StartsWith(DataSource, StringComparison.Ordinal)
                && !ns.StartsWith(DataSourceAbstraction, StringComparison.Ordinal))
            .Distinct()
            .ToArray()!;
    }

    [Test]
    public void Namespaces_TopLevel_DependencyOrderTested()
    {
        List<string> topNamespaces =
        [
            Common,
            Models,
            DataSource,
            WebApi,
            ProgramSetup
        ];

        _types.GetTypes()
            .Select(type => type.Namespace)
            .Where(ns => ns != null && ns != Root)
            .Where(ns => !topNamespaces.Any(tns => ns!.StartsWithOrdinal(tns)))
            .Should().BeEmpty();
    }

    [Test]
    public void Common_Always_NotDependOnUpperOrSideLayers()
    {
        TestResult result = _types.That().ResideInNamespace(Common)
            .ShouldNot().HaveDependencyOnAny(
                Models,
                DataSource,
                WebApi,
                ProgramSetup)
            .GetResult();

        (result.FailingTypeNames ?? []).Should().BeSubsetOf(s_baseline);
    }

    [Test]
    public void Models_Always_NotDependOnUpperOrSideLayers()
    {
        TestResult result = _types.That().ResideInNamespace(Models)
            .ShouldNot().HaveDependencyOnAny(
                DataSource,
                WebApi,
                ProgramSetup)
            .GetResult();

        (result.FailingTypeNames ?? []).Should().BeSubsetOf(s_baseline);
    }

    [Test]
    public void DataSourceAbstraction_Always_NotDependOnUpperOrSideLayers()
    {
        TestResult result = _types.That().ResideInNamespace(DataSourceAbstraction)
            .ShouldNot().HaveDependencyOnAny([
                .._dataSourceExceptAbstraction,
                WebApi,
                ProgramSetup
            ])
            .GetResult();

        (result.FailingTypeNames ?? []).Should().BeSubsetOf(s_baseline);
    }

    [Test]
    public void DataSource_Always_NotDependOnUpperOrSideLayers()
    {
        TestResult result = _types.That().ResideInNamespace(DataSource)
            .ShouldNot().HaveDependencyOnAny(
                WebApi,
                ProgramSetup)
            .GetResult();

        (result.FailingTypeNames ?? []).Should().BeSubsetOf(s_baseline);
    }

    [Test]
    public void WebApi_Always_NotDependOnUpperOrSideLayers()
    {
        TestResult result = _types.That().ResideInNamespace(WebApi)
            .ShouldNot().HaveDependencyOnAny([
                .._dataSourceExceptAbstraction,
                ProgramSetup
             ])
            .GetResult();

        (result.FailingTypeNames ?? []).Should().BeSubsetOf(s_baseline);
    }
}
