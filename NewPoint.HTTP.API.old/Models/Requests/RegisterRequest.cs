﻿namespace NewPoint.Models.Requests;

public class RegisterRequest
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string BirthDate { get; set; }
}
