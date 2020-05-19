using APPHospitalCore_Alberto.Filters;
using APPHospitalCore_Alberto.Repositories;
using APPHospitalCore_Alberto.ViewModels;
using HospitalNuget.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace APPHospital_Alberto.Controllers
{
    public class ManageController : Controller
    {
        IRepositoryHospital repo;
        public ManageController(IRepositoryHospital repo)
        {
            this.repo = repo;
        }
        // GET: Login
        public IActionResult Login()
        {
            ViewData["Mensaje"] = TempData["Mensaje"];
            return View();
        }
        //POST: Login
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (TempData["EMAIL"] != null && TempData["PASSWORD"] != null)
            {
                email = TempData["EMAIL"].ToString();
                password = TempData["PASSWORD"].ToString();
            }

            string token = await repo.GetToken(email, password);
            if (token == null)
            {
                ViewData["Mensaje"] = "Usuario/Password incorrectos o su cuenta ha sido desactivada";
                return View();
            }
            HttpContext.Session.SetString("token", token);
            Usuarios usuario = await repo.GetUserLogin(token);

            if (usuario != null)
            {
                ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, usuario.UserId.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Name, usuario.Email));
                identity.AddClaim(new Claim(ClaimTypes.Role, usuario.NombreRole));
                identity.AddClaim(new Claim("Password", usuario.Password));
                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.Now.AddMinutes(60)
                    });

                HttpContext.Session.SetInt32("userLogged", usuario.UserId);
                string existe = await repo.ExisteTipoUsuario(usuario.UserId);
                if (existe.Equals("SinPerfil") && usuario.NombreRole != "Administrador")
                {
                    Usuarios user = await repo.FindUser(usuario.UserId, token);
                    TempData["User"] = JsonConvert.SerializeObject(user);
                    TempData["EMAIL"] = email;
                    TempData["PASSWORD"] = password;

                    string noPerfil = await repo.CrearPerfilUsuario(user.UserId);
                    if (noPerfil.Equals("PacieNoPerfil")) return RedirectToAction("CrearPaciente", "Paciente");
                    else if (noPerfil.Equals("PersoNoPerfil")) return RedirectToAction("CrearPersonal", "Personal");
                }

                if (TempData["CONTROLLER"] != null && TempData["ACTION"] != null && existe != "SinPerfil")
                {
                    string controller = TempData["CONTROLLER"].ToString();
                    string action = TempData["ACTION"].ToString();

                    return RedirectToAction(action, controller);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ViewBag.Mensaje = "Usuario/Password incorrectos o su cuenta ha sido desactivada";
                return View();
            }
        }
        //GET: LogOut
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["EMAIL"] = null;
            TempData["PASSWORD"] = null;
            HttpContext.Session.Remove("userLogged");

            return RedirectToAction("Index", "Home");
        }
        //GET: RegistrarUsuario
        public ActionResult RegistrarUsuario()
        {
            return View();
        }
        //POST: RegistrarUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarUsuario(Usuarios user)
        {
            if (ModelState.IsValid)
            {
                List<CustomValidationResult> validaciones = await repo.Validate(user);
                if (validaciones != null && validaciones.Count > 0)
                {
                    foreach (CustomValidationResult vali in validaciones)
                    {
                        foreach (string v in vali.MemberNames)
                        {
                            if (v == "Email")
                            {
                                ViewData["validarEmail"] = vali.ErrorMessage;
                                return View();
                            }
                        }
                    }
                }
                Role role;
                string email = user.Email;
                string password = user.Password;
                string confirmPassword = user.ComparePassword;
                if (user.Role == 0) role = Role.Paciente;
                else role = user.Role;

                await repo.CrearUsuario(email, password, confirmPassword, role);

                if (User.IsInRole("Administrador"))
                {
                    return RedirectToAction("ListaUsuarios", "Administrador");
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View();
            }
        }
        [AutorizacionUsuarios("Administrador", "Paciente", "Médico")]
        //GET: PerfilUsuario
        public async Task<IActionResult> VerPerfil(int userLogged)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            string existe = await repo.ExisteTipoUsuario(userLogged);
            Perfil_ViewModel perfil = new Perfil_ViewModel();
            if (existe.Equals("Paciente"))
            {
                Paciente paciente = await repo.FindPaciente(userLogged, token);
                perfil.Paciente = paciente;
                ViewBag.Edad = await repo.EdadPaciente(paciente.Fecha_Nacimiento);
                return View(perfil);
            }
            else if (existe.Equals("Personal"))
            {
                Personal personal = await repo.FindPersonal(userLogged, token);
                perfil.Personal = personal;
                ViewBag.Edad = await repo.EdadPaciente(personal.Fecha_Nacimiento);
                return View(perfil);
            }
            else
            {
                Usuarios user = await repo.FindUser(userLogged, token);
                TempData["User"] = JsonConvert.SerializeObject(user);
                if (user.NombreRole.Equals("Médico")) return RedirectToAction("CrearPersonal", "Personal");
                else return RedirectToAction("CrearPaciente","Paciente");
            }
        }

        //GET: EditarPerfil
        [AutorizacionUsuarios("Administrador","Paciente","Médico")]
        public async Task<IActionResult> EditarPerfil(int? pacienteId, int? personalId)
        {
            Perfil_ViewModel perfil = new Perfil_ViewModel();
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            if (pacienteId != null)
            {
                Paciente paciente = await repo.FindPacienteById(pacienteId.GetValueOrDefault(), token);
                perfil.Paciente = paciente;
            }
            else
            {
                Personal personal = await repo.FindPersonalById(personalId.GetValueOrDefault(), token);
                perfil.Personal = personal;
                ViewBag.Especialidades = await repo.GetEspecialidades();
            }
            int userId = int.Parse(HttpContext.Session.GetInt32("userLogged").ToString());
            TempData["userLogged"] = userId;
            ViewBag.UserId = userId;

            return View(perfil);
        }

        //POST: EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AutorizacionUsuarios("Administrador", "Paciente", "Médico")]
        public async Task<IActionResult> EditarPerfil(Perfil_ViewModel p)
        {
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString("token");
                if (token == null)
                {
                    TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                    return RedirectToAction("Login", "Manage");
                }
                int userLogged = 0;
                if (p.Paciente != null)
                {
                    await repo.EditarPaciente(p.Paciente.PacienteId, p.Paciente.DNI, p.Paciente.Nombre, p.Paciente.Apellidos, p.Paciente.Fecha_Nacimiento,
                        p.Paciente.Sexo, p.Paciente.Telefono, p.Paciente.Ciudad, p.Paciente.Direccion, p.Paciente.CP, p.Paciente.Email, p.Paciente.NSS, token);
                    userLogged = p.Paciente.UserId;
                }
                if (p.Personal != null)
                {
                    await repo.EditarPersonal(p.Personal.PersonalId, p.Personal.DNI, p.Personal.Nombre, p.Personal.Apellidos, p.Personal.Fecha_Nacimiento,
                        p.Personal.Telefono, p.Personal.Ciudad, p.Personal.Direccion, p.Personal.Email, p.Personal.NumColegiado, p.Personal.Turno,
                        p.Personal.EspecialidadId, token);
                    userLogged = p.Personal.UserId;
                }
                return RedirectToAction("VerPerfil", new { userLogged = userLogged });
            }
            else {
                ViewBag.UserId = (int)TempData.Peek("userLogged");
                return View(p);
            }
        }

        //GET: SinPermisos
        public ActionResult SinPermisos()
        {
            return View();
        }
    }
}