// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.E2ETests.TestModels;

public record CheckpointPage<T>(ICollection<T> Data, string? From = null, string? Next = null) : Page<T>(Data);
