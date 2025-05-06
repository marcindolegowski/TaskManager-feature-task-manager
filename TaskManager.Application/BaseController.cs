using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Application;

[Route("api/[controller]")]
public abstract class BaseController : Controller { }