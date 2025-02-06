namespace AdminPortal.Models
{
    public class AddEmployeeDto
    {
        public required string Name { get; set; }

        public required string EmailId { get; set; }

        public required string PhoneNo { get; set; }

        public required Decimal Salary { get; set; }
    }
}
