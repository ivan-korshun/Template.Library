// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Dependency.Emulation.Models;

public record Page<T>(ICollection<T> Data);
