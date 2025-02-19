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
using Serilog;

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
            Log.Information(
                ">>>>>>>>>>>>>>>>>>>>>> POST Login API called at {Time} <<<<<<<<<<<<<<<<",
                DateTime.UtcNow
            );

            try
            {
                if (_authService.ValidateUser(loginRequest))
                {
                    var data = dBContext.Employees.FirstOrDefault(e =>
                        e.EmailId == loginRequest.EmailId
                    );

                    if (data == null)
                    {
                        Log.Warning(
                            "Login attempt failed. User not found: {EmailId}",
                            loginRequest.EmailId
                        );
                        return Unauthorized("Invalid username or password");
                    }

                    var token = _authService.GenerateJwtToken(
                        data.EmailId,
                        data.Role,
                        data.Name,
                        data.Salary,
                        data.PhoneNo,
                        data.Id
                    );

                    Log.Information("User {EmailId} successfully logged in", data.EmailId);
                    return Ok(new { Message = "Successfully Logged In", Token = token });
                }

                Log.Warning("Invalid login attempt for {EmailId}", loginRequest.EmailId);
                return Unauthorized("Invalid username or password");
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "An error occurred while processing the login request for {EmailId}",
                    loginRequest.EmailId
                );
                return StatusCode(500, "An internal server error occurred.");
            }
            finally
            {
                Log.Information(
                    ">>>>>>>>>>>>>>>>>>>>>> Login API Execution Completed at {Time} <<<<<<<<<<<<<<<<",
                    DateTime.UtcNow
                );
            }
        }

        [HttpGet]
        [Route("AllEmployessList")]
        [Authorize]
        public IActionResult GetAllEmployees()
        {
            Log.Information(
                ">>>>>>>>>>>>>>>>>>>>>> GET AllEmployeesList API called at {Time} <<<<<<<<<<<<<<<<",
                DateTime.UtcNow
            );

            try
            {
                var role = _authService.GetClaimValue("Role");

                if (role == "Admin")
                {
                    var allemployees = dBContext.Employees.ToList();
                    Log.Information("Successfully fetched all employees for the Admin role.");
                    return Ok(allemployees);
                }

                Log.Warning("Unauthorized access attempt. User role: {Role}", role);
                return Unauthorized(
                    "Not an admin or administrator! Not allowed to perform this function"
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "An error occurred while fetching employee data at {Time}",
                    DateTime.UtcNow
                );
                return StatusCode(500, "An internal server error occurred.");
            }
            finally
            {
                Log.Information(
                    ">>>>>>>>>>>>>>>>>>>>>> GET AllEmployeesList API execution completed at {Time} <<<<<<<<<<<<<<<<",
                    DateTime.UtcNow
                );
            }
        }

        [HttpGet]
        [Route("MyProfile")]
        [Authorize]
        public IActionResult GetEmployeeById()
        {
            Log.Information(
                ">>>>>>>>>>>>>>>>>>>>>> GET MyProfile API called at {Time} <<<<<<<<<<<<<<<<",
                DateTime.UtcNow
            );

            try
            {
                var id = _authService.GetClaimValue("ID");
                var email = _authService.GetClaimValue("Email");
                var role = _authService.GetClaimValue("Role");
                var name = _authService.GetClaimValue("Name");
                var salary = _authService.GetClaimValue("Salary");
                var PhoneNo = _authService.GetClaimValue("PhoneNo");

                Log.Information("Successfully retrieved employee profile for ID: {ID}", id);

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
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "An error occurred while retrieving the employee profile at {Time}",
                    DateTime.UtcNow
                );
                return StatusCode(500, "An internal server error occurred.");
            }
            finally
            {
                Log.Information(
                    ">>>>>>>>>>>>>>>>>>>>>> GET MyProfile API execution completed at {Time} <<<<<<<<<<<<<<<<",
                    DateTime.UtcNow
                );
            }
        }

        [HttpPost]
        [Route("AddEmployee")]
        [Authorize]
        public IActionResult AddEmployee(AddEmployeeDto addEmployee)
        {
            Log.Information(
                ">>>>>>>>>>>>>>>>>>>>>> POST AddEmployee API called at {Time} <<<<<<<<<<<<<<<<",
                DateTime.UtcNow
            );

            try
            {
                var role = _authService.GetClaimValue("Role");

                if (role == "Admin")
                {
                    var employeeEntity = new Employee()
                    {
                        Name = addEmployee.Name,
                        Password = addEmployee.Password,
                        EmailId = addEmployee.EmailId,
                        PhoneNo = addEmployee.PhoneNo,
                        Salary = addEmployee.Salary,
                        Role = addEmployee.Role,
                    };

                    dBContext.Employees.Add(employeeEntity);
                    dBContext.SaveChanges();

                    Log.Information("Successfully added new employee: {Name}", addEmployee.Name);

                    return Ok(
                        new
                        {
                            Response = "You created the new Employee Successfully",
                            Name = addEmployee.Name,
                        }
                    );
                }

                Log.Warning("Unauthorized attempt to add employee. User role: {Role}", role);
                return Unauthorized(
                    "Not an admin or administrator! Not allowed to perform this type of function"
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "An error occurred while adding a new employee at {Time}",
                    DateTime.UtcNow
                );
                return StatusCode(500, "An internal server error occurred.");
            }
            finally
            {
                Log.Information(
                    ">>>>>>>>>>>>>>>>>>>>>> POST AddEmployee API execution completed at {Time} <<<<<<<<<<<<<<<<",
                    DateTime.UtcNow
                );
            }
        }

        [HttpPut]
        [Authorize]
        [Route("{id:guid}")]
        public IActionResult UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployee)
        {
            Log.Information(
                ">>>>>>>>>>>>>>>>>>>>>> PUT UpdateEmployee API called at {Time} <<<<<<<<<<<<<<<<",
                DateTime.UtcNow
            );

            try
            {
                var role = _authService.GetClaimValue("Role");

                if (role == "Admin")
                {
                    var employeeData = dBContext.Employees.Find(id);

                    if (employeeData == null)
                    {
                        Log.Warning("Employee with ID {Id} not found.", id);
                        return NotFound("Employee not found.");
                    }

                    employeeData.Name = updateEmployee.Name;
                    employeeData.Salary = updateEmployee.Salary;
                    employeeData.EmailId = updateEmployee.EmailId;
                    employeeData.PhoneNo = updateEmployee.PhoneNo;
                    employeeData.Password = updateEmployee.Password;

                    dBContext.SaveChanges();

                    Log.Information("Employee with ID {Id} updated successfully.", id);

                    return Ok(new { Message = "User info Updated Successfully" });
                }

                Log.Warning("Unauthorized attempt to update employee. User role: {Role}", role);
                return Unauthorized(
                    "Not an admin or administrator! Not allowed to perform this function"
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "An error occurred while updating employee data with ID {Id} at {Time}",
                    id,
                    DateTime.UtcNow
                );
                return StatusCode(500, "An internal server error occurred.");
            }
            finally
            {
                Log.Information(
                    ">>>>>>>>>>>>>>>>>>>>>> PUT UpdateEmployee API execution completed at {Time} <<<<<<<<<<<<<<<<",
                    DateTime.UtcNow
                );
            }
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteEmployee(Guid id)
        {
            Log.Information(
                ">>>>>>>>>>>>>>>>>>>>>> DELETE DeleteEmployee API called at {Time} <<<<<<<<<<<<<<<<",
                DateTime.UtcNow
            );

            try
            {
                var role = _authService.GetClaimValue("Role");

                if (role == "Admin")
                {
                    var employee = dBContext.Employees.Find(id);

                    if (employee == null)
                    {
                        Log.Warning("Employee with ID {Id} not found.", id);
                        return NotFound("Employee not found.");
                    }

                    dBContext.Employees.Remove(employee);
                    dBContext.SaveChanges();

                    Log.Information("Employee with ID {Id} deleted successfully.", id);
                    return Ok(new { Message = "Employee Data Deleted Successfully" });
                }

                Log.Warning("Unauthorized attempt to delete employee. User role: {Role}", role);
                return Unauthorized(
                    "Not an admin or administrator! Not allowed to perform this function"
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "An error occurred while deleting employee data with ID {Id} at {Time}",
                    id,
                    DateTime.UtcNow
                );
                return StatusCode(500, "An internal server error occurred.");
            }
            finally
            {
                Log.Information(
                    ">>>>>>>>>>>>>>>>>>>>>> DELETE DeleteEmployee API execution completed at {Time} <<<<<<<<<<<<<<<<",
                    DateTime.UtcNow
                );
            }
        }
    }
}
