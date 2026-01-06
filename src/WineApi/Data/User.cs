using System;
using System.Collections.Generic;

namespace WineApi.Data;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? Key { get; set; }

    public DateTime? KeyExpires { get; set; }

    public string Password { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string? LastOn { get; set; }
}
