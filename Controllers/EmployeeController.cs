using AdminPortal.Data;
using AdminPortal.Models;
using AdminPortal.Models.Entities;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDBContext dBContext;

        public EmployeeController(ApplicationDBContext dBContext)
        {
            this.dBContext = dBContext;
        }
        [HttpGet]
        public IActionResult GetAllEmployees()
        {
            var allemployees = dBContext.Employees.ToList();
            return Ok(allemployees);

        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetEmployeeById(Guid id)
        {
            var employeedata = dBContext.Employees.Find(id);
            if (employeedata == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(new
                {
                    EmployeeDatalist = employeedata,
                });
            }
        }



        [HttpPost]
        public IActionResult AddEmployee(AddEmployeeDto addEmployee)
        {
            var employeeentity = new Employee()
            {
                Name = addEmployee.Name,
                EmailId = addEmployee.EmailId,
                PhoneNo = addEmployee.PhoneNo,
                Salary = addEmployee.Salary,
            };
            dBContext.Employees.Add(employeeentity);
            dBContext.SaveChanges();
            return Ok(new
            {
                Response = "User Added Successfully"
            });

        }
        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployee)
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

            dBContext.SaveChanges();
            return Ok(new
            {
                Message = "User info Updated Successfully"
            });

        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteEmployee(Guid id)
        {
            var employee = dBContext.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();

            }
            dBContext.Remove(employee);
            dBContext.SaveChanges(true);
            return Ok(new
            {
                Message = "User Deleted Successfully"
            });

        }
    }
}
