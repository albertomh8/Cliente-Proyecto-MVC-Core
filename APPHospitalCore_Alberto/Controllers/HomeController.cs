using APPHospitalCore_Alberto.Models;
using APPHospitalCore_Alberto.Repositories;
using HospitalNuget.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace APPHospital_Alberto.Controllers
{
    public class HomeController : Controller
    {
        IRepositoryHospital repo;
        IRepositoryCovid19 repoCovid19;
        public HomeController(IRepositoryHospital repo, IRepositoryCovid19 repoCovid19)
        {
            this.repo = repo;
            this.repoCovid19 = repoCovid19;
        }
        public async Task<IActionResult> Index()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                HttpContext.Session.SetInt32("userLogged", int.Parse(userId));

                string tipoUser = await repo.ExisteTipoUsuario(int.Parse(userId));
                if (tipoUser.Equals("Paciente"))
                {
                    DateTime fecha = DateTime.Now;
                    await repo.CheckCitasCaducadas(int.Parse(userId), fecha);
                }
            }
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }
        public IActionResult News()
        {
            return View();
        }

        //GET: CalcularMasaCorporal
        public IActionResult CalcularMasaCorporal()
        {
            return View();
        }

        //GET: CalcularPesoIdeal
        public IActionResult CalcularPesoIdeal()
        {
            ViewBag.Sexo = Enum.GetNames(typeof(Sexo));
            return View();
        }

        //GET: DatosCOVID19
        public async Task<IActionResult> DatosCOVID19()
        {
            //Hago un bucle por que la api externa en algunos request no trae los datos.
            //De este modo me aseguro de que llegen los datos.
            List<Country> countries = new List<Country>();
            do
            {
                countries = await repoCovid19.GetCountriesAsync();
            } while (countries == null);
            ViewData["countries"] = countries;
            ViewData["slug"] = "";
            ViewData["startDate"] = DateTime.Now;
            ViewData["endDate"] = DateTime.Now;
            return View();
        }

        //POST: PartialView DatosCOVID19
        [HttpPost]
        public async Task<IActionResult> DatosCOVID19(string slug, DateTime startDate, DateTime endDate)
        {
            //Hago un bucle por que la api externa en algunos request no trae los datos.
            //De este modo me aseguro de que llegen los datos.
            List<Country> countries = new List<Country>();
            do
            {
                countries = await repoCovid19.GetCountriesAsync();
            } while (countries == null);
            ViewData["countries"] = countries;

            List<Country> data = await repoCovid19.GetByCountryAllStatusAsync(slug, startDate, endDate);
            ViewData["slug"] = slug;
            ViewData["startDate"] = startDate;
            ViewData["endDate"] = endDate;
            return View(data);
        }
    }
}