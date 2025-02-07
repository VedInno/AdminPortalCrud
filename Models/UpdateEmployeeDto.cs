namespace AdminPortal.Models
{
    public class UpdateEmployeeDto
    {
        public required string Name { get; set; }

        public required string Password { get; set; }

        public required string EmailId { get; set; }

        public required string PhoneNo { get; set; }

        public required Decimal Salary { get; set; }
    }
}
