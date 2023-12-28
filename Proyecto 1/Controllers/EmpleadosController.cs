using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Proyecto_1.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Proyecto_1.Controllers
{
    public class EmpleadosController : Controller
    {
        // Caché
        private readonly IMemoryCache ElCache;

        public EmpleadosController(IMemoryCache elCache)
        {
            ElCache = elCache;
        }

        [NonAction]
        private List<Models.Empleado> ObtenerEmpleados()
        {
            List<Models.Empleado> resultado;

            if (EstaVacioElCache())
            {
                resultado = new List<Models.Empleado>();
                ElCache.Set("ListaDeEmpleados", resultado);
            }
            else
                resultado = (List<Models.Empleado>)ElCache.Get("ListaDeEmpleados");

            return resultado;
        }

        [NonAction]
        private Models.Empleado ObtenerElEmpleado(int id)
        {
            List<Models.Empleado> laLista;
            laLista = ObtenerEmpleados();

            foreach (Models.Empleado empleado in laLista)
            {
                if (empleado.Id == id)
                    return empleado;
            }

            return null;
        }

        [NonAction]
        private bool EstaVacioElCache()
        {
            if (ElCache.Get("ListaDeEmpleados") is null)
                return true;
            else return false;
        }

        // GET: EmpleadosController
        [HttpGet]
        public ActionResult Index()
        {
            return View(ObtenerEmpleados());
        }

        // GET: EmpleadosController/Create
        [HttpGet]
        public ActionResult Create()
        {
            List<Models.Empleado> empleados = ObtenerEmpleados();
            int nextId = 1;

            // Si hay empleados en la lista, obtener el valor máximo de "Id" y agregar 1
            if (empleados.Count > 0)
            {
                nextId = empleados.Max(t => t.Id) + 1;
            }

            // Crear una nueva instancia de empleado con el próximo valor de "Id"
            Models.Empleado nuevoEmpleado = new Models.Empleado
            {
                Id = nextId,
                FechaIngreso = DateTime.Now,
                FechaNacimiento = DateTime.Now
            };

            return View(nuevoEmpleado);
        }

        // POST: EmpleadosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, Models.Empleado empleado)
        {
            List<Models.Empleado> resultado = ObtenerEmpleados();
            var empleadoAlmacenado = ObtenerElEmpleado(id);

            if (empleadoAlmacenado != null)
            {
                ModelState.AddModelError("Id", "El número de empleado ingresado ya existe.");
            }

            if (empleado.FechaIngreso <= empleado.FechaNacimiento)
            {
                ModelState.AddModelError("FechaIngreso", "La fecha de ingreso debe ser mayor que la fecha de nacimiento.");
            }

            if (!Regex.IsMatch(empleado.Telefono.ToString(), "^[2678]"))
            {
                ModelState.AddModelError("Telefono", "El número de teléfono debe comenzar con 2, 6, 7 u 8 (solo números).");
            }

            if (ModelState.IsValid)
            {
                // Generar automáticamente el valor de "Id"
                int nextId = 1;

                if (resultado.Count > 0)
                {
                    // Si hay empleados en la lista, obtener el valor del último "Id" y agregar 1
                    nextId = resultado.Max(t => t.Id) + 1;
                }

                empleado.Id = nextId;

                resultado.Add(empleado);
                ElCache.Set("ListaDeEmpleados", resultado);

                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }       
            return View();
        }

        // GET: EmpleadosController/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            var empleado = ObtenerElEmpleado(id);

            if (id != 0 && empleado != null)
            {
                return View(empleado);
            }
            else return RedirectToAction(nameof(Index));
        }

        // GET: EmpleadosController/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var empleado = ObtenerElEmpleado(id);
            return View(empleado);
        }

        // POST: EmpleadosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Models.Empleado empleado)
        {
            if (empleado.FechaIngreso <= empleado.FechaNacimiento)
            {
                ModelState.AddModelError("FechaIngreso", "La fecha de ingreso debe ser mayor que la fecha de nacimiento.");
            }

            if (!Regex.IsMatch(empleado.Telefono.ToString(), "^[2678]"))
            {
                ModelState.AddModelError("Telefono", "El número de teléfono debe comenzar con 2, 6, 7 u 8 (solo números).");
            }

            if (ModelState.IsValid)
            {
                List<Models.Empleado> resultado = ObtenerEmpleados();

                for (int i = 0; i < resultado.Count; i++)
                {
                    if (resultado[i].Id == id)
                    {
                        resultado.RemoveAt(i);
                        resultado.Add(empleado);
                    }
                }

                ElCache.Set("ListaDeEmpleados", resultado);

                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }
            return View();
        }

        // GET: EmpleadosController/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var empleado = ObtenerElEmpleado(id);
            return View(empleado);
        }

        // POST: EmpleadosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            List<Models.Empleado> resultado = ObtenerEmpleados();

            for (int i = 0; i < resultado.Count; i++)
            {
                if (resultado[i].Id == id)
                    resultado.RemoveAt(i);
            }

            ElCache.Set("ListaDeEmpleados", resultado);

            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }        
    }
}
