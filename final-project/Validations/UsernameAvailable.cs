﻿using System.ComponentModel.DataAnnotations;
using final_project.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace final_project.Validations;

public class UsernameAvailable : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;
        var username = value.ToString();
        if(username is null)
            return ValidationResult.Success;

        var userService = (UserManager<User>)validationContext.GetService(typeof(UserManager<User>))!;

        return userService.FindByNameAsync(username).Result != null
            ? new ValidationResult("Това потребителско име е заето, опитайте с друго!")
            : ValidationResult.Success;
    }
}