using APPHospitalCore_Alberto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APPHospitalCore_Alberto.Repositories
{
    public interface IRepositoryCovid19
    {
        Task<List<Country>> GetCountriesAsync();
        Task<List<Country>> GetByCountryAllStatusAsync(string slug, DateTime startDate, DateTime endDate);
    }
}
