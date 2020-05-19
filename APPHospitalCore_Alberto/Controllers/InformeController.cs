using APPHospitalCore_Alberto.Filters;
using APPHospitalCore_Alberto.Repositories;
using HospitalNuget.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APPHospital_Alberto.Controllers
{
    [AutorizacionUsuarios("Médico", "Paciente")]
    public class InformeController : Controller
    {
        IRepositoryHospital repo;
        public InformeController(IRepositoryHospital repo)
        {
            this.repo = repo;
        }

        #region Informes
        // GET: Informes
        public async Task<IActionResult> Informes(int? pacienteId, string sortOrder, string currentFilter, string searchString, int? page)
        {
            //Mostrar todos los informes del usuario que esta logeado o el paciente seleccionado.
            if (HttpContext.Session.GetInt32("userLogged") == null)
            {
                return RedirectToAction("Login", "Manage");
            }
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            int userLogged = int.Parse(HttpContext.Session.GetInt32("userLogged").ToString());
            Personal personal = await repo.FindPersonal(userLogged, token);
            if (personal != null)
            {
                ViewBag.MedicoId = personal.PersonalId;
            }
            List<Informe> informes;
            if (pacienteId == null)
            {
                Paciente paciente = await repo.FindPaciente(userLogged, token);
                if (paciente == null)
                {
                    return RedirectToAction("VerPerfil", "Manage", new { userLogged = userLogged });
                }
                informes = await repo.GetInformes(paciente.PacienteId, token);
            }
            else
            {
                informes = await repo.GetInformes(pacienteId.GetValueOrDefault(), token);
                ViewBag.PacienteId = pacienteId.GetValueOrDefault();
            }

            ViewBag.FechaSortParm = String.IsNullOrEmpty(sortOrder) ? "fecha_desc" : "";
            ViewBag.DescripcionSortParm = sortOrder == "Descripcion" ? "descripcion_desc" : "Descripcion";
            ViewBag.MedicoSortParm = sortOrder == "Medico" ? "medico_desc" : "Medico";

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
                informes = informes.Where(i => i.Descripcion.ToLower().Contains(searchString.ToLower()) || 
                i.Personal.Nombre.ToLower().Contains(searchString.ToLower()) ||
                i.Personal.Apellidos.ToLower().Contains(searchString.ToLower())).ToList();
            }

            switch (sortOrder)
            {
                case "fecha_desc":
                    informes = informes.OrderByDescending(i => i.Fecha).ToList();
                    break;
                case "Descripcion":
                    informes = informes.OrderBy(i => i.Descripcion).ToList();
                    break;
                case "descripcion_desc":
                    informes = informes.OrderByDescending(i => i.Descripcion).ToList();
                    break;
                case "Medico":
                    informes = informes.OrderBy(i => i.Personal.Nombre).ToList();
                    break;
                case "medico_desc":
                    informes = informes.OrderByDescending(i => i.Personal.Nombre).ToList();
                    break;
                default:
                    informes = informes.OrderBy(i => i.Fecha).ToList();
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);
            return View(informes.ToPagedList(pageNumber, pageSize));
        }

        //GET: DetallesInforme
        public async Task<IActionResult> DetallesInforme(int informeId)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            //Mostrar los detalles para el informe seleccionado.
            Informe informe = await repo.GetDetallesInforme(informeId, token);
            int edad = await repo.EdadPaciente(informe.Paciente.Fecha_Nacimiento);
            ViewBag.Edad = edad;

            return View(informe);
        }

        //GET: CrearInforme
        [AutorizacionUsuarios("Médico")]
        public async Task<IActionResult> CrearInforme(int pacienteId)
        {
            if (HttpContext.Session.GetInt32("userLogged") == null)
            {
                return RedirectToAction("Login", "Manage");
            }
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            int userLogged = int.Parse(HttpContext.Session.GetInt32("userLogged").ToString());
            Paciente paciente = await repo.FindPacienteById(pacienteId, token);
            int edad = await repo.EdadPaciente(paciente.Fecha_Nacimiento);
            ViewBag.Edad = edad;
            ViewBag.Paciente = paciente;
            ViewBag.Personal = await repo.FindPersonal(userLogged, token);

            return View();
        }

        //POST: CrearInforme
        [AutorizacionUsuarios("Médico")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearInforme(Informe informe, int pacienteId, int personalId)
        {
            if (ModelState.IsValid)
            {
                string token = HttpContext.Session.GetString("token");
                if (token == null)
                {
                    TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                    return RedirectToAction("Login", "Manage");
                }
                await repo.CrearInforme(pacienteId, personalId, informe.Fecha, informe.Descripcion, informe.Diagnostico, token);
                int informeId = await repo.UltimoInformeId();

                return RedirectToAction("DetallesInforme", new { informeId = informeId });
            }
            else
            {
                return View("ErrorInforme");
            }

        }

        //GET: EditarInforme
        public async Task<IActionResult> EditarInforme(int informeId)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            Informe informe = await repo.GetDetallesInforme(informeId, token);
            return View(informe);
        }

        //POST: EditarInforme
        [AutorizacionUsuarios("Médico")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarInforme(Informe informe)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            await repo.EditarInforme(informe.InformeId, informe.Descripcion, informe.Diagnostico, token);
            return RedirectToAction("DetallesInforme", new { informeId = informe.InformeId });
        }

        //GET: ErrorInforme
        public ActionResult ErrorInforme()
        {
            return View();
        }
        #endregion
    }
}