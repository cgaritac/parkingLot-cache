using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Proyecto_1.Models;

namespace Proyecto_1.Controllers
{
    public class ParqueosController : Controller
    {
        // Caché
        private readonly IMemoryCache ElCache;

        public ParqueosController(IMemoryCache elCache)
        {
            ElCache = elCache;
        }

        [NonAction]
        private List<Models.Parqueo> ObtenerParqueos()
        {
            List<Models.Parqueo> resultado;

            if (EstaVacioElCache())
            {
                resultado = new List<Models.Parqueo>();
                ElCache.Set("ListaDeParqueos", resultado);
            }
            else
                resultado = (List<Models.Parqueo>)ElCache.Get("ListaDeParqueos");

            return resultado;
        }

        [NonAction]
        private Models.Parqueo ObtenerElParqueo(int id)
        {
            List<Models.Parqueo> laLista;
            laLista = ObtenerParqueos();

            foreach (Models.Parqueo parqueo in laLista)
            {
                if (parqueo.Id == id)
                    return parqueo;
            }

            return null;
        }

        [NonAction]
        private bool EstaVacioElCache()
        {
            if (ElCache.Get("ListaDeParqueos") is null)
                return true;
            else return false;
        }

        // GET: ParqueosController
        [HttpGet]
        public ActionResult Index()
        {
            return View(ObtenerParqueos());
        }

        // GET: ParqueosController/Create
        [HttpGet]
        public ActionResult Create()
        {
            List<Models.Parqueo> parqueos = ObtenerParqueos();
            int nextId = 1;

            // Si hay parqueos en la lista, obtener el valor máximo de "Id" y agregar 1
            if (parqueos.Count > 0)
            {
                try
                {
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }
            }

            // Crear una nueva instancia de Parqueo con el próximo valor de "Id"
            Models.Parqueo nuevoParqueo = new Models.Parqueo
            {
                Id = nextId
            };

            return View(nuevoParqueo);
        }

        // POST: ParqueosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, Models.Parqueo parqueo)
        {
            List<Models.Parqueo> resultado = ObtenerParqueos();
            var parqueoAlmacenado = ObtenerElParqueo(id);

            if (parqueoAlmacenado != null)
            {
                ModelState.AddModelError("Id", "El número de parqueo ingresado ya existe.");
            }

            if (parqueo.HoraCierre <= parqueo.HoraApertura)
            {
                ModelState.AddModelError("HoraApertura", "La hora de apertura no puede ser mayor que la hora de cierre.");
            }

            if (int.Parse(parqueo.TarifaHora) <= int.Parse(parqueo.TarifaMedia))
            {
                ModelState.AddModelError("TarifaHora", "La tarifa por hora debe ser mayor que la tarifa por media hora.");
            }

            if (ModelState.IsValid)
            {
                // Generar automáticamente el valor de "Id"
                int nextId = 1;

                if (resultado.Count > 0)
                {
                    // Si hay parqueos en la lista, obtener el valor del último "Id" y agregar 1
                    nextId = resultado.Max(t => t.Id) + 1;
                }

                parqueo.Id = nextId;

                resultado.Add(parqueo);
                ElCache.Set("ListaDeParqueos", resultado);

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

        // GET: ParqueosController/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var parqueo = ObtenerElParqueo(id);
            return View(parqueo);
        }

        // POST: ParqueosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Models.Parqueo parqueo)
        {
            if (parqueo.HoraCierre <= parqueo.HoraApertura)
            {
                ModelState.AddModelError("HoraApertura", "La hora de apertura no puede ser mayor que la hora de cierre.");
            }

            if (int.Parse(parqueo.TarifaHora) <= int.Parse(parqueo.TarifaMedia))
            {
                ModelState.AddModelError("TarifaHora", "La tarifa por hora debe ser mayor que la tarifa por media hora.");
            }

            if (ModelState.IsValid)
            {
                List<Models.Parqueo> resultado = ObtenerParqueos();

                for (int i = 0; i < resultado.Count; i++)
                {
                    if (resultado[i].Id == id)
                    {
                        resultado.RemoveAt(i);
                        resultado.Add(parqueo);
                    }
                }

                ElCache.Set("ListaDeParqueos", resultado);

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

        // GET: ParqueosController/Delete/5
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var parqueo = ObtenerElParqueo(id);
            return View(parqueo);
        }

        // POST: ParqueosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            List<Models.Parqueo> resultado = ObtenerParqueos();

            for (int i = 0; i < resultado.Count; i++)
            {
                if (resultado[i].Id == id)
                    resultado.RemoveAt(i);
            }

            ElCache.Set("ListaDeParqueos", resultado);

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
