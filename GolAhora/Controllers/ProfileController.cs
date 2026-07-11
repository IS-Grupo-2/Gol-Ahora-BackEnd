using GolAhora.DTOs;
using GolAhora.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolAhora.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly ProfileServices _profileService;

        public ProfileController(ProfileServices profileService)
        {
            _profileService = profileService;
        }

        [HttpPut("/{adminId}")]
        public async Task<IActionResult> UpdateAdmin(int adminId, [FromBody] UpdateAdminDTO dto)
        {
            var result = await _profileService.UpdateAdmin(adminId, dto);
            return new JsonResult(result) { StatusCode = 200 };
        }

        [HttpPut("/employee/{employeeId}")]
        public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] UpdateEmployeeDTO dto)
        {
            var result = await _profileService.UpdateEmployee(employeeId, dto);
            return new JsonResult(result) { StatusCode = 200 };
        }

        [HttpPut("/professor/{professorId}")]
        public async Task<IActionResult> UpdateProfessor(int professorId, [FromBody] UpdateProfessorDTO dto)
        {
            var result = await _profileService.UpdateProfessor(professorId, dto);
            return new JsonResult(result) { StatusCode = 200 };
        }
    }
}

