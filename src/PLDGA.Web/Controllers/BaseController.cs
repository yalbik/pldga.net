using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PLDGA.Web.Controllers;

public abstract class BaseController : Controller
{
    protected Guid? CurrentMemberId
    {
        get
        {
            var claim = User.FindFirst("MemberId");
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }
    }

    protected bool IsAdmin => User.IsInRole("Admin");
}
