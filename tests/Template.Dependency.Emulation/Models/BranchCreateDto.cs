// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Dependency.Emulation.Models;

public record BranchCreateDto(
    string Country, string City, string Street, int BuildingNumber, DateTime AddedAt, string? Phone);
