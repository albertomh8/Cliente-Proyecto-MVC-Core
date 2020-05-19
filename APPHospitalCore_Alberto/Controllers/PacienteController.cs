using APPHospitalCore_Alberto.Filters;
using APPHospitalCore_Alberto.Repositories;
using APPHospitalCore_Alberto.ViewModels;
using HospitalNuget.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace APPHospital_Alberto.Controllers
{
    public class PacienteController : Controller
    {
        IRepositoryHospital repo;
        public PacienteController(IRepositoryHospital repo)
        {
            this.repo = repo;
        }

        #region Paciente
        [AutorizacionUsuarios("Paciente")]
        // GET: Paciente
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("userLogged") == null)
            {
                return RedirectToAction("Login", "Manage");
            }
            int userLogged = int.Parse(HttpContext.Session.GetInt32("userLogged").ToString());
            string tipoUser = await repo.ExisteTipoUsuario(userLogged);
            if (tipoUser.Equals("Personal"))
            {
                return RedirectToAction("VerCitas","Personal",new { userLogged = userLogged });
            }
            DateTime fecha = DateTime.Now;
            await repo.CheckCitasCaducadas(userLogged, fecha);

            return View();
        }

        //GET: CrearPaciente
        [AutorizacionUsuarios("Administrador", "Paciente")]
        public ActionResult CrearPaciente()
        {
            Usuarios user = JsonConvert.DeserializeObject<Usuarios>(TempData["User"].ToString());
            ViewBag.User = user;
            TempData["User"] = JsonConvert.SerializeObject(user);

            return View();
        }

        //POST: CrearPaciente
        [AutorizacionUsuarios("Administrador", "Paciente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPaciente(Paciente p)
        {
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString("token");
                if (token == null)
                {
                    TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                    return RedirectToAction("Login", "Manage");
                }
                await repo.CrearPaciente(p.DNI, p.Nombre, p.Apellidos, p.Fecha_Nacimiento, p.Sexo, p.Telefono, p.Ciudad, p.Direccion, p.CP, p.Email, p.NSS, p.UserId, token);

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
                ViewBag.User = JsonConvert.DeserializeObject<Usuarios>(TempData.Peek("User").ToString());

                return View();
            }
        }
        [AutorizacionUsuarios("Administrador", "Paciente", "Médico")]

        //GET: DetallesPaciente
        public async Task<IActionResult> DetallesPaciente(int pacienteId)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            Paciente paciente = await repo.FindPacienteById(pacienteId, token);
            ViewBag.Edad = await repo.EdadPaciente(paciente.Fecha_Nacimiento);
            return View(paciente);
        }
        #endregion

        #region Citas
        [AutorizacionUsuarios("Paciente")]
        //GET: Citas
        public async Task<IActionResult> Citas(string sortOrder, string currentFilter, string searchString, int? page)
        {
            //Mostrar todas las citas pendientes del usuario logeado.
            if (HttpContext.Session.GetInt32("userLogged") == null)
            {
                return RedirectToAction("Login", "Manage");
            }
            int userLogged = int.Parse(HttpContext.Session.GetInt32("userLogged").ToString());
            string existe = await repo.ExisteTipoUsuario(userLogged);
            if (existe.Equals("SinPerfil")) return RedirectToAction("CrearPaciente", "Paciente");

            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }

            List<Cita> citasPaciente = await repo.GetCitasPaciente(userLogged, token);

            ViewBag.FechaSortParm = String.IsNullOrEmpty(sortOrder) ? "fecha_desc" : "";
            ViewBag.HoraSortParm = sortOrder == "Hora" ? "hora_desc" : "Hora";
            ViewBag.MedicoSortParm = sortOrder == "Medico" ? "medico_desc" : "Medico";
            ViewBag.EspecialidadSortParm = sortOrder == "Especialidad" ? "especialidad_desc" : "Especialidad";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                citasPaciente = citasPaciente.Where(c => c.Fecha.ToString("dd/MM/yyyy").Contains(searchString.ToLower()) || 
                c.Hora.ToString("hh:mm").Contains(searchString.ToLower()) ||
                c.Personal.Nombre.ToLower().Contains(searchString.ToLower()) ||
                c.Personal.Apellidos.ToLower().Contains(searchString.ToLower()) ||
                c.Personal.Especialidad.Nombre.ToLower().Contains(searchString.ToLower())).ToList();
            }
            switch (sortOrder)
            {
                case "fecha_desc":
                    citasPaciente = citasPaciente.OrderByDescending(c => c.Fecha).ToList();
                    break;
                case "Hora":
                    citasPaciente = citasPaciente.OrderBy(c => c.Hora).ToList();
                    break;
                case "hora_desc":
                    citasPaciente = citasPaciente.OrderByDescending(c => c.Hora).ToList();
                    break;
                case "Medico":
                    citasPaciente = citasPaciente.OrderBy(c => c.Personal.Nombre).ToList();
                    break;
                case "medico_desc":
                    citasPaciente = citasPaciente.OrderByDescending(c => c.Personal.Nombre).ToList();
                    break;
                case "Especialidad":
                    citasPaciente = citasPaciente.OrderBy(c => c.Personal.MedicoEspecialidad).ToList();
                    break;
                case "especialidad_desc":
                    citasPaciente = citasPaciente.OrderByDescending(c => c.Personal.MedicoEspecialidad).ToList();
                    break;
                default:
                    citasPaciente = citasPaciente.OrderBy(c => c.Fecha).ToList();
                    break;
            }

            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(citasPaciente.ToPagedList(pageNumber, pageSize));
        }

        //GET: SeleccionarCita
        [AutorizacionUsuarios("Paciente")]
        public async Task<IActionResult> SeleccionarCita()
        {
            List<Personal> personal = await repo.GetPersonal();
            CitasViewModel vm = new CitasViewModel();
            vm.Personal = personal;

            ViewBag.personal = vm;
            return View();
        }
        //POST: SeleccionarCita
        [AutorizacionUsuarios("Paciente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SeleccionarCita(Cita cita, Turno turno, int? selectedPersonal)
        {
            //Pasar a la vista de crear una nueva cita los datos de fecha,turno y personalId
            if (selectedPersonal == null || selectedPersonal == 0)
            {
                return RedirectToAction("SeleccionarCita");
            }
            if (HttpContext.Session.GetInt32("userLogged") == null)
            {
                return RedirectToAction("Login", "Manage");
            }

            if (ModelState.IsValid)
            {
                DateTime fecha = cita.Fecha;
                int personalId = selectedPersonal.GetValueOrDefault();
                TempData["fecha"] = fecha;
                TempData["personalId"] = personalId;
                TempData["turno"] = turno;
            }
            return RedirectToAction("CrearCita");
        }

        //GET: CrearCita
        [AutorizacionUsuarios("Paciente")]
        public async Task<IActionResult> CrearCita()
        {
            string token = HttpContext.Session.GetString("token");
            int userLogged = int.Parse(HttpContext.Session.GetInt32("userLogged").ToString());
            Paciente paciente = await repo.FindPaciente(userLogged, token);
            int pacienteId = paciente.PacienteId;
            TempData.Keep("fecha");
            TempData.Keep("personalId");
            TempData.Keep("turno");
            ViewBag.Fecha = (DateTime)TempData["fecha"];
            ViewBag.PersonalId = (int)TempData["personalId"];
            ViewBag.PacienteId = pacienteId;
            ViewBag.Turno = (Turno)TempData["turno"];

            return View();
        }

        //POST: CrearCita
        [AutorizacionUsuarios("Paciente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCita(Cita cita, int personalId, DateTime hora)
        {
            //Crear una nueva cita para el paciente
            if (HttpContext.Session.GetInt32("userLogged") == null)
            {
                return RedirectToAction("Login", "Manage");
            }

            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString("token");
                int userLogged = int.Parse(HttpContext.Session.GetInt32("userLogged").ToString());
                Paciente paciente = await repo.FindPaciente(userLogged, token);
                int pacienteId = paciente.PacienteId;
                DateTime fecha = cita.Fecha;
                await repo.CrearCita(pacienteId, fecha, hora, personalId, token);
            }
            return RedirectToAction("Citas");
        }

        //GET: PartialView _GetMédicosDisponibles
        [AutorizacionUsuarios("Paciente")]
        public async Task<IActionResult> _GetMédicosDisponibles(Turno turno)
        {
            List<Personal> personal = new List<Personal>();
            personal = await repo.GetPersonalByTurno(turno);
            foreach (Personal p in personal)
            {
                p.Especialidad = await repo.GetEspecialidadPersonal(p.EspecialidadId);
                p.MedicoEspecialidad = p.Especialidad.Nombre + " - " + p.Nombre + " " + p.Apellidos;
            }
            CitasViewModel vm = new CitasViewModel();
            vm.Personal = personal;

            return PartialView(vm);
        }

        //GET: PartialView _GetHorasDisponibles
        [AutorizacionUsuarios("Paciente")]
        public async Task<IActionResult> _GetHorasDisponibles(int selectedPersonal, Turno turno, DateTime fecha)
        {
            List<DateTime> horas = await repo.GetCitasLibres(selectedPersonal, turno, fecha);
            return PartialView(horas);
        }

        //POST: PartialView _CheckCitaInDay
        [AutorizacionUsuarios("Paciente")]
        [HttpPost]
        public async Task<IActionResult> _CheckCitaInDay(int selectedPersonal, DateTime fecha, int paciente)
        {
            string token = HttpContext.Session.GetString("token");
            List<Cita> citasMédico = await repo.CheckCitaInDay(selectedPersonal, fecha, paciente, token);
            if (citasMédico.Count > 0)
            {
                return Json(new { success = true, message = "Lo sentimos, no puede concertar más de 1 cita al día con el mismo médico" });
            }
            else
            {
                return Json(new { success = false, message = "" });
            }
        }

        //GET: AnularCita
        [AutorizacionUsuarios("Paciente")]
        public async Task<IActionResult> AnularCita(int citaId)
        {
            string token = HttpContext.Session.GetString("token");
            //Eliminar citas pendientes del usuario.
            await repo.AnularCita(citaId, token);
            return RedirectToAction("Citas");
        }

        //GET: CambiarCita
        [AutorizacionUsuarios("Paciente")]
        public async Task<IActionResult> CambiarCita(int citaId)
        {
            string token = HttpContext.Session.GetString("token");
            //Muestra los datos de la cita seleccionada para cambiarlos.
            Cita cita = await repo.FindCita(citaId, token);
            ViewBag.Turno = await repo.GetTurnoPersonal(cita.PersonalId);

            return View(cita);
        }

        //POST: CambiarCita
        [AutorizacionUsuarios("Paciente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarCita(Cita cita)
        {
            //Cambiar la fecha/hora de la cita seleccionada.
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString("token");
                int citaId = cita.CitaId;
                DateTime fecha = cita.Fecha;
                DateTime hora = cita.Hora;

                await repo.CambiarCita(citaId, fecha, hora, token);
            }
            return RedirectToAction("Citas");
        }
        #endregion
    }
}