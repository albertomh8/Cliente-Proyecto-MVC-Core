using HospitalNuget.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APPHospitalCore_Alberto.ViewModels
{
    public class CitasViewModel
    {
        public List<Personal> Personal { get; set; }
        [Display(Name = "Médico")]
        [Required(ErrorMessage = "{0} is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un médico")]
        public int SelectedPersonal { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un turno valido")]
        public Turno Turno { get; set; }
        public int SelectedTurno { get; set; }
    }
}
