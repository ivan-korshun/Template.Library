// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.E2ETests.TestModels;

public record OffsetPage<T>(ICollection<T> Data, int PageNumber, int PageSize, int? Total = null) : Page<T>(Data);
