// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit4;

namespace Template.Library.UnitTests;

public class AutoNSubstituteDataAttribute(params Type[] extraCustomizations)
    : AutoDataAttribute(() => CustomFixtureFactory(extraCustomizations))
{
    private static IFixture CustomFixtureFactory(params Type[] extraCustomizations)
    {
        IFixture fixture = new Fixture();
        fixture = fixture.Customize(new AutoNSubstituteCustomization());

        foreach (var extraCustomization in extraCustomizations)
        {
            fixture = fixture.Customize(Activator.CreateInstance(extraCustomization) as ICustomization);
        }

        return fixture;
    }
}
