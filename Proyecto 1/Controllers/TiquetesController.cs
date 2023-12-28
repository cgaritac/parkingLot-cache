using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Versioning;
using Proyecto_1.Models;

namespace Proyecto_1.Controllers
{
    public class TiquetesController : Controller
    {
        // Caché
        private readonly IMemoryCache ElCache;
        //private readonly ParqueosController parqueosController;

        public TiquetesController(IMemoryCache elCache)
        {
            ElCache = elCache;
            //this.parqueosController = parqueosController;
        }

        [NonAction]
        private List<Models.Tiquete> ObtenerTiquetes()
        {
            List<Models.Tiquete> resultado;

            if (EstaVacioElCache())
            {
                resultado = new List<Models.Tiquete>();
                ElCache.Set("ListaDeTiquetes", resultado);
            }
            else
                resultado = (List<Models.Tiquete>)ElCache.Get("ListaDeTiquetes");

            return resultado;
        }

        [NonAction]
        private Models.Tiquete ObtenerElTiquete(int id)
        {
            List<Models.Tiquete> laLista;
            laLista = ObtenerTiquetes();

            foreach (Models.Tiquete tiquete in laLista)
            {
                if (tiquete.Id == id)
                    return tiquete;
            }

            return null;
        }

        [NonAction]
        private bool EstaVacioElCache()
        {
            if (ElCache.Get("ListaDeTiquetes") is null)
                return true;
            else return false;
        }

        [NonAction]
        public static bool EsEntero(double numero)
        {
            // Comprueba si el número es igual a su versión redondeada al entero más cercano
            return numero == Math.Round(numero);
        }

        [NonAction]
        public static bool EsFraccionMayorA0_5(double numero)
        {
            if (!EsEntero(numero)) // Verifica si no es un número entero
            {
                // Obtiene la parte decimal del número
                double parteDecimal = numero - Math.Floor(numero);

                // Comprueba si la parte decimal es mayor a 0.5
                return parteDecimal > 0.5;
            }

            return false; // El número es entero, no es una fracción mayor a 0.5
        }

        // GET: TiquetesController
        [HttpGet]
        public ActionResult Index()
        {
            return View(ObtenerTiquetes());
        }

        // GET: TiquetesController/Create
        [HttpGet]
        public ActionResult Create()
        {
            List<Models.Tiquete> tiquetes = ObtenerTiquetes();
            int nextId = 1;

            // Si hay tiquetes en la lista, obtener el valor máximo de "Id" y agregar 1
            if (tiquetes.Count > 0)
            {
                nextId = tiquetes.Max(t => t.Id) + 1;
            }

            // Crear una nueva instancia de Tiquete con el próximo valor de "Id"
            Models.Tiquete nuevoTiquete = new Models.Tiquete
            {
                Id = nextId,
                Ingreso = DateTime.Now
            };

            return View(nuevoTiquete);
        }
   
        // POST: TiquetesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Models.Tiquete tiquete)
        {
            List<Models.Tiquete> resultado = ObtenerTiquetes();
            List<Models.Parqueo> listadoParqueos = (List<Models.Parqueo>)ElCache.Get("ListaDeParqueos");

            if (listadoParqueos == null)
            {
                ModelState.AddModelError("Ingreso", "Debe crear primero un parqueo.");
            }else

            if (listadoParqueos.Count() == 0)
            {
                ModelState.AddModelError("Ingreso", "Debe crear primero un parqueo.");
            }

            if (listadoParqueos != null && listadoParqueos.Count() != 0)
            {
                // Verificar si la fecha y hora de ingreso están dentro del rango de tiempo del parqueo
                if (tiquete.Ingreso.TimeOfDay < listadoParqueos[0].HoraApertura.TimeOfDay || tiquete.Ingreso.TimeOfDay > listadoParqueos[0].HoraCierre.TimeOfDay)
                {
                    ModelState.AddModelError("Ingreso", "La hora de ingreso debe estar dentro del horario de apertura y cierre del parqueo.");
                }
            }
                
            if (ModelState.IsValid)
            {
                // Generar automáticamente el valor de "Id"
                int nextId = 1;

                if (resultado.Count > 0)
                {
                    // Si hay tiquetes en la lista, obtener el valor del último "Id" y agregar 1
                    nextId = resultado.Max(t => t.Id) + 1;
                }

                tiquete.Id = nextId;

                resultado.Add(tiquete);
                ElCache.Set("ListaDeTiquetes", resultado);

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

        // GET: TiquetesController/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            var tiquete = ObtenerElTiquete(id);

            if (id != 0 && tiquete != null)
            {
                return View(tiquete);
            }
            else return RedirectToAction(nameof(Index));
        }

        // GET: TiquetesController/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var tiquete = ObtenerElTiquete(id);

            tiquete.Salida = DateTime.Now;
            
            return View(tiquete);
        }

        // POST: TiquetesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Models.Tiquete tiquete)
        {
            List<Models.Tiquete> resultado = ObtenerTiquetes();
            List<Models.Parqueo> listadoParqueos = (List<Models.Parqueo>)ElCache.Get("ListaDeParqueos");

            if (listadoParqueos.Count() == 0)
            {
                ModelState.AddModelError("Salida", "Debe crear primero un parqueo.");
            }

            if (listadoParqueos.Count() != 0)
            {
                if (tiquete.Salida <= tiquete.Ingreso)
                {
                    ModelState.AddModelError("Salida", "La fecha y hora de salida no puede ser mayor que la fecha y hora de entrada.");
                } else

                // Verificar si la fecha y hora de ingreso están dentro del rango de tiempo del parqueo
                if (tiquete.Salida.TimeOfDay < listadoParqueos[0].HoraApertura.TimeOfDay || tiquete.Salida.TimeOfDay > listadoParqueos[0].HoraCierre.TimeOfDay)
                {
                    ModelState.AddModelError("Salida", "La hora de salida debe estar dentro del horario de apertura y cierre del parqueo.");
                }
            }

            if (ModelState.IsValid)
            {
                var HorasEstacionado = tiquete.Salida.Subtract(tiquete.Ingreso).TotalHours;

                for (int i = 0; i < resultado.Count; i++)
                {
                    if (resultado[i].Id == id)
                    {
                        // Cerrar el tiquete
                        tiquete.Estado = "Cerrado";

                        // Establecer la tarifa por hora y la tarifa por media hora
                        double tarifaHora = Double.Parse(listadoParqueos[0].TarifaHora);
                        double tarifaMedia = Double.Parse(listadoParqueos[0].TarifaMedia);

                        double costo = 0.0;

                        if (HorasEstacionado <= 0.5)
                        {
                            costo = tarifaMedia;
                        }
                        else if (EsEntero(HorasEstacionado))
                        {
                            costo = tarifaHora * HorasEstacionado;
                        }
                        else if (EsFraccionMayorA0_5(HorasEstacionado))
                        {
                            costo = tarifaHora * (HorasEstacionado + 1);
                        }
                        else
                        {
                            costo = Math.Floor(HorasEstacionado) * tarifaHora + 1 * tarifaMedia;
                        }

                        tiquete.Venta = costo;
                        resultado.RemoveAt(i);
                        resultado.Add(tiquete);
                    }
                }

                ElCache.Set("ListaDeTiquetes", resultado);

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

        // GET: TiquetesController/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var tiquete = ObtenerElTiquete(id);
            return View(tiquete);
        }

        // POST: TiquetesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            List<Models.Tiquete> resultado = ObtenerTiquetes();

            for (int i = 0; i < resultado.Count; i++)
            {
                if (resultado[i].Id == id)
                    resultado.RemoveAt(i);
            }

            ElCache.Set("ListaDeTiquetes", resultado);

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
