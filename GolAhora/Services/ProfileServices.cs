using GolAhora.Data.UnitOfWork;
using GolAhora.DTOs;
using GolAhora.Exceptions;
using GolAhora.Models;
using GolAhora.Query;
using Microsoft.AspNetCore.Identity;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Services
{
    public class ProfileServices
    {
        private readonly AppContext _appContext;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _appContext = unitOfWork.Context;
        }

        public async Task<string?> UpdateAdmin(int adminId, UpdateAdminDTO dto)
        {
            var admin = await _appContext.AdminProfiles.FindAsync(adminId);

            if (admin == null)
                throw new NotFoundException("Admin no encontrado");

            admin.accessLevel = dto.accessLevel;

            _appContext.AdminProfiles.Update(admin);
            await _unitOfWork.SaveChangesAsync();

            return "Admin actualizado exitosamente";
        }

        public async Task<string?> UpdateEmployee(int employeeId, UpdateEmployeeDTO dto)
        {
            var employee = await _appContext.EmployeeProfiles.FindAsync(employeeId);

            if (employee == null)
                throw new NotFoundException("Empleado no encontrado");

            employee.sector = dto.sector;

            _appContext.EmployeeProfiles.Update(employee);
            await _unitOfWork.SaveChangesAsync();

            return "Empleado actualizado exitosamente";
        }

        public async Task<string?> UpdateProfessor(int professorId, UpdateProfessorDTO dto)
        {
            var professor = await _appContext.ProfessorProfiles.FindAsync(professorId);

            if (professor == null)
                throw new NotFoundException("Profesor no encontrado");

            professor.speciality = dto.speciality;
            professor.certification = dto.certification;

            _appContext.ProfessorProfiles.Update(professor);
            await _unitOfWork.SaveChangesAsync();

            return "Profesor actualizado exitosamente";
        }
    }
}


