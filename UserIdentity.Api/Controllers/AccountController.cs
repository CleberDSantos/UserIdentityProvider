using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _config;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="emailSender"></param>
        /// <param name="configuration"></param>
        public AccountController(
         UserManager<ApplicationUser> userManager,
         SignInManager<ApplicationUser> signInManager,
         IEmailSender emailSender,
         IConfiguration configuration
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _config = configuration;
        }//NOSONAR

        /// <summary>
        /// Efetuar o logout do sistema
        /// </summary>
        /// <returns></returns>
        [Route("logout")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return Ok();
        }

        /// <summary>
        /// Efetua o login de um usuario
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return BadRequest("Model state is invalid!");

            ApplicationUser user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
                BadRequest("user is null");

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);

            if (!result.Succeeded)
                BadRequest("result isn't success");

            return Ok();

        }

        /// <summary>
        /// Faz o cadastro de novos usuarios
        /// </summary>
        /// <param name="model">view model com usuario e senha</param>
        /// <param name="returnUrl">URL de retorno apos o cadastro</param>
        /// <returns>Objeto contendo a altura em pés e metros</returns>
        [HttpPost]
        [Route("register")]
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
        /// Reponsavel pela geração do token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("generatetoken")]
        public async Task<IActionResult> GenerateToken([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                    if (result.Succeeded)
                    {
                        var claims = new[] {
                          new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                          _config["Tokens:Issuer"],
                          claims,
                          expires: DateTime.Now.AddMinutes(30),
                          signingCredentials: creds);

                        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
                    }
                }
            }

            return BadRequest("Could not create token");
        }

        /// <summary>
        /// confirmação de e-mail
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal async Task<IActionResult> ConfirmEmail(Guid userId, string code)
        {

            if (string.IsNullOrEmpty(code))
                return BadRequest();

            if (string.IsNullOrEmpty(userId.ToString()))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
                return BadRequest();

            return Ok();
        }


        /// <summary>
        /// confirmação de e-mail
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal async Task<IActionResult> ResetPassword(Guid userId, string code)
        {

            if (userId == null || string.IsNullOrEmpty(code))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (string.IsNullOrEmpty(user.ToString()))
                return BadRequest();

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
                return BadRequest();

            return Ok();
        }
    }
}