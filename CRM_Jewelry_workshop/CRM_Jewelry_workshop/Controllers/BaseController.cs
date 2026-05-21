using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM_Jewelry_workshop.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    protected string CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value ?? "";
}