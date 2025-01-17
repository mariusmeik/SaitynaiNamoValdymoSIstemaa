﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaitynaiNamoValdymoSIstema.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SignInResult = SaitynaiNamoValdymoSIstema.Services.SignInResult;

namespace SaitynaiNamoValdymoSIstema.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager _signInManager;

        public AccountController(ILogger<AccountController> logger,
                                SignInManager signInManager,
                                JWTAuthService jwtAuthManager)
        {
            _logger = logger;
            _signInManager = signInManager;
        }


        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SignInResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {

            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            var result = await _signInManager.SignIn(request.UserName, request.Password);

            if (!result.Success) return Unauthorized();

            _logger.LogInformation($"User [{request.UserName}] logged in the system.");

            return Ok(new LoginResult
            {
                UserName = result.User.Name,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken,
                Role=result.User.Role
            });
        }



        [HttpPost("refreshtoken")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SignInResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }

            var result = await _signInManager.RefreshToken(request.AccessToken, request.RefreshToken);

            if (!result.Success) return Unauthorized();

            return Ok(new LoginResult
            {
                UserName = result.User.Name,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

    }

    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResult
    {
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Role { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
