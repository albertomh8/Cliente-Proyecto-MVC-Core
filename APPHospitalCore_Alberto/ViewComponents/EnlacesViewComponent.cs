using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace APPHospitalCore_Alberto.ViewComponents
{
    [ViewComponent(Name = "Enlaces")]
    public class EnlacesViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
