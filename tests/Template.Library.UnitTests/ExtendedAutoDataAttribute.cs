// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture;
using AutoFixture.NUnit4;

namespace Template.Library.UnitTests;

public class ExtendedAutoDataAttribute() : AutoDataAttribute(CreateFixture)
{
    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Customize<DateOnly>(composer => composer.FromFactory((DateTime dt) => DateOnly.FromDateTime(dt)));
        fixture.Customize<TimeOnly>(composer => composer.FromFactory((DateTime dt) => TimeOnly.FromDateTime(dt)));

        return fixture;
    }
}
