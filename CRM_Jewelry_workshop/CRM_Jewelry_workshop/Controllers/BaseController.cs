using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using CRM_Jewelry_workshop.Data;
using CRM_Jewelry_workshop.DTOs;
using CRM_Jewelry_workshop.Models;

namespace CRM_Jewelry_workshop.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        protected string CurrentUserRole => User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    }
}
