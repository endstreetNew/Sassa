using System;
using System.Collections.Generic;

namespace Sassa.Brm.Common.Models;

public class ActiveUser
{
    public string Name { get; set; } = Guid.NewGuid().ToString();
}
public class ActiveUserList
{
    public List<ActiveUser> Users { get; set; } = new();
}
