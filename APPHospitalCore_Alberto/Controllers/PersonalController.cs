using APPHospitalCore_Alberto.Filters;
using APPHospitalCore_Alberto.Repositories;
using HospitalNuget.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APPHospital_Alberto.Controllers
{
    public class PersonalController : Controller
    {
        IRepositoryHospital repo;
        public PersonalController(IRepositoryHospital repo)
        {
            this.repo = repo;
        }
        // GET: Personal
        public IActionResult Index()
        {
            return View();
        }

        //GET: CrearPersonal
        [AutorizacionUsuarios("Administrador", "Médico")]
        public async Task<IActionResult> CrearPersonal()
        {
            Usuarios user = (Usuarios)TempData["User"];
            ViewBag.User = user;
            ViewBag.Especialidades = await repo.GetEspecialidades();
            TempData["User"] = user;

            return View();
        }

        //POST: CrearPersonal
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AutorizacionUsuarios("Administrador", "Médico")]
        public async Task<IActionResult> CrearPersonal(Personal p)
        {
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString("token");
                await repo.CrearPersonal(p.DNI, p.Nombre, p.Apellidos, p.Fecha_Nacimiento, p.Telefono, p.Ciudad, p.Direccion, p.Email,
                    p.NumColegiado, p.Turno, p.EspecialidadId, p.UserId, token);

                if (TempData["CONTROLLER"] != null && TempData["ACTION"] != null)
                {
                    string controller = TempData["CONTROLLER"].ToString();
                    string action = TempData["ACTION"].ToString();

                    return RedirectToAction(action, controller);
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (TempData["EMAIL"] != null && TempData["PASSWORD"] != null)
                {
                    string email = TempData["EMAIL"].ToString();
                    string password = TempData["PASSWORD"].ToString();
                    TempData["EMAIL"] = email;
                    TempData["PASSWORD"] = password;
                }
                ViewBag.User = (Usuarios)TempData.Peek("User");
                ViewBag.Especialidades = await repo.GetEspecialidades();
                return View();
            }
        }

        //GET: VerCitas
        [AutorizacionUsuarios("Médico")]
        public async Task<IActionResult> VerCitas(int userLogged)
        {
            string existe = await repo.ExisteTipoUsuario(userLogged);
            if (existe.Equals("SinPerfil")) return RedirectToAction("CrearPersonal", "Personal");

            string token = HttpContext.Session.GetString("token");
            Personal personal = await repo.FindPersonal(userLogged, token);
            if (personal == null)
            {
                return RedirectToAction("VerPerfil", "Manage", new { userLogged = userLogged });
            }
            ViewBag.fechaActual = DateTime.Now;
            ViewBag.PersonalId = personal.PersonalId;

            return View();
        }

        //GET: PartialView _GetCitasDia
        [AutorizacionUsuarios("Médico")]
        public async Task<IActionResult> _GetCitasDia(DateTime fecha, int selectedPersonal)
        {
            string token = HttpContext.Session.GetString("token");
            ViewBag.fecha = fecha;
            ViewBag.selectedPersonal = selectedPersonal;
            int paciente = 0;
            List<Cita> citasMedico = await repo.CheckCitaInDay(selectedPersonal, fecha, paciente, token);

            return PartialView(citasMedico);
        }
    }
}