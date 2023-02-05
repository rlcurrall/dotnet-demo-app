using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Core.Models;

public class OffsetLimit
{
    [Required]
    [FromQuery]
    [DefaultValue(0)]
    [Range(0, int.MaxValue)]
    public int Offset { get; set; }

    [Required]
    [FromQuery]
    [DefaultValue(50)]
    [Range(0, int.MaxValue)]
    public int Limit { get; set; }
}