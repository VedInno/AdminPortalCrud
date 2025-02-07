using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdminPortal.Data;
using AdminPortal.Models;
using AdminPortal.Models.Entities;
using AdminPortal.Services;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDBContext dBContext;
        private readonly UserLoginServices _authService;

        public EmployeeController(ApplicationDBContext dBContext, UserLoginServices user)
        {
            this.dBContext = dBContext;
            this._authService = user;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginDto loginRequest)
        {
            if (_authService.ValidateUser(loginRequest))
            {
                var data = dBContext.Employees.FirstOrDefault(e =>
                    e.EmailId == loginRequest.EmailId
                );
                var token = _authService.GenerateJwtToken(
                    data.EmailId,
                    data.Role,
                    data.Name,
                    data.Salary,
                    data.PhoneNo,
                    data.Id
                );
                return Ok(new { Message = "Sucessfully Logged In", Token = token });
            }
            return Unauthorized("Invalid username or password");
        }

        [HttpGet]
        [Route("AllEmployessList")]
        [Authorize]
        public IActionResult GetAllEmployees()
        {
            var role = _authService.GetClaimValue("Role");
            if (role == "Admin")
            {
                var allemployees = dBContext.Employees.ToList();
                return Ok(allemployees);
            }
            return Unauthorized(
                "Not a admin or adminstrator! Not allowed to perform this function"
            );
        }

        [HttpGet]
        [Route("MyProfile")]
        [Authorize]
        public IActionResult GetEmployeeById()
        {
            var id = _authService.GetClaimValue("ID");
            var email = _authService.GetClaimValue("Email");
            var role = _authService.GetClaimValue("Role");
            var name = _authService.GetClaimValue("Name");
            var salary = _authService.GetClaimValue("Salary");
            var PhoneNo = _authService.GetClaimValue("PhoneNo");

            return Ok(
                new
                {
                    Message = "Welcome To InnoAge Technologies",
                    EmployeerID = id,
                    Name = name,
                    Email = email,
                    PhoneNo = PhoneNo,
                    Salary = salary,
                    Role = role,
                }
            );
        }

        [HttpPost]
        [Route("AddEmployee")]
        [Authorize]
        public IActionResult AddEmployee(AddEmployeeDto addEmployee)
        {
            var role = _authService.GetClaimValue("Role");
            if (role == "Admin")
            {
                var employeeentity = new Employee()
                {
                    Name = addEmployee.Name,
                    Password = addEmployee.Password,
                    EmailId = addEmployee.EmailId,
                    PhoneNo = addEmployee.PhoneNo,
                    Salary = addEmployee.Salary,
                    Role = addEmployee.Role,
                };
                dBContext.Employees.Add(employeeentity);
                dBContext.SaveChanges();
                return Ok(
                    new
                    {
                        Response = "You created the new Employee Successfully",
                        name = addEmployee.Name,
                    }
                );
            }
            return Unauthorized(
                "Not a admin or adminstrator! Not allowed to perform this function"
            );
        }

        [HttpPut]
        [Authorize]
        [Route("{id:guid}")]
        public IActionResult UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployee)
        {
            var role = _authService.GetClaimValue("Role");
            if (role == "Admin")
            {
                var employeedata = dBContext.Employees.Find(id);
                if (employeedata == null)
                {
                    return NotFound();
                }
                employeedata.Name = updateEmployee.Name;
                employeedata.Salary = updateEmployee.Salary;
                employeedata.EmailId = updateEmployee.EmailId;
                employeedata.PhoneNo = updateEmployee.PhoneNo;
                employeedata.Password = updateEmployee.Password;

                dBContext.SaveChanges();
                return Ok(new { Message = "User info Updated Successfully" });
            }
            return Unauthorized(
                "Not a admin or adminstrator! Not allowed to perform this function"
            );
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteEmployee(Guid id)
        {
            var role = _authService.GetClaimValue("Role");
            if (role == "Admin")
            {
                var employee = dBContext.Employees.Find(id);
                if (employee == null)
                {
                    return NotFound();
                }
                dBContext.Remove(employee);
                dBContext.SaveChanges(true);
                return Ok(new { Message = "Employee Data Deleted Successfully" });
            }
            return Unauthorized(
                "Not a admin or adminstrator! Not allowed to perform this function"
            );
        }
    }
}
