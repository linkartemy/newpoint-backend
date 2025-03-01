﻿namespace NewPoint.Models.Requests;

public class EditProfileRequest
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public DateTime BirthDate { get; set; }
}
