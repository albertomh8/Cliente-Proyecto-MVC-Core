using APPHospitalCore_Alberto.ViewModels;
using HospitalNuget.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace APPHospitalCore_Alberto.Repositories
{
    public class RepositoryHospitalSQL : IRepositoryHospital
    {
        string url;
        MediaTypeWithQualityHeaderValue header;
        public RepositoryHospitalSQL()
        {
            this.url = "https://apihospitalalberto.azurewebsites.net";
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
        }

        private async Task<T> CallAPI<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T datos = await response.Content.ReadAsAsync<T>();
                    return (T)Convert.ChangeType(datos, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
        }

        private async Task<T> CallAPI<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T datos = await response.Content.ReadAsAsync<T>();
                    return (T)Convert.ChangeType(datos, typeof(T));
                }
                else
                {
                    return default(T);
                }
            }
        }

        private async Task CallAPICreate<T>(string request, T postObject)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                string json = JsonConvert.SerializeObject(postObject);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }

        private async Task CallAPICreate<T>(string request, T postObject, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                string json = JsonConvert.SerializeObject(postObject);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
            }
        }

        private async Task CallAPIUpdate<T>(string request, T putObject)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                string json = JsonConvert.SerializeObject(putObject);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PutAsync(request, content);
            }
        }

        private async Task CallAPIUpdate<T>(string request, T putObject, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                string json = JsonConvert.SerializeObject(putObject);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PutAsync(request, content);
            }
        }

        private async Task CallAPIDelete(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                await client.DeleteAsync(request);
            }
        }

        private async Task CallAPIDelete(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                await client.DeleteAsync(request);
            }
        }

        #region Citas
        public async Task<List<Cita>> GetTodasCitasPaciente(int userLogged, string token)
        {
            string request = "/api/Citas/HistorialCitasPaciente/" + userLogged;
            List<Cita> citas = await CallAPI<List<Cita>>(request, token);
            return citas;
        }

        public async Task<List<Cita>> GetCitasPaciente(int userLogged, string token)
        {
            string request = "/api/Citas/CitasPaciente/" + userLogged;
            List<Cita> citas = await CallAPI<List<Cita>>(request, token);
            return citas;
        }

        public async Task CrearCita(int pacienteId, DateTime fecha, DateTime hora, int personalId, string token)
        {
            string request = "/api/Citas/CrearCita";
            Cita cita = new Cita();
            cita.PacienteId = pacienteId;
            cita.PersonalId = personalId;
            cita.Fecha = fecha;
            cita.Hora = hora;
            cita.Caducada = false;
            await CallAPICreate(request, cita, token);
        }

        public async Task AnularCita(int citaId, string token)
        {
            string request = "/api/Citas/AnularCita/" + citaId;
            await CallAPIDelete(request, token);
        }

        public async Task CambiarCita(int citaId, DateTime fecha, DateTime hora, string token)
        {
            string request = "/api/Citas/CambiarCita";
            Cita cita = await FindCita(citaId, token);
            cita.Fecha = fecha;
            cita.Hora = hora;
            cita.Caducada = false;
            await CallAPIUpdate(request, cita, token);
        }

        public async Task CheckCitasCaducadas(int userLogged, DateTime fecha)
        {
            string request = "/api/Citas/CitasCaducadas/" + userLogged + "/" + fecha.ToString("yyyy-MM-dd");
            await CallAPI<string>(request);
        }

        public async Task<List<Cita>> CheckCitaInDay(int selectedPersonal, DateTime fecha, int? paciente, string token)
        {
            string request = "/api/Citas/CitasDia/" + selectedPersonal + "/" + fecha.ToString("yyyy-MM-dd") + "/" + paciente;
            List<Cita> citas = await CallAPI<List<Cita>>(request, token);
            return citas;
        }

        public async Task<Cita> FindCita(int citaId, string token)
        {
            string request = "/api/Citas/" + citaId;
            Cita citas = await CallAPI<Cita>(request, token);
            return citas;
        }

        #endregion

        #region Paciente

        public async Task<Paciente> FindPaciente(int userLogged, string token)
        {
            string request = "/api/Pacientes/FindPaciente/" + userLogged;
            Paciente paciente = await CallAPI<Paciente>(request, token);
            return paciente;
        }
        public async Task<Paciente> FindPacienteById(int pacienteId, string token)
        {
            string request = "/api/Pacientes/FindPacienteById/" + pacienteId;
            Paciente paciente = await CallAPI<Paciente>(request, token);
            return paciente;
        }

        public async Task CrearPaciente(string dni, string nombre, string apellidos, DateTime fecha_nac, Sexo sexo, string telefono, string ciudad,
            string direccion, int cp, string email, Int64 nss, int userId, string token)
        {
            string request = "/api/Pacientes";
            Paciente p = new Paciente();
            p.DNI = dni;
            p.Nombre = nombre;
            p.Apellidos = apellidos;
            p.Fecha_Nacimiento = fecha_nac;
            p.Sexo = sexo;
            p.Telefono = telefono;
            p.Ciudad = ciudad;
            p.Direccion = direccion;
            p.CP = cp;
            p.Email = email;
            p.NSS = nss;
            p.UserId = userId;

            await CallAPICreate(request, p, token);
        }

        public async Task EditarPaciente(int pacienteId, string dni, string nombre, string apellidos, DateTime fecha_nac, Sexo sexo, string telefono, string ciudad,
             string direccion, int cp, string email, Int64 nss, string token)
        {
            string request = "/api/Pacientes";
            Paciente p = await FindPacienteById(pacienteId, token);
            p.DNI = dni;
            p.Nombre = nombre;
            p.Apellidos = apellidos;
            p.Fecha_Nacimiento = fecha_nac;
            p.Sexo = sexo;
            p.Telefono = telefono;
            p.Ciudad = ciudad;
            p.Direccion = direccion;
            p.CP = cp;
            p.Email = email;
            p.NSS = nss;

            await CallAPIUpdate(request, p, token);
        }

        public async Task<int> EdadPaciente(DateTime fechaNacimiento)
        {
            string request = "/api/Pacientes/EdadPaciente/" + fechaNacimiento.ToString("dd-MM-yyyy");
            int edad = await CallAPI<int>(request);
            return edad;
        }

        #endregion

        #region Personal
        public async Task CrearPersonal(string dni, string nombre, string apellidos, DateTime fecha_nac, string telefono, string ciudad,
            string direccion, string email, int numColegiado, Turno turno, int especialidadId, int userId, string token)
        {
            string request = "/api/Personal";
            Personal p = new Personal();
            p.DNI = dni;
            p.Nombre = nombre;
            p.Apellidos = apellidos;
            p.Fecha_Nacimiento = fecha_nac;
            p.Telefono = telefono;
            p.Ciudad = ciudad;
            p.Direccion = direccion;
            p.Email = email;
            p.NumColegiado = numColegiado;
            p.Turno = turno;
            p.EspecialidadId = especialidadId;
            p.UserId = userId;

            await CallAPICreate(request, p, token);
        }

        public async Task<Personal> FindPersonal(int userLogged, string token)
        {
            string request = "/api/Personal/FindPersonal/" + userLogged;
            Personal personal = await CallAPI<Personal>(request, token);
            return personal;
        }

        public async Task<Personal> FindPersonalById(int personalId, string token)
        {
            string request = "/api/Personal/FindPersonalById/" + personalId;
            Personal personal = await CallAPI<Personal>(request, token);
            return personal;
        }

        public async Task EditarPersonal(int personalId, string dni, string nombre, string apellidos, DateTime fecha_nac, string telefono, string ciudad,
            string direccion, string email, int numColegiado, Turno turno, int especialidadId, string token)
        {
            string request = "/api/Personal";
            Personal p = await FindPersonalById(personalId, token);
            p.DNI = dni;
            p.Nombre = nombre;
            p.Apellidos = apellidos;
            p.Fecha_Nacimiento = fecha_nac;
            p.Telefono = telefono;
            p.Ciudad = ciudad;
            p.Direccion = direccion;
            p.Email = email;
            p.NumColegiado = numColegiado;
            p.Turno = turno;
            p.EspecialidadId = especialidadId;

            await CallAPIUpdate(request, p, token);
        }

        public async Task<List<Especialidad>> GetEspecialidades()
        {
            string request = "/api/Especialidades";
            List<Especialidad> especialidades = await CallAPI<List<Especialidad>>(request);
            return especialidades;
        }

        public async Task<Especialidad> GetEspecialidadPersonal(int especialidadId)
        {
            string request = "/api/Especialidades/" + especialidadId;
            Especialidad especialidad = await CallAPI<Especialidad>(request);
            return especialidad;
        }

        public async Task<List<string>> GetAllTurnos()
        {
            string request = "/api/Personal/Turnos";
            List<string> turnos = await CallAPI<List<string>>(request);
            return turnos;
        }

        public async Task<List<DateTime>> GetCitasConcertadas(int personalId, DateTime fecha)
        {
            string request = "/api/Personal/CitasConcertadas/" + personalId + "/" + fecha.ToString("yyyy-MM-dd");
            List<DateTime> citas = await CallAPI<List<DateTime>>(request);
            return citas;
        }

        public async Task<List<DateTime>> GetCitasLibres(int personalId, Turno turno, DateTime fecha)
        {
            //Saca las citas disponibles para el médico seleccionado
            string request = "/api/Personal/CitasLibres/" + personalId + "/" + turno + "/" + fecha.ToString("yyyy-MM-dd");
            List<DateTime> horas = await CallAPI<List<DateTime>>(request);
            return horas;
        }

        public async Task<List<Personal>> GetPersonal()
        {
            string request = "/api/Personal";
            List<Personal> personal = await CallAPI<List<Personal>>(request);
            return personal;
        }

        public async Task<List<Personal>> GetPersonalByTurno(Turno turno)
        {
            string request = "/api/Personal/PersonalTurno/" + turno;
            List<Personal> personal = await CallAPI<List<Personal>>(request);
            return personal;
        }

        public async Task<string> GetTurnoPersonal(int personalId)
        {
            string request = "/api/Personal/TurnoPersonal/" + personalId;
            string turno = await CallAPI<string>(request);
            return turno;
        }

        #endregion

        #region Informes

        public async Task<Informe> GetDetallesInforme(int informeId, string token)
        {
            string request = "/api/Informes/DetallesInforme/" + informeId;
            Informe informe = await CallAPI<Informe>(request, token);
            return informe;
        }
        public async Task<List<Informe>> GetInformes(int pacienteId, string token)
        {
            string request = "/api/Informes/" + pacienteId;
            List<Informe> informes = await CallAPI<List<Informe>>(request, token);
            return informes;
        }
        public async Task CrearInforme(int pacienteId, int personalId, DateTime fecha, string descripcion, string diagnostico, string token)
        {
            string request = "/api/Informes";
            Informe informe = new Informe();
            informe.PacienteId = pacienteId;
            informe.PersonalId = personalId;
            informe.Fecha = fecha;
            informe.Descripcion = descripcion;
            informe.Diagnostico = diagnostico;

            await CallAPICreate(request, informe, token);
        }

        public async Task EditarInforme(int informeId, string descripcion, string diagnostico, string token)
        {
            string request = "/api/Informes";
            Informe informe = await GetDetallesInforme(informeId, token);
            informe.Descripcion = descripcion;
            informe.Diagnostico = diagnostico;

            await CallAPIUpdate(request, informe, token);
        }

        public async Task<int> UltimoInformeId()
        {
            string request = "/api/Informes/UltimoInforme";
            int ultimoInformeId = await CallAPI<int>(request);
            return ultimoInformeId;
        }

        #endregion

        #region Administrador
        public async Task<string> GetToken(string user, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                string request = "/api/Manage/Login";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(header);
                Usuarios login = new Usuarios();
                login.Email = user;
                login.Password = password;
                //Pasamos el comparePassword por necesidad de los DataAnnotations
                login.ComparePassword = password;
                string json = JsonConvert.SerializeObject(login);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var jsonObj = JObject.Parse(data);
                    string token = jsonObj.GetValue("response").ToString();
                    return token;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<Usuarios> GetUserLogin(string token)
        {
            string request = "/api/Manage/UserLogin";
            Usuarios userLogged = await CallAPI<Usuarios>(request, token);
            return userLogged;
        }

        public async Task<List<CustomValidationResult>> Validate(Usuarios user)
        {
            string request = "/api/Administradores/Validate/" + user.Email;
            List<CustomValidationResult> validationResult = await CallAPI<List<CustomValidationResult>>(request);
            return validationResult;
        }

        public async Task<Usuarios> ExisteUsuario(string email, string password,string token)
        {
            //Comprobamos si existe el usuario, si existe comprobamos
            //si la contraseña es correcta y de ser así devolvemos el usuario
            string request = "/api/Administradores/ExisteUsuario/" + email + "/" + password;
            Usuarios user = await CallAPI<Usuarios>(request, token);
            return user;
        }

        public async Task<string> ExisteTipoUsuario(int userLogged)
        {
            //Buscar el usuario logeado y devuelve un string con el rol del usuario
            string request = "/api/Administradores/ExisteTipoUsuario/" + userLogged;
            string result = await CallAPI<string>(request);
            return result;
        }

        public async Task<string> CrearPerfilUsuario(int userLogged)
        {
            string request = "/api/Administradores/CrearPerfilUsuario/" + userLogged;
            string result = await CallAPI<string>(request);
            return result;
        }

        public async Task<Usuarios> FindUser(int userLogged, string token)
        {
            string request = "/api/Administradores/Usuarios/" + userLogged;
            Usuarios user = await CallAPI<Usuarios>(request, token);
            return user;
        }

        public async Task<List<Usuarios>> GetUsers(string token)
        {
            string request = "/api/Administradores/Usuarios";
            List<Usuarios> users = await CallAPI<List<Usuarios>>(request, token);
            return users;
        }

        public async Task CrearUsuario(string email, string password, string confirmarPass, Role role)
        {
            string request = "/api/Administradores/CrearUsuario";
            Usuarios user = new Usuarios();
            user.Email = email;
            user.Password = password;
            user.ComparePassword = confirmarPass;
            user.Role = role;
            user.Activo = true;

            await CallAPICreate(request, user);
        }

        public async Task EditarUsuario(int userId, string email, string password, Role role, bool activo, string token)
        {
            string request = "/api/Administradores/EditarUsuario";
            //La password de cofirmacion es la password porque no esta mapeada en la bbdd
            Usuarios user = await FindUser(userId, token);
            user.Email = email;
            user.Password = password;
            user.ComparePassword = password;
            user.Role = role;
            user.Activo = activo;

            await CallAPIUpdate(request, user, token);
        }
        #endregion
    }
}
