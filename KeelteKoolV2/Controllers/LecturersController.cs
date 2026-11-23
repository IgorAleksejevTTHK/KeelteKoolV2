using KeelteKoolV2.Core.DTO;
using KeelteKoolV2.Core.ServiceInterface;
using KeelteKoolV2.Data;
using KeelteKoolV2.Models.Lecturers;
using Microsoft.AspNetCore.Mvc;

namespace KeelteKoolV2.Controllers
{
	public class LecturersController : Controller
	{
		private readonly KeelteKoolV2Context _context;
		private readonly ILecturersServices _lecturersServices;
		private readonly IFilesServices _filesServices;

		public LecturersController(
			KeelteKoolV2Context context,
			ILecturersServices lecturersServices,
			IFilesServices filesServices)
		{
			_context = context;
			_lecturersServices = lecturersServices;
			_filesServices = filesServices;
		}
		public IActionResult Index()
		{
			//võtame andmebaasist õpetajad ainult vajalike andmetega
			var result = _context.Lecturers
				.Select(x => new LecturerViewModel
				{
					Id = x.Id,
					FirstName = x.FirstName,
					LastName = x.LastName,
				}).Take(20).OrderBy(x => x.LastName);
			return View(result);
		}

		[HttpGet]
		public IActionResult Create()
		{
			LecturerCreateUpdateViewModel vm = new();
			return View("CreateUpdate", vm);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(LecturerCreateUpdateViewModel vm)
		{
			//kontrollime et vm ei oleks null ja andmed on korras
			if (vm == null || !ModelState.IsValid)
			{
				return RedirectToAction("Error", "Home");
			}
			//paneme vm andmed dto sisse
			var dto = new LecturerDTO()
			{
				FirstName = vm.FirstName,
				LastName = vm.LastName,
				Qualifications = vm.Qualifications
			};
			var result = await _lecturersServices.Create(dto);
			if (result == null)
			{
				return RedirectToAction("Error", "Home");
			}
			return RedirectToAction(nameof(Index));
		}
		[HttpGet]
		public async Task<IActionResult> Details(Guid id)
		{
			if (id == Guid.Empty)
			{
				return NotFound();
			}
			var lecturer = await _lecturersServices.DetailAsync(id);
			if (lecturer == null)
			{
				return NotFound();
			}
			var vm = new LecturerViewModel();
			vm.Id = lecturer.Id;
			vm.FirstName = lecturer.FirstName;
			vm.LastName = lecturer.LastName;
			vm.Qualifications = lecturer.Qualifications;

			ViewData["ViewType"] = "details";

			return View("DetailsDelete", vm);
		}
		[HttpGet]
		public async Task<IActionResult> Update(Guid id)
		{
			if (id == Guid.Empty)
			{
				return NotFound();
			}
			var lecturer = await _lecturersServices.DetailAsync(id);
			if (lecturer == null)
			{
				return NotFound();
			}
			var vm = new LecturerCreateUpdateViewModel();
			vm.Id = lecturer.Id;
			vm.FirstName = lecturer.FirstName;
			vm.LastName = lecturer.LastName;
			vm.Qualifications = lecturer.Qualifications;
			vm.CreatedAt = lecturer.CreatedAt;
			vm.ModifiedAt = lecturer.ModifiedAt;
			return View("CreateUpdate", vm);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update(LecturerCreateUpdateViewModel vm)
		{
			if (!ModelState.IsValid)
			{
				return View("CreateUpdate", vm);
			}
			var dto = new LecturerDTO()
			{
				Id = (Guid)vm.Id,
				FirstName = vm.FirstName,
				LastName = vm.LastName,
				Qualifications = vm.Qualifications,
				CreatedAt = vm.CreatedAt,
				ModifiedAt = vm.ModifiedAt
			};
			var result = await _lecturersServices.Update(dto);
			if (result == null)
			{
				return RedirectToAction(nameof(Index));
			}
			//jääme muutmise vaatesse nagu keelekursuste juures
			return RedirectToAction(nameof(Update), new { id = result.Id });
		}
		[HttpGet]
		public async Task<IActionResult> Delete(Guid id)
		{
			if (id == Guid.Empty)
			{
				return NotFound();
			}
			var lecturer = await _lecturersServices.DetailAsync(id);
			if (lecturer == null)
			{
				return NotFound();
			}
			var vm = new LecturerViewModel();
			vm.Id = lecturer.Id;
			vm.FirstName = lecturer.FirstName;
			vm.LastName = lecturer.LastName;
			vm.Qualifications = lecturer.Qualifications;

			ViewData["ViewType"] = "delete";

			return View("DetailsDelete", vm);
		}
		[HttpPost]
		[ActionName("DeleteConfirmed")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(Guid id)
		{
			var lecturerToDelete = await _lecturersServices.Delete(id);
			if (lecturerToDelete == null)
			{
				return NotFound();
			}
			return RedirectToAction(nameof(Index));
		}
	}
}
