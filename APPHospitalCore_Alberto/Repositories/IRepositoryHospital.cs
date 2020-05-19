using APPHospitalCore_Alberto.ViewModels;
using HospitalNuget.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APPHospitalCore_Alberto.Repositories
{
    public interface IRepositoryHospital
    {
        #region Administrador
        Task<string> GetToken(string email, string password);
        Task<Usuarios> GetUserLogin(string token);
        Task<List<Usuarios>> GetUsers(string token);
        Task<List<CustomValidationResult>> Validate(Usuarios user);
        Task<Usuarios> ExisteUsuario(string email, string password, string token);
        Task<Usuarios> FindUser(int userId, string token);
        Task<string> ExisteTipoUsuario(int userLogged);
        Task<string> CrearPerfilUsuario(int userLogged);
        Task CrearUsuario(string email, string password, string confirmarPass, Role role);
        Task EditarUsuario(int userId, string email, string password, Role role, bool activo, string token);
        #endregion

        #region Pacientes
        Task<Paciente> FindPaciente(int userLogged, string token);
        Task<Paciente> FindPacienteById(int pacienteId, string token);
        Task CrearPaciente(string dni, string nombre, string apellidos, DateTime fecha_nac, Sexo sexo, string telefono, string ciudad,
            string direccion, int cp, string email, Int64 nss, int userId, string token);
        Task EditarPaciente(int pacienteId, string dni, string nombre, string apellidos, DateTime fecha_nac, Sexo sexo, string telefono, string ciudad,
            string direccion, int cp, string email, Int64 nss, string token);
        Task CheckCitasCaducadas(int userLogged, DateTime fecha);
        Task<List<Cita>> GetTodasCitasPaciente(int userLogged, string token);
        Task<List<Cita>> GetCitasPaciente(int userLogged, string token);
        Task<List<Cita>> CheckCitaInDay(int selectedPersonal, DateTime fecha, int? paciente, string token);
        Task<Cita> FindCita(int citaId, string token);
        Task CrearCita(int pacienteId, DateTime fecha, DateTime hora, int personalId, string token);
        Task AnularCita(int citaId, string token);
        Task CambiarCita(int citaId, DateTime fecha, DateTime hora, string token);
        Task<int> EdadPaciente(DateTime fechaNacimiento);

        #endregion

        #region Personal
        Task CrearPersonal(string dni, string nombre, string apellidos, DateTime fecha_nac, string telefono, string ciudad,
           string direccion, string email, int numColegiado, Turno turno, int especialidadId, int userId, string token);
        Task<Personal> FindPersonal(int userLogged, string token);
        Task<Personal> FindPersonalById(int persnonalId, string token);
        Task EditarPersonal(int personalId, string dni, string nombre, string apellidos, DateTime fecha_nac, string telefono, string ciudad,
            string direccion, string email, int numColegiado, Turno turno, int especialidadId, string token);
        Task<List<Personal>> GetPersonal();
        Task<List<Personal>> GetPersonalByTurno(Turno turno);
        Task<string> GetTurnoPersonal(int personalId);
        Task<List<Especialidad>> GetEspecialidades();
        Task<Especialidad> GetEspecialidadPersonal(int especialidadId);
        Task<List<string>> GetAllTurnos();
        Task<List<DateTime>> GetCitasConcertadas(int personalId, DateTime fecha);
        Task<List<DateTime>> GetCitasLibres(int personalId, Turno turno, DateTime fecha);
        #endregion

        #region Informes
        Task<List<Informe>> GetInformes(int pacienteId, string token);
        Task<Informe> GetDetallesInforme(int informeId, string token);
        Task CrearInforme(int pacienteId, int personalId, DateTime fecha, string descripcion, string diagnostico, string token);
        Task EditarInforme(int informeId, string descripcion, string diagnostico, string token);
        Task<int> UltimoInformeId();
        #endregion

    }
}
