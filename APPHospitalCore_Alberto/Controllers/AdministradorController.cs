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
    [AutorizacionUsuarios("Administrador")]
    public class AdministradorController : Controller
    {
        IRepositoryHospital repo;
        public AdministradorController(IRepositoryHospital repo)
        {
            this.repo = repo;
        }

        // GET: ListaUsuarios
        public async Task<IActionResult> ListaUsuarios(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.EmailSortParm = String.IsNullOrEmpty(sortOrder) ? "email_desc" : "";
            ViewBag.RoleSortParm = sortOrder == "Role" ? "role_desc" : "Role";
            ViewBag.ActivoSortParm = sortOrder == "Activo" ? "activo_desc" : "Activo";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentFilter = searchString;

            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            List<Usuarios> usuarios = await repo.GetUsers(token);

            if (!String.IsNullOrEmpty(searchString))
            {
                usuarios = usuarios.Where(u => u.Email.ToLower().Contains(searchString.ToLower())|| 
                u.NombreRole.ToLower().Contains(searchString.ToLower())).ToList();
            }
            switch (sortOrder)
            {
                case "email_desc":
                    usuarios = usuarios.OrderByDescending(u => u.Email).ToList();
                    break;
                case "Role":
                    usuarios = usuarios.OrderBy(u => u.NombreRole).ToList();
                    break;
                case "role_desc":
                    usuarios = usuarios.OrderByDescending(u => u.NombreRole).ToList();
                    break;
                case "Activo":
                    usuarios = usuarios.OrderBy(u => u.Activo).ToList();
                    break;
                case "activo_desc":
                    usuarios = usuarios.OrderByDescending(u => u.Activo).ToList();
                    break;
                default:
                    usuarios = usuarios.OrderBy(u => u.Email).ToList();
                    break;
            }
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            return View(usuarios.ToPagedList(pageNumber,pageSize));
        }

        //GET: CrearUsuario
        public ActionResult CrearUsuario()
        {
            return RedirectToAction("RegistrarUsuario", "Manage");
        }

        //GET: EditarUsuario
        public async Task<IActionResult> EditarUsuario(int userId)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            Usuarios user = await repo.FindUser(userId, token);
            return View(user);
        }

        //POST: EditarUsuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(Usuarios user)
        {
            string token = HttpContext.Session.GetString("token");
            if (token == null)
            {
                TempData["Mensaje"] = "Sesion expirada,vuelva a hacer el log in por favor.";
                return RedirectToAction("Login", "Manage");
            }
            int userId = user.UserId;
            string email = user.Email;
            string password = user.Password;
            Role role = user.Role;
            bool activo = user.Activo;
            await repo.EditarUsuario(userId, email, password, role, activo, token);

            return RedirectToAction("ListaUsuarios");
        }

        //GET: PerfilUsuario
        public ActionResult PerfilUsuario(int userId)
        {
            return RedirectToAction("VerPerfil", "Manage", new { userLogged = userId });
        }
    }
}