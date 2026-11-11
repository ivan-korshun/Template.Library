// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Dependency.Emulation.Models;

public class Branch(
    int id, string country, string city, string street, int buildingNumber, DateTime addedAt, string? phone = null)
{
    public int Id { get; set; } = id;
    public string Country { get; set; } = country;
    public string City { get; set; } = city;
    public string Street { get; set; } = street;
    public int BuildingNumber { get; set; } = buildingNumber;
    public DateTime AddedAt { get; set; } = addedAt;
    public string? Phone { get; set; } = phone;
}
