using System;
using System.Collections.Generic;
using Sassa.Brm.Common.Helpers;

namespace Sassa.Brm.Common.Models;


public class ActiveUser
{
    public string Name { get; set; } = Guid.NewGuid().ToString();
}
public class ActiveUserList
{
    public List<ActiveUser> Users { get; set; } = new();

    public void RemoveDuplicates()
    {
        Users = Users.Distinct().ToList();
    }   

}
