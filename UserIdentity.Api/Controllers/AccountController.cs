using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserIdentity.Api.Extension;
using UserIdentity.Api.ViewModel;
using UserIdentity.IdentityProvider.Entities;
using UserIdentity.WebUI.Infrastructure.Services;

namespace UserIdentity.Api.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
         UserManager<ApplicationUser> userManager,
         SignInManager<ApplicationUser> signInManager,
         IEmailSender emailSender
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="returnUrl"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> Login(string returnUrl = "")
        //{
        //    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        //    ViewData["ReturnUrl"] = returnUrl;
        //    return Ok();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="userName"></param>
        ///// <param name="password"></param>
        ///// <param name="returnUrl"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<IActionResult> Login(string userName, string password, string returnUrl = null)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    ApplicationUser user = await _userManager.FindByNameAsync(userName);

        //    if (user != null)
        //    {
        //        var result = await _signInManager.PasswordSignInAsync(user, password, true, true);
        //    }

        //    return Ok();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="returnUrl"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public IActionResult Register(string returnUrl = null)
        //{
        //    // If the user is already authenticated we do not need to display the registration page, so we redirect to the landing page.
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        return RedirectToAction("index", "home");
        //    }

        //    ViewData["ReturnUrl"] = returnUrl;
        //    return Ok();
        //}


        /// <summary>
        /// Faz o cadastro de novos usuarios
        /// </summary>
        /// <param name="model">view model com usuario e senha</param>
        /// <param name="returnUrl">URL de retorno apos o cadastro</param>
        /// <returns>Objeto contendo a altura em pés e metros</returns>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] LoginViewModel model, string returnUrl = null)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                UserName = model.UserName,
                RegistrationDate = DateTime.Now
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.EmailConfirmationLink(user.Id, code, Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(ApplicationUser user, string code)
        {
            if (user != null || code != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, code);

                if (result.Succeeded)
                {
                    return Ok();
                }
            }


            return Ok();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="model"></param>
        ///// <returns></returns>
        //[HttpGet]
        //public async Task<IActionResult> GenerateToken([FromBody] LoginViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.FindByEmailAsync(model.Email);

        //        if (user != null)
        //        {
        //            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

        //            if (result.Succeeded)
        //            {
        //                var claims = new[] {
        //                  new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        //                  new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //                };

        //                //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
        //                //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //                //var token = new JwtSecurityToken(_config["Tokens:Issuer"],
        //                //  _config["Tokens:Issuer"],
        //                //  claims,
        //                //  expires: DateTime.Now.AddMinutes(30),
        //                //  signingCredentials: creds);

        //                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(null) });
        //            }
        //        }
        //    }

        //    return BadRequest("Could not create token");
        //}
    }
}