namespace AdminPortal.Models.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }

        public required string EmailId { get; set; }

        public required string PhoneNo { get; set; }

        public required Decimal Salary { get; set; }
    }
}
